# Copy Project from Steam Workshop to Git

# Read settings from JSON file
$settingsPath = Join-Path $PSScriptRoot "settings.json"
if (!(Test-Path $settingsPath)) {
    Write-Host "❌ Settings file not found: $settingsPath" -ForegroundColor Red
    exit 1
}

$settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
$projectId = $settings.projectId
$steamWorkshopPath = $settings.steamWorkshopPath
$gitRepositoryPath = $settings.gitRepositoryPath

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Project $projectId from Steam Workshop to Git" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "Steam Workshop Path: $steamWorkshopPath" -ForegroundColor Cyan
Write-Host "Git Repository Path: $gitRepositoryPath" -ForegroundColor Cyan

# Get the root directory and set paths
$sourcePath = Join-Path $steamWorkshopPath $projectId
$destinationPath = Join-Path $gitRepositoryPath $projectId

# Check if source exists
if (!(Test-Path $sourcePath)) {
    Write-Host "❌ Source folder not found: $sourcePath" -ForegroundColor Red
    exit 1
}

# Function to copy with error handling
function Copy-WithErrorHandling {
    param (
        [string]$Source,
        [string]$Destination
    )
    
    # Get all items recursively
    Get-ChildItem -Path $Source -Recurse | ForEach-Object {
        $relativePath = $_.FullName.Substring($Source.Length)
        $targetPath = Join-Path $Destination $relativePath
        
        if ($_.PSIsContainer) {
            # Create directory if it doesn't exist
            if (!(Test-Path $targetPath)) {
                New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
            }
        } else {
            # Copy file with error handling
            try {
                Copy-Item -Path $_.FullName -Destination $targetPath -Force -ErrorAction Stop
            } catch {
                Write-Host "⚠️  Skipped (in use): $($_.Name)" -ForegroundColor Yellow
            }
        }
    }
}

# Copy folder from Steam to Git
Write-Host "`nCopying project $projectId folder from Steam Workshop to Git..." -ForegroundColor Yellow
Copy-WithErrorHandling -Source $sourcePath -Destination $destinationPath

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ COPY COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Project $projectId has been copied from Steam Workshop to Git!" -ForegroundColor White
