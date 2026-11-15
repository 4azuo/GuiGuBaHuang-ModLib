<#
.SYNOPSIS
    Shared helpers for scripts/setup/*.ps1
.DESCRIPTION
    Dot-source from a setup script:  . (Join-Path $PSScriptRoot '_common.ps1')

    Keep this file ASCII-only: Windows PowerShell 5.1 reads BOM-less scripts as ANSI,
    so non-ASCII text (e.g. the game folder name) is built from char codes instead.
#>

$script:RepoUrlDefault       = 'https://github.com/4azuo/GuiGuBaHuang-ModLib.git'
$script:ModCreatorUrlDefault = 'https://github.com/4azuo/GuiGuBaHuang-ModCreator.git'
$script:DefaultRepoPath      = 'C:\git\GuiGuBaHuang-ModLib'
$script:GameAppId            = '1468810'
# Game install folder name (Gui Gu Ba Huang in Chinese characters)
$script:GameFolderName       = -join [char[]](0x9B3C, 0x8C37, 0x516B, 0x8352)

if (-not (Get-Variable -Name SetupAssumeYes -Scope Global -ErrorAction SilentlyContinue)) {
    $Global:SetupAssumeYes = $false
}

# ---------------------------------------------------------------- output

function Write-Info([string]$Message) { Write-Host "INFO:    $Message" -ForegroundColor Cyan }
function Write-Ok([string]$Message)   { Write-Host "OK:      $Message" -ForegroundColor Green }
function Write-Warn([string]$Message) { Write-Host "WARNING: $Message" -ForegroundColor Yellow }
function Write-Err([string]$Message)  { Write-Host "ERROR:   $Message" -ForegroundColor Red }

function Write-Title([string]$Message) {
    Write-Host ''
    Write-Host ('=' * 64) -ForegroundColor DarkCyan
    Write-Host "  $Message" -ForegroundColor Magenta
    Write-Host ('=' * 64) -ForegroundColor DarkCyan
}

# ---------------------------------------------------------------- prompts

# Yes/No question. Returns $Default without asking when -Yes was given
# ($Global:SetupAssumeYes) or when the console cannot read input.
function Confirm-Choice {
    param([string]$Message, [bool]$Default = $true)
    $hint = if ($Default) { 'Y/n' } else { 'y/N' }
    if ($Global:SetupAssumeYes) {
        $auto = if ($Default) { 'y' } else { 'n' }
        Write-Host "$Message [$hint] -> $auto (auto)" -ForegroundColor DarkGray
        return $Default
    }
    try { $answer = Read-Host "$Message [$hint]" } catch { return $Default }
    if ([string]::IsNullOrWhiteSpace($answer)) { return $Default }
    return ($answer.Trim() -match '^(y|yes)$')
}

# Ask for a value, Enter keeps the default.
function Read-Value {
    param([string]$Label, [string]$Default)
    if ($Global:SetupAssumeYes) { return $Default }
    try { $answer = Read-Host "$Label`n  [Enter = $Default]" } catch { return $Default }
    if ([string]::IsNullOrWhiteSpace($answer)) { return $Default }
    return $answer.Trim().Trim('"')
}

# ---------------------------------------------------------------- system

function Test-IsAdmin {
    $principal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Test-Command([string]$Name) {
    return [bool](Get-Command $Name -ErrorAction SilentlyContinue)
}

# Reload PATH from the registry so tools installed a moment ago are usable.
function Update-SessionPath {
    $machine = [Environment]::GetEnvironmentVariable('Path', 'Machine')
    $user    = [Environment]::GetEnvironmentVariable('Path', 'User')
    $env:Path = "$machine;$user"
}

function Test-SamePath([string]$A, [string]$B) {
    if (-not $A -or -not $B) { return $false }
    return ($A.TrimEnd('\', '/') -replace '/', '\') -ieq ($B.TrimEnd('\', '/') -replace '/', '\')
}

# ---------------------------------------------------------------- repository

function Test-RepoRoot([string]$Path) {
    if ([string]::IsNullOrWhiteSpace($Path)) { return $false }
    return (Test-Path (Join-Path $Path '3385996759')) -and (Test-Path (Join-Path $Path 'scripts\settings.json'))
}

# Repo root when the script runs from <repo>\scripts\setup, otherwise $null.
function Get-RepoRootFromScript([string]$ScriptDir) {
    if (-not $ScriptDir) { return $null }
    $candidate = Split-Path (Split-Path $ScriptDir -Parent) -Parent
    if ($candidate -and (Test-RepoRoot $candidate)) { return (Resolve-Path $candidate).Path }
    return $null
}

function Resolve-RepoPath([string]$RepoPath, [string]$ScriptDir) {
    if ($RepoPath) {
        if (-not (Test-RepoRoot $RepoPath)) { throw "Not a GuiGuBaHuang-ModLib repository: $RepoPath" }
        return (Resolve-Path $RepoPath).Path
    }
    $found = Get-RepoRootFromScript $ScriptDir
    if (-not $found) { throw 'Cannot find the repository root. Pass -RepoPath <path-to-GuiGuBaHuang-ModLib>.' }
    return $found
}

# Make sure GuiGuBaHuang-ModCreator is checked out.
# Uses the submodule mapping when .gitmodules has one; otherwise clones the
# ModCreator repo directly and checks out the commit the parent repo points to.
function Initialize-ModCreator {
    param([string]$RepoPath, [string]$Url = $script:ModCreatorUrlDefault)
    $ErrorActionPreference = 'Continue'
    $dir = Join-Path $RepoPath 'GuiGuBaHuang-ModCreator'

    if (Test-Path (Join-Path $dir 'ModCreator.sln')) {
        Write-Ok "ModCreator already present: $dir"
        return $true
    }

    $pinned = $null
    Push-Location $RepoPath
    try {
        $gitmodules = Join-Path $RepoPath '.gitmodules'
        if ((Test-Path $gitmodules) -and (Select-String -Path $gitmodules -Pattern 'GuiGuBaHuang-ModCreator' -Quiet)) {
            Write-Info 'Initializing ModCreator submodule...'
            git submodule update --init --recursive
            if (Test-Path (Join-Path $dir 'ModCreator.sln')) {
                Write-Ok 'ModCreator submodule initialized'
                return $true
            }
        }
        $entry = git ls-tree HEAD GuiGuBaHuang-ModCreator
        if ($entry -match '^160000 commit ([0-9a-f]{40})') { $pinned = $Matches[1] }
    }
    finally { Pop-Location }

    if ((Test-Path $dir) -and (Get-ChildItem $dir -Force | Select-Object -First 1)) {
        Write-Warn "Folder exists but has no ModCreator.sln: $dir"
        Write-Warn 'Empty or delete that folder, then run this step again.'
        return $false
    }

    Write-Info "Cloning ModCreator: $Url"
    git clone $Url $dir
    if ($LASTEXITCODE -ne 0) {
        Write-Err 'Failed to clone ModCreator'
        return $false
    }

    if ($pinned) {
        $head = git -C $dir rev-parse HEAD
        if ($head -ne $pinned) {
            git -C $dir checkout --quiet $pinned
            if ($LASTEXITCODE -eq 0) {
                Write-Info "ModCreator checked out at pinned commit $($pinned.Substring(0, 7)) (detached HEAD)"
            }
            else {
                Write-Warn "Could not check out pinned commit $pinned, staying on default branch"
            }
        }
    }
    Write-Ok "ModCreator ready: $dir"
    return $true
}

# ---------------------------------------------------------------- Steam / game

function Get-SteamRoot {
    # HKLM InstallPath keeps the real casing; HKCU SteamPath is often lower-case
    $candidates = @()
    foreach ($key in 'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam', 'HKLM:\SOFTWARE\Valve\Steam', 'HKCU:\Software\Valve\Steam') {
        $props = Get-ItemProperty -Path $key -ErrorAction SilentlyContinue
        if ($props) {
            if ($props.InstallPath) { $candidates += $props.InstallPath }
            if ($props.SteamPath)   { $candidates += $props.SteamPath }
        }
    }
    $candidates += "${env:ProgramFiles(x86)}\Steam"
    foreach ($c in $candidates) {
        $p = $c -replace '/', '\'
        if (Test-Path (Join-Path $p 'steamapps')) { return (Get-RealPath $p) }
    }
    return $null
}

# All Steam library folders (from steamapps\libraryfolders.vdf).
function Get-SteamLibraries([string]$SteamRoot) {
    if (-not $SteamRoot) { return @() }
    $libs = New-Object System.Collections.Generic.List[string]
    $libs.Add($SteamRoot)
    $vdf = Join-Path $SteamRoot 'steamapps\libraryfolders.vdf'
    if (Test-Path $vdf) {
        $text = [IO.File]::ReadAllText($vdf, [Text.Encoding]::UTF8)
        foreach ($m in [regex]::Matches($text, '"path"\s+"([^"]+)"')) {
            $p = Get-RealPath ($m.Groups[1].Value -replace '\\\\', '\')
            $exists = $false
            foreach ($l in $libs) { if (Test-SamePath $l $p) { $exists = $true } }
            if (-not $exists) { $libs.Add($p) }
        }
    }
    return $libs.ToArray()
}

# Folder path with the casing stored on disk (registry/vdf paths are often
# lower-case). Returns the input unchanged when it does not exist.
function Get-RealPath([string]$Path) {
    if (-not $Path -or -not (Test-Path -LiteralPath $Path)) { return $Path }
    $dir = New-Object IO.DirectoryInfo($Path)
    if (-not $dir.Parent) { return $dir.FullName.ToUpperInvariant() }   # drive root, e.g. C:\
    $name = ($dir.Parent.GetFileSystemInfos($dir.Name) | Select-Object -First 1).Name
    if (-not $name) { $name = $dir.Name }
    return Join-Path (Get-RealPath $dir.Parent.FullName) $name
}

# Locate the game. Returns @{ Library; GamePath; WorkshopPath } or $null.
function Find-GameInstall {
    $root = Get-SteamRoot
    foreach ($lib in (Get-SteamLibraries $root)) {
        $apps = Join-Path $lib 'steamapps'
        $installDir = $null
        $manifest = Join-Path $apps "appmanifest_$($script:GameAppId).acf"
        if (Test-Path $manifest) {
            $m = [regex]::Match([IO.File]::ReadAllText($manifest, [Text.Encoding]::UTF8), '"installdir"\s+"([^"]+)"')
            if ($m.Success) { $installDir = $m.Groups[1].Value }
        }
        if (-not $installDir) { $installDir = $script:GameFolderName }
        $game = Join-Path $apps "common\$installDir"
        if (Test-Path $game) {
            return [pscustomobject]@{
                Library      = $lib
                GamePath     = $game
                WorkshopPath = Join-Path $apps "workshop\content\$($script:GameAppId)"
            }
        }
    }
    return $null
}

# ---------------------------------------------------------------- BaseGameResources

function Get-ResourceSearchDirs([string]$RepoPath) {
    $dirs = @()
    if ($RepoPath) {
        $dirs += Join-Path $RepoPath 'GuiGuBaHuang-ModCreator'
        $dirs += $RepoPath
    }
    $dirs += Join-Path $env:USERPROFILE 'Downloads'
    $dirs += Join-Path $env:USERPROFILE 'Desktop'
    return $dirs | Where-Object { Test-Path $_ }
}

# Already-extracted BaseGameResources folder (newest), or $null.
function Find-BaseGameResourcesDir([string]$RepoPath) {
    foreach ($d in (Get-ResourceSearchDirs $RepoPath)) {
        $hit = Get-ChildItem -Path $d -Directory -Filter 'BaseGameResources*' -ErrorAction SilentlyContinue |
            Where-Object { Get-ChildItem $_.FullName -Force | Select-Object -First 1 } |
            Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($hit) { return $hit.FullName }
    }
    return $null
}

# BaseGameResources archive (newest), or $null.
function Find-BaseGameResourcesArchive([string]$RepoPath) {
    foreach ($d in (Get-ResourceSearchDirs $RepoPath)) {
        $hit = Get-ChildItem -Path $d -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like 'BaseGameResources*' -and $_.Extension -in '.rar', '.7z', '.zip' } |
            Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($hit) { return $hit.FullName }
    }
    return $null
}

# ---------------------------------------------------------------- JSON settings

function Read-JsonFile([string]$Path) {
    return ([IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8) | ConvertFrom-Json)
}

# Replace the string value of "Key" in a JSON file in place, keeping the file's
# formatting, encoding and BOM. Returns 'changed', 'same' or 'missing'.
function Set-JsonStringValue {
    param([string]$Path, [string]$Key, [string]$Value, [switch]$PathValue, [switch]$WhatIf)
    $bytes  = [IO.File]::ReadAllBytes($Path)
    $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $text   = [IO.File]::ReadAllText($Path, [Text.Encoding]::UTF8)

    $pattern = '("' + [regex]::Escape($Key) + '"\s*:\s*)"((?:[^"\\]|\\.)*)"'
    $match = [regex]::Match($text, $pattern)
    if (-not $match.Success) { return 'missing' }

    # -PathValue: the same path with different casing / trailing slash is not a change
    if ($PathValue) {
        $current = $match.Groups[2].Value.Replace('\\', '\').Replace('\"', '"')
        if (Test-SamePath $current $Value) { return 'same' }
    }

    $escaped = $Value.Replace('\', '\\').Replace('"', '\"')
    $newText = [regex]::Replace($text, $pattern, { param($m) $m.Groups[1].Value + '"' + $escaped + '"' })
    if ($newText -ceq $text) { return 'same' }

    $null = $newText | ConvertFrom-Json   # throws if the result is not valid JSON
    if (-not $WhatIf) {
        [IO.File]::WriteAllText($Path, $newText, (New-Object Text.UTF8Encoding($hasBom)))
    }
    return 'changed'
}
