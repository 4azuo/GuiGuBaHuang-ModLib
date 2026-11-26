# Copy Both Projects from Steam Workshop to Git
param([string]$RepoRoot)

# Repository: -RepoRoot, else the git repository of the current folder (shared: GuiGuBaHuang-ModLib/scripts)
if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel 2>$null) }
if (-not $RepoRoot) { Write-Host 'Not in a git repository: run from the repository, or pass -RepoRoot' -ForegroundColor Red; exit 1 }
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Both Projects from Steam Workshop to Git" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Read settings of the repository
$settingsPath = Join-Path $RepoRoot "scripts/settings.json"
if (!(Test-Path $settingsPath)) {
    Write-Host "❌ Settings file not found: $settingsPath" -ForegroundColor Red
    exit 1
}

$settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
$steamWorkshopPath = $settings.steamWorkshopPath
$repositoryPath = $RepoRoot
. (Join-Path $PSScriptRoot 'project\_workshop.ps1')

Write-Host "Steam Workshop Path: $steamWorkshopPath" -ForegroundColor Cyan
Write-Host "Git Repository Path: $repositoryPath" -ForegroundColor Cyan

# Process each project
foreach ($projectId in $settings.projects) {
    Write-Host "`n[Project $projectId]" -ForegroundColor Yellow
    
    $source = Join-Path $steamWorkshopPath $projectId
    $dest = Join-Path $repositoryPath $projectId
    
    # Check if source exists
    if (!(Test-Path $source)) {
        Write-Host "❌ Source folder not found: $source" -ForegroundColor Red
        continue
    }

    # The 3 project files into ModProject, the rest into debug\Mod_<id> (layout: project\_workshop.ps1)
    Write-Host "Copying from Steam Workshop to Git..." -ForegroundColor Cyan
    try { $failed = Copy-FromWorkshop $source $dest }
    catch { Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red; continue }
    if ($failed -gt 0) { Write-Host "$failed file(s) not copied (in use)" -ForegroundColor Yellow }
    Write-Host "✅ $projectId copied successfully" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ COPY COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "All projects have been copied from Steam Workshop to Git!" -ForegroundColor White
