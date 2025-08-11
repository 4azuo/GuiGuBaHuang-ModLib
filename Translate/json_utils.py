#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Utilities để làm việc với JSON files
"""

import json
import re
from typing import Any, Optional

from data_types import LocalTextData
from consts import FILE_CONFIG, COMBINED_KEY_WITH_EN_PATTERN

class JsonUtils:
    """Utilities cho việc xử lý JSON data"""
    
    @staticmethod
    def fix_json_format(content: str) -> str:
        """
        Sửa các lỗi format JSON phổ biến
        
        Args:
            content: Raw JSON string content
            
        Returns:
            Cleaned JSON string
        """
        # Xóa trailing commas
        content = re.sub(r',(\s*[}\]])', r'\1', content)
        
        # Thay single quotes thành double quotes cho keys
        content = re.sub(r"'([^']*)':", r'"\1":', content)
        
        return content
    
    @staticmethod
    def read_json_file(file_path: str) -> Optional[LocalTextData]:
        """
        Đọc file JSON và trả về LocalTextData
        
        Args:
            file_path: Đường dẫn tới file JSON
            
        Returns:
            LocalTextData object hoặc None nếu lỗi
        """
        try:
            with open(file_path, 'r', encoding=FILE_CONFIG['encoding']) as f:
                content = f.read()
            
            # Tự động sửa format nếu cần
            content = JsonUtils.fix_json_format(content)
            
            # Parse JSON
            data = json.loads(content)
            
            return LocalTextData(data)
            
        except Exception as e:
            print(f"Lỗi đọc file {file_path}: {e}")
            return None
    
    @staticmethod
    def write_json_file(file_path: str, data: LocalTextData) -> bool:
        """
        Ghi LocalTextData ra file JSON
        
        Args:
            file_path: Đường dẫn tới file JSON
            data: LocalTextData cần ghi
            
        Returns:
            True nếu thành công
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
            print(f"Lỗi ghi file {file_path}: {e}")
            return False
    
    @staticmethod
    def sort_json_data(data: LocalTextData) -> LocalTextData:
        """
        Sắp xếp dữ liệu JSON theo thứ tự key
        
        Args:
            data: LocalTextData cần sắp xếp
            
        Returns:
            LocalTextData đã được sắp xếp
        """
        if data.is_list:
            # Sort list items by 'id' field nếu có
            sorted_data = sorted(data.data, key=lambda x: x.get('id', ''))
            return LocalTextData(sorted_data)
        elif data.is_dict:
            # Sort dict by keys
            sorted_data = dict(sorted(data.data.items()))
            return LocalTextData(sorted_data)
        
        return data
    
    @staticmethod
    def validate_json_structure(data: Any) -> bool:
        """
        Kiểm tra cấu trúc JSON có hợp lệ cho việc dịch không
        
        Args:
            data: Dữ liệu JSON cần kiểm tra
            
        Returns:
            True nếu cấu trúc hợp lệ
        """
        if isinstance(data, list):
            # Kiểm tra ít nhất một item có key 'en' hoặc combined key chứa 'en'
            return any(
                isinstance(item, dict) and (
                    'en' in item or 
                    any(COMBINED_KEY_WITH_EN_PATTERN.match(key) for key in item.keys())
                ) 
                for item in data
            )
        elif isinstance(data, dict):
            # Kiểm tra có key 'en' hoặc combined key chứa 'en'
            return 'en' in data or any(COMBINED_KEY_WITH_EN_PATTERN.match(key) for key in data.keys())
        
        return False
    
    @staticmethod
    def get_translatable_count(data: LocalTextData) -> int:
        """
        Đếm số lượng text có thể dịch được
        
        Args:
            data: LocalTextData cần đếm
            
        Returns:
            Số lượng text có thể dịch
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
                        # Kiểm tra combined keys chứa 'en'
                        for key in item.keys():
                            if COMBINED_KEY_WITH_EN_PATTERN.match(key):
                                text = item[key]
                                if text and text.strip():
                                    count += 1
                                break  # Chỉ đếm một combined key per item
                        
        elif data.is_dict:
            if 'en' in data.data:
                en_text = data.data['en']
                if en_text and en_text.strip():
                    count += 1
            else:
                # Kiểm tra combined keys chứa 'en'
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
        Lấy text tiếng Anh từ item, hỗ trợ cả 'en' key và combined keys
        
        Args:
            item: Dictionary item từ JSON
            
        Returns:
            Text tiếng Anh hoặc None
        """
        # Ưu tiên key 'en' trước
        if 'en' in item:
            return item['en']
        
        # Nếu không có, tìm combined key chứa 'en'
        for key in item.keys():
            if COMBINED_KEY_WITH_EN_PATTERN.match(key):
                return item[key]
        
        return None
