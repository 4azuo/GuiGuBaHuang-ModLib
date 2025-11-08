#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Utilities để làm việc với files và directories
"""

import os
import glob
from typing import List, Optional

from consts import DIR_PATTERNS, FILE_CONFIG
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
        File locale là file nằm trong child folder của ModConf
        
        Args:
            file_path: Đường dẫn tới file
            
        Returns:
            True nếu là locale file
        """
        try:
            # Normalize path for cross-platform compatibility
            normalized_path = os.path.normpath(file_path)
            
            # Lấy thư mục cha của file
            parent_dir = os.path.dirname(normalized_path)
            
            # Lấy thư mục cha của thư mục cha (should be ModConf)
            grandparent_dir = os.path.dirname(parent_dir)
            
            # Kiểm tra xem thư mục ông/bà có phải là ModConf không
            grandparent_name = os.path.basename(grandparent_dir)
            
            # Nếu thư mục ông/bà là ModConf thì file này nằm trong child folder của ModConf
            return grandparent_name == "ModConf"
            
        except Exception:
            return False
    
    @staticmethod
    def get_locale_language(file_path: str) -> Optional[str]:
        """
        Lấy mã ngôn ngữ từ đường dẫn locale file
        Mã ngôn ngữ là tên folder locale
        
        Args:
            file_path: Đường dẫn tới locale file
            
        Returns:
            Mã ngôn ngữ hoặc None nếu không xác định được
        """
        if not FileUtils.is_locale_file(file_path):
            return None
        
        try:
            # Lấy thư mục cha của file (thư mục ngôn ngữ)
            locale_dir = os.path.dirname(file_path)
            
            # Tên thư mục chính là mã ngôn ngữ
            language_code = os.path.basename(locale_dir)
            
            return language_code if language_code else None
            
        except Exception:
            return None
    
    @staticmethod
    def find_main_file(locale_file_path: str) -> Optional[str]:
        """
        Tìm file main tương ứng với locale file
        Tìm file có tên tương tự trong thư mục cha (parent folder)
        
        Args:
            locale_file_path: Đường dẫn tới locale file
            
        Returns:
            Đường dẫn tới main file tương ứng hoặc None
        """
        if not FileUtils.is_locale_file(locale_file_path):
            return None
        
        try:
            # Lấy tên file từ locale file
            filename = os.path.basename(locale_file_path)
            
            # Lấy thư mục cha của locale file (thư mục ngôn ngữ)
            locale_dir = os.path.dirname(locale_file_path)
            
            # Lấy thư mục cha của thư mục ngôn ngữ (ModConf)
            parent_dir = os.path.dirname(locale_dir)
            
            # Tạo đường dẫn tới main file có cùng tên trong thư mục cha
            main_file_path = os.path.join(parent_dir, filename)
            
            # Kiểm tra xem file có tồn tại không
            if os.path.exists(main_file_path) and os.path.isfile(main_file_path):
                return main_file_path
            
            return None
            
        except Exception:
            return None
    
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