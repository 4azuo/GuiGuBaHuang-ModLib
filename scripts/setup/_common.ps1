<#
.SYNOPSIS
    Shared helpers of the setup scripts and the game tools (GuiGuBaHuang-ModLib/scripts/setup, used by ModLib and by
    every mod repository holding ModLib as a submodule).
.DESCRIPTION
    Dot-source from a setup script:  . (Join-Path $PSScriptRoot '_common.ps1')
    A mod repository adds its own helpers (clone, ModCreator...) in its scripts/setup/_common.ps1.

    Keep this file ASCII-only: Windows PowerShell 5.1 reads BOM-less scripts as ANSI,
    so non-ASCII text (e.g. the game folder name) is built from char codes instead.
#>

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

# A repository of the tools: ModLib (ModLib\ModLib.csproj) or a mod repository (scripts\settings.json with "projects").
function Test-RepoRoot([string]$Path) {
    if ([string]::IsNullOrWhiteSpace($Path) -or -not (Test-Path (Join-Path $Path '.git'))) { return $false }
    return (Test-Path (Join-Path $Path 'scripts\settings.json')) -or (Test-Path (Join-Path $Path 'ModLib\ModLib.csproj'))
}

# Repo root when the script runs from <repo>\scripts\setup, otherwise $null.
function Get-RepoRootFromScript([string]$ScriptDir) {
    if (-not $ScriptDir) { return $null }
    $candidate = Split-Path (Split-Path $ScriptDir -Parent) -Parent
    if ($candidate -and (Test-RepoRoot $candidate)) { return (Resolve-Path $candidate).Path }
    return $null
}

# The repository to set up: -RepoPath, else the git repository of the current folder (the mod repository when run from
# its root, ModLib when run from GuiGuBaHuang-ModLib), else the repository of the script (ModLib).
function Resolve-RepoPath([string]$RepoPath, [string]$ScriptDir) {
    if ($RepoPath) {
        if (-not (Test-RepoRoot $RepoPath)) { throw "Not a repository of the mods (no scripts\settings.json, no ModLib\ModLib.csproj): $RepoPath" }
        return (Resolve-Path $RepoPath).Path
    }
    $ErrorActionPreference = 'Continue'
    $top = (git rev-parse --show-toplevel 2>$null)
    if ($top -and (Test-RepoRoot $top)) { return (Resolve-Path $top).Path }
    $found = Get-RepoRootFromScript $ScriptDir
    if (-not $found) { throw 'Cannot find the repository root. Pass -RepoPath <repository>.' }
    return $found
}

# Project folders of a repository (relative): "projects" of scripts\settings.json, else every <dir>\scripts\settings.json.
function Get-RepoProjects([string]$Repo) {
    $f = Join-Path $Repo 'scripts\settings.json'
    if (Test-Path $f) {
        $s = Read-JsonFile $f
        if ($s.projects) { return @($s.projects) }
    }
    return @(Get-ChildItem -LiteralPath $Repo -Directory | Where-Object { Test-Path (Join-Path $_.FullName 'scripts\settings.json') } | ForEach-Object { $_.Name })
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
