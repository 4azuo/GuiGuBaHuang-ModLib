<#
.SYNOPSIS
    Effect galleries (TE-01 skill, TE-02 unit, TE-03 scene effects) in batches, on several games side by side.

.DESCRIPTION
    The effects of each gallery are cut into batches (-Batch effects each) put in one queue; every game slot (1 = the
    normal game, 2.. = copies made by new-game-slot.ps1) takes the next batch when it is free and runs it with
    run-autotest.ps1 -Slot <n> -NoSaveBackup (<autotest folder of the slot>\efx-range.txt = "start count"). Each run
    collects its pictures to GuiGuBaHuang-ModLib/docs/Effects (collect-effects.ps1). A batch stopped early (watchdog: the game stopped
    running the mod) goes back to the queue after the effect it stopped on, which is reported and skipped.
    Each game plays the test character of its slot, "AutoTest Slot<n>" (run-autotest.ps1 -SlotCharacter): created the
    first time, kept and played again by the next runs; the other saves are never loaded. The saves are backed up
    before (logs\effect-gallery\<time>\save-backup, to restore by hand when needed); only the global settings of the
    mods (mod\<id>\globals: S&M, new game options set by the character creation) are restored after.

    Memory: ~3 GB per game in a battle (4 games: ~12 GB).

.PARAMETER Id
    Galleries to run: TE-01, TE-02, TE-03 (default: TE-01).

.PARAMETER Slots
    Number of games side by side (1 to 4, default 4; ~3 GB of memory each).

.EXAMPLE
    .\scripts\game\run-effect-gallery.ps1 -Id TE-01 -Slots 3 -Deploy
    .\scripts\game\run-effect-gallery.ps1 -Id TE-02,TE-03 -Slots 2 -Batch 200
    .\scripts\game\run-effect-gallery.ps1 -Id TE-01 -Start 600 -End 900      # a part of a gallery
#>
param(
    [string[]]$Id = @('TE-01'),
    [ValidateRange(1, 4)][int]$Slots = 4,
    [int]$Batch = 100,
    [int]$Start = 0,
    [int]$End = -1,
    [int]$StallSeconds = 300,
    # window size of the games ("" = as set in the game); several games: windowed
    [string]$Window = '1600x900',
    [switch]$Deploy,
    # go on with an earlier gallery: only the effects without measures in GuiGuBaHuang-ModLib/docs/Effects/<kind>/index.json
    [switch]$Resume,
    # the user confirmed the gallery: no prompt
    [switch]$ConfirmTools
)

$ErrorActionPreference = 'Stop'
# The games run in the game slots 2..Slots+1 (new-game-slot.ps1): every slot has its own game data folder
# (tools/SlotDataPath: ...\LocalLow\guigugame\guigubahuang-slot<n>); the normal game (slot 1) and its saves are not used
# (docs/Known-Issues.md, "Games side by side lose saves").
$slotIds = @(2..($Slots + 1))
. (Join-Path $PSScriptRoot '_game.ps1')
. (Join-Path $PSScriptRoot '_parallel.ps1')
$paths = Get-GamePaths
$repo = $paths.Repo
$lists = @{ 'TE-01' = '_Def_Res_Efx_Battle_Skill.txt'; 'TE-02' = '_Def_Res_Efx_Battle_Unit.txt'; 'TE-03' = '_Def_Res_Efx_Battle_Scenes.txt' }
$Id = @($Id | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim().ToUpper() } | Where-Object { $_ })
foreach ($g in $Id) { if (-not $lists.ContainsKey($g)) { throw "Unknown gallery $g (TE-01, TE-02, TE-03)" } }

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$work = Join-Path $repo "logs\effect-gallery\$stamp"
New-Item -ItemType Directory -Path $work -Force | Out-Null
$queueFile = Join-Path $work 'queue.json'
$logFile = Join-Path $work 'gallery.log'
function Write-Log([string]$Text) { Write-ParallelLog $logFile $Text }

# ------------------------------------------------------------------ queue
# Tools (kind "tool": the effect galleries TE-xx, the investigations TI-xx) run for a long time: only once the user
# confirmed it - a prompt in an interactive terminal, -ConfirmTools otherwise (never just because the scenarios ran).
function Confirm-ToolRun([string[]]$Ids, [switch]$Confirmed) {
    if ($Ids.Count -eq 0 -or $Confirmed) { return $true }
    $interactive = [Environment]::UserInteractive -and -not [Console]::IsInputRedirected -and $Host.Name -eq 'ConsoleHost'
    if ($interactive) {
        $a = Read-Host "Run the tool scenario(s) $($Ids -join ', ') (long, records the game)? [y/N]"
        return $a -match '^(y|yes)$'
    }
    Write-Host "Tool scenario(s) $($Ids -join ', '): not run without the user's confirmation (add -ConfirmTools)." -ForegroundColor Yellow
    return $false
}

if (-not (Confirm-ToolRun $Id -Confirmed:$ConfirmTools)) { exit 0 }

$kinds = @{ 'TE-01' = 'Skill'; 'TE-02' = 'Unit'; 'TE-03' = 'Scenes' }
$queue = New-Object System.Collections.Generic.List[object]
foreach ($g in $Id) {
    $total = @(Get-Content -LiteralPath (Join-Path $repo "$($paths.Mod.Project)\ModProject\ModConf\$($lists[$g])") | Where-Object { $_.Trim() }).Count
    $to = if ($End -ge 0) { [Math]::Min($End, $total) } else { $total }
    if (-not $Resume) {
        for ($i = $Start; $i -lt $to; $i += $Batch) {
            # the last batch of a whole list takes everything after it (the fresh list of the game may be longer)
            $count = if ($End -lt 0 -and $i + $Batch -ge $to) { 100000 } else { [Math]::Min($Batch, $to - $i) }
            $queue.Add([pscustomobject]@{ id = $g; start = $i; count = $count })
        }
        continue
    }
    # -Resume: done = measures in index.json (a webp alone is not enough: its measures may be lost; an invisible effect
    # has measures but no webp); the missing effects cut into batches of consecutive indexes (a batch is "start count")
    $dir = Join-Path $paths.ModLib "docs\Effects\$($kinds[$g])"
    $done = New-Object 'System.Collections.Generic.HashSet[int]'
    $idxFile = Join-Path $dir 'index.json'
    if (Test-Path -LiteralPath $idxFile) {
        # PowerShell 5.1: ConvertFrom-Json gives a JSON array as one object; a variable enumerates it
        $entries = ConvertFrom-Json (Get-Content -LiteralPath $idxFile -Raw -Encoding UTF8)
        foreach ($e in $entries) {
            if ($null -ne $e.PSObject.Properties['visible'] -and $null -ne $e.visible) { [void]$done.Add([int]$e.index) }
        }
    }
    $i = $Start
    while ($i -lt $to) {
        if ($done.Contains($i)) { $i++; continue }
        $first = $i
        while ($i -lt $to -and -not $done.Contains($i) -and ($i - $first) -lt $Batch) { $i++ }
        $queue.Add([pscustomobject]@{ id = $g; start = $first; count = $i - $first })
    }
    Write-Host "$g`: $($done.Count) of $total effects done, $(@($queue | Where-Object { $_.id -eq $g } | ForEach-Object { $_.count } | Measure-Object -Sum).Sum) to record"
}
Write-Queue $queueFile $queue.ToArray()

Enter-ParallelLock $repo
$backups = @{}
try {
    Write-Log "$($queue.Count) batch(es) of $($Id -join ', ') on $Slots game(s); work folder $work"
    # ------------------------------------------------------------------ deploy, slots, window, backup
    if ($Deploy) {
        # one deploy at a time, the copy only when no game runs (or skipped: same build) - _slots.ps1
        Invoke-SafeDeploy $repo $paths.Workshop $paths.Mod.Projects $paths.Mod.Dependencies | Out-Null
        Write-Log 'built and deployed'
    }
    # the slots of this run, locked until it ends (children: GGBH_LOCK_PARENT)
    foreach ($s in $slotIds) { [void](Select-GameSlot -Requested $s -WaitMinutes 120 -What 'run-effect-gallery') }
    $env:GGBH_LOCK_PARENT = "$PID"
    foreach ($s in $slotIds) { & (Join-Path $PSScriptRoot 'new-game-slot.ps1') -Slot $s | Out-Null }
    $gameArgs = Set-GameWindow $repo $Window
    $backups = Backup-Saves $paths (Join-Path $work 'save-backup')
    Remove-Item -LiteralPath (Join-Path $paths.ModLogs "$($paths.Mod.Id)\autotest-shared\creating.lock") -Force -ErrorAction SilentlyContinue
    Write-Log "saves backed up; games $(if ($gameArgs) { "windowed $Window" } else { 'as set in the game' })"

    # ------------------------------------------------------------------ workers (one per slot)
    $worker = {
        param($Slot, $Repo, $QueueFile, $LogFile, $StallSeconds, $AutotestDir, $GameArgs, $Delay, $Window)
        $ErrorActionPreference = 'Continue'
        . (Join-Path $Repo 'scripts\game\_game.ps1')
        . (Join-Path $Repo 'scripts\game\_parallel.ps1')
        function Log([string]$t) { Write-ParallelLog $LogFile "[slot $Slot] $t" }
        # slots start apart: the game start (assemblies, Workshop) is the heaviest part
        Start-Sleep -Seconds $Delay
        while ($true) {
            $item = Use-Queue $QueueFile { param($l) if ($l.Count -eq 0) { return $null }; $x = $l[0]; $l.RemoveAt(0); return $x }
            if (-not $item) { Log 'queue empty: done'; break }
            New-Item -ItemType Directory -Path $AutotestDir -Force | Out-Null
            [IO.File]::WriteAllText((Join-Path $AutotestDir 'efx-range.txt'), "$($item.start) $($item.count)")
            Log "$($item.id) from $($item.start) ($($item.count))"
            $out = & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $Repo 'scripts\game\run-autotest.ps1') `
                -Scenario $item.id -Slot $Slot -NoSaveBackup -SlotCharacter -NoViewerUpdate -ConfirmTools -Timeout 7200 -StallSeconds $StallSeconds -GameArgs $GameArgs -Window $Window 2>&1 | ForEach-Object { "$_" }
            $runLine = $out | Where-Object { $_ -match 'Run:\s+(\S+)' } | Select-Object -First 1
            $runId = if ($runLine -match 'Run:\s+(\S+)') { $Matches[1] } else { $null }
            $last = $null
            if ($runId) {
                $meta = Join-Path $Repo "logs\autotest\$runId\effects-meta.jsonl"
                if (Test-Path -LiteralPath $meta) {
                    $tail = Get-Content -LiteralPath $meta -Tail 1
                    if ($tail) { $last = [int]($tail | ConvertFrom-Json).index }
                }
            }
            $made = ($out | Where-Object { $_ -match 'webp made' }) -join ' '
            $status = ($out | Where-Object { $_ -match 'Runner status|stalled' }) -join ' | '
            Log "$($item.id) from $($item.start): run $runId, last effect $last, $made, $status"
            $end = $item.start + $item.count - 1
            if ($null -eq $last) { $last = $item.start - 1 }
            $listDone = ($out | Where-Object { $_ -match 'Runner status: DONE' }).Count -gt 0
            if (-not $listDone -and $last -lt $end) {
                $stalled = ($out | Where-Object { $_ -match 'stalled' }).Count -gt 0
                $tries = if ($item.PSObject.Properties['tries']) { [int]$item.tries } else { 0 }
                if ($stalled) {
                    # the game stopped running the mod on the effect after the last one measured: skipped, the rest queued
                    $from = $last + 2
                    Log "!!! $($item.id): the game stopped on effect $($last + 1) - skipped"
                    $tries = 0
                }
                else {
                    # an other failure (game start, save load...): the rest tried again, twice at most
                    $from = $last + 1
                    $tries++
                    if ($tries -gt 2) { Log "!!! $($item.id): effects $from to $end not done (failed $tries times)"; continue }
                    Log "$($item.id): run failed before the end - effects from $from queued again (try $tries)"
                }
                if ($from -le $end) {
                    $rest = [pscustomobject]@{ id = $item.id; start = $from; count = $end - $from + 1; tries = $tries }
                    Use-Queue $QueueFile { param($l) $l.Insert(0, $rest); return $null } | Out-Null
                }
            }
        }
    }
    $jobs = foreach ($s in $slotIds) {
        $env:GGBH_SLOT = "$s"
        $atDir = Join-Path (Get-GamePaths).ModLogs "$($paths.Mod.Id)\autotest-slot$s"
        $env:GGBH_SLOT = $null
        Start-Job -Name "gallery-slot$s" -ScriptBlock $worker -ArgumentList $s, $repo, $queueFile, $logFile, $StallSeconds, $atDir, $gameArgs, (($s - 2) * 45), $Window
    }
    while (@($jobs | Where-Object { $_.State -eq 'Running' }).Count -gt 0) {
        $jobs | Receive-Job -ErrorAction Continue 2>&1 | Out-Null
        Start-Sleep -Seconds 5
    }
    $jobs | Remove-Job -Force -ErrorAction SilentlyContinue
}
finally {
    Stop-ParallelChildren $slotIds
    foreach ($s in $slotIds) {
        $env:GGBH_SLOT = "$s"
        $atDir = Join-Path (Get-GamePaths).ModLogs "$($paths.Mod.Id)\autotest-slot$s"
        $env:GGBH_SLOT = $null
        Remove-Item -LiteralPath (Join-Path $atDir 'efx-range.txt') -ErrorAction SilentlyContinue
    }
    # the saves stay (the test characters are played again next time); the global settings of the mods come back
    if ($backups.Count -gt 0) { Restore-ModGlobals $backups }
    Exit-ParallelLock $repo
    Write-Log 'games closed; global settings of the mods restored (saves and window kept)'
}
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot '..\..\test-viewer\update-data.ps1') -RepoRoot $repo | Out-Null
$pics = @(Get-ChildItem (Join-Path $paths.ModLib 'docs\Effects') -Recurse -Filter *.webp -ErrorAction SilentlyContinue).Count
Write-Log "done: $pics webp in GuiGuBaHuang-ModLib/docs/Effects; skipped effects: $(@(Select-String -LiteralPath $logFile -Pattern '!!!').Count) (see $logFile)"
