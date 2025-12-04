<#
.SYNOPSIS
    Update ModLib repository and all submodules
.DESCRIPTION
    This script fetches and pulls the latest changes from the main repository
    and all its submodules.
.PARAMETER Force
    Force update even if there are local changes (stash them first)
.PARAMETER NoSubmodules
    Skip updating submodules
.EXAMPLE
    .\update-repo.ps1
    Updates repository and all submodules
.EXAMPLE
    .\update-repo.ps1 -Force
    Stashes local changes and updates everything
#>

[CmdletBinding()]
param(
    [switch]$Force,
    [switch]$NoSubmodules
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

function Write-ErrorMsg($message) {
    Write-ColorOutput Red "ERROR: $message"
}

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Info "Starting repository update..."
Write-Info "Repository: $ScriptDir"

# Check if git is available
try {
    $gitVersion = git --version
    Write-Info "Git version: $gitVersion"
} catch {
    Write-ErrorMsg "Git is not installed or not in PATH"
    exit 1
}

# Check for local changes
Write-Info "Checking for local changes..."
$hasChanges = $false

$status = git status --porcelain
if ($status) {
    $hasChanges = $true
    Write-Warning "Local changes detected:"
    Write-Output $status
    
    if ($Force) {
        Write-Info "Stashing local changes..."
        git stash push -m "Auto-stash before update $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        Write-Success "Changes stashed"
    } else {
        Write-ErrorMsg "Local changes detected. Please commit or stash them, or use -Force to auto-stash."
        Write-Info "You can stash changes manually with: git stash"
        Write-Info "Or force update with: .\update-repo.ps1 -Force"
        exit 1
    }
}

# Fetch from origin
Write-Info "Fetching from origin..."
try {
    git fetch --all --prune
    Write-Success "Fetch completed"
} catch {
    Write-ErrorMsg "Failed to fetch from origin: $_"
    exit 1
}

# Get current branch
$currentBranch = git rev-parse --abbrev-ref HEAD
Write-Info "Current branch: $currentBranch"

# Pull latest changes
Write-Info "Pulling latest changes..."
try {
    $pullOutput = git pull origin $currentBranch
    Write-Output $pullOutput
    
    if ($pullOutput -match "Already up to date") {
        Write-Success "Repository is already up to date"
    } else {
        Write-Success "Repository updated successfully"
    }
} catch {
    Write-ErrorMsg "Failed to pull changes: $_"
    
    if ($Force -and $hasChanges) {
        Write-Info "Restoring stashed changes..."
        git stash pop
    }
    
    exit 1
}

# Update submodules
if (-not $NoSubmodules) {
    Write-Info "Updating submodules..."
    
    # Check if there are any submodules
    $submodules = git submodule status
    if ($submodules) {
        Write-Info "Found submodules:"
        Write-Output $submodules
        
        try {
            # Initialize submodules if not already done
            Write-Info "Initializing submodules..."
            git submodule init
            
            # Update submodules
            Write-Info "Fetching and updating submodules..."
            git submodule update --remote --merge
            
            Write-Success "Submodules updated successfully"
        } catch {
            Write-ErrorMsg "Failed to update submodules: $_"
            
            if ($Force -and $hasChanges) {
                Write-Info "Restoring stashed changes..."
                git stash pop
            }
            
            exit 1
        }
    } else {
        Write-Info "No submodules found"
    }
} else {
    Write-Info "Skipping submodule update (NoSubmodules flag)"
}

# Restore stashed changes if any
if ($Force -and $hasChanges) {
    Write-Info "Restoring stashed changes..."
    try {
        git stash pop
        Write-Success "Stashed changes restored"
    } catch {
        Write-Warning "Failed to restore stashed changes. You can restore them manually with: git stash pop"
    }
}

# Show final status
Write-Info "Final status:"
$finalStatus = git status --short
if ($finalStatus) {
    Write-Output $finalStatus
} else {
    Write-Success "Working tree clean"
}

Write-Success "Repository update completed successfully!"
Write-Info "You may need to rebuild projects if there were code changes."
