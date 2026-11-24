# Rebuild and Deploy Project to Steam
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
Write-Host "Rebuild and Deploy Project $projectId" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Step 1: Run Rebuild Script
Write-Host "`n[Step 1/2] Running Rebuild Script..." -ForegroundColor Yellow
$rebuildScript = Join-Path $PSScriptRoot "rebuild-project.ps1"
& $rebuildScript -ProjectScripts $ProjectScripts

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Rebuild script failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Step 2: Run Copy to Steam Script
Write-Host "`n[Step 2/2] Running Copy to Steam Script..." -ForegroundColor Yellow
$copyScript = Join-Path $PSScriptRoot "copy-to-steam.ps1"
& $copyScript -ProjectScripts $ProjectScripts

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "🎉 DEPLOYMENT COMPLETED SUCCESSFULLY! 🎉" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Project $projectId has been rebuilt and deployed to Steam Workshop!" -ForegroundColor White
} else {
    Write-Host "❌ Copy to Steam script failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}