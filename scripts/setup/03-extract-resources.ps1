<#
.SYNOPSIS
    Extract the BaseGameResources archive used by ModCreator.
.DESCRIPTION
    The archive (e.g. "BaseGameResources 1.2.111.rar") is NOT stored in git (*.rar is
    ignored), it must be obtained separately. This script looks for it in:
      1. -ArchivePath (if given)
      2. <repo>\GuiGuBaHuang-ModCreator\
      3. <repo>\
      4. %USERPROFILE%\Downloads\ and %USERPROFILE%\Desktop\
    and extracts it to <repo>\GuiGuBaHuang-ModCreator\<archive name> (or -Destination).
    Supports .rar / .7z (WinRAR or 7-Zip) and .zip (built-in).

    Exit codes: 0 = extracted or already present, 1 = error, 2 = archive not found.
.EXAMPLE
    .\03-extract-resources.ps1
.EXAMPLE
    .\03-extract-resources.ps1 -ArchivePath "D:\Downloads\BaseGameResources 1.2.111.rar"
#>

[CmdletBinding()]
param(
    [string]$RepoPath,
    [string]$ArchivePath,
    [string]$Destination,
    [switch]$Force,
    [switch]$Yes
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_common.ps1')
if ($Yes) { $Global:SetupAssumeYes = $true }

Write-Title 'GuiGuBaHuang-ModLib - Extract BaseGameResources'

$repo = Resolve-RepoPath $RepoPath $PSScriptRoot
$modCreatorDir = Join-Path $repo 'GuiGuBaHuang-ModCreator'

if ($ArchivePath) {
    if (-not (Test-Path $ArchivePath)) { Write-Err "Archive not found: $ArchivePath"; exit 1 }
    $ArchivePath = (Resolve-Path $ArchivePath).Path
}
else {
    $ArchivePath = Find-BaseGameResourcesArchive $repo
}

if (-not $ArchivePath) {
    $existing = Find-BaseGameResourcesDir $repo
    if ($existing) {
        Write-Ok "No archive found, but resources are already extracted: $existing"
        exit 0
    }
    Write-Warn 'BaseGameResources archive not found (it is not part of the git repository).'
    Write-Info 'Searched:'
    Get-ResourceSearchDirs $repo | ForEach-Object { Write-Info "  $_" }
    Write-Info 'Get "BaseGameResources <version>.rar" from the project author, put it in one of'
    Write-Info 'the folders above and run this script again (or pass -ArchivePath <file>).'
    Write-Info 'ModCreator still builds without it; only its base-game config browser needs it.'
    exit 2
}
Write-Ok "Archive: $ArchivePath"

if (-not $Destination) {
    $Destination = Join-Path $modCreatorDir ([IO.Path]::GetFileNameWithoutExtension($ArchivePath))
}
$Destination = [IO.Path]::GetFullPath($Destination)

if ((Test-Path $Destination) -and (Get-ChildItem $Destination -Force | Select-Object -First 1)) {
    Write-Warn "Destination already has files: $Destination"
    if ($Force -or (Confirm-Choice 'Delete it and extract again?' $false)) {
        Remove-Item -Path $Destination -Recurse -Force
    }
    else {
        Write-Ok 'Keeping existing folder'
        exit 0
    }
}
New-Item -ItemType Directory -Path $Destination -Force | Out-Null
Write-Info "Extracting to: $Destination"

$ext = [IO.Path]::GetExtension($ArchivePath).ToLowerInvariant()
$ErrorActionPreference = 'Continue'
$code = 0

if ($ext -eq '.zip') {
    try { Expand-Archive -Path $ArchivePath -DestinationPath $Destination -Force } catch { Write-Err $_; $code = 1 }
}
else {
    $pf = $env:ProgramFiles; $pf86 = ${env:ProgramFiles(x86)}
    $sevenZip = @("$pf\7-Zip\7z.exe", "$pf86\7-Zip\7z.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
    if (-not $sevenZip -and (Test-Command 7z)) { $sevenZip = (Get-Command 7z).Source }
    $unrar = @("$pf\WinRAR\UnRAR.exe", "$pf86\WinRAR\UnRAR.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1
    $winrar = @("$pf\WinRAR\WinRAR.exe", "$pf86\WinRAR\WinRAR.exe") | Where-Object { Test-Path $_ } | Select-Object -First 1

    if ($sevenZip) {
        Write-Info "Using 7-Zip: $sevenZip"
        & $sevenZip x -y "-o$Destination" $ArchivePath | Out-Null
        $code = $LASTEXITCODE
    }
    elseif ($unrar -and $ext -eq '.rar') {
        Write-Info "Using UnRAR: $unrar"
        & $unrar x -y $ArchivePath "$Destination\" | Out-Null
        $code = $LASTEXITCODE
    }
    elseif ($winrar) {
        Write-Info "Using WinRAR: $winrar"
        $p = Start-Process -FilePath $winrar -ArgumentList @('x', '-y', '-ibck', "`"$ArchivePath`"", "`"$Destination\`"") -Wait -PassThru
        $code = $p.ExitCode
    }
    else {
        Write-Err 'No extractor found. Install 7-Zip (https://www.7-zip.org/) or WinRAR, then run again.'
        exit 1
    }
}

if ($code -ne 0) {
    Write-Err "Extraction failed (exit code $code)"
    exit 1
}

# If the archive wraps everything in one top folder, move its content up one level
$children = @(Get-ChildItem $Destination -Force)
if ($children.Count -eq 1 -and $children[0].PSIsContainer) {
    $inner = $children[0].FullName
    Get-ChildItem $inner -Force | Move-Item -Destination $Destination
    Remove-Item $inner -Recurse -Force
}

$count = @(Get-ChildItem -Path $Destination -Recurse -File).Count
Write-Ok "Extracted $count files to: $Destination"
Write-Info 'Next: .\scripts\setup\02-configure-settings.ps1 (detects this folder automatically)'
exit 0
