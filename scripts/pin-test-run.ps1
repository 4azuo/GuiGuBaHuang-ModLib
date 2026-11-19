<#
.SYNOPSIS
    Pin test runs (autotest / unit tests) so that they are never deleted, and list the runs.
.DESCRIPTION
    update-test-viewer.ps1 keeps only the newest -Keep runs (default 10) of logs\autotest and logs\unittests:
    a pinned run (a pinned.json file in its folder) is never deleted and does not count in -Keep, so the most
    complete run (e.g. a full autotest run) stays available in test-viewer\index.html while only some scenarios
    are run afterwards (run-autotest.ps1 -Scenario ...; the OK/NG of every id stays in scenario-status.json).
    run-autotest.ps1 pins a full run (no -Scenario) where every scenario passed by itself (note "auto"); only the
    newest 3 auto pins are kept, manual pins stay until unpinned.
    Keep ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI).
.PARAMETER Run
    Run id (folder name, e.g. 20261117-074658) to pin or unpin.
.PARAMETER Best
    Pin the most complete autotest run: the most scenarios executed, then the fewest NG, then the newest.
.PARAMETER Latest
    Pin the latest run.
.PARAMETER Unpin
    Remove the pin of -Run.
.PARAMETER Unit
    Unit test runs (logs\unittests) instead of autotest runs.
.PARAMETER Note
    Note stored with the pin (shown by -List and in the test viewer).
.PARAMETER List
    List the runs with their counts and pins (default when nothing else is given).
.EXAMPLE
    .\scripts\pin-test-run.ps1 -List
    .\scripts\pin-test-run.ps1 -Best -Note "full run before the release"
    .\scripts\pin-test-run.ps1 -Run 20261117-074658
    .\scripts\pin-test-run.ps1 -Run 20261117-074658 -Unpin
    .\scripts\pin-test-run.ps1 -Unit -Latest
#>
[CmdletBinding()]
param([string]$Run, [switch]$Best, [switch]$Latest, [switch]$Unpin, [switch]$Unit, [string]$Note = '', [switch]$List, [switch]$Auto)

$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$root = Join-Path $repo $(if ($Unit) { 'logs\unittests' } else { 'logs\autotest' })
$PIN = 'pinned.json'
$MAX_AUTO = 3

function Read-Json([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return $null }
    try { return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json } catch { return $null }
}

# counts of a run: autotest summary.json (pass/fail/error/skip/notRun), unit tests: the trx file
function Get-RunInfo($dir) {
    $pin = Read-Json (Join-Path $dir.FullName $PIN)
    $info = [ordered]@{ Id = $dir.Name; Executed = 0; Ok = 0; Ng = 0; NotRun = 0; Pinned = [bool]$pin; Note = if ($pin) { $pin.note } else { '' }; Auto = if ($pin) { [bool]$pin.auto } else { $false } }
    if ($Unit) {
        $trx = Get-ChildItem -LiteralPath $dir.FullName -Filter *.trx -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($trx) {
            try {
                [xml]$x = Get-Content -LiteralPath $trx.FullName -Raw -Encoding UTF8
                $c = $x.TestRun.ResultSummary.Counters
                $info.Executed = [int]$c.executed; $info.Ok = [int]$c.passed; $info.Ng = [int]$c.failed + [int]$c.error
            } catch { }
        }
    } else {
        $s = Read-Json (Join-Path $dir.FullName 'summary.json')
        if ($s -and $s.counts) {
            $info.Ok = [int]$s.counts.pass; $info.Ng = [int]$s.counts.fail + [int]$s.counts.error
            $info.Executed = $info.Ok + $info.Ng + [int]$s.counts.skip
            $info.NotRun = [int]$s.counts.notRun
        }
    }
    [pscustomobject]$info
}

$dirs = @(Get-ChildItem -LiteralPath $root -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^\d{8}-\d{6}(-s\d+)?' } | Sort-Object Name)
if ($dirs.Count -eq 0) { Write-Host "No runs in $root"; exit 0 }
$runs = @($dirs | ForEach-Object { Get-RunInfo $_ })

function Set-Pin([string]$Id, [bool]$IsAuto) {
    $dir = Join-Path $root $Id
    if (-not (Test-Path -LiteralPath $dir)) { throw "Run not found: $dir" }
    $data = [ordered]@{ pinnedAt = (Get-Date).ToString('s'); note = $Note; auto = $IsAuto }
    [IO.File]::WriteAllText((Join-Path $dir $PIN), ($data | ConvertTo-Json), (New-Object Text.UTF8Encoding $false))
    Write-Host "Pinned $Id$(if ($Note) { " ($Note)" })" -ForegroundColor Green
}

$target = $null
if ($Best) {
    $target = ($runs | Where-Object { $_.Executed -gt 0 } | Sort-Object @{ e = 'Executed'; Descending = $true }, @{ e = 'Ng'; Descending = $false }, @{ e = 'Id'; Descending = $true } | Select-Object -First 1).Id
    if (-not $target) { throw 'No run with results' }
} elseif ($Latest) { $target = $runs[-1].Id }
elseif ($Run) { $target = $Run }

if ($target) {
    if ($Unpin) {
        $f = Join-Path (Join-Path $root $target) $PIN
        if (Test-Path -LiteralPath $f) { Remove-Item -LiteralPath $f -Force; Write-Host "Unpinned $target" -ForegroundColor Yellow }
        else { Write-Host "$target was not pinned" }
    } else {
        Set-Pin $target ([bool]$Auto)
        if ($Auto) {
            # only the newest auto pins are kept
            $autos = @(Get-ChildItem -LiteralPath $root -Directory | Where-Object { (Read-Json (Join-Path $_.FullName $PIN)).auto } | Sort-Object Name -Descending | Select-Object -Skip $MAX_AUTO)
            foreach ($d in $autos) { Remove-Item -LiteralPath (Join-Path $d.FullName $PIN) -Force; Write-Host "Auto pin removed from $($d.Name) (newest $MAX_AUTO kept)" -ForegroundColor DarkGray }
        }
    }
    $runs = @($dirs | ForEach-Object { Get-RunInfo $_ })
    if (-not $List) { exit 0 }
}

Write-Host "$(if ($Unit) { 'Unit test' } else { 'Autotest' }) runs in $root (pinned runs are never deleted):"
$runs | Sort-Object Id -Descending | Format-Table @{ l = 'Run'; e = 'Id' }, @{ l = 'Pin'; e = { if ($_.Pinned) { if ($_.Auto) { 'auto' } else { 'yes' } } else { '' } } }, Executed, Ok, Ng, NotRun, Note -AutoSize
