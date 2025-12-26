<#
.SYNOPSIS
    Extract BaseGameResources archive
.DESCRIPTION
    This script extracts "BaseGameResources 1.2.111.rar" to "BaseGameResources 1.2.111" folder
    in the GuiGuBaHuang-ModCreator directory.
.PARAMETER Force
    Force extraction even if folder already exists
#>

[CmdletBinding()]
param(
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

function Write-Info($message) {
    Write-ColorOutput Cyan "INFO: $message"
}

function Write-Success($message) {
    Write-ColorOutput Green "SUCCESS: $message"
}

function Write-Warning($message) {
    Write-ColorOutput Yellow "WARNING: $message"
}

function Write-Error($message) {
    Write-ColorOutput Red "ERROR: $message"
}

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Paths
$rarFile = Join-Path $scriptDir "GuiGuBaHuang-ModCreator\BaseGameResources 1.2.111.rar"
$targetFolder = Join-Path $scriptDir "GuiGuBaHuang-ModCreator\BaseGameResources 1.2.111"

Write-Info "=== Extract BaseGameResources Archive ==="
Write-Info ""

# Check if RAR file exists
if (-not (Test-Path $rarFile)) {
    Write-Error "RAR file not found: $rarFile"
    Write-Info ""
    Write-Info "Please make sure the file exists at the expected location."
    exit 1
}

Write-Success "Found RAR file: $rarFile"

# Check if target folder already exists
if (Test-Path $targetFolder) {
    if ($Force) {
        Write-Warning "Target folder already exists: $targetFolder"
        Write-Info "Force parameter specified - removing existing folder..."
        Remove-Item -Path $targetFolder -Recurse -Force
        Write-Success "Existing folder removed"
    }
    else {
        Write-Warning "Target folder already exists: $targetFolder"
        $response = Read-Host "Do you want to remove it and extract again? (y/n)"
        
        if ($response -eq 'y' -or $response -eq 'Y') {
            Write-Info "Removing existing folder..."
            Remove-Item -Path $targetFolder -Recurse -Force
            Write-Success "Existing folder removed"
        }
        else {
            Write-Info "Extraction skipped - using existing folder"
            exit 0
        }
    }
}

# Check for extraction tools
$winrarPath = $null
$sevenZipPath = $null

# Common WinRAR paths
$winrarPaths = @(
    "C:\Program Files\WinRAR\UnRAR.exe",
    "C:\Program Files (x86)\WinRAR\UnRAR.exe",
    "C:\Program Files\WinRAR\WinRAR.exe",
    "C:\Program Files (x86)\WinRAR\WinRAR.exe"
)

foreach ($path in $winrarPaths) {
    if (Test-Path $path) {
        $winrarPath = $path
        break
    }
}

# Common 7-Zip paths
$sevenZipPaths = @(
    "C:\Program Files\7-Zip\7z.exe",
    "C:\Program Files (x86)\7-Zip\7z.exe"
)

foreach ($path in $sevenZipPaths) {
    if (Test-Path $path) {
        $sevenZipPath = $path
        break
    }
}

# Extract the archive
if ($winrarPath) {
    Write-Info "Using WinRAR to extract..."
    Write-Info "Extracting to: $targetFolder"
    Write-Info ""
    
    try {
        # Create target folder
        New-Item -ItemType Directory -Path $targetFolder -Force | Out-Null
        
        # Extract with WinRAR
        $arguments = @(
            "x",                    # Extract with full paths
            "-y",                   # Yes to all
            "`"$rarFile`"",        # Source file
            "`"$targetFolder\`""   # Target folder
        )
        
        $process = Start-Process -FilePath $winrarPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
        
        if ($process.ExitCode -eq 0) {
            Write-Success "Extraction completed successfully"
        }
        else {
            Write-Error "WinRAR extraction failed with exit code: $($process.ExitCode)"
            exit 1
        }
    }
    catch {
        Write-Error "Failed to extract with WinRAR: $_"
        exit 1
    }
}
elseif ($sevenZipPath) {
    Write-Info "Using 7-Zip to extract..."
    Write-Info "Extracting to: $targetFolder"
    Write-Info ""
    
    try {
        # Create target folder
        New-Item -ItemType Directory -Path $targetFolder -Force | Out-Null
        
        # Extract with 7-Zip
        $arguments = @(
            "x",                    # Extract with full paths
            "-y",                   # Yes to all
            "-o`"$targetFolder`"",  # Output directory
            "`"$rarFile`""         # Source file
        )
        
        $process = Start-Process -FilePath $sevenZipPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
        
        if ($process.ExitCode -eq 0) {
            Write-Success "Extraction completed successfully"
        }
        else {
            Write-Error "7-Zip extraction failed with exit code: $($process.ExitCode)"
            exit 1
        }
    }
    catch {
        Write-Error "Failed to extract with 7-Zip: $_"
        exit 1
    }
}
else {
    Write-Error "No extraction tool found!"
    Write-Info ""
    Write-Info "Please install one of the following:"
    Write-Info "  - WinRAR: https://www.win-rar.com/"
    Write-Info "  - 7-Zip: https://www.7-zip.org/"
    Write-Info ""
    Write-Info "After installation, run this script again."
    exit 1
}

# Verify extraction
if (Test-Path $targetFolder) {
    $itemCount = (Get-ChildItem -Path $targetFolder -Recurse).Count
    Write-Info ""
    Write-Info "==============================================================="
    Write-ColorOutput Green "Extraction Complete!"
    Write-Info "==============================================================="
    Write-Info ""
    Write-Success "Extracted to: $targetFolder"
    Write-Info "Total items: $itemCount"
    Write-Info ""
}
else {
    Write-Error "Extraction failed - target folder not created"
    exit 1
}
