# Build Project 3385996759 - Release
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building Project 3385996759 - Release" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Get the root directory (parent of 3161035078 and 3385996759)
$rootDir = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
Push-Location $rootDir

Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build "3385996759/ModProject/ModCode/ModMain/ModMain.sln" --configuration Release

Pop-Location

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Project built successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to build project!" -ForegroundColor Red
    exit $LASTEXITCODE
}