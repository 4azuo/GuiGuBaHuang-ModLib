# Master script to compress both mod folders
# This script calls individual compress scripts from each project
param([string]$RepoRoot)

# Repository: -RepoRoot, else the git repository of the current folder (shared: GuiGuBaHuang-ModLib/scripts)
if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel 2>$null) }
if (-not $RepoRoot) { Write-Host 'Not in a git repository: run from the repository, or pass -RepoRoot' -ForegroundColor Red; exit 1 }
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path

$ErrorActionPreference = "Stop"

# Read task settings
$taskSettings = Get-Content (Join-Path $RepoRoot "scripts\settings.json") | ConvertFrom-Json

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Compressing Mod Folders to RAR" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Compress each project
foreach ($projectId in $taskSettings.projects) {
    Write-Host "[Project $projectId]" -ForegroundColor Cyan
    $compressScript = Join-Path $RepoRoot "$projectId\scripts\compress.ps1"
    
    if (Test-Path $compressScript) {
        & $compressScript
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to compress project $projectId" -ForegroundColor Red
        }
    } else {
        Write-Host "✗ Compress script not found: $compressScript" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Compression Complete!" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

