#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Định nghĩa các class và types cho hệ thống dịch localText
"""

from dataclasses import dataclass
from typing import List, Dict, Any, Optional
from enum import Enum

from consts import TRANSLATION_CONFIG, PROGRESS_BAR_CONFIG, UI_ICONS

@dataclass
class ProgressConfig:
    """Configuration for progress bar"""
    width: int = PROGRESS_BAR_CONFIG['width']
    fill_char: str = UI_ICONS.get('success', '█')
    empty_char: str = '░'
    show_percentage: bool = True
    show_time: bool = True
    show_count: bool = True
    prefix: str = ""
    suffix: str = ""

class FileType(Enum):
    """Loại file localText"""
    MAIN = "main"
    LOCALE = "locale"

class ProcessingStatus(Enum):
    """Trạng thái xử lý"""
    SUCCESS = "success"
    FAILED = "failed"
    SKIPPED = "skipped"
    INTERRUPTED = "interrupted"

@dataclass
class TranslationConfig:
    """Cấu hình cho quá trình dịch"""
    target_languages: List[str]
    max_retries: int = TRANSLATION_CONFIG['max_retries']
    delay_between_requests: float = TRANSLATION_CONFIG['delay_between_requests']
    retry_delay: float = TRANSLATION_CONFIG['retry_delay']
    source_language: str = TRANSLATION_CONFIG['source_language']
    preserve_existing_translations: bool = False

@dataclass
class ProcessingStats:
    """Thống kê quá trình xử lý"""
    processed_count: int = 0
    translated_count: int = 0
    failed_count: int = 0
    skipped_count: int = 0
    
    def __str__(self) -> str:
        return f"Processed: {self.processed_count}, Translated: {self.translated_count}, Failed: {self.failed_count}, Skipped: {self.skipped_count}"

@dataclass
class FileInfo:
    """Thông tin về file localText"""
    path: str
    file_type: FileType
    language: Optional[str] = None
    main_file_path: Optional[str] = None
    
    def __str__(self) -> str:
        lang_info = f"({self.language})" if self.language else ""
        return f"{self.path} [{self.file_type.value}]{lang_info}"

@dataclass
class TranslationItem:
    """Item cần được dịch"""
    original_text: str
    target_language: str
    translated_text: Optional[str] = None
    status: ProcessingStatus = ProcessingStatus.SKIPPED
    error_message: Optional[str] = None

class LocalTextData:
    """Wrapper cho dữ liệu localText JSON"""
    
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
        """Lấy danh sách các item có thể dịch được"""
        if self.is_list:
            return [item for item in self.data if isinstance(item, dict) and 'en' in item]
        elif self.is_dict and 'en' in self.data:
            return [self.data]
        return []
    
    def to_dict(self) -> Any:
        """Trả về dữ liệu dưới dạng dict/list để serialize"""
        return self.data
