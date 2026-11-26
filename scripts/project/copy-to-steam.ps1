# Copy to Steam Workshop
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

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Project $projectId to Steam Workshop" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "Steam Workshop Path: $steamWorkshopPath" -ForegroundColor Cyan

# Create directory if it doesn't exist
if (!(Test-Path $steamWorkshopPath)) {
    Write-Host "Creating directory: $steamWorkshopPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $steamWorkshopPath -Force
} else {
    Write-Host "Directory already exists: $steamWorkshopPath" -ForegroundColor Green
}

# Get the root directory and set source path
$rootDir = Split-Path (Split-Path $ProjectScripts -Parent) -Parent
$sourcePath = Join-Path $rootDir $projectId

# The content of debug\Mod_<id> and the 3 project files of ModProject: no source, scripts or tests (_workshop.ps1)
. (Join-Path $PSScriptRoot '_workshop.ps1')
Write-Host "`nCopying $projectId (debug\Mod_<id> + ModProject files) to Steam Workshop..." -ForegroundColor Yellow
$destination = Join-Path $steamWorkshopPath $projectId
try { $failed = Copy-ToWorkshop $sourcePath $destination }
catch { Write-Host "❌ $($_.Exception.Message)" -ForegroundColor Red; exit 1 }
if ($failed -gt 0) { Write-Host "⚠️  $failed file(s) not copied (in use)" -ForegroundColor Yellow }

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ COPY COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Project $projectId has been copied to Steam Workshop!" -ForegroundColor White