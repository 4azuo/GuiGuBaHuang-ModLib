<#
.SYNOPSIS
    One-shot setup of a clone of GuiGuBaHuang-ModLib (or of the repository given by -RepoPath).
.DESCRIPTION
    Steps (each can be skipped):
      1. 00-install-dependencies.ps1   install tools (needs Administrator)
      2. 02-configure-settings.ps1     write local paths into the settings.json files
      3. 04-verify-environment.ps1     check everything
    A mod repository (GuiGuBaHuang-Taoist...) has its own scripts\setup\install.ps1: it clones the repository with this
    submodule, then runs these steps.
    Keep ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI).
.EXAMPLE
    .\scripts\setup\install.ps1
.EXAMPLE
    # no admin: configure + verify only
    .\scripts\setup\install.ps1 -SkipDependencies -Yes
#>

[CmdletBinding()]
param(
    [string]$RepoPath,
    [switch]$Yes,
    [switch]$SkipDependencies,
    [switch]$SkipConfigure,
    [switch]$SkipVerify,
    [switch]$OpenAll
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')
if ($Yes) { $Global:SetupAssumeYes = $true }
if (-not $RepoPath) { $RepoPath = Get-RepoRootFromScript $PSScriptRoot }
$RepoPath = Resolve-RepoPath $RepoPath $PSScriptRoot

Write-Title "Setup ($(Split-Path $RepoPath -Leaf))"
Write-Info "Repository: $RepoPath"

function Invoke-SetupScript([string]$Name, [hashtable]$Arguments) {
    $global:LASTEXITCODE = 0
    & (Join-Path $PSScriptRoot $Name) @Arguments
    return $LASTEXITCODE
}

$warnings = New-Object System.Collections.Generic.List[string]

Write-Title 'Step 1/3 - Dependencies'
if ($SkipDependencies) { Write-Info 'Skipped (-SkipDependencies)' }
elseif (-not (Test-IsAdmin)) {
    Write-Err 'Installing dependencies needs Administrator: open "Terminal (Admin)", or run again with -SkipDependencies.'
    exit 1
}
else {
    if ((Invoke-SetupScript '00-install-dependencies.ps1' @{}) -ne 0) { Write-Err 'Dependency installation failed'; exit 1 }
    Update-SessionPath
}

Write-Title 'Step 2/3 - Settings'
if ($SkipConfigure) { Write-Info 'Skipped (-SkipConfigure)' }
elseif ((Invoke-SetupScript '02-configure-settings.ps1' @{ RepoPath = $RepoPath; Yes = $Yes; OpenFiles = $OpenAll }) -ne 0) { $warnings.Add('Settings were not configured') }

Write-Title 'Step 3/3 - Verify'
if ($SkipVerify) { Write-Info 'Skipped (-SkipVerify)' }
elseif ((Invoke-SetupScript '04-verify-environment.ps1' @{ RepoPath = $RepoPath }) -ne 0) { $warnings.Add('Verification found problems - see the FAIL lines above') }

Write-Title 'Setup finished'
foreach ($w in $warnings) { Write-Warn $w }
Write-Info 'Build ModLib:  powershell -NoProfile -ExecutionPolicy Bypass -File .\3385996759\scripts\build-project.ps1'
Write-Info 'VS Code: Terminal -> Run Task... (.vscode\tasks.json); docs: docs\README.md, docs\06-Shared-Tools.md'
exit 0
