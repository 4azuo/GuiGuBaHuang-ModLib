# Copy Project from Steam Workshop to Git
# Shared by the mods using ModLib (GuiGuBaHuang-ModLib/scripts/project): run through the wrapper of the same name in
# <ProjectId>/scripts (-ProjectScripts = that folder: settings.json of the project, auto-built-errors.log).
param([Parameter(Mandatory = $true)][string]$ProjectScripts)

# Read settings from JSON file
$settingsPath = Join-Path $ProjectScripts "settings.json"
if (!(Test-Path $settingsPath)) {
    Write-Host "❌ Settings file not found: $settingsPath" -ForegroundColor Red
    exit 1
}

$settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
$projectId = $settings.projectId
$steamWorkshopPath = $settings.steamWorkshopPath
# The repository is the one holding <ProjectId>/scripts (same as copy-to-steam.ps1)
$repositoryPath = Split-Path (Split-Path $ProjectScripts -Parent) -Parent

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Project $projectId from Steam Workshop to Git" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "Steam Workshop Path: $steamWorkshopPath" -ForegroundColor Cyan
Write-Host "Git Repository Path: $repositoryPath" -ForegroundColor Cyan

# Get the root directory and set paths
$sourcePath = Join-Path $steamWorkshopPath $projectId
$destinationPath = Join-Path $repositoryPath $projectId

# Check if source exists
if (!(Test-Path $sourcePath)) {
    Write-Host "❌ Source folder not found: $sourcePath" -ForegroundColor Red
    exit 1
}

# The 3 project files into ModProject, the rest into debug\Mod_<id> (layout: _workshop.ps1)
. (Join-Path $PSScriptRoot '_workshop.ps1')
Write-Host "`nCopying project $projectId folder from Steam Workshop to Git..." -ForegroundColor Yellow
try { $script:failedCount = Copy-FromWorkshop $sourcePath $destinationPath }
catch { Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red; exit 1 }

if ($script:failedCount -gt 0) {
    Write-Host "`n❌ COPY INCOMPLETE: $($script:failedCount) file(s) not copied" -ForegroundColor Red
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ COPY COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Project $projectId has been copied from Steam Workshop to Git!" -ForegroundColor White
exit 0
