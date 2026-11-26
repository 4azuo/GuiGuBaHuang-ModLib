# Workshop folder of a mod (dot-sourced by copy-to-steam, copy-from-steam, copy-both-*, game/_slots.ps1).
# <workshop>\<project id> holds what the game loads, as a Workshop download does: the content of the build output
# <project>\debug\Mod_<id> (ModCode\dll, ModConf, ModImg, SteamImg, ModExportData.cache, ModProjectPreview.png).
# No source, scripts or tests: the Workshop item stays small.

# The build output of a project: the only debug\Mod_* folder of <project dir>; $null when none or several
function Get-ModBuildFolder([string]$ProjectDir) {
    $debug = Join-Path $ProjectDir 'debug'
    $found = @(Get-ChildItem -LiteralPath $debug -Directory -Filter 'Mod_*' -ErrorAction SilentlyContinue)
    if ($found.Count -ne 1) { return $null }
    return $found[0].FullName
}
