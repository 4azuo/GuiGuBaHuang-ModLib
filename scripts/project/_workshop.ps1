# Workshop folder of a mod (dot-sourced by copy-to-steam, copy-both-to-steam).
# <workshop>\<project id> holds:
#   debug\                 the build output (debug\Mod_<id>): what the game loads
#   ModProject\<3 files>   the project files of the game's mod editor: ModProject.cache (name, author, description,
#                          version, tags, Workshop item), ModData.cache (the content made with the editor's tables),
#                          ModProjectPreview.png (picture of the project)
# No source (ModProject\ModCode...), configs sources, scripts or tests: the Workshop item stays small.

# Files of the ModProject folder copied with the build output
$script:WORKSHOP_PROJECT_FILES = @('ModProject.cache', 'ModData.cache', 'ModProjectPreview.png')

# Copies a project to its Workshop folder (the former copy removed first); returns the number of files not copied
# (in use by a running game)
function Copy-ToWorkshop([string]$ProjectDir, [string]$Destination) {
    $failed = 0
    $debug = Join-Path $ProjectDir 'debug'
    if (-not (Test-Path -LiteralPath $debug)) { throw "No build output: $debug (build the project first)" }
    if (Test-Path -LiteralPath $Destination) { Remove-Item -LiteralPath $Destination -Recurse -Force -ErrorAction SilentlyContinue }
    # same spelling as the FullName of its items (short 8.3 names): relative paths are cut from it
    $debug = (Get-Item -LiteralPath $debug).FullName
    $target0 = Join-Path $Destination 'debug'
    New-Item -ItemType Directory -Path $target0 -Force | Out-Null
    foreach ($item in Get-ChildItem -LiteralPath $debug -Recurse) {
        $target = Join-Path $target0 $item.FullName.Substring($debug.Length)
        if ($item.PSIsContainer) {
            if (-not (Test-Path -LiteralPath $target)) { New-Item -ItemType Directory -Path $target -Force | Out-Null }
            continue
        }
        $dir = Split-Path $target -Parent
        if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
        try { Copy-Item -LiteralPath $item.FullName -Destination $target -Force -ErrorAction Stop }
        catch { $failed++; Write-Host "Skipped (in use): debug$($item.FullName.Substring($debug.Length))" -ForegroundColor Yellow }
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
