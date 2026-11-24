#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Utilities for working with files and directories
"""

import os
import glob
from typing import List, Optional

from consts import DIR_PATTERNS, FILE_CONFIG
from data_types import FileType, FileInfo

class FileUtils:
    """Utilities for handling files and directories"""
    
    @staticmethod
    def find_localtext_files(base_path: str, target_path: str = ".") -> List[str]:
        """
        Find all *localText.json files in directory
        
        Args:
            base_path: Path to ModConf
            target_path: Relative path to search
            
        Returns:
            List of full paths to localText files
        """
        if not os.path.exists(base_path):
            return []
        
        # Build search path
        if target_path == ".":
            search_path = base_path
        else:
            search_path = os.path.join(base_path, target_path)
        
        if not os.path.exists(search_path):
            return []
        
        files = []
        
        if os.path.isfile(search_path):
            # If it's a specific file
            if search_path.endswith(FILE_CONFIG['localtext_suffix']):
                files.append(search_path)
        else:
            # If it's a directory, find all localText files
            pattern = os.path.join(search_path, "**", DIR_PATTERNS['localtext_pattern'])
            files = glob.glob(pattern, recursive=True)
        
        return sorted(files)
    
    @staticmethod
    def is_locale_file(file_path: str) -> bool:
        """
        Check if file is a locale file
        Locale files are files located in child folders of ModConf
        
        Args:
            file_path: Path to file
            
        Returns:
            True if it's a locale file
        """
        try:
            # Normalize path for cross-platform compatibility
            normalized_path = os.path.normpath(file_path)
            
            # Get parent directory of file
            parent_dir = os.path.dirname(normalized_path)
            
            # Get parent directory of parent directory (should be ModConf)
            grandparent_dir = os.path.dirname(parent_dir)
            
            # Check if grandparent directory is ModConf
            grandparent_name = os.path.basename(grandparent_dir)
            
            # If grandparent is ModConf then this file is in a child folder of ModConf
            return grandparent_name == "ModConf"
            
        except Exception:
            return False
    
    @staticmethod
    def get_locale_language(file_path: str) -> Optional[str]:
        """
        Get language code from locale file path
        Language code is the locale folder name
        
        Args:
            file_path: Path to locale file
            
        Returns:
            Language code or None if cannot be determined
        """
        if not FileUtils.is_locale_file(file_path):
            return None
        
        try:
            # Get parent directory of file (language directory)
            locale_dir = os.path.dirname(file_path)
            
            # Directory name is the language code
            language_code = os.path.basename(locale_dir)
            
            return language_code if language_code else None
            
        except Exception:
            return None
    
    @staticmethod
    def find_main_file(locale_file_path: str) -> Optional[str]:
        """
        Find main file corresponding to locale file
        Find file with similar name in parent folder
        
        Args:
            locale_file_path: Path to locale file
            
        Returns:
            Path to corresponding main file or None
        """
        if not FileUtils.is_locale_file(locale_file_path):
            return None
        
        try:
            # Get filename from locale file
            filename = os.path.basename(locale_file_path)
            
            # Get parent directory of locale file (language directory)
            locale_dir = os.path.dirname(locale_file_path)
            
            # Get parent directory of language directory (ModConf)
            parent_dir = os.path.dirname(locale_dir)
            
            # Create path to main file with same name in parent directory
            main_file_path = os.path.join(parent_dir, filename)
            
            # Check if file exists
            if os.path.exists(main_file_path) and os.path.isfile(main_file_path):
                return main_file_path
            
            return None
            
        except Exception:
            return None
    
    @staticmethod
    def ensure_directory_exists(file_path: str) -> None:
        """
        Ensure directory containing file exists
        
        Args:
            file_path: Path to file
        """
        directory = os.path.dirname(file_path)
        if not os.path.exists(directory):
            os.makedirs(directory)
    
    @staticmethod
    def get_file_info(file_path: str) -> FileInfo:
        """
        Get detailed information about localText file
        
        Args:
            file_path: Path to file
            
        Returns:
            FileInfo object containing file information
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