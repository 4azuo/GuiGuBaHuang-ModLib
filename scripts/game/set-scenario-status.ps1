<#
.SYNOPSIS
    Record the OK/NG result of scenarios by id (mainly manual TM-xx scenarios) or list the status.
.DESCRIPTION
    Updates 3161035078\tests\scenario-status.json (2-space JSON) and the test viewer data;
    -Open opens test-viewer\index.html on the "Scenario status" tab.
    Automatic scenarios are updated by run-autotest.ps1 after each run.
.EXAMPLE
    .\scripts\game\set-scenario-status.ps1 -Id TM-03,TM-05 -Result OK
.EXAMPLE
    .\scripts\game\set-scenario-status.ps1 -Id TM-07 -Result NG -Note "deal accepted but item not transferred"
.EXAMPLE
    .\scripts\game\set-scenario-status.ps1 -Open            # test viewer, Scenario status tab
.EXAMPLE
    .\scripts\game\set-scenario-status.ps1 -List            # all ids with their result
    .\scripts\game\set-scenario-status.ps1 -List -Only NG   # only NG ids
#>

[CmdletBinding()]
param(
    [string[]]$Id,
    [ValidateSet('OK', 'NG', 'SKIP', 'NOT RUN', '-')]
    [string]$Result,
    [string]$Note,
    [string]$Tester = $env:USERNAME,
    [switch]$List,
    [switch]$Open,
    [string]$Only
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_game.ps1')
. (Join-Path $PSScriptRoot '_autotest.ps1')
$repo = (Get-GamePaths).Repo
$status = Read-ScenarioStatus $repo

if ($Id) {
    if (-not $Result) { Write-Err 'Pass -Result OK|NG|SKIP|NOT RUN|-'; exit 1 }
    foreach ($i in @($Id | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })) {
        if (-not $status.Contains($i)) { Write-Err "Unknown scenario id: $i (see 3161035078\tests\scenarios.json)"; exit 1 }
        $e = $status[$i]
        $e.result = $Result
        $e.lastRun = (Get-Date).ToString('s')
        $e.tester = $Tester
        if ($PSBoundParameters.ContainsKey('Note')) { $e.note = $Note }
        Write-Ok "$i -> $Result"
    }
    Save-ScenarioStatus $repo $status
}

if ($Id -or $Open) { & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $repo 'scripts\update-test-viewer.ps1') }
if ($Open) { Start-Process ((Join-Path $repo 'test-viewer\index.html') + '#tab=status') }

if ($List -or (-not $Id -and -not $Open)) {
    $rows = @($status.Values | Where-Object { -not $Only -or $_.result -eq $Only })
    $rows | ForEach-Object { [pscustomobject]@{ Id = $_.id; Kind = $_.kind; Result = $_.result; Feature = $_.feature; LastRun = $_.lastRun; Note = $_.note } } | Format-Table -AutoSize -Wrap
}
