#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Translation service for handling text translation
"""

import time
import re
from deep_translator import GoogleTranslator

from consts import TRANSLATION_CONFIG, SKIP_TRANSLATION_PATTERNS
from data_types import ProcessingStats, TranslationConfig

class TranslationService:
    """Service để thực hiện dịch text"""
    
    def __init__(self, config: TranslationConfig):
        self.config = config
        self.stats = ProcessingStats()
    
    def translate_text(self, text: str, target_lang: str) -> str:
        """Dịch text sang ngôn ngữ đích"""
        try:
            if not text or text.strip() == "":
                return text
            
            # Skip nếu text khớp với pattern
            for pattern in SKIP_TRANSLATION_PATTERNS:
                if re.match(pattern, text):
                    return text
            
            # Skip nếu text quá ngắn (1-2 ký tự)
            if len(text.strip()) <= TRANSLATION_CONFIG['min_text_length']:
                return text
            
            # Retry mechanism for network issues
            for attempt in range(self.config.max_retries):
                try:
                    result = GoogleTranslator(
                        source=self.config.source_language, 
                        target=target_lang
                    ).translate(text)
                    
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
