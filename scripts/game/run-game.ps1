<#
.SYNOPSIS
    Launch the game (or attach to it), follow its logs live and save them per run.
.DESCRIPTION
    Watches, from the moment the game starts:
      player  %USERPROFILE%\AppData\LocalLow\guigugame\guigubahuang\Player.log   (Unity, game mod loader)
      melon   <game>\MelonLoader\Latest.log                                      (MelonLoader, code mods)
      mod-*   <game-data>\mod\<modId>\*.log                                      (ModLib DebugHelper logs, incl. per save game)
      extra   any file given with -ExtraLog                                      (appended lines only)

    Each run is saved to <repo>\logs\game-runs\<yyyyMMdd-HHmmss>\:
      timeline.log      every line from every log, time-stamped and tagged with its source
      <source>.log      the new content of each log file
      summary.json      MelonLoader started?, assemblies regenerated?, mods loaded, errors...

    The console shows the important lines only (errors, MelonLoader, ModLib logs,
    mod loading); Unity stack-frame noise is hidden unless -ShowAll.

    Stops when: the game exits (a self-restart, e.g. to load MelonLoader, is followed),
    -Duration elapses, a line matches -Until, -UntilModLoaded fires, or Ctrl+C.

    Exit codes: 0 = game ran, 1 = setup error, 2 = game did not start.
.EXAMPLE
    .\scripts\game\run-game.ps1
    # launch through Steam, follow logs until the game is closed
.EXAMPLE
    .\scripts\game\run-game.ps1 -UntilModLoaded -StopGame -Duration 600
    # automated check: start, wait until code mods are loaded, close the game
.EXAMPLE
    .\scripts\game\run-game.ps1 -Attach
    # game already running: only follow new log lines
#>

[CmdletBinding()]
param(
    [int]$Duration = 0,
    [int]$StartTimeout = 180,
    [string]$Until,
    [switch]$UntilModLoaded,
    [switch]$Attach,
    [switch]$Restart,
    [switch]$Direct,
    [switch]$Steam,
    [switch]$StopGame,
    [switch]$ShowAll,
    [string[]]$ExtraLog,
    [string]$GamePath,
    # arguments of the exe (e.g. Unity's "-screen-fullscreen 0 -screen-width 1600 -screen-height 900"); started directly only
    [string]$GameArgs,
    [int]$RestartGraceSec = 30,
    [int]$PollMs = 300
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_game.ps1')

$paths = Get-GamePaths -GamePath $GamePath
if (-not $paths.GamePath -or -not (Test-Path $paths.GamePath)) {
    Write-Err 'Game folder not found. Pass -GamePath <folder>.'
    exit 1
}

# ------------------------------------------------------------------ patterns
# "<load mod (Chinese)>:<path>" = game loaded a mod's config (ModExportData.cache)
$modConfPattern = (-join [char[]](0x52A0, 0x8F7D, 0x6A21, 0x7EC4, 0xFF1A)) + '(.+)'
# Unity stack frames ("ModMgr:LoadMod(String)") and "(Filename: ...)" lines
$framePattern = '^\s*(\(Filename:|[\w\.<>`$\[\],]+:[\w<>`$\.\[\],]+\(.*\)\s*$)'
$errorPattern = '(?i)exception\b|\[error\]|\berror\b|\bfailed\b'

# ------------------------------------------------------------------ session
$session = Join-Path $paths.GameRuns ((Get-Date -Format 'yyyyMMdd-HHmmss') + $(if ((Get-GameSlot) -ge 2) { "-s$(Get-GameSlot)" }))
New-Item -ItemType Directory -Path $session -Force | Out-Null
$utf8 = New-Object Text.UTF8Encoding($false)
$writers = @{}
$timeline = New-Object IO.StreamWriter((Join-Path $session 'timeline.log'), $false, $utf8)
$timeline.AutoFlush = $true

$summary = [ordered]@{
    session               = $session
    startedAt             = (Get-Date).ToString('s')
    gameStartedAt         = $null
    endedAt               = $null
    stopReason            = $null
    gamePath              = $paths.GamePath
    attach                = [bool]$Attach
    melonProxyPresent     = $false
    melonLoaderStarted    = $false
    melonLoaderVersion    = $null
    assembliesRegenerated = $false
    modLoadComplete       = $false
    gameRestarts          = 0
    modsConfLoaded        = @()
    errorCount            = 0
    errors                = @()
    logFiles              = @()
}
$script:untilHit = $null

# ------------------------------------------------------------------ log watching
$specs = @(
    @{ Source = 'player'; Pattern = $paths.PlayerLog; Mode = 'overwrite' }
    @{ Source = 'melon'; Pattern = $paths.MelonLog; Mode = 'overwrite' }
    @{ Source = 'mod'; Pattern = (Join-Path $paths.ModLogs '*\*.log'); Mode = 'append' }
)
foreach ($x in $ExtraLog) { $specs += @{ Source = 'extra'; Pattern = $x; Mode = 'append' } }

$watched = @{}

function Get-SpecFiles($spec) {
    if ([Management.Automation.WildcardPattern]::ContainsWildcardCharacters($spec.Pattern)) {
        return @(Get-ChildItem -Path $spec.Pattern -File -ErrorAction SilentlyContinue)
    }
    # the file can disappear between Test-Path and Get-Item (MelonLoader rotates Latest.log at startup): no error then
    if (Test-Path -LiteralPath $spec.Pattern) { return @(Get-Item -LiteralPath $spec.Pattern -ErrorAction SilentlyContinue) }
    return @()
}

function Get-OutName($spec, $file) {
    switch ($spec.Source) {
        'player' { 'player' }
        'melon'  { 'melonloader' }
        'mod'    { 'mod-' + $file.Directory.Name + '-' + $file.BaseName }
        default  { 'extra-' + $file.BaseName }
    }
}

# Pre-existing files: "append" logs are read from their current end,
# "overwrite" logs (rewritten each game start) wait until written after launch.
function Add-Watch($spec, $file, [bool]$PreExisting) {
    $active = (-not $PreExisting) -or $spec.Mode -eq 'append' -or $Attach
    $offset = if ($PreExisting -and ($spec.Mode -eq 'append' -or $Attach)) { $file.Length } else { 0 }
    $watched[$file.FullName] = @{
        Path    = $file.FullName
        Source  = $spec.Source
        Mode    = $spec.Mode
        Out     = Get-OutName $spec $file
        Offset  = [long]$offset
        Active  = $active
        Decoder = [Text.Encoding]::UTF8.GetDecoder()
        Pending = ''
    }
}

function Write-LogLine($st, [string]$line) {
    $line = $line.TrimStart([char]0xFEFF)
    if (-not $writers.ContainsKey($st.Out)) {
        $w = New-Object IO.StreamWriter((Join-Path $session "$($st.Out).log"), $true, $utf8)
        $w.AutoFlush = $true
        $writers[$st.Out] = $w
        $summary.logFiles += $st.Path
    }
    $writers[$st.Out].WriteLine($line)
    $stamped = '{0} [{1}] {2}' -f (Get-Date).ToString('HH:mm:ss.fff'), $st.Out, $line
    $timeline.WriteLine($stamped)

    if ([string]::IsNullOrWhiteSpace($line)) { return }
    $isFrame = $line -match $framePattern
    $isError = (-not $isFrame) -and ($line -match $errorPattern)

    # ---- analysis
    if ($st.Source -eq 'melon' -and $line -match 'MelonLoader v(\d[\w\.\-]*)') {
        $summary.melonLoaderStarted = $true
        $summary.melonLoaderVersion = $Matches[1]
    }
    if ($line -match $modConfPattern) {
        $mod = $Matches[1].Trim()
        if ($summary.modsConfLoaded -notcontains $mod) { $summary.modsConfLoaded += $mod }
    }
    if ($isError) {
        $summary.errorCount++
        $text = "[$($st.Out)] " + $line.Trim()
        if ($text.Length -gt 300) { $text = $text.Substring(0, 300) + '...' }
        if ($summary.errors.Count -lt 50 -and $summary.errors -notcontains $text) { $summary.errors += $text }
    }
    if ($Until -and -not $script:untilHit -and $line -match $Until) { $script:untilHit = $line }

    # ---- console
    $show = $ShowAll -or ((-not $isFrame) -and ($st.Source -ne 'player' -or $isError -or $line -match $modConfPattern -or $line -match '(?i)melon'))
    if ($show) {
        $color = if ($isError) { 'Red' } elseif ($st.Source -eq 'melon') { 'Magenta' } elseif ($st.Source -eq 'player') { 'Gray' } else { 'Cyan' }
        Write-Host ("[{0}] {1}" -f $st.Out, $line) -ForegroundColor $color
    }
}

function Read-Watch($st) {
    if (-not $st.Active) {
        $item = Get-Item -LiteralPath $st.Path -ErrorAction SilentlyContinue
        if (-not $item -or $item.LastWriteTime -le $script:launchTime) { return }
        $st.Active = $true
        $st.Offset = 0
    }
    try { $fs = [IO.File]::Open($st.Path, 'Open', 'Read', 'ReadWrite, Delete') } catch { return }
    try {
        $len = $fs.Length
        if ($len -lt $st.Offset) {
            # file was recreated/truncated: read it again from the start
            $st.Offset = 0
            $st.Pending = ''
            $st.Decoder = [Text.Encoding]::UTF8.GetDecoder()
            Write-LogLine $st '--- (log file was rewritten) ---'
        }
        if ($len -eq $st.Offset) { return }
        $fs.Position = $st.Offset
        $count = [int][Math]::Min($len - $st.Offset, 16MB)
        $buf = New-Object byte[] $count
        $read = 0
        while ($read -lt $count) {
            $n = $fs.Read($buf, $read, $count - $read)
            if ($n -le 0) { break }
            $read += $n
        }
        $st.Offset += $read
        $chars = New-Object char[] ($st.Decoder.GetCharCount($buf, 0, $read))
        $null = $st.Decoder.GetChars($buf, 0, $read, $chars, 0)
        $text = $st.Pending + [string]::new($chars)
    }
    finally { $fs.Dispose() }

    $parts = $text -split "`r?`n"
    $st.Pending = $parts[-1]
    for ($i = 0; $i -lt $parts.Count - 1; $i++) { Write-LogLine $st $parts[$i] }
}

function Update-Watches {
    foreach ($spec in $specs) {
        foreach ($f in (Get-SpecFiles $spec)) {
            if (-not $watched.ContainsKey($f.FullName)) { Add-Watch $spec $f $false }
        }
    }
    foreach ($st in @($watched.Values)) { Read-Watch $st }
}

function Complete-Watches {
    Update-Watches
    foreach ($st in $watched.Values) {
        if ($st.Pending) { Write-LogLine $st $st.Pending; $st.Pending = '' }
    }
}

# ------------------------------------------------------------------ start
Write-Title 'Game run'
Write-Info "Game:    $($paths.GamePath)"
Write-Info "Session: $session"

$summary.melonProxyPresent = Test-Path $paths.MelonProxy
if (-not $summary.melonProxyPresent) {
    Write-Warn 'version.dll (MelonLoader proxy) is missing in the game folder: MelonLoader and code mods (ModLib, Taoist) will NOT load.'
    Write-Warn 'Restore it: Steam > game > Properties > Installed Files > Verify integrity of game files.'
}

$proc = Get-GameProcess
if ($proc -and -not $Attach) {
    if ($Restart) { Stop-Game }
    else {
        Write-Err 'The game is already running. Use -Attach to watch it, or -Restart to restart it.'
        exit 1
    }
}

$managedDll = Join-Path $paths.MelonDir 'Managed\Assembly-CSharp.dll'
$script:launchTime = Get-Date
foreach ($spec in $specs) { foreach ($f in (Get-SpecFiles $spec)) { Add-Watch $spec $f $true } }

$exitCode = 0
try {
    if (-not $Attach) {
        # MelonLoader's proxy stops the game when its path is not ASCII (Steam installs
        # to a Chinese folder name), so start the exe through an ASCII junction instead.
        $launchDir = $null
        if (-not $Steam -and $summary.melonProxyPresent -and $paths.GamePath -match '[^\x00-\x7F]') {
            $launchDir = Get-AsciiGamePath -GamePath $paths.GamePath -Repo $paths.Repo
            if (-not $launchDir) { Write-Warn 'No ASCII junction available - MelonLoader will probably refuse to start' }
        }
        elseif ($Direct -or (Get-GameSlot) -ge 2) { $launchDir = $paths.GamePath }   # a slot copy: its own exe

        if ($launchDir) {
            # Started outside Steam, the game cannot init the Steam API (no Workshop mods, no saves in
            # Steam\CacheData) unless steam_appid.txt is next to the exe (Steam itself must be running).
            $appIdFile = Join-Path $paths.GamePath 'steam_appid.txt'
            if (-not (Test-Path -LiteralPath $appIdFile)) {
                [IO.File]::WriteAllText($appIdFile, $GameAppId)
                Write-Info "Created $appIdFile ($GameAppId) so the Steam API works when the exe is started directly"
            }
            if (-not (Get-Process -Name steam -ErrorAction SilentlyContinue)) { Write-Warn 'Steam is not running: Workshop mods will not be loaded' }
            $exe = Join-Path $launchDir 'guigubahuang.exe'
            Write-Info "Starting $exe"
            $launched = if ($GameArgs) { Start-Process -FilePath $exe -WorkingDirectory $launchDir -ArgumentList $GameArgs -PassThru }
                        else { Start-Process -FilePath $exe -WorkingDirectory $launchDir -PassThru }
        }
        else {
            Write-Info "Starting through Steam (steam://rungameid/$GameAppId)"
            Start-Process "steam://rungameid/$GameAppId"
        }
    }

    Write-Info 'Waiting for the game process...'
    $deadline = (Get-Date).AddSeconds($StartTimeout)
    while (-not ($proc = Get-GameProcess)) {
        Update-Watches
        if ($launched -and $launched.HasExited -and ((Get-Date) - $script:launchTime).TotalSeconds -gt 5) {
            $hint = if ($summary.melonProxyPresent -and $launchDir -match '[^\x00-\x7F]') { ' - MelonLoader refuses non-ASCII game paths' } else { '' }
            throw [TimeoutException]"Game exited immediately (exit code $($launched.ExitCode))$hint"
        }
        if ((Get-Date) -gt $deadline) { throw [TimeoutException]"Game did not start within $StartTimeout s" }
        Start-Sleep -Milliseconds $PollMs
    }
    $gameStart = Get-Date
    $summary.gameStartedAt = $gameStart.ToString('s')
    Write-Ok "Game running (pid $($proc.Id))"

    while (-not $summary.stopReason) {
        Update-Watches

        if ($script:untilHit) {
            $summary.stopReason = "until: $($script:untilHit.Trim())"
        }
        elseif ($UntilModLoaded -and (Test-Path $paths.ModLoadComplete) -and (Get-Item $paths.ModLoadComplete).LastWriteTime -gt $script:launchTime) {
            $summary.stopReason = 'mod-load-complete'
        }
        elseif ($Duration -gt 0 -and ((Get-Date) - $gameStart).TotalSeconds -ge $Duration) {
            $summary.stopReason = "duration ($Duration s)"
        }
        elseif ($proc.HasExited) {
            # the game restarts itself sometimes (e.g. to load MelonLoader) - follow the new process
            Write-Info "Game process exited - waiting up to $RestartGraceSec s for a restart..."
            $graceEnd = (Get-Date).AddSeconds($RestartGraceSec)
            $next = $null
            while (-not $next -and (Get-Date) -lt $graceEnd) {
                Update-Watches
                Start-Sleep -Milliseconds $PollMs
                $next = Get-GameProcess
            }
            if ($next) {
                $summary.gameRestarts++
                $proc = $next
                Write-Ok "Game restarted (pid $($proc.Id))"
            }
            else { $summary.stopReason = 'game-exited' }
        }
        if (-not $summary.stopReason) { Start-Sleep -Milliseconds $PollMs }
    }
}
catch [TimeoutException] {
    Write-Err $_.Exception.Message
    $summary.stopReason = 'game-not-started'
    $exitCode = 2
}
finally {
    if (-not $summary.stopReason) { $summary.stopReason = 'interrupted' }
    if ($StopGame -and $summary.stopReason -ne 'game-exited') { Stop-Game }
    Complete-Watches

    $summary.endedAt = (Get-Date).ToString('s')
    $summary.modLoadComplete = (Test-Path $paths.ModLoadComplete) -and ((Get-Item $paths.ModLoadComplete).LastWriteTime -gt $script:launchTime)
    $summary.assembliesRegenerated = (Test-Path $managedDll) -and ((Get-Item $managedDll).LastWriteTime -gt $script:launchTime)

    $json = $summary | ConvertTo-Json -Depth 5
    [IO.File]::WriteAllText((Join-Path $session 'summary.json'), $json, $utf8)
    foreach ($w in $writers.Values) { $w.Dispose() }
    $timeline.Dispose()

    Write-Title 'Summary'
    Write-Info "Stop reason:            $($summary.stopReason)"
    Write-Info "MelonLoader proxy:      $($summary.melonProxyPresent)"
    Write-Info "MelonLoader started:    $($summary.melonLoaderStarted) $($summary.melonLoaderVersion)"
    Write-Info "Assemblies regenerated: $($summary.assembliesRegenerated)"
    Write-Info "Mod load complete:      $($summary.modLoadComplete)"
    Write-Info "Game restarts:          $($summary.gameRestarts)"
    Write-Info "Mod configs loaded:     $($summary.modsConfLoaded.Count)"
    if ($summary.errorCount -gt 0) {
        Write-Warn "Errors: $($summary.errorCount) (first ones below, all in timeline.log)"
        $summary.errors | Select-Object -First 10 | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
    }
    else { Write-Ok 'No errors in logs' }
    Write-Info "Logs: $session"
}
exit $exitCode
