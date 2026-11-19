<#
.SYNOPSIS
  Zips the debug/<mod> folder of each project of a release target (GitHub Release, Nexus Mods) + release notes.

.DESCRIPTION
  Each zip is the whole build output folder <ProjectId>/debug/<modName> (DLLs, ModConf, ModImg, SteamImg,
  ModExportData.cache...), as made by the build task. In the CI, which cannot build (no game DLLs), -PrepareResources
  first runs <ProjectId>/scripts/build-project.ps1 -ResourcesOnly: the committed DLLs + the copy of ModConf/ModImg/
  SteamImg (git-ignored in debug/).

  Targets and tags: scripts/_release.ps1 (ModLib-<ver>, Taoist-<ver>, Release-<Taoist ver> = All).
  With -Tag (the CI) the target comes from the tag and the build stops when the tag does not match the version of the
  source code.

.EXAMPLE
  powershell -NoProfile -ExecutionPolicy Bypass -File scripts/package-release.ps1 -Target All
  pwsh ./scripts/package-release.ps1 -Tag Taoist-5.5.45 -PrepareResources -GitHubOutput
#>
param(
    [ValidateSet("ModLib", "Taoist", "All")]
    [string]$Target = "All",
    [string]$Tag,
    [string]$OutDir = "dist",
    [switch]$PrepareResources,
    [switch]$GitHubOutput
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "_release.ps1")
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

if ($Tag) {
    $Target = Get-TargetOfTag $Tag
    if (-not $Target) { throw "'$Tag' is not a release tag (ModLib-<ver>, Taoist-<ver>, $($script:ReleaseCommon.releaseAllTagPrefix)<ver>)" }
}
$release = Get-ReleaseTarget $Target
if ($Tag -and $Tag -ne $release.Tag) {
    throw "Tag $Tag does not match the source code: expected $($release.Tag) ($($release.Title))"
}

$root = $script:ReleaseRoot
if (-not [System.IO.Path]::IsPathRooted($OutDir)) { $OutDir = Join-Path $root $OutDir }
if (Test-Path $OutDir) { Remove-Item $OutDir -Recurse -Force }
New-Item -ItemType Directory -Path $OutDir | Out-Null

# Zip of a folder: entries "<folder name>/..." with '/' (ZipFile.CreateFromDirectory of Windows PowerShell 5.1 writes '\')
function New-FolderZip([string]$Folder, [string]$Zip) {
    $base = (Get-Item $Folder).FullName.TrimEnd('\', '/')
    $parent = Split-Path $base -Parent
    $archive = [System.IO.Compression.ZipFile]::Open($Zip, "Create")
    try {
        # GetFiles returns paths starting with the exact folder string given (no 8.3 / long name mismatch)
        [System.IO.Directory]::GetFiles($base, "*", [System.IO.SearchOption]::AllDirectories) | Sort-Object | ForEach-Object {
            $entry = $_.Substring($parent.Length + 1).Replace('\', '/')
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $_, $entry, [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
        }
    } finally { $archive.Dispose() }
}

$packages = @()
foreach ($p in $release.Projects) {
    Write-Host "[$($p.ProjectId)] $($p.Name) $($p.Version)" -ForegroundColor Cyan
    if ($PrepareResources) {
        & (Join-Path $root "$($p.ProjectId)/scripts/build-project.ps1") -ResourcesOnly
        if ($LASTEXITCODE -ne 0) { throw "build-project.ps1 -ResourcesOnly failed for $($p.ProjectId)" }
    }
    $modDir = Join-Path $root "$($p.ProjectId)/debug/$($p.ModName)"
    foreach ($need in @("ModCode/dll", "ModConf", "ModExportData.cache")) {
        if (-not (Test-Path (Join-Path $modDir $need))) { throw "$need missing in $modDir (run the build task first)" }
    }
    $zip = Join-Path $OutDir "$($p.Name)-$($p.Version).zip"
    New-FolderZip $modDir $zip
    $item = Get-Item $zip
    $packages += [pscustomobject]@{ Project = $p; Zip = $item.Name; Sha256 = (Get-FileHash $zip -Algorithm SHA256).Hash.ToLower(); Size = $item.Length }
    Write-Host ("  -> {0} ({1:N1} MB)" -f $item.Name, ($item.Length / 1MB)) -ForegroundColor Green
}

# Release notes (Nexus links from the env vars NEXUS_<NAME>_URL: repository variables in the CI)
$lines = @("## Downloads", "", "| Mod | Version | File | Steam Workshop | Nexus Mods |", "|---|---|---|---|---|")
foreach ($k in $packages) {
    $nexus = [Environment]::GetEnvironmentVariable("NEXUS_$($k.Project.Name.ToUpper())_URL")
    $nexusCell = if ($nexus) { "[Nexus Mods]($nexus)" } else { "-" }
    $lines += "| $($k.Project.Name) | $($k.Project.Version) | ``$($k.Zip)`` | [Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=$($k.Project.ProjectId)) | $nexusCell |"
}
$lines += @(
    "",
    "## Install",
    "",
    "- **Steam** (recommended): subscribe on the Workshop, updates are automatic.",
    "- **Manual**: close the game, extract the zip and copy its ``Mod_xxx`` folder into the ``ModExportData`` folder of the game (the folder of ``guigubahuang.exe``). If you are also subscribed on the Workshop, unsubscribe: a mod must be loaded only once. Update: delete the old folder first.",
    "- Taoist needs ModLib.",
    "",
    "### SHA-256",
    ""
)
$lines += $packages | ForEach-Object { "- ``$($_.Zip)``: ``$($_.Sha256)``" }
$lines | Set-Content (Join-Path $OutDir "release-notes.md") -Encoding UTF8

Write-Host "Tag: $($release.Tag)  ($($release.Title))" -ForegroundColor Cyan
if ($GitHubOutput -and $env:GITHUB_OUTPUT) {
    $out = @("tag=$($release.Tag)", "title=$($release.Title)")
    foreach ($k in $packages) {
        $n = $k.Project.Name.ToLower()
        $out += "${n}_version=$($k.Project.Version)"
        $out += "${n}_zip=$($k.Zip)"
    }
    $out | Add-Content $env:GITHUB_OUTPUT -Encoding UTF8
}
