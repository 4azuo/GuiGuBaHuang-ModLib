<#
.SYNOPSIS
  Creates and pushes the release tag of a project of the repository (shared by the repositories using ModLib:
  GuiGuBaHuang-ModLib/scripts/release; each repository releases its own projects): the push starts the CI of that
  repository (.github/workflows/release.yml),
  which zips debug/<mod> and publishes the GitHub Release (+ Nexus Mods when set up).

.DESCRIPTION
  The version is read from the source code (_release.ps1): <releaseName>-<version>, e.g. ModLib-2.2.36, Taoist-6.1.06.
  Repository: -RepoRoot, else the git repository of the current folder.
  The tag is put on HEAD. Checks before tagging:
    - the tag does not exist yet (local and origin): bump the version first;
    - uncommitted changes of the mods (debug/, ModProject/): they would not be in the release;
    - HEAD pushed to origin (asks to push the branch).
  Asks for a confirmation unless -Yes.

.EXAMPLE
  powershell -NoProfile -ExecutionPolicy Bypass -File GuiGuBaHuang-ModLib/scripts/release/create-release-tag.ps1 -Target Taoist
#>
param(
    [string]$Target,
    [string]$RepoRoot,
    [switch]$Yes,
    [switch]$NoPush
)

# "Continue": with "Stop", Windows PowerShell 5.1 turns any stderr line of git into an error (exit codes are checked)
$ErrorActionPreference = "Continue"
. (Join-Path $PSScriptRoot "_release.ps1")
Initialize-Release $RepoRoot
$root = $script:ReleaseRoot

function Confirm-Step([string]$Question) {
    if ($Yes) { return $true }
    $a = Read-Host "$Question [y/N]"
    return $a -match '^(y|yes)$'
}

$release = Get-ReleaseTarget $Target
$tag = $release.Tag
$head = (git -C $root rev-parse --short HEAD).Trim()
$branch = (git -C $root rev-parse --abbrev-ref HEAD).Trim()
$subject = (git -C $root log -1 --format=%s).Trim()

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Release $Target" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Repo:   $root"
foreach ($p in $release.Projects) { Write-Host ("  {0,-7} {1,-10} ({2}/debug/{3})" -f $p.Name, $p.Version, $p.Dir, $p.ModName) }
Write-Host "  Tag:    $tag"
Write-Host "  Commit: $head ($branch) $subject"
Write-Host ""

# 1. tag must be new
git -C $root fetch --tags --quiet origin 2>$null
if (git -C $root tag --list $tag) {
    Write-Host "Tag $tag already exists: bump the version in the source code first." -ForegroundColor Red
    exit 1
}
git -C $root ls-remote --exit-code --tags origin "refs/tags/$tag" *> $null
if ($LASTEXITCODE -eq 0) {
    Write-Host "Tag $tag already exists on origin: bump the version in the source code first." -ForegroundColor Red
    exit 1
}

# 2. uncommitted changes of the released projects
$paths = $release.Projects | ForEach-Object { "$($_.Dir)/debug", "$($_.Dir)/ModProject" }
$dirty = @(git -C $root status --porcelain -- $paths)
if ($dirty.Count -gt 0) {
    Write-Host "Uncommitted changes (not in the release, the CI zips what is committed):" -ForegroundColor Yellow
    $dirty | Select-Object -First 15 | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
    if ($dirty.Count -gt 15) { Write-Host "  ... ($($dirty.Count) files)" -ForegroundColor Yellow }
    if (-not (Confirm-Step "Tag anyway?")) { exit 1 }
}

# 3. HEAD on origin
$onRemote = @(git -C $root branch -r --contains HEAD 2>$null)
if ($onRemote.Count -eq 0 -and -not $NoPush) {
    Write-Host "HEAD ($head) is not pushed to origin." -ForegroundColor Yellow
    if (Confirm-Step "Push branch '$branch' to origin first?") {
        git -C $root push origin $branch
        if ($LASTEXITCODE -ne 0) { Write-Host "Push failed." -ForegroundColor Red; exit 1 }
    } elseif (-not (Confirm-Step "Tag a commit that is not on a branch of origin?")) { exit 1 }
}

if (-not (Confirm-Step "Create tag $tag on $head$(if (-not $NoPush) { ' and push it (starts the release CI)' })?")) {
    Write-Host "Cancelled." -ForegroundColor Yellow
    exit 1
}

git -C $root tag -a $tag -m $release.Title
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Host "Tag $tag created." -ForegroundColor Green
if ($NoPush) { Write-Host "Not pushed (-NoPush): git push origin $tag" -ForegroundColor Yellow; exit 0 }

git -C $root push origin $tag
if ($LASTEXITCODE -ne 0) { Write-Host "Push of the tag failed (delete it locally: git tag -d $tag)." -ForegroundColor Red; exit 1 }

$remote = (git -C $root remote get-url origin).Trim() -replace '\.git$', '' -replace '^git@github\.com:', 'https://github.com/'
Write-Host "Pushed. CI: $remote/actions/workflows/release.yml" -ForegroundColor Green
Write-Host "Release (when the CI is done): $remote/releases/tag/$tag" -ForegroundColor Green
