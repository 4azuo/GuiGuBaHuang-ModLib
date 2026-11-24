# Rebuild Project - Release
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

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Rebuild Project $projectId - Release" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Run Clean Script
Write-Host "`n[Step 1/2] Running Clean Script..." -ForegroundColor Yellow
$cleanScript = Join-Path $PSScriptRoot "clean-project.ps1"
& $cleanScript -ProjectScripts $ProjectScripts

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Clean script failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Step 2: Run Build Script  
Write-Host "`n[Step 2/2] Running Build Script..." -ForegroundColor Yellow
$buildScript = Join-Path $PSScriptRoot "build-project.ps1"
& $buildScript -ProjectScripts $ProjectScripts

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "🎉 REBUILD COMPLETED SUCCESSFULLY! 🎉" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
} else {
    Write-Host "❌ Build script failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}