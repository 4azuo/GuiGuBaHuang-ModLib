#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Translation service for handling text translation
"""

import time
import re
from deep_translator import GoogleTranslator
from typing import Dict, Tuple

from consts import SKIP_TRANSLATION_TEXTS, FORMAT_PROTECTION_CONFIG, PLACEHOLDER_DETECTION_PATTERN
from data_types import ProcessingStats, TranslationConfig

class TranslationService:
    """Service để thực hiện dịch text"""
    
    def __init__(self, config: TranslationConfig):
        self.config = config
        self.stats = ProcessingStats()
        # Compile tất cả format patterns từ config
        self.format_patterns = [
            re.compile(pattern) for pattern in FORMAT_PROTECTION_CONFIG['patterns']
        ]
    
    def _protect_format_strings(self, text: str) -> Tuple[str, Dict[str, str]]:
        """
        Bảo vệ format strings bằng cách thay thế chúng bằng placeholders
        
        Args:
            text: Text cần bảo vệ
            
        Returns:
            Tuple[protected_text, format_map] - Text đã được bảo vệ và map các format strings
        """
        if not text or not isinstance(text, str):
            return text, {}
            
        format_map = {}
        protected_text = text
        
        # Tìm tất cả format strings từ các patterns
        all_matches = []
        for pattern in self.format_patterns:
            matches = pattern.findall(text)
            all_matches.extend(matches)
        
        if not all_matches:
            return text, {}
        
        # Loại bỏ duplicates và giữ thứ tự xuất hiện
        unique_matches = []
        seen = set()
        for match in all_matches:
            if match not in seen:
                unique_matches.append(match)
                seen.add(match)
        
        # Thay thế từng format string bằng placeholder an toàn
        prefix = FORMAT_PROTECTION_CONFIG['placeholder']['prefix']
        suffix = FORMAT_PROTECTION_CONFIG['placeholder']['suffix']
        
        for i, format_str in enumerate(unique_matches):
            placeholder = f"{prefix}{i}{suffix}"
            format_map[placeholder] = format_str
            protected_text = protected_text.replace(format_str, placeholder, 1)
            
        return protected_text, format_map
    
    def _restore_format_strings(self, translated_text: str, format_map: Dict[str, str]) -> str:
        """
        Khôi phục các format strings từ placeholders
        
        Args:
            translated_text: Text đã được dịch có chứa placeholders
            format_map: Map các placeholders và format strings tương ứng
            
        Returns:
            Text đã được khôi phục format strings
        """
        if not translated_text or not format_map:
            return translated_text
            
        restored_text = translated_text
        
        # Khôi phục từng format string
        for placeholder, format_str in format_map.items():
            restored_text = restored_text.replace(placeholder, format_str)
            
        return restored_text

    def translate_text(self, text: str, target_lang: str) -> str:
        """Dịch text sang ngôn ngữ đích"""
        try:
            if not text or text.strip() == "":
                return text
            
            # Skip nếu text nằm trong danh sách cần bỏ qua
            if text.strip() in SKIP_TRANSLATION_TEXTS:
                return text
            
            # Skip nếu text khớp với placeholder pattern (có thể bị biến đổi zenkaku)
            if PLACEHOLDER_DETECTION_PATTERN.match(text.strip()):
                return text

            # Bảo vệ format strings trước khi dịch
            protected_text, format_map = self._protect_format_strings(text)

            # Retry mechanism for network issues
            for attempt in range(self.config.max_retries):
                try:
                    translated_result = GoogleTranslator(
                        source=self.config.source_language, 
                        target=target_lang
                    ).translate(protected_text)
                    
                    # Khôi phục format strings sau khi dịch
                    result = self._restore_format_strings(translated_result, format_map)
                    
                    time.sleep(self.config.delay_between_requests)
                    self.stats.translated_count += 1
                    return result
                    
                except KeyboardInterrupt:
                    # Re-raise KeyboardInterrupt to allow clean exit
                    raise
                    
                except Exception as network_error:
                    if attempt < self.config.max_retries - 1:
                        # Silent retry - progress bar will show overall progress
                        time.sleep(self.config.retry_delay)
                    else:
                        self.stats.failed_count += 1
                        return text
                        
        except KeyboardInterrupt:
            raise
            
        except Exception as e:
            self.stats.failed_count += 1
            return text
