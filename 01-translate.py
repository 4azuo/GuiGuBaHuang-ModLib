#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script dịch localText.json cho mod GuiGuBaHuang
- Dịch "en", "tc", "ch", "kr" trong các file ModConf/*localText.json
- Các ngôn ngữ khác đặt trong folder locale: ModConf/*/*localText.json
- File locale dùng combined key "en|tc|ch|kr"
- Không sửa các giá trị đang có
"""

import json
import os
import glob
import subprocess
import sys
import time
import requests
import urllib.parse
import re
from pathlib import Path
from typing import Dict, List, Any
import argparse
import shutil

# Đảm bảo UTF-8 encoding cho Windows
if os.name == 'nt':  # Windows
    os.environ['PYTHONIOENCODING'] = 'utf-8'

class GoogleTranslator:
    """Google Translate API wrapper sử dụng requests"""
    
    def __init__(self):
        self.base_url = "https://translate.googleapis.com/translate_a/single"
        self.max_length = 200
        
    def translate(self, text: str, dest: str, src: str = "auto") -> object:
        """Dịch text sử dụng Google Translate API"""
        if not text or not text.strip() or len(text) > self.max_length:
            return type('obj', (object,), {'text': text})()
            
        try:
            # Encode text
            encoded_text = urllib.parse.quote(text)
            
            # Tạo URL
            params = {
                'client': 'gtx',
                'sl': src,
                'tl': dest,
                'dt': 't',
                'q': encoded_text
            }
            
            # Gọi API
            response = requests.get(self.base_url, params=params, timeout=10)
            response.raise_for_status()
            
            # Parse JSON response
            data = response.json()
            if data and isinstance(data, list) and len(data) > 0:
                translations = data[0]
                if translations and isinstance(translations, list):
                    result_text = ''.join([item[0] for item in translations if item and len(item) > 0])
                    return type('obj', (object,), {'text': result_text or text})()
            
            return type('obj', (object,), {'text': text})()
            
        except Exception as e:
            print(f"    ⚠️  Lỗi dịch: {e}")
            return type('obj', (object,), {'text': text})()

class LocalTextTranslator:
    def __init__(self):
        # Ngôn ngữ chính được dịch trực tiếp trong file gốc
        self.main_languages = ['en', 'tc', 'ch', 'kr']
        # Ngôn ngữ khác được đặt trong folder locale
        self.locale_languages = ['vi', 'ja', 'es', 'la', 'ru']
        # Khởi tạo Google Translator
        self.translator = GoogleTranslator()
        self.use_google_translate = False
        # Mapping ngôn ngữ cho Google Translate
        self.lang_mapping = {
            'en': 'en',     # English
            'tc': 'zh-tw',  # Traditional Chinese
            'ch': 'zh-cn',  # Simplified Chinese
            'kr': 'ko',     # Korean
            'vi': 'vi',     # Vietnamese
            'ja': 'ja',     # Japanese
            'es': 'es',     # Spanish
            'la': 'la',     # Latin
            'ru': 'ru'      # Russian
        }
    
    def is_variable_name(self, text: str) -> bool:
        """Kiểm tra xem text có phải là tên variable/constant không"""
        if not text or not text.strip():
            return False
            
        text = text.strip()
        
        # Hardcode exceptions - những tên variable/constant cụ thể không được dịch
        hardcode_exceptions = [
            'DEFAULT_DRAMA_OPT',
            # Có thể thêm nhiều tên khác ở đây
        ]
        
        # Chỉ kiểm tra hardcode exceptions, không dùng pattern
        return text in hardcode_exceptions
        
    def translate_text(self, text: str, target_lang: str, source_lang: str = 'auto') -> str:
        """Dịch text sử dụng Google Translate"""
        if not self.use_google_translate or not text or not text.strip():
            return text
        
        # Kiểm tra xem có phải tên variable không - nếu có thì không dịch
        if self.is_variable_name(text):
            return text
            
        try:
            # Chuyển đổi language code
            source_google_lang = self.lang_mapping.get(source_lang, source_lang)
            target_google_lang = self.lang_mapping.get(target_lang, target_lang)
            
            # Dịch text
            result = self.translator.translate(text, dest=target_google_lang, src=source_google_lang)
            return result.text if result.text else text
            
        except Exception as e:
            print(f"    ⚠️  Lỗi dịch '{text[:50]}...': {e}")
            return text
        
    def fix_json_syntax(self, root_dir: str):
        """Gọi script fix_json_syntax để sửa lỗi JSON trước khi dịch"""
        print("🔧 Sửa lỗi JSON syntax trước khi dịch...")
        
        # Đường dẫn đến script fix_json_syntax
        script_dir = os.path.dirname(os.path.abspath(__file__))
        fix_script = os.path.join(script_dir, "02-fix_json_syntax.py")
        
        if not os.path.exists(fix_script):
            print(f"⚠️  Không tìm thấy script: {fix_script}")
            return
        
        try:
            # Gọi script fix_json_syntax với pattern cho localText files
            cmd = [sys.executable, fix_script, "--root", root_dir, "--pattern", "**/*localText.json"]
            env = os.environ.copy()
            env['PYTHONIOENCODING'] = 'utf-8'
            result = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8', errors='replace', env=env)
            
            if result.returncode == 0:
                print("✅ Hoàn thành sửa lỗi JSON syntax")
                # In output nếu có
                if result.stdout.strip():
                    print(result.stdout)
            else:
                print(f"❌ Lỗi khi chạy fix_json_syntax: {result.stderr}")
                
        except Exception as e:
            print(f"❌ Lỗi gọi script fix_json_syntax: {e}")
            
    def fix_single_file_json_syntax(self, file_path: str):
        """Gọi script fix_json_syntax cho một file cụ thể"""
        file_name = os.path.basename(file_path)
        
        print(f"🔧 Sửa lỗi JSON syntax cho file: {file_name}")
        
        # Đường dẫn đến script fix_json_syntax
        script_dir = os.path.dirname(os.path.abspath(__file__))
        fix_script = os.path.join(script_dir, "02-fix_json_syntax.py")
        
        if not os.path.exists(fix_script):
            print(f"⚠️  Không tìm thấy script: {fix_script}")
            return
        
        try:
            # Gọi script fix_json_syntax cho file cụ thể bằng cách truyền đường dẫn tuyệt đối
            cmd = [sys.executable, fix_script, "--root", os.path.dirname(file_path), "--pattern", file_name]
            env = os.environ.copy()
            env['PYTHONIOENCODING'] = 'utf-8'
            result = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8', errors='replace', env=env)
            
            if result.returncode == 0:
                print(f"  ✅ Hoàn thành sửa lỗi JSON syntax")
                # In output nếu có (chỉ in dòng quan trọng)
                if result.stdout.strip():
                    lines = result.stdout.strip().split('\n')
                    for line in lines:
                        if 'Fixed' in line or 'Error' in line or file_name in line or '✅' in line or '❌' in line:
                            print(f"    {line}")
            else:
                print(f"  ❌ Lỗi khi chạy fix_json_syntax: {result.stderr}")
                
        except Exception as e:
            print(f"  ❌ Lỗi gọi script fix_json_syntax: {e}")
            
    def cleanup_locale_files(self, root_dir: str):
        """Gọi script cleanup_locale để dọn dẹp file locale sau khi dịch"""
        print("🧹 Dọn dẹp file locale...")
        
        # Đường dẫn đến script cleanup_locale
        script_dir = os.path.dirname(os.path.abspath(__file__))
        cleanup_script = os.path.join(script_dir, "03-cleanup_locale.py")
        
        if not os.path.exists(cleanup_script):
            print(f"⚠️  Không tìm thấy script: {cleanup_script}")
            return
        
        try:
            # Gọi script cleanup_locale
            cmd = [sys.executable, cleanup_script, "--root", root_dir]
            env = os.environ.copy()
            env['PYTHONIOENCODING'] = 'utf-8'
            result = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8', errors='replace', env=env)
            
            if result.returncode == 0:
                print("✅ Hoàn thành dọn dẹp file locale")
                # In output nếu có
                if result.stdout.strip():
                    print(result.stdout)
            else:
                print(f"❌ Lỗi khi chạy cleanup_locale: {result.stderr}")
                
        except Exception as e:
            print(f"❌ Lỗi gọi script cleanup_locale: {e}")
            
    def cleanup_single_file_locale(self, file_path: str):
        """Gọi script cleanup_locale cho các file locale của một file cụ thể"""
        file_dir = os.path.dirname(file_path)
        
        # Tìm các folder locale trong cùng thư mục với file
        locale_files = []
        for lang in self.locale_languages:
            locale_dir = os.path.join(file_dir, lang)
            if os.path.exists(locale_dir):
                locale_file = os.path.join(locale_dir, os.path.basename(file_path))
                if os.path.exists(locale_file):
                    locale_files.append(locale_file)
        
        if not locale_files:
            return  # Không có file locale nào
            
        print(f"🧹 Dọn dẹp locale cho file: {os.path.basename(file_path)}")
        
        # Đường dẫn đến script cleanup_locale
        script_dir = os.path.dirname(os.path.abspath(__file__))
        cleanup_script = os.path.join(script_dir, "03-cleanup_locale.py")
        
        if not os.path.exists(cleanup_script):
            print(f"⚠️  Không tìm thấy script: {cleanup_script}")
            return
        
        try:
            # Gọi script cleanup_locale cho từng file locale
            for locale_file in locale_files:
                cmd = [sys.executable, cleanup_script, "--file", locale_file]
                env = os.environ.copy()
                env['PYTHONIOENCODING'] = 'utf-8'
                result = subprocess.run(cmd, capture_output=True, text=True, encoding='utf-8', errors='replace', env=env)
                
                if result.returncode == 0:
                    # In output nếu có (chỉ in dòng quan trọng)
                    if result.stdout.strip():
                        lines = result.stdout.strip().split('\n')
                        for line in lines:
                            if 'Removed' in line or 'Cleaned' in line or 'duplicate' in line.lower() or '✅' in line or '❌' in line:
                                print(f"    {line}")
                else:
                    print(f"  ❌ Lỗi khi chạy cleanup_locale cho {locale_file}: {result.stderr}")
                    
            print(f"  ✅ Hoàn thành dọn dẹp {len(locale_files)} file locale")
                
        except Exception as e:
            print(f"  ❌ Lỗi gọi script cleanup_locale: {e}")
        
    def get_source_text(self, entry: Dict[str, Any]) -> tuple:
        """Lấy text nguồn để copy (ưu tiên en > ch > tc > kr)"""
        priority = ['en', 'ch', 'tc', 'kr']
        
        for lang in priority:
            if lang in entry and entry[lang] and entry[lang].strip():
                return entry[lang], lang
                
        # Nếu không có, thử tìm trong combined key
        for combined_key in [
            'ch|en|tc|kr', 'en|tc|ch|kr', 'en|ch|tc|kr', 'tc|en|ch|kr',
            'kr|en|tc|ch', 'en|kr|tc|ch', 'tc|kr|en|ch', 'ch|kr|en|tc',
            'kr|ch|en|tc', 'kr|tc|en|ch', 'tc|ch|en|kr', 'ch|tc|en|kr',
            # Bổ sung các tổ hợp 3 ngôn ngữ
            'en|tc|ch', 'en|ch|tc', 'tc|en|ch', 'tc|ch|en', 'ch|en|tc', 'ch|tc|en',
            'en|kr|tc', 'en|tc|kr', 'tc|en|kr', 'tc|kr|en', 'kr|en|tc', 'kr|tc|en',
            'en|kr|ch', 'en|ch|kr', 'ch|en|kr', 'ch|kr|en', 'kr|en|ch', 'kr|ch|en',
            'tc|kr|ch', 'tc|ch|kr', 'kr|tc|ch', 'kr|ch|tc', 'ch|tc|kr', 'ch|kr|tc',
            # Bổ sung các tổ hợp 2 ngôn ngữ
            'en|tc', 'tc|en', 'en|ch', 'ch|en', 'en|kr', 'kr|en',
            'tc|ch', 'ch|tc', 'tc|kr', 'kr|tc', 'ch|kr', 'kr|ch'
        ]:
            if combined_key in entry and entry[combined_key]:
                return entry[combined_key], 'auto'
                
        return "", "auto"
    
    def process_main_file(self, file_path: str, target_langs: List[str]):
        """Xử lý file localText chính - dịch en, tc, ch, kr"""
        print(f"\n📄 Xử lý file chính: {os.path.basename(file_path)}")
        
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                data = json.load(f)
                
            modified = False
            
            for entry in data:
                if not isinstance(entry, dict):
                    continue
                    
                source_text, source_lang = self.get_source_text(entry)
                if not source_text:
                    continue
                    
                # Chỉ dịch các ngôn ngữ chính
                for target_lang in target_langs:
                    if target_lang not in self.main_languages:
                        continue
                        
                    # Bỏ qua nếu đã có translation
                    if target_lang in entry and entry[target_lang] and entry[target_lang].strip():
                        continue
                    
                    # Dịch hoặc copy text
                    if source_lang == target_lang or not self.use_google_translate:
                        # Nếu cùng ngôn ngữ hoặc không dùng Google Translate thì copy
                        translated_text = source_text
                        print(f"  📝 Copy {source_lang} -> {target_lang}: {entry.get('key', 'Unknown')}")
                    elif self.is_variable_name(source_text):
                        # Nếu là tên variable thì không dịch, chỉ copy
                        translated_text = source_text
                        print(f"  🔧 Variable {source_lang} -> {target_lang}: {entry.get('key', 'Unknown')} = '{source_text}'")
                    else:
                        # Dịch bằng Google Translate
                        print(f"  🌐 Dịch {source_lang} -> {target_lang}: {entry.get('key', 'Unknown')}")
                        translated_text = self.translate_text(source_text, target_lang, source_lang)
                        # Thêm delay nhỏ để tránh rate limit
                        time.sleep(0.1)
                    
                    entry[target_lang] = translated_text
                    modified = True
            
            # Lưu file nếu có thay đổi
            if modified:
                with open(file_path, 'w', encoding='utf-8') as f:
                    json.dump(data, f, ensure_ascii=False, indent='\t')
                print(f"  ✅ Đã cập nhật")
            else:
                print(f"  ℹ️  Không có thay đổi")
                
        except Exception as e:
            print(f"  ❌ Lỗi: {e}")
    
    def create_locale_file(self, source_file: str, target_lang: str, source_data: List[Dict]):
        """Tạo file localText trong folder locale"""
        source_dir = os.path.dirname(source_file)
        locale_dir = os.path.join(source_dir, target_lang)
        
        # Tạo thư mục locale nếu chưa có
        os.makedirs(locale_dir, exist_ok=True)
        
        target_file = os.path.join(locale_dir, os.path.basename(source_file))
        
        print(f"📁 Tạo file locale: {target_lang}/{os.path.basename(source_file)}")
        
        try:
            locale_data = []
            
            for entry in source_data:
                if not isinstance(entry, dict):
                    continue
                    
                # Tạo entry mới cho locale
                locale_entry = {
                    "id": entry.get("id", ""),
                    "key": entry.get("key", "")
                }
                
                # Lấy text nguồn
                source_text, source_lang = self.get_source_text(entry)
                if source_text:
                    # Sử dụng combined key cho file locale
                    locale_entry["en|tc|ch|kr"] = source_text
                
                locale_data.append(locale_entry)
            
            # Lưu file locale
            with open(target_file, 'w', encoding='utf-8') as f:
                json.dump(locale_data, f, ensure_ascii=False, indent='\t')
            print(f"  ✅ Đã tạo file locale")
                
        except Exception as e:
            print(f"  ❌ Lỗi: {e}")
    
    def update_locale_file(self, file_path: str, source_data: List[Dict]):
        """Cập nhật file locale có sẵn"""
        print(f"📁 Cập nhật file locale: {os.path.basename(file_path)}")
        
        try:
            # Đọc file locale hiện tại
            with open(file_path, 'r', encoding='utf-8') as f:
                locale_data = json.load(f)
                
            # Tạo map để dễ tìm kiếm
            locale_map = {entry.get('key', ''): entry for entry in locale_data if isinstance(entry, dict)}
            
            modified = False
            
            for source_entry in source_data:
                if not isinstance(source_entry, dict):
                    continue
                    
                key = source_entry.get('key', '')
                if not key:
                    continue
                    
                # Kiểm tra xem entry đã tồn tại chưa
                if key in locale_map:
                    locale_entry = locale_map[key]
                    # Chỉ cập nhật nếu chưa có combined key hoặc rỗng
                    if "en|tc|ch|kr" not in locale_entry or not locale_entry["en|tc|ch|kr"]:
                        source_text, source_lang = self.get_source_text(source_entry)
                        if source_text:
                            print(f"  📝 Cập nhật {source_lang}: {key}")
                            locale_entry["en|tc|ch|kr"] = source_text
                            modified = True
                else:
                    # Thêm entry mới
                    source_text, source_lang = self.get_source_text(source_entry)
                    if source_text:
                        new_entry = {
                            "id": source_entry.get("id", ""),
                            "key": key,
                            "en|tc|ch|kr": source_text
                        }
                        locale_data.append(new_entry)
                        print(f"  📝 Thêm mới {source_lang}: {key}")
                        modified = True
            
            # Lưu file nếu có thay đổi
            if modified:
                with open(file_path, 'w', encoding='utf-8') as f:
                    json.dump(locale_data, f, ensure_ascii=False, indent='\t')
                print(f"  ✅ Đã cập nhật")
            else:
                print(f"  ℹ️  Không có thay đổi")
                
        except Exception as e:
            print(f"  ❌ Lỗi: {e}")
    
    
    def translate_all(self, root_dir: str, target_langs: List[str], target_path: str = None):
        """Dịch tất cả file localText theo quy tắc:
        - en, tc, ch, kr: dịch trực tiếp trong file gốc
        - Ngôn ngữ khác: tạo file trong folder locale với combined key
        """
        print(f"🚀 Dịch localText trong: {root_dir}")
        if target_path:
            print(f"🎯 Target: {target_path}")
        print(f"🎯 Ngôn ngữ: {', '.join(target_langs)}")
        
        # Tìm file localText theo target
        main_files = self._find_target_files(root_dir, target_path)
        
        print(f"📊 Tìm thấy {len(main_files)} file localText chính")
        
        # Xử lý từng file
        for i, file_path in enumerate(main_files, 1):
            print(f"\n[{i}/{len(main_files)}] Đang xử lý: {os.path.relpath(file_path, root_dir)}")
            
            # Sửa lỗi JSON syntax cho file này trước
            self.fix_single_file_json_syntax(file_path)
            
            # Dọn dẹp locale trước khi xử lý file này
            self.cleanup_single_file_locale(file_path)
            
            try:
                # Đọc dữ liệu gốc
                with open(file_path, 'r', encoding='utf-8') as f:
                    source_data = json.load(f)
                
                # Xử lý ngôn ngữ chính (en, tc, ch, kr)
                main_langs = [lang for lang in target_langs if lang in self.main_languages]
                if main_langs:
                    self.process_main_file(file_path, main_langs)
                    # Đọc lại dữ liệu sau khi cập nhật
                    with open(file_path, 'r', encoding='utf-8') as f:
                        source_data = json.load(f)
                
                # Xử lý ngôn ngữ locale
                locale_langs = [lang for lang in target_langs if lang in self.locale_languages]
                for locale_lang in locale_langs:
                    locale_file_path = self._get_locale_file_path(file_path, locale_lang)
                    
                    if os.path.exists(locale_file_path):
                        # Cập nhật file locale có sẵn
                        self.update_locale_file(locale_file_path, source_data)
                    else:
                        # Tạo file locale mới
                        self.create_locale_file(file_path, locale_lang, source_data)
                        
            except Exception as e:
                print(f"  ❌ Lỗi xử lý file: {e}")
        
        print(f"\n🎉 Hoàn thành dịch {len(main_files)} file!")
        
        if main_files:
            print("💡 Lưu ý:")
            print(f"  • Mỗi file được sửa lỗi JSON syntax và dọn dẹp locale trước khi xử lý")
            print(f"  • Ngôn ngữ chính ({', '.join(self.main_languages)}) được dịch trực tiếp trong file gốc")
            print(f"  • Ngôn ngữ locale ({', '.join(self.locale_languages)}) được tạo trong folder riêng với combined key")
        else:
            print("❌ Không có file nào được xử lý!")
    
    def _find_target_files(self, root_dir: str, target_path: str = None) -> List[str]:
        """Tìm file localText dựa trên target path"""
        if target_path:
            full_target_path = os.path.join(root_dir, target_path)
            
            if os.path.isfile(full_target_path):
                # Nếu target là file cụ thể
                if full_target_path.endswith('localText.json') and not self._is_locale_file(full_target_path):
                    print(f"🎯 Dịch file: {target_path}")
                    return [full_target_path]
                else:
                    print(f"❌ File không phải localText.json hoặc là file locale: {target_path}")
                    return []
            elif os.path.isdir(full_target_path):
                # Nếu target là folder
                pattern = os.path.join(full_target_path, "**", "*localText.json")
                files = glob.glob(pattern, recursive=True)
                files = [f for f in files if os.path.isfile(f) and not self._is_locale_file(f)]
                print(f"🎯 Dịch folder: {target_path} ({len(files)} file)")
                return files
            else:
                print(f"❌ Không tìm thấy file/folder: {target_path}")
                return []
        else:
            # Tìm tất cả file ModConf/*localText.json (không bao gồm locale)
            pattern = os.path.join(root_dir, "**", "ModConf", "*localText.json")
            files = glob.glob(pattern, recursive=True)
            files = [f for f in files if os.path.isfile(f) and not self._is_locale_file(f)]
            print(f"🎯 Dịch toàn bộ workspace ({len(files)} file)")
            return files
    
    def _is_locale_file(self, file_path: str) -> bool:
        """Kiểm tra xem file có nằm trong folder locale không"""
        parent_dir = os.path.basename(os.path.dirname(file_path))
        return parent_dir in self.locale_languages
    
    def _get_locale_file_path(self, source_file: str, locale_lang: str) -> str:
        """Tạo đường dẫn file locale"""
        source_dir = os.path.dirname(source_file)
        locale_dir = os.path.join(source_dir, locale_lang)
        return os.path.join(locale_dir, os.path.basename(source_file))

def main():
    parser = argparse.ArgumentParser(description='Dịch file localText.json cho mod GuiGuBaHuang')
    parser.add_argument('--root', '-r', default='.', help='Thư mục gốc chứa các mod')
    parser.add_argument('--langs', '-l', default='en,tc,ch,kr,vi', help='Ngôn ngữ cần dịch (cách nhau bởi dấu phẩy)')
    parser.add_argument('--translate', '-t', action='store_true', help='Sử dụng Google Translate thay vì copy text')
    parser.add_argument('--target', default=None, help='File hoặc folder cụ thể cần dịch (relative đến root)')
    
    args = parser.parse_args()
    
    root_dir = os.path.abspath(args.root)
    target_langs = [lang.strip() for lang in args.langs.split(',')]
    
    translator = LocalTextTranslator()
    translator.use_google_translate = args.translate
    
    if args.translate:
        print("🌐 Sử dụng Google Translate để dịch text")
    else:
        print("📝 Chỉ copy text giữa các ngôn ngữ")
    
    translator.translate_all(root_dir, target_langs, args.target)

if __name__ == "__main__":
    main()
