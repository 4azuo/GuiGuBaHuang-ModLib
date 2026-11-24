<#
.SYNOPSIS
    Build the projects of a repository (Release) and run its unit tests (shared by ModLib and the mod repositories).
.DESCRIPTION
    Repository: -RepoRoot, else the git repository of the current folder. Test project: tests\UnitTests\UnitTests.csproj
    (a mod repository: its tests + the ModLib tests), else tests\ModLibTests.csproj (ModLib alone).
    Builds every project of scripts\settings.json "projects" first (-NoBuild skips it); a mod uses the ModLib.dll committed
    in the submodule. The tests use the build output in <project>\debug\...\dll and the game assemblies in
    <game>\MelonLoader\Managed (compile/resolve only).
    Results: <repo>\logs\unittests\<time>\unittests.trx, shown by <repo>\logs\test-viewer\index.html (tab Unit tests).
    Tests reproducing open bugs (TestCategory KnownIssue) are excluded unless -IncludeKnownIssues.
.EXAMPLE
    .\scripts\run-unittests.ps1                                         (in GuiGuBaHuang-ModLib)
    .\GuiGuBaHuang-ModLib\scripts\run-unittests.ps1 -NoBuild -Filter "FullyQualifiedName~Taoist"   (in a mod repository)
#>
[CmdletBinding()]
param(
    [switch]$NoBuild,
    [string]$Filter,
    [switch]$IncludeKnownIssues,
    [string]$GameFolder,
    [string]$RepoRoot
)
$ErrorActionPreference = 'Stop'
if (-not $RepoRoot) {
    $RepoRoot = (git rev-parse --show-toplevel 2>$null)
    if (-not $RepoRoot) { throw 'Not in a git repository: run from the repository, or pass -RepoRoot' }
}
$repo = (Resolve-Path -LiteralPath $RepoRoot).Path
$settings = Get-Content -LiteralPath (Join-Path $repo 'scripts\settings.json') -Raw | ConvertFrom-Json

if (-not $NoBuild) {
    foreach ($p in $settings.projects) {
        dotnet build (Join-Path $repo "$p\ModProject\ModCode\ModMain\ModMain.sln") --configuration Release -v q -nologo
        if ($LASTEXITCODE -ne 0) { Write-Host "Build of $p failed" -ForegroundColor Red; exit 1 }
    }
}

$project = Join-Path $repo 'tests\UnitTests\UnitTests.csproj'
if (-not (Test-Path -LiteralPath $project)) { $project = Join-Path $repo 'tests\ModLibTests.csproj' }
if (-not (Test-Path -LiteralPath $project)) { throw "No test project in $repo\tests" }

# one folder per run (history shown by logs\test-viewer\index.html)
$runDir = Join-Path $repo ("logs\unittests\" + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$testArgs = @('test', $project, '--configuration', 'Release', '--logger', 'console;verbosity=normal',
    '--results-directory', $runDir, '--logger', 'trx;LogFileName=unittests.trx')
if (-not $IncludeKnownIssues) { $Filter = if ($Filter) { "($Filter)&TestCategory!=KnownIssue" } else { 'TestCategory!=KnownIssue' } }
if ($Filter) { $testArgs += @('--filter', $Filter) }
if ($GameFolder) { $testArgs += "-p:GameFolder=$GameFolder" }
& dotnet @testArgs
$code = $LASTEXITCODE
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot '..\test-viewer\update-data.ps1') -RepoRoot $repo
Write-Host "Results: $(Join-Path $repo 'logs\test-viewer\index.html')#tab=unit" -ForegroundColor Cyan
exit $code
