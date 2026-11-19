<#
.SYNOPSIS
    Effect galleries (TE-01 skill, TE-02 unit, TE-03 scene effects) of an autotest run -> docs/Effects (in git):
    animated webp per effect + index.json with the measures of the game (casting time, duration, direction, size, zoom)
    and the classification fields; the fresh effect lists of the game replace ModConf's. See collect_effects.py.
    Called by run-autotest.ps1 after a run with TE-xx; can be run alone on a former run. Needs Python + Pillow
    (python -m pip install --user pillow).

.PARAMETER Run
    Folder of the autotest run (logs/autotest/<time>); default: the latest.

.EXAMPLE
    .\scripts\game\collect-effects.ps1
    .\scripts\game\collect-effects.ps1 -Run logs\autotest\20261117-210000 -KeepFrames
#>
param(
    [string]$Run,
    [int]$Width = 960,
    [int]$Quality = 60,
    [switch]$KeepFrames
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not $Run) {
    $Run = Get-ChildItem (Join-Path $repo 'logs\autotest') -Directory | Where-Object { $_.Name -match '^\d{8}-\d{6}(-s\d+)?$' } |
        Sort-Object Name -Descending | Select-Object -First 1 -ExpandProperty FullName
}
$argsList = @((Join-Path $PSScriptRoot 'collect_effects.py'), $Run, '--width', $Width, '--quality', $Quality)
if ($KeepFrames) { $argsList += '--keep-frames' }
& python @argsList
