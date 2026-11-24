# Clean Project - Release
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
Write-Host "Cleaning Project $projectId - Release" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Root of the mod repository (parent of the project folder)
$rootDir = Split-Path (Split-Path $ProjectScripts -Parent) -Parent
Push-Location $rootDir

Write-Host "Cleaning solution..." -ForegroundColor Yellow
dotnet clean "$projectId/ModProject/ModCode/ModMain/ModMain.sln" --configuration Release

Pop-Location

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Project cleaned successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to clean project!" -ForegroundColor Red
    exit $LASTEXITCODE
}