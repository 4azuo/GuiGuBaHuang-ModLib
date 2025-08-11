#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script chính để xử lý và dịch các file *localText.json

Cách sử dụng:
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
    # Clear console trước khi bắt đầu
    os.system('cls' if os.name == 'nt' else 'clear')
    
    parser = argparse.ArgumentParser(
        description='Script xử lý và dịch file localText.json',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog='''
Ví dụ sử dụng:
  python run.py --project 3385996759 --path .
  python run.py --project 3385996759 --path game_localText.json
  python run.py --project 3385996759 --path . --dry-run
  python run.py --project 3385996759 --path . --create-locales vi,es,fr
  python run.py --project 3385996759 --path . --file-type main
  python run.py --project 3385996759 --path . --file-type locale
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
        help='Loại file cần xử lý: main (dịch main file với 4 key en,ch,tc,kr), locale (xóa locale cũ và tạo mới với combined key), both (cả hai - mặc định)'
    )
    
    args = parser.parse_args()
    
    print_header(
        UI_MESSAGES['script_title'],
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
                description = f"hiển thị {len(files_to_show)} file loại '{args.file_type}'"
            elif args.file_type == "locale":
                # Với locale, sẽ tạo lại từ main files
                files_to_show = main_files
                description = f"sẽ tạo lại locale từ {len(files_to_show)} main file"
            else:  # both
                files_to_show = files
                description = f"hiển thị {len(files_to_show)} file loại '{args.file_type}'"
            
            print_info(f"Tìm thấy {len(files)} file", description)
            for i, file_path in enumerate(files_to_show, 1):
                rel_path = os.path.relpath(file_path, os.getcwd())
                file_info = FileUtils.get_file_info(file_path)
                if args.file_type == "locale":
                    # Hiển thị main file nhưng ghi chú sẽ tạo locale
                    print(f"  {i}. {rel_path} [main → sẽ tạo locale]")
                else:
                    print(f"  {i}. {rel_path} [{file_info.file_type.value}]")
        else:
            print_warning("Không tìm thấy file nào!")
    else:
        # Xử lý thực tế
        try:
            processor.process_files(project_path, args.path, args.file_type)
        except KeyboardInterrupt:
            from progressbar_utils import print_stats
            print_warning("Quá trình xử lý đã bị dừng bởi người dùng")
            
            interrupt_stats = {
                f"{UI_ICONS['folder']} Đã xử lý": f"{processor.stats.processed_count} file",
                f"{UI_ICONS['globe']} Đã dịch": f"{processor.translation_service.stats.translated_count} text"
            }
            print_stats(interrupt_stats)
            print_info("Bạn có thể chạy lại lệnh để tiếp tục từ nơi đã dừng")
            return
        except Exception as e:
            print_error("Lỗi trong quá trình xử lý", str(e))
            return

if __name__ == "__main__":
    main()
