#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Local text processor for handling localText files
"""

import os
import sys
import time
import signal
import threading
import concurrent.futures
from typing import List

from consts import DIR_PATTERNS, UI_ICONS, UI_MESSAGES
from data_types import ProcessingStats, TranslationConfig
from translation_service import TranslationService
from translate_utils import TranslateUtils
from json_utils import JsonUtils
from progressbar_utils import (
    ProgressBar, ProgressContext, create_file_progress_config, 
    print_info, print_section, print_warning, print_error, print_stats,
    print_result
)

class LocalTextProcessor:
    """Main class for processing localText files"""
    
    def __init__(self, translation_config: TranslationConfig):
        self.translation_service = TranslationService(translation_config)
        self.config = translation_config
        self.stats = ProcessingStats()
        self.executor = None
        self._setup_signal_handler()
    
    def _setup_signal_handler(self):
        """Setup signal handler to catch Ctrl+C"""
        def signal_handler(signum, frame):
            print()
            print_error("Received stop signal (Ctrl+C)...")
            
            # Shutdown executor if running
            if self.executor:
                print_info("Stopping running tasks...")
                self.executor.shutdown(wait=False)
                self.executor = None
                
            print_error("Processing has been stopped")
            # Use os._exit() to exit main thread and entire process
            os._exit(1)
        
        # Register signal handler for SIGINT (Ctrl+C)
        signal.signal(signal.SIGINT, signal_handler)
    
    def process_files(self, project_path: str, target_path: str = ".", file_type: str = "both", max_workers: int = 4) -> None:
        """
        Process localText files in project
        
        Args:
            project_path: Path to project (e.g.: ../3385996759)
            target_path: Relative path within ModConf
            file_type: Type of files to process ('main', 'locale', 'both')
            max_workers: Number of parallel threads for file processing
        """
        from file_utils import FileUtils
        
        start_time = time.time()
        
        # Build ModConf path
        modconf_path = os.path.join(project_path, DIR_PATTERNS['modconf_path'])
        
        if not os.path.exists(modconf_path):
            print_error(UI_MESSAGES['not_found_modconf'], modconf_path)
            return
        
        print_info(f"ModConf directory", modconf_path)
        
        # Find all localText files
        files = FileUtils.find_localtext_files(modconf_path, target_path)
        
        if not files:
            print_warning(UI_MESSAGES['no_files'])
            return
        
        # Classify files
        main_files = [f for f in files if not FileUtils.is_locale_file(f)]
        locale_files = [f for f in files if FileUtils.is_locale_file(f)]
        
        # Filter files by specified type (for stats display only)
        if file_type == "main":
            files_to_process = main_files
        elif file_type == "locale":
            files_to_process = main_files  # Will create locale from main files
        else:  # both
            files_to_process = files
        
        # Display statistics
        stats_info = {
            f"{UI_ICONS['folder']} Total files found": len(files),
            f"{UI_ICONS['file']} Main files": len(main_files),
            f"{UI_ICONS['globe']} Locale files": len(locale_files),
            f"{UI_ICONS['target']} Processing type": file_type,
            f"{UI_ICONS['globe']} Target languages": ', '.join(self.config.target_languages),
            f"{UI_ICONS['success']} Preserve mode": 'On' if self.config.preserve_existing_translations else 'Off'
        }
        if file_type != "both":
            stats_info[f"{UI_ICONS['list']} Will process"] = f"{len(files_to_process)} files"
        
        print_stats(stats_info)
        
        # Prepare thread-safety primitives for concurrent runs
        stats_lock = threading.Lock()

        # Step 1: Process main files (parallel)
        if file_type in ["main", "both"] and main_files:
            print_section(f"Processing {len(main_files)} main files (parallel with {max_workers} workers)")
            
            def process_main_file_worker(file_path: str, progress: ProgressBar) -> bool:
                """Worker function to process a main file"""
                try:
                    result = self.process_main_file(progress, file_path)
                    with stats_lock:
                        if result:
                            self.stats.processed_count += 1
                    return result
                except Exception:
                    # process_main_file already logged error, continue
                    return False
            
            with ProgressContext(
                len(main_files), 
                "Processing main files...", 
                create_file_progress_config(UI_ICONS['file'])
            ) as progress:
                if max_workers == 1:
                    # Sequential processing
                    for file_path in main_files:
                        filename = os.path.basename(file_path)
                        progress.update(1, f"Processing {filename}")
                        process_main_file_worker(file_path, progress)
                else:
                    # Parallel processing with ThreadPoolExecutor
                    self.executor = concurrent.futures.ThreadPoolExecutor(max_workers=max_workers)
                    try:
                        futures = {self.executor.submit(process_main_file_worker, fp, progress): fp for fp in main_files}
                        for future in concurrent.futures.as_completed(futures):
                            future.result()
                            filename = os.path.basename(futures[future])
                            progress.update(1, f"Completed {filename}")
                    finally:
                        if self.executor:
                            self.executor.shutdown(wait=False)
                            self.executor = None
        
        # Step 2: Process locale files (parallel)
        if file_type in ["locale", "both"] and main_files and not self.interrupted:
            print_section(f"Recreating locale files from {len(main_files)} main files (parallel with {max_workers} workers)")
            
            def process_locale_file_worker(file_path: str, progress: ProgressBar) -> bool:
                """Worker function to process locale files from a main file"""
                if self.interrupted:
                    return False
                
                try:
                    main_filename = os.path.basename(file_path)
                    success_any = 0
                    
                    for lang in self.config.target_languages:
                        locale_dir = os.path.join(modconf_path, lang)
                        locale_file_path = os.path.join(locale_dir, main_filename)

                        # Delete old locale files for this file (unless preserve mode is enabled)
                        if not self.config.preserve_existing_translations:
                            self.cleanup_locale_file(locale_file_path)
                        
                        # Recreate locale files from main file
                        if self.process_locale_file(progress, file_path, locale_file_path):
                            with stats_lock:
                                self.stats.processed_count += 1
                            success_any += 1

                    return success_any == len(self.config.target_languages)
                except Exception:
                    return False
            
            with ProgressContext(
                len(main_files), 
                "Recreating locale files...", 
                create_file_progress_config(UI_ICONS['file'])
            ) as progress:
                if max_workers == 1:
                    # Sequential processing
                    for file_path in main_files:
                        filename = os.path.basename(file_path)
                        progress.update(1, f"Processing {filename}")
                        process_locale_file_worker(file_path, progress)
                else:
                    # Parallel processing with ThreadPoolExecutor
                    self.executor = concurrent.futures.ThreadPoolExecutor(max_workers=max_workers)
                    try:
                        futures = {self.executor.submit(process_locale_file_worker, fp, progress): fp for fp in main_files}
                        for future in concurrent.futures.as_completed(futures):
                            future.result()
                            filename = os.path.basename(futures[future])
                            progress.update(1, f"Completed {filename}")
                    finally:
                        if self.executor:
                            self.executor.shutdown(wait=False)
                            self.executor = None
        
        # Result statistics
        end_time = time.time()
        elapsed_time = end_time - start_time
        
        result_stats = {
            f"{UI_ICONS['folder']} Total files": len(files),
            f"{UI_ICONS['success']} Successfully processed": self.stats.processed_count,
            f"{UI_ICONS['globe']} Translated": f"{self.translation_service.stats.translated_count} text",
            f"{UI_ICONS['error']} Translation errors": f"{self.translation_service.stats.failed_count} text",
            f"{UI_ICONS['time']} Time elapsed": f"{elapsed_time:.1f}s"
        }
        print_stats(result_stats)
    
    def process_main_file(self, progress: ProgressBar, file_path: str) -> bool:
        """Process main file and translate languages within it"""
        
        try:
            filename = os.path.basename(file_path)
            
            # Read main file
            main_data = JsonUtils.read_json_file(file_path)
            if not main_data:
                print_result(UI_ICONS['error'], f"Cannot read file {filename}")
                return False
            
            # Normalize combined keys to simple keys (e.g., "en|ch|tc|kr" -> "en")
            main_data = TranslateUtils.normalize_main_file_keys(main_data)
            
            # Check structure
            if not JsonUtils.validate_json_structure(main_data.data):
                print_result(UI_ICONS['warning'], f"File {filename} has no English data to translate")
                return True  # Not an error, just a file that doesn't need translation
            
            def progress_translator(text: str, target_lang: str, item_id: str = 'N/A') -> str:
                progress.rerender(f"Processing {filename}\nTranslating to '{target_lang}'\nId: '{item_id}'")
                result = self.translation_service.translate_text(text, target_lang)
                return result
            
            # Translate text in main file (update language fields: ch, tc, kr)
            translated_data = TranslateUtils.translate_main_file_languages(
                main_data,
                progress_translator,
                preserve_existing=self.config.preserve_existing_translations
            )
            
            # Sort data
            translated_data = JsonUtils.sort_json_data(translated_data)
            
            # Write back main file with translated text
            if not JsonUtils.write_json_file(file_path, translated_data):
                print_result(UI_ICONS['error'], f"Error writing main file", filename)
                return False

            return True
            
        except Exception as e:
            print_result(UI_ICONS['error'], f"Error processing file {filename}", str(e))
            return False

    def process_locale_file(self, progress: ProgressBar, main_file_path: str, locale_file_path: str) -> bool:
        """Process locale file based on corresponding main file"""
        from file_utils import FileUtils
        
        try:
            filename = os.path.basename(main_file_path)

            # Determine target language
            target_lang = FileUtils.get_locale_language(locale_file_path)
            if not target_lang:
                print_result(UI_ICONS['error'], f"Cannot determine language for {filename}")
                return False
            
            # Read main file
            main_data = JsonUtils.read_json_file(main_file_path)
            if not main_data:
                print_result(UI_ICONS['error'], f"Cannot read main file", main_file_path)
                return False
            
            def progress_translator(text: str, target_lang: str, item_id: str = 'N/A') -> str:
                progress.rerender(f"Processing {filename}\nTranslating to '{target_lang}'\nId: '{item_id}'")
                result = self.translation_service.translate_text(text, target_lang)
                return result
            
            # Read existing locale data if preserve mode is enabled
            existing_locale_data = None
            if self.config.preserve_existing_translations and os.path.exists(locale_file_path):
                existing_locale_data = JsonUtils.read_json_file(locale_file_path)
            
            # Tạo dữ liệu locale
            locale_data = TranslateUtils.create_locale_data_from_main(
                main_data, 
                target_lang, 
                progress_translator,
                existing_locale_data=existing_locale_data,
                preserve_existing=self.config.preserve_existing_translations
            )
            
            # Sort data
            locale_data = JsonUtils.sort_json_data(locale_data)
            
            # Ensure directory exists
            FileUtils.ensure_directory_exists(locale_file_path)
            
            # Write file
            if not JsonUtils.write_json_file(locale_file_path, locale_data):
                print_result(UI_ICONS['error'], f"Error writing locale file {locale_file_path}")
                return False

            return True
            
        except Exception as e:
            print_result(UI_ICONS['error'], f"Error processing locale file {filename}", str(e))
            return False

    def cleanup_locale_file(self, locale_file_path: str) -> None:
        """
        Delete locale files corresponding to a specific main file
        
        Args:
            locale_file_path: Path to locale file
            modconf_path: Path to ModConf directory
        """
        if os.path.exists(locale_file_path):
            try:
                os.remove(locale_file_path)
            except Exception as e:
                print_result(UI_ICONS['error'], f"Error deleting {locale_file_path}", str(e))
