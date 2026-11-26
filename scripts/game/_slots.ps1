<#
.SYNOPSIS
    Game slots and deploy shared by every autotest run of the machine (dot-source; no dependency on GGBH_SLOT).
.DESCRIPTION
    Keep ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI).

    Several runs (other terminals, other AI sessions, run-autotest-parallel.ps1) may start at the same time:
    - slot lock: a run holds <locks>\slot<N>.lock open (exclusive write) for its whole life; another run never uses,
      restarts or closes the game of a slot locked by someone else. The handle is closed by Windows when the process
      ends, so a crashed run never leaves a stale lock.
    - deploy lock: build + copy to the Steam Workshop folder one run at a time; the copy waits until no game runs
      (the games read the Workshop folder; a running game locks the DLLs) and is skipped when the Workshop folder
      already has this build.
    Locks folder: %LOCALAPPDATA%\ggbh-locks (machine-wide: every clone / worktree of the repository shares the games).
#>

# open lock handles of this process (kept when the file is dot-sourced again)
if ($null -eq $script:SlotLocks) { $script:SlotLocks = @{} }

function Get-GameLockDir {
    $d = Join-Path $env:LOCALAPPDATA 'ggbh-locks'
    if (-not (Test-Path -LiteralPath $d)) { New-Item -ItemType Directory -Path $d -Force | Out-Null }
    return $d
}

# Slot 1 (the normal game) + the slot copies made by new-game-slot.ps1 (%LOCALAPPDATA%\ggbh-slot<N>)
function Get-InstalledGameSlots {
    $slots = @(1)
    foreach ($n in 2..9) {
        if (Test-Path -LiteralPath (Join-Path $env:LOCALAPPDATA "ggbh-slot$n\guigubahuang.exe")) { $slots += $n }
    }
    return $slots
}

# Game process of a slot (same rule as Get-GameProcess of _game.ps1)
function Get-SlotGameProcess([int]$Slot) {
    return Get-Process -Name 'guigubahuang' -ErrorAction SilentlyContinue | Where-Object {
        $exe = "$($_.Path)"
        if ($Slot -ge 2) { $exe -match "\\ggbh-slot$Slot\\" } else { $exe -notmatch '\\ggbh-slot\d+\\' }
    } | Select-Object -First 1
}

# Opens a lock file for writing, readable by others; $null when another process holds it.
function Open-LockFile([string]$Name, [string]$Info) {
    $path = Join-Path (Get-GameLockDir) $Name
    try {
        $fs = [IO.File]::Open($path, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::Read)
    } catch { return $null }
    if ($Info) {
        $fs.SetLength(0)
        $b = [Text.Encoding]::UTF8.GetBytes($Info)
        $fs.Write($b, 0, $b.Length)
        $fs.Flush()
    }
    return $fs
}

function Test-LockHeld([string]$Name) {
    if ($script:SlotLocks.ContainsKey($Name)) { return $false }   # held by this process: not "someone else"
    $fs = Open-LockFile $Name $null
    if ($fs) { $fs.Dispose(); return $false }
    return $true
}

# Who holds a lock (pid, command line, start time), "" when unknown
function Get-LockOwner([string]$Name) {
    $path = Join-Path (Get-GameLockDir) $Name
    try {
        $fs = [IO.File]::Open($path, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
        try { return (New-Object IO.StreamReader($fs)).ReadToEnd() } finally { $fs.Dispose() }
    } catch { return '' }
}

function Get-LockInfo([string]$What) {
    return (ConvertTo-Json -Compress @{ pid = $PID; what = $What; at = (Get-Date).ToString('s'); cwd = (Get-Location).Path })
}

# A slot locked by the orchestrator that started this process (run-autotest-parallel.ps1 sets GGBH_LOCK_PARENT to its
# PID and keeps its slots locked for the whole run): its children use them.
function Test-SlotLockedByParent([int]$Slot) {
    if (-not $env:GGBH_LOCK_PARENT) { return $false }
    try { return ((Get-LockOwner "slot$Slot.lock" | ConvertFrom-Json).pid -eq [int]$env:GGBH_LOCK_PARENT) } catch { return $false }
}

function Test-SlotLockedByOther([int]$Slot) {
    return ((Test-LockHeld "slot$Slot.lock") -and -not (Test-SlotLockedByParent $Slot))
}

function Enter-SlotLock([int]$Slot, [string]$What) {
    $name = "slot$Slot.lock"
    if ($script:SlotLocks.ContainsKey($name)) { return $true }
    if ((Test-LockHeld $name) -and (Test-SlotLockedByParent $Slot)) { return $true }
    $fs = Open-LockFile $name (Get-LockInfo $What)
    if (-not $fs) { return $false }
    $script:SlotLocks[$name] = $fs
    return $true
}

function Exit-GameLocks {
    foreach ($k in @($script:SlotLocks.Keys)) { try { $script:SlotLocks[$k].Dispose() } catch { } }
    $script:SlotLocks.Clear()
}

<#
  Picks and locks a game slot. $Requested >= 1: that slot (waits while another run holds it). 0: automatic - the
  first slot of $Candidates without lock and without game, else the first without lock (its leftover game is
  restarted by the run); waits while every slot is locked. Returns the slot; throws after $WaitMinutes.
#>
function Select-GameSlot([int]$Requested, [int[]]$Candidates, [int]$WaitMinutes = 60, [string]$What = 'run-autotest') {
    if (-not $Candidates) { $Candidates = Get-InstalledGameSlots }
    if ($Requested -ge 1) { $Candidates = @($Requested) }
    $deadline = (Get-Date).AddMinutes($WaitMinutes)
    $said = $false
    while ($true) {
        $free = @($Candidates | Where-Object { -not (Test-SlotLockedByOther $_) })
        $ordered = @($free | Where-Object { -not (Get-SlotGameProcess $_) }) + @($free | Where-Object { Get-SlotGameProcess $_ })
        foreach ($s in $ordered) {
            if (Enter-SlotLock $s $What) { return $s }
        }
        if (-not $said) {
            $said = $true
            Write-Host "Every game slot ($($Candidates -join ', ')) is used by another run - waiting (max $WaitMinutes min):" -ForegroundColor Yellow
            foreach ($s in $Candidates) { Write-Host "  slot ${s}: $(Get-LockOwner "slot$s.lock")" -ForegroundColor Yellow }
            if ($Requested -eq 0 -and $Candidates.Count -eq 1) { Write-Host '  (more slots: .\scripts\game\new-game-slot.ps1 -Slot 2)' -ForegroundColor Yellow }
        }
        if ((Get-Date) -gt $deadline) { throw "No free game slot after $WaitMinutes min" }
        Start-Sleep -Seconds 10
    }
}

# ------------------------------------------------------------------ deploy

# Waits while another run builds / copies the mods (before a game is started: it must not read a half-copied folder)
function Wait-DeployFree([int]$WaitMinutes = 30) {
    $deadline = (Get-Date).AddMinutes($WaitMinutes)
    $said = $false
    while (Test-LockHeld 'deploy.lock') {
        if (-not $said) { $said = $true; Write-Host "Another run is deploying the mods - waiting: $(Get-LockOwner 'deploy.lock')" -ForegroundColor Yellow }
        if ((Get-Date) -gt $deadline) { throw "Deploy of another run not finished after $WaitMinutes min" }
        Start-Sleep -Seconds 5
    }
}

# True when every file of <repo>\<project>\debug\Mod_<id> is in <workshop>\<project id> with the same content
# (<project>: a project id, or a path in the repository such as GuiGuBaHuang-ModLib/3385996759; layout:
# scripts\project\_workshop.ps1)
function Test-WorkshopUpToDate([string]$Repo, [string]$Workshop, [string[]]$Projects) {
    foreach ($p in $Projects) {
        $src = @(Get-ChildItem -LiteralPath (Join-Path $Repo "$p\debug") -Directory -Filter 'Mod_*' -ErrorAction SilentlyContinue)
        if ($src.Count -ne 1) { return $false }
        $src = $src[0].FullName
        $dst = Join-Path $Workshop (Split-Path $p -Leaf)
        if (-not (Test-Path -LiteralPath $dst)) { return $false }
        foreach ($f in Get-ChildItem -LiteralPath $src -Recurse -File) {
            $rel = $f.FullName.Substring($src.Length)
            $o = Get-Item -LiteralPath (Join-Path $dst $rel) -ErrorAction SilentlyContinue
            if (-not $o -or $o.Length -ne $f.Length) { return $false }
            if ((Get-FileHash -LiteralPath $f.FullName -Algorithm SHA1).Hash -ne (Get-FileHash -LiteralPath $o.FullName -Algorithm SHA1).Hash) { return $false }
        }
    }
    return $true
}

<#
  Rebuilds the projects and copies them to the Workshop folder, one run at a time (deploy lock), after the dependencies
  (copied as built by their own repository: the ModLib DLL of the submodule). The copy is skipped when the Workshop
  folder already has this build; otherwise it waits until no game runs (any slot).
#>
function Invoke-SafeDeploy([string]$Repo, [string]$Workshop, [string[]]$Projects, [string[]]$Dependencies = @(), [int]$WaitMinutes = 120) {
    $deployed = @($Dependencies | Where-Object { $_ }) + @($Projects)
    $deadline = (Get-Date).AddMinutes($WaitMinutes)
    $lock = $null
    $said = $false
    while (-not ($lock = Open-LockFile 'deploy.lock' (Get-LockInfo 'deploy'))) {
        if (-not $said) { $said = $true; Write-Host "Another run is deploying - waiting: $(Get-LockOwner 'deploy.lock')" -ForegroundColor Yellow }
        if ((Get-Date) -gt $deadline) { throw "Deploy lock not free after $WaitMinutes min" }
        Start-Sleep -Seconds 5
    }
    try {
        foreach ($p in $Projects) {
            Write-Host "Rebuild $p" -ForegroundColor Cyan
            & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $Repo "$p\scripts\rebuild-project.ps1")
            if ($LASTEXITCODE -ne 0) { throw "Build of $p failed" }
        }
        if (Test-WorkshopUpToDate $Repo $Workshop $deployed) {
            Write-Host 'Workshop folder already has this build: not copied (running games are not disturbed)' -ForegroundColor Green
            return
        }
        $said = $false
        while ($g = Get-Process -Name 'guigubahuang' -ErrorAction SilentlyContinue) {
            if (-not $said) {
                $said = $true
                Write-Host "New build: waiting until no game runs before copying it to the Workshop folder (max $WaitMinutes min):" -ForegroundColor Yellow
                $g | ForEach-Object { Write-Host "  pid $($_.Id) $($_.Path)" -ForegroundColor Yellow }
            }
            if ((Get-Date) -gt $deadline) { throw "Games still running after $WaitMinutes min: build not deployed" }
            Start-Sleep -Seconds 10
        }
        foreach ($p in $deployed) {
            Write-Host "Copy $p to the Workshop folder" -ForegroundColor Cyan
            & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $Repo "$p\scripts\copy-to-steam.ps1") | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "Copy of $p failed" }
        }
        if (-not (Test-WorkshopUpToDate $Repo $Workshop $deployed)) { throw 'Workshop folder differs from the build after the copy' }
        Write-Host 'Deployed' -ForegroundColor Green
    }
    finally { $lock.Dispose() }
}
