#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Constants for localText translation system
"""

import re

# Default supported languages
DEFAULT_TARGET_LANGUAGES = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la', 'th']

# Language code mapping
LANGUAGE_CODES = {
    'ch': 'zh-CN',  # Chinese Simplified
    'tc': 'zh-TW',  # Traditional Chinese  
    'kr': 'ko'      # Korean
}

# Main language keys in main files
MAIN_LANGUAGE_KEYS = ['en', 'ch', 'tc', 'kr']

# Combined key for locale files
LOCALE_COMBINED_KEY = 'en|ch|tc|kr'

# Pattern to recognize combined keys containing 'en'
COMBINED_KEY_WITH_EN_PATTERN = re.compile(r'.*\ben\b.*')  # Key containing 'en' at any position

# Translation configuration
TRANSLATION_CONFIG = {
    'max_retries': 3,
    'delay_between_requests': 0.2,  # seconds
    'retry_delay': 1.0,  # seconds
    'source_language': 'en'
}

# Texts to skip translation (do not translate)
SKIP_TRANSLATION_TEXTS = [
    "DEFAULT_DRAMA_OPT",
    "townmaster420041110desc",
    "team420041120desc"
]

# Terminology configuration - terms that should not be translated
TERMINOLOGY_CONFIG = {
    # Prefix/suffix to protect terminology
    'prefix': '{\uF8B3}',
    'suffix': '{/\uF8B3}',
    'marker_pattern': r'{\uF8B3}\d+{/\uF8B3}',  # Pattern for indexed markers
    
    # List of terms to protect (cultivation/martial arts terms)
    # NOTE: Matching is case-insensitive
    'terms': [
        # Basic Game Stats
        'HP', 'MP', 'SP', 'DP', 'CD',

        # Game mechanics
        'DPS', 'DoT', 'AoE', 'PvP', 'PvE',

        # Common abbreviations that should not be translated
        'NPC', 'AI', 'UI', 'GUI', 'RAM',

        # Cultivation/Martial Arts terms
        'Qi'
    ]
}

# Context configuration for translation
CONTEXT_CONFIG = {
    # Context prefix for all languages (not translated)
    'context_prefix': '{\uF8B2}Game cultivation context{/\uF8B2}\n',
    'context_marker': r'{\uF8B2}.+?{/\uF8B2}'
}

# Format string patterns to protect from modification during translation
FORMAT_PROTECTION_CONFIG = {
    # Regex patterns to identify format strings that need protection
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
    
    # Placeholder prefix/suffix to temporarily replace format strings
    'placeholder': {
        'prefix': '{\uF8B1}',
        'suffix': '{/\uF8B1}',
        'placeholder_marker': r'{\uF8B1}\d+?{/\uF8B1}'
    }
}

# File configuration (combines FILE_CONFIG and FILE_PATTERNS)
FILE_CONFIG = {
    'encoding': 'utf-8',
    'json_indent': '\t',
    'ensure_ascii': False,
    'localtext_suffix': 'localText.json',
    'json_extension': '.json'
}

# Directory and file patterns
DIR_PATTERNS = {
    'modconf_path': 'ModProject/ModConf',
    'localtext_pattern': '*localText.json'
}

# UI Icons and Symbols
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
    'header': '🚀',
    'worker': '👷'
}

# Common UI messages
UI_MESSAGES = {
    'processing': 'Processing',
    'analyzing': 'Analyzing',
    'translating': 'Translating',
    'completed': 'Completed',
    'success': 'Success',
    'failed': 'Failed',
    'interrupted': 'Interrupted',
    'no_files': 'No localText files found!',
    'not_found_modconf': 'ModConf directory not found',
    'script_title': 'LocalText.json Processing Script'
}

# Progress bar configuration
PROGRESS_BAR_CONFIG = {
    'overall_width': 40,
    'children_width': 20,
    'fill_char': '█',
    'empty_char': '░',
    'show_percentage': True,
    'show_count': True,
    'show_time': True,
    'min_update_interval': 0.1,  # Minimum seconds between updates
    # 'max_desc_length': 50,  # Description length limit
    # 'max_line_length': 120,  # Terminal width limit
    'clear_line_width': 200  # Width for clearing terminal line
}
