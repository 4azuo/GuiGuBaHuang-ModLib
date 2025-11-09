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
    """Utilities for handling translation and locale data"""
    
    @staticmethod
    def sort_language_keys(item_dict):
        """
        Sort keys in dictionary by order: en, ch, tc, kr, then other keys
        
        Args:
            item_dict: Dictionary to sort
            
        Returns:
            Dictionary sorted by key order
        """
        # Priority order for language keys
        language_order = ['__name', 'id', 'key', 'en', 'ch', 'tc', 'kr']
        
        # Create new dict with sorted order
        sorted_dict = {}
        
        # Add language keys in order
        for lang_key in language_order:
            if lang_key in item_dict:
                sorted_dict[lang_key] = item_dict[lang_key]
        
        # Add remaining keys (non-language keys)
        for key, value in item_dict.items():
            if key not in language_order:
                sorted_dict[key] = value
                
        return sorted_dict

    @staticmethod
    def normalize_main_file_keys(main_data):
        """
        Normalize combined keys in main file to simple keys
        Combined keys containing 'en' will be converted to 'en' key
        Also sort keys by order: en, ch, tc, kr
        
        Args:
            main_data: Main file data
            
        Returns:
            LocalTextData with normalized and sorted keys
        """
        if main_data.is_list:
            result = []
            for item in main_data.data:
                if isinstance(item, dict):
                    new_item = {}
                    
                    # Copy keys that are not combined keys
                    for key, value in item.items():
                        if not COMBINED_KEY_WITH_EN_PATTERN.match(key):
                            new_item[key] = value
                    
                    # Find combined key containing 'en' and convert to 'en'
                    for key, value in item.items():
                        if COMBINED_KEY_WITH_EN_PATTERN.match(key):
                            new_item['en'] = value
                            break  # Only take the first combined key
                    
                    # Sort keys in order: en, ch, tc, kr
                    new_item = TranslateUtils.sort_language_keys(new_item)
                    result.append(new_item)
                else:
                    result.append(item)
            
            return LocalTextData(result)
            
        elif main_data.is_dict:
            new_item = {}
            
            # Copy keys that are not combined keys
            for key, value in main_data.data.items():
                if not COMBINED_KEY_WITH_EN_PATTERN.match(key):
                    new_item[key] = value
            
            # Find combined key containing 'en' and convert to 'en'
            for key, value in main_data.data.items():
                if COMBINED_KEY_WITH_EN_PATTERN.match(key):
                    new_item['en'] = value
                    break  # Only take the first combined key
            
            # Sort keys in order: en, ch, tc, kr
            new_item = TranslateUtils.sort_language_keys(new_item)
            return LocalTextData(new_item)
        
        return main_data

    @staticmethod
    def translate_main_file_text(main_data, translator_func):
        """
        Translate text in main file (e.g., update English fields)
        
        Args:
            main_data: Main file data
            translator_func: Text translation function
            
        Returns:
            LocalTextData with translated text
        """
        if main_data.is_list:
            result = []
            for item in main_data.data:
                if isinstance(item, dict) and 'en' in item:
                    new_item = item.copy()
                    
                    # Translate English to other languages in main file
                    en_text = item['en']
                    item_id = item.get('id', 'N/A')
                    if en_text and en_text.strip():
                        # Translate to Vietnamese and update 'vi' field (if present)
                        if 'vi' not in new_item or not new_item['vi']:
                            translated_vi = translator_func(en_text, 'vi', item_id)
                            new_item['vi'] = translated_vi
                        
                        # Can add translation to other languages
                        # if 'ch' not in new_item or not new_item['ch']:
                        #     translated_ch = translator_func(en_text, 'zh')
                        #     new_item['ch'] = translated_ch
                    
                    result.append(new_item)
                else:
                    result.append(item)
            
            return LocalTextData(result)
            
        elif main_data.is_dict and 'en' in main_data.data:
            new_item = main_data.data.copy()
            
            # Translate English to Vietnamese in main file
            en_text = main_data.data['en']
            item_id = main_data.data.get('id', 'N/A')
            if en_text and en_text.strip():
                # Translate to Vietnamese and update 'vi' field (if present)
                if 'vi' not in new_item or not new_item['vi']:
                    translated_vi = translator_func(en_text, 'vi', item_id)
                    new_item['vi'] = translated_vi
            
            return LocalTextData(new_item)
        
        return main_data

    @staticmethod
    def analyze_main_file_text(main_data, progress_callback):
        """
        Analyze text in main file and count the number of translatable texts
        
        Args:
            main_data: Main file data
            progress_callback: Callback function to update progress
            
        Returns:
            Number of texts analyzed
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
        Translate and update missing language fields in main file
        Main file structure: en, ch, tc, kr
        
        Args:
            main_data: Main file data
            translator_func: Text translation function
            preserve_existing: If True, keep existing translations, only translate new ones
            
        Returns:
            LocalTextData with updated language fields
        """
        if main_data.is_list:
            result = []
            for item in main_data.data:
                if isinstance(item, dict):
                    new_item = item.copy()
                    
                    # Get English text from item (supports both 'en' and combined keys)
                    en_text = JsonUtils.get_english_text(item)
                    
                    # Get item id for progress display
                    item_id = item.get('id', 'N/A')
                    
                    if en_text and en_text.strip():
                        # Translate to Chinese Simplified (ch) if missing or empty
                        if not TranslateUtils._has_valid_translation(new_item, 'ch', preserve_existing):
                            translated_ch = translator_func(en_text, LANGUAGE_CODES['ch'], item_id)
                            new_item['ch'] = translated_ch
                        
                        # Translate to Traditional Chinese (tc) if missing or empty
                        if not TranslateUtils._has_valid_translation(new_item, 'tc', preserve_existing):
                            translated_tc = translator_func(en_text, LANGUAGE_CODES['tc'], item_id)
                            new_item['tc'] = translated_tc
                        
                        # Translate to Korean (kr) if missing or empty
                        if not TranslateUtils._has_valid_translation(new_item, 'kr', preserve_existing):
                            translated_kr = translator_func(en_text, LANGUAGE_CODES['kr'], item_id)
                            new_item['kr'] = translated_kr
                    
                    # Sort keys in order: en, ch, tc, kr
                    new_item = TranslateUtils.sort_language_keys(new_item)
                    result.append(new_item)
                else:
                    result.append(item)
            
            return LocalTextData(result)
            
        elif main_data.is_dict:
            new_item = main_data.data.copy()
            
            # Get English text from dict
            en_text = JsonUtils.get_english_text(main_data.data)
            
            # Get dict id for progress display
            item_id = main_data.data.get('id', 'N/A')
            
            if en_text and en_text.strip():
                # Translate to missing or empty languages
                if not TranslateUtils._has_valid_translation(new_item, 'ch', preserve_existing):
                    translated_ch = translator_func(en_text, LANGUAGE_CODES['ch'], item_id)
                    new_item['ch'] = translated_ch
                
                if not TranslateUtils._has_valid_translation(new_item, 'tc', preserve_existing):
                    translated_tc = translator_func(en_text, LANGUAGE_CODES['tc'], item_id)
                    new_item['tc'] = translated_tc
                
                if not TranslateUtils._has_valid_translation(new_item, 'kr', preserve_existing):
                    translated_kr = translator_func(en_text, LANGUAGE_CODES['kr'], item_id)
                    new_item['kr'] = translated_kr
            
            # Sort keys in order: en, ch, tc, kr
            new_item = TranslateUtils.sort_language_keys(new_item)
            return LocalTextData(new_item)
        
        return main_data

    @staticmethod
    def create_locale_data_from_main(main_data: LocalTextData, target_language: str, translator_func, existing_locale_data=None, preserve_existing=False) -> LocalTextData:
        """
        Create locale data from main data
        
        Args:
            main_data: Main file data
            target_language: Target language
            translator_func: Text translation function
            existing_locale_data: Existing locale data (if preserve mode)
            preserve_existing: If True, keep existing translations, only translate new ones
            
        Returns:
            LocalTextData for locale file
        """
        if main_data.is_list:
            result = []
            for item in main_data.data:
                if isinstance(item, dict):
                    # Get English text from item (supports both 'en' and combined keys)
                    en_text = JsonUtils.get_english_text(item)
                    if en_text and en_text.strip():
                        new_item = {}
                        
                        # Copy non-language keys
                        for key, value in item.items():
                            if key not in MAIN_LANGUAGE_KEYS and not COMBINED_KEY_WITH_EN_PATTERN.match(key):
                                new_item[key] = value
                        
                        # Get item id for progress display
                        item_id = item.get('id', 'N/A')
                        
                        # Check if this entry already has translation in locale file (by key/id)
                        existing_translation = TranslateUtils._find_existing_translation_by_key(item, existing_locale_data, preserve_existing)
                        
                        if existing_translation:
                            # Use existing translation from locale file
                            new_item[LOCALE_COMBINED_KEY] = existing_translation
                        else:
                            # Translate from English to target language
                            translated = translator_func(en_text, target_language, item_id)
                            new_item[LOCALE_COMBINED_KEY] = translated
                        
                        result.append(new_item)
                    else:
                        result.append(item)
                else:
                    result.append(item)
            
            return LocalTextData(result)
            
        elif main_data.is_dict:
            # Get English text from dict
            en_text = JsonUtils.get_english_text(main_data.data)
            if en_text and en_text.strip():
                new_item = {}
                
                # Copy non-language keys
                for key, value in main_data.data.items():
                    if key not in MAIN_LANGUAGE_KEYS and not COMBINED_KEY_WITH_EN_PATTERN.match(key):
                        new_item[key] = value
                
                # Get item id for progress display
                item_id = main_data.data.get('id', 'N/A')
                
                # Check if this entry already has translation in locale file (by key/id)
                existing_translation = TranslateUtils._find_existing_translation_by_key(main_data.data, existing_locale_data, preserve_existing)
                
                if existing_translation:
                    # Use existing translation from locale file
                    new_item[LOCALE_COMBINED_KEY] = existing_translation
                else:
                    # Translate from English to target language
                    translated = translator_func(en_text, target_language, item_id)
                    new_item[LOCALE_COMBINED_KEY] = translated
                
                return LocalTextData(new_item)
        
        # If no data needs translation, return original data
        return main_data
    
    @staticmethod
    def _get_existing_translation_from_main(main_item, target_language, preserve_existing):
        """
        Get existing translation from main item for target language
        
        Args:
            main_item: Item from main file (dict)
            target_language: Target language (vi, es, fr, ...)
            preserve_existing: Whether to preserve existing translations
            
        Returns:
            str: Existing translation or None if not found
        """
        if not preserve_existing or not isinstance(main_item, dict):
            return None
            
        # Check if main item has target language field
        if target_language in main_item:
            translation = main_item[target_language]
            if translation and isinstance(translation, str) and translation.strip():
                return translation.strip()
        
        return None
    
    @staticmethod
    def _find_existing_translation_by_key(main_item, existing_locale_data, preserve_existing):
        """
        Find existing translation in locale data based on key/id of main item
        
        Args:
            main_item: Item from main file (dict)  
            existing_locale_data: Existing locale data (LocalTextData or None)
            preserve_existing: Whether to preserve existing translations
            
        Returns:
            str: Existing translation or None if not found
        """
        if not preserve_existing or not existing_locale_data or not isinstance(main_item, dict):
            return None
            
        # Find identifier for main item (priority: key > id)
        main_identifier = None
        main_id_key = None
        
        for identifier_key in ['key', 'id']:
            if identifier_key in main_item and main_item[identifier_key]:
                main_identifier = main_item[identifier_key]
                main_id_key = identifier_key
                break
                
        if not main_identifier:
            return None  # No identifier for comparison
            
        # Search in existing locale data
        if existing_locale_data.is_list:
            for locale_item in existing_locale_data.data:
                if isinstance(locale_item, dict) and main_id_key in locale_item:
                    if locale_item[main_id_key] == main_identifier:
                        # Found entry with matching key/id, get translation
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
        Find existing translation in locale data based on main item
        
        Args:
            main_item: Item from main file (dict)
            existing_locale_data: Existing locale data (LocalTextData)
            
        Returns:
            str: Existing translation or None if not found
        """
        if not existing_locale_data or not existing_locale_data.data:
            return None
        
        # Find corresponding item in locale data
        if existing_locale_data.is_list:
            for locale_item in existing_locale_data.data:
                if isinstance(locale_item, dict):
                    # Compare by key/id to find corresponding item
                    if TranslateUtils._items_match(main_item, locale_item):
                        return locale_item.get(LOCALE_COMBINED_KEY)
        elif existing_locale_data.is_dict:
            # If single dict, check if it matches
            if TranslateUtils._items_match(main_item, existing_locale_data.data):
                return existing_locale_data.data.get(LOCALE_COMBINED_KEY)
        
        return None
    
    @staticmethod
    def _items_match(main_item, locale_item):
        """
        Check if main item and locale item match each other
        
        Args:
            main_item: Item from main file (dict)
            locale_item: Item from locale file (dict)
            
        Returns:
            bool: True if they match
        """
        if not isinstance(main_item, dict) or not isinstance(locale_item, dict):
            return False
        
        # Priority comparison by identifier keys
        for key in ['id', 'key', '__name']:
            if key in main_item and key in locale_item:
                return main_item[key] == locale_item[key]
        
        # If no identifier key, compare by English text
        main_en = JsonUtils.get_english_text(main_item)
        if main_en:
            # Find corresponding English text in locale item (may be combined key)
            for k, v in locale_item.items():
                if k not in MAIN_LANGUAGE_KEYS and v == main_en:
                    return True
        
        return False
    
    @staticmethod
    def _has_valid_translation(item, lang_key, preserve_existing):
        """
        Check if an entry has valid translation for specific language
        
        Args:
            item: Dictionary item to check
            lang_key: Language key to check ('ch', 'tc', 'kr', etc.)
            preserve_existing: Whether to preserve existing translations
            
        Returns:
            bool: True if has valid translation and should skip translation
        """
        if not preserve_existing:
            return False  # No preserve -> always retranslate
            
        if lang_key not in item:
            return False  # No key -> need translation
            
        translation = item[lang_key]
        if not translation or not isinstance(translation, str):
            return False  # Empty value or not string -> need translation
            
        translation = translation.strip()
        if not translation:
            return False  # Empty string after strip -> need translation
            
        return True  # Has valid translation -> no need to translate
