<#
.SYNOPSIS
    Install the development tools of ModLib and the mods (requires Administrator; shared: GuiGuBaHuang-ModLib/scripts/setup).
.DESCRIPTION
    Installs via Chocolatey only what is missing (each tool is detected first,
    so re-running is safe):
      - .NET Framework 4.7.2 Developer Pack  (build ModLib and mods)
      - .NET 9 SDK                            (dotnet CLI, build ModCreator)
      - Git                                   (clone repository)
      - Python                                (auto-build watcher, Translate tool)
      - GitHub CLI                            (optional)
      - Notepad++                             (optional editor)
      - 7-Zip                                 (only when neither WinRAR nor 7-Zip exists)
.EXAMPLE
    .\00-install-dependencies.ps1
.EXAMPLE
    .\00-install-dependencies.ps1 -SkipGitHubCLI -SkipNotepadPlusPlus
#>

[CmdletBinding()]
param(
    [switch]$SkipDotNetFramework,
    [switch]$SkipDotNet9,
    [switch]$SkipPython,
    [switch]$SkipGit,
    [switch]$SkipGitHubCLI,
    [switch]$SkipNotepadPlusPlus,
    [switch]$Skip7Zip
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')

function Test-Quiet([scriptblock]$Check) {
    $ErrorActionPreference = 'Continue'
    try { return [bool](& $Check) } catch { return $false }
}

$tools = @(
    @{ Name = '.NET Framework 4.7.2 Developer Pack'; Package = 'netfx-4.7.2-devpack'; Skip = $SkipDotNetFramework
       Check = { Test-Path "${env:ProgramFiles(x86)}\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll" } }
    @{ Name = '.NET 9 SDK'; Package = 'dotnet-9.0-sdk'; Skip = $SkipDotNet9
       Check = { (Test-Command dotnet) -and (& dotnet --list-sdks | Where-Object { $_ -match '^9\.' }) } }
    @{ Name = 'Git'; Package = 'git'; Skip = $SkipGit
       Check = { Test-Command git } }
    @{ Name = 'Python'; Package = 'python'; Skip = $SkipPython
       # "python" may be the Microsoft Store stub, so check that it really runs
       Check = { (& python --version 2>&1 | Out-String) -match 'Python \d+\.\d+' } }
    @{ Name = 'GitHub CLI'; Package = 'gh'; Skip = $SkipGitHubCLI
       Check = { Test-Command gh } }
    @{ Name = 'Notepad++'; Package = 'notepadplusplus'; Skip = $SkipNotepadPlusPlus
       Check = { (Test-Path "$env:ProgramFiles\Notepad++\notepad++.exe") -or (Test-Path "${env:ProgramFiles(x86)}\Notepad++\notepad++.exe") } }
    @{ Name = '7-Zip (archive extractor)'; Package = '7zip'; Skip = $Skip7Zip
       Check = { (Test-Path "$env:ProgramFiles\7-Zip\7z.exe") -or (Test-Path "$env:ProgramFiles\WinRAR\WinRAR.exe") -or
                 (Test-Path "${env:ProgramFiles(x86)}\7-Zip\7z.exe") -or (Test-Path "${env:ProgramFiles(x86)}\WinRAR\WinRAR.exe") } }
)

Write-Title 'Install Dependencies (ModLib and mods)'

if (-not (Test-IsAdmin)) {
    Write-Err 'This script needs Administrator privileges (Chocolatey installs system-wide).'
    Write-Info 'Open "Terminal (Admin)" (Win + X) and run it again,'
    Write-Info 'or install the tools manually - see INSTALLATION.md, section "Manual installation".'
    exit 1
}

if (-not (Test-Command choco)) {
    Write-Info 'Installing Chocolatey package manager...'
    Set-ExecutionPolicy Bypass -Scope Process -Force
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor 3072
    Invoke-Expression ((New-Object Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
    Update-SessionPath
    if (-not (Test-Command choco)) {
        Write-Err 'Chocolatey installation failed. Install the tools manually (see INSTALLATION.md).'
        exit 1
    }
    Write-Ok 'Chocolatey installed'
}
else {
    Write-Ok 'Chocolatey is already installed'
}

$results = @()
foreach ($tool in $tools) {
    Write-Host ''
    if ($tool.Skip) {
        Write-Info "Skipping $($tool.Name)"
        $results += [pscustomobject]@{ Tool = $tool.Name; Status = 'skipped' }
        continue
    }
    if (Test-Quiet $tool.Check) {
        Write-Ok "$($tool.Name) is already installed"
        $results += [pscustomobject]@{ Tool = $tool.Name; Status = 'already installed' }
        continue
    }

    Write-Info "Installing $($tool.Name) (choco install $($tool.Package))..."
    $ErrorActionPreference = 'Continue'
    choco install $tool.Package -y --no-progress
    $code = $LASTEXITCODE
    $ErrorActionPreference = 'Stop'
    Update-SessionPath

    # 3010 / 1641 = installed, reboot required
    if ($code -in 0, 3010, 1641) {
        Write-Ok "$($tool.Name) installed"
        $status = if ($code -eq 0) { 'installed' } else { 'installed (reboot required)' }
    }
    else {
        Write-Err "$($tool.Name) failed (choco exit code $code)"
        $status = "FAILED ($code)"
    }
    $results += [pscustomobject]@{ Tool = $tool.Name; Status = $status }
}

Write-Title 'Summary'
$results | Format-Table -AutoSize | Out-String | Write-Host

if ($results | Where-Object { $_.Status -like 'FAILED*' }) {
    Write-Err 'Some tools failed to install. Re-run this script, or install them manually (INSTALLATION.md).'
    exit 1
}

Write-Warn 'Open a NEW terminal afterwards so PATH picks up the new tools.'
exit 0
