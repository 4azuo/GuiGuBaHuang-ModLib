#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script để xử lý các file *localText.json
- Đọc và sửa JSON format
- Dịch từ tiếng Anh sang Chinese, Traditional Chinese, Korean
- Lưu lại file với format chuẩn

Cách sử dụng:
    python translate_localtext.py                           # Xử lý tất cả file trong thư mục hiện tại
    python translate_localtext.py file.json                 # Xử lý file cụ thể
    python translate_localtext.py folder/                   # Xử lý tất cả file trong folder
    python translate_localtext.py --help                    # Hiển thị hướng dẫn
"""

import json
import os
import glob
import time
import sys
import argparse
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
        """Kiểm tra xem có phải file locale không (nằm trong child folder của ModConf)"""
        try:
            parts = file_path.split(os.sep)
            
            # Tìm xem có ModConf không và file có nằm trong subfolder của ModConf không
            for i, part in enumerate(parts):
                if part == "ModConf":
                    # Nếu file nằm trực tiếp trong ModConf thì là main file
                    if i + 1 == len(parts) - 1:
                        return False
                    # Nếu file nằm trong subfolder của ModConf thì là locale file
                    elif i + 1 < len(parts) - 1:
                        return True
            
            return False
        except:
            return False
    
    def get_locale_language(self, file_path):
        """Lấy mã ngôn ngữ từ đường dẫn file locale"""
        try:
            parts = file_path.split(os.sep)
            
            # Tìm folder chứa file (folder này chính là mã ngôn ngữ)
            for i, part in enumerate(parts):
                if part == "ModConf" and i + 1 < len(parts) - 1:
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
                        print(f"    Đang dịch: {en_text[:50]}{'...' if len(en_text) > 50 else ''}")
                        translated = self.translate_text(en_text, target_lang_code)
                        new_item[target_lang_code] = translated
                        print(f"    → {translated}")
                    else:
                        new_item[target_lang_code] = en_text
                    
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
                    print(f"    Đang dịch: {en_text[:50]}{'...' if len(en_text) > 50 else ''}")
                    translated = self.translate_text(en_text, target_lang_code)
                    new_item[target_lang_code] = translated
                    print(f"    → {translated}")
                else:
                    new_item[target_lang_code] = en_text
                
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
            
        print(f"  Item {item_index + 1}: {en_text[:50]}{'...' if len(en_text) > 50 else ''}")
        
        # Dịch sang các ngôn ngữ khác nếu chưa có hoặc rỗng
        for lang_key, google_lang_code in self.language_codes.items():
            if lang_key not in item or not item[lang_key] or item[lang_key].strip() == "":
                print(f"    Đang dịch sang {lang_key}...")
                translated = self.translate_text(en_text, google_lang_code)
                item[lang_key] = translated
                print(f"    {lang_key}: {translated}")
            # else:
            #     print(f"    {lang_key}: {item[lang_key]} (đã có)")
        
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
            print(f"\nTạo file locale từ: {main_file_path}")
            
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
                # Tạo thư mục ngôn ngữ nếu chưa có
                lang_dir = os.path.join(modconf_path, lang)
                if not os.path.exists(lang_dir):
                    os.makedirs(lang_dir)
                    print(f"  Tạo thư mục: {lang_dir}")
                
                # Đường dẫn file locale
                locale_file_path = os.path.join(lang_dir, main_filename)
                
                # Chỉ tạo nếu file chưa tồn tại
                if not os.path.exists(locale_file_path):
                    created_files.append(locale_file_path)
                    print(f"  Sẽ tạo: {locale_file_path}")
                
        return created_files
    
    def process_locale_file(self, file_path):
        """Xử lý file locale dựa trên file main"""
        print(f"\n{'='*60}")
        print(f"Đang xử lý file locale: {file_path}")
        
        try:
            # Tìm file main tương ứng
            main_file_path = self.find_main_file(file_path)
            if not main_file_path:
                print(f"Không tìm thấy file main cho {file_path}")
                return self.process_json_file(file_path)  # Fallback to normal processing
            
            print(f"File main tương ứng: {main_file_path}")
            
            # Đọc file main
            with open(main_file_path, 'r', encoding='utf-8') as f:
                main_content = f.read()
            
            main_content = self.fix_json_format(main_content)
            main_data = json.loads(main_content)
            
            # Xác định ngôn ngữ đích từ thư mục chứa file locale
            target_lang = self.get_locale_language(file_path)
            if not target_lang:
                print(f"Không xác định được ngôn ngữ đích cho {file_path}")
                return False
            
            print(f"Ngôn ngữ đích: {target_lang}")
            
            # Tạo thư mục chứa file nếu chưa có
            locale_dir = os.path.dirname(file_path)
            if not os.path.exists(locale_dir):
                os.makedirs(locale_dir)
                print(f"Tạo thư mục: {locale_dir}")
            
            # Tạo bản dịch cho locale
            translated_data = self.create_combined_translation(main_data, target_lang)
            
            # Sắp xếp theo id nếu là array
            if isinstance(translated_data, list):
                try:
                    translated_data.sort(key=lambda x: x.get('id', 0) if isinstance(x.get('id'), (int, float)) else 0)
                except:
                    pass
            
            # Lưu file locale (tạo mới hoặc ghi đè)
            with open(file_path, 'w', encoding='utf-8') as f:
                json.dump(translated_data, f, ensure_ascii=False, indent='\t')
            
            file_status = "tạo mới" if not os.path.exists(file_path) else "cập nhật"
            print(f"✓ Hoàn thành file locale ({file_status}): {file_path}")
            self.processed_count += 1
            return True
            
        except Exception as e:
            print(f"Lỗi xử lý file locale {file_path}: {e}")
            return False

    def process_json_file(self, file_path):
        """Xử lý một file JSON"""
        print(f"\n{'='*60}")
        print(f"Đang xử lý: {file_path}")
        
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
            
            print(f"✓ Hoàn thành: {file_path}")
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
        
        # Tạo file locale mới từ file main nếu cần
        if main_files:
            print(f"\n--- Kiểm tra và tạo file locale mới ---")
            new_locale_files = self.create_locale_files_from_main(main_files)
            if new_locale_files:
                print(f"Tạo {len(new_locale_files)} file locale mới")
                locale_files.extend(new_locale_files)
                # Cập nhật lại tổng số file
                files.extend(new_locale_files)
            else:
                print("Không có file locale mới cần tạo")
        
        # Xử lý file locale sau
        print(f"\n--- Xử lý {len(locale_files)} file locale ---")
        for file_path in locale_files:
            if self.process_locale_file(file_path):
                success_count += 1
        
        end_time = time.time()
        elapsed_time = end_time - start_time
        
        print(f"\n{'='*60}")
        print(f"=== KẾT QUẢ ===")
        print(f"Tổng số file: {len(files)}")
        print(f"Thành công: {success_count}")
        print(f"Thất bại: {len(files) - success_count}")
        print(f"Số lượt dịch: {self.translated_count}")
        print(f"Thời gian: {elapsed_time:.2f} giây")

def main():
    parser = argparse.ArgumentParser(
        description="Script xử lý file *localText.json - dịch tự động từ tiếng Anh sang Chinese, Traditional Chinese, Korean và các ngôn ngữ khác",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Ví dụ sử dụng:
  python translate_localtext.py                              # Xử lý tất cả file trong thư mục hiện tại
  python translate_localtext.py game_localText.json         # Xử lý file cụ thể
  python translate_localtext.py ModConf/                     # Xử lý tất cả file trong folder ModConf
  python translate_localtext.py 3385996759/ModProject/ModConf      # Xử lý tất cả file trong project cụ thể
  python translate_localtext.py --create-locales vi,es,fr   # Tạo file locale cho Vietnamese, Spanish, French
        """
    )
    
    parser.add_argument(
        'path', 
        nargs='?', 
        default='.', 
        help='Đường dẫn đến file hoặc thư mục cần xử lý (mặc định: thư mục hiện tại)'
    )
    
    parser.add_argument(
        '--dry-run', 
        action='store_true', 
        help='Chỉ hiển thị danh sách file sẽ được xử lý, không thực hiện dịch'
    )
    
    parser.add_argument(
        '--create-locales',
        type=str,
        help='Danh sách ngôn ngữ cần tạo file locale, cách nhau bởi dấu phẩy (vd: vi,es,fr,de)'
    )
    
    args = parser.parse_args()
    
    print("=== Script Xử Lý LocalText.json ===")
    print(f"Đường dẫn: {os.path.abspath(args.path)}")
    
    if args.dry_run:
        print("Chế độ: DRY RUN (chỉ hiển thị danh sách file)")
    
    # Xử lý danh sách ngôn ngữ tùy chỉnh
    target_languages = ['vi', 'es', 'fr', 'de', 'ru', 'ja', 'la']  # Mặc định
    if args.create_locales:
        target_languages = [lang.strip() for lang in args.create_locales.split(',')]
        print(f"Ngôn ngữ locale sẽ tạo: {', '.join(target_languages)}")
    
    processor = LocalTextProcessor()
    processor.target_languages = target_languages  # Truyền danh sách ngôn ngữ
    
    if args.dry_run:
        # Chỉ hiển thị danh sách file
        files = processor.find_localtext_files(args.path)
        if files:
            print(f"\nTìm thấy {len(files)} file:")
            for i, file_path in enumerate(files, 1):
                rel_path = os.path.relpath(file_path, os.getcwd())
                print(f"  {i}. {rel_path}")
        else:
            print("Không tìm thấy file nào!")
    else:
        # Xử lý thực tế
        processor.process_files(args.path)

if __name__ == "__main__":
    main()
