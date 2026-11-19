# Shared by package-release.ps1, create-release-tag.ps1 (dot-sourced).
#
# Release targets and tags (versions read from the source code: versionFile / versionPattern of
# <ProjectId>/scripts/settings.json):
#   ModLib -> ModLib-<ModLib version>      (zip: ModLib-<version>.zip)
#   Taoist -> Taoist-<Taoist version>      (zip: Taoist-<version>.zip)
#   All    -> Release-<Taoist version>     (both zips; scripts/settings.json releaseAllTagPrefix / releaseAllVersionProject)

$script:ReleaseRoot = Split-Path $PSScriptRoot -Parent
$script:ReleaseCommon = Get-Content (Join-Path $PSScriptRoot "settings.json") -Raw | ConvertFrom-Json

# Every project of scripts/settings.json: ProjectId, Name (releaseName), ModName, Version
function Get-ReleaseProjects {
    foreach ($projectId in $script:ReleaseCommon.projects) {
        $s = Get-Content (Join-Path $script:ReleaseRoot "$projectId/scripts/settings.json") -Raw | ConvertFrom-Json
        $file = Join-Path $script:ReleaseRoot (Join-Path $projectId $s.versionFile)
        $m = [regex]::Match((Get-Content $file -Raw), $s.versionPattern)
        if (-not $m.Success) { throw "Version not found in $file (pattern $($s.versionPattern))" }
        [pscustomobject]@{ ProjectId = $projectId; Name = $s.releaseName; ModName = $s.modName; Version = $m.Groups[1].Value }
    }
}

# Target (ModLib / Taoist / All) -> Projects, Tag, Title
function Get-ReleaseTarget([string]$Target) {
    $all = @(Get-ReleaseProjects)
    if ($Target -eq "All") {
        $main = $all | Where-Object { $_.ProjectId -eq $script:ReleaseCommon.releaseAllVersionProject } | Select-Object -First 1
        $projects = $all
        $tag = "$($script:ReleaseCommon.releaseAllTagPrefix)$($main.Version)"
    } else {
        $projects = @($all | Where-Object { $_.Name -eq $Target })
        if ($projects.Count -eq 0) { throw "Unknown target '$Target' (ModLib, Taoist, All)" }
        $tag = "$($projects[0].Name)-$($projects[0].Version)"
    }
    [pscustomobject]@{
        Target   = $Target
        Projects = $projects
        Tag      = $tag
        Title    = ($projects | ForEach-Object { "$($_.Name) $($_.Version)" }) -join " / "
    }
}

# Tag pushed by create-release-tag.ps1 -> target; $null when it is not a release tag
function Get-TargetOfTag([string]$Tag) {
    if ($Tag.StartsWith($script:ReleaseCommon.releaseAllTagPrefix)) { return "All" }
    foreach ($p in Get-ReleaseProjects) {
        if ($Tag.StartsWith("$($p.Name)-")) { return $p.Name }
    }
    return $null
}
