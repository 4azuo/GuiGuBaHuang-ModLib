<#
.SYNOPSIS
    Merges the runs of a parallel autotest (run-autotest-parallel.ps1) into one run of logs\autotest.
.DESCRIPTION
    The games side by side make a run per scenario (logs\autotest\<time>-s<slot>, listed in runs.txt of the parallel
    folder). This script writes them as ONE run, logs\autotest\<parallel time>: one results.json (the latest result of
    every scenario; the pre-test TP-01 of a slot that had its character is left out when another TP-01 has a result),
    the screenshots of every scenario (renumbered in the order of the runs, the scenario's file names kept), the log of
    every run one after the other. Then run-autotest.ps1 -ReportFrom writes its report.json and the OK/NG of every id
    (scenario-status.json; NOT RUN for the auto scenarios without a result), the part runs are deleted and the merged
    run is pinned (scripts\pin-test-run.ps1).
    Keep ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI).
.PARAMETER Batch
    The parallel run folder (logs\autotest-parallel\<time>); default: the latest.
.PARAMETER KeepParts
    Keep the part runs (default: deleted once merged).
.PARAMETER Note
    Note of the pin.
.EXAMPLE
    .\scripts\game\merge-parallel-run.ps1
    .\scripts\game\merge-parallel-run.ps1 -Batch logs\autotest-parallel\20261120-234754 -Note "full run on 6 games"
#>
# several -Batch folders (a full run, then a run of its failed scenarios): ONE run named after the first one, the
# latest result of every scenario
# -Base: an earlier merged run (logs\autotest\<time>) the new runs are merged into (scenarios run again later:
# their new result replaces the old one; the run keeps its name - one history)
param([string[]]$Batch, [switch]$KeepParts, [string]$Note = '', [string]$Base)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_game.ps1')
$repo = Get-ModRepo
$parallelRoot = Join-Path $repo 'logs\autotest-parallel'
if (-not $Batch) { $Batch = @((Get-ChildItem -LiteralPath $parallelRoot -Directory | Sort-Object Name | Select-Object -Last 1).FullName) }
$Batch = @($Batch | ForEach-Object { "$_" -split "," } | Where-Object { $_ } | ForEach-Object { (Resolve-Path -LiteralPath $_.Trim()).Path } | Sort-Object)
$stamp = if ($Base) { Split-Path (Resolve-Path -LiteralPath $Base).Path -Leaf } else { Split-Path $Batch[0] -Leaf }
$utf8 = New-Object Text.UTF8Encoding($false)
$runIds = @()
foreach ($b in $Batch) {
    $runsFile = Join-Path $b 'runs.txt'
    if (-not (Test-Path -LiteralPath $runsFile)) { throw "No runs.txt in $b" }
    $runIds += @(Get-Content -LiteralPath $runsFile | ForEach-Object { "$_".Trim() } | Where-Object { $_ })
}
# in the order of the runs (their start time): a later result of a scenario replaces an earlier one
$runIds = @($runIds | Select-Object -Unique | Sort-Object { ($_ -split '-s')[0] })
$out = Join-Path $repo "logs\autotest\$stamp"
# the base run moved aside, read first (the new runs replace its results)
$dirs = @()
if ($Base) {
    $baseTmp = "$out.base"
    if (Test-Path -LiteralPath $baseTmp) { Remove-Item -LiteralPath $baseTmp -Recurse -Force }
    Move-Item -LiteralPath $out -Destination $baseTmp
    $dirs += [pscustomobject]@{ Id = "$stamp (base)"; Dir = $baseTmp }
}
foreach ($runId in $runIds) { $dirs += [pscustomobject]@{ Id = $runId; Dir = (Join-Path $repo "logs\autotest\$runId") } }
if (Test-Path -LiteralPath $out) { Remove-Item -LiteralPath $out -Recurse -Force }
New-Item -ItemType Directory -Path (Join-Path $out 'screens') -Force | Out-Null

$byId = [ordered]@{}
$logs = New-Object System.Collections.Generic.List[string]
$first = $null; $last = $null; $startedAt = $null; $updatedAt = $null
$shot = 0
$parts = @()
foreach ($d in $dirs) {
    $runId = $d.Id
    $dir = $d.Dir
    $rf = Join-Path $dir 'results.json'
    if (-not (Test-Path -LiteralPath $rf)) { Write-Host "missing: $runId" -ForegroundColor Yellow; continue }
    $parts += $dir
    $res = Get-Content -LiteralPath $rf -Raw -Encoding UTF8 | ConvertFrom-Json
    if (-not $first) { $first = $res }
    $last = $res
    if (-not $startedAt -or "$($res.startedAt)" -lt $startedAt) { $startedAt = "$($res.startedAt)" }
    if (-not $updatedAt -or "$($res.updatedAt)" -gt $updatedAt) { $updatedAt = "$($res.updatedAt)" }
    $logs.Add("===== run $runId =====")
    foreach ($l in @($res.log)) { $logs.Add("$l") }
    foreach ($s in @($res.scenarios)) {
        if (-not $s -or -not $s.Id) { continue }
        $id = "$($s.Id)"
        # the pre-test of a slot that had its character (SKIP) or an interrupted one: only when no other TP-01 result
        if ($id -like 'TP-*' -and $s.Status -in 'SKIP', 'RUNNING' -and $byId.Contains($id)) { continue }
        if ($s.Status -eq 'RUNNING' -and $byId.Contains($id)) { continue }
        # its screenshots copied, renumbered (the order of the runs), the scenario's names kept
        foreach ($sc in @($s.Screenshots)) {
            if (-not $sc -or -not $sc.File) { continue }
            $src = Join-Path $dir ("$($sc.File)" -replace '/', '\')
            if (-not (Test-Path -LiteralPath $src)) { continue }
            $name = (Split-Path "$($sc.File)" -Leaf) -replace '^\d+_', ''
            $file = 'screens/{0:0000}_{1}' -f (++$shot), $name
            Copy-Item -LiteralPath $src -Destination (Join-Path $out ($file -replace '/', '\')) -Force
            $sc.File = $file
        }
        $byId[$id] = $s
    }
}
if (-not $first) { throw "No run of $runsFile found in logs\autotest" }

$scenarios = @($byId.Values)
$merged = [ordered]@{
    runId = $stamp
    status = 'DONE'
    phase = 'Done'
    startedAt = $startedAt
    updatedAt = $updatedAt
    modVersion = $(if ($first.modVersion) { $first.modVersion } else { $first.taoistVersion })
    modLibVersion = $first.modLibVersion
    player = "parallel run: $($runIds.Count) run(s) on the game slots"
    summary = [ordered]@{
        total = $scenarios.Count
        pass = @($scenarios | Where-Object { $_.Status -eq 'PASS' }).Count
        fail = @($scenarios | Where-Object { $_.Status -eq 'FAIL' }).Count
        error = @($scenarios | Where-Object { $_.Status -eq 'ERROR' }).Count
        skip = @($scenarios | Where-Object { $_.Status -eq 'SKIP' }).Count
    }
    scenarios = $scenarios
    log = @($logs)
    parts = $runIds
}
[IO.File]::WriteAllText((Join-Path $out 'results.json'), ($merged | ConvertTo-Json -Depth 30), $utf8)
# the screenshots of a result replaced by a later run: not referenced any more, deleted
$used = @{}
foreach ($s2 in $scenarios) { foreach ($sc in @($s2.Screenshots)) { if ($sc -and $sc.File) { $used[(Split-Path "$($sc.File)" -Leaf)] = $true } } }
$extra = @(Get-ChildItem -LiteralPath (Join-Path $out 'screens') -File | Where-Object { -not $used.ContainsKey($_.Name) })
$extra | Remove-Item -Force -ErrorAction SilentlyContinue
if ($extra.Count -gt 0) { Write-Host "screenshots of replaced results removed: $($extra.Count)" }
Write-Host "merged $($runIds.Count) run(s) -> logs\autotest\$stamp ($($scenarios.Count) scenario(s), $shot screenshot(s))"

# report.json and scenario-status.json (a whole run: the auto scenarios without a result are NOT RUN)
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-autotest.ps1') -ReportFrom $out -NoViewerUpdate | Out-Host

if (-not $KeepParts) {
    foreach ($p in $parts) { Remove-Item -LiteralPath $p -Recurse -Force -ErrorAction SilentlyContinue }
    Write-Host "part runs deleted: $($parts.Count)"
}
$pinNote = if ($Note) { $Note } else { "parallel run ${stamp}: $($merged.summary.pass) OK, $($merged.summary.fail + $merged.summary.error) NG, $($merged.summary.skip) SKIP" }
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot '..\pin-test-run.ps1') -Run $stamp -Note $pinNote | Out-Host
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot '..\..\test-viewer\update-data.ps1') -RepoRoot $repo | Out-Null
Write-Host "test viewer updated: one run $stamp"
