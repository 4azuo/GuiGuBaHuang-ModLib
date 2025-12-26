<#
.SYNOPSIS
    Full installation script for GuiGuBaHuang-ModLib
.DESCRIPTION
    This script performs complete setup:
    1. Install all dependencies (00-install-dependencies.ps1)
    2. Clone repository with submodules (01-clone-repository.ps1)
    3. Configure settings (02-configure-settings.ps1 in cloned repo)
.PARAMETER SkipStar
    Skip starring the repository on GitHub
.PARAMETER OpenAll
    Open all configuration files without confirmation
.PARAMETER SkipCheckpoint
    Skip system restore point prompt
#>

[CmdletBinding()]
param(
    [switch]$SkipStar,
    [switch]$OpenAll,
    [switch]$SkipCheckpoint
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

function Write-Step($stepNumber, $stepName) {
    Write-Info ''
    Write-Info '==============================================================='
    Write-ColorOutput Magenta "STEP $stepNumber $stepName"
    Write-Info '==============================================================='
    Write-Info ''
}

# Get script directory
$scriptDir = if ($PSScriptRoot) {
    $PSScriptRoot
}
elseif ($MyInvocation.MyCommand.Path) {
    Split-Path -Parent $MyInvocation.MyCommand.Path
}
else {
    Get-Location | Select-Object -ExpandProperty Path
}

Write-Host "DEBUG: Script directory detected: $scriptDir" -ForegroundColor Gray

# Hardcoded target path
$TargetPath = "C:\git"
$clonedRepoPath = "C:\git\GuiGuBaHuang-ModLib"

Write-Host "DEBUG: Target path: $TargetPath" -ForegroundColor Gray
Write-Host "DEBUG: Repository path: $clonedRepoPath" -ForegroundColor Gray

# Check if running as administrator
$user = [Security.Principal.WindowsIdentity]::GetCurrent()
$principal = New-Object Security.Principal.WindowsPrincipal($user)
$isAdmin = $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Error "This script MUST be run as Administrator"
    Write-Info ""
    Write-Info "Please restart PowerShell with administrator privileges:"
    Write-Info "  1. Right-click PowerShell"
    Write-Info "  2. Select 'Run as Administrator'"
    Write-Info "  3. Run: .\install.ps1"
    Write-Info ""
    exit 1
}

Write-Success "Running with administrator privileges"
Write-Info ""

# Prompt for system checkpoint
if (-not $SkipCheckpoint) {
    Write-Info '==============================================================='
    Write-ColorOutput Yellow 'System Checkpoint Recommendation'
    Write-Info '==============================================================='
    Write-Info ''
    Write-Warning "This script will install software and make system changes."
    Write-Info "It is recommended to create a Windows System Restore Point first."
    Write-Info ""
    
    $response = Read-Host "Do you want to create a System Restore Point now? (y/n)"
    
    if ($response -eq 'y' -or $response -eq 'Y') {
        try {
            Write-Info ""
            Write-Info "Creating System Restore Point..."
            
            $checkpointName = "GuiGuBaHuang-ModLib Installation - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
            
            Enable-ComputerRestore -Drive "$env:SystemDrive\" -ErrorAction SilentlyContinue | Out-Null
            Checkpoint-Computer -Description $checkpointName -RestorePointType "MODIFY_SETTINGS" | Out-Null
            
            Write-Success "System Restore Point created: $checkpointName"
        }
        catch {
            Write-Warning "Failed to create System Restore Point: $_"
            
            $continueResponse = Read-Host "Continue installation anyway? (y/n)"
            if ($continueResponse -ne 'y' -and $continueResponse -ne 'Y') {
                Write-Info "Installation cancelled"
                exit 0
            }
        }
    }
    else {
        Write-Info ""
        $continueResponse = Read-Host "Continue installation without checkpoint? (y/n)"
        if ($continueResponse -ne 'y' -and $continueResponse -ne 'Y') {
            Write-Info "Installation cancelled"
            exit 0
        }
    }
}

# Start installation
Write-Info ''
Write-Info '==============================================================='
Write-ColorOutput Magenta '    GuiGuBaHuang-ModLib Full Installation Script'
Write-Info '==============================================================='
Write-Info ''

$startTime = Get-Date

try {
    # Step 1: Install dependencies
    Write-Step 1 "Installing Dependencies"
    
    $script00 = Join-Path $scriptDir "00-install-dependencies.ps1"
    Write-Host "DEBUG: Looking for: $script00" -ForegroundColor Gray
    
    if (-not (Test-Path $script00)) {
        throw "Cannot find: $script00"
    }
    
    Write-Info "Running: $script00"
    & $script00
    
    Write-Success "Dependencies installation completed"
    
    # Step 2: Clone repository
    Write-Step 2 "Cloning Repository"
    
    # Check if repository already exists
    if (Test-Path $clonedRepoPath) {
        Write-Warning "Repository already exists at: $clonedRepoPath"
        $response = Read-Host "Do you want to skip cloning? (y/n)"
        
        if ($response -eq 'y' -or $response -eq 'Y') {
            Write-Info "Skipping clone step"
            
            # Copy script 02 to existing repo
            $script02Source = Join-Path $scriptDir "02-configure-settings.ps1"
            $script02Dest = Join-Path $clonedRepoPath "02-configure-settings.ps1"
            
            if (Test-Path $script02Source) {
                Write-Info "Copying 02-configure-settings.ps1 to repository..."
                Copy-Item -Path $script02Source -Destination $script02Dest -Force
                Write-Success "Configuration script copied"
            }
        }
        else {
            Write-Warning "Please remove or rename the existing repository first"
            throw "Repository already exists. Cannot proceed with clone."
        }
    }
    else {
        # Clone repository
        $script01 = Join-Path $scriptDir "01-clone-repository.ps1"
        Write-Host "DEBUG: Looking for: $script01" -ForegroundColor Gray
        
        if (-not (Test-Path $script01)) {
            throw "Cannot find: $script01"
        }
        
        Write-Info "Running: $script01"
        
        if ($SkipStar) {
            & $script01 -SkipStar
        }
        else {
            & $script01
        }
        
        Write-Success "Repository clone completed"
    }
    
    # Step 3: Extract resources
    Write-Step 3 "Extracting BaseGameResources"
    
    $script03 = Join-Path $clonedRepoPath "03-extract-resources.ps1"
    Write-Host "DEBUG: Looking for: $script03" -ForegroundColor Gray
    
    if (-not (Test-Path $script03)) {
        Write-Warning "Extract resources script not found: $script03"
        Write-Warning "Skipping extraction step"
    }
    else {
        Write-Info "Running: $script03"
        
        Push-Location $clonedRepoPath
        try {
            & $script03
        }
        catch {
            Write-Warning "Resource extraction failed: $_"
        }
        finally {
            Pop-Location
        }
        
        Write-Success "Resource extraction completed"
    }
    
    # Step 4: Configure settings
    Write-Step 4 "Configuring Settings"
    
    $script02 = Join-Path $clonedRepoPath "02-configure-settings.ps1"
    Write-Host "DEBUG: Looking for: $script02" -ForegroundColor Gray
    
    if (-not (Test-Path $script02)) {
        Write-Warning "Configuration script not found: $script02"
        Write-Warning "Skipping configuration step"
    }
    else {
        Write-Info "Running: $script02"
        
        Push-Location $clonedRepoPath
        try {
            if ($OpenAll) {
                & $script02 -OpenAll
            }
            else {
                & $script02
            }
        }
        catch {
        }
        finally {
            Pop-Location
        }
        
        Write-Success "Configuration completed"
    }
    
    # Installation complete
    $endTime = Get-Date
    $duration = $endTime - $startTime
    $durationStr = "{0:D2}:{1:D2}" -f [int]$duration.TotalMinutes, $duration.Seconds
    
    Write-Info ""
    Write-Info "================================================================="
    Write-ColorOutput Green "   Installation Complete!"
    Write-Info "================================================================="
    Write-Info ""
    Write-Success "Total time: $durationStr"
    Write-Info ""
    Write-Info "Repository location: $clonedRepoPath"
    Write-Info ""
    Write-Info "Next steps:"
    Write-Info "  1. Navigate to repository: cd $clonedRepoPath"
    Write-Info "  2. Read documentation: .github\docs\00-Getting-Started.md"
    Write-Info "  3. Build the project: .\GuiGuBaHuang-ModCreator\build.ps1"
    Write-Info "  4. Start developing your mod!"
    Write-Info ""
    Write-Info "For help and documentation, visit:"
    Write-Info "  https://github.com/4azuo/GuiGuBaHuang-ModLib"
    Write-Info ""
}
catch {
    $errorMsg = $_.Exception.Message
    Write-Error "Installation failed: $errorMsg"
    Write-Info ""
    Write-Info "You can run individual scripts manually:"
    Write-Info "  .\00-install-dependencies.ps1"
    Write-Info "  .\01-clone-repository.ps1"
    Write-Info "  cd C:\git\GuiGuBaHuang-ModLib"
    Write-Info "  .\03-extract-resources.ps1"
    Write-Info "  .\02-configure-settings.ps1"
    exit 1
}