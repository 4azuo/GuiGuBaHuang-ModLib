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
    progress_manager, create_translation_progress_config,
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
            f"{UI_ICONS['globe']} Ngôn ngữ target": ', '.join(self.config.target_languages)
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
            
            # Bước 2: Xử lý locale files
            if file_type == "locale":
                if main_files:
                    print_section(f"Tạo lại locale files từ {len(main_files)} main file")
                    
                    with ProgressContext(
                        len(main_files), 
                        "Đang tạo lại locale files...", 
                        create_file_progress_config(UI_ICONS['globe'])
                    ) as progress:
                        for file_path in main_files:
                            filename = os.path.basename(file_path)
                            # Cắt ngắn filename cho progress display
                            display_name = filename[:25] + "..." if len(filename) > 25 else filename
                            progress.update(1, f"Xử lý {display_name}")
                            
                            # Xóa locale files cũ cho file này
                            self._cleanup_locale_file(file_path, modconf_path)
                            
                            # Tạo lại locale files từ main file
                            if self.process_main_file(file_path, create_locales=True, verbose=False):
                                self.stats.processed_count += 1
                else:
                    print_warning("Không tìm thấy main file nào để tạo locale files!")
            
            # Bước 3: Xử lý locale files hiện có (chỉ khi file_type là "both")
            if file_type == "both" and locale_files:
                print_section(f"Xử lý {len(locale_files)} locale file hiện có")
                
                with ProgressContext(
                    len(locale_files), 
                    "Đang xử lý locale files...", 
                    create_file_progress_config(UI_ICONS['globe'])
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
            f"{UI_ICONS['folder']} Tổng số file": len(files),
            f"{UI_ICONS['success']} Xử lý thành công": self.stats.processed_count,
            f"{UI_ICONS['globe']} Đã dịch": f"{self.translation_service.stats.translated_count} text",
            f"{UI_ICONS['error']} Lỗi dịch": f"{self.translation_service.stats.failed_count} text",
            f"{UI_ICONS['time']} Thời gian": f"{elapsed_time:.1f}s"
        }
        print_stats(result_stats)
    
    def process_main_file(self, file_path: str, create_locales: bool = True, verbose: bool = True) -> bool:
        """Xử lý main file và tạo các locale file tương ứng (tùy chọn)"""
        from file_utils import FileUtils
        
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
                    locale_data = TranslateUtils.create_locale_data_from_main(
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
                # Xử lý và đếm text có thể dịch trong main file
                if verbose:
                    print_info(f"Đang dịch {filename}", f"{translatable_count} text")
                
                # Tạo progress bar cho việc phân tích main file
                progress_bar = progress_manager.create_progress(
                    f"analyze_main", 
                    translatable_count, 
                    create_translation_progress_config("🔄")
                )
                progress_manager.set_active(f"translate_main")
                
                def progress_translator(text: str, target_lang: str) -> str:
                    result = self.translation_service.translate_text(text, target_lang)
                    progress_manager.update_active(1, f"Dịch: {text[:30]}{'...' if len(text) > 30 else ''}")
                    return result
                
                # Dịch text trong main file (cập nhật các trường ngôn ngữ: ch, tc, kr)
                translated_data = TranslateUtils.translate_main_file_languages(
                    main_data,
                    progress_translator
                )
                
                # Hoàn thành progress bar
                progress_manager.finish_active(f"Hoàn thành dịch main file")
                
                # Sắp xếp dữ liệu
                translated_data = JsonUtils.sort_json_data(translated_data)
                
                # Ghi lại main file với text đã dịch
                if JsonUtils.write_json_file(file_path, translated_data):
                    if verbose:
                        print_result("✅", f"Đã dịch và cập nhật main file", filename)
                else:
                    print_result("❌", f"Lỗi ghi main file", filename)
                    return False
            
            return True
            
        except KeyboardInterrupt:
            progress_manager.cleanup()
            raise
            
        except Exception as e:
            print_result("❌", f"Lỗi xử lý file {filename}", str(e))
            return False
    
    def process_locale_file(self, file_path: str) -> bool:
        """Xử lý locale file dựa trên main file tương ứng"""
        from file_utils import FileUtils
        
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
            locale_data = TranslateUtils.create_locale_data_from_main(
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

    def _cleanup_locale_files(self, main_files: List[str], modconf_path: str) -> None:
        """
        Xóa các file locale tương ứng với main files
        
        Args:
            main_files: Danh sách đường dẫn main files
            modconf_path: Đường dẫn tới thư mục ModConf
        """
        print_info("Đang xóa locale files cũ...")
        
        for main_file_path in main_files:
            main_filename = os.path.basename(main_file_path)
            
            # Xóa locale files cho tất cả ngôn ngữ target
            for lang in self.config.target_languages:
                locale_dir = os.path.join(modconf_path, lang)
                locale_file_path = os.path.join(locale_dir, main_filename)
                
                if os.path.exists(locale_file_path):
                    try:
                        os.remove(locale_file_path)
                        print_result("🗑️", f"Xóa {lang}/{main_filename}")
                    except Exception as e:
                        print_result("❌", f"Lỗi xóa {lang}/{main_filename}", str(e))

    def _cleanup_locale_file(self, main_file_path: str, modconf_path: str) -> None:
        """
        Xóa các file locale tương ứng với một main file cụ thể
        
        Args:
            main_file_path: Đường dẫn main file
            modconf_path: Đường dẫn tới thư mục ModConf
        """
        main_filename = os.path.basename(main_file_path)
        
        # Xóa locale files cho tất cả ngôn ngữ target
        for lang in self.config.target_languages:
            locale_dir = os.path.join(modconf_path, lang)
            locale_file_path = os.path.join(locale_dir, main_filename)
            
            if os.path.exists(locale_file_path):
                try:
                    os.remove(locale_file_path)
                    print_result("🗑️", f"Xóa {lang}/{main_filename}")
                except Exception as e:
                    print_result("❌", f"Lỗi xóa {lang}/{main_filename}", str(e))
