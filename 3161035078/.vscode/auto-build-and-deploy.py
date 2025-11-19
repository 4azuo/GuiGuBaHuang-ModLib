#!/usr/bin/env python3
"""
Auto Build and Deploy Script
Monitors file changes and automatically runs rebuild-and-deploy.ps1
"""

import os
import sys
import time
import json
import subprocess
from pathlib import Path
from datetime import datetime
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

# Configuration
SCRIPT_DIR = Path(__file__).parent
PROJECT_ROOT = SCRIPT_DIR.parent
REBUILD_SCRIPT = SCRIPT_DIR / "rebuild-and-deploy.ps1"
SETTINGS_FILE = SCRIPT_DIR / "settings.json"
ERROR_LOG_FILE = SCRIPT_DIR / "auto-built-errors.log"
WATCH_DIRS = [
    PROJECT_ROOT / "ModProject" / "ModCode",
    PROJECT_ROOT / "ModProject" / "ModConf",
    PROJECT_ROOT / "ModProject" / "ModImg",
]

# Load settings
def load_settings():
    """Load settings from settings.json"""
    try:
        with open(SETTINGS_FILE, 'r', encoding='utf-8') as f:
            settings = json.load(f)
            project_id = settings.get('projectId', 'Unknown')
            auto_build_debounce = settings.get('autoBuildDebounceSeconds', 60)
            rapid_change_debounce = settings.get('rapidChangeDebounceSeconds', 3)
            return project_id, auto_build_debounce, rapid_change_debounce
    except Exception as e:
        print(f"⚠️  Warning: Could not load settings.json, using defaults (auto: 60s, rapid: 3s): {e}")
        return 'Unknown', 60, 3

PROJECT_ID, DEBOUNCE_SECONDS, RAPID_CHANGE_DEBOUNCE_SECONDS = load_settings()

class FileChangeHandler(FileSystemEventHandler):
    def __init__(self):
        self.last_triggered = 0
        self.timer_start = time.time()
        self.last_auto_build = time.time()  # Track last auto-build separately
        self.is_rebuilding = False  # Flag to prevent double rebuilds
        
    def on_modified(self, event):
        if event.is_directory or self.is_rebuilding:
            return
            
        # Skip if file doesn't exist (sometimes happens with temp files)
        file_path = Path(event.src_path)
        if not file_path.exists():
            return
        
        # Skip if it's the error log file itself
        if file_path == ERROR_LOG_FILE:
            return
            
        current_time = time.time()
        
        # Always reset the auto-build countdown when a file changes
        self.last_auto_build = current_time
        
        # Debounce: only trigger rebuild if enough time has passed since last rebuild
        if current_time - self.last_triggered < RAPID_CHANGE_DEBOUNCE_SECONDS:
            return
            
        self.last_triggered = current_time
        self.timer_start = current_time  # Reset timer when triggered by file change
        
        print(f"\n🔄 File changed: {file_path.name}")
        self.run_rebuild()
        
    def run_rebuild(self):
        """Execute the rebuild-and-deploy script"""
        self.is_rebuilding = True  # Set flag before rebuild
        
        print(f"\n{'='*60}")
        print(f"🚀 Starting rebuild and deploy at {datetime.now().strftime('%H:%M:%S')}")
        print(f"{'='*60}\n")
        
        stdout_lines = []
        stderr_lines = []
        
        try:
            # Run PowerShell script with real-time output
            process = subprocess.Popen(
                ["powershell.exe", "-ExecutionPolicy", "Bypass", "-File", str(REBUILD_SCRIPT)],
                cwd=str(SCRIPT_DIR),
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,  # Merge stderr to stdout
                text=True,
                encoding='utf-8',
                errors='replace',
                bufsize=1,  # Line buffered
                universal_newlines=True
            )
            
            # Read output line by line in real-time
            for line in iter(process.stdout.readline, ''):
                if line:
                    print(line, end='')
                    stdout_lines.append(line)
                    sys.stdout.flush()
            
            returncode = process.wait()
            
            if returncode == 0:
                print(f"\n{'='*60}")
                print(f"✅ Rebuild completed successfully!")
                print(f"{'='*60}\n")
                
                # Delete error log file if build succeeded
                if ERROR_LOG_FILE.exists():
                    try:
                        ERROR_LOG_FILE.unlink()
                    except Exception as e:
                        pass  # Ignore errors when deleting old log
            else:
                print(f"\n{'='*60}")
                print(f"❌ Rebuild failed with code {returncode}")
                print(f"{'='*60}\n")
                
                # Delete old log file before writing new one
                if ERROR_LOG_FILE.exists():
                    try:
                        ERROR_LOG_FILE.unlink()
                    except Exception:
                        pass  # Ignore errors when deleting old log
                
                # Write errors to log file
                try:
                    with open(ERROR_LOG_FILE, 'w', encoding='utf-8') as f:
                        f.write(f"Build failed at {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
                        f.write(f"Exit code: {returncode}\n")
                        f.write("="*60 + "\n\n")
                        
                        if stdout_lines:
                            f.write("OUTPUT:\n")
                            f.writelines(stdout_lines)
                            f.write("\n")
                    
                    print(f"📝 Errors logged to: {ERROR_LOG_FILE.name}\n")
                except Exception as e:
                    print(f"⚠️  Warning: Could not write error log: {e}\n")
                
        except Exception as e:
            print(f"\n❌ Error running rebuild script: {e}\n")
            
            # Delete old log file before writing new one
            if ERROR_LOG_FILE.exists():
                try:
                    ERROR_LOG_FILE.unlink()
                except Exception:
                    pass  # Ignore errors when deleting old log
            
            # Log exception to file
            try:
                with open(ERROR_LOG_FILE, 'w', encoding='utf-8') as f:
                    f.write(f"Exception at {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
                    f.write("="*60 + "\n\n")
                    f.write(f"Error: {str(e)}\n")
                
                print(f"📝 Exception logged to: {ERROR_LOG_FILE.name}\n")
            except Exception as log_error:
                print(f"⚠️  Warning: Could not write error log: {log_error}\n")
        finally:
            self.is_rebuilding = False  # Clear flag after rebuild

def main():
    print(f"""
{'='*60}
🤖 Auto Build and Deploy - Project {PROJECT_ID}
{'='*60}
Watching directories:
""")
    
    for watch_dir in WATCH_DIRS:
        if watch_dir.exists():
            print(f"  ✓ {watch_dir.relative_to(PROJECT_ROOT)}")
        else:
            print(f"  ✗ {watch_dir.relative_to(PROJECT_ROOT)} (not found)")
    
    print(f"""
Settings:
  • Auto-rebuild interval: {DEBOUNCE_SECONDS} seconds
  • Rapid change debounce: {RAPID_CHANGE_DEBOUNCE_SECONDS} seconds
  • Rebuild script: {REBUILD_SCRIPT.name}

Press Ctrl+C to stop monitoring...
{'='*60}
""")
    
    # Create event handler and observer
    event_handler = FileChangeHandler()
    observer = Observer()
    
    # Watch all configured directories
    for watch_dir in WATCH_DIRS:
        if watch_dir.exists():
            observer.schedule(event_handler, str(watch_dir), recursive=True)
    
    observer.start()
    
    try:
        while True:
            time.sleep(1)
            current_time = time.time()
            
            # Auto-rebuild every 60 seconds if no file changes triggered it
            if current_time - event_handler.last_auto_build >= DEBOUNCE_SECONDS:
                if not event_handler.is_rebuilding:  # Only auto-rebuild if not already rebuilding
                    print(f"\n⏰ Auto-rebuild triggered (60 seconds elapsed)")
                    event_handler.run_rebuild()
                    event_handler.last_triggered = current_time
                    event_handler.last_auto_build = current_time
            else:
                # Show countdown only if not rebuilding
                if not event_handler.is_rebuilding:
                    remaining = DEBOUNCE_SECONDS - (current_time - event_handler.last_auto_build)
                    sys.stdout.write(f"\r⏳ Next auto-rebuild in {int(remaining)} seconds...  ")
                    sys.stdout.flush()
                
    except KeyboardInterrupt:
        print(f"\n\n{'='*60}")
        print("🛑 Stopping auto build and deploy...")
        print(f"{'='*60}\n")
        observer.stop()
    
    observer.join()

if __name__ == "__main__":
    # Check if watchdog is installed
    try:
        import watchdog
    except ImportError:
        print("❌ Error: 'watchdog' module not found!")
        print("Please install it with: pip install watchdog")
        sys.exit(1)
    
    main()
