# Debug and Troubleshooting

## Log Location

**Mod logs:** `C:\Users\<USER>\AppData\LocalLow\guigugame\guigubahuang\mod\flQrxq\`

All mods using ModLib save logs in this folder.

**Important:** Admin permissions required to create and write logs. Without admin rights, mod will not load.

## Log Files

**Global logs:** `debug-YYYYMMDD.log`
- Example: `debug-20251122.log`
- Contains logs from menu screen and global functions
- Created daily

**Save game logs:** `<saveId>_debug-YYYYMMDD.log`
- Example: `8jAQEE_debug-20251122.log`
- Contains logs for specific save game
- `saveId` is the player unit ID (e.g., `8jAQEE`)
- Created daily per save game

**Save state:** `<saveId>_configs.json`
- Example: `8jAQEE_configs.json`
- Stores global state for the save game

## MelonLoader Logs

**Location:** `<GameFolder>\MelonLoader\Latest.log`

Contains MelonLoader system logs, mod loading sequence, and framework-level errors.

**View in real-time:**
```powershell
Get-Content -Path "<GameFolder>\MelonLoader\Latest.log" -Wait -Tail 50
```

## Using DebugHelper

**Basic logging:**
```csharp
DebugHelper.WriteLine("Message");  // With timestamp
DebugHelper.WriteLine("Message", addTime: false);  // Without timestamp
DebugHelper.Write("Message");  // No line break
```

**Log exceptions:**
```csharp
try {
    // Your code
} catch (Exception ex) {
    DebugHelper.WriteLine(ex);  // Logs full exception with stack trace
}
```

**Manual save:**
```csharp
DebugHelper.Save();  // Flush logs to file immediately
```

**Debug mode:**
```csharp
DebugHelper.IsDebugMode = true;  // Show exception dialogs (default)
DebugHelper.IsDebugMode = false;  // Silent mode
```

**Log format:**
- Global: `[HH:mm:ss.fff] Message`
- In-game: `[HH:mm:ss.fff (YYYY/MM/DD)] Message` (includes game date)
