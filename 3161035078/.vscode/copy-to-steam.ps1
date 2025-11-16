# Copy Project 3161035078 to Steam Workshop
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Project 3161035078 to Steam Workshop" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Read settings from JSON file
$settingsPath = Join-Path $PSScriptRoot "settings.json"
if (!(Test-Path $settingsPath)) {
    Write-Host "❌ Settings file not found: $settingsPath" -ForegroundColor Red
    exit 1
}

$settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
$steamWorkshopPath = $settings.steamWorkshopPath

Write-Host "Steam Workshop Path: $steamWorkshopPath" -ForegroundColor Cyan

# Create directory if it doesn't exist
if (!(Test-Path $steamWorkshopPath)) {
    Write-Host "Creating directory: $steamWorkshopPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $steamWorkshopPath -Force
} else {
    Write-Host "Directory already exists: $steamWorkshopPath" -ForegroundColor Green
}

# Get the root directory and set source path
$rootDir = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$sourcePath = Join-Path $rootDir "3161035078"

# Copy only 3161035078 folder
Write-Host "`nCopying 3161035078 folder to Steam Workshop..." -ForegroundColor Yellow
Copy-Item -Path $sourcePath -Destination "$steamWorkshopPath\" -Recurse -Force

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ COPY COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Project 3161035078 has been copied to Steam Workshop!" -ForegroundColor White