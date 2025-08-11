#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script để xử lý các file *localText.json - Tự động dịch đa ngôn ngữ

Cấu trúc file:
- Main files: ModConf/*localText.json (1 cấp thư mục)
- Locale files: ModConf/*/*localText.json (2 cấp thư mục)

Workflow:
1. Xử lý file main trong ModConf/
2. Xóa toàn bộ file locale cũ
3. Tạo lại file locale mới từ file main
4. Dịch tự động từ tiếng Anh

Cách sử dụng:
    # BẮT BUỘC: Phải có cả --project và --path
    
    # Xử lý toàn bộ ModConf
    python translate_localtext.py --project 3385996759 --path .
    
    # Xử lý file main cụ thể
    python translate_localtext.py --project 3385996759 --path game_localText.json
    
    # Xử lý file locale (sẽ tự động chuyển về file main)
    python translate_localtext.py --project 3385996759 --path vi/game_localText.json
    
    # Xử lý folder con trong ModConf
    python translate_localtext.py --project 3385996759 --path subfolder
    
    # Tùy chọn ngôn ngữ
    python translate_localtext.py --project 3385996759 --path . --create-locales vi,es
    python translate_localtext.py --project 3385996759 --path . --create-locales all
    
    # Kiểm tra trước khi chạy
    python translate_localtext.py --project 3385996759 --path . --dry-run
    
    # Trợ giúp
    python translate_localtext.py --help

Ngôn ngữ hỗ trợ:
- Mặc định: vi (Vietnamese), es (Spanish), fr (French), de (German), 
           ru (Russian), ja (Japanese), la (Latin)
- Tùy chỉnh: Bất kỳ mã ngôn ngữ ISO nào

Ví dụ cấu trúc:
    ModConf/
    ├── game_localText.json          # ← Main file (en, ch, tc, kr)
    ├── balance_localText.json       # ← Main file
    ├── vi/
    │   ├── game_localText.json      # ← Locale Vietnamese
    │   └── balance_localText.json   # ← Locale Vietnamese
    ├── es/
    │   ├── game_localText.json      # ← Locale Spanish
    │   └── balance_localText.json   # ← Locale Spanish
    └── fr/
        ├── game_localText.json      # ← Locale French
        └── balance_localText.json   # ← Locale French
"""

import json
import os
import glob
import time
import argparse
import shutil
from deep_translator import GoogleTranslator
import re

class LocalTextProcessor:
    def __init__(self):
        self.language_codes = {
            'ch': 'zh-CN',  # Chinese Simplified
            'tc': 'zh-TW',  # Traditional Chinese
            'kr': 'ko'      # Korean
        }
        self.processed_count = 0
        self.translated_count = 0
        self.target_languages = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la']  # Mặc định
        
    def fix_json_format(self, content):
        """Sửa các lỗi JSON format phổ biến"""
        # Loại bỏ trailing commas
        content = re.sub(r',(\s*[}\]])', r'\1', content)
        # Sửa single quotes thành double quotes (chỉ cho keys)
        content = re.sub(r"'([^']*)':", r'"\1":', content)
        return content
    
    def translate_text(self, text, target_lang):
        """Dịch text sang ngôn ngữ đích"""
        try:
            if not text or text.strip() == "":
                return text
                
            # Skip nếu text chỉ chứa số hoặc ký tự đặc biệt
            if re.match(r'^[\d\s\+\-\*\/\=\(\)\.%]+$', text):
                return text
                
            # Skip nếu text quá ngắn (1-2 ký tự)
            if len(text.strip()) <= 2:
                return text
                
            result = GoogleTranslator(source='en', target=target_lang).translate(text)
            time.sleep(0.2)  # Tránh rate limit
            self.translated_count += 1
            return result
        except Exception as e:
            print(f"    Lỗi dịch '{text}' sang {target_lang}: {e}")
            return text
    
    def find_main_file(self, locale_path):
        """Tìm file main tương ứng trong ModConf"""
        try:
            # Từ đường dẫn locale, tìm thư mục ModConf
            parts = locale_path.split(os.sep)
            
            # Tìm thư mục ModConf gần nhất
            modconf_index = None
            for i in range(len(parts) - 1, -1, -1):
                if parts[i] == "ModConf":
                    modconf_index = i
                    break
            
            if modconf_index is not None:
                # Tạo đường dẫn ModConf
                modconf_path = os.sep.join(parts[:modconf_index + 1])
                
                # Lấy tên file từ locale path
                locale_filename = os.path.basename(locale_path)
                
                # Tên file main sẽ giống tên file locale
                main_file_path = os.path.join(modconf_path, locale_filename)
                
                if os.path.exists(main_file_path):
                    return main_file_path
        except:
            pass
        
        return None
    
    def is_locale_file(self, file_path):
        """Kiểm tra xem có phải file locale không (nằm trong ModConf/*/*localText.json)"""
        try:
            parts = file_path.split(os.sep)
            
            # Tìm index của ModConf
            modconf_index = None
            for i, part in enumerate(parts):
                if part == "ModConf":
                    modconf_index = i
                    break
            
            if modconf_index is None:
                return False
            
            # Tính số cấp thư mục từ ModConf đến file (không tính tên file)
            folder_levels = len(parts) - modconf_index - 2  # -2 để bỏ ModConf và tên file
            
            # File main: ModConf/*localText.json (0 cấp thư mục con)
            # File locale: ModConf/*/*localText.json (1 cấp thư mục con)
            if folder_levels == 1:  # ModConf/lang/file.json
                return True
            elif folder_levels == 0:  # ModConf/file.json
                return False
            else:
                return False  # Không phải cấu trúc hợp lệ
                
        except:
            return False
    
    def get_locale_language(self, file_path):
        """Lấy mã ngôn ngữ từ đường dẫn file locale"""
        try:
            parts = file_path.split(os.sep)
            
            # Tìm ModConf và lấy thư mục ngôn ngữ (cấp kế tiếp)
            for i, part in enumerate(parts):
                if part == "ModConf" and i + 1 < len(parts):
                    lang_folder = parts[i + 1]
                    
                    # Map các mã ngôn ngữ phổ biến
                    lang_map = {
                        'vi': 'vi',      # Vietnamese
                        'es': 'es',      # Spanish
                        'fr': 'fr',      # French
                        'de': 'de',      # German
                        'ru': 'ru',      # Russian
                        'ja': 'ja',      # Japanese
                        'pt': 'pt',      # Portuguese
                        'it': 'it',      # Italian
                        'th': 'th',      # Thai
                        'ar': 'ar',      # Arabic
                        'hi': 'hi',      # Hindi
                        'tr': 'tr',      # Turkish
                        'la': 'la',      # Latin
                    }
                    
                    return lang_map.get(lang_folder, lang_folder)
            
            return None
        except:
            return None
    
    def clean_existing_locale_files(self, main_files):
        """Xóa tất cả file locale cũ"""
        cleaned_count = 0
        
        for main_file_path in main_files:
            # Tìm thư mục ModConf
            parts = main_file_path.split(os.sep)
            modconf_index = None
            for i, part in enumerate(parts):
                if part == "ModConf":
                    modconf_index = i
                    break
            
            if modconf_index is None:
                continue
                
            modconf_path = os.sep.join(parts[:modconf_index + 1])
            
            # Tìm và xóa các thư mục ngôn ngữ
            for lang in self.target_languages:
                lang_dir = os.path.join(modconf_path, lang)
                if os.path.exists(lang_dir) and os.path.isdir(lang_dir):
                    try:
                        shutil.rmtree(lang_dir)
                        cleaned_count += 1
                    except Exception as e:
                        print(f"Lỗi xóa {lang_dir}: {e}")
        
        if cleaned_count > 0:
            print(f"Dọn dẹp {cleaned_count} thư mục locale cũ")
        
        return cleaned_count
    
    def create_combined_translation(self, main_data, target_lang_code):
        """Tạo bản dịch cho locale file từ main file"""
        if isinstance(main_data, list):
            result = []
            for item in main_data:
                if isinstance(item, dict) and 'en' in item:
                    new_item = {}
                    # Copy các key không phải ngôn ngữ
                    for key, value in item.items():
                        if key not in ['en', 'ch', 'tc', 'kr']:
                            new_item[key] = value
                    
                    # Dịch từ tiếng Anh sang ngôn ngữ đích
                    en_text = item['en']
                    if en_text and en_text.strip():
                        translated = self.translate_text(en_text, target_lang_code)
                        # Sử dụng key gộp "en|ch|tc|kr" với giá trị đã dịch
                        new_item['en|ch|tc|kr'] = translated
                    else:
                        new_item['en|ch|tc|kr'] = en_text
                    
                    result.append(new_item)
                else:
                    result.append(item)
            return result
        elif isinstance(main_data, dict):
            # Xử lý tương tự cho dict
            if 'en' in main_data:
                new_item = {}
                for key, value in main_data.items():
                    if key not in ['en', 'ch', 'tc', 'kr']:
                        new_item[key] = value
                
                en_text = main_data['en']
                if en_text and en_text.strip():
                    translated = self.translate_text(en_text, target_lang_code)
                    # Sử dụng key gộp "en|ch|tc|kr" với giá trị đã dịch
                    new_item['en|ch|tc|kr'] = translated
                else:
                    new_item['en|ch|tc|kr'] = en_text
                
                return new_item
        
        return main_data
    
    def process_json_item(self, item, item_index):
        """Xử lý một item JSON"""
        if not isinstance(item, dict):
            return item
            
        # Kiểm tra có key 'en' không
        if 'en' not in item:
            return item
            
        en_text = item['en']
        if not en_text or en_text.strip() == "":
            return item
            
        # Dịch sang các ngôn ngữ khác nếu chưa có hoặc rỗng
        for lang_key, google_lang_code in self.language_codes.items():
            if lang_key not in item or not item[lang_key] or item[lang_key].strip() == "":
                translated = self.translate_text(en_text, google_lang_code)
                item[lang_key] = translated
        
        # Kiểm tra nếu tất cả các giá trị ngôn ngữ giống nhau
        lang_keys = ['en', 'ch', 'tc', 'kr']
        lang_values = []
        for lang_key in lang_keys:
            if lang_key in item:
                lang_values.append(item[lang_key])
        
        # Nếu tất cả giá trị giống nhau và có ít nhất 2 ngôn ngữ
        if len(set(lang_values)) == 1 and len(lang_values) >= 2:
            # Gộp thành key chung
            combined_key = '|'.join([key for key in lang_keys if key in item])
            combined_value = lang_values[0]
            
            # Tạo ordered_item với key gộp
            ordered_item = {}
            
            # Thêm các key khác trước
            priority_keys = ['__name', 'id', 'key']
            for key in priority_keys:
                if key in item:
                    ordered_item[key] = item[key]
            
            # Thêm key gộp
            ordered_item[combined_key] = combined_value
            
            # Thêm các key còn lại
            for key, value in item.items():
                if key not in priority_keys and key not in lang_keys:
                    ordered_item[key] = value
        else:
            # Giữ nguyên logic cũ nếu các giá trị khác nhau
            ordered_item = {}
            
            # Thêm các key có thứ tự ưu tiên
            priority_keys = ['__name', 'id', 'key', 'en', 'ch', 'tc', 'kr']
            for key in priority_keys:
                if key in item:
                    ordered_item[key] = item[key]
            
            # Thêm các key còn lại
            for key, value in item.items():
                if key not in priority_keys:
                    ordered_item[key] = value
                
        return ordered_item
    
    def create_locale_files_from_main(self, main_files, target_languages=None):
        """Tạo các file locale từ file main"""
        if target_languages is None:
            target_languages = self.target_languages
            
        created_files = []
        
        for main_file_path in main_files:
            # Tìm thư mục ModConf
            parts = main_file_path.split(os.sep)
            modconf_index = None
            for i, part in enumerate(parts):
                if part == "ModConf":
                    modconf_index = i
                    break
            
            if modconf_index is None:
                continue
                
            modconf_path = os.sep.join(parts[:modconf_index + 1])
            main_filename = os.path.basename(main_file_path)
            
            # Tạo file locale cho mỗi ngôn ngữ
            for lang in target_languages:
                # Tạo thư mục ngôn ngữ
                lang_dir = os.path.join(modconf_path, lang)
                os.makedirs(lang_dir, exist_ok=True)
                
                # Đường dẫn file locale
                locale_file_path = os.path.join(lang_dir, main_filename)
                created_files.append(locale_file_path)
                
        if created_files:
            print(f"Tạo {len(created_files)} file locale mới")
                
        return created_files
    
    def process_locale_file(self, file_path):
        """Xử lý file locale dựa trên file main"""
        try:
            # Tìm file main tương ứng
            main_file_path = self.find_main_file(file_path)
            if not main_file_path:
                return self.process_json_file(file_path)  # Fallback to normal processing
            
            # Đọc file main
            with open(main_file_path, 'r', encoding='utf-8') as f:
                main_content = f.read()
            
            main_content = self.fix_json_format(main_content)
            main_data = json.loads(main_content)
            
            # Xác định ngôn ngữ đích từ thư mục chứa file locale
            target_lang = self.get_locale_language(file_path)
            if not target_lang:
                return False
            
            # Tạo thư mục chứa file nếu chưa có
            locale_dir = os.path.dirname(file_path)
            if not os.path.exists(locale_dir):
                os.makedirs(locale_dir)
            
            # Tạo bản dịch cho locale
            translated_data = self.create_combined_translation(main_data, target_lang)
            
            # Sắp xếp theo id nếu là array
            if isinstance(translated_data, list):
                try:
                    translated_data.sort(key=lambda x: x.get('id', 0) if isinstance(x.get('id'), (int, float)) else 0)
                except:
                    pass
            
            # Lưu file locale (tạo mới)
            with open(file_path, 'w', encoding='utf-8') as f:
                json.dump(translated_data, f, ensure_ascii=False, indent='\t')
            
            self.processed_count += 1
            return True
            
        except Exception as e:
            print(f"Lỗi xử lý file locale {file_path}: {e}")
            return False

    def process_json_file(self, file_path):
        """Xử lý một file JSON"""
        try:
            # Đọc file
            with open(file_path, 'r', encoding='utf-8') as f:
                content = f.read()
            
            # Sửa JSON format
            content = self.fix_json_format(content)
            
            # Parse JSON
            try:
                data = json.loads(content)
            except json.JSONDecodeError as e:
                print(f"Lỗi parse JSON trong {file_path}: {e}")
                return False
            
            # Xử lý data
            if isinstance(data, list):
                processed_data = []
                for i, item in enumerate(data):
                    processed_item = self.process_json_item(item, i)
                    processed_data.append(processed_item)
                
                # Sắp xếp theo id nếu có
                try:
                    processed_data.sort(key=lambda x: x.get('id', 0) if isinstance(x.get('id'), (int, float)) else 0)
                except:
                    pass  # Nếu không sort được thì giữ nguyên thứ tự
                    
            elif isinstance(data, dict):
                processed_data = self.process_json_item(data, 0)
            else:
                print(f"Định dạng dữ liệu không hỗ trợ trong {file_path}")
                return False
            
            # Lưu lại file với format đẹp
            with open(file_path, 'w', encoding='utf-8') as f:
                json.dump(processed_data, f, ensure_ascii=False, indent='\t')
            
            self.processed_count += 1
            return True
            
        except Exception as e:
            print(f"Lỗi xử lý {file_path}: {e}")
            return False
    
    def find_localtext_files(self, path):
        """Tìm tất cả file *localText.json từ path cho trước"""
        files = []
        
        if os.path.isfile(path):
            # Nếu là file cụ thể
            if path.endswith('localText.json'):
                files.append(os.path.abspath(path))
        elif os.path.isdir(path):
            # Nếu là thư mục
            patterns = [
                os.path.join(path, "**", "*localText.json"),
                os.path.join(path, "**", "*_localText.json")
            ]
            
            for pattern in patterns:
                files.extend(glob.glob(pattern, recursive=True))
        else:
            print(f"Đường dẫn không tồn tại: {path}")
            return []
        
        # Loại bỏ duplicates
        unique_files = []
        for file_path in files:
            if file_path not in unique_files:
                unique_files.append(file_path)
        
        return sorted(unique_files)
    
    def process_files(self, path):
        """Xử lý file hoặc thư mục"""
        files = self.find_localtext_files(path)
        
        if not files:
            print(f"Không tìm thấy file *localText.json trong: {path}")
            return
        
        # Phân loại file main và locale
        main_files = []
        locale_files = []
        
        for file_path in files:
            if self.is_locale_file(file_path):
                locale_files.append(file_path)
            else:
                main_files.append(file_path)
        
        print(f"Tìm thấy {len(files)} file localText.json")
        print(f"  - File main: {len(main_files)}")
        print(f"  - File locale: {len(locale_files)}")
        
        if len(files) <= 10:
            print("Danh sách file:")
            for i, file_path in enumerate(files, 1):
                rel_path = os.path.relpath(file_path, os.getcwd())
                file_type = "(locale)" if self.is_locale_file(file_path) else "(main)"
                print(f"  {i}. {rel_path} {file_type}")
        else:
            print(f"Danh sách file (hiển thị 5 đầu tiên):")
            for i, file_path in enumerate(files[:5], 1):
                rel_path = os.path.relpath(file_path, os.getcwd())
                file_type = "(locale)" if self.is_locale_file(file_path) else "(main)"
                print(f"  {i}. {rel_path} {file_type}")
            print(f"  ... và {len(files)-5} file khác")
        
        print(f"\nBắt đầu xử lý...")
        start_time = time.time()
        
        success_count = 0
        
        # Xử lý file main trước
        print(f"\n--- Xử lý {len(main_files)} file main ---")
        for file_path in main_files:
            if self.process_json_file(file_path):
                success_count += 1
        
        # Dọn dẹp và tạo lại file locale
        if main_files:
            print(f"\n--- Dọn dẹp file locale cũ ---")
            self.clean_existing_locale_files(main_files)
            
            print(f"\n--- Tạo file locale mới ---")
            new_locale_files = self.create_locale_files_from_main(main_files)
            if new_locale_files:
                print(f"Tạo {len(new_locale_files)} file locale mới")
                locale_files = new_locale_files  # Chỉ xử lý file mới tạo
                # Cập nhật lại tổng số file
                files.extend(new_locale_files)
            else:
                print("Không có file locale mới cần tạo")
                locale_files = []
        
        # Xử lý file locale sau
        print(f"\n--- Xử lý {len(locale_files)} file locale ---")
        for file_path in locale_files:
            if self.process_locale_file(file_path):
                success_count += 1
        
        end_time = time.time()
        elapsed_time = end_time - start_time
        
        print(f"📊 KẾT QUẢ")
        print(f"📁 Tổng số file: {len(files)}")
        print(f"✅ Tạo: {success_count}")
        print(f"❌ Xóa: {len(files) - success_count}")
        print(f"🔄 Số lượt dịch: {self.translated_count}")
        print(f"⏱️ Thời gian: {elapsed_time:.2f} giây")

def main():
    parser = argparse.ArgumentParser(
        description="Script xử lý file *localText.json - Tự động dịch đa ngôn ngữ",
        formatter_class=argparse.RawDescriptionHelpFormatter
    )
    
    parser.add_argument(
        '--project', 
        required=True,
        help='Tên project (vd: 3385996759)'
    )
    
    parser.add_argument(
        '--path', 
        required=True,
        help='Đường dẫn tương đối trong ModConf (folder hoặc file). Vd: "." cho toàn bộ, "game_localText.json" cho file cụ thể'
    )
    
    parser.add_argument(
        '--dry-run', 
        action='store_true', 
        help='Chỉ hiển thị danh sách file sẽ được xử lý, không thực hiện dịch'
    )
    
    parser.add_argument(
        '--create-locales',
        type=str,
        help='Danh sách ngôn ngữ cần tạo file locale, cách nhau bởi dấu phẩy. Ví dụ: vi,es,fr hoặc "all" cho tất cả'
    )
    
    args = parser.parse_args()
    
    print("🚀 Script Xử Lý LocalText.json")
    print(f"� Project: {args.project}")
    print(f"�📂 Path: {args.path}")
    
    # Xây dựng đường dẫn đầy đủ
    project_modconf_path = os.path.join(args.project, "ModProject", "ModConf")
    
    if not os.path.exists(project_modconf_path):
        print(f"❌ Không tìm thấy thư mục: {project_modconf_path}")
        return
    
    # Xử lý đường dẫn đích
    if args.path == ".":
        # Xử lý toàn bộ ModConf
        target_path = project_modconf_path
    else:
        # Đường dẫn cụ thể trong ModConf
        target_path = os.path.join(project_modconf_path, args.path)
        
        # Nếu là file locale, chuyển về file main
        if os.path.isfile(target_path):
            processor_temp = LocalTextProcessor()
            if processor_temp.is_locale_file(target_path):
                main_file = processor_temp.find_main_file(target_path)
                if main_file:
                    target_path = main_file
                    print(f"🔄 Chuyển từ file locale sang file main: {os.path.relpath(main_file, os.getcwd())}")
    
    if not os.path.exists(target_path):
        print(f"❌ Không tìm thấy đường dẫn: {target_path}")
        return
    
    print(f"🎯 Đường dẫn đích: {os.path.relpath(target_path, os.getcwd())}")
    
    if args.dry_run:
        print("🔍 Chế độ: DRY RUN (chỉ xem danh sách file)")
    
    # Xử lý danh sách ngôn ngữ tùy chỉnh
    target_languages = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la']  # Mặc định
    if args.create_locales:
        if args.create_locales.lower() == 'all':
            target_languages = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la', 'pt', 'it', 'th', 'ar', 'hi', 'tr']
        else:
            target_languages = [lang.strip() for lang in args.create_locales.split(',')]
        print(f"🌍 Ngôn ngữ locale: {', '.join(target_languages)}")
    
    processor = LocalTextProcessor()
    processor.target_languages = target_languages  # Truyền danh sách ngôn ngữ
    
    if args.dry_run:
        # Chỉ hiển thị danh sách file
        files = processor.find_localtext_files(target_path)
        if files:
            print(f"\nTìm thấy {len(files)} file:")
            for i, file_path in enumerate(files, 1):
                rel_path = os.path.relpath(file_path, os.getcwd())
                file_type = "(locale)" if processor.is_locale_file(file_path) else "(main)"
                print(f"  {i}. {rel_path} {file_type}")
        else:
            print("Không tìm thấy file nào!")
    else:
        # Xử lý thực tế
        processor.process_files(target_path)

if __name__ == "__main__":
    main()
