#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Constants cho hệ thống dịch localText
"""

import re

# Ngôn ngữ mặc định được hỗ trợ
DEFAULT_TARGET_LANGUAGES = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la', 'th']

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
    'source_language': 'en'
}

# Texts cần skip translation (không dịch)
SKIP_TRANSLATION_TEXTS = [
    "DEFAULT_DRAMA_OPT",
    "townmaster420041110desc",
    "team420041120desc"
]

# Cấu hình thuật ngữ (terminology) - các từ không được dịch
TERMINOLOGY_CONFIG = {
    # Prefix/suffix để bảo vệ thuật ngữ
    'prefix': '{\uF8B3}',
    'suffix': '{/\uF8B3}',
    'marker_pattern': r'{\uF8B3}\d+{/\uF8B3}',  # Pattern cho indexed markers
    
    # Danh sách thuật ngữ cần bảo vệ (cultivation/martial arts terms)
    # LƯU Ý: Matching không phân biệt hoa thường (case-insensitive)
    'terms': [
        # Basic Game Stats/Chỉ số cơ bản
        'HP', 'MP', 'SP', 'ATK', 'DEF', 'Dmg', 'DP', 'EXP', 'CD',

        # Game mechanics
        'DPS', 'DoT', 'AoE', 'PvP', 'PvE',

        # Common abbreviations that should not be translated
        'NPC', 'AI', 'UI', 'GUI', 'RAM',

        # Cultivation/Martial Arts terms - thuật ngữ tu luyện
        'Qi'
    ]
}

# Cấu hình ngữ cảnh cho dịch thuật
CONTEXT_CONFIG = {
    # Prefix ngữ cảnh chung cho tất cả ngôn ngữ (không bị dịch)
    'context_prefix': '{\uF8B2}Game cultivation context{/\uF8B2}\n',
    'context_marker': r'{\uF8B2}.+?{/\uF8B2}'
}

# Format string patterns cần bảo vệ khỏi bị biến đổi trong quá trình dịch
FORMAT_PROTECTION_CONFIG = {
    # Các patterns regex để nhận diện format strings cần bảo vệ
    'patterns': [
        r'{\d+(?::[^}]+)?}',  # Standard format strings: {0}, {1}, {0:#,##0}, {0:0.00}, etc.
        r'%[sdf]',            # Printf style: %s, %d, %f
        r'%\d+[sdf]',         # Printf with position: %1s, %2d, %3f
        r'\$\{\w+\}',         # Template strings: ${variable}
        # r'[+\-*/=<>!]+',      # Mathematical operators: +, -, *, /, =, <, >, !
        # r'\([+\-*/=<>!\d\s,\.]+\)', # Mathematical expressions in parentheses
        # r'\[[+\-*/=<>!\d\s,\.]+\]', # Mathematical expressions in brackets
        # r'[\d\s]*[+\-*/=<>!]+[\d\s]*', # Simple math expressions with operators
    ],
    
    # Placeholder prefix/suffix để thay thế format strings tạm thời
    'placeholder': {
        'prefix': '{\uF8B1}',
        'suffix': '{/\uF8B1}',
        'placeholder_marker': r'{\uF8B1}\d+?{/\uF8B1}'
    }
}

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
    'time': '⏱️',
    'delete': '🗑️',
    'result': '📊',
    'header': '🚀'
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
    # 'max_desc_length': 50,  # Giới hạn độ dài description
    # 'max_line_length': 120,  # Terminal width limit
    'clear_line_width': 200  # Width for clearing terminal line
}
