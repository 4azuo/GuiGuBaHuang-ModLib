#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Terminology Processor - Xử lý thuật ngữ để bảo vệ khỏi bị dịch
"""

import re
from typing import Dict, List, Tuple
from consts import TERMINOLOGY_CONFIG


class TerminologyProcessor:
    """Xử lý thuật ngữ trong text để bảo vệ khỏi bị dịch"""
    
    def __init__(self):
        self.config = TERMINOLOGY_CONFIG
        self.prefix = self.config['prefix']
        self.suffix = self.config['suffix']
        self.marker_pattern = re.compile(self.config['marker_pattern'], re.IGNORECASE)
        self.terms = self.config['terms']
        
        # Tạo pattern để match các thuật ngữ (case insensitive, word boundary)
        self.term_patterns = []
        for term in self.terms:
            # Escape special regex characters và tạo word boundary
            escaped_term = re.escape(term)
            pattern = re.compile(r'\b' + escaped_term + r'\b', re.IGNORECASE)
            self.term_patterns.append((term, pattern))
    
    def protect_terms(self, text: str, preserve_case: bool = True) -> Tuple[str, Dict[str, str]]:
        """
        Bảo vệ thuật ngữ bằng cách thay thế bằng markers với index
        
        Args:
            text: Text cần xử lý
            preserve_case: Nếu True, giữ nguyên case của text gốc khi khôi phục
                          Nếu False, sử dụng case từ terminology config
            
        Returns:
            Tuple[str, Dict[str, str]]: (protected_text, replacement_map)
        """
        if not text or not isinstance(text, str):
            return text, {}
        
        protected_text = text
        replacement_map = {}
        term_index = 0
        
        # Tìm tất cả matches trước
        all_matches = []
        for original_term, pattern in self.term_patterns:
            matches = list(pattern.finditer(protected_text))
            for match in matches:
                all_matches.append((match.start(), match.end(), match.group(), original_term, pattern))
        
        # Sắp xếp theo vị trí xuất hiện để xử lý từ cuối lên đầu (tránh thay đổi index)
        all_matches.sort(key=lambda x: x[0], reverse=True)
        
        # Thay thế các thuật ngữ bằng indexed markers
        for start, end, match_text, original_term, pattern in all_matches:
            # Tạo indexed marker
            indexed_marker = f"{self.prefix}{term_index}{self.suffix}"
            # Quyết định sử dụng case gốc hay case từ config
            term_to_restore = match_text if preserve_case else original_term
            replacement_map[indexed_marker] = term_to_restore
            
            # Thay thế trong text bằng cách sử dụng vị trí chính xác
            protected_text = protected_text[:start] + indexed_marker + protected_text[end:]
            term_index += 1
        
        return protected_text, replacement_map
    
    def restore_terms(self, text: str, replacement_map: Dict[str, str] = None) -> str:
        """
        Khôi phục thuật ngữ từ indexed markers
        
        Args:
            text: Text đã được dịch
            replacement_map: Map các indexed markers -> thuật ngữ gốc (optional)
            
        Returns:
            str: Text với thuật ngữ được khôi phục
        """
        if not text or not isinstance(text, str):
            return text
        
        restored_text = text
        
        # Nếu có replacement_map, sử dụng nó để khôi phục chính xác
        if replacement_map:
            # Sắp xếp markers theo index để thay thế đúng thứ tự
            sorted_markers = sorted(replacement_map.items(), key=lambda x: x[0])
            for marker, original_term in sorted_markers:
                restored_text = restored_text.replace(marker, original_term)
        else:
            # Fallback: tìm tất cả indexed markers và cảnh báo
            def restore_marker(match):
                marker_content = match.group(0)
                # Trả về marker gốc vì không có replacement_map
                return marker_content
            
            restored_text = self.marker_pattern.sub(restore_marker, restored_text)
        
        return restored_text
    
    def is_protected_text(self, text: str) -> bool:
        """
        Kiểm tra xem text có chứa thuật ngữ được bảo vệ không
        
        Args:
            text: Text cần kiểm tra
            
        Returns:
            bool: True nếu có chứa markers
        """
        if not text or not isinstance(text, str):
            return False
        return bool(self.marker_pattern.search(text))
    
    def count_protected_terms(self, text: str) -> int:
        """
        Đếm số lượng thuật ngữ được bảo vệ trong text
        
        Args:
            text: Text cần đếm
            
        Returns:
            int: Số lượng thuật ngữ được bảo vệ
        """
        if not text or not isinstance(text, str):
            return 0
        return len(self.marker_pattern.findall(text))
    
    def get_protected_terms_list(self, text: str, replacement_map: Dict[str, str] = None) -> List[str]:
        """
        Lấy danh sách các thuật ngữ được bảo vệ trong text
        
        Args:
            text: Text cần phân tích
            replacement_map: Map để lấy thuật ngữ gốc từ indexed markers
            
        Returns:
            List[str]: Danh sách thuật ngữ được bảo vệ (không có markers)
        """
        if not text or not isinstance(text, str):
            return []
        
        matches = self.marker_pattern.findall(text)
        terms = []
        
        if replacement_map:
            # Sử dụng replacement_map để lấy thuật ngữ gốc
            for match in matches:
                if match in replacement_map:
                    term = replacement_map[match]
                    if term not in terms:
                        terms.append(term)
        else:
            # Fallback: chỉ trả về danh sách indexed markers
            for match in matches:
                if match not in terms:
                    terms.append(match)
                    
        return terms
    
    def add_custom_term(self, term: str):
        """
        Thêm thuật ngữ tùy chỉnh vào danh sách bảo vệ
        
        Args:
            term: Thuật ngữ mới cần bảo vệ
        """
        if term and term not in self.terms:
            self.terms.append(term)
            # Tạo pattern mới cho thuật ngữ này
            escaped_term = re.escape(term)
            pattern = re.compile(r'\b' + escaped_term + r'\b', re.IGNORECASE)
            self.term_patterns.append((term, pattern))
    
    def remove_custom_term(self, term: str):
        """
        Xóa thuật ngữ khỏi danh sách bảo vệ
        
        Args:
            term: Thuật ngữ cần xóa
        """
        if term in self.terms:
            self.terms.remove(term)
            # Xóa pattern tương ứng
            self.term_patterns = [
                (t, p) for t, p in self.term_patterns 
                if t.lower() != term.lower()
            ]
    
    def get_statistics(self, text: str, replacement_map: Dict[str, str] = None) -> Dict[str, any]:
        """
        Lấy thống kê về thuật ngữ trong text
        
        Args:
            text: Text cần phân tích
            replacement_map: Map để lấy thuật ngữ gốc từ indexed markers
            
        Returns:
            Dict: Thông tin thống kê
        """
        if not text or not isinstance(text, str):
            return {
                'total_terms': 0,
                'unique_terms': 0,
                'protected_terms': [],
                'has_protection': False
            }
        
        protected_terms = self.get_protected_terms_list(text, replacement_map)
        
        return {
            'total_terms': self.count_protected_terms(text),
            'unique_terms': len(protected_terms),
            'protected_terms': protected_terms,
            'has_protection': len(protected_terms) > 0
        }