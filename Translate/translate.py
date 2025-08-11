#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script chính để xử lý và dịch các file *localText.json

Cách sử dụng:
    python 00-translate.py --project 3385996759 --path .
    python 00-translate.py --project 3385996759 --path game_localText.json --dry-run
    python 00-translate.py --project 3385996759 --path . --create-locales vi,es,fr
"""

import os
import time
import argparse
import re
from typing import List, Optional
from deep_translator import GoogleTranslator

# Import modules
import sys
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from consts import (
    DEFAULT_TARGET_LANGUAGES, 
    TRANSLATION_CONFIG, 
    SKIP_TRANSLATION_PATTERNS,
    DIR_PATTERNS
)
from data_types import ProcessingStats, TranslationConfig, FileType, ProgressConfig
from file_utils import FileUtils
from json_utils import JsonUtils
from progressbar_utils import (
    ProgressContext, 
    progress_manager, 
    create_file_progress_config, 
    create_translation_progress_config,
    print_header, 
    print_section, 
    print_file_info, 
    print_result, 
    print_stats, 
    print_error, 
    print_warning, 
    print_success, 
    print_info
)

class TranslationService:
    """Service để thực hiện dịch text"""
    
    def __init__(self, config: TranslationConfig):
        self.config = config
        self.stats = ProcessingStats()
    
    def translate_text(self, text: str, target_lang: str) -> str:
        """Dịch text sang ngôn ngữ đích"""
        try:
            if not text or text.strip() == "":
                return text
            
            # Skip nếu text khớp với pattern
            for pattern in SKIP_TRANSLATION_PATTERNS:
                if re.match(pattern, text):
                    return text
            
            # Skip nếu text quá ngắn (1-2 ký tự)
            if len(text.strip()) <= 2:
                return text
            
            # Retry mechanism for network issues
            for attempt in range(self.config.max_retries):
                try:
                    result = GoogleTranslator(
                        source=self.config.source_language, 
                        target=target_lang
                    ).translate(text)
                    
                    time.sleep(self.config.delay_between_requests)
                    self.stats.translated_count += 1
                    return result
                    
                except KeyboardInterrupt:
                    # Re-raise KeyboardInterrupt to allow clean exit
                    raise
                    
                except Exception as network_error:
                    if attempt < self.config.max_retries - 1:
                        # Silent retry - progress bar will show overall progress
                        time.sleep(self.config.retry_delay)
                    else:
                        self.stats.failed_count += 1
                        return text
                        
        except KeyboardInterrupt:
            raise
            
        except Exception as e:
            self.stats.failed_count += 1
            return text

class LocalTextProcessor:
    """Class chính để xử lý các file localText"""
    
    def __init__(self, translation_config: TranslationConfig):
        self.translation_service = TranslationService(translation_config)
        self.config = translation_config
        self.stats = ProcessingStats()
    
    def process_files(self, project_path: str, target_path: str = ".", file_type: str = "both") -> None:
        """
        Xử lý các file localText trong project
        
        Args:
            project_path: Đường dẫn tới project (vd: ../3385996759)
            target_path: Đường dẫn tương đối trong ModConf
            file_type: Loại file cần xử lý ('main', 'locale', 'both')
        """
        start_time = time.time()
        
        # Xây dựng đường dẫn ModConf
        modconf_path = os.path.join(project_path, DIR_PATTERNS['modconf_path'])
        
        if not os.path.exists(modconf_path):
            print_error(f"Không tìm thấy thư mục", modconf_path)
            return
        
        print_info(f"Thư mục ModConf", modconf_path)
        
        # Tìm tất cả file localText
        files = FileUtils.find_localtext_files(modconf_path, target_path)
        
        if not files:
            print_warning("Không tìm thấy file localText nào!")
            return
        
        # Phân loại file
        main_files = [f for f in files if not FileUtils.is_locale_file(f)]
        locale_files = [f for f in files if FileUtils.is_locale_file(f)]
        
        # Lọc file theo loại được chỉ định
        if file_type == "main":
            files_to_process = main_files
            locale_files = []  # Không xử lý locale files
        elif file_type == "locale":
            files_to_process = locale_files
            main_files = []  # Không xử lý main files
        else:  # both
            files_to_process = files
        
        # Hiển thị thống kê
        stats_info = {
            "📁 Tổng số file tìm thấy": len(files),
            "📄 Main files": len(main_files),
            "🌍 Locale files": len(locale_files),
            "🎯 Loại xử lý": file_type,
            "🌍 Ngôn ngữ target": ', '.join(self.config.target_languages)
        }
        if file_type != "both":
            stats_info["📋 Sẽ xử lý"] = f"{len(files_to_process)} file"
        
        print_stats(stats_info)
        
        try:
            # Bước 1: Xử lý main files
            if file_type in ["main", "both"] and main_files:
                print_section(f"Xử lý {len(main_files)} main file")
                
                with ProgressContext(
                    len(main_files), 
                    "Đang xử lý main files...", 
                    create_file_progress_config("📄")
                ) as progress:
                    for i, file_path in enumerate(main_files):
                        filename = os.path.basename(file_path)
                        # Cắt ngắn filename cho progress display
                        display_name = filename[:25] + "..." if len(filename) > 25 else filename
                        progress.update(1, f"Xử lý {display_name}")
                        
                        # Chỉ tạo locale files khi file_type là "both"
                        create_locales = (file_type == "both")
                        # Tắt verbose output khi đang có progress bar
                        if self.process_main_file(file_path, create_locales, verbose=False):
                            self.stats.processed_count += 1
            
            # Bước 2: Xử lý locale files (chỉ khi file_type là "locale")
            if file_type == "locale" and locale_files:
                print_section(f"Xử lý {len(locale_files)} locale file")
                
                with ProgressContext(
                    len(locale_files), 
                    "Đang xử lý locale files...", 
                    create_file_progress_config("🌍")
                ) as progress:
                    for file_path in locale_files:
                        filename = os.path.basename(file_path)
                        # Cắt ngắn filename cho progress display
                        display_name = filename[:25] + "..." if len(filename) > 25 else filename
                        progress.update(1, f"Xử lý {display_name}")
                        
                        if self.process_locale_file(file_path):
                            self.stats.processed_count += 1
        
        except KeyboardInterrupt:
            print_warning("Quá trình xử lý đã bị dừng bởi người dùng")
            # Cleanup progress bars
            progress_manager.cleanup()
            raise
        
        # Thống kê kết quả
        end_time = time.time()
        elapsed_time = end_time - start_time
        
        result_stats = {
            "📁 Tổng số file": len(files),
            "✅ Xử lý thành công": self.stats.processed_count,
            "🌍 Đã dịch": f"{self.translation_service.stats.translated_count} text",
            "❌ Lỗi dịch": f"{self.translation_service.stats.failed_count} text",
            "⏱️ Thời gian": f"{elapsed_time:.1f}s"
        }
        print_stats(result_stats)
    
    def process_main_file(self, file_path: str, create_locales: bool = True, verbose: bool = True) -> bool:
        """Xử lý main file và tạo các locale file tương ứng (tùy chọn)"""
        try:
            filename = os.path.basename(file_path)
            
            # Đọc main file
            main_data = JsonUtils.read_json_file(file_path)
            if not main_data:
                if verbose:
                    print_result("❌", f"Không đọc được file {filename}")
                return False
            
            # Kiểm tra cấu trúc
            if not JsonUtils.validate_json_structure(main_data.data):
                if verbose:
                    print_result("⚠️", f"File {filename} không có dữ liệu tiếng Anh để dịch")
                return True  # Không phải lỗi, chỉ là file không cần dịch
            
            # Đếm số text cần dịch
            translatable_count = JsonUtils.get_translatable_count(main_data)
            
            if create_locales:
                # Tạo locale file cho mỗi ngôn ngữ
                for language in self.config.target_languages:
                    locale_file_path = FileUtils.create_locale_file_path(file_path, language)
                    
                    # Đảm bảo thư mục tồn tại
                    FileUtils.ensure_directory_exists(locale_file_path)
                    
                    if verbose:
                        print_info(f"Đang dịch {filename} sang {language}", f"{translatable_count} text")
                    
                    # Tạo progress-aware translator
                    progress_bar = progress_manager.create_progress(
                        f"translate_{language}", 
                        translatable_count, 
                        create_translation_progress_config()
                    )
                    progress_manager.set_active(f"translate_{language}")
                    
                    def progress_translator(text: str, target_lang: str) -> str:
                        result = self.translation_service.translate_text(text, target_lang)
                        progress_manager.update_active(1, f"Dịch: {text[:30]}{'...' if len(text) > 30 else ''}")
                        return result
                    
                    # Tạo dữ liệu locale
                    locale_data = JsonUtils.create_locale_data_from_main(
                        main_data, 
                        language, 
                        progress_translator
                    )
                    
                    # Hoàn thành progress bar
                    progress_manager.finish_active(f"Hoàn thành {language}")
                    
                    # Sắp xếp dữ liệu
                    locale_data = JsonUtils.sort_json_data(locale_data)
                    
                    # Ghi file
                    if JsonUtils.write_json_file(locale_file_path, locale_data):
                        print_result("✅", f"{language}", os.path.basename(locale_file_path))
                    else:
                        print_result("❌", f"Lỗi ghi file {language}")
                        return False
            else:
                # Chỉ xử lý main file, không tạo locale files
                if verbose:
                    print_result("✅", f"Đã xử lý main file", filename)
            
            return True
            
        except KeyboardInterrupt:
            progress_manager.cleanup()
            raise
            
        except Exception as e:
            print_result("❌", f"Lỗi xử lý file {filename}", str(e))
            return False
    
    def process_locale_file(self, file_path: str) -> bool:
        """Xử lý locale file dựa trên main file tương ứng"""
        try:
            filename = os.path.basename(file_path)
            
            # Dọn dẹp file locale cũ trước khi tạo mới
            if os.path.exists(file_path):
                os.remove(file_path)
            
            # Tìm main file tương ứng
            main_file_path = FileUtils.find_main_file(file_path)
            if not main_file_path:
                print_warning(f"Không tìm thấy main file tương ứng cho {filename}")
                # Fallback: xử lý như main file
                return self.process_main_file(file_path)
            
            # Xác định ngôn ngữ đích
            target_lang = FileUtils.get_locale_language(file_path)
            if not target_lang:
                print_result("❌", f"Không xác định được ngôn ngữ cho {filename}")
                return False
            
            # Đọc main file
            main_data = JsonUtils.read_json_file(main_file_path)
            if not main_data:
                print_result("❌", f"Không đọc được main file", main_file_path)
                return False
            
            # Đếm số text cần dịch
            translatable_count = JsonUtils.get_translatable_count(main_data)
            
            print_info(f"Tạo lại {filename} cho {target_lang}", f"{translatable_count} text")
            
            # Tạo progress-aware translator cho locale file
            progress_bar = progress_manager.create_progress(
                f"locale_{target_lang}", 
                translatable_count, 
                create_translation_progress_config()
            )
            progress_manager.set_active(f"locale_{target_lang}")
            
            def progress_translator(text: str, target_lang: str) -> str:
                result = self.translation_service.translate_text(text, target_lang)
                progress_manager.update_active(1, f"Dịch: {text[:30]}{'...' if len(text) > 30 else ''}")
                return result
            
            # Tạo dữ liệu locale
            locale_data = JsonUtils.create_locale_data_from_main(
                main_data, 
                target_lang, 
                progress_translator
            )
            
            # Hoàn thành progress bar
            progress_manager.finish_active(f"Hoàn thành {target_lang}")
            
            # Sắp xếp dữ liệu
            locale_data = JsonUtils.sort_json_data(locale_data)
            
            # Đảm bảo thư mục tồn tại
            FileUtils.ensure_directory_exists(file_path)
            
            # Ghi file
            if JsonUtils.write_json_file(file_path, locale_data):
                print_result("✅", f"Tạo lại locale file cho {target_lang}")
                return True
            else:
                print_result("❌", f"Lỗi ghi locale file {filename}")
                return False
                
        except KeyboardInterrupt:
            progress_manager.cleanup()
            raise
            
        except Exception as e:
            print_result("❌", f"Lỗi xử lý locale file {filename}", str(e))
            return False

def parse_target_languages(languages_str: str) -> List[str]:
    """Parse chuỗi ngôn ngữ thành list"""
    if not languages_str:
        return DEFAULT_TARGET_LANGUAGES
    
    if languages_str.lower() == 'all':
        return DEFAULT_TARGET_LANGUAGES
    
    # Parse danh sách ngôn ngữ
    languages = [lang.strip() for lang in languages_str.split(',') if lang.strip()]
    return languages if languages else DEFAULT_TARGET_LANGUAGES

def main():
    """Hàm main"""
    parser = argparse.ArgumentParser(
        description='Script xử lý và dịch file localText.json',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
Ví dụ sử dụng:
  python translate.py --project 3385996759 --path .
  python translate.py --project 3385996759 --path game_localText.json
  python translate.py --project 3385996759 --path . --dry-run
  python translate.py --project 3385996759 --path . --create-locales vi,es,fr
  python translate.py --project 3385996759 --path . --file-type main
  python translate.py --project 3385996759 --path . --file-type locale
        '''
    )
    
    parser.add_argument(
        '--project',
        required=True,
        help='Tên thư mục project (vd: 3385996759)'
    )
    
    parser.add_argument(
        '--path', 
        required=True,
        help='Đường dẫn tương đối trong ModConf (folder hoặc file). Vd: "." cho toàn bộ, "game_localText.json" cho file cụ thể'
    )
    
    parser.add_argument(
        '--dry-run', 
        action='store_true', 
        help='Chỉ hiển thị danh sách file sẽ được xử lý, không thực hiện dịch'
    )
    
    parser.add_argument(
        '--create-locales',
        type=str,
        help='Danh sách ngôn ngữ cần tạo file locale, cách nhau bởi dấu phẩy. Ví dụ: vi,es,fr hoặc "all" cho tất cả'
    )
    
    parser.add_argument(
        '--file-type',
        choices=['main', 'locale', 'both'],
        default='both',
        help='Loại file cần xử lý: main (chỉ main files), locale (chỉ locale files), both (cả hai - mặc định)'
    )
    
    args = parser.parse_args()
    
    print_header(
        "Script Xử Lý LocalText.json",
        f"Project: {args.project} | Path: {args.path} | Type: {args.file_type}"
    )
    
    # Xây dựng đường dẫn project
    project_path = os.path.join("..", args.project)
    
    if not os.path.exists(project_path):
        print_error("Không tìm thấy project", project_path)
        return
    
    # Parse target languages
    target_languages = parse_target_languages(args.create_locales)
    print_info("Ngôn ngữ locale", ', '.join(target_languages))
    
    # Tạo config
    translation_config = TranslationConfig(
        target_languages=target_languages,
        max_retries=TRANSLATION_CONFIG['max_retries'],
        delay_between_requests=TRANSLATION_CONFIG['delay_between_requests'],
        retry_delay=TRANSLATION_CONFIG['retry_delay'],
        source_language=TRANSLATION_CONFIG['source_language']
    )
    
    processor = LocalTextProcessor(translation_config)
    
    if args.dry_run:
        # Chỉ hiển thị danh sách file
        modconf_path = os.path.join(project_path, DIR_PATTERNS['modconf_path'])
        files = FileUtils.find_localtext_files(modconf_path, args.path)
        
        if files:
            # Phân loại file để hiển thị theo file_type
            main_files = [f for f in files if not FileUtils.is_locale_file(f)]
            locale_files = [f for f in files if FileUtils.is_locale_file(f)]
            
            if args.file_type == "main":
                files_to_show = main_files
            elif args.file_type == "locale":
                files_to_show = locale_files
            else:  # both
                files_to_show = files
            
            print_info(f"Tìm thấy {len(files)} file", f"hiển thị {len(files_to_show)} file loại '{args.file_type}'")
            for i, file_path in enumerate(files_to_show, 1):
                rel_path = os.path.relpath(file_path, os.getcwd())
                file_info = FileUtils.get_file_info(file_path)
                print(f"  {i}. {rel_path} [{file_info.file_type.value}]")
        else:
            print_warning("Không tìm thấy file nào!")
    else:
        # Xử lý thực tế
        try:
            processor.process_files(project_path, args.path, args.file_type)
        except KeyboardInterrupt:
            print_warning("Quá trình xử lý đã bị dừng bởi người dùng")
            
            interrupt_stats = {
                "📁 Đã xử lý": f"{processor.stats.processed_count} file",
                "🌍 Đã dịch": f"{processor.translation_service.stats.translated_count} text"
            }
            print_stats(interrupt_stats)
            print_info("Bạn có thể chạy lại lệnh để tiếp tục từ nơi đã dừng")
            return
        except Exception as e:
            print_error("Lỗi trong quá trình xử lý", str(e))
            return

if __name__ == "__main__":
    main()
