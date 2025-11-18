# Copy Both Projects from Steam Workshop to Git
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Both Projects from Steam Workshop to Git" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Read settings from JSON file (try multiple locations)
$settingsPath = "tasks/settings.json"
if (!(Test-Path $settingsPath)) {
    $settingsPath = "3161035078/.vscode/settings.json"
    if (!(Test-Path $settingsPath)) {
        $settingsPath = "3385996759/.vscode/settings.json"
        if (!(Test-Path $settingsPath)) {
            Write-Host "❌ Settings file not found in any expected location!" -ForegroundColor Red
            exit 1
        }
    }
}

$settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
$steamWorkshopPath = $settings.steamWorkshopPath
$gitRepositoryPath = if ($settings.gitRepositoryPath) { $settings.gitRepositoryPath } else { Get-Location }

Write-Host "Steam Workshop Path: $steamWorkshopPath" -ForegroundColor Cyan
Write-Host "Git Repository Path: $gitRepositoryPath" -ForegroundColor Cyan

# Get paths
$source3161 = Join-Path $steamWorkshopPath "3161035078"
$source3385 = Join-Path $steamWorkshopPath "3385996759"
$dest3161 = Join-Path $gitRepositoryPath "3161035078"
$dest3385 = Join-Path $gitRepositoryPath "3385996759"

# Check if sources exist
if (!(Test-Path $source3161)) {
    Write-Host "❌ Source folder not found: $source3161" -ForegroundColor Red
    exit 1
}
if (!(Test-Path $source3385)) {
    Write-Host "❌ Source folder not found: $source3385" -ForegroundColor Red
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

# Copy 3161035078 folder
Write-Host "`nCopying 3161035078 folder from Steam Workshop to Git..." -ForegroundColor Cyan
Copy-WithErrorHandling -Source $source3161 -Destination $dest3161
Write-Host "✅ 3161035078 copied successfully" -ForegroundColor Green

# Copy 3385996759 folder
Write-Host "Copying 3385996759 folder from Steam Workshop to Git..." -ForegroundColor Cyan
Copy-WithErrorHandling -Source $source3385 -Destination $dest3385
Write-Host "✅ 3385996759 copied successfully" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ COPY COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Both projects have been copied from Steam Workshop to Git!" -ForegroundColor White
