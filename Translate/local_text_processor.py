#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Local text processor for handling localText files
"""

import os
import time
from typing import List

from consts import DIR_PATTERNS, UI_ICONS, UI_MESSAGES
from data_types import ProcessingStats, TranslationConfig
from translation_service import TranslationService
from translate_utils import TranslateUtils
from json_utils import JsonUtils
from progressbar_utils import (
    ProgressContext, create_file_progress_config, 
    print_info, print_section, print_warning, print_error, print_stats,
    print_result
)

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
        from file_utils import FileUtils
        
        start_time = time.time()
        
        # Xây dựng đường dẫn ModConf
        modconf_path = os.path.join(project_path, DIR_PATTERNS['modconf_path'])
        
        if not os.path.exists(modconf_path):
            print_error(UI_MESSAGES['not_found_modconf'], modconf_path)
            return
        
        print_info(f"Thư mục ModConf", modconf_path)
        
        # Tìm tất cả file localText
        files = FileUtils.find_localtext_files(modconf_path, target_path)
        
        if not files:
            print_warning(UI_MESSAGES['no_files'])
            return
        
        # Phân loại file
        main_files = [f for f in files if not FileUtils.is_locale_file(f)]
        locale_files = [f for f in files if FileUtils.is_locale_file(f)]
        
        # Lọc file theo loại được chỉ định (chỉ để hiển thị stats)
        if file_type == "main":
            files_to_process = main_files
        elif file_type == "locale":
            files_to_process = main_files  # Sẽ tạo locale từ main files
        else:  # both
            files_to_process = files
        
        # Hiển thị thống kê
        stats_info = {
            f"{UI_ICONS['folder']} Tổng số file tìm thấy": len(files),
            f"{UI_ICONS['file']} Main files": len(main_files),
            f"{UI_ICONS['globe']} Locale files": len(locale_files),
            f"{UI_ICONS['target']} Loại xử lý": file_type,
            f"{UI_ICONS['globe']} Ngôn ngữ target": ', '.join(self.config.target_languages),
            f"{UI_ICONS['success']} Preserve mode": 'Bật' if self.config.preserve_existing_translations else 'Tắt'
        }
        if file_type != "both":
            stats_info[f"{UI_ICONS['list']} Sẽ xử lý"] = f"{len(files_to_process)} file"
        
        print_stats(stats_info)
        
        try:
            # Bước 1: Xử lý main files
            if file_type in ["main", "both"] and main_files:
                print_section(f"Xử lý {len(main_files)} main file")
                
                with ProgressContext(
                    len(main_files), 
                    "Đang xử lý main files...", 
                    create_file_progress_config(UI_ICONS['file'])
                ) as progress:
                    for file_path in main_files:
                        filename = os.path.basename(file_path)
                        progress.update(1, f"Xử lý {filename}")
                        
                        # Tắt verbose output khi đang có progress bar
                        if self.process_main_file(file_path):
                            self.stats.processed_count += 1
            
            # Bước 2: Xử lý locale files
            if file_type in ["locale", "both"] and main_files:
                print_section(f"Tạo lại locale files từ {len(main_files)} main file")
                
                with ProgressContext(
                    len(main_files), 
                    "Đang tạo lại locale files...", 
                    create_file_progress_config(UI_ICONS['file'])
                ) as progress:
                    for file_path in main_files:
                        filename = os.path.basename(file_path)
                        # Cắt ngắn filename cho progress display
                        progress.update(1, f"Xử lý {filename}")

                        main_filename = os.path.basename(file_path)
                        for lang in self.config.target_languages:
                            locale_dir = os.path.join(modconf_path, lang)
                            locale_file_path = os.path.join(locale_dir, main_filename)

                            # Xóa locale files cũ cho file này (trừ khi preserve mode được bật)
                            if not self.config.preserve_existing_translations:
                                self.cleanup_locale_file(locale_file_path)
                            
                            # Tạo lại locale files từ main file
                            if self.process_locale_file(file_path, locale_file_path):
                                self.stats.processed_count += 1
        
        except KeyboardInterrupt:
            raise
        
        # Thống kê kết quả
        end_time = time.time()
        elapsed_time = end_time - start_time
        
        result_stats = {
            f"{UI_ICONS['folder']} Tổng số file": len(files),
            f"{UI_ICONS['success']} Xử lý thành công": self.stats.processed_count,
            f"{UI_ICONS['globe']} Đã dịch": f"{self.translation_service.stats.translated_count} text",
            f"{UI_ICONS['error']} Lỗi dịch": f"{self.translation_service.stats.failed_count} text",
            f"{UI_ICONS['time']} Thời gian": f"{elapsed_time:.1f}s"
        }
        print_stats(result_stats)
    
    def process_main_file(self, file_path: str) -> bool:
        """Xử lý main file và dịch các ngôn ngữ trong đó"""
        
        try:
            filename = os.path.basename(file_path)
            
            # Đọc main file
            main_data = JsonUtils.read_json_file(file_path)
            if not main_data:
                print_result(UI_ICONS['error'], f"Không đọc được file {filename}")
                return False
            
            # Chuẩn hóa combined keys thành key đơn giản (ví dụ: "en|ch|tc|kr" -> "en")
            main_data = TranslateUtils.normalize_main_file_keys(main_data)
            
            # Kiểm tra cấu trúc
            if not JsonUtils.validate_json_structure(main_data.data):
                print_result(UI_ICONS['warning'], f"File {filename} không có dữ liệu tiếng Anh để dịch")
                return True  # Không phải lỗi, chỉ là file không cần dịch
            
            # Dịch text trong main file (cập nhật các trường ngôn ngữ: ch, tc, kr)
            translated_data = TranslateUtils.translate_main_file_languages(
                main_data,
                self.translation_service.translate_text,
                preserve_existing=self.config.preserve_existing_translations
            )
            
            # Sắp xếp dữ liệu
            translated_data = JsonUtils.sort_json_data(translated_data)
            
            # Ghi lại main file với text đã dịch
            if not JsonUtils.write_json_file(file_path, translated_data):
                print_result(UI_ICONS['error'], f"Lỗi ghi main file", filename)
                return False

            return True
            
        except KeyboardInterrupt:
            raise
            
        except Exception as e:
            print_result(UI_ICONS['error'], f"Lỗi xử lý file {filename}", str(e))
            return False

    def process_locale_file(self, main_file_path: str, locale_file_path: str) -> bool:
        """Xử lý locale file dựa trên main file tương ứng"""
        from file_utils import FileUtils
        
        try:
            filename = os.path.basename(main_file_path)

            # Xác định ngôn ngữ đích
            target_lang = FileUtils.get_locale_language(locale_file_path)
            if not target_lang:
                print_result(UI_ICONS['error'], f"Không xác định được ngôn ngữ cho {filename}")
                return False
            
            # Đọc main file
            main_data = JsonUtils.read_json_file(main_file_path)
            if not main_data:
                print_result(UI_ICONS['error'], f"Không đọc được main file", main_file_path)
                return False
            
            # Đọc locale data hiện có nếu preserve mode được bật
            existing_locale_data = None
            if self.config.preserve_existing_translations and os.path.exists(locale_file_path):
                existing_locale_data = JsonUtils.read_json_file(locale_file_path)
            
            # Tạo dữ liệu locale
            locale_data = TranslateUtils.create_locale_data_from_main(
                main_data, 
                target_lang, 
                self.translation_service.translate_text,
                existing_locale_data=existing_locale_data,
                preserve_existing=self.config.preserve_existing_translations
            )
            
            # Sắp xếp dữ liệu
            locale_data = JsonUtils.sort_json_data(locale_data)
            
            # Đảm bảo thư mục tồn tại
            FileUtils.ensure_directory_exists(locale_file_path)
            
            # Ghi file
            if not JsonUtils.write_json_file(locale_file_path, locale_data):
                print_result(UI_ICONS['error'], f"Lỗi ghi locale file {locale_file_path}")
                return False

            return True
                
        except KeyboardInterrupt:
            raise
            
        except Exception as e:
            print_result(UI_ICONS['error'], f"Lỗi xử lý locale file {filename}", str(e))
            return False

    def cleanup_locale_file(self, locale_file_path: str) -> None:
        """
        Xóa các file locale tương ứng với một main file cụ thể
        
        Args:
            locale_file_path: Đường dẫn locale file
            modconf_path: Đường dẫn tới thư mục ModConf
        """
        if os.path.exists(locale_file_path):
            try:
                os.remove(locale_file_path)
            except Exception as e:
                print_result(UI_ICONS['error'], f"Lỗi xóa {locale_file_path}", str(e))
