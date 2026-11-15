<#
.SYNOPSIS
    Clone GuiGuBaHuang-ModLib (and GuiGuBaHuang-ModCreator) to any folder.
.DESCRIPTION
    - Clones the repository to -RepoPath (default C:\git\GuiGuBaHuang-ModLib)
    - If the folder already holds the repository, cloning is skipped
    - Makes sure GuiGuBaHuang-ModCreator is checked out (submodule, or direct clone
      when .gitmodules has no mapping)
    - Optionally stars the repository when GitHub CLI is logged in
.EXAMPLE
    .\01-clone-repository.ps1
.EXAMPLE
    .\01-clone-repository.ps1 -RepoPath D:\Projects\GuiGuBaHuang-ModLib -SkipStar
#>

[CmdletBinding()]
param(
    [string]$RepoPath = 'C:\git\GuiGuBaHuang-ModLib',
    [string]$RepoUrl = 'https://github.com/4azuo/GuiGuBaHuang-ModLib.git',
    [string]$Branch,
    [switch]$SkipStar,
    [switch]$Force,
    [switch]$NoExplorer,
    [switch]$Yes
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')
if ($Yes) { $Global:SetupAssumeYes = $true }

Write-Title 'GuiGuBaHuang-ModLib - Clone Repository'

if (-not (Test-Command git)) {
    Write-Err 'Git is not installed. Run .\00-install-dependencies.ps1 or install Git from https://git-scm.com/'
    exit 1
}

$RepoPath = [IO.Path]::GetFullPath($RepoPath)
Write-Info "Repository URL: $RepoUrl"
Write-Info "Target folder:  $RepoPath"

$ErrorActionPreference = 'Continue'   # git writes progress to stderr

if (Test-RepoRoot $RepoPath) {
    Write-Ok 'Repository already exists here - skipping clone'
}
else {
    if ((Test-Path $RepoPath) -and (Get-ChildItem $RepoPath -Force | Select-Object -First 1)) {
        Write-Warn "Folder exists and is not empty: $RepoPath"
        if ($Force -or (Confirm-Choice 'Delete it and clone fresh?' $false)) {
            Remove-Item -Path $RepoPath -Recurse -Force
        }
        else {
            Write-Err 'Clone cancelled. Choose another folder with -RepoPath.'
            exit 1
        }
    }

    $parent = Split-Path $RepoPath -Parent
    if (-not (Test-Path $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }

    $cloneArgs = @('clone', '--recurse-submodules')
    if ($Branch) { $cloneArgs += @('--branch', $Branch) }
    $cloneArgs += @($RepoUrl, $RepoPath)
    git @cloneArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Err 'git clone failed (check internet connection / URL)'
        exit 1
    }
    Write-Ok "Repository cloned to $RepoPath"
}

if (-not (Initialize-ModCreator -RepoPath $RepoPath)) {
    Write-Warn 'ModCreator is not ready. See INSTALLATION.md, "ModCreator folder is empty".'
}

# Star the repository (only when GitHub CLI is installed and logged in)
if (-not $SkipStar -and (Test-Command gh) -and $RepoUrl -match 'github\.com[/:]([^/]+)/([^/]+?)(\.git)?$') {
    $owner = $Matches[1]; $name = $Matches[2]
    gh auth status *> $null
    if ($LASTEXITCODE -eq 0 -and (Confirm-Choice "Star $owner/$name on GitHub?" $true)) {
        gh api --method PUT "/user/starred/$owner/$name" --silent
        if ($LASTEXITCODE -eq 0) { Write-Ok 'Repository starred - thank you!' }
        else { Write-Warn 'Could not star the repository' }
    }
}

if (-not $NoExplorer -and -not $Global:SetupAssumeYes) {
    Invoke-Item $RepoPath
}

Write-Host ''
Write-Ok "Repository ready: $RepoPath"
Write-Info 'Next: .\scripts\setup\03-extract-resources.ps1 then .\scripts\setup\02-configure-settings.ps1'
exit 0
