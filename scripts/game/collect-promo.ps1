<#
.SYNOPSIS
    Copies the promotion screenshots of an autotest run to 3161035078/ModProject/SteamImg/promo_<name>.png (Steam
    Workshop / Ko-fi pictures). The pictures are marked in 3161035078/tests/scenarios.json, per scenario:
        "promo": [ { "step": "NPCs-joined", "nth": 1, "name": "battle_npcs_join", "caption": "..." } ]
    step = the step part of the screenshot name (NNN_<id>_SS-<step>.png), nth = which one when a step has several
    screenshots (dramas of a storyline: 1 = the first). Can be run alone on an earlier run.
    The other pictures of SteamImg (1.jpg...) are not touched.
    Called by run-autotest.ps1 -Promo (scripts/game/run-promo.ps1) only: the test runs never write SteamImg.
    Only pictures go to SteamImg (it is deployed with the mod): the captions stay in scenarios.json, read by the
    marketing video (marketing/video) and the promo viewer.

.PARAMETER Run
    Folder of the autotest run (logs/autotest/<time>); default: the latest.

.EXAMPLE
    .\scripts\game\collect-promo.ps1 -Run logs\autotest\20261117-074658
#>
param([string]$Run)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not $Run) {
    $Run = Get-ChildItem (Join-Path $repo 'logs\autotest') -Directory | Where-Object { $_.Name -match '^\d{8}-\d{6}(-s\d+)?$' } |
        Sort-Object Name -Descending | Select-Object -First 1 -ExpandProperty FullName
}
$screens = Join-Path $Run 'screens'
if (-not (Test-Path $screens)) { Write-Host "No screenshots in $Run (nothing to copy)" }
$defs = (Get-Content (Join-Path $repo '3161035078\tests\scenarios.json') -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios
$out = Join-Path $repo '3161035078\ModProject\SteamImg'
New-Item -ItemType Directory -Force -Path $out | Out-Null
$all = if (Test-Path $screens) { @(Get-ChildItem $screens -Filter '*.png') } else { @() }
Add-Type -AssemblyName System.Drawing

# the game area of a window screenshot: without the title bar and the borders (16:9 at the bottom of the window)
function Save-GameArea([string]$src, [string]$dst) {
    $img = [System.Drawing.Image]::FromFile($src)
    try {
        $border = 8
        $w = $img.Width - 2 * $border
        $h = [int]($w * 9 / 16)
        $top = $img.Height - $border - $h
        if ($w -le 0 -or $top -lt 0) { $img.Save($dst, [System.Drawing.Imaging.ImageFormat]::Png); return }
        $bmp = New-Object System.Drawing.Bitmap $w, $h
        $gfx = [System.Drawing.Graphics]::FromImage($bmp)
        $gfx.DrawImage($img, (New-Object System.Drawing.Rectangle 0, 0, $w, $h), (New-Object System.Drawing.Rectangle $border, $top, $w, $h), [System.Drawing.GraphicsUnit]::Pixel)
        $bmp.Save($dst, [System.Drawing.Imaging.ImageFormat]::Png)
        $gfx.Dispose(); $bmp.Dispose()
    } finally { $img.Dispose() }
}
# scenarios that ran to the end (results.json): only they may drop an older picture they skipped; a scenario that
# stopped on an error keeps the pictures of its steps not reached
$finished = @()
$resultsFile = Join-Path $Run 'results.json'
if (Test-Path $resultsFile) {
    try {
        $finished = @((Get-Content $resultsFile -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios |
            Where-Object { $_.Status -in @('PASS', 'OK', 'SKIP', 'DONE') } | ForEach-Object { $_.Id })
    } catch { Write-Host "results.json: $_" -ForegroundColor Yellow }
}
$copied = 0
foreach ($d in $defs | Where-Object { $_.PSObject.Properties['promo'] }) {
    foreach ($p in $d.promo) {
        # step "drama" = any step of that name; "07-drama" = step 07 only
        $pattern = if ($p.step -match '^\d+-') { "^\d+_$([regex]::Escape($d.id))_$([regex]::Escape($p.step))\.png$" }
            else { "^\d+_$([regex]::Escape($d.id))_\d+-$([regex]::Escape($p.step))\.png$" }
        $shots = @($all | Where-Object { $_.Name -match $pattern } | Sort-Object Name)
        $nth = if ($p.PSObject.Properties['nth'] -and $p.nth) { [int]$p.nth } else { 1 }
        if ($shots.Count -lt $nth) {
            # the scenario ran but skipped this picture (nothing to show in this save): an older picture is removed
            $old = Join-Path $out "promo_$($p.name).png"
            if ((Test-Path $old) -and $finished -contains $d.id -and @($all | Where-Object { $_.Name -match "^\d+_$([regex]::Escape($d.id))_" }).Count -gt 0) {
                Remove-Item -LiteralPath $old; Write-Host "removed (not taken in this run): promo_$($p.name).png"
            }
            continue
        }
        Save-GameArea $shots[$nth - 1].FullName (Join-Path $out "promo_$($p.name).png")
        $copied++
    }
}
# pictures of names no longer in scenarios.json removed (renamed / dropped promo entries)
$known = @($defs | Where-Object { $_.PSObject.Properties['promo'] } | ForEach-Object { $_.promo } | ForEach-Object { "promo_$($_.name).png" })
foreach ($f in Get-ChildItem $out -Filter 'promo_*.png') {
    if ($known -notcontains $f.Name) { Remove-Item -LiteralPath $f.FullName; Write-Host "removed (no promo entry): $($f.Name)" }
}
# promo viewer (promo-viewer/index.html)
try { & python (Join-Path $repo 'scripts/update-promo-viewer.py') } catch { Write-Host "update-promo-viewer: $_" -ForegroundColor Yellow }
Write-Host "Promotion pictures: $copied -> $out"
