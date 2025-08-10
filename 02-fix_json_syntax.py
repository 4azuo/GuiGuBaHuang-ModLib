#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Script sửa lỗi JSON syntax trong các file localText.json
- Sửa trailing comma (dấu phẩy dư ở cuối mảng/object)
- Validate và format lại JSON
"""

import json
import os
import glob
import re
from pathlib import Path
from typing import List, Dict, Any
import argparse

class JSONSyntaxFixer:
    def __init__(self):
        self.fixed_count = 0
        self.error_count = 0
        
    def fix_trailing_commas(self, content: str) -> str:
        """Sửa trailing comma trong JSON"""
        # Sửa trailing comma trước đóng ngoặc }
        content = re.sub(r',(\s*})', r'\1', content)
        
        # Sửa trailing comma trước đóng ngoặc ]
        content = re.sub(r',(\s*])', r'\1', content)
        
        return content
    
    def fix_json_file(self, file_path: str) -> bool:
        """Sửa lỗi JSON trong một file"""
        print(f"🔧 Sửa: {os.path.relpath(file_path)}")
        
        try:
            # Đọc file gốc
            with open(file_path, 'r', encoding='utf-8') as f:
                original_content = f.read()
            
            # Thử parse JSON trước khi sửa
            try:
                json.loads(original_content)
                print(f"  ✅ File đã hợp lệ, không cần sửa")
                return True
            except json.JSONDecodeError as e:
                print(f"  🔍 Phát hiện lỗi JSON: {e.msg} (line {e.lineno})")
            
            # Sửa trailing comma
            fixed_content = self.fix_trailing_commas(original_content)
            
            # Thử parse JSON sau khi sửa
            try:
                data = json.loads(fixed_content)
                
                # Format lại JSON với indent
                formatted_content = json.dumps(data, ensure_ascii=False, indent='\t')
                
                # Lưu file đã sửa
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(formatted_content)
                
                print(f"  ✅ Đã sửa và format lại")
                self.fixed_count += 1
                return True
                
            except json.JSONDecodeError as e:
                print(f"  ❌ Vẫn có lỗi sau khi sửa: {e.msg} (line {e.lineno})")
                
                # Thử sửa thêm một số lỗi phổ biến khác
                fixed_content = self.fix_additional_issues(fixed_content)
                
                try:
                    data = json.loads(fixed_content)
                    formatted_content = json.dumps(data, ensure_ascii=False, indent='\t')
                    
                    with open(file_path, 'w', encoding='utf-8') as f:
                        f.write(formatted_content)
                    
                    print(f"  ✅ Đã sửa sau lần thử thứ 2")
                    self.fixed_count += 1
                    return True
                    
                except json.JSONDecodeError as e:
                    print(f"  ❌ Không thể sửa: {e.msg} (line {e.lineno})")
                    self.error_count += 1
                    return False
                
        except Exception as e:
            print(f"  ❌ Lỗi xử lý file: {e}")
            self.error_count += 1
            return False
    
    def fix_additional_issues(self, content: str) -> str:
        """Sửa các lỗi JSON phổ biến khác"""
        
        # Sửa single quote thành double quote (nếu có)
        content = re.sub(r"'([^']*)':", r'"\1":', content)
        
        # Sửa missing quote cho key
        content = re.sub(r'(\w+):', r'"\1":', content)
        
        # Sửa các trailing comma còn sót
        content = re.sub(r',(\s*[}\]])', r'\1', content)
        
        return content
    
    def fix_all_files(self, root_dir: str, pattern: str = "**/*localText.json"):
        """Sửa tất cả file localText.json"""
        print(f"🚀 Sửa lỗi JSON syntax trong: {root_dir}")
        print(f"📁 Pattern: {pattern}")
        
        # Tìm tất cả file localText.json
        search_pattern = os.path.join(root_dir, pattern)
        files = glob.glob(search_pattern, recursive=True)
        files = [f for f in files if os.path.isfile(f)]
        
        print(f"📊 Tìm thấy {len(files)} file cần kiểm tra")
        
        if not files:
            print("❌ Không tìm thấy file nào!")
            return
        
        # Xử lý từng file
        for i, file_path in enumerate(files, 1):
            print(f"\n[{i}/{len(files)}]", end=" ")
            self.fix_json_file(file_path)
        
        # Thống kê
        print(f"\n🎉 Hoàn thành!")
        print(f"✅ Đã sửa: {self.fixed_count} file")
        print(f"❌ Lỗi: {self.error_count} file")
        print(f"📄 Tổng: {len(files)} file")
        
        if self.error_count > 0:
            print(f"\n💡 Tip: Kiểm tra thủ công {self.error_count} file bị lỗi")

def main():
    parser = argparse.ArgumentParser(description='Sửa lỗi JSON syntax trong file localText.json')
    parser.add_argument('--root', '-r', default='.', help='Thư mục gốc')
    parser.add_argument('--pattern', '-p', default='**/*localText.json', help='Pattern tìm file')
    
    args = parser.parse_args()
    
    root_dir = os.path.abspath(args.root)
    
    if not os.path.exists(root_dir):
        print(f"❌ Thư mục không tồn tại: {root_dir}")
        return
    
    fixer = JSONSyntaxFixer()
    fixer.fix_all_files(root_dir, args.pattern)

if __name__ == "__main__":
    main()
