#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Utilities để làm việc với files và directories
"""

import os
import glob
import shutil
from typing import List, Optional
from pathlib import Path

from consts import DIR_PATTERNS, DEFAULT_TARGET_LANGUAGES, FILE_CONFIG
from data_types import FileType, FileInfo

class FileUtils:
    """Utilities cho việc xử lý files và directories"""
    
    @staticmethod
    def find_localtext_files(base_path: str, target_path: str = ".") -> List[str]:
        """
        Tìm tất cả file *localText.json trong thư mục
        
        Args:
            base_path: Đường dẫn tới ModConf
            target_path: Đường dẫn tương đối cần tìm
            
        Returns:
            List đường dẫn đầy đủ tới các file localText
        """
        if not os.path.exists(base_path):
            return []
        
        # Xây dựng đường dẫn tìm kiếm
        if target_path == ".":
            search_path = base_path
        else:
            search_path = os.path.join(base_path, target_path)
        
        if not os.path.exists(search_path):
            return []
        
        files = []
        
        if os.path.isfile(search_path):
            # Nếu là file cụ thể
            if search_path.endswith(FILE_CONFIG['localtext_suffix']):
                files.append(search_path)
        else:
            # Nếu là thư mục, tìm tất cả file localText
            pattern = os.path.join(search_path, "**", DIR_PATTERNS['localtext_pattern'])
            files = glob.glob(pattern, recursive=True)
        
        return sorted(files)
    
    @staticmethod
    def is_locale_file(file_path: str) -> bool:
        """
        Kiểm tra xem file có phải là locale file không
        Locale file nằm trong thư mục con của ModConf
        
        Args:
            file_path: Đường dẫn tới file
            
        Returns:
            True nếu là locale file
        """
        try:
            # Normalize path for cross-platform compatibility
            normalized_path = os.path.normpath(file_path)
            parts = normalized_path.split(os.sep)
            
            # Tìm vị trí của ModConf
            modconf_index = None
            for i in range(len(parts) - 1, -1, -1):
                if parts[i] == "ModConf":
                    modconf_index = i
                    break
            
            if modconf_index is None:
                return False
            
            # Nếu file nằm ngay trong ModConf thì là main file
            # Nếu file nằm trong thư mục con của ModConf thì là locale file
            file_depth = len(parts) - modconf_index - 1
            return file_depth > 1
            
        except Exception:
            return False
    
    @staticmethod
    def get_locale_language(file_path: str) -> Optional[str]:
        """
        Lấy mã ngôn ngữ từ đường dẫn locale file
        
        Args:
            file_path: Đường dẫn tới locale file
            
        Returns:
            Mã ngôn ngữ hoặc None nếu không xác định được
        """
        if not FileUtils.is_locale_file(file_path):
            return None
        
        try:
            normalized_path = os.path.normpath(file_path)
            parts = normalized_path.split(os.sep)
            
            # Tìm vị trí của ModConf
            modconf_index = None
            for i in range(len(parts) - 1, -1, -1):
                if parts[i] == "ModConf":
                    modconf_index = i
                    break
            
            if modconf_index is None or modconf_index + 1 >= len(parts):
                return None
            
            # Thư mục ngay sau ModConf là mã ngôn ngữ
            language_code = parts[modconf_index + 1]
            return language_code
            
        except Exception:
            return None
    
    @staticmethod
    def find_main_file(locale_file_path: str) -> Optional[str]:
        """
        Tìm file main tương ứng với locale file
        
        Args:
            locale_file_path: Đường dẫn tới locale file
            
        Returns:
            Đường dẫn tới main file tương ứng hoặc None
        """
        if not FileUtils.is_locale_file(locale_file_path):
            return None
        
        try:
            normalized_path = os.path.normpath(locale_file_path)
            parts = normalized_path.split(os.sep)
            
            # Tìm vị trí của ModConf
            modconf_index = None
            for i in range(len(parts) - 1, -1, -1):
                if parts[i] == "ModConf":
                    modconf_index = i
                    break
            
            if modconf_index is None:
                return None
            
            # Tạo đường dẫn main file bằng cách bỏ thư mục ngôn ngữ
            main_parts = parts[:modconf_index + 1] + parts[modconf_index + 2:]
            main_file_path = os.sep.join(main_parts)
            
            if os.path.exists(main_file_path):
                return main_file_path
            
            return None
            
        except Exception:
            return None
    
    @staticmethod
    def clean_old_locale_directories(modconf_path: str, target_languages: List[str] = None) -> int:
        """
        Xóa các thư mục locale cũ
        
        Args:
            modconf_path: Đường dẫn tới thư mục ModConf
            target_languages: Danh sách ngôn ngữ cần xóa
            
        Returns:
            Số thư mục đã xóa
        """
        if target_languages is None:
            target_languages = DEFAULT_TARGET_LANGUAGES
        
        cleaned_count = 0
        
        for lang in target_languages:
            lang_dir = os.path.join(modconf_path, lang)
            if os.path.exists(lang_dir) and os.path.isdir(lang_dir):
                try:
                    shutil.rmtree(lang_dir)
                    cleaned_count += 1
                except Exception as e:
                    print(f"Lỗi xóa {lang_dir}: {e}")
        
        return cleaned_count
    
    @staticmethod
    def ensure_directory_exists(file_path: str) -> None:
        """
        Đảm bảo thư mục chứa file tồn tại
        
        Args:
            file_path: Đường dẫn tới file
        """
        directory = os.path.dirname(file_path)
        if not os.path.exists(directory):
            os.makedirs(directory)
    
    @staticmethod
    def get_file_info(file_path: str) -> FileInfo:
        """
        Lấy thông tin chi tiết về file localText
        
        Args:
            file_path: Đường dẫn tới file
            
        Returns:
            FileInfo object chứa thông tin file
        """
        if FileUtils.is_locale_file(file_path):
            file_type = FileType.LOCALE
            language = FileUtils.get_locale_language(file_path)
            main_file_path = FileUtils.find_main_file(file_path)
        else:
            file_type = FileType.MAIN
            language = None
            main_file_path = None
        
        return FileInfo(
            path=file_path,
            file_type=file_type,
            language=language,
            main_file_path=main_file_path
        )
    
    @staticmethod
    def create_locale_file_path(main_file_path: str, language: str) -> str:
        """
        Tạo đường dẫn cho locale file từ main file
        
        Args:
            main_file_path: Đường dẫn tới main file
            language: Mã ngôn ngữ
            
        Returns:
            Đường dẫn tới locale file
        """
        normalized_path = os.path.normpath(main_file_path)
        parts = normalized_path.split(os.sep)
        
        # Tìm vị trí của ModConf
        modconf_index = None
        for i in range(len(parts) - 1, -1, -1):
            if parts[i] == "ModConf":
                modconf_index = i
                break
        
        if modconf_index is None:
            raise ValueError(f"Không tìm thấy ModConf trong đường dẫn: {main_file_path}")
        
        # Chèn thư mục ngôn ngữ sau ModConf
        locale_parts = parts[:modconf_index + 1] + [language] + parts[modconf_index + 1:]
        return os.sep.join(locale_parts)
