#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Constants cho hệ thống dịch localText
"""

import re

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

# Pattern để nhận dạng combined keys có chứa 'en'
COMBINED_KEY_WITH_EN_PATTERN = re.compile(r'.*\ben\b.*')  # Key có chứa 'en' ở bất kỳ vị trí nào

# Cấu hình dịch
TRANSLATION_CONFIG = {
    'max_retries': 3,
    'delay_between_requests': 0.2,  # giây
    'retry_delay': 1.0,  # giây
    'source_language': 'en',
    'min_text_length': 2
}

# Patterns để skip translation
SKIP_TRANSLATION_PATTERNS = [
    r'^[\d\s\+\-\*\/\=\(\)\.%]+$',  # Chỉ số và ký tự toán học
]

# Cấu hình file (gộp FILE_CONFIG và FILE_PATTERNS)
FILE_CONFIG = {
    'encoding': 'utf-8',
    'json_indent': '\t',
    'ensure_ascii': False,
    'localtext_suffix': 'localText.json',
    'json_extension': '.json'
}

# Thư mục và pattern file
DIR_PATTERNS = {
    'modconf_path': 'ModProject/ModConf',
    'localtext_pattern': '*localText.json'
}

# UI Icons và Symbols
UI_ICONS = {
    'folder': '📁',
    'file': '📄',
    'globe': '🌍',
    'success': '✅',
    'error': '❌',
    'warning': '⚠️',
    'info': 'ℹ️',
    'target': '🎯',
    'list': '📋',
    'time': '⏱️'
}

# Các thông điệp UI thường dùng
UI_MESSAGES = {
    'processing': 'Đang xử lý',
    'analyzing': 'Phân tích',
    'translating': 'Dịch',
    'completed': 'Hoàn thành',
    'success': 'Thành công',
    'failed': 'Thất bại',
    'interrupted': 'Bị gián đoạn',
    'no_files': 'Không tìm thấy file localText nào!',
    'not_found_modconf': 'Không tìm thấy thư mục ModConf',
    'script_title': 'Script Xử Lý LocalText.json'
}

# Cấu hình progress bar
PROGRESS_BAR_CONFIG = {
    'width': 40,  # Độ rộng cố định cho tất cả progress bar
    'fill_char': '█',
    'empty_char': '░',
    'show_percentage': True,
    'show_count': True,
    'show_time': True,
    'min_update_interval': 0.1,  # Minimum seconds between updates
    'max_desc_length': 50,  # Giới hạn độ dài description
    'max_line_length': 120,  # Terminal width limit
    'clear_line_width': 150  # Width for clearing terminal line
}
