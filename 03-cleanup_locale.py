#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script cleanup file locale - loại bỏ dữ liệu dư thừa
"""

import json
import os
import glob
import argparse

def cleanup_locale_file(file_path: str):
    """Clean up file locale, chỉ giữ lại en|tc|ch|kr"""
    print(f"🧹 Cleanup: {file_path}")
    
    try:
        # Đọc file
        with open(file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        modified = False
        
        for entry in data:
            if not isinstance(entry, dict):
                continue
                
            # Tìm tất cả các combined key
            combined_keys = []
            combined_value = None
            
            # Tìm tất cả key có dấu |
            for key in list(entry.keys()):
                if "|" in key:
                    combined_keys.append(key)
                    if not combined_value and entry[key]:  # Lấy giá trị đầu tiên không rỗng
                        combined_value = entry[key]
            
            # Nếu có nhiều hơn 1 combined key hoặc key không phải "en|tc|ch|kr"
            if len(combined_keys) > 1 or (len(combined_keys) == 1 and combined_keys[0] != "en|tc|ch|kr"):
                # Xóa tất cả combined key hiện tại
                for key in combined_keys:
                    del entry[key]
                    modified = True
                    print(f"  ❌ Removed key: {key}")
                
                # Thêm lại key chuẩn nếu có giá trị
                if combined_value:
                    entry["en|tc|ch|kr"] = combined_value
                    modified = True
                    print(f"  ✅ Added en|tc|ch|kr: {combined_value}")
        
        # Lưu file nếu có thay đổi
        if modified:
            with open(file_path, 'w', encoding='utf-8') as f:
                json.dump(data, f, ensure_ascii=False, indent='\t')
            print(f"  ✅ File đã được cleanup")
        else:
            print(f"  ℹ️  File không cần cleanup")
            
    except Exception as e:
        print(f"  ❌ Lỗi: {e}")

def main():
    parser = argparse.ArgumentParser(description='Cleanup file locale - loại bỏ dữ liệu dư thừa')
    parser.add_argument('--root', '-r', default='.', help='Thư mục gốc')
    parser.add_argument('--file', '-f', help='File cụ thể cần cleanup')
    
    args = parser.parse_args()
    
    if args.file:
        # Cleanup file cụ thể
        if os.path.exists(args.file):
            print(f"🔍 Cleanup file cụ thể: {args.file}")
            cleanup_locale_file(args.file)
        else:
            print(f"❌ File không tồn tại: {args.file}")
    else:
        # Tìm tất cả file locale trong thư mục
        root_dir = os.path.abspath(args.root)
        pattern = os.path.join(root_dir, "**", "ModConf", "*", "*localText.json")
        locale_files = glob.glob(pattern, recursive=True)
        
        print(f"🔍 Tìm thấy {len(locale_files)} file locale trong {root_dir}")
        
        for file_path in locale_files:
            cleanup_locale_file(file_path)
        
        print(f"\n🎉 Hoàn thành cleanup {len(locale_files)} file!")

if __name__ == "__main__":
    main()
