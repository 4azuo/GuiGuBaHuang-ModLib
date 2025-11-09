#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Utilities for working with JSON files
"""

import json
import re
from typing import Any, Optional

from data_types import LocalTextData
from consts import FILE_CONFIG, COMBINED_KEY_WITH_EN_PATTERN

class JsonUtils:
    """Utilities for handling JSON data"""
    
    @staticmethod
    def fix_json_format(content: str) -> str:
        """
        Fix common JSON format errors
        
        Args:
            content: Raw JSON string content
            
        Returns:
            Cleaned JSON string
        """
        # Remove trailing commas
        content = re.sub(r',(\s*[}\]])', r'\1', content)
        
        # Replace single quotes with double quotes for keys
        content = re.sub(r"'([^']*)':", r'"\1":', content)
        
        return content
    
    @staticmethod
    def read_json_file(file_path: str) -> Optional[LocalTextData]:
        """
        Read JSON file and return LocalTextData
        
        Args:
            file_path: Path to JSON file
            
        Returns:
            LocalTextData object or None if error
        """
        try:
            with open(file_path, 'r', encoding=FILE_CONFIG['encoding']) as f:
                content = f.read()
            
            # Auto-fix format if needed
            content = JsonUtils.fix_json_format(content)
            
            # Parse JSON
            data = json.loads(content)
            
            return LocalTextData(data)
            
        except Exception as e:
            print(f"Error reading file {file_path}: {e}")
            return None
    
    @staticmethod
    def write_json_file(file_path: str, data: LocalTextData) -> bool:
        """
        Write LocalTextData to JSON file
        
        Args:
            file_path: Path to JSON file
            data: LocalTextData to write
            
        Returns:
            True if successful
        """
        try:
            with open(file_path, 'w', encoding=FILE_CONFIG['encoding']) as f:
                json.dump(
                    data.data, 
                    f, 
                    ensure_ascii=FILE_CONFIG['ensure_ascii'], 
                    indent=FILE_CONFIG['json_indent'],
                    separators=(',', ': ')
                )
            return True
            
        except Exception as e:
            print(f"Error writing file {file_path}: {e}")
            return False
    
    @staticmethod
    def sort_json_data(data: LocalTextData) -> LocalTextData:
        """
        Sort JSON data by key order
        
        Args:
            data: LocalTextData to sort
            
        Returns:
            Sorted LocalTextData
        """
        # if data.is_list:
        #     # Sort list items by 'id' field if available
        #     sorted_data = sorted(data.data, key=lambda x: x.get('id', ''))
        #     return LocalTextData(sorted_data)
        # elif data.is_dict:
        #     # Sort dict by keys
        #     sorted_data = dict(sorted(data.data.items()))
        #     return LocalTextData(sorted_data)
        
        return data
    
    @staticmethod
    def validate_json_structure(data: Any) -> bool:
        """
        Check if JSON structure is valid for translation
        
        Args:
            data: JSON data to check
            
        Returns:
            True if structure is valid
        """
        if isinstance(data, list):
            # Check if at least one item has 'en' key or combined key containing 'en'
            return any(
                isinstance(item, dict) and (
                    'en' in item or 
                    any(COMBINED_KEY_WITH_EN_PATTERN.match(key) for key in item.keys())
                ) 
                for item in data
            )
        elif isinstance(data, dict):
            # Check if has 'en' key or combined key containing 'en'
            return 'en' in data or any(COMBINED_KEY_WITH_EN_PATTERN.match(key) for key in data.keys())
        
        return False
    
    @staticmethod
    def get_translatable_count(data: LocalTextData) -> int:
        """
        Count number of translatable texts
        
        Args:
            data: LocalTextData to count
            
        Returns:
            Number of translatable texts
        """
        count = 0
        
        if data.is_list:
            for item in data.data:
                if isinstance(item, dict):
                    if 'en' in item:
                        en_text = item['en']
                        if en_text and en_text.strip():
                            count += 1
                    else:
                        # Check combined keys containing 'en'
                        for key in item.keys():
                            if COMBINED_KEY_WITH_EN_PATTERN.match(key):
                                text = item[key]
                                if text and text.strip():
                                    count += 1
                                break  # Only count one combined key per item
                        
        elif data.is_dict:
            if 'en' in data.data:
                en_text = data.data['en']
                if en_text and en_text.strip():
                    count += 1
            else:
                # Check combined keys containing 'en'
                for key in data.data.keys():
                    if COMBINED_KEY_WITH_EN_PATTERN.match(key):
                        text = data.data[key]
                        if text and text.strip():
                            count += 1
                        break
        
        return count
    
    @staticmethod
    def get_english_text(item: dict) -> Optional[str]:
        """
        Get English text from item, supporting both 'en' key and combined keys
        
        Args:
            item: Dictionary item from JSON
            
        Returns:
            English text or None
        """
        # Prioritize 'en' key first
        if 'en' in item:
            return item['en']
        
        # If not available, find combined key containing 'en'
        for key in item.keys():
            if COMBINED_KEY_WITH_EN_PATTERN.match(key):
                return item[key]
        
        return None
