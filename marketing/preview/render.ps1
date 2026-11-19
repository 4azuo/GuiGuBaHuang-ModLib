<#
.SYNOPSIS
    Renders the preview images of the mods from marketing/preview/preview.html (Edge headless, 2x pixels):
    - <mod>/ModProject/SteamImg/Preview.png      square 1024 x 1024 (Steam Workshop preview)
    - <mod>/ModProject/ModProjectPreview.png     banner 1092 x 300 (title of the mod in the game's mod list)
    for Taoist (3161035078) and ModLib (3385996759).
.EXAMPLE
    .\marketing\preview\render.ps1
#>
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$edge = "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe"
if (-not (Test-Path $edge)) { $edge = "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe" }
$page = ([Uri](Join-Path $PSScriptRoot 'preview.html')).AbsoluteUri
$mods = @{ taoist = '3161035078'; modlib = '3385996759' }
foreach ($m in $mods.Keys) {
    $proj = Join-Path $repo "$($mods[$m])\ModProject"
    foreach ($k in @(@{ kind = 'square'; w = 512; h = 512; out = 'SteamImg\Preview.png' }, @{ kind = 'banner'; w = 546; h = 150; out = 'ModProjectPreview.png' })) {
        $out = Join-Path $proj $k.out
        New-Item -ItemType Directory -Force -Path (Split-Path $out) | Out-Null
        & $edge --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=2 "--window-size=$($k.w),$($k.h)" "--screenshot=$out" "$page`?mod=$m&kind=$($k.kind)" 2>$null | Out-Null
        Start-Sleep -Milliseconds 800
        Write-Host "$m $($k.kind) -> $out ($([math]::Round((Get-Item $out).Length / 1KB)) KB)"
    }
}
