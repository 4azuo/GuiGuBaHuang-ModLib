#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Utility module for console progress bars
"""

import time
import sys
import threading
from typing import Optional, Any, Dict
from data_types import ProgressConfig
from consts import PROGRESS_BAR_CONFIG, UI_ICONS, UI_MESSAGES

class ProgressBar:
    """Console progress bar with customizable display (thread-safe)"""
    
    def __init__(self, total: int, config: Optional[ProgressConfig] = None):
        self.total = total
        self.current = 0
        self.config = config or ProgressConfig()
        self.start_time = time.time()
        self.last_update_time = 0
        self.min_update_interval = PROGRESS_BAR_CONFIG['min_update_interval']
        self.last_line_count = 0  # Track number of lines from last render
        self._lock = threading.Lock()  # Thread safety lock
        
    def update(self, increment: int = 1, description: str = "") -> None:
        """Update progress bar (thread-safe)"""
        with self._lock:
            self.current = min(self.current + increment, self.total)
            
            # Throttle updates to avoid flickering
            current_time = time.time()
            if current_time - self.last_update_time < self.min_update_interval and self.current < self.total:
                return
            
            self.last_update_time = current_time
            self._render(description)
        
    def rerender(self, description: str = "") -> None:
        """Rerender progress bar (thread-safe)"""
        with self._lock:
            # Throttle updates to avoid flickering
            current_time = time.time()
            if current_time - self.last_update_time < self.min_update_interval and self.current < self.total:
                return
            
            self.last_update_time = current_time
            self._render(description)
    
    def set_progress(self, value: int, description: str = "") -> None:
        """Set absolute progress value (thread-safe)"""
        with self._lock:
            self.current = min(max(value, 0), self.total)
            self._render(description)
    
    def finish(self, description: str = None) -> None:
        """Complete the progress bar (thread-safe)"""
        with self._lock:
            if description is None:
                description = UI_MESSAGES['completed']
            self.current = self.total
            
            print(f"\n{description}")
            print()  # New line after completion
    
    def _render(self, description: str = "") -> None:
        """Render the progress bar"""
        if self.total == 0:
            return
        
        # Calculate progress
        progress = self.current / self.total
        filled_width = int(self.config.width * progress)
        
        # Build progress bar
        bar = (self.config.fill_char * filled_width + 
               self.config.empty_char * (self.config.width - filled_width))
        
        # Build display components
        components = []
        
        # Prefix
        if self.config.prefix:
            components.append(self.config.prefix)
        
        # Progress bar
        components.append(f"[{bar}]")
        
        # Percentage
        if self.config.show_percentage:
            components.append(f"{progress * 100:.1f}%")
        
        # Count
        if self.config.show_count:
            components.append(f"({self.current}/{self.total})")
        
        # Time info
        if self.config.show_time:
            elapsed = time.time() - self.start_time
            if self.current > 0:
                eta = (elapsed / self.current) * (self.total - self.current)
                components.append(f"ETA: {self._format_time(eta)}")
        
        # Description
        components.append(description)

        # Suffix
        if self.config.suffix:
            components.append(self.config.suffix)

        # Progress bar line
        line = " ".join(components)
        
        # Clear previous render
        self._clear_previous_render()
        
        # Handle multiline description
        self.last_line_count = description.count('\n') + 1 if description else 1
        
        sys.stdout.write(f"\r{line}")
        sys.stdout.flush()
    
    def _clear_previous_render(self) -> None:
        """Clear previous render based on line count"""
        # Move cursor up and clear each line
        for i in range(self.last_line_count):
            sys.stdout.write('\033[2K')  # Clear entire line
            if i != self.last_line_count - 1:
                sys.stdout.write('\033[1A')  # Move cursor up one line
    
    def _format_time(self, seconds: float) -> str:
        """Format time in readable format"""
        if seconds < 60:
            return f"{seconds:.0f}s"
        elif seconds < 3600:
            minutes = seconds // 60
            seconds = seconds % 60
            return f"{minutes:.0f}m {seconds:.0f}s"
        else:
            hours = seconds // 3600
            minutes = (seconds % 3600) // 60
            return f"{hours:.0f}h {minutes:.0f}m"

class ProgressContext:
    """Context manager for progress bars"""
    
    def __init__(self, total: int, description: str = "", config: Optional[ProgressConfig] = None):
        self.total = total
        self.description = description
        self.config = config
        self.progress_bar: Optional[ProgressBar] = None
    
    def __enter__(self) -> ProgressBar:
        self.progress_bar = ProgressBar(self.total, self.config)
        if self.description:
            print(self.description)
        return self.progress_bar
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        if self.progress_bar:
            self.progress_bar.finish()

def create_file_progress_config(prefix: str = None) -> ProgressConfig:
    """Create progress config for file processing"""
    if prefix is None:
        prefix = UI_ICONS['folder']
    
    return ProgressConfig(
        width=PROGRESS_BAR_CONFIG['width'],
        fill_char=PROGRESS_BAR_CONFIG['fill_char'],
        empty_char=PROGRESS_BAR_CONFIG['empty_char'],
        prefix=prefix,
        show_percentage=PROGRESS_BAR_CONFIG['show_percentage'],
        show_count=PROGRESS_BAR_CONFIG['show_count'],
        show_time=PROGRESS_BAR_CONFIG['show_time']
    )

def print_header(title: str, subtitle: str = "") -> None:
    """Print formatted header"""
    print(f"\n{UI_ICONS['header']} {title}")
    if subtitle:
        print(f"   {subtitle}")

def print_section(title: str) -> None:
    """Print section header"""
    print(f"\n--- {title} ---")

def print_file_info(filename: str, action: str = None, details: str = "") -> None:
    """Print file processing info"""
    if action is None:
        action = UI_MESSAGES['processing']
    
    line = f"  {UI_ICONS['file']} {filename}"
    if action != UI_MESSAGES['processing']:
        line += f" ({action})"
    if details:
        line += f" - {details}"
    print(line)

def print_result(icon: str, message: str, details: str = "") -> None:
    """Print result message"""
    line = f"    {icon} {message}"
    if details:
        line += f": {details}"
    print(line)

def print_stats(stats_dict: Dict[str, Any]) -> None:
    """Print statistics"""
    print(f"\n{UI_ICONS['result']} REPORT")
    for key, value in stats_dict.items():
        print(f"{key}: {value}")

def print_error(message: str, details: str = "") -> None:
    """Print error message"""
    line = f"{UI_ICONS['error']} {message}"
    if details:
        line += f": {details}"
    print(line)

def print_warning(message: str, details: str = "") -> None:
    """Print warning message"""
    line = f"{UI_ICONS['warning']} {message}"
    if details:
        line += f": {details}"
    print(line)

def print_success(message: str, details: str = "") -> None:
    """Print success message"""
    line = f"{UI_ICONS['success']} {message}"
    if details:
        line += f": {details}"
    print(line)

def print_info(message: str, details: str = "") -> None:
    """Print info message"""
    line = f"{UI_ICONS['info']} {message}"
    if details:
        line += f": {details}"
    print(line)
