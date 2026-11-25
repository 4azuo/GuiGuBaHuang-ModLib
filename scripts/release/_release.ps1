# Shared by package-release.ps1, create-release-tag.ps1 (dot-sourced). Shared by the repositories using ModLib: each
# repository releases its own projects (ModLib: 3385996759 in GuiGuBaHuang-ModLib; Taoist: 3161035078 in
# GuiGuBaHuang-Taoist...), a mod reuses the ModLib DLL of the submodule and does not release ModLib.
#
# Repository: -RepoRoot of the caller, else the git repository of the current folder (run from the root of the
# repository to release: GuiGuBaHuang-ModLib for ModLib, the mod repository for the mod).
# Projects: "projects" of <repo>/scripts/settings.json, else every <repo>/<ProjectId>/scripts/settings.json.
# Target = "releaseName" of a project (ModLib, Taoist...); tag <releaseName>-<version>, the version read from the source
# code (versionFile / versionPattern of <ProjectId>/scripts/settings.json).

function Initialize-Release([string]$RepoRoot) {
    if (-not $RepoRoot) {
        $RepoRoot = (git rev-parse --show-toplevel 2>$null)
        if (-not $RepoRoot) { throw "Not in a git repository: run from the repository to release, or pass -RepoRoot" }
    }
    $script:ReleaseRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
}

# Folders of the projects of the repository (relative to the repository)
function Get-ReleaseProjectDirs {
    $common = Join-Path $script:ReleaseRoot "scripts/settings.json"
    if (Test-Path -LiteralPath $common) {
        $s = Get-Content -LiteralPath $common -Raw | ConvertFrom-Json
        if ($s.projects) { return @($s.projects) }
    }
    return @(Get-ChildItem -LiteralPath $script:ReleaseRoot -Directory | Where-Object { Test-Path (Join-Path $_.FullName "scripts/settings.json") } | ForEach-Object { $_.Name })
}

# Every project: ProjectId, Dir (relative), Name (releaseName), ModName, Version, Requires (releaseRequires)
function Get-ReleaseProjects {
    foreach ($dir in Get-ReleaseProjectDirs) {
        $s = Get-Content (Join-Path $script:ReleaseRoot "$dir/scripts/settings.json") -Raw | ConvertFrom-Json
        if (-not $s.releaseName) { continue }
        $file = Join-Path $script:ReleaseRoot (Join-Path $dir $s.versionFile)
        $m = [regex]::Match((Get-Content $file -Raw), $s.versionPattern)
        if (-not $m.Success) { throw "Version not found in $file (pattern $($s.versionPattern))" }
        [pscustomobject]@{ ProjectId = $s.projectId; Dir = $dir; Name = $s.releaseName; ModName = $s.modName; Version = $m.Groups[1].Value; Requires = $s.releaseRequires }
    }
}

# A release this repository needs ("releaseRequires", e.g. ModLib): the project of that name in a submodule
# (<root>/<submodule>/<ProjectId>/scripts/settings.json), its version (the build shipped with the mod), tag and the URL of
# its release on the repository of the submodule. $null when no submodule holds it.
function Get-RequiredRelease([string]$Name) {
    foreach ($sub in Get-ChildItem -LiteralPath $script:ReleaseRoot -Directory) {
        if (-not (Test-Path (Join-Path $sub.FullName ".git"))) { continue }
        foreach ($proj in Get-ChildItem -LiteralPath $sub.FullName -Directory) {
            $file = Join-Path $proj.FullName "scripts/settings.json"
            if (-not (Test-Path -LiteralPath $file)) { continue }
            $s = Get-Content -LiteralPath $file -Raw | ConvertFrom-Json
            if ($s.releaseName -ne $Name) { continue }
            $m = [regex]::Match((Get-Content (Join-Path $proj.FullName $s.versionFile) -Raw), $s.versionPattern)
            if (-not $m.Success) { return $null }
            $remote = "$(git -C $sub.FullName remote get-url origin 2>$null)".Trim() -replace '\.git$', '' -replace '^git@github\.com:', 'https://github.com/'
            $tag = "$Name-$($m.Groups[1].Value)"
            return [pscustomobject]@{
                Name = $Name; Version = $m.Groups[1].Value; Tag = $tag; ProjectId = $s.projectId; Dir = $sub.FullName; Remote = $remote
                Url = $(if ($remote) { "$remote/releases/tag/$tag" } else { $null })
            }
        }
    }
    return $null
}

# Target (releaseName; empty = the only project of the repository) -> Projects, Tag, Title
function Get-ReleaseTarget([string]$Target) {
    $all = @(Get-ReleaseProjects)
    if (-not $Target) {
        if ($all.Count -ne 1) { throw "Several projects in $($script:ReleaseRoot): pass -Target ($(($all | ForEach-Object Name) -join ', '))" }
        $Target = $all[0].Name
    }
    $projects = @($all | Where-Object { $_.Name -eq $Target })
    if ($projects.Count -eq 0) { throw "Unknown target '$Target' in $($script:ReleaseRoot) ($(($all | ForEach-Object Name) -join ', '))" }
    [pscustomobject]@{
        Target   = $Target
        Projects = $projects
        Tag      = "$($projects[0].Name)-$($projects[0].Version)"
        Title    = "$($projects[0].Name) $($projects[0].Version)"
    }
}

# Tag pushed by create-release-tag.ps1 -> target; $null when it is not a release tag of this repository
function Get-TargetOfTag([string]$Tag) {
    foreach ($p in Get-ReleaseProjects) {
        if ($Tag.StartsWith("$($p.Name)-")) { return $p.Name }
    }
    return $null
}
