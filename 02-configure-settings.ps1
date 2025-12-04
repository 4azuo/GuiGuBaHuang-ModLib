<#
.SYNOPSIS
    Configure project settings and open configuration files
.DESCRIPTION
    This script prompts to open important configuration files and folders:
    - 3161035078/.vscode/settings.json (Taoist mod settings)
    - 3385996759/.vscode/settings.json (ModLib settings)
    - GuiGuBaHuang-ModCreator/ModCreator/bin/Release/net9.0-windows/Resources (Resources folder)
.PARAMETER OpenAll
    Open all files without confirmation
.PARAMETER SkipConfirmation
    Skip all confirmations (don't open any files)
#>

[CmdletBinding()]
param(
    [switch]$OpenAll,
    [switch]$SkipConfirmation
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

# Configuration files and folders to open
$configItems = @(
    @{
        Path = "3161035078\.vscode\settings.json"
        Description = "Taoist mod VS Code settings"
        Type = "File"
    },
    @{
        Path = "3385996759\.vscode\settings.json"
        Description = "ModLib VS Code settings"
        Type = "File"
    },
    @{
        Path = "GuiGuBaHuang-ModCreator\ModCreator\bin\Release\net9.0-windows\Resources"
        Description = "ModCreator Resources folder"
        Type = "Folder"
    }
)

# Check if path exists
function Test-ConfigPath {
    param(
        [string]$Path
    )
    
    $fullPath = Join-Path $scriptDir $Path
    return Test-Path $fullPath
}

# Open file or folder
function Open-ConfigItem {
    param(
        [string]$Path,
        [string]$Type
    )
    
    $fullPath = Join-Path $scriptDir $Path
    
    if (-not (Test-Path $fullPath)) {
        Write-Warning "Path not found: $fullPath"
        
        if ($Type -eq "Folder") {
            Write-Info "Creating folder: $fullPath"
            New-Item -ItemType Directory -Path $fullPath -Force | Out-Null
            Write-Success "Folder created"
        }
        else {
            Write-Error "File does not exist: $fullPath"
            return $false
        }
    }
    
    try {
        if ($Type -eq "Folder") {
            Write-Info "Opening folder in Explorer: $fullPath"
            Invoke-Item $fullPath
        }
        else {
            Write-Info "Opening file: $fullPath"
            
            # Try to open with VS Code if available
            try {
                $null = Get-Command code -ErrorAction Stop
                code $fullPath
            }
            catch {
                # Fall back to default editor
                Invoke-Item $fullPath
            }
        }
        
        Write-Success "Opened: $Path"
        return $true
    }
    catch {
        Write-Error "Failed to open: $Path - $_"
        return $false
    }
}

# Ask for confirmation
function Get-Confirmation {
    param(
        [string]$Message
    )
    
    $response = Read-Host "$Message (y/n)"
    return ($response -eq 'y' -or $response -eq 'Y')
}

# Main process
function Main {
    Write-Info "=== GuiGuBaHuang-ModLib Configuration Script ==="
    Write-Info ""
    Write-Info "This script will help you open important configuration files and folders."
    Write-Info ""
    
    if ($SkipConfirmation) {
        Write-Info "Skipping all confirmations (SkipConfirmation parameter specified)"
        return
    }
    
    $openedCount = 0
    $failedCount = 0
    
    foreach ($item in $configItems) {
        $fullPath = Join-Path $scriptDir $item.Path
        $exists = Test-Path $fullPath
        
        Write-Info ""
        Write-Info "─────────────────────────────────────"
        Write-Info "Item: $($item.Description)"
        Write-Info "Type: $($item.Type)"
        Write-Info "Path: $($item.Path)"
        
        if ($exists) {
            Write-Success "Status: Found"
        }
        else {
            Write-Warning "Status: Not found"
            if ($item.Type -eq "Folder") {
                Write-Info "(Will be created if you choose to open)"
            }
        }
        
        Write-Info "─────────────────────────────────────"
        
        $shouldOpen = $false
        
        if ($OpenAll) {
            $shouldOpen = $true
            Write-Info "Opening (OpenAll parameter specified)..."
        }
        else {
            $shouldOpen = Get-Confirmation "Do you want to open this $($item.Type.ToLower())?"
        }
        
        if ($shouldOpen) {
            $result = Open-ConfigItem -Path $item.Path -Type $item.Type
            if ($result) {
                $openedCount++
            }
            else {
                $failedCount++
            }
            
            # Small delay to avoid overwhelming the system
            Start-Sleep -Milliseconds 500
        }
        else {
            Write-Info "Skipped"
        }
    }
    
    Write-Info ""
    Write-Info "═════════════════════════════════════"
    Write-Success "Configuration Complete"
    Write-Info "═════════════════════════════════════"
    Write-Info ""
    Write-Info "Summary:"
    Write-Info "  Opened: $openedCount"
    Write-Info "  Failed: $failedCount"
    Write-Info "  Skipped: $($configItems.Count - $openedCount - $failedCount)"
    Write-Info ""
    
    if ($failedCount -gt 0) {
        Write-Warning "Some items failed to open. Please check the paths manually."
    }
    
    Write-Info "Next steps:"
    Write-Info "  1. Review and update the VS Code settings files"
    Write-Info "  2. Check the ModCreator Resources folder"
    Write-Info "  3. Run the build script: .\GuiGuBaHuang-ModCreator\build.ps1"
    Write-Info ""
    Write-Info "For more information, see:"
    Write-Info "  - .github/docs/00-Getting-Started.md"
    Write-Info "  - .github/docs/07-Build-Deploy.md"
}

# Run main function
Main
