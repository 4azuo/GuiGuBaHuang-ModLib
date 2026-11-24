# Shared by the scripts that run several games side by side (run-autotest-parallel.ps1, run-effect-gallery.ps1) and by
# their workers (Start-Job: dot-source this file). Needs _game.ps1 (Get-GamePaths, Get-GameProcess, Stop-Game).
#
# - one orchestrator at a time (a lock with its PID);
# - the games windowed (kept after the run);
# - saves backed up once, the global settings of the mods restored after (the saves are kept: test characters);
# - a queue (JSON file under an exclusive lock) the workers take items from;
# - no game started without the memory for it (~4 GB a game, Wait-GameMemory): the slot waits, else stops;
# - a snapshot of the test character of every slot ("AutoTest Slot<n>") restored before every run: each run starts from
#   the same fresh character, like a single run does with a new character.

. (Join-Path $PSScriptRoot '_slots.ps1')   # slot / deploy locks shared with every other run of the machine
$script:ParallelUtf8 = New-Object Text.UTF8Encoding($false)
$script:GameRegPath = 'HKCU:\Software\guigugame\guigubahuang'

# ------------------------------------------------------------------ orchestrator lock

# Takes the lock of the orchestrators (logs\parallel.lock = PID): an other one running stops this one.
function Enter-ParallelLock([string]$Repo) {
    $lock = Join-Path $Repo 'logs\parallel.lock'
    if (Test-Path -LiteralPath $lock) {
        $other = (Get-Content -LiteralPath $lock -Raw).Trim()
        $p = Get-CimInstance Win32_Process -Filter "ProcessId=$([int]$other)" -ErrorAction SilentlyContinue
        if ($p -and $p.CommandLine -match 'run-autotest-parallel|run-effect-gallery') {
            throw "An other parallel run is running (pid ${other}: $($p.CommandLine)). Stop it first (Stop-ProcessTree $other)."
        }
    }
    New-Item -ItemType Directory -Path (Split-Path $lock) -Force | Out-Null
    [IO.File]::WriteAllText($lock, "$PID")
}

function Exit-ParallelLock([string]$Repo) {
    $lock = Join-Path $Repo 'logs\parallel.lock'
    if ((Test-Path -LiteralPath $lock) -and (Get-Content -LiteralPath $lock -Raw).Trim() -eq "$PID") { Remove-Item -LiteralPath $lock -Force }
}

# A process and everything it started (jobs, run-autotest, run-game, the games).
function Stop-ProcessTree([int]$ProcessId) {
    & taskkill /F /T /PID $ProcessId 2>&1 | Out-Null
}

# The processes started by this script (its Start-Job hosts and what they started), then the games of the slots.
function Stop-ParallelChildren([int[]]$SlotIds) {
    Get-CimInstance Win32_Process -Filter "ParentProcessId=$PID" -ErrorAction SilentlyContinue |
        ForEach-Object { Stop-ProcessTree $_.ProcessId }
    foreach ($s in $SlotIds) {
        # never the game of a slot used by another run (other terminal / session)
        if (Test-SlotLockedByOther $s) { continue }
        $env:GGBH_SLOT = if ($s -ge 2) { "$s" } else { $null }
        $g = Get-GameProcess
        if ($g) { Stop-Process -Id $g.Id -Force -ErrorAction SilentlyContinue }
    }
    $env:GGBH_SLOT = $null
}

# ------------------------------------------------------------------ window of the games

# The games windowed at $Window ("1600x900"; kept after the run): Unity's registry keys and the game's own ones
# (GameSetting_*), Unity's command line (returned, "" when no size), and the mod sets the game's settings at the title
# screen (run-autotest.ps1 -Window: the game applies its own settings at start, over the command line).
function Set-GameWindow([string]$Repo, [string]$Window) {
    if ($Window -notmatch '^(\d+)x(\d+)$') { return '' }
    $w = [int]$Matches[1]; $h = [int]$Matches[2]
    $wanted = [ordered]@{
        '^GameSetting_windowModel_h' = 1; '^GameSetting_screenWidth_h' = $w; '^GameSetting_screenHeight_h' = $h
        '^Screenmanager Fullscreen mode_h' = 3; '^Screenmanager Resolution Width_h' = $w; '^Screenmanager Resolution Height_h' = $h
    }
    if (Test-Path $script:GameRegPath) {
        $names = @((Get-ItemProperty $script:GameRegPath).PSObject.Properties | ForEach-Object { $_.Name })
        foreach ($k in $wanted.Keys) {
            foreach ($name in @($names | Where-Object { $_ -match $k })) { Set-ItemProperty -Path $script:GameRegPath -Name $name -Value $wanted[$k] -Type DWord }
        }
    }
    return "-screen-fullscreen 0 -screen-width $w -screen-height $h"
}

# ------------------------------------------------------------------ saves

# Game saves (<game data>\<platform>\CacheData) and the ModLib caches of every mod (mod\<id>\globals|saves) copied to
# $Dest. Returns @{ <folder> = <copy> }.
function Backup-Saves($Paths, [string]$Dest) {
    $dirs = @(
        @(Get-ChildItem -LiteralPath $Paths.GameData -Directory -ErrorAction SilentlyContinue | ForEach-Object { Join-Path $_.FullName 'CacheData' })
        @(Get-ChildItem -LiteralPath $Paths.ModLogs -Directory -ErrorAction SilentlyContinue | ForEach-Object { Join-Path $_.FullName 'globals'; Join-Path $_.FullName 'saves' })
    ) | Where-Object { Test-Path -LiteralPath $_ }
    $rs = @{}
    foreach ($sd in $dirs) {
        $copy = Join-Path $Dest ($sd.Substring($Paths.GameData.Length).Trim('\') -replace '[\\/]', '_')
        & robocopy $sd $copy /E /NFL /NDL /NJH /NJS /NP | Out-Null
        if ($LASTEXITCODE -ge 8) { throw "Save backup failed ($sd)" }
        $rs[$sd] = $copy
    }
    return $rs
}

# The global settings of the mods (mod\<id>\globals) back from a Backup-Saves copy (the saves stay).
function Restore-ModGlobals($Backups) {
    foreach ($sd in @($Backups.Keys | Where-Object { (Split-Path $_ -Leaf) -eq 'globals' })) {
        & robocopy $Backups[$sd] $sd /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
    }
}

# ------------------------------------------------------------------ test character snapshots

function Get-SnapshotDir($Paths, [int]$Slot) { return (Join-Path $Paths.ModLogs "$($Paths.Mod.Id)\autotest-shared\snapshots\slot$Slot") }

# The save of the slot's character (game save folder + mod\<id>\saves\<player id> of every mod) copied to its snapshot.
function Save-SlotSnapshot($Paths, [int]$Slot, [string]$Folder, [string]$PlayerId) {
    $snap = Get-SnapshotDir $Paths $Slot
    Remove-Item -LiteralPath $snap -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $snap -Force | Out-Null
    foreach ($cache in Get-ChildItem -LiteralPath $Paths.GameData -Directory | ForEach-Object { Join-Path $_.FullName "CacheData\$Folder" } | Where-Object { Test-Path -LiteralPath $_ }) {
        # <game data>\<platform>\CacheData\<folder> -> <snapshot>\game\<platform>
        $platform = Split-Path -Leaf (Split-Path (Split-Path $cache))
        & robocopy $cache (Join-Path $snap "game\$platform") /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
    }
    foreach ($mod in Get-ChildItem -LiteralPath $Paths.ModLogs -Directory) {
        $src = Join-Path $mod.FullName "saves\$PlayerId"
        if (Test-Path -LiteralPath $src) { & robocopy $src (Join-Path $snap "mod\$($mod.Name)") /MIR /NFL /NDL /NJH /NJS /NP | Out-Null }
    }
    [IO.File]::WriteAllText((Join-Path $snap 'info.json'), (ConvertTo-Json @{ slot = $Slot; folder = $Folder; playerId = $PlayerId; at = (Get-Date).ToString('s') }), $script:ParallelUtf8)
}

# The slot's character back to its snapshot (the slot's game closed). False when there is no snapshot.
function Restore-SlotSnapshot($Paths, [int]$Slot) {
    $snap = Get-SnapshotDir $Paths $Slot
    $infoFile = Join-Path $snap 'info.json'
    if (-not (Test-Path -LiteralPath $infoFile)) { return $false }
    $info = Get-Content -LiteralPath $infoFile -Raw | ConvertFrom-Json
    foreach ($platform in Get-ChildItem -LiteralPath (Join-Path $snap 'game') -Directory -ErrorAction SilentlyContinue) {
        & robocopy $platform.FullName (Join-Path $Paths.GameData "$($platform.Name)\CacheData\$($info.folder)") /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
    }
    foreach ($mod in Get-ChildItem -LiteralPath (Join-Path $snap 'mod') -Directory -ErrorAction SilentlyContinue) {
        & robocopy $mod.FullName (Join-Path $Paths.ModLogs "$($mod.Name)\saves\$($info.playerId)") /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
    }
    return $true
}

function Get-SlotSnapshotInfo($Paths, [int]$Slot) {
    $infoFile = Join-Path (Get-SnapshotDir $Paths $Slot) 'info.json'
    if (Test-Path -LiteralPath $infoFile) { return (Get-Content -LiteralPath $infoFile -Raw | ConvertFrom-Json) }
    return $null
}

# The "slot character: slot=N folder=F id=I created=B" line of a run (AutoTestEvent), $null when none.
function Get-SlotCharacterOfRun([string]$Repo, [string]$RunId) {
    $file = Join-Path $Repo "logs\autotest\$RunId\results.json"
    if (-not $RunId -or -not (Test-Path -LiteralPath $file)) { return $null }
    $res = Get-Content -LiteralPath $file -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($l in @($res.log)) {
        if ($l -match 'slot character: slot=(\d+) folder=(\S+) id=(\S+) created=(\w+)') {
            return [pscustomobject]@{ Slot = [int]$Matches[1]; Folder = $Matches[2]; PlayerId = $Matches[3]; Created = $Matches[4] -eq 'True' }
        }
    }
    return $null
}

# ------------------------------------------------------------------ memory

# ~4 GB a game in a battle (4 games + other apps on 32 GB: the run was stopped for low memory).
$script:GameMemoryBytes = 4GB

# Memory left for one more game: the available memory less what the games of the machine may still take (each one grows
# to ~4 GB; a game just started is still small) and the games reserved by Wait-GameMemory that do not run yet.
function Get-GameMemoryRoom {
    $avail = [double](Get-CimInstance Win32_OperatingSystem).FreePhysicalMemory * 1KB
    $still = 0.0
    foreach ($p in @(Get-Process -Name 'guigubahuang' -ErrorAction SilentlyContinue)) {
        $used = [double][Math]::Max($p.WorkingSet64, $p.PrivateMemorySize64)
        $still += [Math]::Max([double]0, [double]$script:GameMemoryBytes - $used)
    }
    foreach ($f in @(Get-ChildItem -LiteralPath (Get-GameLockDir) -Filter 'memory-slot*.txt' -ErrorAction SilentlyContinue)) {
        # a reservation ends when the game of its slot runs (counted above) or after 5 minutes
        $s = [int]($f.BaseName -replace '\D', '')
        if ((Get-SlotGameProcess $s) -or $f.LastWriteTime -lt (Get-Date).AddMinutes(-5)) {
            Remove-Item -LiteralPath $f.FullName -Force -ErrorAction SilentlyContinue; continue
        }
        $still += $script:GameMemoryBytes
    }
    return ($avail - $still)
}

# Before a game of $Slot is started: waits (checked every 15 s) until there is room for one more game, then reserves it
# (so that two slots never take the same free memory). False - no game may be started - when $Cancel returns true (e.g.
# the queue is empty) or after $TimeoutMinutes. $Log gets the lines to log.
function Wait-GameMemory([int]$Slot, [scriptblock]$Log, [scriptblock]$Cancel, [int]$TimeoutMinutes = 30) {
    $mutex = New-Object Threading.Mutex($false, 'Global\ggbh-game-memory')
    $deadline = (Get-Date).AddMinutes($TimeoutMinutes)
    $waiting = $false
    try {
        while ($true) {
            try { [void]$mutex.WaitOne() } catch [Threading.AbandonedMutexException] { }
            try {
                $room = Get-GameMemoryRoom
                if ($room -ge $script:GameMemoryBytes) {
                    [IO.File]::WriteAllText((Join-Path (Get-GameLockDir) "memory-slot$Slot.txt"), "$PID")
                    if ($waiting -and $Log) { & $Log ("memory free again ({0:N1} GB): the game starts" -f ($room / 1GB)) }
                    return $true
                }
            }
            finally { $mutex.ReleaseMutex() }
            if (-not $waiting -and $Log) {
                & $Log ("not enough memory for one more game ({0:N1} GB left, a game takes {1} GB): waiting" -f ([Math]::Max([double]0, $room) / 1GB), ($script:GameMemoryBytes / 1GB))
                $waiting = $true
            }
            if ($Cancel -and (& $Cancel)) { return $false }
            if ((Get-Date) -gt $deadline) {
                if ($Log) { & $Log "!!! still not enough memory after $TimeoutMinutes min: no game started" }
                return $false
            }
            Start-Sleep -Seconds 15
        }
    }
    finally { $mutex.Dispose() }
}

# The reservation of Wait-GameMemory dropped (no game started after all).
function Clear-GameMemoryReservation([int]$Slot) {
    Remove-Item -LiteralPath (Join-Path (Get-GameLockDir) "memory-slot$Slot.txt") -Force -ErrorAction SilentlyContinue
}

# ------------------------------------------------------------------ queue

function Write-Queue([string]$File, $Items) {
    [IO.File]::WriteAllText($File, (ConvertTo-Json @($Items) -Depth 5), $script:ParallelUtf8)
}

# The queue file under an exclusive lock: $Action gets the list (List[object]) and returns what it takes.
function Use-Queue([string]$File, [scriptblock]$Action) {
    while ($true) {
        try { $fs = [IO.File]::Open($File, 'Open', 'ReadWrite', 'None'); break } catch { Start-Sleep -Milliseconds 200 }
    }
    try {
        $sr = New-Object IO.StreamReader($fs, $script:ParallelUtf8, $true, 4096, $true)
        $text = $sr.ReadToEnd(); $sr.Dispose()
        $list = New-Object System.Collections.Generic.List[object]
        # PowerShell 5.1: ConvertFrom-Json gives a JSON array as one object; a variable enumerates it
        $parsed = ConvertFrom-Json $text
        foreach ($x in $parsed) { if ($x) { $list.Add($x) } }
        $r = & $Action $list
        $fs.SetLength(0)
        $bytes = $script:ParallelUtf8.GetBytes((ConvertTo-Json @($list.ToArray()) -Depth 5))
        $fs.Write($bytes, 0, $bytes.Length)
        return $r
    }
    finally { $fs.Dispose() }
}

# One line in the shared log of the orchestrator (several workers append).
function Write-ParallelLog([string]$File, [string]$Text) {
    $line = "$(Get-Date -Format 'HH:mm:ss') $Text"
    Write-Host $line
    for ($k = 0; $k -lt 20; $k++) { try { [IO.File]::AppendAllText($File, $line + "`r`n", $script:ParallelUtf8); break } catch { Start-Sleep -Milliseconds 100 } }
}
