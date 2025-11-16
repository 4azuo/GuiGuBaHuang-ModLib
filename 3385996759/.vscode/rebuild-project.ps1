# Rebuild Project 3385996759 - Release
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Rebuild Project 3385996759 - Release" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Run Clean Script
Write-Host "`n[Step 1/2] Running Clean Script..." -ForegroundColor Yellow
$cleanScript = Join-Path $PSScriptRoot "clean-project.ps1"
& $cleanScript

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Clean script failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Step 2: Run Build Script  
Write-Host "`n[Step 2/2] Running Build Script..." -ForegroundColor Yellow
$buildScript = Join-Path $PSScriptRoot "build-project.ps1"
& $buildScript

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "🎉 REBUILD COMPLETED SUCCESSFULLY! 🎉" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
} else {
    Write-Host "❌ Build script failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}