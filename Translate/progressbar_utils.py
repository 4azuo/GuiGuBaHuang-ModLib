#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Utility module for console progress bars
"""

import time
import sys
from typing import Optional, Any, Dict
from data_types import ProgressConfig
from consts import PROGRESS_BAR_CONFIG

class ProgressBar:
    """Console progress bar with customizable display"""
    
    def __init__(self, total: int, config: Optional[ProgressConfig] = None):
        self.total = total
        self.current = 0
        self.config = config or ProgressConfig()
        self.start_time = time.time()
        self.last_update_time = 0
        self.min_update_interval = PROGRESS_BAR_CONFIG['min_update_interval']
    
    def _flush_line(self, content: str = "") -> None:
        """Clear current line and write new content"""
        clear_width = PROGRESS_BAR_CONFIG['clear_line_width']
        sys.stdout.write(f"\r{' ' * clear_width}\r{content}")
        sys.stdout.flush()
    
    def _clear_line(self) -> None:
        """Clear current line completely"""
        self._flush_line("")
        
    def update(self, increment: int = 1, description: str = "") -> None:
        """Update progress bar"""
        self.current = min(self.current + increment, self.total)
        
        # Throttle updates to avoid flickering
        current_time = time.time()
        if current_time - self.last_update_time < self.min_update_interval and self.current < self.total:
            return
        
        self.last_update_time = current_time
        self._render(description)
    
    def set_progress(self, value: int, description: str = "") -> None:
        """Set absolute progress value"""
        self.current = min(max(value, 0), self.total)
        self._render(description)
    
    def finish(self, description: str = "Hoàn thành") -> None:
        """Complete the progress bar"""
        self.current = self.total
        self._render(description)
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
        
        # Description (truncate if too long)
        if description:
            max_desc_length = PROGRESS_BAR_CONFIG['max_desc_length']
            if len(description) > max_desc_length:
                description = description[:max_desc_length-3] + "..."
            components.append(description)
        
        # Suffix
        if self.config.suffix:
            components.append(self.config.suffix)
        
        # Print with clear line để tránh text overlap
        line = " ".join(components)
        # Đảm bảo line không quá dài cho terminal
        max_line_length = PROGRESS_BAR_CONFIG['max_line_length']
        if len(line) > max_line_length:
            line = line[:max_line_length-3] + "..."
        
        # Use flush helper function
        self._flush_line(line)
    
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

class MultiProgressManager:
    """Manager for multiple progress bars"""
    
    def __init__(self):
        self.progress_bars: Dict[str, ProgressBar] = {}
        self.active_key: Optional[str] = None
    
    def create_progress(self, key: str, total: int, config: Optional[ProgressConfig] = None) -> ProgressBar:
        """Create a new progress bar"""
        progress_bar = ProgressBar(total, config)
        self.progress_bars[key] = progress_bar
        return progress_bar
    
    def get_progress(self, key: str) -> Optional[ProgressBar]:
        """Get existing progress bar"""
        return self.progress_bars.get(key)
    
    def set_active(self, key: str) -> None:
        """Set which progress bar is currently active for display"""
        self.active_key = key
    
    def update_active(self, increment: int = 1, description: str = "") -> None:
        """Update the currently active progress bar"""
        if self.active_key and self.active_key in self.progress_bars:
            self.progress_bars[self.active_key].update(increment, description)
    
    def finish_active(self, description: str = "Hoàn thành") -> None:
        """Finish the currently active progress bar"""
        if self.active_key and self.active_key in self.progress_bars:
            self.progress_bars[self.active_key].finish(description)
    
    def cleanup(self) -> None:
        """Clean up all progress bars"""
        for progress_bar in self.progress_bars.values():
            if progress_bar.current < progress_bar.total:
                progress_bar.finish("Đã dừng")
            else:
                # Clear line for completed progress bars too
                progress_bar._clear_line()
        self.progress_bars.clear()

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
            if exc_type is KeyboardInterrupt:
                self.progress_bar.finish("Đã dừng bởi người dùng")
            elif exc_type:
                self.progress_bar.finish("Lỗi xử lý")
            else:
                self.progress_bar.finish()

# Singleton manager instance
progress_manager = MultiProgressManager()

def create_file_progress_config(prefix: str = "📁") -> ProgressConfig:
    """Create progress config for file processing"""
    return ProgressConfig(
        width=PROGRESS_BAR_CONFIG['width'],
        fill_char=PROGRESS_BAR_CONFIG['fill_char'],
        empty_char=PROGRESS_BAR_CONFIG['empty_char'],
        prefix=prefix,
        show_percentage=PROGRESS_BAR_CONFIG['show_percentage'],
        show_count=PROGRESS_BAR_CONFIG['show_count'],
        show_time=PROGRESS_BAR_CONFIG['show_time']
    )

def create_translation_progress_config(prefix: str = "🌍") -> ProgressConfig:
    """Create progress config for translation"""
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
    print(f"\n🚀 {title}")
    if subtitle:
        print(f"   {subtitle}")

def print_section(title: str) -> None:
    """Print section header"""
    print(f"\n--- {title} ---")

def print_file_info(filename: str, action: str = "Xử lý", details: str = "") -> None:
    """Print file processing info"""
    line = f"  📄 {filename}"
    if action != "Xử lý":
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
    print("\n📊 KẾT QUẢ")
    for key, value in stats_dict.items():
        print(f"{key}: {value}")

def print_error(message: str, details: str = "") -> None:
    """Print error message"""
    line = f"❌ {message}"
    if details:
        line += f": {details}"
    print(line)

def print_warning(message: str, details: str = "") -> None:
    """Print warning message"""
    line = f"⚠️ {message}"
    if details:
        line += f": {details}"
    print(line)

def print_success(message: str, details: str = "") -> None:
    """Print success message"""
    line = f"✅ {message}"
    if details:
        line += f": {details}"
    print(line)

def print_info(message: str, details: str = "") -> None:
    """Print info message"""
    line = f"ℹ️ {message}"
    if details:
        line += f": {details}"
    print(line)
