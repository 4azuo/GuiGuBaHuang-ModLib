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
                    item_id = item.get('id', 'N/A')
                    if en_text and en_text.strip():
                        # Dịch sang tiếng Việt và cập nhật trường 'vi' (nếu có)
                        if 'vi' not in new_item or not new_item['vi']:
                            translated_vi = translator_func(en_text, 'vi', item_id)
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
            item_id = main_data.data.get('id', 'N/A')
            if en_text and en_text.strip():
                # Dịch sang tiếng Việt và cập nhật trường 'vi' (nếu có)
                if 'vi' not in new_item or not new_item['vi']:
                    translated_vi = translator_func(en_text, 'vi', item_id)
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
    def translate_main_file_languages(main_data, translator_func, preserve_existing=False):
        """
        Dịch và cập nhật các trường ngôn ngữ thiếu trong main file
        Main file có cấu trúc: en, ch, tc, kr
        
        Args:
            main_data: Dữ liệu main file
            translator_func: Hàm dịch text
            preserve_existing: Nếu True, giữ lại bản dịch đã có, chỉ dịch các từ mới
            
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
                    
                    # Lấy id của item để hiển thị progress
                    item_id = item.get('id', 'N/A')
                    
                    if en_text and en_text.strip():
                        # Dịch sang Chinese Simplified (ch) nếu thiếu hoặc rỗng
                        if not TranslateUtils._has_valid_translation(new_item, 'ch', preserve_existing):
                            translated_ch = translator_func(en_text, LANGUAGE_CODES['ch'], item_id)
                            new_item['ch'] = translated_ch
                        
                        # Dịch sang Traditional Chinese (tc) nếu thiếu hoặc rỗng
                        if not TranslateUtils._has_valid_translation(new_item, 'tc', preserve_existing):
                            translated_tc = translator_func(en_text, LANGUAGE_CODES['tc'], item_id)
                            new_item['tc'] = translated_tc
                        
                        # Dịch sang Korean (kr) nếu thiếu hoặc rỗng
                        if not TranslateUtils._has_valid_translation(new_item, 'kr', preserve_existing):
                            translated_kr = translator_func(en_text, LANGUAGE_CODES['kr'], item_id)
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
            
            # Lấy id của dict để hiển thị progress
            item_id = main_data.data.get('id', 'N/A')
            
            if en_text and en_text.strip():
                # Dịch sang các ngôn ngữ thiếu hoặc rỗng
                if not TranslateUtils._has_valid_translation(new_item, 'ch', preserve_existing):
                    translated_ch = translator_func(en_text, LANGUAGE_CODES['ch'], item_id)
                    new_item['ch'] = translated_ch
                
                if not TranslateUtils._has_valid_translation(new_item, 'tc', preserve_existing):
                    translated_tc = translator_func(en_text, LANGUAGE_CODES['tc'], item_id)
                    new_item['tc'] = translated_tc
                
                if not TranslateUtils._has_valid_translation(new_item, 'kr', preserve_existing):
                    translated_kr = translator_func(en_text, LANGUAGE_CODES['kr'], item_id)
                    new_item['kr'] = translated_kr
            
            # Sắp xếp key theo thứ tự: en, ch, tc, kr
            new_item = TranslateUtils.sort_language_keys(new_item)
            return LocalTextData(new_item)
        
        return main_data

    @staticmethod
    def create_locale_data_from_main(main_data: LocalTextData, target_language: str, translator_func, existing_locale_data=None, preserve_existing=False) -> LocalTextData:
        """
        Tạo dữ liệu locale từ main data
        
        Args:
            main_data: Dữ liệu main file
            target_language: Ngôn ngữ đích
            translator_func: Hàm dịch text
            existing_locale_data: Dữ liệu locale đã có (nếu preserve mode)
            preserve_existing: Nếu True, giữ lại bản dịch đã có, chỉ dịch các từ mới
            
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
                        
                        # Lấy id của item để hiển thị progress
                        item_id = item.get('id', 'N/A')
                        
                        # Kiểm tra xem entry này đã có bản dịch trong locale file chưa (theo key/id)
                        existing_translation = TranslateUtils._find_existing_translation_by_key(item, existing_locale_data, preserve_existing)
                        
                        if existing_translation:
                            # Sử dụng bản dịch đã có từ locale file
                            new_item[LOCALE_COMBINED_KEY] = existing_translation
                        else:
                            # Dịch từ tiếng Anh sang ngôn ngữ đích
                            translated = translator_func(en_text, target_language, item_id)
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
                
                # Lấy id của item để hiển thị progress
                item_id = main_data.data.get('id', 'N/A')
                
                # Kiểm tra xem entry này đã có bản dịch trong locale file chưa (theo key/id)
                existing_translation = TranslateUtils._find_existing_translation_by_key(main_data.data, existing_locale_data, preserve_existing)
                
                if existing_translation:
                    # Sử dụng bản dịch đã có từ locale file
                    new_item[LOCALE_COMBINED_KEY] = existing_translation
                else:
                    # Dịch từ tiếng Anh sang ngôn ngữ đích
                    translated = translator_func(en_text, target_language, item_id)
                    new_item[LOCALE_COMBINED_KEY] = translated
                
                return LocalTextData(new_item)
        
        # Nếu không có dữ liệu cần dịch, trả về dữ liệu gốc
        return main_data
    
    @staticmethod
    def _get_existing_translation_from_main(main_item, target_language, preserve_existing):
        """
        Lấy bản dịch đã có từ main item cho target language
        
        Args:
            main_item: Item từ main file (dict)
            target_language: Ngôn ngữ đích (vi, es, fr, ...)
            preserve_existing: Có bảo tồn bản dịch đã có không
            
        Returns:
            str: Bản dịch đã có hoặc None nếu không tìm thấy
        """
        if not preserve_existing or not isinstance(main_item, dict):
            return None
            
        # Kiểm tra xem main item có trường ngôn ngữ target không
        if target_language in main_item:
            translation = main_item[target_language]
            if translation and isinstance(translation, str) and translation.strip():
                return translation.strip()
        
        return None
    
    @staticmethod
    def _find_existing_translation_by_key(main_item, existing_locale_data, preserve_existing):
        """
        Tìm bản dịch đã có trong locale data dựa trên key/id của main item
        
        Args:
            main_item: Item từ main file (dict)  
            existing_locale_data: Dữ liệu locale đã có (LocalTextData hoặc None)
            preserve_existing: Có bảo tồn bản dịch đã có không
            
        Returns:
            str: Bản dịch đã có hoặc None nếu không tìm thấy
        """
        if not preserve_existing or not existing_locale_data or not isinstance(main_item, dict):
            return None
            
        # Tìm identifier cho main item (ưu tiên key > id)
        main_identifier = None
        main_id_key = None
        
        for identifier_key in ['key', 'id']:
            if identifier_key in main_item and main_item[identifier_key]:
                main_identifier = main_item[identifier_key]
                main_id_key = identifier_key
                break
                
        if not main_identifier:
            return None  # Không có identifier để so sánh
            
        # Tìm trong existing locale data
        if existing_locale_data.is_list:
            for locale_item in existing_locale_data.data:
                if isinstance(locale_item, dict) and main_id_key in locale_item:
                    if locale_item[main_id_key] == main_identifier:
                        # Tìm thấy entry trùng key/id, lấy bản dịch
                        translation = locale_item.get(LOCALE_COMBINED_KEY)
                        if translation and isinstance(translation, str) and translation.strip():
                            return translation.strip()
                        
        elif existing_locale_data.is_dict and main_id_key in existing_locale_data.data:
            if existing_locale_data.data[main_id_key] == main_identifier:
                translation = existing_locale_data.data.get(LOCALE_COMBINED_KEY)
                if translation and isinstance(translation, str) and translation.strip():
                    return translation.strip()
                    
        return None
    
    @staticmethod
    def _find_existing_translation(main_item, existing_locale_data):
        """
        Tìm bản dịch đã có trong locale data dựa trên main item
        
        Args:
            main_item: Item từ main file (dict)
            existing_locale_data: Dữ liệu locale đã có (LocalTextData)
            
        Returns:
            str: Bản dịch đã có hoặc None nếu không tìm thấy
        """
        if not existing_locale_data or not existing_locale_data.data:
            return None
        
        # Tìm item tương ứng trong locale data
        if existing_locale_data.is_list:
            for locale_item in existing_locale_data.data:
                if isinstance(locale_item, dict):
                    # So sánh theo key/id để tìm item tương ứng
                    if TranslateUtils._items_match(main_item, locale_item):
                        return locale_item.get(LOCALE_COMBINED_KEY)
        elif existing_locale_data.is_dict:
            # Nếu là dict đơn lẻ, kiểm tra xem có khớp không
            if TranslateUtils._items_match(main_item, existing_locale_data.data):
                return existing_locale_data.data.get(LOCALE_COMBINED_KEY)
        
        return None
    
    @staticmethod
    def _items_match(main_item, locale_item):
        """
        Kiểm tra xem main item và locale item có khớp với nhau không
        
        Args:
            main_item: Item từ main file (dict)
            locale_item: Item từ locale file (dict)
            
        Returns:
            bool: True nếu khớp
        """
        if not isinstance(main_item, dict) or not isinstance(locale_item, dict):
            return False
        
        # Ưu tiên so sánh theo các key định danh
        for key in ['id', 'key', '__name']:
            if key in main_item and key in locale_item:
                return main_item[key] == locale_item[key]
        
        # Nếu không có key định danh, so sánh theo English text
        main_en = JsonUtils.get_english_text(main_item)
        if main_en:
            # Tìm English text tương ứng trong locale item (có thể là combined key)
            for k, v in locale_item.items():
                if k not in MAIN_LANGUAGE_KEYS and v == main_en:
                    return True
        
        return False
    
    @staticmethod
    def _has_valid_translation(item, lang_key, preserve_existing):
        """
        Kiểm tra xem một entry có bản dịch hợp lệ cho ngôn ngữ cụ thể chưa
        
        Args:
            item: Dictionary item cần kiểm tra
            lang_key: Key ngôn ngữ cần kiểm tra ('ch', 'tc', 'kr', v.v.)
            preserve_existing: Có bảo tồn bản dịch đã có không
            
        Returns:
            bool: True nếu đã có bản dịch hợp lệ và nên bỏ qua dịch
        """
        if not preserve_existing:
            return False  # Không preserve -> luôn dịch lại
            
        if lang_key not in item:
            return False  # Không có key -> cần dịch
            
        translation = item[lang_key]
        if not translation or not isinstance(translation, str):
            return False  # Giá trị rỗng hoặc không phải string -> cần dịch
            
        translation = translation.strip()
        if not translation:
            return False  # String rỗng sau khi strip -> cần dịch
            
        return True  # Có bản dịch hợp lệ -> không cần dịch
