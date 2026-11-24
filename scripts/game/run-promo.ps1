<#
.SYNOPSIS
    Promotion run: takes the Steam Workshop / Ko-fi pictures of the mod (<modProject>/ModProject/SteamImg) in a
    run of their own, instead of picking them from the test runs.
.DESCRIPTION
    Runs (run-autotest.ps1 -Promo) the scenarios with "promo" pictures in <modProject>/tests/scenarios.json: the tools
    TR-01 (title screen: S&M configs), TR-02 (in the game: the windows of the features - trainer, bank, market,
    weapon details, team, town master...), TR-03 (houses and the city) and TR-04 (the pictures of the 6.0 video: city,
    house, children, clans and clan wars, the new paths of cultivation; AutoTest/Scenarios/Tool/TR_PromoShots.cs,
    TR04_Promo60.cs), with no check panel over the screenshots, in a 1600x900 window by default. Like the tools, a
    promotion run only starts once confirmed: at the prompt, or -ConfirmTools. collect-promo.ps1 then copies the marked
    screenshots (without the window frame) to SteamImg/promo-<version>/promo_<scenario>_<name>.jpg (one folder per
    version of the mod, "promoVersion" of the scenario: promo-5.5.45, promo-6.0.00; under 2 MB, the Steam limit); the
    captions stay in scenarios.json,
    read by the marketing video (marketing/video-6.0.0, Remotion) and the promo viewer (logs/promo-viewer/index.html).
    The test runs no longer write SteamImg, and a promotion run does not write scenario-status.json (not a test run).

    To add a picture: a "promo" entry on the scenario ({ "step", "nth", "name", "caption" }, docs of the autotest of the mod)
    and its "promoVersion".

.PARAMETER Version
    Only the pictures of these versions ("promoVersion" of scenarios.json, e.g. 6.0.00: SteamImg/promo-6.0.00).
.PARAMETER Scenario
    Only the pictures of these scenarios (e.g. TR-04: the 6.0 set).
.PARAMETER Name
    Only the scenarios having one of these promo names (e.g. bank,bank-loan,team).
.PARAMETER Window
    Game window size (default 1600x900, as every autotest run: 16:9 for the Workshop pictures and the video).
.PARAMETER PassThru
    Other parameters of run-autotest.ps1 (e.g. @{ Save = 'AutoTest'; Deploy = $true }).

.EXAMPLE
    .\scripts\game\run-promo.ps1 -Deploy
.EXAMPLE
    .\scripts\game\run-promo.ps1 -Version 6.0.00 -Save AutoTest -Deploy -ConfirmTools
    # every picture of the 6.0 folder (TR-04 and the test scenarios of the 6.0 features)
.EXAMPLE
    .\scripts\game\run-promo.ps1 -Scenario TR-04 -Save AutoTest -ConfirmTools
    # the 6.0 set only: SteamImg/promo-6.0.00/promo_TR-04_*.jpg
.EXAMPLE
    .\scripts\game\run-promo.ps1 -Name bank,bank-loan -Save AutoTest
.EXAMPLE
    .\scripts\game\run-promo.ps1 -List
    # the promo pictures, their scenario and whether SteamImg has them
#>
param(
    [string[]]$Version,
    [string[]]$Scenario,
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
. (Join-Path $PSScriptRoot '_game.ps1')
$mod = Get-ModInfo
$repo = $mod.Repo
$defs = (Get-Content (Join-Path $mod.Tests 'scenarios.json') -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios
$names = @($Name | Where-Object { $_ } | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$scenarioIds = @($Scenario | Where-Object { $_ } | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim().ToUpper() } | Where-Object { $_ })
$versions = @($Version | Where-Object { $_ } | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim() } | Where-Object { $_ })
$steamImg = Join-Path $repo "$($mod.Project)\ModProject\SteamImg"

$pictures = foreach ($d in $defs | Where-Object { $_.PSObject.Properties['promo'] }) {
    if ($scenarioIds.Count -gt 0 -and $scenarioIds -notcontains $d.id) { continue }
    foreach ($p in $d.promo) {
        if ($names.Count -gt 0 -and $names -notcontains $p.name) { continue }
        # promo-<version>/promo_<scenario>_<name>.jpg: one folder per version of the mod (collect-promo.ps1)
        $ver = if ($d.PSObject.Properties['promoVersion'] -and $d.promoVersion) { $d.promoVersion } else { 'other' }
        if ($versions.Count -gt 0 -and $versions -notcontains $ver) { continue }
        $file = "promo-$ver\promo_$($d.id)_$($p.name).jpg"
        [pscustomobject]@{ Scenario = $d.id; Version = $ver; Kind = $d.kind; Name = $p.name; File = $file; Step = $p.step; Nth = $p.nth
            InSteamImg = Test-Path (Join-Path $steamImg $file); Caption = $p.caption }
    }
}
if (-not $pictures) { throw "No promo picture$(if ($versions) { " of version $($versions -join ', ')" })$(if ($scenarioIds) { " of $($scenarioIds -join ', ')" })$(if ($names) { " named $($names -join ', ')" }) in scenarios.json" }
if ($List) { $pictures | Format-Table Scenario, Version, File, Step, Nth, InSteamImg, Caption -AutoSize; return }

$ids = @($pictures | Select-Object -ExpandProperty Scenario -Unique)
Write-Host "Promotion run: $($pictures.Count) picture(s) from $($ids.Count) scenario(s): $($ids -join ', ')" -ForegroundColor Cyan

# a promotion run (long, records the game) starts only once the user confirmed it, as the tool scenarios: at the
# prompt in an interactive terminal, -ConfirmTools otherwise (the test scenarios with promo pictures included)
if (-not $ConfirmTools) {
    $interactive = [Environment]::UserInteractive -and -not [Console]::IsInputRedirected -and $Host.Name -eq 'ConsoleHost'
    if (-not $interactive) { Write-Host "Promotion run: not run without the user's confirmation (add -ConfirmTools)." -ForegroundColor Yellow; exit 0 }
    if ((Read-Host "Run the promotion scenario(s) $($ids -join ', ') (long, records the game)? [y/N]") -notmatch '^(y|yes)$') { exit 0 }
}

$args2 = @{ Promo = $true; Scenario = $ids; Window = $Window; Timeout = $Timeout }
if ($Save) { $args2.Save = $Save }
if ($Deploy) { $args2.Deploy = $true }
$args2.ConfirmTools = $true   # confirmed above
# a picture of the pre-test TP-01 (character creation) needs a new character: run-autotest skips TP-01 otherwise
if ($ids -contains 'TP-01' -and -not $Save) { $args2.NewCharacter = $true }
foreach ($k in $PassThru.Keys) { $args2[$k] = $PassThru[$k] }
& (Join-Path $PSScriptRoot 'run-autotest.ps1') @args2
$code = $LASTEXITCODE

$missing = @($pictures | Where-Object { -not (Test-Path (Join-Path $steamImg $_.File)) })
if ($missing) { Write-Host "Not taken: $(($missing | ForEach-Object File) -join ', ')" -ForegroundColor Yellow }
Write-Host "SteamImg: $steamImg (promo viewer: logs/promo-viewer/index.html)"
exit $code
