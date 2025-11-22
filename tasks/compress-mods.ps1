# Master script to compress both mod folders
# This script calls individual compress scripts from each project

$ErrorActionPreference = "Stop"

# Read task settings
$taskSettings = Get-Content "$PSScriptRoot\settings.json" | ConvertFrom-Json

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "Compressing Mod Folders to RAR" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Compress each project
foreach ($projectId in $taskSettings.projects) {
    Write-Host "[Project $projectId]" -ForegroundColor Cyan
    $compressScript = "$PSScriptRoot\..\$projectId\tasks\compress.ps1"
    
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

