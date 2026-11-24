# Shared by the mods using ModLib (GuiGuBaHuang-ModLib/scripts/project): run through the wrapper of the same name in
# <ProjectId>/scripts (-ProjectScripts = that folder: settings.json of the project, auto-built-errors.log).
# Build Project - Release
# -ResourcesOnly: no dotnet build, only the copy of ModConf/ModImg/SteamImg to debug/ (the CI, which cannot build:
# debug/<mod>/ModCode/dll is committed, the copied folders are git-ignored)
param([Parameter(Mandatory = $true)][string]$ProjectScripts, [switch]$ResourcesOnly)

# Read settings from JSON file
$settingsPath = Join-Path $ProjectScripts "settings.json"
if (!(Test-Path $settingsPath)) {
    Write-Host "❌ Settings file not found: $settingsPath" -ForegroundColor Red
    exit 1
}

$settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
$projectId = $settings.projectId

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Building Project $projectId - Release" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Root of the mod repository (parent of the project folder)
$rootDir = Split-Path (Split-Path $ProjectScripts -Parent) -Parent
Push-Location $rootDir

if ($ResourcesOnly) {
    Write-Host "Resources only (no build)" -ForegroundColor Yellow
    $global:LASTEXITCODE = 0
} else {
    Write-Host "Building solution..." -ForegroundColor Yellow
    dotnet build "$projectId/ModProject/ModCode/ModMain/ModMain.sln" --configuration Release
}

Pop-Location

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Project built successfully!" -ForegroundColor Green
    
    # Copy ModConf, ModImg, and SteamImg to debug folder
    Write-Host "`nCopying resources to debug folder..." -ForegroundColor Yellow
    $modName = $settings.modName
    $projectPath = Join-Path $rootDir "$projectId/ModProject"
    $debugPath = Join-Path $rootDir "$projectId/debug/$modName"
    
    # Ensure debug folder exists
    if (!(Test-Path $debugPath)) {
        New-Item -ItemType Directory -Path $debugPath -Force | Out-Null
    }
    
    # Copy ModConf folder
    $modConfSource = Join-Path $projectPath "ModConf"
    $modConfDest = Join-Path $debugPath "ModConf"
    if (Test-Path $modConfSource) {
        if (Test-Path $modConfDest) {
            Remove-Item -Path $modConfDest -Recurse -Force
        }
        Copy-Item -Path $modConfSource -Destination $modConfDest -Recurse -Force
        Write-Host "  ✓ Copied ModConf" -ForegroundColor Green
    }
    
    # Copy ModImg folder
    $modImgSource = Join-Path $projectPath "ModImg"
    $modImgDest = Join-Path $debugPath "ModImg"
    if (Test-Path $modImgSource) {
        if (Test-Path $modImgDest) {
            Remove-Item -Path $modImgDest -Recurse -Force
        }
        Copy-Item -Path $modImgSource -Destination $modImgDest -Recurse -Force
        Write-Host "  ✓ Copied ModImg" -ForegroundColor Green
    }
    
    # Copy SteamImg folder
    $steamImgSource = Join-Path $projectPath "SteamImg"
    $steamImgDest = Join-Path $debugPath "SteamImg"
    if (Test-Path $steamImgSource) {
        if (Test-Path $steamImgDest) {
            Remove-Item -Path $steamImgDest -Recurse -Force
        }
        Copy-Item -Path $steamImgSource -Destination $steamImgDest -Recurse -Force
        Write-Host "  ✓ Copied SteamImg" -ForegroundColor Green
    }
    
    Write-Host "✅ Resources copied successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to build project!" -ForegroundColor Red
    exit $LASTEXITCODE
}