<#
.SYNOPSIS
    Makes a "game slot": a second (third...) copy of the game folder that can run next to the normal one, for parallel
    autotest runs. Nothing of the real game folder is changed.

.DESCRIPTION
    The game is built with Unity's "Force Single Instance" (PlayerSettings.forceSingleInstance): a second guigubahuang.exe
    exits at once. A slot is %LOCALAPPDATA%\ggbh-slot<N> (ASCII path, MelonLoader needs it):
    - the big parts are shared: files hard-linked (same drive) or copied, folders as junctions (no disk space);
    - guigubahuang_Data\globalgamemanagers copied with forceSingleInstance = 0 (the byte 4 bytes before the length of the
      string "public.app-category...", checked before it is changed);
    - MelonLoader\ with its own Latest.log / Logs (its other folders are junctions).
    The saves and mod data (%USERPROFILE%\AppData\LocalLow\guigugame\guigubahuang) stay shared: run the slots on different
    characters (see run-autotest.ps1 -Slot). Run it again after a game update (-Force rebuilds the slot).

.EXAMPLE
    .\scripts\game\new-game-slot.ps1 -Slot 2
#>
param(
    [Parameter(Mandatory)][ValidateRange(2, 9)][int]$Slot,
    [string]$GamePath,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '_game.ps1')
$paths = Get-GamePaths -GamePath $GamePath
$src = $paths.GamePath
$dst = Join-Path $env:LOCALAPPDATA "ggbh-slot$Slot"
if ($dst -match '[^\x00-\x7F]') { throw "The slot path must be ASCII (MelonLoader): $dst" }

if (Test-Path -LiteralPath $dst) {
    if (-not $Force) { Write-Info "Slot $Slot already exists: $dst (-Force to rebuild)"; return }
    # junctions first (Remove-Item -Recurse would follow them into the game folder)
    Get-ChildItem -LiteralPath $dst -Recurse -Force -Attributes ReparsePoint | Sort-Object { $_.FullName.Length } -Descending |
        ForEach-Object { [IO.Directory]::Delete($_.FullName) }
    Remove-Item -LiteralPath $dst -Recurse -Force
}

function Link-File([string]$From, [string]$To) {
    try { New-Item -ItemType HardLink -Path $To -Target $From | Out-Null }
    catch { Copy-Item -LiteralPath $From -Destination $To }
}

# mirrors a folder: files linked, sub folders as junctions, except the ones in $Own (made as real folders by the caller)
function Mirror-Folder([string]$From, [string]$To, [string[]]$Own = @(), [string[]]$Skip = @()) {
    New-Item -ItemType Directory -Path $To -Force | Out-Null
    foreach ($item in Get-ChildItem -LiteralPath $From -Force) {
        if ($Own -contains $item.Name -or $Skip -contains $item.Name) { continue }
        $target = Join-Path $To $item.Name
        if ($item.PSIsContainer) { New-Item -ItemType Junction -Path $target -Target $item.FullName | Out-Null }
        else { Link-File $item.FullName $target }
    }
}

Write-Info "Slot $Slot`: $src -> $dst"
Mirror-Folder $src $dst -Own @('guigubahuang_Data', 'MelonLoader', 'Plugins')
Mirror-Folder (Join-Path $src 'guigubahuang_Data') (Join-Path $dst 'guigubahuang_Data') -Skip @('globalgamemanagers')
Mirror-Folder (Join-Path $src 'MelonLoader') (Join-Path $dst 'MelonLoader') -Skip @('Latest.log', 'Logs')
New-Item -ItemType Directory -Path (Join-Path $dst 'MelonLoader\Logs') -Force | Out-Null

# globalgamemanagers without "Force Single Instance"
$ggm = Join-Path $src 'guigubahuang_Data\globalgamemanagers'
$bytes = [IO.File]::ReadAllBytes($ggm)
$marker = [Text.Encoding]::ASCII.GetBytes('public.app-category')
$at = -1
for ($i = 0; $i -le $bytes.Length - $marker.Length; $i++) {
    $ok = $true
    for ($j = 0; $j -lt $marker.Length; $j++) { if ($bytes[$i + $j] -ne $marker[$j]) { $ok = $false; break } }
    if ($ok) { $at = $i; break }
}
if ($at -lt 8) { throw "PlayerSettings not recognized in $ggm (game updated?): the slot cannot run next to the game" }
# ... usePlayerLog, bakeCollisionMeshes, forceSingleInstance, useFlipModelSwapchain, resizableWindow,
# useMacAppStoreValidation, <int length of macAppStoreCategory>, "public.app-category..."
$flag = $at - 4 - 4
if ($bytes[$flag] -gt 1 -or $bytes[$flag - 2] -gt 1 -or $bytes[$flag + 1] -gt 1) { throw "Unexpected bytes around forceSingleInstance in $ggm" }
Write-Info "forceSingleInstance at byte $flag`: $($bytes[$flag]) -> 0"
$bytes[$flag] = 0
[IO.File]::WriteAllBytes((Join-Path $dst 'guigubahuang_Data\globalgamemanagers'), $bytes)
# its own game data folder: the SlotDataPath plugin (tools/SlotDataPath), in a Plugins folder of the slot only
$plugin = Join-Path $paths.Repo 'tools\SlotDataPath\bin\SlotDataPath.dll'
if (-not (Test-Path -LiteralPath $plugin)) {
    & dotnet build (Join-Path $paths.Repo 'tools\SlotDataPath\SlotDataPath.csproj') -c Release | Out-Null
    if (-not (Test-Path -LiteralPath $plugin)) { throw "SlotDataPath plugin not built ($plugin)" }
}
$plugins = Join-Path $dst 'Plugins'
New-Item -ItemType Directory -Path $plugins -Force | Out-Null
Copy-Item -LiteralPath $plugin -Destination $plugins -Force
Write-Info "game data folder of the slot: $(Join-Path $env:USERPROFILE "AppData\LocalLow\guigugame\guigubahuang-slot$Slot") (SlotDataPath plugin)"
Write-Ok "Slot $Slot ready: $dst"
