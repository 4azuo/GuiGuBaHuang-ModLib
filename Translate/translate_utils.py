#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Translation utilities
"""

try:
    from consts import MAIN_LANGUAGE_KEYS, LOCALE_COMBINED_KEY
    from data_types import LocalTextData
except ImportError:
    # Fallback cho trường hợp import trực tiếp
    import sys
    import os
    sys.path.append(os.path.dirname(os.path.abspath(__file__)))
    from consts import MAIN_LANGUAGE_KEYS, LOCALE_COMBINED_KEY
    from data_types import LocalTextData

# Translation-specific utilities
class TranslateUtils:
    """Utilities cho việc xử lý translation và locale data"""
    
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
                if isinstance(item, dict) and 'en' in item:
                    new_item = item.copy()
                    en_text = item['en']
                    
                    if en_text and en_text.strip():
                        # Dịch sang Chinese Simplified (ch) nếu thiếu
                        if 'ch' not in new_item or not new_item['ch']:
                            translated_ch = translator_func(en_text, 'zh-CN')
                            new_item['ch'] = translated_ch
                        
                        # Dịch sang Traditional Chinese (tc) nếu thiếu  
                        if 'tc' not in new_item or not new_item['tc']:
                            translated_tc = translator_func(en_text, 'zh-TW')
                            new_item['tc'] = translated_tc
                        
                        # Dịch sang Korean (kr) nếu thiếu
                        if 'kr' not in new_item or not new_item['kr']:
                            translated_kr = translator_func(en_text, 'ko')
                            new_item['kr'] = translated_kr
                    
                    result.append(new_item)
                else:
                    result.append(item)
            
            return LocalTextData(result)
            
        elif main_data.is_dict and 'en' in main_data.data:
            new_item = main_data.data.copy()
            en_text = main_data.data['en']
            
            if en_text and en_text.strip():
                # Dịch sang các ngôn ngữ thiếu
                if 'ch' not in new_item or not new_item['ch']:
                    translated_ch = translator_func(en_text, 'zh-CN')
                    new_item['ch'] = translated_ch
                
                if 'tc' not in new_item or not new_item['tc']:
                    translated_tc = translator_func(en_text, 'zh-TW')
                    new_item['tc'] = translated_tc
                
                if 'kr' not in new_item or not new_item['kr']:
                    translated_kr = translator_func(en_text, 'ko')
                    new_item['kr'] = translated_kr
            
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
                if isinstance(item, dict) and 'en' in item:
                    new_item = {}
                    
                    # Copy các key không phải ngôn ngữ
                    for key, value in item.items():
                        if key not in MAIN_LANGUAGE_KEYS:
                            new_item[key] = value
                    
                    # Dịch từ tiếng Anh sang ngôn ngữ đích
                    en_text = item['en']
                    if en_text and en_text.strip():
                        translated = translator_func(en_text, target_language)
                        new_item[LOCALE_COMBINED_KEY] = translated
                    else:
                        new_item[LOCALE_COMBINED_KEY] = en_text
                    
                    result.append(new_item)
                else:
                    result.append(item)
            
            return LocalTextData(result)
            
        elif main_data.is_dict and 'en' in main_data.data:
            new_item = {}
            
            # Copy các key không phải ngôn ngữ
            for key, value in main_data.data.items():
                if key not in MAIN_LANGUAGE_KEYS:
                    new_item[key] = value
            
            # Dịch từ tiếng Anh sang ngôn ngữ đích
            en_text = main_data.data['en']
            if en_text and en_text.strip():
                translated = translator_func(en_text, target_language)
                new_item[LOCALE_COMBINED_KEY] = translated
            else:
                new_item[LOCALE_COMBINED_KEY] = en_text
            
            return LocalTextData(new_item)
        
        # Nếu không có dữ liệu cần dịch, trả về dữ liệu gốc
        return main_data
