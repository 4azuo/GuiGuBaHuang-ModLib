#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Utility module for console progress bars
"""

import time
import sys
import threading
import os
from typing import Optional, Any, Dict, List
from data_types import ProgressConfig
from consts import PROGRESS_BAR_CONFIG, UI_ICONS, UI_MESSAGES
import shutil

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
        self.not_render = False  # If True, disable auto-rendering
        
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

    def _render(self, description: str = "", forceRender: bool = False) -> None:
        """Render the progress bar"""
        if self.not_render and not forceRender:
            return
        line = self._get_rendered_text(description)
        
        # Clear previous render
        self._clear_previous_render()
        
        # Handle multiline description
        self.last_line_count = description.count('\n') + 1 if description else 1
        
        sys.stdout.write(f"\r{line}")
        sys.stdout.flush()

    def _get_rendered_text(self, description: str = "") -> str:
        if self.total == 0:
            return ""
        
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

        return line
    
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

def create_file_progress_config(prefix: str = None) -> ProgressConfig:
    """Create progress config for file processing"""
    if prefix is None:
        prefix = UI_ICONS['folder']
    
    return ProgressConfig(
        width=PROGRESS_BAR_CONFIG['overall_width'],
        fill_char=PROGRESS_BAR_CONFIG['fill_char'],
        empty_char=PROGRESS_BAR_CONFIG['empty_char'],
        prefix=prefix,
        show_percentage=PROGRESS_BAR_CONFIG['show_percentage'],
        show_count=PROGRESS_BAR_CONFIG['show_count'],
        show_time=PROGRESS_BAR_CONFIG['show_time']
    )

def print_header(title: str, subtitle: str = "") -> None:
    """Print formatted header"""
    print(f"{UI_ICONS['header']} {title}")
    if subtitle:
        print(f"   {subtitle}")

def print_section(title: str) -> None:
    """Print section header"""
    print(f"--- {title} ---")

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
    print(f"{UI_ICONS['result']} REPORT")
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


class FileProgressState:
    """State for individual file progress using ProgressBar"""
    
    def __init__(self, filename: str):
        self.filename = filename
        self.status = "waiting"  # waiting, processing, completed, error
        self.current_task = ""
        
        # Create individual progress bar for this file (100 steps for percentage)
        file_config = create_file_progress_config(prefix=self._get_status_icon())
        file_config.width = PROGRESS_BAR_CONFIG['children_width']
        self.progress_bar = ProgressBar(total=100, config=file_config)
        self.progress_bar.not_render = True  # Disable auto-rendering
        
    def _get_status_icon(self):
        """Get icon based on current status"""
        if self.status == "completed":
            return UI_ICONS['success']
        elif self.status == "processing":
            return UI_ICONS['file']
        elif self.status == "error":
            return UI_ICONS['error']
        else:
            return UI_ICONS['time']
    
    def set_processing(self, task: str = ""):
        self.status = "processing"
        self.current_task = task
        self.progress_bar.config.prefix = self._get_status_icon()
        
    def set_completed(self):
        self.status = "completed" 
        self.current_task = ""
        self.progress_bar.config.prefix = self._get_status_icon()
        self.progress_bar.set_progress(100)
        
    def set_error(self, error_msg: str = ""):
        self.status = "error"
        self.current_task = error_msg
        self.progress_bar.config.prefix = self._get_status_icon()
        
    def update_progress(self, progress: float, task: str = ""):
        progress_int = int(min(max(progress, 0.0), 1.0) * 100)
        self.progress_bar.set_progress(progress_int)
        if progress_int < 100:
            self.set_processing(task)
        else:
            self.set_completed()


class MultiFileProgressManager:
    """Manager for multiple file progress bars using ProgressBar"""
    
    def __init__(self, file_paths: List[str]):
        self.file_states: Dict[str, FileProgressState] = {}
        self.file_order = []
        self.total_files = len(file_paths)
        self.last_render_time = 0
        self.min_render_interval = 0.1  # Minimum time between renders
        self._lock = threading.Lock()
        self.last_line_count = 0
        
        # Create overall progress bar
        self.overall_progress = ProgressBar(
            total=self.total_files,
            config=create_file_progress_config(UI_ICONS['result'])
        )
        self.overall_progress.not_render = True  # Disable auto-rendering
        
        # Initialize file states
        for file_path in file_paths:
            filename = os.path.basename(file_path)
            self.file_states[filename] = FileProgressState(filename)
            self.file_order.append(filename)
    
    def update_file_progress(self, file_path: str, progress: float, task: str = ""):
        """Update progress for a specific file"""
        filename = os.path.basename(file_path)
        if filename in self.file_states:
            with self._lock:
                self.file_states[filename].update_progress(progress, task)
                self._throttled_render()
    
    def complete_file(self, file_path: str):
        """Mark file as completed"""
        filename = os.path.basename(file_path)
        if filename in self.file_states:
            with self._lock:
                self.file_states[filename].set_completed()
                # Update overall progress
                completed_count = sum(1 for state in self.file_states.values() if state.status == "completed")
                self.overall_progress.set_progress(completed_count, f"Completed {filename}")
                self._render()
    
    def error_file(self, file_path: str, error_msg: str = ""):
        """Mark file as error"""
        filename = os.path.basename(file_path)
        if filename in self.file_states:
            with self._lock:
                self.file_states[filename].set_error(error_msg)
                self._render()
    
    def _throttled_render(self):
        """Render with throttling to prevent flickering"""
        current_time = time.time()
        if current_time - self.last_render_time >= self.min_render_interval:
            self.last_render_time = current_time
            self._render()
    
    def _render(self):
        """Render all progress bars"""
        # Calculate statistics
        completed = sum(1 for state in self.file_states.values() if state.status == "completed")
        processing = sum(1 for state in self.file_states.values() if state.status == "processing")
        errors = sum(1 for state in self.file_states.values() if state.status == "error")
        waiting = self.total_files - completed - processing - errors
        
        # Render overall progress using ProgressBar
        overall_description = (
            f"Overall Progress - "
            f"{UI_ICONS['success']} {completed}  "
            f"{UI_ICONS['file']} {processing}  "
            f"{UI_ICONS['time']}  {waiting}  "
            f"{UI_ICONS['error']} {errors}"
            "\n"
        )

        for i, filename in enumerate(self.file_order):
            state = self.file_states[filename]
            if state.status in ["processing", "error"]:
                overall_description += f"　{i:3} {self._render_file_progress(state)}\n"
        
        # Update overall progress bar display
        self.overall_progress.current = completed
        self.overall_progress._render(overall_description, forceRender=True)
    
    def _render_file_progress(self, state: FileProgressState) -> int:
        """Render progress for a single file using ProgressBar, return number of lines printed"""
        # Create description with filename and task
        description = f"{state.filename}"
        if state.current_task and state.status in ["processing", "error"]:
            task_text = state.current_task
            description += f" - {task_text}"
        
        # Render using ProgressBar
        return state.progress_bar._get_rendered_text(description)
    
    def finish(self):
        """Complete the progress display"""
        with self._lock:
            print("=" * 60)
            # Calculate final statistics
            completed = sum(1 for state in self.file_states.values() if state.status == "completed")
            processing = sum(1 for state in self.file_states.values() if state.status == "processing")
            errors = sum(1 for state in self.file_states.values() if state.status == "error")
            waiting = self.total_files - completed - processing - errors
            
            # Complete overall progress bar
            self.overall_progress.finish(f"Processing finished! {completed}/{self.total_files} files completed")
            
            # Final detailed report
            print("=" * 60)
            print(f"📊 FINAL REPORT")
            print("=" * 60)
            
            # Summary statistics
            print(f"Total files: {self.total_files}")
            print(f"{UI_ICONS['success']} Completed: {completed}")
            print(f"{UI_ICONS['error']} Errors: {errors}")
            print(f"{UI_ICONS['file']} Still processing: {processing}")
            print(f"{UI_ICONS['time']} Not started: {waiting}")
            print()
            
            # Detailed file results by category
            if completed > 0:
                print(f"✅ Successfully completed files ({completed}):")
                for filename in self.file_order:
                    state = self.file_states[filename]
                    if state.status == "completed":
                        print(f"  {UI_ICONS['success']} {filename}")
                print()
            
            if errors > 0:
                print(f"❌ Failed files ({errors}):")
                for filename in self.file_order:
                    state = self.file_states[filename]
                    if state.status == "error":
                        error_msg = f" - {state.current_task}" if state.current_task else ""
                        print(f"  {UI_ICONS['error']} {filename}{error_msg}")
                print()
            
            if processing > 0:
                print(f"⏳ Files still processing ({processing}):")
                for filename in self.file_order:
                    state = self.file_states[filename]
                    if state.status == "processing":
                        task_msg = f" - {state.current_task}" if state.current_task else ""
                        print(f"  {UI_ICONS['file']} {filename}{task_msg}")
                print()
            
            if waiting > 0:
                print(f"⏸️ Files not started ({waiting}):")
                for filename in self.file_order:
                    state = self.file_states[filename]
                    if state.status == "waiting":
                        print(f"  {UI_ICONS['time']} {filename}")
                print()
            
            print("=" * 60)
