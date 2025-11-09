#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Terminology Processor - Handle terminology to protect from translation
"""

import re
from typing import Dict, List, Tuple
from consts import TERMINOLOGY_CONFIG


class TerminologyProcessor:
    """Process terminology in text to protect from translation"""
    
    def __init__(self):
        self.config = TERMINOLOGY_CONFIG
        self.prefix = self.config['prefix']
        self.suffix = self.config['suffix']
        self.marker_pattern = re.compile(self.config['marker_pattern'], re.IGNORECASE)
        self.terms = self.config['terms']
        
        # Create patterns to match terminology (case insensitive, word boundary)
        self.term_patterns = []
        for term in self.terms:
            # Escape special regex characters and create word boundary
            escaped_term = re.escape(term)
            pattern = re.compile(r'\b' + escaped_term + r'\b', re.IGNORECASE)
            self.term_patterns.append((term, pattern))
    
    def protect_terms(self, text: str, preserve_case: bool = False) -> Tuple[str, Dict[str, str]]:
        """
        Protect terminology by replacing with indexed markers
        
        Args:
            text: Text to process
            preserve_case: If True, preserve original text case when restoring
                          If False, use case from terminology config
            
        Returns:
            Tuple[str, Dict[str, str]]: (protected_text, replacement_map)
        """
        if not text or not isinstance(text, str):
            return text, {}
        
        protected_text = text
        replacement_map = {}
        term_index = 0
        
        # Find all matches first
        all_matches = []
        for original_term, pattern in self.term_patterns:
            matches = list(pattern.finditer(protected_text))
            for match in matches:
                all_matches.append((match.start(), match.end(), match.group(), original_term, pattern))
        
        # Sort by position to process from end to start (avoid index changes)
        all_matches.sort(key=lambda x: x[0], reverse=True)
        
        # Replace terminology with indexed markers
        for start, end, match_text, original_term, pattern in all_matches:
            # Create indexed marker
            indexed_marker = f"{self.prefix}{term_index}{self.suffix}"
            # Decide whether to use original case or case from config
            term_to_restore = match_text if preserve_case else original_term
            replacement_map[indexed_marker] = term_to_restore
            
            # Replace in text using exact position
            protected_text = protected_text[:start] + indexed_marker + protected_text[end:]
            term_index += 1
        
        return protected_text, replacement_map
    
    def restore_terms(self, text: str, replacement_map: Dict[str, str] = None) -> str:
        """
        Restore terminology from indexed markers
        
        Args:
            text: Translated text
            replacement_map: Map of indexed markers -> original terms (optional)
            
        Returns:
            str: Text with restored terminology
        """
        if not text or not isinstance(text, str):
            return text
        
        restored_text = text
        
        # If replacement_map exists, use it for accurate restoration
        if replacement_map:
            # Sort markers by index for correct replacement order
            sorted_markers = sorted(replacement_map.items(), key=lambda x: x[0])
            for marker, original_term in sorted_markers:
                restored_text = restored_text.replace(marker, original_term)
        else:
            # Fallback: find all indexed markers and warn
            def restore_marker(match):
                marker_content = match.group(0)
                # Return original marker since no replacement_map
                return marker_content
            
            restored_text = self.marker_pattern.sub(restore_marker, restored_text)
        
        return restored_text
    
    def is_protected_text(self, text: str) -> bool:
        """
        Check if text contains protected terminology
        
        Args:
            text: Text to check
            
        Returns:
            bool: True if contains markers
        """
        if not text or not isinstance(text, str):
            return False
        return bool(self.marker_pattern.search(text))
    
    def count_protected_terms(self, text: str) -> int:
        """
        Count number of protected terms in text
        
        Args:
            text: Text to count
            
        Returns:
            int: Number of protected terms
        """
        if not text or not isinstance(text, str):
            return 0
        return len(self.marker_pattern.findall(text))
    
    def get_protected_terms_list(self, text: str, replacement_map: Dict[str, str] = None) -> List[str]:
        """
        Get list of protected terms in text
        
        Args:
            text: Text to analyze
            replacement_map: Map to get original terms from indexed markers
            
        Returns:
            List[str]: List of protected terms (without markers)
        """
        if not text or not isinstance(text, str):
            return []
        
        matches = self.marker_pattern.findall(text)
        terms = []
        
        if replacement_map:
            # Use replacement_map to get original terms
            for match in matches:
                if match in replacement_map:
                    term = replacement_map[match]
                    if term not in terms:
                        terms.append(term)
        else:
            # Fallback: only return list of indexed markers
            for match in matches:
                if match not in terms:
                    terms.append(match)
                    
        return terms
    
    def add_custom_term(self, term: str):
        """
        Add custom term to protection list
        
        Args:
            term: New term to protect
        """
        if term and term not in self.terms:
            self.terms.append(term)
            # Create new pattern for this term
            escaped_term = re.escape(term)
            pattern = re.compile(r'\b' + escaped_term + r'\b', re.IGNORECASE)
            self.term_patterns.append((term, pattern))
    
    def remove_custom_term(self, term: str):
        """
        Remove term from protection list
        
        Args:
            term: Term to remove
        """
        if term in self.terms:
            self.terms.remove(term)
            # Remove corresponding pattern
            self.term_patterns = [
                (t, p) for t, p in self.term_patterns 
                if t.lower() != term.lower()
            ]
    
    def get_statistics(self, text: str, replacement_map: Dict[str, str] = None) -> Dict[str, any]:
        """
        Get statistics about terminology in text
        
        Args:
            text: Text to analyze
            replacement_map: Map to get original terms from indexed markers
            
        Returns:
            Dict: Statistical information
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