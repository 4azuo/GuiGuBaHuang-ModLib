# Auto Build and Deploy Script
# Shared by the mods using ModLib (GuiGuBaHuang-ModLib/scripts/project): run through the wrapper of the same name in
# <ProjectId>/scripts (-ProjectScripts = that folder: settings.json of the project, auto-built-errors.log).
param([Parameter(Mandatory = $true)][string]$ProjectScripts)
# Launches the Python auto-build watcher

$ErrorActionPreference = "Stop"

# Read settings from JSON file
$settingsPath = Join-Path $ProjectScripts "settings.json"
if (!(Test-Path $settingsPath)) {
    Write-Host "✗ Settings file not found: $settingsPath" -ForegroundColor Red
    exit 1
}

$settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
$projectId = $settings.projectId
$modName = $settings.modName

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Starting Auto Build and Deploy Watcher" -ForegroundColor Cyan
Write-Host "  Project: $projectId ($modName)" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# Check if Python is available
try {
    $pythonVersion = python --version 2>&1
    Write-Host "✓ Python found: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Python not found! Please install Python 3.x" -ForegroundColor Red
    exit 1
}

# Check if watchdog is installed
Write-Host "Checking for watchdog module..." -ForegroundColor Yellow
$watchdogCheck = python -c "import watchdog; print('installed')" 2>&1
if ($watchdogCheck -ne "installed") {
    Write-Host "✗ watchdog module not found!" -ForegroundColor Red
    Write-Host "Installing watchdog..." -ForegroundColor Yellow
    pip install watchdog
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Failed to install watchdog" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ watchdog installed successfully" -ForegroundColor Green
} else {
    Write-Host "✓ watchdog module is installed" -ForegroundColor Green
}

Write-Host ""

# Run the shared watcher on the project
python (Join-Path $PSScriptRoot "auto-build-and-deploy.py") $ProjectScripts
