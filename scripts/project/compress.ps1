# Compress the mod folder (debug/<modName>) of the project to a RAR
# Shared by the mods using ModLib (GuiGuBaHuang-ModLib/scripts/project): run through the wrapper of the same name in
# <ProjectId>/scripts (-ProjectScripts = that folder: settings.json of the project, auto-built-errors.log).
param([Parameter(Mandatory = $true)][string]$ProjectScripts)
$ErrorActionPreference = "Stop"

# Read settings
$settings = Get-Content "$ProjectScripts\settings.json" | ConvertFrom-Json

# Define paths
$projectId = $settings.projectId
$modName = $settings.modName
$projectRoot = Split-Path $ProjectScripts -Parent
$sourcePath = Join-Path $projectRoot "debug\$modName"
$targetPath = Join-Path $projectRoot "debug\$modName.rar"

# Check if WinRAR is installed
$winrarPath = "C:\Program Files\WinRAR\WinRAR.exe"
if (-not (Test-Path $winrarPath)) {
    $winrarPath = "C:\Program Files (x86)\WinRAR\WinRAR.exe"
    if (-not (Test-Path $winrarPath)) {
        Write-Host "ERROR: WinRAR not found. Please install WinRAR." -ForegroundColor Red
        exit 1
    }
}

Write-Host "Project: $projectId" -ForegroundColor Yellow
Write-Host "Source:  $sourcePath" -ForegroundColor Gray
Write-Host "Target:  $targetPath" -ForegroundColor Gray

if (Test-Path $sourcePath) {
    # Remove existing RAR if exists
    if (Test-Path $targetPath) {
        Remove-Item $targetPath -Force
        Write-Host "Removed existing RAR file" -ForegroundColor DarkGray
    }
    
    # Compress to RAR
    $arguments = @(
        "a",                    # Add to archive
        "-r",                   # Recurse subdirectories
        "-ep1",                 # Exclude base folder from paths
        "-m5",                  # Compression level (maximum)
        "`"$targetPath`"",      # Target archive
        "`"$sourcePath\*`""     # Source files
    )
    
    & $winrarPath $arguments | Out-Null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Successfully compressed $modName" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "✗ Failed to compress $modName" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✗ Source folder not found: $sourcePath" -ForegroundColor Red
    exit 1
}
