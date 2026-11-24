<#
.SYNOPSIS
    Run the unit tests, then the autotest of the mod in the game, then open the test viewer (shared: GuiGuBaHuang-ModLib/scripts).
.DESCRIPTION
    1. run-unittests.ps1 (a failure does not stop the autotest)
    2. game\run-autotest.ps1 (-Save / -Deploy / -Timeout passed through)
    3. logs\test-viewer\index.html in the default browser
    Exit code: 0 when both passed, otherwise 1 (unit tests failed) / the autotest exit code.
    Keep ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI).
.EXAMPLE
    .\GuiGuBaHuang-ModLib\scripts\run-all-tests.ps1 -Save AutoTest -Deploy      (in the mod repository)
#>
[CmdletBinding()]
param(
    [string]$Save,
    [switch]$Deploy,
    [int]$Timeout = 2700,
    [switch]$NoOpen,
    [string]$RepoRoot
)

# Repository: -RepoRoot, else the git repository of the current folder (shared: GuiGuBaHuang-ModLib/scripts)
if (-not $RepoRoot) { $RepoRoot = (git rev-parse --show-toplevel 2>$null) }
if (-not $RepoRoot) { Write-Host 'Not in a git repository: run from the repository, or pass -RepoRoot' -ForegroundColor Red; exit 1 }
$RepoRoot = (Resolve-Path -LiteralPath $RepoRoot).Path
$repo = $RepoRoot
$env:MOD_REPO = $repo   # the game scripts run on this repository
$ps = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File')

Write-Host "=== Unit tests" -ForegroundColor Cyan
& powershell @ps (Join-Path $PSScriptRoot 'run-unittests.ps1') -RepoRoot $repo
$unit = $LASTEXITCODE

Write-Host "=== Autotest" -ForegroundColor Cyan
$args2 = @('-Timeout', $Timeout)
if ($Save) { $args2 += @('-Save', $Save) }
if ($Deploy) { $args2 += '-Deploy' }
& powershell @ps (Join-Path $PSScriptRoot 'game\run-autotest.ps1') @args2
$auto = $LASTEXITCODE

Write-Host ""
Write-Host ("Unit tests: {0}   Autotest: {1}" -f $(if ($unit -eq 0) { 'OK' } else { "NG (exit $unit)" }), $(if ($auto -eq 0) { 'OK' } else { "NG (exit $auto)" })) -ForegroundColor $(if ($unit -eq 0 -and $auto -eq 0) { 'Green' } else { 'Red' })
if (-not $NoOpen) { Start-Process (Join-Path $repo 'logs\test-viewer\index.html') }
if ($auto -ne 0) { exit $auto }
if ($unit -ne 0) { exit 1 }
exit 0
