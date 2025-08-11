#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Constants cho hệ thống dịch localText
"""

# Ngôn ngữ mặc định được hỗ trợ
DEFAULT_TARGET_LANGUAGES = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la']

# Mapping các mã ngôn ngữ
LANGUAGE_CODES = {
    'ch': 'zh-CN',  # Chinese Simplified
    'tc': 'zh-TW',  # Traditional Chinese  
    'kr': 'ko'      # Korean
}

# Các key ngôn ngữ chính trong main files
MAIN_LANGUAGE_KEYS = ['en', 'ch', 'tc', 'kr']

# Key gộp cho locale files
LOCALE_COMBINED_KEY = 'en|ch|tc|kr'

# Cấu hình dịch
TRANSLATION_CONFIG = {
    'max_retries': 3,
    'delay_between_requests': 0.2,  # giây
    'retry_delay': 1.0,  # giây
    'source_language': 'en'
}

# Patterns để skip translation
SKIP_TRANSLATION_PATTERNS = [
    r'^[\d\s\+\-\*\/\=\(\)\.%]+$',  # Chỉ số và ký tự toán học
]

# Cấu hình file
FILE_CONFIG = {
    'encoding': 'utf-8',
    'json_indent': '\t',
    'ensure_ascii': False
}

# Thư mục và pattern file
DIR_PATTERNS = {
    'modconf_path': 'ModProject/ModConf',
    'localtext_pattern': '*localText.json'
}
