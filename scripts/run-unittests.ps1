<#
.SYNOPSIS
    Build ModLib and Taoist (Release) and run the unit tests (tests\UnitTests).
.DESCRIPTION
    The tests use the build output in <project>\debug\...\dll and the game assemblies in
    <game>\MelonLoader\Managed (compile/resolve only). Pass -NoBuild to skip the mod builds.
.EXAMPLE
    .\scripts\run-unittests.ps1
    .\scripts\run-unittests.ps1 -NoBuild -Filter "FullyQualifiedName~Taoist"
#>
[CmdletBinding()]
param(
    [switch]$NoBuild,
    [string]$Filter,
    [switch]$IncludeKnownIssues,
    [string]$GameFolder
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
if (-not $NoBuild) {
    foreach ($p in '3385996759', '3161035078') {
        dotnet build (Join-Path $repo "$p\ModProject\ModCode\ModMain\ModMain.sln") --configuration Release -v q -nologo
        if ($LASTEXITCODE -ne 0) { Write-Host "Build of $p failed" -ForegroundColor Red; exit 1 }
    }
}
# one folder per run (history shown by test-viewer\index.html)
$runDir = Join-Path $repo ("logs\unittests\" + (Get-Date -Format 'yyyyMMdd-HHmmss'))
$testArgs = @('test', (Join-Path $repo 'tests\UnitTests\UnitTests.csproj'), '--configuration', 'Release', '--logger', 'console;verbosity=normal',
    '--results-directory', $runDir, '--logger', 'trx;LogFileName=unittests.trx')
# tests reproducing open bugs (docs\Known-Issues.md) fail by design: excluded unless -IncludeKnownIssues
if (-not $IncludeKnownIssues) { $Filter = if ($Filter) { "($Filter)&TestCategory!=KnownIssue" } else { 'TestCategory!=KnownIssue' } }
if ($Filter) { $testArgs += @('--filter', $Filter) }
if ($GameFolder) { $testArgs += "-p:GameFolder=$GameFolder" }
& dotnet @testArgs
$code = $LASTEXITCODE
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'update-test-viewer.ps1')
Write-Host "Results: $(Join-Path $repo 'test-viewer\index.html')#tab=unit" -ForegroundColor Cyan
exit $code
