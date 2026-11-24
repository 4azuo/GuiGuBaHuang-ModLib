<#
.SYNOPSIS
    Open a game / mod / log folder in Explorer.
.DESCRIPTION
    Paths are detected (Steam registry + libraryfolders.vdf) or read from
    scripts\settings.json of the repository (-RepoRoot, else the mod repository holding ModLib), so they work on any machine.
    Shared: GuiGuBaHuang-ModLib/scripts.
    Without -Target a numbered menu is shown.
.PARAMETER Target
    game            game install folder
    melonloader     <game>\MelonLoader
    melon-logs      <game>\MelonLoader\Logs
    workshop        Steam Workshop folder of the game (all subscribed mods)
    workshop-<id>   Workshop folder of a project, e.g. workshop-3385996759
    debug-<id>      <repo>\<id>\debug (build output), e.g. debug-3161035078
    game-data       %USERPROFILE%\AppData\LocalLow\guigugame\guigubahuang (Player.log)
    mod-logs        <game-data>\mod (ModLib debug logs, per save game)
    game-runs       <repo>\logs\game-runs (output of GuiGuBaHuang-ModLib\scripts\game\run-game.ps1)
.PARAMETER Print
    Only print the path, do not open Explorer.
.EXAMPLE
    .\GuiGuBaHuang-ModLib\scripts\open-folder.ps1 workshop-3385996759
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)][string]$Target,
    [switch]$Print,
    [string]$RepoRoot
)

$ErrorActionPreference = 'Stop'
if ($RepoRoot) { $env:MOD_REPO = (Resolve-Path -LiteralPath $RepoRoot).Path }
. (Join-Path $PSScriptRoot 'game\_game.ps1')

$p = Get-GamePaths
$targets = [ordered]@{
    'game'        = $p.GamePath
    'melonloader' = $p.MelonDir
    'melon-logs'  = if ($p.MelonDir) { Join-Path $p.MelonDir 'Logs' } else { $null }
    'workshop'    = $p.Workshop
}
foreach ($dir in @($p.Mod.Dependencies) + @($p.Projects)) {
    $id = Split-Path $dir -Leaf
    $targets["workshop-$id"] = if ($p.Workshop) { Join-Path $p.Workshop $id } else { $null }
}
foreach ($dir in @($p.Mod.Dependencies) + @($p.Projects)) {
    $targets["debug-$(Split-Path $dir -Leaf)"] = Join-Path $p.Repo "$dir\debug"
}
$targets['game-data'] = $p.GameData
$targets['mod-logs'] = $p.ModLogs
$targets['game-runs'] = $p.GameRuns

if (-not $Target) {
    $keys = @($targets.Keys)
    for ($i = 0; $i -lt $keys.Count; $i++) {
        Write-Host ('  {0,2}. {1,-22} {2}' -f ($i + 1), $keys[$i], $targets[$keys[$i]])
    }
    $answer = Read-Host 'Folder number'
    if (-not ($answer -as [int]) -or [int]$answer -lt 1 -or [int]$answer -gt $keys.Count) { Write-Err 'Invalid choice'; exit 1 }
    $Target = $keys[[int]$answer - 1]
}

if (-not $targets.Contains($Target)) {
    Write-Err "Unknown target '$Target'. Valid: $(@($targets.Keys) -join ', ')"
    exit 1
}

$path = $targets[$Target]
if (-not $path) { Write-Err "Path for '$Target' is unknown (game not found?)"; exit 1 }

if ($Print) { Write-Output $path; exit 0 }

# Open the nearest existing folder if the target does not exist yet
$open = $path
while ($open -and -not (Test-Path $open)) { $open = Split-Path $open -Parent }
if ($open -ne $path) { Write-Warn "Not found: $path - opening $open" }
if (-not $open) { Write-Err "Not found: $path"; exit 1 }

Write-Info "Opening $open"
Start-Process explorer.exe -ArgumentList "`"$open`""
exit 0
