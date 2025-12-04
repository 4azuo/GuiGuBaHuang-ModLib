<#
.SYNOPSIS
    Clone GuiGuBaHuang-ModLib repository with submodules and star it
.DESCRIPTION
    This script performs the following actions:
    - Clones the GuiGuBaHuang-ModLib repository from GitHub to C:\git
    - Initializes and updates all submodules
    - Stars the repository on GitHub
.PARAMETER SkipStar
    Skip starring the repository on GitHub
#>

[CmdletBinding()]
param(
    [switch]$SkipStar
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

# Check if Git is installed
function Test-Git {
    try {
        $null = Get-Command git -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

# Check if GitHub CLI is installed
function Test-GitHubCLI {
    try {
        $null = Get-Command gh -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

# Check if authenticated with GitHub CLI
function Test-GitHubAuth {
    try {
        $authStatus = gh auth status 2>&1
        return $authStatus -match "Logged in"
    }
    catch {
        return $false
    }
}

# Clone repository with submodules
function Copy-Repository {
    param(
        [string]$RepoUrl,
        [string]$TargetPath
    )
    
    Write-Info "Cloning repository: $RepoUrl"
    Write-Info "Target path: $TargetPath"
    
    # Check if target path already exists
    if (Test-Path $TargetPath) {
        Write-Warning "Target path already exists: $TargetPath"
        $response = Read-Host "Do you want to remove it and clone fresh? (y/n)"
        if ($response -eq 'y' -or $response -eq 'Y') {
            Write-Info "Removing existing directory..."
            Remove-Item -Path $TargetPath -Recurse -Force
        }
        else {
            throw "Clone cancelled - target path already exists"
        }
    }
    
    # Clone the repository directly to target path
    git clone --recurse-submodules $RepoUrl $TargetPath
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to clone repository"
    }
    
    Write-Success "Repository cloned successfully to $TargetPath"
    
    # Navigate to repository directory to update submodules
    if (Test-Path $TargetPath) {
        Push-Location $TargetPath
        
        Write-Info "Initializing and updating submodules..."
        git submodule update --init --recursive
        
        if ($LASTEXITCODE -ne 0) {
            Pop-Location
            throw "Failed to update submodules"
        }
        
        Write-Success "Submodules initialized and updated successfully"
        Pop-Location
    }
}

# Star repository on GitHub
function Add-StarRepository {
    param(
        [string]$RepoOwner,
        [string]$RepoName
    )
    
    Write-Info "Starring repository: $RepoOwner/$RepoName"
    
    gh repo star "$RepoOwner/$RepoName" --yes
    
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Failed to star repository. You may need to authenticate with GitHub CLI first."
        Write-Info "Run: gh auth login"
        return $false
    }
    
    Write-Success "Repository starred successfully"
    return $true
}

# Main process
function Main {
    Write-Info "=== GuiGuBaHuang-ModLib Repository Clone Script ==="
    Write-Info ""
    
    # Check if Git is installed
    if (-not (Test-Git)) {
        Write-Error "Git is not installed"
        Write-Info "Please install Git first by running: .\00-install-dependencies.ps1"
        exit 1
    }
    
    Write-Success "Git is installed"
    
    # Check if GitHub CLI is installed
    if (-not (Test-GitHubCLI)) {
        Write-Error "GitHub CLI is not installed"
        Write-Info "Please install GitHub CLI first by running: .\00-install-dependencies.ps1"
        exit 1
    }
    
    Write-Success "GitHub CLI is installed"
    
    # Hardcoded paths
    $repoPath = "C:\git\GuiGuBaHuang-ModLib"
    $repoUrl = "https://github.com/4azuo/GuiGuBaHuang-ModLib.git"
    $repoOwner = "4azuo"
    $repoName = "GuiGuBaHuang-ModLib"
    
    Write-Info "Target path: $repoPath"
    Write-Info "Repository: $repoUrl"
    
    # Clone repository
    try {
        Copy-Repository -RepoUrl $repoUrl -TargetPath $repoPath
    }
    catch {
        Write-Error "Failed to clone repository: $_"
        exit 1
    }
    
    # Check GitHub authentication
    if (Test-GitHubAuth) {
        # Star repository
        Write-Success "Already authenticated with GitHub CLI"
        Add-StarRepository -RepoOwner $repoOwner -RepoName $repoName
    }
    
    Write-Info ""
    Write-Success "=== Clone Process Complete ==="
    
    # Open the cloned repository folder
    Write-Info ""
    Write-Info "Opening repository folder in Explorer..."
    try {
        Invoke-Item $repoPath
        Write-Success "Repository folder opened in Explorer"
    }
    catch {
        Write-Warning "Failed to open folder: $_"
    }
    
    Write-Info ""
    Write-Info "Repository location: $repoPath"
    Write-Info ""
    Write-Info "Next steps:"
    Write-Info "  1. Navigate to the repository: cd $repoPath"
    Write-Info "  2. Configure settings: .\02-configure-settings.ps1"
    Write-Info "  3. Start development!"
}

# Run main function
Main
