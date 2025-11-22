# Copy Both Projects to Steam Workshop
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Copy Both Projects to Steam Workshop" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Read settings from tasks folder
$settingsPath = "$PSScriptRoot/settings.json"
if (!(Test-Path $settingsPath)) {
    Write-Host "❌ Settings file not found: $settingsPath" -ForegroundColor Red
    exit 1
}

$settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
$steamWorkshopPath = $settings.steamWorkshopPath
$gitRepositoryPath = $settings.gitRepositoryPath

Write-Host "Steam Workshop Path: $steamWorkshopPath" -ForegroundColor Cyan
Write-Host "Git Repository Path: $gitRepositoryPath" -ForegroundColor Cyan

# Create directory if it doesn't exist
if (!(Test-Path $steamWorkshopPath)) {
    Write-Host "Creating directory: $steamWorkshopPath" -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $steamWorkshopPath -Force
} else {
    Write-Host "Directory already exists: $steamWorkshopPath" -ForegroundColor Green
}

# Function to copy with exclusions
function Copy-WithExclusions {
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

# Process each project
foreach ($projectId in $settings.projects) {
    Write-Host "`n[Project $projectId]" -ForegroundColor Yellow
    
    $source = Join-Path $gitRepositoryPath $projectId
    $dest = Join-Path $steamWorkshopPath $projectId
    
    # Check if source exists
    if (!(Test-Path $source)) {
        Write-Host "❌ Source folder not found: $source" -ForegroundColor Red
        continue
    }
    
    Write-Host "Copying to Steam Workshop..." -ForegroundColor Cyan
    if (Test-Path $dest) {
        Remove-Item -Path $dest -Recurse -Force -ErrorAction SilentlyContinue
    }
    Copy-WithExclusions -Source $source -Destination $dest
    Write-Host "✅ $projectId copied successfully" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "✅ COPY COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "All projects have been copied to Steam Workshop!" -ForegroundColor White