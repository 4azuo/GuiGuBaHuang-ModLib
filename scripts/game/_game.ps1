<#
.SYNOPSIS
    Shared paths for game tools (dot-source: . (Join-Path $PSScriptRoot '_game.ps1'))
.DESCRIPTION
    Keep ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI).
#>

$script:GameToolsDir = $PSScriptRoot
. (Join-Path $script:GameToolsDir '..\setup\_common.ps1')

$script:GameProcessName = 'guigubahuang'

# All folders/files the game tools care about.
function Get-GamePaths {
    param([string]$GamePath)

    $repo = (Resolve-Path (Join-Path $script:GameToolsDir '..\..')).Path
    $settings = Read-JsonFile (Join-Path $repo 'scripts\settings.json')

    $install = $null
    if (-not $GamePath) {
        $install = Find-GameInstall
        if ($install) { $GamePath = $install.GamePath }
    }
    if (-not $GamePath) {
        $mcSettings = Join-Path $repo 'GuiGuBaHuang-ModCreator\ModCreator\Resources\settings.json'
        if (Test-Path $mcSettings) { $GamePath = (Read-JsonFile $mcSettings).gameFolderPath }
    }

    $workshop = $settings.steamWorkshopPath
    if (-not $workshop -and $install) { $workshop = $install.WorkshopPath }

    # Unity persistent data: Player.log, ModLib logs (mod\<modId>\*.log)
    $data = Join-Path $env:USERPROFILE 'AppData\LocalLow\guigugame\guigubahuang'
    $melon = if ($GamePath) { Join-Path $GamePath 'MelonLoader' } else { $null }

    return [pscustomobject]@{
        Repo            = $repo
        Projects        = @($settings.projects)
        GamePath        = $GamePath
        GameExe         = if ($GamePath) { Join-Path $GamePath 'guigubahuang.exe' } else { $null }
        Workshop        = $workshop
        GameData        = $data
        PlayerLog       = Join-Path $data 'Player.log'
        ModLogs         = Join-Path $data 'mod'
        MelonDir        = $melon
        MelonLog        = if ($melon) { Join-Path $melon 'Latest.log' } else { $null }
        MelonProxy      = if ($GamePath) { Join-Path $GamePath 'version.dll' } else { $null }
        ModLoadComplete = if ($melon) { Join-Path $melon 'GGBH_MOD_LOAD_COMPLETE.txt' } else { $null }
        GameRuns        = Join-Path $repo 'logs\game-runs'
    }
}

# MelonLoader's proxy (version.dll) refuses to start when the game path contains
# non-ASCII characters (the Steam folder name is Chinese). Starting the exe through
# a directory junction with an ASCII path works around it. Returns the junction path
# (created if needed), or the game path itself when it is already ASCII.
function Get-AsciiGamePath {
    param([string]$GamePath, [string]$Repo)
    if ($GamePath -notmatch '[^\x00-\x7F]') { return $GamePath }

    # Outside the repo on purpose: editors/indexers would follow a junction into the 29 GB game folder
    $candidates = @(
        (Join-Path $env:LOCALAPPDATA 'ggbh-game-link'),
        (Join-Path $env:PUBLIC 'ggbh-game-link')
    ) | Where-Object { $_ -notmatch '[^\x00-\x7F]' -and $_.Length -le 100 }
    # Length limit: MelonLoader's assembly generator writes paths ~150 chars below the
    # game folder and fails past MAX_PATH (260)

    foreach ($link in $candidates) {
        $item = Get-Item -LiteralPath $link -Force -ErrorAction SilentlyContinue
        if ($item) {
            if ($item.LinkType -eq 'Junction' -and (Test-SamePath ($item.Target | Select-Object -First 1) $GamePath)) { return $link }
            continue   # something else lives there, try the next candidate
        }
        try {
            New-Item -ItemType Junction -Path $link -Target $GamePath -ErrorAction Stop | Out-Null
            Write-Info "Created ASCII junction $link -> $GamePath"
            return $link
        }
        catch { Write-Warn "Could not create junction at ${link}: $($_.Exception.Message)" }
    }
    return $null
}

function Get-GameProcess {
    return Get-Process -Name $script:GameProcessName -ErrorAction SilentlyContinue | Select-Object -First 1
}

# Ask the game to close, kill it after $TimeoutSec.
function Stop-Game {
    param([int]$TimeoutSec = 15)
    $p = Get-GameProcess
    if (-not $p) { return }
    Write-Info "Closing game (pid $($p.Id))..."
    $null = $p.CloseMainWindow()
    if (-not $p.WaitForExit($TimeoutSec * 1000)) {
        Write-Warn 'Game did not close in time - killing it'
        Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
        $null = $p.WaitForExit(5000)
    }
}
