<#
.SYNOPSIS
    Configure project settings and open configuration files
.DESCRIPTION
    This script prompts to open important configuration files and folders:
    - 3161035078/tasks/settings.json (Taoist mod settings)
    - 3385996759/tasks/settings.json (ModLib settings)
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
        Path = "3161035078\tasks\settings.json"
        Description = "Taoist mod VS Code settings"
        Type = "File"
    },
    @{
        Path = "3385996759\tasks\settings.json"
        Description = "ModLib VS Code settings"
        Type = "File"
    },
    @{
        Path = "GuiGuBaHuang-ModCreator\ModCreator\Resources\settings.json"
        Description = "ModCreator Resources settings"
        Type = "File"
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
            Write-Success "Opened folder: $Path"
            Write-Warning "Please review the folder contents and press any key to continue..."
            $null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
        }
        else {
            Write-Info "Opening file in Notepad: $fullPath"
            Write-ColorOutput Yellow "IMPORTANT: Please configure this file manually"
            Write-ColorOutput Yellow "The script will wait until you close Notepad to continue"
            Write-Info ''
            
            # Open with notepad and wait for it to close
            $process = Start-Process notepad.exe -ArgumentList $fullPath -PassThru -Wait
            
            Write-Success "File closed: $Path"
        }
        
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
    Write-Info "This script will help you configure important settings files."
    Write-ColorOutput Yellow "You MUST manually configure each file before continuing."
    Write-ColorOutput Yellow "Files will open in Notepad - the script waits until you close each file."
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
        Write-Info "==============================================================="
        Write-ColorOutput Magenta "Configuration Item: $($item.Description)"
        Write-Info "==============================================================="
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
        
        Write-Info "---------------------------------------------------------------"
        
        $shouldOpen = $false
        
        if ($OpenAll) {
            $shouldOpen = $true
            Write-Info "Opening automatically (OpenAll parameter specified)..."
        }
        else {
            $response = Read-Host "Do you want to configure this $($item.Type.ToLower())? (y/n)"
            $shouldOpen = ($response -eq 'y' -or $response -eq 'Y')
        }
        
        if ($shouldOpen) {
            $result = Open-ConfigItem -Path $item.Path -Type $item.Type
            if ($result) {
                $openedCount++
            }
            else {
                $failedCount++
            }
        }
        else {
            Write-Warning "Skipped - You can configure this later manually"
        }
    }
    
    Write-Info ""
    Write-Info "==============================================================="
    Write-Success "Configuration Complete"
    Write-Info "==============================================================="
    Write-Info ""
    Write-Info "Summary:"
    Write-Info "  Configured: $openedCount"
    Write-Info "  Failed: $failedCount"
    Write-Info "  Skipped: $($configItems.Count - $openedCount - $failedCount)"
    Write-Info ""
    
    if ($failedCount -gt 0) {
        Write-Warning "Some items failed to open. Please check the paths manually."
    }
    
    if (($configItems.Count - $openedCount - $failedCount) -gt 0) {
        Write-Warning "Some items were skipped. Please configure them manually later."
    }
    
    Write-Info ""
    Write-Info "Next steps:"
    Write-Info "  1. Build the projects: cd tasks ; .\rebuild-and-deploy-all.ps1"
    Write-Info "  2. Or build individually:"
    Write-Info "     - Taoist mod: cd 3161035078\tasks ; .\rebuild-and-deploy.ps1"
    Write-Info "     - ModLib: cd 3385996759\tasks ; .\rebuild-and-deploy.ps1"
    Write-Info ""
    Write-Info "For more information, see:"
    Write-Info "  - .github/docs/00-Getting-Started.md"
    Write-Info "  - .github/docs/07-Build-Deploy.md"
}

# Run main function
Main
