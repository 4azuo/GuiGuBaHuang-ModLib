#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Class and type definitions for localText translation system
"""

from dataclasses import dataclass
from typing import List, Dict, Any, Optional
from enum import Enum

from consts import TRANSLATION_CONFIG, PROGRESS_BAR_CONFIG, UI_ICONS

@dataclass
class ProgressConfig:
    """Configuration for progress bar"""
    width: int = PROGRESS_BAR_CONFIG['overall_width']
    fill_char: str = PROGRESS_BAR_CONFIG['fill_char']
    empty_char: str = PROGRESS_BAR_CONFIG['empty_char']
    show_percentage: bool = True
    show_time: bool = True
    show_count: bool = True
    prefix: str = ""
    suffix: str = ""

class FileType(Enum):
    """LocalText file type"""
    MAIN = "main"
    LOCALE = "locale"

class ProcessingStatus(Enum):
    """Processing status"""
    SUCCESS = "success"
    FAILED = "failed"
    SKIPPED = "skipped"
    INTERRUPTED = "interrupted"

@dataclass
class TranslationConfig:
    """Configuration for translation process"""
    target_languages: List[str]
    max_retries: int = TRANSLATION_CONFIG['max_retries']
    delay_between_requests: float = TRANSLATION_CONFIG['delay_between_requests']
    retry_delay: float = TRANSLATION_CONFIG['retry_delay']
    source_language: str = TRANSLATION_CONFIG['source_language']
    preserve_existing_translations: bool = False

@dataclass
class ProcessingStats:
    """Processing statistics"""
    processed_count: int = 0
    translated_count: int = 0
    failed_count: int = 0
    skipped_count: int = 0
    
    def __str__(self) -> str:
        return f"Processed: {self.processed_count}, Translated: {self.translated_count}, Failed: {self.failed_count}, Skipped: {self.skipped_count}"

@dataclass
class FileInfo:
    """Information about localText file"""
    path: str
    file_type: FileType
    language: Optional[str] = None
    main_file_path: Optional[str] = None
    
    def __str__(self) -> str:
        lang_info = f"({self.language})" if self.language else ""
        return f"{self.path} [{self.file_type.value}]{lang_info}"

@dataclass
class TranslationItem:
    """Item to be translated"""
    original_text: str
    target_language: str
    translated_text: Optional[str] = None
    status: ProcessingStatus = ProcessingStatus.SKIPPED
    error_message: Optional[str] = None

class LocalTextData:
    """Wrapper for localText JSON data"""
    
    def __init__(self, data: Any):
        self.data = data
        self._is_list = isinstance(data, list)
        self._is_dict = isinstance(data, dict)
    
    @property
    def is_list(self) -> bool:
        return self._is_list
    
    @property
    def is_dict(self) -> bool:
        return self._is_dict
    
    def get_translatable_items(self) -> List[Dict[str, Any]]:
        """Get list of translatable items"""
        if self.is_list:
            return [item for item in self.data if isinstance(item, dict) and 'en' in item]
        elif self.is_dict and 'en' in self.data:
            return [self.data]
        return []
    
    def to_dict(self) -> Any:
        """Return data as dict/list for serialization"""
        return self.data
