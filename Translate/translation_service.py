#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Translation service for handling text translation
"""

import time
import re
import threading
from deep_translator import GoogleTranslator
from typing import Dict, Tuple

from consts import SKIP_TRANSLATION_TEXTS, FORMAT_PROTECTION_CONFIG, CONTEXT_CONFIG
from data_types import ProcessingStats, TranslationConfig
from terminology_utils import TerminologyProcessor

class TranslationService:
    """Service for performing text translation"""
    
    def __init__(self, config: TranslationConfig):
        self.config = config
        self.stats = ProcessingStats()
        self._stats_lock = threading.Lock()  # Thread safety lock for stats
        # Compile all format patterns from config
        self.format_patterns = [
            re.compile(pattern) for pattern in FORMAT_PROTECTION_CONFIG['patterns']
        ]
        # Initialize terminology processor
        self.terminology_processor = TerminologyProcessor()
    
    def _protect_format_strings(self, text: str) -> Tuple[str, Dict[int, str]]:
        """
        Protect format strings by replacing them with placeholders
        
        Args:
            text: Text to protect
            
        Returns:
            Tuple[protected_text, format_map] - Protected text and format strings map
        """
        if not text or not isinstance(text, str):
            return text, {}
            
        format_map = {}
        protected_text = text
        
        # Find all format strings from patterns
        all_matches = []
        for pattern in self.format_patterns:
            matches = pattern.findall(text)
            all_matches.extend(matches)
        
        if not all_matches:
            return text, {}
        
        # Remove duplicates and maintain order of appearance
        unique_matches = []
        seen = set()
        for match in all_matches:
            if match not in seen:
                unique_matches.append(match)
                seen.add(match)
        
        # Replace each format string with safe placeholder
        prefix = FORMAT_PROTECTION_CONFIG['placeholder']['prefix']
        suffix = FORMAT_PROTECTION_CONFIG['placeholder']['suffix']
        
        for i, format_str in enumerate(unique_matches):
            placeholder = f"{prefix}{i}{suffix}"
            format_map[i] = format_str
            protected_text = protected_text.replace(format_str, placeholder)
            
        return protected_text, format_map
    
    def _restore_format_strings(self, translated_text: str, format_map: Dict[int, str]) -> str:
        """
        Restore format strings from placeholders using index-based lookup
        
        Args:
            translated_text: Translated text containing placeholders
            format_map: Map of index -> format_string
            
        Returns:
            Text with format strings restored
        """
        if not translated_text or not format_map:
            return translated_text
            
        restored_text = translated_text
        placeholder_pattern = FORMAT_PROTECTION_CONFIG['placeholder']['placeholder_marker']
        
        # Find all placeholder matches in text and replace by index
        def replace_placeholder(match):
            # Get index from capture group 2
            index = int(match.group(2))
            if index in format_map:
                return format_map[index]
            
            # If index not found in format_map, return original placeholder
            return match.group(0)
        
        # Use regex to replace all placeholders
        return re.sub(placeholder_pattern, replace_placeholder, restored_text)

    def _add_context(self, text: str, target_lang: str) -> str:
        """
        Add context to text before translation
        
        Args:
            text: Text that needs context added
            target_lang: Target language (not used but kept for compatibility)
            
        Returns:
            Text with context added
        """
        if not text or not isinstance(text, str):
            return text
            
        context_prefix = CONTEXT_CONFIG['context_prefix']
        if context_prefix:
            return context_prefix + text
        return text
    
    def _remove_context(self, translated_text: str) -> str:
        """
        Remove context from translated text using regex pattern
        
        Args:
            translated_text: Translated text containing context
            
        Returns:
            Text with context removed
        """
        if not translated_text or not isinstance(translated_text, str):
            return translated_text
            
        context_marker = CONTEXT_CONFIG['context_marker']
        if context_marker:
            # Use regex to find and remove context marker
            cleaned_text = re.sub(context_marker, '', translated_text).strip()
            return cleaned_text
        
        return translated_text

    def translate_text(self, text: str, target_lang: str) -> str:
        """Translate text to target language"""
        try:
            if not text or text.strip() == "":
                return text
            
            # Skip if text is in the skip translation list
            if text.strip() in SKIP_TRANSLATION_TEXTS:
                return text

            # Protect terminology before other processing
            terminology_protected_text, terminology_map = self.terminology_processor.protect_terms(text)

            # Protect format strings before translation
            protected_text, format_map = self._protect_format_strings(terminology_protected_text)
            
            # Add context before translation
            contextualized_text = self._add_context(protected_text, target_lang)

            # Retry mechanism for network issues
            for attempt in range(self.config.max_retries):
                try:
                    translated_result = GoogleTranslator(
                        source=self.config.source_language, 
                        target=target_lang
                    ).translate(contextualized_text)
                    
                    # Remove context after translation
                    decontextualized_result = self._remove_context(translated_result)
                    
                    # Restore format strings after translation
                    format_restored_result = self._restore_format_strings(decontextualized_result, format_map)
                    
                    # Restore terminology last
                    result = self.terminology_processor.restore_terms(format_restored_result, terminology_map)
                    
                    time.sleep(self.config.delay_between_requests)
                    with self._stats_lock:
                        self.stats.translated_count += 1
                    return result
                    
                except Exception:
                    if attempt < self.config.max_retries - 1:
                        # Silent retry - progress bar will show overall progress
                        time.sleep(self.config.retry_delay)
                    else:
                        with self._stats_lock:
                            self.stats.failed_count += 1
                        # return text
                        raise
            
        except Exception:
            with self._stats_lock:
                self.stats.failed_count += 1
            # return text
            raise
