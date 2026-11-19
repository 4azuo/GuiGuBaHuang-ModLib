<#
.SYNOPSIS
    Promotion run: takes the Steam Workshop / Ko-fi pictures of the Taoist mod (3161035078/ModProject/SteamImg) in a
    run of their own, instead of picking them from the test runs.
.DESCRIPTION
    Runs (run-autotest.ps1 -Promo) the scenarios with "promo" pictures in 3161035078/tests/scenarios.json: the tools
    TR-01 (title screen: S&M configs) and TR-02 (in the game: the windows of the features - trainer, bank, market,
    weapon details, team, town master...; AutoTest/Scenarios/Tool/TR_PromoShots.cs), with no check panel over the
    screenshots, in a 1600x900 window by default. They are tools: confirmed at the prompt, or -ConfirmTools. collect-promo.ps1 then copies the marked
    screenshots (without the window frame) to SteamImg/promo_<name>.png; the captions stay in scenarios.json,
    read by the marketing video (marketing/video, Remotion) and the promo viewer (promo-viewer/index.html).
    The test runs no longer write SteamImg. The run results still update scenario-status.json like any run.

    To add a picture: a "promo" entry on the scenario ({ "step", "nth", "name", "caption" }, docs/Taoist/04-Autotest.md).

.PARAMETER Name
    Only the scenarios having one of these promo names (e.g. bank,bank-loan,team).
.PARAMETER Window
    Game window size (default 1600x900, as every autotest run: 16:9 for the Workshop pictures and the video).
.PARAMETER PassThru
    Other parameters of run-autotest.ps1 (e.g. @{ Save = 'AutoTest'; Deploy = $true }).

.EXAMPLE
    .\scripts\game\run-promo.ps1 -Deploy
.EXAMPLE
    .\scripts\game\run-promo.ps1 -Name bank,bank-loan -Save AutoTest
.EXAMPLE
    .\scripts\game\run-promo.ps1 -List
    # the promo pictures, their scenario and whether SteamImg has them
#>
param(
    [string[]]$Name,
    [string]$Window = '1600x900',
    [string]$Save,
    [switch]$Deploy,
    [switch]$List,
    [int]$Timeout = 3600,
    [switch]$ConfirmTools,
    [hashtable]$PassThru = @{}
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$defs = (Get-Content (Join-Path $repo '3161035078\tests\scenarios.json') -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios
$names = @($Name | Where-Object { $_ } | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$steamImg = Join-Path $repo '3161035078\ModProject\SteamImg'

$pictures = foreach ($d in $defs | Where-Object { $_.PSObject.Properties['promo'] }) {
    foreach ($p in $d.promo) {
        if ($names.Count -gt 0 -and $names -notcontains $p.name) { continue }
        [pscustomobject]@{ Scenario = $d.id; Kind = $d.kind; Name = $p.name; Step = $p.step; Nth = $p.nth
            InSteamImg = Test-Path (Join-Path $steamImg "promo_$($p.name).png"); Caption = $p.caption }
    }
}
if (-not $pictures) { throw "No promo picture$(if ($names) { " named $($names -join ', ')" }) in scenarios.json" }
if ($List) { $pictures | Format-Table Scenario, Name, Step, Nth, InSteamImg, Caption -AutoSize; return }

$ids = @($pictures | Select-Object -ExpandProperty Scenario -Unique)
Write-Host "Promotion run: $($pictures.Count) picture(s) from $($ids.Count) scenario(s): $($ids -join ', ')" -ForegroundColor Cyan

$args2 = @{ Promo = $true; Scenario = $ids; Window = $Window; Timeout = $Timeout }
if ($Save) { $args2.Save = $Save }
if ($Deploy) { $args2.Deploy = $true }
if ($ConfirmTools) { $args2.ConfirmTools = $true }
foreach ($k in $PassThru.Keys) { $args2[$k] = $PassThru[$k] }
& (Join-Path $PSScriptRoot 'run-autotest.ps1') @args2
$code = $LASTEXITCODE

$missing = @($pictures | Where-Object { -not (Test-Path (Join-Path $steamImg "promo_$($_.Name).png")) })
if ($missing) { Write-Host "Not taken: $(($missing | ForEach-Object Name) -join ', ')" -ForegroundColor Yellow }
Write-Host "SteamImg: $steamImg (promo viewer: promo-viewer/index.html)"
exit $code
