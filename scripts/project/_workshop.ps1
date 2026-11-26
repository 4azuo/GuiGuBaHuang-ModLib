# Workshop folder of a mod (dot-sourced by copy-to-steam, copy-from-steam, copy-both-to-steam, copy-both-from-steam,
# game\_slots.ps1).
# <workshop>\<project id> holds:
#   the content of debug\Mod_<id>   the build output (ModCode\dll, ModConf, ModImg, ModExportData.cache...): what the
#                                   game loads
#   ModProject\<3 files>            the project files of the game's mod editor: ModProject.cache (name, author,
#                                   description, version, tags, Workshop item), ModData.cache (the content made with the
#                                   editor's tables), ModProjectPreview.png (picture of the project)
# No source (ModProject\ModCode...), configs sources, scripts or tests: the Workshop item stays small.

# Files of the ModProject folder copied with the build output
$script:WORKSHOP_PROJECT_FILES = @('ModProject.cache', 'ModData.cache', 'ModProjectPreview.png')

# The build output folder of a project: its single debug\Mod_<id>
function Get-ModBuildFolder([string]$ProjectDir) {
    $debug = Join-Path $ProjectDir 'debug'
    if (-not (Test-Path -LiteralPath $debug)) { throw "No build output: $debug (build the project first)" }
    $mods = @(Get-ChildItem -LiteralPath $debug -Directory -Filter 'Mod_*')
    if ($mods.Count -ne 1) { throw "Expected one Mod_<id> folder in ${debug}, found $($mods.Count)" }
    # same spelling as the FullName of its items (short 8.3 names): relative paths are cut from it
    return $mods[0].FullName
}

# Copies a project to its Workshop folder (the former copy removed first); returns the number of files not copied
# (in use by a running game)
function Copy-ToWorkshop([string]$ProjectDir, [string]$Destination) {
    $failed = 0
    $build = Get-ModBuildFolder $ProjectDir
    if (Test-Path -LiteralPath $Destination) { Remove-Item -LiteralPath $Destination -Recurse -Force -ErrorAction SilentlyContinue }
    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    foreach ($item in Get-ChildItem -LiteralPath $build -Recurse) {
        $rel = $item.FullName.Substring($build.Length)
        $target = Join-Path $Destination $rel
        if ($item.PSIsContainer) {
            if (-not (Test-Path -LiteralPath $target)) { New-Item -ItemType Directory -Path $target -Force | Out-Null }
            continue
        }
        $dir = Split-Path $target -Parent
        if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
        try { Copy-Item -LiteralPath $item.FullName -Destination $target -Force -ErrorAction Stop }
        catch { $failed++; Write-Host "Skipped (in use): $rel" -ForegroundColor Yellow }
    }
    $projTarget = Join-Path $Destination 'ModProject'
    foreach ($name in $script:WORKSHOP_PROJECT_FILES) {
        $src = Join-Path (Join-Path $ProjectDir 'ModProject') $name
        if (-not (Test-Path -LiteralPath $src)) { Write-Host "Missing: ModProject\$name" -ForegroundColor Yellow; continue }
        if (-not (Test-Path -LiteralPath $projTarget)) { New-Item -ItemType Directory -Path $projTarget -Force | Out-Null }
        try { Copy-Item -LiteralPath $src -Destination (Join-Path $projTarget $name) -Force -ErrorAction Stop }
        catch { $failed++; Write-Host "Skipped (in use): ModProject\$name" -ForegroundColor Yellow }
    }
    return $failed
}

# Copies a Workshop folder back into its project: ModProject\<3 files> into ModProject, the rest into debug\Mod_<id>;
# returns the number of files not copied (in use)
function Copy-FromWorkshop([string]$Source, [string]$ProjectDir) {
    $failed = 0
    $build = Get-ModBuildFolder $ProjectDir
    $Source = (Get-Item -LiteralPath $Source).FullName
    foreach ($item in Get-ChildItem -LiteralPath $Source -Recurse) {
        $rel = $item.FullName.Substring($Source.Length).TrimStart('\')
        if ($item.PSIsContainer) {
            if ($rel -eq 'ModProject') { continue }
            $target = Join-Path $build $rel
            if (-not (Test-Path -LiteralPath $target)) { New-Item -ItemType Directory -Path $target -Force | Out-Null }
            continue
        }
        $name = $rel -replace '^ModProject\\', ''
        $target = if ($name -ne $rel -and $script:WORKSHOP_PROJECT_FILES -contains $name) { Join-Path (Join-Path $ProjectDir 'ModProject') $name } else { Join-Path $build $rel }
        $dir = Split-Path $target -Parent
        if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
        try { Copy-Item -LiteralPath $item.FullName -Destination $target -Force -ErrorAction Stop }
        catch { $failed++; Write-Host "Skipped (in use): $rel" -ForegroundColor Yellow }
    }
    return $failed
}
