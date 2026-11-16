<#
.SYNOPSIS
    Run the unit tests, then the Taoist autotest in the game, then open the test viewer.
.DESCRIPTION
    1. scripts\run-unittests.ps1 (a failure does not stop the autotest)
    2. scripts\game\run-autotest.ps1 (-Save / -Deploy / -Timeout passed through)
    3. test-viewer\index.html in the default browser
    Exit code: 0 when both passed, otherwise 1 (unit tests failed) / the autotest exit code.
    Keep ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI).
.EXAMPLE
    .\scripts\run-all-tests.ps1 -Save AutoTest -Deploy
#>
[CmdletBinding()]
param(
    [string]$Save,
    [switch]$Deploy,
    [int]$Timeout = 2700,
    [switch]$NoOpen
)

$repo = Split-Path $PSScriptRoot -Parent
$ps = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File')

Write-Host "=== Unit tests" -ForegroundColor Cyan
& powershell @ps (Join-Path $repo 'scripts\run-unittests.ps1')
$unit = $LASTEXITCODE

Write-Host "=== Autotest" -ForegroundColor Cyan
$args2 = @('-Timeout', $Timeout)
if ($Save) { $args2 += @('-Save', $Save) }
if ($Deploy) { $args2 += '-Deploy' }
& powershell @ps (Join-Path $repo 'scripts\game\run-autotest.ps1') @args2
$auto = $LASTEXITCODE

Write-Host ""
Write-Host ("Unit tests: {0}   Autotest: {1}" -f $(if ($unit -eq 0) { 'OK' } else { "NG (exit $unit)" }), $(if ($auto -eq 0) { 'OK' } else { "NG (exit $auto)" })) -ForegroundColor $(if ($unit -eq 0 -and $auto -eq 0) { 'Green' } else { 'Red' })
if (-not $NoOpen) { Start-Process (Join-Path $repo 'test-viewer\index.html') }
if ($auto -ne 0) { exit $auto }
if ($unit -ne 0) { exit 1 }
exit 0
