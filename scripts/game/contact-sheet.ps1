<#
.SYNOPSIS
    Contact sheets: pictures in a grid with their names, to look through many screenshots at once (choosing the
    promotion pictures, classifying the effects of docs/Effects).

.PARAMETER Images
    Picture files (paths or wildcards).

.PARAMETER Out
    Output folder (sheet-001.jpg, sheet-002.jpg...).

.EXAMPLE
    .\scripts\game\contact-sheet.ps1 -Images logs\autotest\20261117-074658\screens\*TS-0*.png -Out $env:TEMP\sheets
    .\scripts\game\contact-sheet.ps1 -Images docs\Effects\Skill\*.jpg -Out $env:TEMP\efx -Cols 6 -Rows 5 -Thumb 240
#>
param(
    [Parameter(Mandatory)][string[]]$Images,
    [Parameter(Mandatory)][string]$Out,
    [int]$Cols = 4,
    [int]$Rows = 3,
    [int]$Thumb = 400,
    [int]$LabelHeight = 22
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$files = @($Images | ForEach-Object { Get-ChildItem -Path $_ -File } | Sort-Object FullName -Unique)
if (-not $files) { Write-Host 'No picture.'; exit 0 }
New-Item -ItemType Directory -Force -Path $Out | Out-Null
$jpeg = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() | Where-Object { $_.MimeType -eq 'image/jpeg' }
$params = New-Object System.Drawing.Imaging.EncoderParameters 1
$params.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter ([System.Drawing.Imaging.Encoder]::Quality), 80L
$font = New-Object System.Drawing.Font 'Arial', 9
$per = $Cols * $Rows
$sheet = 0
for ($start = 0; $start -lt $files.Count; $start += $per) {
    $sheet++
    $batch = $files[$start..([Math]::Min($start + $per, $files.Count) - 1)]
    # cell: the picture keeps its ratio inside Thumb x Thumb*9/16 (screens) or a square (effects)
    $first = [System.Drawing.Image]::FromFile($batch[0].FullName)
    $cellH = [int]($Thumb * $first.Height / $first.Width)
    $first.Dispose()
    $bmp = New-Object System.Drawing.Bitmap ($Cols * $Thumb), ($Rows * ($cellH + $LabelHeight))
    $gfx = [System.Drawing.Graphics]::FromImage($bmp)
    $gfx.Clear([System.Drawing.Color]::Black)
    $gfx.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    for ($i = 0; $i -lt $batch.Count; $i++) {
        $x = ($i % $Cols) * $Thumb
        $y = [Math]::Floor($i / $Cols) * ($cellH + $LabelHeight)
        $img = [System.Drawing.Image]::FromFile($batch[$i].FullName)
        try { $gfx.DrawImage($img, $x, $y, $Thumb, $cellH) } finally { $img.Dispose() }
        $gfx.DrawString($batch[$i].BaseName, $font, [System.Drawing.Brushes]::Yellow, $x + 2, $y + $cellH + 3)
    }
    $file = Join-Path $Out ('sheet-{0:000}.jpg' -f $sheet)
    $bmp.Save($file, $jpeg, $params)
    $gfx.Dispose(); $bmp.Dispose()
}
Write-Host "$sheet sheet(s), $($files.Count) picture(s) -> $Out"
