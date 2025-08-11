#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Utilities để làm việc với JSON files
"""

import json
import re
from typing import Any, Optional

try:
    from consts import FILE_CONFIG, MAIN_LANGUAGE_KEYS, LOCALE_COMBINED_KEY
    from data_types import LocalTextData
except ImportError:
    # Fallback cho trường hợp import trực tiếp
    import sys
    import os
    sys.path.append(os.path.dirname(os.path.abspath(__file__)))
    from consts import FILE_CONFIG, MAIN_LANGUAGE_KEYS, LOCALE_COMBINED_KEY
    from data_types import LocalTextData

class JsonUtils:
    """Utilities cho việc xử lý JSON files"""
    
    @staticmethod
    def fix_json_format(content: str) -> str:
        """
        Sửa các lỗi JSON format phổ biến
        
        Args:
            content: Nội dung JSON cần sửa
            
        Returns:
            Nội dung JSON đã được sửa
        """
        # Loại bỏ trailing commas
        content = re.sub(r',(\s*[}\]])', r'\1', content)
        
        # Sửa single quotes thành double quotes (chỉ cho keys)
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
            
            # Sửa lỗi format
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
                    data.to_dict(), 
                    f,
                    ensure_ascii=FILE_CONFIG['ensure_ascii'],
                    indent=FILE_CONFIG['json_indent']
                )
            return True
            
        except Exception as e:
            print(f"Lỗi ghi file {file_path}: {e}")
            return False
    
    @staticmethod
    def sort_json_data(data: LocalTextData) -> LocalTextData:
        """
        Sắp xếp dữ liệu JSON theo id nếu có
        
        Args:
            data: LocalTextData cần sắp xếp
            
        Returns:
            LocalTextData đã sắp xếp
        """
        if data.is_list:
            try:
                sorted_data = sorted(
                    data.data,
                    key=lambda x: x.get('id', 0) if isinstance(x.get('id'), (int, float)) else 0
                )
                return LocalTextData(sorted_data)
            except Exception:
                # Nếu không sắp xếp được, trả về dữ liệu gốc
                pass
        
        return data
    
    @staticmethod
    def validate_json_structure(data: Any) -> bool:
        """
        Kiểm tra cấu trúc JSON có hợp lệ cho localText không
        
        Args:
            data: Dữ liệu JSON cần kiểm tra
            
        Returns:
            True nếu cấu trúc hợp lệ
        """
        if isinstance(data, list):
            # Kiểm tra ít nhất một item có key 'en'
            return any(isinstance(item, dict) and 'en' in item for item in data)
        elif isinstance(data, dict):
            # Kiểm tra có key 'en'
            return 'en' in data
        
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
                if isinstance(item, dict) and 'en' in item:
                    en_text = item['en']
                    if en_text and en_text.strip():
                        count += 1
        elif data.is_dict and 'en' in data.data:
            en_text = data.data['en']
            if en_text and en_text.strip():
                count += 1
        
        return count
