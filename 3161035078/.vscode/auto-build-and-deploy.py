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
WATCH_DIRS = [
    PROJECT_ROOT / "ModProject" / "ModCode",
    PROJECT_ROOT / "ModProject" / "ModConf",
]

# Load settings
def load_settings():
    """Load settings from settings.json"""
    try:
        with open(SETTINGS_FILE, 'r', encoding='utf-8') as f:
            settings = json.load(f)
            return settings.get('autoBuildDebounceSeconds', 60)
    except Exception as e:
        print(f"⚠️  Warning: Could not load settings.json, using default debounce (60s): {e}")
        return 60

DEBOUNCE_SECONDS = load_settings()

class FileChangeHandler(FileSystemEventHandler):
    def __init__(self):
        self.last_triggered = 0
        self.timer_start = time.time()
        self.last_auto_build = time.time()  # Track last auto-build separately
        self.pending_changes = set()  # Track files that changed during debounce
        self.is_rebuilding = False  # Flag to prevent double rebuilds
        
    def on_modified(self, event):
        if event.is_directory or self.is_rebuilding:
            return
            
        # Skip if file doesn't exist (sometimes happens with temp files)
        file_path = Path(event.src_path)
        if not file_path.exists():
            return
            
        current_time = time.time()
        
        # Always reset the auto-build countdown when a file changes
        self.last_auto_build = current_time
        
        # Debounce: only trigger rebuild if enough time has passed since last rebuild
        if current_time - self.last_triggered < 3:  # Use shorter debounce (3 seconds) for rapid saves
            print(f"⏳ File changed: {file_path.name} (debouncing, will rebuild soon...)")
            self.pending_changes.add(file_path.name)
            return
            
        self.last_triggered = current_time
        self.timer_start = current_time  # Reset timer when triggered by file change
        
        # Include pending changes in the message
        if self.pending_changes:
            print(f"\n🔄 Files changed: {', '.join(self.pending_changes | {file_path.name})}")
            self.pending_changes.clear()
        else:
            print(f"\n🔄 File changed: {file_path.name}")
        
        self.run_rebuild()
        
    def run_rebuild(self):
        """Execute the rebuild-and-deploy script"""
        self.is_rebuilding = True  # Set flag before rebuild
        
        print(f"\n{'='*60}")
        print(f"🚀 Starting rebuild and deploy at {datetime.now().strftime('%H:%M:%S')}")
        print(f"{'='*60}\n")
        
        try:
            # Run PowerShell script
            result = subprocess.run(
                ["powershell.exe", "-ExecutionPolicy", "Bypass", "-File", str(REBUILD_SCRIPT)],
                cwd=str(SCRIPT_DIR),
                capture_output=False,
                text=True
            )
            
            if result.returncode == 0:
                print(f"\n{'='*60}")
                print(f"✅ Rebuild completed successfully!")
                print(f"{'='*60}\n")
            else:
                print(f"\n{'='*60}")
                print(f"❌ Rebuild failed with code {result.returncode}")
                print(f"{'='*60}\n")
                
        except Exception as e:
            print(f"\n❌ Error running rebuild script: {e}\n")
        finally:
            self.is_rebuilding = False  # Clear flag after rebuild

def main():
    print(f"""
{'='*60}
🤖 Auto Build and Deploy - Project 3161035078
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
                # Show countdown
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
