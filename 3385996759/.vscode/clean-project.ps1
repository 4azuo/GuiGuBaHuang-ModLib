# Clean Project 3385996759 - Release
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Cleaning Project 3385996759 - Release" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Get the root directory (parent of 3161035078 and 3385996759)
$rootDir = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Push-Location $rootDir

Write-Host "Cleaning solution..." -ForegroundColor Yellow
dotnet clean "3385996759/ModProject/ModCode/ModMain/ModMain.sln" --configuration Release

Pop-Location

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Project cleaned successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to clean project!" -ForegroundColor Red
    exit $LASTEXITCODE
}