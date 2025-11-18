# Copy to Steam Workshop - Project 3385996759
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Project 3385996759 to Steam Workshop" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Read settings from JSON file
$settingsPath = Join-Path $PSScriptRoot "settings.json"
if (!(Test-Path $settingsPath)) {
    Write-Host "❌ Settings file not found: $settingsPath" -ForegroundColor Red
    exit 1
}

$settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
$steamWorkshopPath = $settings.steamWorkshopPath

Write-Host "Steam Workshop Path: $steamWorkshopPath" -ForegroundColor Cyan

# Create directory if it doesn't exist
if (!(Test-Path $steamWorkshopPath)) {
    Write-Host "Creating directory: $steamWorkshopPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $steamWorkshopPath -Force
} else {
    Write-Host "Directory already exists: $steamWorkshopPath" -ForegroundColor Green
}

# Get the root directory and set source path
$rootDir = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$sourcePath = Join-Path $rootDir "3385996759"

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

# Copy folder
Write-Host "`nCopying 3385996759 folder to Steam Workshop..." -ForegroundColor Yellow
$destination = Join-Path $steamWorkshopPath "3385996759"
if (Test-Path $destination) {
    Remove-Item -Path $destination -Recurse -Force -ErrorAction SilentlyContinue
}
Copy-WithErrorHandling -Source $sourcePath -Destination $destination

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ COPY COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Project 3385996759 has been copied to Steam Workshop!" -ForegroundColor White