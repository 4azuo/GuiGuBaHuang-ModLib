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
        
        # Đảm bảo thứ tự key: id, key, __name (nếu có), en, ch, tc, kr, và các key khác
        ordered_item = {}
        
        # Thêm các key có thứ tự ưu tiên
        priority_keys = ['id', 'key', '__name', 'en', 'ch', 'tc', 'kr']
        for key in priority_keys:
            if key in item:
                ordered_item[key] = item[key]
        
        # Thêm các key còn lại
        for key, value in item.items():
            if key not in priority_keys:
                ordered_item[key] = value
                
        return ordered_item
    
    def process_json_file(self, file_path):
        """Xử lý một file JSON"""
        print(f"\n{'='*60}")
        print(f"Đang xử lý: {file_path}")
        
        try:
            # Tạo backup
            backup_path = file_path + ".backup"
            if not os.path.exists(backup_path):
                with open(file_path, 'r', encoding='utf-8') as f:
                    original_content = f.read()
                with open(backup_path, 'w', encoding='utf-8') as f:
                    f.write(original_content)
                print(f"Đã tạo backup: {backup_path}")
            
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
        
        # Loại bỏ duplicates và backup files
        unique_files = []
        for file_path in files:
            if not file_path.endswith('.backup') and file_path not in unique_files:
                unique_files.append(file_path)
        
        return sorted(unique_files)
    
    def process_files(self, path):
        """Xử lý file hoặc thư mục"""
        files = self.find_localtext_files(path)
        
        if not files:
            print(f"Không tìm thấy file *localText.json trong: {path}")
            return
        
        print(f"Tìm thấy {len(files)} file localText.json")
        if len(files) <= 10:
            print("Danh sách file:")
            for i, file_path in enumerate(files, 1):
                rel_path = os.path.relpath(file_path, os.getcwd())
                print(f"  {i}. {rel_path}")
        else:
            print(f"Danh sách file (hiển thị 5 đầu tiên):")
            for i, file_path in enumerate(files[:5], 1):
                rel_path = os.path.relpath(file_path, os.getcwd())
                print(f"  {i}. {rel_path}")
            print(f"  ... và {len(files)-5} file khác")
        
        print(f"\nBắt đầu xử lý...")
        start_time = time.time()
        
        success_count = 0
        for file_path in files:
            if self.process_json_file(file_path):
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
        description="Script xử lý file *localText.json - dịch tự động từ tiếng Anh sang Chinese, Traditional Chinese, Korean",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Ví dụ sử dụng:
  python translate_localtext.py                              # Xử lý tất cả file trong thư mục hiện tại
  python translate_localtext.py game_localText.json         # Xử lý file cụ thể
  python translate_localtext.py ModConf/                     # Xử lý tất cả file trong folder ModConf
  python translate_localtext.py 3385996759/ModProject/      # Xử lý tất cả file trong project cụ thể
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
    
    args = parser.parse_args()
    
    print("=== Script Xử Lý LocalText.json ===")
    print(f"Đường dẫn: {os.path.abspath(args.path)}")
    
    if args.dry_run:
        print("Chế độ: DRY RUN (chỉ hiển thị danh sách file)")
    
    processor = LocalTextProcessor()
    
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
