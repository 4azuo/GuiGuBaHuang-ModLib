<#
.SYNOPSIS
    One-shot setup for GuiGuBaHuang-ModLib.
.DESCRIPTION
    Steps (each can be skipped):
      1. 00-install-dependencies.ps1   install tools (needs Administrator)
      2. 01-clone-repository.ps1       clone repo + ModCreator (skipped if repo exists)
      3. 03-extract-resources.ps1      extract BaseGameResources (if the archive is found)
      4. 02-configure-settings.ps1     write local paths into settings.json files
      5. 04-verify-environment.ps1     check everything

    Where is the repository?
      - -RepoPath given           -> that folder (cloned there if it does not exist)
      - run from <repo>\scripts\setup -> that repo (no clone)
      - otherwise                 -> C:\git\GuiGuBaHuang-ModLib

    Running from a standalone download: only install.ps1 is required; missing
    helper scripts are downloaded from GitHub next to it.
.EXAMPLE
    .\install.ps1
.EXAMPLE
    .\install.ps1 -RepoPath D:\dev\GuiGuBaHuang-ModLib -Yes
.EXAMPLE
    # inside an existing clone, no admin, just configure + verify
    .\scripts\setup\install.ps1 -SkipDependencies -Yes
#>

[CmdletBinding()]
param(
    [string]$RepoPath,
    [string]$RepoUrl = 'https://github.com/4azuo/GuiGuBaHuang-ModLib.git',
    [string]$ArchivePath,
    [switch]$Yes,
    [switch]$SkipCheckpoint,
    [switch]$SkipDependencies,
    [switch]$SkipStar,
    [switch]$SkipExtract,
    [switch]$SkipConfigure,
    [switch]$SkipVerify,
    [switch]$OpenAll
)

$ErrorActionPreference = 'Stop'
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).Path }

# ---- bootstrap: download helper scripts when install.ps1 was downloaded alone
$rawBase = 'https://raw.githubusercontent.com/4azuo/GuiGuBaHuang-ModLib/main/scripts/setup'
foreach ($name in '_common.ps1', '00-install-dependencies.ps1', '01-clone-repository.ps1') {
    $local = Join-Path $scriptDir $name
    if (-not (Test-Path $local)) {
        Write-Host "Downloading $name ..." -ForegroundColor Cyan
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor 3072
        (New-Object Net.WebClient).DownloadFile("$rawBase/$name", $local)
    }
}
. (Join-Path $scriptDir '_common.ps1')
if ($Yes) { $Global:SetupAssumeYes = $true }

# ---- where is the repo?
$insideRepo = Get-RepoRootFromScript $scriptDir
if (-not $RepoPath) { $RepoPath = if ($insideRepo) { $insideRepo } else { $DefaultRepoPath } }
$RepoPath = [IO.Path]::GetFullPath($RepoPath)

Write-Title 'GuiGuBaHuang-ModLib - Full Installation'
Write-Info "Repository folder: $RepoPath"
if (Test-RepoRoot $RepoPath) { Write-Info '(existing repository - clone step will be skipped)' }

$warnings = New-Object System.Collections.Generic.List[string]
$startTime = Get-Date

function Invoke-SetupScript([string]$Path, [hashtable]$Arguments) {
    if (-not (Test-Path $Path)) { throw "Script not found: $Path" }
    $global:LASTEXITCODE = 0
    & $Path @Arguments
    return $LASTEXITCODE
}

try {
    # ---- admin / restore point
    if (-not $SkipDependencies) {
        if (-not (Test-IsAdmin)) {
            Write-Err 'Installing dependencies needs Administrator.'
            Write-Info 'Either open "Terminal (Admin)" and run again, or install the tools yourself'
            Write-Info 'and run:  .\install.ps1 -SkipDependencies'
            exit 1
        }
        if (-not $SkipCheckpoint -and (Confirm-Choice 'Create a Windows System Restore Point first?' $false)) {
            try {
                Enable-ComputerRestore -Drive "$env:SystemDrive\" -ErrorAction SilentlyContinue | Out-Null
                Checkpoint-Computer -Description "GuiGuBaHuang-ModLib setup $(Get-Date -Format 'yyyy-MM-dd HH:mm')" -RestorePointType MODIFY_SETTINGS
                Write-Ok 'Restore point created'
            }
            catch { $warnings.Add("Restore point not created: $($_.Exception.Message)"); Write-Warn $warnings[-1] }
        }
    }

    # ---- 1. dependencies
    Write-Title 'Step 1/5 - Dependencies'
    if ($SkipDependencies) { Write-Info 'Skipped (-SkipDependencies)' }
    else {
        $code = Invoke-SetupScript (Join-Path $scriptDir '00-install-dependencies.ps1') @{}
        if ($code -ne 0) { throw 'Dependency installation failed' }
        Update-SessionPath
    }

    # ---- 2. repository
    Write-Title 'Step 2/5 - Repository'
    $cloneArgs = @{ RepoPath = $RepoPath; RepoUrl = $RepoUrl; SkipStar = $SkipStar; NoExplorer = $true; Yes = $Yes }
    $cloneScript = Join-Path $scriptDir '01-clone-repository.ps1'
    $code = Invoke-SetupScript $cloneScript $cloneArgs
    if ($code -ne 0) { throw 'Repository step failed' }

    # use the scripts from the repository itself from here on
    $setupDir = Join-Path $RepoPath 'scripts\setup'

    # ---- 3. resources
    Write-Title 'Step 3/5 - BaseGameResources'
    if ($SkipExtract) { Write-Info 'Skipped (-SkipExtract)' }
    else {
        $extractArgs = @{ RepoPath = $RepoPath; Yes = $Yes }
        if ($ArchivePath) { $extractArgs.ArchivePath = $ArchivePath }
        $code = Invoke-SetupScript (Join-Path $setupDir '03-extract-resources.ps1') $extractArgs
        if ($code -eq 2) { $warnings.Add('BaseGameResources archive not found - ModCreator config browser will be empty') }
        elseif ($code -ne 0) { $warnings.Add('BaseGameResources extraction failed') }
    }

    # ---- 4. settings
    Write-Title 'Step 4/5 - Settings'
    if ($SkipConfigure) { Write-Info 'Skipped (-SkipConfigure)' }
    else {
        $code = Invoke-SetupScript (Join-Path $setupDir '02-configure-settings.ps1') @{ RepoPath = $RepoPath; Yes = $Yes; OpenFiles = $OpenAll }
        if ($code -ne 0) { $warnings.Add('Settings were not configured') }
    }

    # ---- 5. verify
    Write-Title 'Step 5/5 - Verify'
    if ($SkipVerify) { Write-Info 'Skipped (-SkipVerify)' }
    else {
        $code = Invoke-SetupScript (Join-Path $setupDir '04-verify-environment.ps1') @{ RepoPath = $RepoPath }
        if ($code -ne 0) { $warnings.Add('Verification found problems - see the FAIL lines above') }
    }
}
catch {
    Write-Err "Installation stopped: $($_.Exception.Message)"
    Write-Info 'Fix the problem and run install.ps1 again (finished steps are detected and skipped),'
    Write-Info 'or run the steps one by one - see INSTALLATION.md.'
    exit 1
}

$duration = (Get-Date) - $startTime
Write-Title 'Installation finished'
Write-Info ('Time: {0:mm\:ss}' -f $duration)
Write-Info "Repository: $RepoPath"
if ($warnings.Count -gt 0) {
    Write-Host ''
    foreach ($w in $warnings) { Write-Warn $w }
}
Write-Host ''
Write-Info 'Next steps:'
Write-Info "  1. Open a NEW terminal, then: cd `"$RepoPath`""
Write-Info '  2. Build ModLib first:  powershell -NoProfile -ExecutionPolicy Bypass -File .\3385996759\scripts\build-project.ps1'
Write-Info '  3. Build Taoist:        powershell -NoProfile -ExecutionPolicy Bypass -File .\3161035078\scripts\build-project.ps1'
Write-Info '  4. Read docs\00-Getting-Started.md'
exit 0
