# Rebuild and Deploy All Projects to Steam
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Rebuild and Deploy All Projects to Steam" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Read settings from scripts folder
$settingsPath = "$PSScriptRoot/settings.json"
if (!(Test-Path $settingsPath)) {
    Write-Host "❌ Settings file not found: $settingsPath" -ForegroundColor Red
    exit 1
}

$settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
$repositoryPath = $settings.repositoryPath

$stepNum = 1
$totalSteps = $settings.projects.Count + 1

# Rebuild each project
foreach ($projectId in $settings.projects) {
    Write-Host "`n[Step $stepNum/$totalSteps] Rebuilding Project $projectId..." -ForegroundColor Yellow
    $rebuildScript = Join-Path $repositoryPath "$projectId\scripts\rebuild-project.ps1"
    
    if (!(Test-Path $rebuildScript)) {
        Write-Host "❌ Rebuild script not found: $rebuildScript" -ForegroundColor Red
        exit 1
    }
    
    & $rebuildScript
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to rebuild project $projectId" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    $stepNum++
}

Write-Host "`n✅ All projects built successfully!" -ForegroundColor Green

# Copy all projects to Steam Workshop
Write-Host "`n[Step $stepNum/$totalSteps] Running Copy All Projects to Steam Script..." -ForegroundColor Yellow
$copyScript = "$PSScriptRoot\copy-both-to-steam.ps1"
& $copyScript

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "🎉 ALL PROJECTS DEPLOYMENT COMPLETED! 🎉" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Both projects have been rebuilt and deployed to Steam Workshop!" -ForegroundColor White
} else {
    Write-Host "❌ Copy to Steam script failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}