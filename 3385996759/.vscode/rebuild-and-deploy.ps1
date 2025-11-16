# Rebuild and Deploy Project 3385996759 to Steam
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Rebuild and Deploy Project 3385996759" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Run Rebuild Script
Write-Host "`n[Step 1/2] Running Rebuild Script..." -ForegroundColor Yellow
$rebuildScript = Join-Path $PSScriptRoot "rebuild-project.ps1"
& $rebuildScript

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Rebuild script failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Step 2: Run Copy to Steam Script
Write-Host "`n[Step 2/2] Running Copy to Steam Script..." -ForegroundColor Yellow
$copyScript = Join-Path $PSScriptRoot "copy-to-steam.ps1"
& $copyScript

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "🎉 DEPLOYMENT COMPLETED SUCCESSFULLY! 🎉" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Project 3385996759 has been rebuilt and deployed to Steam Workshop!" -ForegroundColor White
} else {
    Write-Host "❌ Copy to Steam script failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}