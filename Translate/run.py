#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Main script to process and translate *localText.json files

Usage:
    python run.py --project 3385996759 --path .
    python run.py --project 3385996759 --path game_localText.json --dry-run
    python run.py --project 3385996759 --path . --create-locales vi,es,fr
"""

import os
import argparse
from typing import List

# Import modules
import sys
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from consts import DEFAULT_TARGET_LANGUAGES, TRANSLATION_CONFIG, DIR_PATTERNS, UI_MESSAGES, UI_ICONS
from data_types import TranslationConfig, FileType
from file_utils import FileUtils
from local_text_processor import LocalTextProcessor
from progressbar_utils import print_header, print_error, print_warning, print_info

def parse_target_languages(languages_str: str) -> List[str]:
    """Parse language string to list"""
    if not languages_str:
        return DEFAULT_TARGET_LANGUAGES
    
    if languages_str.lower() == 'all':
        return DEFAULT_TARGET_LANGUAGES
    
    # Parse language list
    languages = [lang.strip() for lang in languages_str.split(',') if lang.strip()]
    return languages if languages else DEFAULT_TARGET_LANGUAGES

def main():
    """Main function"""
    # Clear console before starting
    os.system('cls' if os.name == 'nt' else 'clear')
    
    parser = argparse.ArgumentParser(
        description='Script to process and translate localText.json files',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
Usage examples:
  python run.py --project 3385996759 --path .
  python run.py --project 3385996759 --path game_localText.json
  python run.py --project 3385996759 --path . --dry-run
  python run.py --project 3385996759 --path . --create-locales vi,es,fr
  python run.py --project 3385996759 --path . --file-type main
  python run.py --project 3385996759 --path . --file-type locale
  python run.py --project 3385996759 --path . --preserve-translations
  python run.py --project 3385996759 --path . --workers 8
  python run.py --project 3385996759 --path . --workers 1  # Sequential processing
        '''
    )
    
    parser.add_argument(
        '--project',
        required=True,
        help='Project directory name (e.g.: 3385996759)'
    )
    
    parser.add_argument(
        '--path', 
        required=True,
        help='Relative path in ModConf (folder or file). E.g.: "." for entire directory, "game_localText.json" for specific file'
    )
    
    parser.add_argument(
        '--dry-run', 
        action='store_true', 
        help='Only display list of files to be processed, do not perform translation'
    )
    
    parser.add_argument(
        '--create-locales',
        type=str,
        help='List of languages to create locale files for, separated by commas. Example: vi,es,fr or "all" for all languages'
    )
    
    parser.add_argument(
        '--file-type',
        choices=['main', 'locale', 'both'],
        default='both',
        help='Type of files to process: main (translate main files with 4 keys en,ch,tc,kr), locale (delete old locales and create new with combined key), both (both types - default)'
    )
    
    parser.add_argument(
        '--preserve-translations',
        action='store_true',
        help='Keep existing translations, only translate new terms that haven\'t been translated yet'
    )
    
    parser.add_argument(
        '--workers',
        type=int,
        default=8,
        help='Number of parallel threads for file processing (default: 4). Reduce if encountering rate-limits from translation service.'
    )
    
    args = parser.parse_args()
    
    print_header(
        UI_MESSAGES['script_title'],
        f"Project: {args.project} | Path: {args.path} | Type: {args.file_type} | Workers: {args.workers} | Preserve: {'Yes' if args.preserve_translations else 'No'}"
    )
    
    # Build project path
    project_path = os.path.join("..", args.project)
    
    if not os.path.exists(project_path):
        print_error("Project not found", project_path)
        return
    
    # Parse target languages
    target_languages = parse_target_languages(args.create_locales)
    print_info("Locale languages", ', '.join(target_languages))
    
    # Create config
    translation_config = TranslationConfig(
        target_languages=target_languages,
        max_retries=TRANSLATION_CONFIG['max_retries'],
        delay_between_requests=TRANSLATION_CONFIG['delay_between_requests'],
        retry_delay=TRANSLATION_CONFIG['retry_delay'],
        source_language=TRANSLATION_CONFIG['source_language'],
        preserve_existing_translations=args.preserve_translations
    )
    
    processor = LocalTextProcessor(translation_config)
    
    if args.dry_run:
        # Only display file list
        modconf_path = os.path.join(project_path, DIR_PATTERNS['modconf_path'])
        files = FileUtils.find_localtext_files(modconf_path, args.path)
        
        if files:
            # Classify files for display according to file_type
            main_files = [f for f in files if not FileUtils.is_locale_file(f)]
            locale_files = [f for f in files if FileUtils.is_locale_file(f)]
            
            if args.file_type == "main":
                files_to_show = main_files
                description = f"showing {len(files_to_show)} files of type '{args.file_type}'"
            elif args.file_type == "locale":
                # For locale, will recreate from main files
                files_to_show = main_files
                description = f"will recreate locale from {len(files_to_show)} main files"
            else:  # both
                files_to_show = files
                description = f"showing {len(files_to_show)} files of type '{args.file_type}'"
            
            print_info(f"Found {len(files)} files", description)
            for i, file_path in enumerate(files_to_show, 1):
                rel_path = os.path.relpath(file_path, os.getcwd())
                file_info = FileUtils.get_file_info(file_path)
                if args.file_type == "locale":
                    # Show main file but note will create locale
                    print(f"  {i}. {rel_path} [main → will create locale]")
                else:
                    print(f"  {i}. {rel_path} [{file_info.file_type.value}]")
        else:
            print_warning("No files found!")
    else:
        # Actual processing
        try:
            processor.process_files(project_path, args.path, args.file_type, max_workers=args.workers)
        except Exception as e:
            print_error("Error during processing", str(e))
            return

if __name__ == "__main__":
    main()
