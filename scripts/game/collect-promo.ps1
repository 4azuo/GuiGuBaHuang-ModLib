<#
.SYNOPSIS
    Copies the promotion screenshots of an autotest run to
    <mod project>/ModProject/SteamImg/promo-<version>/promo_<scenario>_<name>.jpg (Steam Workshop / Ko-fi pictures; one
    folder per version of the mod they promote, e.g. promo-6.0.00/promo_TR-04_clan-battle.jpg, the files of a folder
    are uploaded together). The pictures are marked in <mod project>/tests/scenarios.json, per scenario:
        "promoVersion": "6.0.00",
        "promo": [ { "step": "NPCs-joined", "nth": 1, "name": "battle_npcs_join", "caption": "..." } ]
    step = the step part of the screenshot name (NNN_<id>_SS-<step>.png), nth = which one when a step has several
    screenshots (dramas of a storyline: 1 = the first). Can be run alone on an earlier run.
    The other pictures of SteamImg (1.jpg...) are not touched.
    Called by run-autotest.ps1 -Promo (scripts/game/run-promo.ps1) only: the test runs never write SteamImg.
    Only pictures go to SteamImg (it is deployed with the mod): the captions stay in scenarios.json, read by the
    marketing video (marketing/video-6.0.0) and the promo viewer.

.PARAMETER Run
    Folder of the autotest run (logs/autotest/<time>); default: the latest.

.EXAMPLE
    .\scripts\game\collect-promo.ps1 -Run logs\autotest\20261117-074658
#>
param([string]$Run)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_game.ps1')
$mod = Get-ModInfo
$repo = $mod.Repo
if (-not $Run) {
    $Run = Get-ChildItem (Join-Path $repo 'logs\autotest') -Directory | Where-Object { $_.Name -match '^\d{8}-\d{6}(-s\d+)?$' } |
        Sort-Object Name -Descending | Select-Object -First 1 -ExpandProperty FullName
}
$screens = Join-Path $Run 'screens'
if (-not (Test-Path $screens)) { Write-Host "No screenshots in $Run (nothing to copy)" }
$defs = (Get-Content (Join-Path $mod.Tests 'scenarios.json') -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios
$out = Join-Path $repo "$($mod.Project)\ModProject\SteamImg"
New-Item -ItemType Directory -Force -Path $out | Out-Null
$all = if (Test-Path $screens) { @(Get-ChildItem $screens -Filter '*.png') } else { @() }
Add-Type -AssemblyName System.Drawing

# Steam Workshop refuses pictures over 2 MB: JPEG, the quality lowered until the file is under the limit
$maxBytes = 1900KB
$jpegCodec = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
function Save-Jpeg([System.Drawing.Image]$bmp, [string]$dst) {
    foreach ($q in 92, 88, 84, 80, 75, 70, 65, 60) {
        $params = New-Object System.Drawing.Imaging.EncoderParameters 1
        $params.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality), ([long]$q)
        $bmp.Save($dst, $jpegCodec, $params)
        if ((Get-Item -LiteralPath $dst).Length -le $maxBytes) { return }
    }
}

# the game area of a window screenshot: without the title bar and the borders (16:9 at the bottom of the window)
Add-Type -AssemblyName System.Windows.Forms
$screenSizes = @([System.Windows.Forms.Screen]::AllScreens | ForEach-Object { "$($_.Bounds.Width)x$($_.Bounds.Height)" })
function Save-GameArea([string]$src, [string]$dst) {
    $img = [System.Drawing.Image]::FromFile($src)
    try {
        # a picture of a whole screen is not the game window (an older run-autotest copied the screen when it could
        # not capture the window: the tester's desktop and browser): never published
        if ($screenSizes -contains "$($img.Width)x$($img.Height)") {
            Write-Host "skipped (a whole screen, not the game window): $(Split-Path $src -Leaf)" -ForegroundColor Yellow
            return $false
        }
        $border = 8
        $w = $img.Width - 2 * $border
        $h = [int]($w * 9 / 16)
        $top = $img.Height - $border - $h
        if ($w -le 0 -or $top -lt 0) { Save-Jpeg $img $dst; return $true }
        $bmp = New-Object System.Drawing.Bitmap $w, $h
        $gfx = [System.Drawing.Graphics]::FromImage($bmp)
        $gfx.DrawImage($img, (New-Object System.Drawing.Rectangle 0, 0, $w, $h), (New-Object System.Drawing.Rectangle $border, $top, $w, $h), [System.Drawing.GraphicsUnit]::Pixel)
        Save-Jpeg $bmp $dst
        $gfx.Dispose(); $bmp.Dispose()
        return $true
    } finally { $img.Dispose() }
}
# scenarios that ran to the end (results.json): only they may drop an older picture they skipped; a scenario that
# stopped on an error keeps the pictures of its steps not reached
$finished = @()
$skipped = @()
$resultsFile = Join-Path $Run 'results.json'
if (Test-Path $resultsFile) {
    try {
        $finished = @((Get-Content $resultsFile -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios |
            Where-Object { $_.Status -in @('PASS', 'OK', 'SKIP', 'DONE') } | ForEach-Object { $_.Id })
        # a skipped test (e.g. TP-01 on an existing save) still takes the screenshot of each step, of whatever is on
        # the screen: none of them (a tool reports SKIP after taking its pictures: kept)
        $skipped = @((Get-Content $resultsFile -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios |
            Where-Object { $_.Status -eq 'SKIP' -and $_.Kind -ne 'Tool' } | ForEach-Object { $_.Id })
    } catch { Write-Host "results.json: $_" -ForegroundColor Yellow }
}
# SteamImg/promo-<version>/promo_<scenario>_<name>.jpg (the version: "promoVersion" of the scenario)
function Get-PromoFile($d, $p) {
    $ver = if ($d.PSObject.Properties['promoVersion'] -and $d.promoVersion) { $d.promoVersion } else { 'other' }
    return "promo-$ver\promo_$($d.id)_$($p.name).jpg"
}
$copied = 0
foreach ($d in $defs | Where-Object { $_.PSObject.Properties['promo'] }) {
    if ($skipped -contains $d.id) { continue }
    foreach ($p in $d.promo) {
        $file = Get-PromoFile $d $p
        # step "drama" = any step of that name; "07-drama" = step 07 only
        $pattern = if ($p.step -match '^\d+-') { "^\d+_$([regex]::Escape($d.id))_$([regex]::Escape($p.step))\.png$" }
            else { "^\d+_$([regex]::Escape($d.id))_\d+-$([regex]::Escape($p.step))\.png$" }
        $shots = @($all | Where-Object { $_.Name -match $pattern } | Sort-Object Name)
        $nth = if ($p.PSObject.Properties['nth'] -and $p.nth) { [int]$p.nth } else { 1 }
        if ($shots.Count -lt $nth) {
            # the scenario ran but skipped this picture (nothing to show in this save): an older picture is removed
            $old = Join-Path $out $file
            if ((Test-Path $old) -and $finished -contains $d.id -and @($all | Where-Object { $_.Name -match "^\d+_$([regex]::Escape($d.id))_" }).Count -gt 0) {
                Remove-Item -LiteralPath $old; Write-Host "removed (not taken in this run): $file"
            }
            continue
        }
        New-Item -ItemType Directory -Force -Path (Split-Path (Join-Path $out $file)) | Out-Null
        if (-not (Save-GameArea $shots[$nth - 1].FullName (Join-Path $out $file))) { continue }
        $copied++
    }
}
# pictures no longer in scenarios.json removed (renamed / dropped promo entries, a scenario moved to another
# version), also the promo pictures of the former formats at the root of SteamImg; empty version folders removed
$known = @($defs | Where-Object { $_.PSObject.Properties['promo'] } | ForEach-Object { $d = $_; $_.promo | ForEach-Object { Get-PromoFile $d $_ } })
foreach ($f in Get-ChildItem $out -Recurse -File -Filter 'promo_*.*' | Where-Object { $_.Extension -in '.png', '.jpg' }) {
    $rel = $f.FullName.Substring($out.Length).TrimStart('\', '/')
    if ($known -notcontains $rel) { Remove-Item -LiteralPath $f.FullName; Write-Host "removed (no promo entry): $rel" }
}
Get-ChildItem $out -Directory -Filter 'promo-*' | Where-Object { -not (Get-ChildItem $_.FullName -Force) } | Remove-Item
# promo viewer (logs/promo-viewer/index.html)
try { & python (Join-Path $PSScriptRoot '..\..\promo-viewer\update-data.py') --repo $repo } catch { Write-Host "update-promo-viewer: $_" -ForegroundColor Yellow }
Write-Host "Promotion pictures: $copied -> $out"
