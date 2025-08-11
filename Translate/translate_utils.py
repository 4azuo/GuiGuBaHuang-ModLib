#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Translation utilities
"""

from consts import MAIN_LANGUAGE_KEYS, LOCALE_COMBINED_KEY, COMBINED_KEY_WITH_EN_PATTERN, LANGUAGE_CODES
from data_types import LocalTextData
from json_utils import JsonUtils

# Translation-specific utilities
class TranslateUtils:
    """Utilities cho việc xử lý translation và locale data"""
    
    @staticmethod
    def sort_language_keys(item_dict):
        """
        Sắp xếp các key trong dictionary theo thứ tự: en, ch, tc, kr, rồi đến các key khác
        
        Args:
            item_dict: Dictionary cần sắp xếp
            
        Returns:
            Dictionary đã được sắp xếp theo thứ tự key
        """
        # Thứ tự ưu tiên cho các key ngôn ngữ
        language_order = ['__name', 'id', 'key', 'en', 'ch', 'tc', 'kr']
        
        # Tạo dict mới với thứ tự đã sắp xếp
        sorted_dict = {}
        
        # Thêm các key ngôn ngữ theo thứ tự
        for lang_key in language_order:
            if lang_key in item_dict:
                sorted_dict[lang_key] = item_dict[lang_key]
        
        # Thêm các key còn lại (không phải ngôn ngữ)
        for key, value in item_dict.items():
            if key not in language_order:
                sorted_dict[key] = value
                
        return sorted_dict

    @staticmethod
    def normalize_main_file_keys(main_data):
        """
        Chuẩn hóa các combined key trong main file thành key đơn giản
        Combined key có chứa 'en' sẽ được chuyển thành key 'en'
        Đồng thời sắp xếp các key theo thứ tự: en, ch, tc, kr
        
        Args:
            main_data: Dữ liệu main file
            
        Returns:
            LocalTextData với các key đã được chuẩn hóa và sắp xếp
        """
        if main_data.is_list:
            result = []
            for item in main_data.data:
                if isinstance(item, dict):
                    new_item = {}
                    
                    # Copy các key không phải combined key
                    for key, value in item.items():
                        if not COMBINED_KEY_WITH_EN_PATTERN.match(key):
                            new_item[key] = value
                    
                    # Tìm combined key có chứa 'en' và chuyển thành 'en'
                    for key, value in item.items():
                        if COMBINED_KEY_WITH_EN_PATTERN.match(key):
                            new_item['en'] = value
                            break  # Chỉ lấy combined key đầu tiên
                    
                    # Sắp xếp key theo thứ tự: en, ch, tc, kr
                    new_item = TranslateUtils.sort_language_keys(new_item)
                    result.append(new_item)
                else:
                    result.append(item)
            
            return LocalTextData(result)
            
        elif main_data.is_dict:
            new_item = {}
            
            # Copy các key không phải combined key
            for key, value in main_data.data.items():
                if not COMBINED_KEY_WITH_EN_PATTERN.match(key):
                    new_item[key] = value
            
            # Tìm combined key có chứa 'en' và chuyển thành 'en'
            for key, value in main_data.data.items():
                if COMBINED_KEY_WITH_EN_PATTERN.match(key):
                    new_item['en'] = value
                    break  # Chỉ lấy combined key đầu tiên
            
            # Sắp xếp key theo thứ tự: en, ch, tc, kr
            new_item = TranslateUtils.sort_language_keys(new_item)
            return LocalTextData(new_item)
        
        return main_data

    @staticmethod
    def translate_main_file_text(main_data, translator_func):
        """
        Dịch text trong main file (ví dụ: cập nhật các trường tiếng Anh)
        
        Args:
            main_data: Dữ liệu main file
            translator_func: Hàm dịch text
            
        Returns:
            LocalTextData với text đã được dịch
        """
        if main_data.is_list:
            result = []
            for item in main_data.data:
                if isinstance(item, dict) and 'en' in item:
                    new_item = item.copy()
                    
                    # Dịch tiếng Anh sang các ngôn ngữ khác trong main file
                    en_text = item['en']
                    if en_text and en_text.strip():
                        # Dịch sang tiếng Việt và cập nhật trường 'vi' (nếu có)
                        if 'vi' not in new_item or not new_item['vi']:
                            translated_vi = translator_func(en_text, 'vi')
                            new_item['vi'] = translated_vi
                        
                        # Có thể thêm dịch sang các ngôn ngữ khác
                        # if 'ch' not in new_item or not new_item['ch']:
                        #     translated_ch = translator_func(en_text, 'zh')
                        #     new_item['ch'] = translated_ch
                    
                    result.append(new_item)
                else:
                    result.append(item)
            
            return LocalTextData(result)
            
        elif main_data.is_dict and 'en' in main_data.data:
            new_item = main_data.data.copy()
            
            # Dịch tiếng Anh sang tiếng Việt trong main file
            en_text = main_data.data['en']
            if en_text and en_text.strip():
                # Dịch sang tiếng Việt và cập nhật trường 'vi' (nếu có)
                if 'vi' not in new_item or not new_item['vi']:
                    translated_vi = translator_func(en_text, 'vi')
                    new_item['vi'] = translated_vi
            
            return LocalTextData(new_item)
        
        return main_data

    @staticmethod
    def analyze_main_file_text(main_data, progress_callback):
        """
        Phân tích text trong main file và đếm số lượng text có thể dịch
        
        Args:
            main_data: Dữ liệu main file
            progress_callback: Hàm callback để cập nhật progress
            
        Returns:
            Số lượng text đã phân tích
        """
        count = 0
        
        if main_data.is_list:
            for item in main_data.data:
                if isinstance(item, dict) and 'en' in item:
                    en_text = item['en']
                    if en_text and en_text.strip():
                        progress_callback(en_text)
                        count += 1
        elif main_data.is_dict and 'en' in main_data.data:
            en_text = main_data.data['en']
            if en_text and en_text.strip():
                progress_callback(en_text)
                count += 1
        
        return count

    @staticmethod
    def translate_main_file_languages(main_data, translator_func):
        """
        Dịch và cập nhật các trường ngôn ngữ thiếu trong main file
        Main file có cấu trúc: en, ch, tc, kr
        
        Args:
            main_data: Dữ liệu main file
            translator_func: Hàm dịch text
            
        Returns:
            LocalTextData với các trường ngôn ngữ đã được cập nhật
        """
        if main_data.is_list:
            result = []
            for item in main_data.data:
                if isinstance(item, dict):
                    new_item = item.copy()
                    
                    # Lấy text tiếng Anh từ item (hỗ trợ cả 'en' và combined keys)
                    en_text = JsonUtils.get_english_text(item)
                    
                    if en_text and en_text.strip():
                        # Dịch sang Chinese Simplified (ch) nếu thiếu
                        if 'ch' not in new_item or not new_item['ch']:
                            translated_ch = translator_func(en_text, LANGUAGE_CODES['ch'])
                            new_item['ch'] = translated_ch
                        
                        # Dịch sang Traditional Chinese (tc) nếu thiếu  
                        if 'tc' not in new_item or not new_item['tc']:
                            translated_tc = translator_func(en_text, LANGUAGE_CODES['tc'])
                            new_item['tc'] = translated_tc
                        
                        # Dịch sang Korean (kr) nếu thiếu
                        if 'kr' not in new_item or not new_item['kr']:
                            translated_kr = translator_func(en_text, LANGUAGE_CODES['kr'])
                            new_item['kr'] = translated_kr
                    
                    # Sắp xếp key theo thứ tự: en, ch, tc, kr
                    new_item = TranslateUtils.sort_language_keys(new_item)
                    result.append(new_item)
                else:
                    result.append(item)
            
            return LocalTextData(result)
            
        elif main_data.is_dict:
            new_item = main_data.data.copy()
            
            # Lấy text tiếng Anh từ dict
            en_text = JsonUtils.get_english_text(main_data.data)
            
            if en_text and en_text.strip():
                # Dịch sang các ngôn ngữ thiếu
                if 'ch' not in new_item or not new_item['ch']:
                    translated_ch = translator_func(en_text, LANGUAGE_CODES['ch'])
                    new_item['ch'] = translated_ch
                
                if 'tc' not in new_item or not new_item['tc']:
                    translated_tc = translator_func(en_text, LANGUAGE_CODES['tc'])
                    new_item['tc'] = translated_tc
                
                if 'kr' not in new_item or not new_item['kr']:
                    translated_kr = translator_func(en_text, LANGUAGE_CODES['kr'])
                    new_item['kr'] = translated_kr
            
            # Sắp xếp key theo thứ tự: en, ch, tc, kr
            new_item = TranslateUtils.sort_language_keys(new_item)
            return LocalTextData(new_item)
        
        return main_data

    @staticmethod
    def create_locale_data_from_main(main_data: LocalTextData, target_language: str, translator_func) -> LocalTextData:
        """
        Tạo dữ liệu locale từ main data
        
        Args:
            main_data: Dữ liệu main file
            target_language: Ngôn ngữ đích
            translator_func: Hàm dịch text
            
        Returns:
            LocalTextData cho locale file
        """
        if main_data.is_list:
            result = []
            for item in main_data.data:
                if isinstance(item, dict):
                    # Lấy text tiếng Anh từ item (hỗ trợ cả 'en' và combined keys)
                    en_text = JsonUtils.get_english_text(item)
                    if en_text and en_text.strip():
                        new_item = {}
                        
                        # Copy các key không phải ngôn ngữ
                        for key, value in item.items():
                            if key not in MAIN_LANGUAGE_KEYS and not COMBINED_KEY_WITH_EN_PATTERN.match(key):
                                new_item[key] = value
                        
                        # Dịch từ tiếng Anh sang ngôn ngữ đích
                        translated = translator_func(en_text, target_language)
                        new_item[LOCALE_COMBINED_KEY] = translated
                        
                        result.append(new_item)
                    else:
                        result.append(item)
                else:
                    result.append(item)
            
            return LocalTextData(result)
            
        elif main_data.is_dict:
            # Lấy text tiếng Anh từ dict
            en_text = JsonUtils.get_english_text(main_data.data)
            if en_text and en_text.strip():
                new_item = {}
                
                # Copy các key không phải ngôn ngữ
                for key, value in main_data.data.items():
                    if key not in MAIN_LANGUAGE_KEYS and not COMBINED_KEY_WITH_EN_PATTERN.match(key):
                        new_item[key] = value
                
                # Dịch từ tiếng Anh sang ngôn ngữ đích
                translated = translator_func(en_text, target_language)
                new_item[LOCALE_COMBINED_KEY] = translated
                
                return LocalTextData(new_item)
        
        # Nếu không có dữ liệu cần dịch, trả về dữ liệu gốc
        return main_data
