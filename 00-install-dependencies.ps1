<#
.SYNOPSIS
    Install development dependencies for GuiGuBaHuang-ModLib project
.DESCRIPTION
    This script installs the following dependencies:
    - .NET Framework 4.7.2 Developer Pack
    - .NET 9 SDK
    - Python (latest version)
    - Git
    - GitHub CLI
    - Notepad++
#>

[CmdletBinding()]
param(
    [switch]$SkipDotNetFramework,
    [switch]$SkipDotNet9,
    [switch]$SkipPython,
    [switch]$SkipGit,
    [switch]$SkipGitHubCLI,
    [switch]$SkipNotepadPlusPlus
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

# Check if running as administrator
function Test-Administrator {
    $user = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($user)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Check if Chocolatey is installed
function Test-Chocolatey {
    try {
        $null = Get-Command choco -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

# Install Chocolatey
function Install-Chocolatey {
    Write-Info "Installing Chocolatey package manager..."
    Set-ExecutionPolicy Bypass -Scope Process -Force
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
    Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
    
    # Refresh environment variables
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
    
    Write-Success "Chocolatey installed successfully"
}

# Check if a package is installed via Chocolatey
function Test-ChocoPackage($packageName) {
    $package = choco list --local-only $packageName --exact --limit-output
    return $package -ne $null -and $package -ne ""
}

# Install .NET Framework 4.7.2 Developer Pack
function Install-DotNetFramework472 {
    Write-Info "Checking .NET Framework 4.7.2..."
    
    # Check if already installed via registry
    $netFx472 = Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" -ErrorAction SilentlyContinue
    if ($netFx472 -and $netFx472.Release -ge 461808) {
        Write-Success ".NET Framework 4.7.2 or later is already installed"
        return
    }
    
    Write-Info "Installing .NET Framework 4.7.2 Developer Pack..."
    choco install netfx-4.7.2-devpack -y
    Write-Success ".NET Framework 4.7.2 Developer Pack installed successfully"
}

# Install .NET 9 SDK
function Install-DotNet9 {
    Write-Info "Checking .NET 9 SDK..."
    
    # Check if dotnet is available
    try {
        $dotnetVersion = dotnet --list-sdks | Select-String "^9\."
        if ($dotnetVersion) {
            Write-Success ".NET 9 SDK is already installed"
            return
        }
    }
    catch {
        Write-Info ".NET SDK not found"
    }
    
    Write-Info "Installing .NET 9 SDK..."
    choco install dotnet-9.0-sdk -y
    
    # Refresh environment variables
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
    
    Write-Success ".NET 9 SDK installed successfully"
}

# Install Python
function Install-Python {
    Write-Info "Checking Python..."
    
    try {
        $pythonVersion = python --version 2>&1
        if ($pythonVersion -match "Python \d+\.\d+\.\d+") {
            Write-Success "Python is already installed: $pythonVersion"
            return
        }
    }
    catch {
        Write-Info "Python not found"
    }
    
    Write-Info "Installing Python..."
    choco install python -y
    
    # Refresh environment variables
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
    
    Write-Success "Python installed successfully"
}

# Install Git
function Install-Git {
    Write-Info "Checking Git..."
    
    try {
        $gitVersion = git --version 2>&1
        if ($gitVersion -match "git version") {
            Write-Success "Git is already installed: $gitVersion"
            return
        }
    }
    catch {
        Write-Info "Git not found"
    }
    
    Write-Info "Installing Git..."
    choco install git -y
    
    # Refresh environment variables
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
    
    Write-Success "Git installed successfully"
}

# Install GitHub CLI
function Install-GitHubCLI {
    Write-Info "Checking GitHub CLI..."
    
    try {
        $ghVersion = gh --version 2>&1
        if ($ghVersion -match "gh version") {
            Write-Success "GitHub CLI is already installed: $($ghVersion | Select-Object -First 1)"
            return
        }
    }
    catch {
        Write-Info "GitHub CLI not found"
    }
    
    Write-Info "Installing GitHub CLI..."
    choco install gh -y
    
    # Refresh environment variables
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
    
    Write-Success "GitHub CLI installed successfully"
}

# Install Notepad++
function Install-NotepadPlusPlus {
    Write-Info "Checking Notepad++..."
    
    try {
        $notepadPath = "C:\Program Files\Notepad++\notepad++.exe"
        $notepadPath86 = "C:\Program Files (x86)\Notepad++\notepad++.exe"
        
        if ((Test-Path $notepadPath) -or (Test-Path $notepadPath86)) {
            Write-Success "Notepad++ is already installed"
            return
        }
    }
    catch {
        Write-Info "Notepad++ not found"
    }
    
    Write-Info "Installing Notepad++..."
    choco install notepadplusplus -y
    
    # Refresh environment variables
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
    
    Write-Success "Notepad++ installed successfully"
}

# Main installation process
function Main {
    Write-Info "=== GuiGuBaHuang-ModLib Dependencies Installer ==="
    Write-Info ""
    
    # Check administrator privileges
    if (-not (Test-Administrator)) {
        Write-Error "This script requires administrator privileges"
        Write-Info "Please run PowerShell as Administrator and try again"
        exit 1
    }
    
    # Install Chocolatey if not present
    if (-not (Test-Chocolatey)) {
        Install-Chocolatey
    }
    else {
        Write-Success "Chocolatey is already installed"
    }
    
    Write-Info ""
    Write-Info "=== Installing Dependencies ==="
    Write-Info ""
    
    # Install .NET Framework 4.7.2
    if (-not $SkipDotNetFramework) {
        Install-DotNetFramework472
    }
    else {
        Write-Info "Skipping .NET Framework 4.7.2 installation"
    }
    
    # Install .NET 9 SDK
    if (-not $SkipDotNet9) {
        Install-DotNet9
    }
    else {
        Write-Info "Skipping .NET 9 SDK installation"
    }
    
    # Install Python
    if (-not $SkipPython) {
        Install-Python
    }
    else {
        Write-Info "Skipping Python installation"
    }
    
    # Install Git
    if (-not $SkipGit) {
        Install-Git
    }
    else {
        Write-Info "Skipping Git installation"
    }
    
    # Install GitHub CLI
    if (-not $SkipGitHubCLI) {
        Install-GitHubCLI
    }
    else {
        Write-Info "Skipping GitHub CLI installation"
    }
    
    # Install Notepad++
    if (-not $SkipNotepadPlusPlus) {
        Install-NotepadPlusPlus
    }
    else {
        Write-Info "Skipping Notepad++ installation"
    }
    
    Write-Info ""
    Write-Success "=== Installation Complete ==="
    Write-Info ""
    Write-Info "Installed components:"
    Write-Info "  - .NET Framework 4.7.2 Developer Pack"
    Write-Info "  - .NET 9 SDK"
    Write-Info "  - Python"
    Write-Info "  - Git"
    Write-Info "  - GitHub CLI"
    Write-Info "  - Notepad++"
    Write-Info ""
    Write-Warning "Please restart your PowerShell session or computer for all changes to take effect"
    Write-Info ""
    Write-Info "You can verify installations with:"
    Write-Info "  dotnet --version"
    Write-Info "  python --version"
    Write-Info "  git --version"
    Write-Info "  gh --version"
}

# Run main function
Main
