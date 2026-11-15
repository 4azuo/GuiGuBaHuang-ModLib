<#
.SYNOPSIS
    Check that everything needed to build the mods is in place.
.DESCRIPTION
    Read-only. Reports PASS / WARN / FAIL for tools, repository, ModCreator,
    settings files, game + MelonLoader and the game paths hardcoded in .csproj.
    Exit code 1 when any check FAILs.
.EXAMPLE
    .\04-verify-environment.ps1
#>

[CmdletBinding()]
param(
    [string]$RepoPath
)

$ErrorActionPreference = 'Continue'
. (Join-Path $PSScriptRoot '_common.ps1')

$checks = New-Object System.Collections.Generic.List[object]
function Add-Check([string]$Status, [string]$Name, [string]$Detail = '', [string]$Fix = '') {
    $checks.Add([pscustomobject]@{ Status = $Status; Check = $Name; Detail = $Detail; Fix = $Fix })
}

Write-Title 'GuiGuBaHuang-ModLib - Verify Environment'
Update-SessionPath

# ---- tools
$netfx = "${env:ProgramFiles(x86)}\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll"
if (Test-Path $netfx) { Add-Check PASS '.NET Framework 4.7.2 Developer Pack' }
else { Add-Check FAIL '.NET Framework 4.7.2 Developer Pack' 'reference assemblies missing' 'choco install netfx-4.7.2-devpack' }

if (Test-Command dotnet) {
    $sdks = @(& dotnet --list-sdks)
    Add-Check PASS 'dotnet CLI' (($sdks | ForEach-Object { ($_ -split ' ')[0] }) -join ', ')
    if ($sdks -match '^9\.') { Add-Check PASS '.NET 9 SDK' }
    else { Add-Check WARN '.NET 9 SDK' 'needed to build ModCreator' 'choco install dotnet-9.0-sdk' }
}
else { Add-Check FAIL 'dotnet CLI' 'not found' 'choco install dotnet-9.0-sdk' }

if (Test-Command git) { Add-Check PASS 'Git' ((& git --version) -replace 'git version ', '') }
else { Add-Check FAIL 'Git' 'not found' 'choco install git' }

$py = (& python --version 2>&1 | Out-String).Trim()
if ($py -match 'Python \d') { Add-Check PASS 'Python' $py }
else { Add-Check WARN 'Python' 'needed for auto-build watcher / Translate' 'choco install python' }

if (Test-Command gh) { Add-Check PASS 'GitHub CLI' } else { Add-Check WARN 'GitHub CLI' 'optional' 'choco install gh' }

# ---- repository
try { $repo = Resolve-RepoPath $RepoPath $PSScriptRoot }
catch { Add-Check FAIL 'Repository' $_.Exception.Message 'pass -RepoPath'; $repo = $null }

if ($repo) {
    Add-Check PASS 'Repository' $repo

    $mcDir = Join-Path $repo 'GuiGuBaHuang-ModCreator'
    if (Test-Path (Join-Path $mcDir 'ModCreator.sln')) { Add-Check PASS 'ModCreator checked out' }
    else { Add-Check FAIL 'ModCreator checked out' "$mcDir is empty" '.\scripts\setup\01-clone-repository.ps1 -RepoPath <repo>' }

    # ---- settings files
    $rootSettingsFile = Join-Path $repo 'scripts\settings.json'
    $files = @($rootSettingsFile)
    try {
        $root = Read-JsonFile $rootSettingsFile
        foreach ($id in $root.projects) { $files += Join-Path $repo "$id\scripts\settings.json" }
    }
    catch { Add-Check FAIL 'scripts\settings.json' 'invalid JSON' }

    foreach ($f in $files) {
        $rel = $f.Substring($repo.Length).TrimStart('\')
        if (-not (Test-Path $f)) { Add-Check FAIL $rel 'file missing'; continue }
        try { $s = Read-JsonFile $f } catch { Add-Check FAIL $rel 'invalid JSON'; continue }
        if (Test-SamePath $s.repositoryPath $repo) { Add-Check PASS "$rel repositoryPath" }
        else { Add-Check FAIL "$rel repositoryPath" "is '$($s.repositoryPath)'" '.\scripts\setup\02-configure-settings.ps1' }
        if ($s.steamWorkshopPath -and (Test-Path $s.steamWorkshopPath)) { Add-Check PASS "$rel steamWorkshopPath" }
        else { Add-Check WARN "$rel steamWorkshopPath" "'$($s.steamWorkshopPath)' does not exist (created on first deploy)" }
    }

    $mcSettings = Join-Path $mcDir 'ModCreator\Resources\settings.json'
    $gamePath = $null
    if (Test-Path $mcSettings) {
        try {
            $mc = Read-JsonFile $mcSettings
            $gamePath = $mc.gameFolderPath
            if (Test-SamePath $mc.rootDir $repo) { Add-Check PASS 'ModCreator settings rootDir' }
            else { Add-Check WARN 'ModCreator settings rootDir' "is '$($mc.rootDir)'" '.\scripts\setup\02-configure-settings.ps1' }
            if ($mc.baseGameResourcesDir -and (Test-Path $mc.baseGameResourcesDir)) { Add-Check PASS 'BaseGameResources' $mc.baseGameResourcesDir }
            else { Add-Check WARN 'BaseGameResources' "'$($mc.baseGameResourcesDir)' not found (only ModCreator needs it)" '.\scripts\setup\03-extract-resources.ps1' }
        }
        catch { Add-Check FAIL 'ModCreator settings' 'invalid JSON' }
    }

    # ---- game + MelonLoader
    if (-not $gamePath) { $g = Find-GameInstall; if ($g) { $gamePath = $g.GamePath } }
    if ($gamePath -and (Test-Path $gamePath)) {
        Add-Check PASS 'Game installed' $gamePath
        if (Test-Path (Join-Path $gamePath 'MelonLoader\Managed\Assembly-CSharp.dll')) { Add-Check PASS 'MelonLoader game assemblies' }
        else { Add-Check FAIL 'MelonLoader game assemblies' 'MelonLoader\Managed\Assembly-CSharp.dll missing' 'install MelonLoader and start the game once' }
    }
    else { Add-Check FAIL 'Game installed' 'game folder not found' 'install the game via Steam' }

    # ---- game paths hardcoded in .csproj HintPath
    $missing = 0; $total = 0; $expected = $null
    foreach ($proj in Get-ChildItem -Path (Join-Path $repo '*\ModProject\ModCode') -Recurse -Filter *.csproj -ErrorAction SilentlyContinue) {
        $text = [IO.File]::ReadAllText($proj.FullName, [Text.Encoding]::UTF8)
        foreach ($m in [regex]::Matches($text, '<HintPath>([A-Za-z]:\\[^<]+)</HintPath>')) {
            $total++
            if (-not (Test-Path $m.Groups[1].Value)) {
                $missing++
                if (-not $expected -and $m.Groups[1].Value -match '^(.+?)\\MelonLoader\\') { $expected = $Matches[1] }
            }
        }
    }
    if ($total -eq 0) { Add-Check WARN '.csproj game references' 'no absolute HintPath found' }
    elseif ($missing -eq 0) { Add-Check PASS '.csproj game references' "$total DLLs found" }
    else {
        $detail = "$missing of $total DLLs missing"
        if ($expected) { $detail += "; .csproj expects game at '$expected'" }
        Add-Check FAIL '.csproj game references' $detail 'see INSTALLATION.md "Game installed in another folder"'
    }

    $modLibProj = Join-Path $repo '3385996759\ModProject\ModCode\ModLib\ModLib.csproj'
    if (Test-Path $modLibProj) {
        $m = [regex]::Match([IO.File]::ReadAllText($modLibProj, [Text.Encoding]::UTF8), '<DocumentationFile>([^<]+)</DocumentationFile>')
        if ($m.Success -and [IO.Path]::IsPathRooted($m.Groups[1].Value) -and -not (Test-Path (Split-Path $m.Groups[1].Value -Parent))) {
            Add-Check WARN 'ModLib.csproj DocumentationFile' "folder of '$($m.Groups[1].Value)' does not exist" 'ModLib.xml for ModCreator is not refreshed on this machine'
        }
    }
}

# ---- report
Write-Host ''
foreach ($c in $checks) {
    $color = switch ($c.Status) { 'PASS' { 'Green' } 'WARN' { 'Yellow' } default { 'Red' } }
    $line = '  [{0}] {1}' -f $c.Status, $c.Check
    if ($c.Detail) { $line += " - $($c.Detail)" }
    Write-Host $line -ForegroundColor $color
    if ($c.Status -ne 'PASS' -and $c.Fix) { Write-Host "         fix: $($c.Fix)" -ForegroundColor DarkGray }
}

$fail = @($checks | Where-Object Status -eq 'FAIL').Count
$warn = @($checks | Where-Object Status -eq 'WARN').Count
Write-Host ''
if ($fail -gt 0) {
    Write-Err "$fail check(s) failed, $warn warning(s)"
    exit 1
}
Write-Ok "All required checks passed ($warn warning(s))"
Write-Info 'Build: powershell -NoProfile -ExecutionPolicy Bypass -File .\3385996759\scripts\build-project.ps1'
exit 0
