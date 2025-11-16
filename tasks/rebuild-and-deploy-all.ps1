# Rebuild and Deploy All Projects to Steam
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Rebuild and Deploy All Projects to Steam" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Rebuild Project 3385996759
Write-Host "`n[Step 1/4] Rebuilding Project 3385996759..." -ForegroundColor Yellow
$rebuild3385Script = "3385996759/.vscode/rebuild-project.ps1"
& $rebuild3385Script
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to rebuild project 3385996759" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Step 2: Rebuild Project 3161035078
Write-Host "`n[Step 2/4] Rebuilding Project 3161035078..." -ForegroundColor Yellow
$rebuild3161Script = "3161035078/.vscode/rebuild-project.ps1"
& $rebuild3161Script
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to rebuild project 3161035078" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`n✅ All projects built successfully!" -ForegroundColor Green

# Step 3: Copy Both Projects to Steam Workshop
Write-Host "`n[Step 3/4] Running Copy Both Projects to Steam Script..." -ForegroundColor Yellow
$copyBothScript = "tasks/copy-both-to-steam.ps1"
& $copyBothScript

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "🎉 ALL PROJECTS DEPLOYMENT COMPLETED! 🎉" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Both projects have been rebuilt and deployed to Steam Workshop!" -ForegroundColor White
} else {
    Write-Host "❌ Copy to Steam script failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}