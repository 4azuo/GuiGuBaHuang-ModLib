# Copy Both Projects to Steam Workshop
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Both Projects to Steam Workshop" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Read settings from JSON file (try multiple locations)
$settingsPath = "tasks/settings.json"
if (!(Test-Path $settingsPath)) {
    $settingsPath = "3161035078/.vscode/settings.json"
    if (!(Test-Path $settingsPath)) {
        $settingsPath = "3385996759/.vscode/settings.json"
        if (!(Test-Path $settingsPath)) {
            Write-Host "❌ Settings file not found in any expected location!" -ForegroundColor Red
            exit 1
        }
    }
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

# Get the root directory (should be current directory when called from root)
$rootDir = Get-Location
$source3161 = Join-Path $rootDir "3161035078"
$source3385 = Join-Path $rootDir "3385996759"

# Copy 3161035078 folder
Write-Host "`nCopying 3161035078 folder..." -ForegroundColor Cyan
Copy-Item -Path $source3161 -Destination "$steamWorkshopPath\" -Recurse -Force
Write-Host "✅ 3161035078 copied successfully" -ForegroundColor Green

# Copy 3385996759 folder
Write-Host "Copying 3385996759 folder..." -ForegroundColor Cyan
Copy-Item -Path $source3385 -Destination "$steamWorkshopPath\" -Recurse -Force
Write-Host "✅ 3385996759 copied successfully" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ COPY COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Both projects have been copied to Steam Workshop!" -ForegroundColor White