<#
.SYNOPSIS
    Run the Taoist (3161035078) test scenarios in the game and collect evidence.
.DESCRIPTION
    1. Writes <game-data>\mod\nE7UL2\autotest\request.json. Only while this file exists the
       in-game runner (Taoist AutoTestEvent) is loaded, so normal play is never affected.
    2. Starts the game through run-game.ps1 (logs are followed and saved as usual).
    3. The runner executes the title-screen scenarios, then the pre-test TP-01 creates a new
       character (S&M trainer on, CrueltyShura) in a free save slot - or, with -Save <name>,
       -ExistingSave (first save) or -ManualLoad, an existing save is used -, executes the game scenarios and, with
       -WatchBattles, checks every battle you play until -WatchMinutes elapse.
       TP-01 runs only when needed: a full run, -NewCharacter, or a selected scenario marked "newCharacter" in
       scenarios.json (TB-16: the 3 artifacts of the new character; TZ-01: ends the character); otherwise the
       first save of the list is used (backed up and restored as usual).
    4. Evidence is collected in <repo>\logs\autotest\<yyyyMMdd-HHmmss>\:
         results.json   results written by the game (checks, notes, runner log)
         screens\*.png  in-game screenshots taken by the scenarios
         game-run\      logs of the run (timeline.log, ModLib/MelonLoader/Unity logs, summary.json)
         report.json    final results (checks incl. log-error rule, screenshots by step) for the viewer
         summary.json   counts and exit status
    OK/NG of every id: 3161035078\tests\scenario-status.json. View everything in test-viewer\index.html.
    Scenario definitions: 3161035078\tests\scenarios.json (auto ids implemented in
    3161035078\ModProject\ModCode\ModMain\AutoTest\AutoTestScenarios.cs).

    The runner never saves, but the scenarios call mod functions on the loaded save. The save
    folders (<game data>\*\CacheData) and the ModLib caches (mod\*\globals, mod\*\saves) are backed up to <run>\save-backup before the game starts
    and restored after it closed (-NoRestore keeps the state after the run; nothing is restored
    with -KeepGame). Prefer a throwaway copy of a save anyway.

    Exit codes: 0 = all executed scenarios passed, 1 = setup error, 2 = game/runner did not
    finish (no results or timeout), 3 = at least one scenario failed or errored.
.EXAMPLE
    .\scripts\game\run-autotest.ps1 -Deploy
    # rebuild + deploy, start the game, create a new character (TP-01), run all auto scenarios
.EXAMPLE
    .\scripts\game\run-autotest.ps1 -Deploy -Save "AutoTest"
    # same on the existing save named AutoTest (no new character)
.EXAMPLE
    .\scripts\game\run-autotest.ps1 -ManualLoad -WatchBattles -WatchMinutes 15
    # you load the save yourself; after the auto scenarios, play battles for 15 minutes
.EXAMPLE
    .\scripts\game\run-autotest.ps1 -Scenario TT-01,TG-04,TG-10
.EXAMPLE
    .\scripts\game\run-autotest.ps1 -Failed
    # only the scenarios whose latest result is NG or NOT RUN (a full run only when really wanted; a full run
    # where everything passed is pinned: scripts\pin-test-run.ps1)
.EXAMPLE
    .\scripts\game\run-autotest.ps1 -Promo -Scenario TG-57,TB-07
    # promotion run: no check panels on the screenshots; the "promo" pictures are copied to ModProject\SteamImg
    # (only promotion runs write SteamImg; scripts\game\run-promo.ps1 runs every scenario with pictures)
.EXAMPLE
    .\scripts\game\run-autotest.ps1 -ListScenarios
    # scenario ids with their latest OK/NG result
.EXAMPLE
    .\scripts\game\run-autotest.ps1 -ReportFrom logs\autotest\20261115-170000
    # rebuild report.json, the OK/NG status and the viewer data from an existing run (no game)
#>

[CmdletBinding()]
param(
    [string]$Save,
    [switch]$ExistingSave,
    [switch]$ManualLoad,
    [string[]]$Scenario,
    [switch]$Failed,
    [switch]$NewCharacter,
    [switch]$WatchBattles,
    [int]$WatchMinutes = 10,
    [int]$Timeout = 900,
    [int]$StallSeconds = 300,
    # game slot: 0 (default) = the first free one (slot 1 = the normal game, 2..9 = new-game-slot.ps1 copies); a slot
    # used by another run (other terminal / session, scripts\game\_slots.ps1) is never taken: the run waits
    [ValidateRange(0, 9)][int]$Slot = 0,
    [int]$SlotWaitMinutes = 60,
    [switch]$NoSaveBackup,
    [switch]$SlotCharacter,
    [string]$GameArgs,
    # window of the game ("1600x900"): set by the mod in the game's own settings (kept)
    # game window: windowed at this size for every run (default 1600x900; '' keeps the game's own setting)
    [string]$Window = '1600x900',
    [switch]$NoViewerUpdate,
    # the user confirmed the tool scenarios (TE-xx, TI-xx) of -Scenario: no prompt
    [switch]$ConfirmTools,
    [int]$StepDelayMs = 1500,
    [switch]$Deploy,
    [switch]$KeepGame,
    # games side by side that stay open (run-autotest-parallel.ps1): -KeepAlive starts the game, runs the scenarios and
    # leaves it waiting in the save; -Attach gives the next scenarios to that game (no start, no restart)
    [switch]$KeepAlive,
    [switch]$Attach,
    [switch]$NoRestore,
    [switch]$ListScenarios,
    [switch]$Promo,
    # the game window may come to the front (default: kept behind the other windows, the focus given back)
    [switch]$Foreground,
    [string]$ReportFrom,
    [string]$GamePath
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_slots.ps1')
# game slot, locked for the whole run (released when this process ends): several runs at the same time each get their
# own game; a slot used by another run is never restarted
if ($ListScenarios -or $ReportFrom) { if ($Slot -lt 1) { $Slot = 1 } }
if ($KeepAlive -or $Attach) { $KeepGame = $true; $NoSaveBackup = $true }
else {
    $autoSlot = $Slot -eq 0
    # a named save / manual load: the saves of the normal game (slot 1); a slot copy has its own game data folder
    $candidates = if ($Save -or $ManualLoad) { @(1) } else { Get-InstalledGameSlots }
    $Slot = Select-GameSlot -Requested $Slot -Candidates $candidates -WaitMinutes $SlotWaitMinutes -What ("run-autotest " + (@($Scenario) -join ','))
    Write-Host "Game slot: $Slot$(if ($autoSlot) { ' (first free)' })" -ForegroundColor Cyan
    if ($autoSlot -and $Slot -ge 2 -and -not $ExistingSave -and -not $NewCharacter) {
        # slot copy: its own test character "AutoTest Slot<n>" (created by TP-01 the first time), as the parallel runs
        $SlotCharacter = $true
    }
}
# game slot (new-game-slot.ps1): read by _game.ps1 here and by run-game.ps1 (inherited)
$env:GGBH_SLOT = if ($Slot -ge 2) { "$Slot" } else { $null }
. (Join-Path $PSScriptRoot '_game.ps1')
. (Join-Path $PSScriptRoot '_autotest.ps1')
. (Join-Path $PSScriptRoot '_parallel.ps1')   # Set-GameWindow

$TaoistModId = 'nE7UL2'

# -Failed: only the auto scenarios whose latest result is NG or NOT RUN (scenario-status.json)
if ($Failed) {
    $statusFile = Join-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) '3161035078\tests\scenario-status.json'
    $st = Get-Content -LiteralPath $statusFile -Raw -Encoding UTF8 | ConvertFrom-Json
    $Scenario = @($st.scenarios | Where-Object { $_.kind -eq 'auto' -and $_.result -in 'NG', 'NOT RUN' } | ForEach-Object { $_.id })
    if ($Scenario.Count -eq 0) { Write-Host 'No NG / NOT RUN scenario in scenario-status.json: nothing to run.'; exit 0 }
    Write-Host "Scenarios NG / NOT RUN: $($Scenario -join ', ')"
}

# ------------------------------------------------------------------ screenshots
# The game cannot capture itself (UnityEngine.ScreenCapture is stripped): the runner writes
# <autotest>\snaps\<n>.req files containing the target png path, captured here.
Add-Type -AssemblyName System.Drawing
if (-not ('AutoTestWin32b' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class AutoTestWin32b {
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
}
'@
}
[void][AutoTestWin32b]::SetProcessDPIAware()

# Captures the game window with PrintWindow (PW_RENDERFULLCONTENT): works when other windows cover it.
# Falls back to a screen copy of the primary screen when the window is minimized or PrintWindow fails.
function Save-GameScreenshot([string]$Path) {
    New-Item -ItemType Directory -Path (Split-Path $Path) -Force | Out-Null
    # a few tries: the window handle of the game is sometimes not readable for a moment (the screen copy below then
    # gives the desktop instead of the game, TT-06 2026-11-19)
    for ($try = 0; $try -lt 3; $try++) {
        if ($try -gt 0) { Start-Sleep -Milliseconds 300 }
        $p = Get-GameProcess
        if (-not $p -or $p.MainWindowHandle -eq [IntPtr]::Zero -or [AutoTestWin32b]::IsIconic($p.MainWindowHandle)) { continue }
        $r = New-Object AutoTestWin32b+RECT
        if ([AutoTestWin32b]::GetWindowRect($p.MainWindowHandle, [ref]$r) -and ($r.Right - $r.Left) -gt 100) {
            $bmp = New-Object Drawing.Bitmap ($r.Right - $r.Left), ($r.Bottom - $r.Top)
            $gfx = [Drawing.Graphics]::FromImage($bmp)
            $hdc = $gfx.GetHdc()
            try { $ok = [AutoTestWin32b]::PrintWindow($p.MainWindowHandle, $hdc, 2) }
            finally { $gfx.ReleaseHdc($hdc); $gfx.Dispose() }
            try { if ($ok) { $bmp.Save($Path, [Drawing.Imaging.ImageFormat]::Png); return } }
            finally { $bmp.Dispose() }
        }
    }
    Add-Type -AssemblyName System.Windows.Forms
    $rect = [Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bmp = New-Object Drawing.Bitmap $rect.Width, $rect.Height
    $gfx = [Drawing.Graphics]::FromImage($bmp)
    try {
        $gfx.CopyFromScreen($rect.Location, [Drawing.Point]::Empty, $rect.Size)
        $bmp.Save($Path, [Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $gfx.Dispose(); $bmp.Dispose() }
}

# The game window as a bitmap (PrintWindow), null when it cannot be captured this way.
function Get-GameBitmap {
    $p = Get-GameProcess
    if (-not $p -or $p.MainWindowHandle -eq [IntPtr]::Zero -or [AutoTestWin32b]::IsIconic($p.MainWindowHandle)) { return $null }
    $r = New-Object AutoTestWin32b+RECT
    if (-not [AutoTestWin32b]::GetWindowRect($p.MainWindowHandle, [ref]$r) -or ($r.Right - $r.Left) -le 100) { return $null }
    $bmp = New-Object Drawing.Bitmap ($r.Right - $r.Left), ($r.Bottom - $r.Top)
    $gfx = [Drawing.Graphics]::FromImage($bmp)
    $hdc = $gfx.GetHdc()
    try { $ok = [AutoTestWin32b]::PrintWindow($p.MainWindowHandle, $hdc, 2) }
    finally { $gfx.ReleaseHdc($hdc); $gfx.Dispose() }
    if (-not $ok) { $bmp.Dispose(); return $null }
    return $bmp
}

# The game window kept behind the other windows while the game starts (Windows activates a new window, and again
# when the game applies its window settings at the title screen): sent to the bottom without activation, the focus
# given back to the window used before the run. Only during the first minutes, so a window the tester brings to the
# front later stays there. Screenshots use PrintWindow: they work behind other windows.
$script:UserWindow = [AutoTestWin32b]::GetForegroundWindow()
function Push-GameBehind {
    $p = Get-GameProcess
    if (-not $p -or $p.MainWindowHandle -eq [IntPtr]::Zero) { return }
    if ([AutoTestWin32b]::GetForegroundWindow() -ne $p.MainWindowHandle) { return }
    # HWND_BOTTOM, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE
    [AutoTestWin32b]::SetWindowPos($p.MainWindowHandle, [IntPtr]1, 0, 0, 0, 0, 0x13) | Out-Null
    if ($script:UserWindow -ne [IntPtr]::Zero -and $script:UserWindow -ne $p.MainWindowHandle) {
        [AutoTestWin32b]::SetForegroundWindow($script:UserWindow) | Out-Null
    }
}

# A recording request of the game (<n>.rec = "<frame folder>|<ms>"): frames of the game window (jpg, ~10 a second) and
# frames.txt (time of each frame in ms) for the duration; the request is deleted when done (the game waits for it).
$jpegCodec = [Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$jpegParams = New-Object Drawing.Imaging.EncoderParameters 1
$jpegParams.Param[0] = New-Object Drawing.Imaging.EncoderParameter ([Drawing.Imaging.Encoder]::Quality), 85L
function Invoke-Recording([IO.FileInfo]$Req) {
    $spec = (Get-Content -LiteralPath $Req.FullName -Raw).Trim() -split '\|'
    $dir = $spec[0]; $ms = [int]$spec[1]
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    # the game waits for it before it plays the effect: the recording has its whole start
    [IO.File]::WriteAllText((Join-Path $dir 'started'), '')
    $times = New-Object System.Collections.Generic.List[string]
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $n = 0
    while ($sw.ElapsedMilliseconds -lt $ms) {
        $t = $sw.ElapsedMilliseconds
        $bmp = Get-GameBitmap
        if ($bmp) {
            try { $bmp.Save((Join-Path $dir ('f{0:000}.jpg' -f $n)), $jpegCodec, $jpegParams) } finally { $bmp.Dispose() }
            $times.Add("$t"); $n++
        }
        $wait = 100 - ($sw.ElapsedMilliseconds - $t)
        if ($wait -gt 0) { Start-Sleep -Milliseconds $wait }
    }
    [IO.File]::WriteAllLines((Join-Path $dir 'frames.txt'), $times)
}

function Invoke-SnapRequests([string]$Dir) {
    if (-not (Test-Path -LiteralPath $Dir)) { return }
    foreach ($req in @(Get-ChildItem -LiteralPath $Dir -Filter *.rec -ErrorAction SilentlyContinue | Sort-Object Name)) {
        try { Invoke-Recording $req } catch { Write-Warn "Recording failed ($($req.Name)): $($_.Exception.Message)" }
        Remove-Item -LiteralPath $req.FullName -ErrorAction SilentlyContinue
    }
    foreach ($req in @(Get-ChildItem -LiteralPath $Dir -Filter *.req -ErrorAction SilentlyContinue | Sort-Object Name)) {
        try {
            $target = (Get-Content -LiteralPath $req.FullName -Raw).Trim()
            Save-GameScreenshot $target
        }
        catch { Write-Warn "Screenshot failed ($($req.Name)): $($_.Exception.Message)" }
        Remove-Item -LiteralPath $req.FullName -ErrorAction SilentlyContinue
    }
}
$paths = Get-GamePaths -GamePath $GamePath
$definitions = Read-ScenarioDefinitions $paths.Repo
# TP-01 (new character) only when needed: a full run (TZ-01 ends the character), -NewCharacter, or a selected
# scenario marked "newCharacter" in scenarios.json (e.g. TB-16 needs the 3 artifacts of the new character);
# otherwise the existing save (-Save <name>, else the first save of the list) is used. -Save / -ExistingSave /
# -ManualLoad always use an existing save.
$selectedIds = @($Scenario | Where-Object { $_ } | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$needNew = $NewCharacter -or $selectedIds.Count -eq 0 -or @($definitions | Where-Object { $_.newCharacter -and $selectedIds -contains $_.id }).Count -gt 0
# -SlotCharacter (games side by side): the game plays the test character of its slot ("AutoTest Slot<n>", created when missing)
$useNewCharacter = $needNew -and -not ($Save -or $ExistingSave -or $ManualLoad -or $SlotCharacter)
if ($needNew -and -not $useNewCharacter -and $selectedIds.Count -gt 0) {
    Write-Warning "Scenarios needing a new character run on an existing save: $(@($definitions | Where-Object { $_.newCharacter -and $selectedIds -contains $_.id } | ForEach-Object { $_.id }) -join ', ')"
}

# Tools (kind "tool": the effect galleries TE-xx, the investigations TI-xx) run for a long time: only once the user
# confirmed it - a prompt in an interactive terminal, -ConfirmTools otherwise (never just because the scenarios ran).
function Confirm-ToolRun([string[]]$Ids, [switch]$Confirmed) {
    if ($Ids.Count -eq 0 -or $Confirmed) { return $true }
    $interactive = [Environment]::UserInteractive -and -not [Console]::IsInputRedirected -and $Host.Name -eq 'ConsoleHost'
    if ($interactive) {
        $a = Read-Host "Run the tool scenario(s) $($Ids -join ', ') (long, records the game)? [y/N]"
        return $a -match '^(y|yes)$'
    }
    Write-Host "Tool scenario(s) $($Ids -join ', '): not run without the user's confirmation (add -ConfirmTools)." -ForegroundColor Yellow
    return $false
}

$toolIds = @($definitions | Where-Object { $_.kind -eq 'tool' -and @($Scenario | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim().ToUpper() }) -contains $_.id } | ForEach-Object { $_.id })
if (-not $ListScenarios -and -not $ReportFrom -and -not (Confirm-ToolRun $toolIds -Confirmed:$ConfirmTools)) { exit 0 }

if ($ListScenarios) {
    $st = Read-ScenarioStatus $paths.Repo
    $definitions | ForEach-Object { [pscustomobject]@{ Id = $_.id; Kind = $_.kind; LastResult = $st[$_.id].result; Feature = $_.feature; Title = $_.title } } | Format-Table -AutoSize -Wrap
    exit 0
}
$utf8 = New-Object Text.UTF8Encoding($false)
$exitCode = 0
if ($ReportFrom) {
    # regenerate report/status of an existing run without starting the game
    $outDir = (Resolve-Path -LiteralPath $ReportFrom).Path
    $runId = Split-Path $outDir -Leaf
    $resultsFile = Join-Path $outDir 'results.json'
    $request = @{ Scenarios = @() }
    $startTime = Get-Date
}
else {
    if (-not $paths.GamePath -or -not (Test-Path $paths.GamePath)) {
        Write-Err 'Game folder not found. Pass -GamePath <folder>.'
        exit 1
    }

    # ------------------------------------------------------------------ deploy
    if ($Attach -and -not (Get-GameProcess)) {
        Write-Err "-Attach: no game of slot $Slot is running"
        exit 4
    }
    if ($Deploy -and -not $Attach) {
        # ModLib first: Taoist is compiled against it. One deploy at a time; the copy waits until no game runs and is
        # skipped when the Workshop folder already has this build (_slots.ps1)
        Write-Title 'Rebuild and deploy'
        try { Invoke-SafeDeploy $paths.Repo $paths.Workshop @('3385996759', '3161035078') }
        catch { Write-Err "Build/deploy failed: $($_.Exception.Message)"; exit 1 }
    }

    # ------------------------------------------------------------------ request
    $runId = (Get-Date -Format 'yyyyMMdd-HHmmss') + $(if ($Slot -ge 2) { "-s$Slot" })
    $outDir = Join-Path $paths.Repo "logs\autotest\$runId"
    New-Item -ItemType Directory -Path (Join-Path $outDir 'screens') -Force | Out-Null
    $atDir = Join-Path $paths.ModLogs ("$TaoistModId\autotest" + $(if ($Slot -ge 2) { "-slot$Slot" }))   # AutoTestEvent.GetAutoTestFolder
    if ($Slot -ge 2 -and -not (Test-Path -LiteralPath $paths.GameExe)) {
        Write-Err "Game slot $Slot not found ($($paths.GamePath)): .\scripts\game\new-game-slot.ps1 -Slot $Slot"
        exit 1
    }
    New-Item -ItemType Directory -Path $atDir -Force | Out-Null
    $requestFile = Join-Path $atDir 'request.json'
    $stopFile = Join-Path $atDir 'stop.flag'
    $snapDir = Join-Path $atDir 'snaps'
    Remove-Item -LiteralPath $snapDir -Recurse -Force -ErrorAction SilentlyContinue
    $resultsFile = Join-Path $outDir 'results.json'
    Remove-Item -LiteralPath $stopFile -ErrorAction SilentlyContinue

    $request = [ordered]@{
        RunId        = $runId
        OutDir       = $outDir
        Save         = $Save
        # new character by default (pre-test TP-01); -Save / -ExistingSave / -ManualLoad use an existing save
        NewCharacter = $useNewCharacter
        SlotCharacter = [bool]$SlotCharacter
        Window       = $Window
        AutoLoad     = -not $ManualLoad
        Scenarios    = @($Scenario | Where-Object { $_ } | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
        StepDelayMs  = $StepDelayMs
        WatchBattles = [bool]$WatchBattles
        Promo        = [bool]$Promo
        KeepAlive    = [bool]($KeepAlive -or $Attach)
    }
    $utf8 = New-Object Text.UTF8Encoding($false)
    Write-Json2File $requestFile $request
    if (-not $useNewCharacter -and -not ($Save -or $ExistingSave -or $ManualLoad)) {
        Write-Info 'No selected scenario needs a new character ("newCharacter" in scenarios.json): TP-01 skipped, the existing save is used (-NewCharacter to force it).'
    }

    Write-Title 'Taoist autotest'
    Write-Info "Run:       $runId"
    Write-Info "Evidence:  $outDir"
    Write-Info "Request:   $requestFile"
    Write-Info ("Save:      " + $(if ($ManualLoad) { 'load it yourself in the game' } elseif ($Save) { $Save } elseif (-not $useNewCharacter) { '(first save in the list)' } else { 'new character created by the pre-test TP-01 (removed by the restore)' }))
    Write-Warn 'Use a throwaway copy of a save: scenarios run mod functions on it.'

    # ------------------------------------------------------------------ backup saves
    # game saves (<game data>\<platform>\CacheData) and the ModLib caches of every mod (mod\<id>\globals|saves)
    # -NoSaveBackup: the caller backs up / restores (parallel runs share the save folders)
    $saveDirs = if ($NoSaveBackup) { @() } else { @(
        @(Get-ChildItem -LiteralPath $paths.GameData -Directory -ErrorAction SilentlyContinue | ForEach-Object { Join-Path $_.FullName 'CacheData' })
        @(Get-ChildItem -LiteralPath $paths.ModLogs -Directory -ErrorAction SilentlyContinue | ForEach-Object { Join-Path $_.FullName 'globals'; Join-Path $_.FullName 'saves' })
    ) | Where-Object { Test-Path -LiteralPath $_ } }
    $backupDir = Join-Path $outDir 'save-backup'
    function Get-BackupName([string]$Dir) { return ($Dir.Substring($paths.GameData.Length).Trim('\') -replace '[\\/]', '_') }
    $backupCounts = @{}
    foreach ($sd in $saveDirs) {
        $dest = Join-Path $backupDir (Get-BackupName $sd)
        & robocopy $sd $dest /E /NFL /NDL /NJH /NJS /NP | Out-Null
        if ($LASTEXITCODE -ge 8) { Write-Err "Save backup failed ($sd)"; exit 1 }
        $backupCounts[$sd] = @(Get-ChildItem -LiteralPath $sd -Recurse -File -ErrorAction SilentlyContinue).Count
        Write-Info "Save backup: $sd -> $dest"
    }
    if ($saveDirs.Count -eq 0 -and -not $NoSaveBackup) { Write-Warn "No save folder found under $($paths.GameData)" }

    # ------------------------------------------------------------------ run the game
    $maxDuration = $Timeout + $(if ($WatchBattles) { $WatchMinutes * 60 } else { 0 })
    $runGameArgs = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', (Join-Path $PSScriptRoot 'run-game.ps1'),
        '-Duration', $maxDuration, '-Until', $(if ($Slot -ge 2) { "`"\[AUTOTEST\] finished \[slot $Slot\]`"" } else { '"\[AUTOTEST\] finished:"' }))
    if (Get-GameProcess) { $runGameArgs += '-Restart' }   # the request is read when the game starts
    if (-not $KeepGame) { $runGameArgs += '-StopGame' }
    if ($GamePath) { $runGameArgs += @('-GamePath', "`"$GamePath`"") }
    # windowed at -Window from the start (Unity registry keys + command line), as the parallel runs; the mod applies it
    # again at the title screen (request.Window)
    if (-not $GameArgs -and $Window) { $GameArgs = Set-GameWindow $repo $Window }
    if ($GameArgs) { $runGameArgs += @('-GameArgs', "`"$GameArgs`"") }

    # another run copying the mods to the Workshop folder: start the game after it
    if (-not $Attach) { try { Wait-DeployFree } catch { Write-Err $_.Exception.Message; exit 1 } }

    $startTime = Get-Date
    $exitCode = 0
    $status = $null
    $printed = @{}
    $watchStart = $null
    try {
        # -Attach: the game waiting in its save (request.KeepAlive) reads the new request.json by itself
        $runner = if ($Attach) { $null } else { Start-Process -FilePath 'powershell.exe' -ArgumentList $runGameArgs -NoNewWindow -PassThru }
        if ($Attach) { Write-Info "Attached to the running game of slot $Slot (no restart)" }
        $finishedAt = $null
        $tick = 0
        $lastUpdate = $null
        $lastUpdateAt = Get-Date
        while (($runner -and -not $runner.HasExited) -or ($Attach -and (Get-GameProcess))) {
            Start-Sleep -Milliseconds 250
            Invoke-SnapRequests $snapDir
            if (-not $Foreground -and ((Get-Date) - $startTime).TotalMinutes -lt 4) { Push-GameBehind }
            if ((++$tick % 4) -ne 0) { continue }
            if (-not (Test-Path -LiteralPath $resultsFile)) { continue }
            try { $res = Get-Content -LiteralPath $resultsFile -Raw -Encoding UTF8 | ConvertFrom-Json } catch { continue }
            if (-not $res) { continue }

            # progress output only: an error here (e.g. results.json read while the game writes it) must never stop
            # the script, or the saves are not restored
            try {
            foreach ($s in @($res.scenarios)) {
                if (-not $s -or -not $s.id) { continue }
                if ($s.status -ne 'RUNNING' -and -not $printed.ContainsKey($s.id)) {
                    $printed[$s.id] = $true
                    $color = switch ($s.status) { 'PASS' { 'Green' } 'SKIP' { 'DarkGray' } default { 'Red' } }
                    $label = switch ($s.status) { 'PASS' { 'OK' } 'SKIP' { 'SKIP' } default { 'NG' } }
                    Write-Host ("  [{0,-4}] {1,-8} {2}" -f $label, $s.id, $s.title) -ForegroundColor $color
                }
            }
            }
            catch { Write-Warn "progress output: $($_.Exception.Message)" }
            # watchdog: the runner writes results.json after every step; no write for StallSeconds = the game stopped
            # running the mod's ticks (e.g. an exception inside the game's TimerMgr stops every timer for good)
            if ("$($res.updatedAt)" -ne $lastUpdate) { $lastUpdate = "$($res.updatedAt)"; $lastUpdateAt = Get-Date }
            elseif ($res.status -eq 'RUNNING' -and $res.phase -ne 'Watch' -and $StallSeconds -gt 0 -and
                ((Get-Date) - $lastUpdateAt).TotalSeconds -ge $StallSeconds) {
                $cur = @($res.scenarios) | Where-Object { $_.status -eq 'RUNNING' } | Select-Object -Last 1
                $lastCheck = if ($cur) { @($cur.checks) | Select-Object -Last 1 } else { $null }
                $msg = "stalled: no progress for $StallSeconds s (last: $(if ($lastCheck) { $lastCheck.step } else { $res.phase })) - the game stopped running the mod (see Player.log)"
                Write-Err $msg
                try { Save-GameScreenshot (Join-Path $outDir 'screens\stalled.png') | Out-Null } catch { }
                $res.status = 'ERROR'
                if ($cur) { $cur.status = 'ERROR'; $cur.error = $msg; $cur.endedAt = (Get-Date).ToString('s') }
                [IO.File]::WriteAllText($resultsFile, ($res | ConvertTo-Json -Depth 30), $utf8)
                $status = 'ERROR'
                Stop-Game
                if ($runner -and -not $runner.WaitForExit(30000)) { Stop-Process -Id $runner.Id -Force -ErrorAction SilentlyContinue }
                break
            }
            if ($res.phase -eq 'Watch') {
                if (-not $watchStart) {
                    $watchStart = Get-Date
                    Write-Info "Automatic scenarios done. Play battles now - watching for $WatchMinutes min (create $stopFile to stop earlier)."
                }
                elseif (((Get-Date) - $watchStart).TotalMinutes -ge $WatchMinutes -and -not (Test-Path $stopFile)) {
                    [IO.File]::WriteAllText($stopFile, 'stop', $utf8)
                }
            }
            if ($res.status -ne 'RUNNING') {
                $status = $res.status
                if (-not $finishedAt) { $finishedAt = Get-Date; Write-Info "Runner finished: $status" }
                if ($Attach) { break }   # the game stays, waiting for the next request
                # run-game.ps1 stops on the "[AUTOTEST] finished" log line; make sure it does
                if (((Get-Date) - $finishedAt).TotalSeconds -gt 45) {
                    if (-not $KeepGame) { Stop-Game }
                    if (-not $runner.WaitForExit(30000)) { Stop-Process -Id $runner.Id -Force -ErrorAction SilentlyContinue }
                    break
                }
            }
            # -Attach: a whole run without progress (the game not reading the request, a hang) - Timeout seconds
            if ($Attach -and ((Get-Date) - $startTime).TotalSeconds -gt $Timeout) { Write-Err "attached run: no end within $Timeout s"; Stop-Game; break }
        }
        if ($Attach -and -not $status -and -not (Get-GameProcess)) { Write-Err 'the game of the slot closed during the attached run' }
    }
    finally {
        Remove-Item -LiteralPath $requestFile -ErrorAction SilentlyContinue
        Remove-Item -LiteralPath $stopFile -ErrorAction SilentlyContinue
    }
}

# ------------------------------------------------------------------ restore saves
if (-not $ReportFrom -and $saveDirs.Count -gt 0) {
    if ($NoRestore -or $KeepGame) {
        Write-Warn "Saves NOT restored$(if ($KeepGame) { ' (-KeepGame: game still running)' }). Backup: $backupDir"
    }
    else {
        if (Get-GameProcess) { Stop-Game }
        foreach ($sd in $saveDirs) {
            $src = Join-Path $backupDir (Get-BackupName $sd)
            # mirror only a complete backup: /MIR deletes what the backup lacks. A folder that was empty before the
            # run is mirrored too (files created by the run are removed); an incomplete backup is never mirrored.
            $srcFiles = @(Get-ChildItem -LiteralPath $src -Recurse -File -ErrorAction SilentlyContinue).Count
            $expected = if ($backupCounts -and $backupCounts.ContainsKey($sd)) { $backupCounts[$sd] } else { $srcFiles }
            if ($srcFiles -lt $expected) {
                Write-Warn "Not restored (backup has $srcFiles of $expected file(s)): $sd - copy it from $src by hand"
                continue
            }
            if (-not (Test-Path -LiteralPath $src)) { New-Item -ItemType Directory -Force -Path $src | Out-Null }
            & robocopy $src $sd /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
            if ($LASTEXITCODE -ge 8) { Write-Err "Save restore failed: copy $src back to $sd manually" }
            else { Write-Ok "Restored: $sd" }
        }
    }
}

# ------------------------------------------------------------------ collect
# an attached run has no game run of its own (the game was started by an earlier run of the slot)
$gameRun = if ($ReportFrom -or $Attach) { $null } else { Get-ChildItem -LiteralPath $paths.GameRuns -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.CreationTime -ge $startTime.AddSeconds(-5) } | Sort-Object CreationTime | Select-Object -Last 1 }
$gameSummary = $null
if ($gameRun) {
    Copy-Item -LiteralPath $gameRun.FullName -Destination (Join-Path $outDir 'game-run') -Recurse -Force
    $gs = Join-Path $gameRun.FullName 'summary.json'
    if (Test-Path $gs) { $gameSummary = Get-Content -LiteralPath $gs -Raw -Encoding UTF8 | ConvertFrom-Json }
}
elseif (Test-Path (Join-Path $outDir 'game-run\summary.json')) {
    $gameSummary = Get-Content -LiteralPath (Join-Path $outDir 'game-run\summary.json') -Raw -Encoding UTF8 | ConvertFrom-Json
}

$res = $null
if (Test-Path -LiteralPath $resultsFile) { $res = Get-Content -LiteralPath $resultsFile -Raw -Encoding UTF8 | ConvertFrom-Json }
$results = @(if ($res) { $res.scenarios })

# A scenario is NG when ModLib logged an error while it was running, even if its own checks passed
# (e.g. a UI built only half because a button could not be created).
$timelineFile = Join-Path $outDir 'game-run\timeline.log'
if ($res -and (Test-Path -LiteralPath $timelineFile)) {
    $errMark = [string][char]0x3010 + 'Error' + [char]0x3011
    # games side by side share the mod logs: only the log of this game's character (<mod>\<player id>_debug-<day>.log)
    $logTag = '\[mod-[^\]]+\]'
    if ($SlotCharacter -and "$($res.player)" -match '\(([^)]+)\)\s*$') { $logTag = '\[mod-[^\]]*-' + [regex]::Escape($Matches[1]) + '_debug[^\]]*\]' }
    $logErrors = @(Get-Content -LiteralPath $timelineFile -Encoding UTF8 | ForEach-Object {
        if ($_ -match ($logTag + ' \[(\d\d:\d\d:\d\d\.\d{3})\] (.*)$') -and $Matches[2].Contains($errMark)) {
            [pscustomobject]@{ Time = [TimeSpan]::Parse($Matches[1]); Text = $Matches[2].Replace($errMark, '').Trim() }
        }
    })
    $windows = @{}
    foreach ($l in @($res.log)) {
        if ($l -match '^(\d\d:\d\d:\d\d\.\d{3}) \[([^\]]+)\] (start|PASS|FAIL|ERROR|SKIP)') {
            $t = [TimeSpan]::Parse($Matches[1])
            if ($Matches[3] -eq 'start') { $windows[$Matches[2]] = @($t, $null) }
            elseif ($windows.ContainsKey($Matches[2])) { $windows[$Matches[2]][1] = $t }
        }
    }
    foreach ($s in $results) {
        if (-not $s.id -or -not $windows.ContainsKey($s.id) -or -not $windows[$s.id][1]) { continue }
        $from = $windows[$s.id][0]; $to = $windows[$s.id][1]
        $errs = @($logErrors | Where-Object { $_.Time -ge $from -and $_.Time -le $to })
        if ($errs.Count -eq 0) { continue }
        $s.checks = @($s.checks) + [pscustomobject]@{
            Id = "$(($s.id -split '#')[0])-LOG"; Step = 'game log'; Status = 'FAIL'; Time = $null
            Message = "$($errs.Count) error(s) logged during the scenario: " + (($errs | Select-Object -First 3 | ForEach-Object { $_.Text }) -join ' | ')
        }
        if ($s.status -in 'PASS', 'SKIP') { $s.status = 'FAIL' }
    }
}
$executedIds = @($results | ForEach-Object { ($_.id -split '#')[0] })
# effect galleries (TE-xx): their pictures to docs/Effects (in git)
if ($executedIds | Where-Object { $_ -like 'TE-*' }) {
    # a game slot (parallel galleries): in a background process, so the slot starts its next batch at once (the
    # collects of several runs lock index.json); otherwise here
    if ($Slot -ge 2) {
        Start-Process -FilePath 'powershell.exe' -WindowStyle Hidden -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File',
            "`"$(Join-Path $PSScriptRoot 'collect-effects.ps1')`"", '-Run', "`"$outDir`"") | Out-Null
        Write-Host "collect-effects: started in the background ($outDir)"
    }
    else {
        try { & (Join-Path $PSScriptRoot 'collect-effects.ps1') -Run $outDir } catch { Write-Host "collect-effects: $_" -ForegroundColor Yellow }
    }
}
# promotion pictures ("promo" of scenarios.json) to ModProject/SteamImg: only from a promotion run (run-promo.ps1),
# never from the test runs (their screenshots carry check panels and test states)
if ($Promo) {
    try { & (Join-Path $PSScriptRoot 'collect-promo.ps1') -Run $outDir } catch { Write-Host "collect-promo: $_" -ForegroundColor Yellow }
}
$notRun = @($definitions | Where-Object { $_.kind -ne 'manual' -and ($_.kind -ne 'watch' -or $WatchBattles) -and ($_.kind -ne 'tool' -or $request.Scenarios -contains $_.id) -and $executedIds -notcontains $_.id -and ($request.Scenarios.Count -eq 0 -or $request.Scenarios -contains $_.id) })
$manual = @($definitions | Where-Object { $_.kind -eq 'manual' })
$count = [ordered]@{
    pass   = @($results | Where-Object status -eq 'PASS').Count
    fail   = @($results | Where-Object status -eq 'FAIL').Count
    error  = @($results | Where-Object status -eq 'ERROR').Count
    skip   = @($results | Where-Object status -eq 'SKIP').Count
    notRun = $notRun.Count
    manual = $manual.Count
}

if (-not $res -or $res.status -eq 'RUNNING' -or $res.status -eq 'ERROR') { $exitCode = 2 }
elseif ($count.fail -gt 0 -or $count.error -gt 0) { $exitCode = 3 }

# ------------------------------------------------------------------ report.json (final results, read by the test viewer)
if ($res) {
    $report = [ordered]@{}
    foreach ($pr in $res.PSObject.Properties) { $report[$pr.Name] = $pr.Value }
    $report.scenarios = $results
    $report.counts = $count
    $report.notRun = @($notRun | ForEach-Object { $_.id })
    Write-Json2File (Join-Path $outDir 'report.json') $report 12
}

# ------------------------------------------------------------------ OK/NG status per id
# not for a promotion run (-Promo): its tools only take pictures, and the scenarios it does not run are not "NOT RUN"
# read-modify-write under a lock shared by the runs of every game slot (run-autotest-parallel.ps1)
$statusLock = New-Object System.Threading.Mutex($false, 'Global\ggbh-autotest-scenario-status')
try { [void]$statusLock.WaitOne(120000) } catch [System.Threading.AbandonedMutexException] { }
try {
$status = Read-ScenarioStatus $paths.Repo
$evidence = "test-viewer/index.html#tab=auto&run=$runId"
if (-not $Promo) {
foreach ($grp in ($results | Group-Object { ($_.id -split '#')[0] })) {
    if (-not $status.Contains($grp.Name)) { continue }
    $e = $status[$grp.Name]
    $sts = @($grp.Group | ForEach-Object { $_.status })
    $e.result = if ($sts | Where-Object { $_ -in 'FAIL', 'ERROR', 'RUNNING' }) { 'NG' } elseif ($sts -contains 'PASS') { 'OK' } else { 'SKIP' }
    $e.lastRun = (Get-Date).ToString('s')
    $e.evidence = $evidence
    $e.tester = 'autotest'
    $e.note = (@($grp.Group | Where-Object { $_.error } | ForEach-Object { ($_.error -split "`n")[0] }) | Select-Object -First 1)
    $e.checks = [ordered]@{}
    foreach ($c in @($grp.Group | ForEach-Object { $_.checks } | Where-Object { $_.Id })) {
        if ($e.checks[$c.Id] -ne 'NG') { $e.checks[$c.Id] = $(if ($c.Status -eq 'PASS') { 'OK' } else { 'NG' }) }
    }
}
foreach ($d in $notRun) {
    $e = $status[$d.id]
    $e.result = 'NOT RUN'; $e.lastRun = (Get-Date).ToString('s'); $e.evidence = $evidence; $e.tester = 'autotest'
}
Save-ScenarioStatus $paths.Repo $status
}
}
finally { try { $statusLock.ReleaseMutex() } catch { } ; $statusLock.Dispose() }
$ngIds = @($status.Values | Where-Object { $_.result -eq 'NG' } | ForEach-Object { $_.id })

$summary = [ordered]@{
    runId       = $runId
    status      = if ($res) { $res.status } else { $null }
    exitCode    = $exitCode
    counts      = $count
    notRun      = @($notRun | ForEach-Object { $_.id })
    ngIds       = $ngIds
    logErrors   = if ($gameSummary) { $gameSummary.errorCount } else { $null }
    screenshots = @(Get-ChildItem -LiteralPath (Join-Path $outDir 'screens') -Filter *.png -ErrorAction SilentlyContinue).Count
    viewer      = Join-Path $paths.Repo 'test-viewer\index.html'
}
Write-Json2File (Join-Path $outDir 'summary.json') $summary

Write-Title 'Autotest summary'
Write-Info "Runner status: $($summary.status)"
Write-Info ("OK {0}  NG {1} (FAIL {2}, ERROR {3})  SKIP {4}  NOT RUN {5}  MANUAL {6}" -f $count.pass, ($count.fail + $count.error), $count.fail, $count.error, $count.skip, $count.notRun, $count.manual)
if ($ngIds.Count -gt 0) { Write-Warn "NG ids (latest result of every id): $($ngIds -join ', ')" }
Write-Info "Status by id:  3161035078\tests\scenario-status.json"
Write-Info "Screenshots:   $($summary.screenshots)"
if ($summary.logErrors) { Write-Warn "Errors in game logs: $($summary.logErrors) (see game-run\timeline.log)" }
if ($exitCode -eq 2) { Write-Err 'The runner did not finish. Check results.json (log) and game-run\timeline.log; try -ManualLoad if the save could not be opened.' }
# a full run (every auto scenario, none failed or left) is pinned: never deleted by the retention of the viewer
if (-not $Scenario -and $exitCode -eq 0 -and $count.notRun -eq 0 -and $count.pass -gt 0) {
    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $paths.Repo 'scripts\pin-test-run.ps1') -Run $runId -Auto -Note 'full run, all passed'
}
# -NoViewerUpdate: the caller updates the viewer once (run-autotest-parallel.ps1)
if (-not $NoViewerUpdate) { & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $paths.Repo 'scripts\update-test-viewer.ps1') }
Write-Info "Results:       $($summary.viewer)  (run $runId, evidence in $outDir)"
exit $exitCode
