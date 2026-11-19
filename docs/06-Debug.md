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

## Unity Player Log

**Location:** `C:\Users\<USER>\AppData\LocalLow\guigugame\guigubahuang\Player.log` (previous run: `Player-prev.log`)

Shows the game's own mod loader: one `加载模组：<path>` line per mod config (`ModExportData.cache`) it loads, plus Unity errors.

**MelonLoader must be active for code mods:** `<GameFolder>\version.dll` is the MelonLoader proxy. Without it the game still starts and still loads mod *configs*, but no mod DLL (ModLib, Taoist) runs and `MelonLoader\Latest.log` is not updated. Restore it with Steam → game → Properties → Installed Files → *Verify integrity of game files*.

**Non-ASCII game path:** the proxy stops the game within about 2 s (exit code 0, no log) when the game path contains non-ASCII characters, as with Steam's default `...\common\鬼谷八荒`. `run-game.ps1` starts the game through an ASCII junction (`%LOCALAPPDATA%\ggbh-game-link`) to avoid this. `-Steam` forces a normal Steam launch instead.

## Run the Game and Collect Logs

`scripts\game\run-game.ps1` starts the game (through Steam), follows **Player.log**, **MelonLoader\Latest.log** and the **ModLib logs** (`mod\<modId>\*.log`, incl. per save game) live, and saves each run to `logs\game-runs\<yyyyMMdd-HHmmss>\`:

| File | Content |
|---|---|
| `timeline.log` | Every new line of every log, time-stamped and tagged with its source |
| `player.log`, `melonloader.log`, `mod-<modId>-<file>.log` | New content of each log during the run |
| `summary.json` | `melonProxyPresent`, `melonLoaderStarted`, `assembliesRegenerated`, `modLoadComplete`, `modsConfLoaded`, `errorCount`, `errors`, `stopReason` |

The console shows only important lines (errors, MelonLoader, ModLib logs, mod loading); `-ShowAll` shows everything.

```powershell
# start, follow logs until the game is closed
.\scripts\game\run-game.ps1
# automated check: wait until code mods are loaded (max 10 min), then close the game
.\scripts\game\run-game.ps1 -UntilModLoaded -StopGame -Duration 600
# stop when a log line matches a regex, e.g. a ModLib message
.\scripts\game\run-game.ps1 -Until 'ModLib loaded' -StopGame
# game already running: follow new lines only
.\scripts\game\run-game.ps1 -Attach
```

Other options: `-Restart` (close a running game first), `-Direct` (start the exe instead of Steam), `-ExtraLog <path/glob>` (follow more log files), `-StartTimeout`, `-RestartGraceSec` (the game may restart itself, the new process is followed).
Exit codes: `0` game ran, `1` setup error, `2` game did not start.

VS Code tasks: **Game: Run & Watch Logs**, **Game: Run until Mods Loaded**, **Game: Watch Logs (attach)**, and **Open: Folder...** (game, MelonLoader, Workshop, build output, log folders; same as `scripts\open-folder.ps1 <target>`).

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

## Error Reports (BugSplat)

`DebugHelper.WriteLine(Exception)` also hands the exception to `CrashReportHelper` (ModLib), which sends it to [BugSplat](https://www.bugsplat.com), to the database **of the mod where the error happened**, only when the player allowed it.

**Each mod sets its own database** in its `ModConf/crash_reports.json` (not a game config: `ConfHelper` skips it). No file / invalid database: the errors of that mod are never sent. Taoist: `3161035078/ModProject/ModConf/crash_reports.json`.

```json
{
	"Database": "my-bugsplat-db",
	"ModName": "My mod",
	"LogLines": 200
}
```

| Key | Meaning |
|---|---|
| `Database` | BugSplat database (`https://<db>.bugsplat.com`; letters, digits, `-`, `_`) |
| `ModName` | Shown to the player in the consent dialog; BugSplat application name (default: the mod id) |
| `LogLines` | Last lines of the ModLib log sent as `log.txt` (default 200, 0 = no log; at most 64 KB, the end kept) |

Version of the reports: `CrashReportHelper.SetModVersion(modId, version)` at init (Taoist: `ModMain.OnLoadClass`, `TAOIST_VERSION`), else the ModLib version.

**Mod of an error**: the first `MOD_<modId>` assembly with a database in the stack of the exception (innermost first), then in the stack of the code that logged it (an error inside ModLib called by Taoist goes to Taoist); else ModLib's own mod when it has a database; else not sent.

**Consent**: on the title screen a Yes/No dialog lists the mods sending reports and what is sent; the answer is saved with its key (`ModTitleEvent.CrashReportConsent` + `CrashReportConsentKey` = `v<CONSENT_VERSION>:<mod ids>`, global cache). The player is asked again when the key changes: a new mod with a database, or `CrashReportHelper.CONSENT_VERSION` increased (do it whenever more data is sent). Change it any time: **title screen → ModCreator → Error reports**. Closing the dialog without answering = asked again next start.

**Sent**: `Callstack.txt` (`Exception.ToString()`), `log.txt` (last `LogLines` lines of the ModLib log of the day, with the game data folder, the user folder, the Windows account and the player id replaced by `<game data>`, `<user folder>`, `<user>`, `<player>`), application name/version of the mod, a one-line description, an anonymous random id (`user`) and attributes `modId`, `modLibVersion`, `caller`, `gameVersion`, `mods` (loaded mod ids), `language`. Not sent: saves, Steam account. Each distinct error (`DebugHelper.GetErrorKey`) is sent once per session, at most 5 per session, on a worker thread (one retry); the result is in the MelonLoader log (`[CrashReport] sent` / `not sent`).

Protocol (BugSplat .NET Standard client): `GET https://<db>.bugsplat.com/api/getCrashUploadUrl` → `PUT` zip to the returned URL → `POST /api/commitS3CrashUpload` (crashTypeId 18). Check a database without the game: `$env:BUGSPLAT_TEST_DB="<db>"; scripts/run-unittests.ps1` runs `CrashReportHelperTests.Live_SendToBugSplat` (skipped when the variable is not set).

In the game: autotest **TT-06** (`.\scripts\game\run-autotest.ps1 -Deploy -Scenario TT-06`) drives the consent dialog and checks every request against a local stand-in of BugSplat (`AutoTest/MockBugSplatServer.cs`, through `CrashReportHelper.ServerUrlOverride`); evidence in the test viewer (screenshots: consent dialog, captured requests, ModCreator row) and in `logs/autotest/<run>/bugsplat/` (`requests.json`, uploaded zip, `Callstack.txt`, commit form). Set `$env:BUGSPLAT_TEST_DB` before the run to also send one real report.
