<#
.SYNOPSIS
    Fill in machine-specific paths in all settings.json files.
.DESCRIPTION
    Detects and writes:
      - repositoryPath / rootDir      : where this repository lives
      - steamWorkshopPath             : <Steam library>\steamapps\workshop\content\1468810
      - gameFolderPath                : <Steam library>\steamapps\common\<game>
      - baseGameResourcesDir          : extracted BaseGameResources folder (if found)
      - modCreatorDir, translateScriptPath
    into:
      - scripts\settings.json
      - <projectId>\scripts\settings.json   (every id in scripts\settings.json "projects")
      - GuiGuBaHuang-ModCreator\ModCreator\Resources\settings.json

    Steam/game paths come from the registry and libraryfolders.vdf, so games
    installed on other drives are found too. Interactive mode lets you confirm
    or override each value; -Yes accepts the detected values.

    Only the path values are replaced; formatting and other keys are untouched.
.EXAMPLE
    .\02-configure-settings.ps1
.EXAMPLE
    .\02-configure-settings.ps1 -Yes
.EXAMPLE
    .\02-configure-settings.ps1 -GamePath "D:\SteamLibrary\steamapps\common\<game>" -DryRun
#>

[CmdletBinding()]
param(
    [string]$RepoPath,
    [string]$WorkshopPath,
    [string]$GamePath,
    [string]$BaseGameResourcesDir,
    [switch]$OpenFiles,
    [switch]$SkipWorktree,
    [switch]$DryRun,
    [switch]$Yes,
    # Backward compatibility
    [switch]$OpenAll,
    [switch]$SkipConfirmation
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')
if ($Yes -or $SkipConfirmation) { $Global:SetupAssumeYes = $true }
if ($OpenAll) { $OpenFiles = $true }

Write-Title 'GuiGuBaHuang-ModLib - Configure Settings'

$repo = Resolve-RepoPath $RepoPath $PSScriptRoot
$rootSettings = Join-Path $repo 'scripts\settings.json'
$modCreatorDir = Join-Path $repo 'GuiGuBaHuang-ModCreator'
$modCreatorSettings = Join-Path $modCreatorDir 'ModCreator\Resources\settings.json'
$root = Read-JsonFile $rootSettings
$mcCurrent = if (Test-Path $modCreatorSettings) { Read-JsonFile $modCreatorSettings } else { $null }

# ---- detect
Write-Info 'Detecting Steam, game and resources...'
$game = Find-GameInstall
$steamRoot = Get-SteamRoot

if (-not $WorkshopPath) {
    if ($game) { $WorkshopPath = $game.WorkshopPath }
    elseif ($steamRoot) { $WorkshopPath = Join-Path $steamRoot "steamapps\workshop\content\$GameAppId" }
    else { $WorkshopPath = $root.steamWorkshopPath }
}
if (-not $GamePath) {
    if ($game) { $GamePath = $game.GamePath }
    elseif ($mcCurrent) { $GamePath = $mcCurrent.gameFolderPath }
}
$WorkshopPath = Get-RealPath $WorkshopPath
$GamePath = Get-RealPath $GamePath
if (-not $BaseGameResourcesDir) {
    $BaseGameResourcesDir = Find-BaseGameResourcesDir $repo
    if (-not $BaseGameResourcesDir -and $mcCurrent -and $mcCurrent.baseGameResourcesDir -and (Test-Path $mcCurrent.baseGameResourcesDir)) {
        $BaseGameResourcesDir = $mcCurrent.baseGameResourcesDir
    }
}

if ($game) { Write-Ok "Game found: $($game.GamePath)" }
else { Write-Warn 'Game not found in any Steam library - enter the path manually if it is installed' }

Write-Host ''
Write-Info 'Confirm each value (press Enter to accept, or type another path):'
$WorkshopPath = Read-Value 'Steam Workshop folder of the game' $WorkshopPath
$GamePath = Read-Value 'Game install folder' $GamePath
$BaseGameResourcesDir = Read-Value 'BaseGameResources folder (leave as-is if you do not have it)' $BaseGameResourcesDir

$values = [ordered]@{
    'Repository'        = $repo
    'Steam Workshop'    = $WorkshopPath
    'Game folder'       = $GamePath
    'BaseGameResources' = $BaseGameResourcesDir
}
Write-Host ''
foreach ($k in $values.Keys) {
    $v = $values[$k]
    $state = if (-not $v) { 'not set' } elseif (Test-Path $v) { 'exists' } else { 'MISSING' }
    $color = if ($state -eq 'exists') { 'Green' } else { 'Yellow' }
    Write-Host ('  {0,-18} {1}  [{2}]' -f $k, $v, $state) -ForegroundColor $color
}
Write-Host ''

if ($DryRun) { Write-Info 'Dry run - nothing will be written' }
elseif (-not (Confirm-Choice 'Write these values to the settings files?' $true)) {
    Write-Warn 'Cancelled - no file changed'
    exit 0
}

# ---- build the edit list: file -> key -> value
$edits = @()
$edits += @{ File = $rootSettings; Values = [ordered]@{ repositoryPath = $repo; steamWorkshopPath = $WorkshopPath } }
foreach ($projectId in $root.projects) {
    $f = Join-Path $repo "$projectId\scripts\settings.json"
    if (Test-Path $f) {
        $edits += @{ File = $f; Values = [ordered]@{ repositoryPath = $repo; steamWorkshopPath = $WorkshopPath } }
    }
    else { Write-Warn "Project settings not found: $f" }
}
if (Test-Path $modCreatorSettings) {
    $mc = [ordered]@{
        rootDir             = $repo
        modCreatorDir       = $modCreatorDir
        translateScriptPath = Join-Path $repo 'Translate\run.py'
        steamWorkshopDir    = $WorkshopPath
    }
    if ($GamePath) { $mc.gameFolderPath = $GamePath }
    if ($BaseGameResourcesDir) { $mc.baseGameResourcesDir = $BaseGameResourcesDir }
    $edits += @{ File = $modCreatorSettings; Values = $mc }
}
else {
    Write-Warn "ModCreator settings not found: $modCreatorSettings (run 01-clone-repository.ps1 first)"
}

# ---- apply
$changedFiles = @()
foreach ($e in $edits) {
    $rel = $e.File.Substring($repo.Length).TrimStart('\')
    Write-Info $rel
    $fileChanged = $false
    foreach ($key in $e.Values.Keys) {
        $result = Set-JsonStringValue -Path $e.File -Key $key -Value $e.Values[$key] -PathValue -WhatIf:$DryRun
        switch ($result) {
            'changed' { Write-Host "    $key = $($e.Values[$key])" -ForegroundColor Green; $fileChanged = $true }
            'same'    { Write-Host "    $key (unchanged)" -ForegroundColor DarkGray }
            'missing' { Write-Warn "    key `"$key`" not found in file - add it manually" }
        }
    }
    if ($fileChanged) { $changedFiles += $e.File }
}

# ---- optionally hide local path edits from git
# Only files that now differ from what is committed in git
$ErrorActionPreference = 'Continue'
$dirtyFiles = @($changedFiles | Where-Object {
    $dir = Split-Path $_ -Parent
    git -C $dir diff --quiet -- (Split-Path $_ -Leaf)
    $LASTEXITCODE -eq 1
})
$ErrorActionPreference = 'Stop'

if ($dirtyFiles.Count -gt 0 -and -not $DryRun) {
    Write-Host ''
    Write-Info 'These files now differ from the committed version (your local paths):'
    $dirtyFiles | ForEach-Object { Write-Info "  $_" }
    Write-Info '  N = keep them visible in git (choose this if you want to commit the new paths)'
    Write-Info '  y = hide them from git status/commit on this machine only (git update-index --skip-worktree)'
    $hide = $SkipWorktree -or (Confirm-Choice 'Hide these local path changes from git?' $false)
    if ($hide) {
        $ErrorActionPreference = 'Continue'
        foreach ($f in $dirtyFiles) {
            $dir = Split-Path $f -Parent
            git -C $dir update-index --skip-worktree (Split-Path $f -Leaf)
            if ($LASTEXITCODE -eq 0) { Write-Ok "skip-worktree: $f" }
        }
        $ErrorActionPreference = 'Stop'
        Write-Info 'Undo later with: git update-index --no-skip-worktree <file>'
    }
}

# ---- optionally open files for manual review
if ($OpenFiles) {
    foreach ($e in $edits) {
        Write-Info "Opening $($e.File) - close Notepad to continue"
        Start-Process notepad.exe -ArgumentList "`"$($e.File)`"" -Wait
    }
}

Write-Host ''
Write-Ok 'Settings configured'
Write-Info 'Other keys (projects, modName, debounce...) can be edited by hand - see INSTALLATION.md.'
Write-Info 'Next: .\scripts\setup\04-verify-environment.ps1'
exit 0
