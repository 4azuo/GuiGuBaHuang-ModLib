<#
.SYNOPSIS
    Autotest of the mod on several games side by side (game slots, new-game-slot.ps1): the scenarios are cut into groups that
    the games take from one queue.

.DESCRIPTION
    - Every slot plays its own test character "AutoTest Slot<n>" (run-autotest.ps1 -SlotCharacter), kept between runs.
      Its save is snapshot once (when created, or when there is no snapshot: <mod cache>\autotest-shared\snapshots) and
      restored before every run, so every group starts from the same fresh character.
    - Groups: the scenarios sorted by their last duration (the longest first), ~GroupSeconds of work each; a free game
      takes the next group (run-autotest.ps1 -Slot <n> -NoSaveBackup -NoViewerUpdate). TP-01 runs only when a slot
      creates its character.
    - "exclusive" scenarios (scenarios.json: they change the global settings of the mods, which every game reads) run
      after the others, alone, on the first slot.
    - Only the log of the slot's character counts for the "errors in the game log" check (the mod logs are shared).
    - scenario-status.json is updated by every run under a lock; the test viewer once at the end; a merged summary in
      logs\autotest-parallel\<time>\summary.json.
    - The games are windowed (-Window, default 1600x900); the saves are backed up once (kept after: test characters);
      the global settings of the mods are restored after (the window is kept).
    One parallel run at a time (logs\parallel.lock); a stop by force: stop the process tree of this script.

.EXAMPLE
    .\scripts\game\run-autotest-parallel.ps1 -Deploy                     # every auto scenario on 4 games
    .\scripts\game\run-autotest-parallel.ps1 -Failed -Slots 2
    .\scripts\game\run-autotest-parallel.ps1 -Scenario TG-04,TG-10,TB-03
#>
param(
    [string[]]$Scenario,
    [switch]$Failed,
    # 4 games by default (~3 GB each in a battle; 6 games ran out of memory on 32 GB with other apps open)
    [ValidateRange(1, 8)][int]$Slots = 4,
    # 0: one scenario at a time (a free game takes the next one); > 0: groups of ~GroupSeconds of work
    [int]$GroupSeconds = 0,
    [int]$Timeout = 1800,
    [int]$StallSeconds = 300,
    [string]$Window = '1600x900',
    [switch]$Deploy,
    # the runs are not merged at the end (merge-parallel-run.ps1 -Batch <first>,<this one> merges several parallel runs)
    [switch]$NoMerge
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

# ------------------------------------------------------------------ scenarios
$definitions = @((Get-Content -LiteralPath (Join-Path $paths.Mod.Tests 'scenarios.json') -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios)
$auto = @($definitions | Where-Object { $_.kind -eq 'auto' -and $_.id -notlike 'TP-*' })
$ids = @($Scenario | Where-Object { $_ } | ForEach-Object { $_ -split ',' } | ForEach-Object { $_.Trim().ToUpper() } | Where-Object { $_ })
if ($Failed) {
    $st = Get-Content -LiteralPath (Join-Path $paths.Mod.Tests 'scenario-status.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    $ids = @($st.scenarios | Where-Object { $_.kind -eq 'auto' -and $_.result -in 'NG', 'NOT RUN' } | ForEach-Object { $_.id })
    if ($ids.Count -eq 0) { Write-Host 'No NG / NOT RUN scenario: nothing to run.'; exit 0 }
}
$chosen = if ($ids.Count -gt 0) { @($auto | Where-Object { $ids -contains $_.id }) } else { $auto }
if ($chosen.Count -eq 0) { Write-Host 'No auto scenario selected.'; exit 0 }

# last known duration of every scenario (the latest runs), 60 s when unknown
$durations = @{}
foreach ($f in @(Get-ChildItem (Join-Path $repo 'logs\autotest') -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending | Select-Object -First 40)) {
    $rf = Join-Path $f.FullName 'results.json'
    if (-not (Test-Path -LiteralPath $rf)) { continue }
    try { $res = Get-Content -LiteralPath $rf -Raw -Encoding UTF8 | ConvertFrom-Json } catch { continue }
    foreach ($s in @($res.scenarios)) {
        $id = ($s.id -split '#')[0]
        if ($durations.ContainsKey($id) -or -not $s.startedAt -or -not $s.endedAt) { continue }
        $durations[$id] = ([datetime]$s.endedAt - [datetime]$s.startedAt).TotalSeconds
    }
}
function Get-Duration([string]$Id) { if ($durations.ContainsKey($Id)) { return [Math]::Max(5, $durations[$Id]) } else { return 60 } }

# groups: the longest first, ~GroupSeconds each
# the title-screen scenarios (TT-xx) run with the exclusive ones: the games of the queue stay open inside their save
function Test-Exclusive($s) { return [bool]$s.exclusive -or $s.id -like 'TT-*' }
$normal = @($chosen | Where-Object { -not (Test-Exclusive $_) } | Sort-Object { Get-Duration $_.id } -Descending)
$exclusive = @($chosen | Where-Object { Test-Exclusive $_ } | ForEach-Object { $_.id })
# "freshGameAfter": what the scenario changes cannot be undone by the reset of the player (NPCs killed, years passed):
# the game of the slot is closed after it, the next scenario starts from the snapshot
$freshAfter = @($chosen | Where-Object { $_.freshGameAfter } | ForEach-Object { $_.id })
$groups = New-Object System.Collections.Generic.List[object]
$cur = @(); $sum = 0
foreach ($s in $normal) {
    $cur += $s.id; $sum += Get-Duration $s.id
    if ($sum -ge $GroupSeconds) { $groups.Add([pscustomobject]@{ ids = $cur; seconds = [int]$sum }); $cur = @(); $sum = 0 }
}
if ($cur.Count -gt 0) { $groups.Add([pscustomobject]@{ ids = $cur; seconds = [int]$sum }) }

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$work = Join-Path $repo "logs\autotest-parallel\$stamp"
New-Item -ItemType Directory -Path $work -Force | Out-Null
$queueFile = Join-Path $work 'queue.json'
$logFile = Join-Path $work 'parallel.log'
$runsFile = Join-Path $work 'runs.txt'
Write-Queue $queueFile $groups.ToArray()
New-Item -ItemType File -Path $runsFile -Force | Out-Null

Enter-ParallelLock $repo
$backups = @{}
$exitCode = 0
try {
    Write-ParallelLog $logFile "$($chosen.Count) scenario(s): $($groups.Count) group(s) on $Slots game(s) (~$([int](($groups | Measure-Object seconds -Sum).Sum / 60)) min of scenarios), exclusive after: $(if ($exclusive) { $exclusive -join ', ' } else { 'none' })"

    # ------------------------------------------------------------------ deploy, slots, window, backup
    if ($Deploy) {
        # one deploy at a time, the copy only when no game runs (or skipped: same build) - _slots.ps1
        Invoke-SafeDeploy $repo $paths.Workshop $paths.Mod.Projects $paths.Mod.Dependencies | Out-Null
        Write-ParallelLog $logFile 'built and deployed'
    }
    # the slots of this run, locked until it ends (waits for the runs of other terminals / sessions using them); the
    # run-autotest.ps1 children use them through GGBH_LOCK_PARENT
    foreach ($s in $slotIds) { [void](Select-GameSlot -Requested $s -WaitMinutes 120 -What 'run-autotest-parallel') }
    $env:GGBH_LOCK_PARENT = "$PID"
    Write-ParallelLog $logFile "slots locked: $($slotIds -join ', ')"
    foreach ($s in $slotIds) { & (Join-Path $PSScriptRoot 'new-game-slot.ps1') -Slot $s | Out-Null }
    $gameArgs = Set-GameWindow $repo $Window
    $backups = Backup-Saves $paths (Join-Path $work 'save-backup')
    Remove-Item -LiteralPath (Join-Path $paths.ModLogs "$($paths.Mod.Id)\autotest-shared\creating.lock") -Force -ErrorAction SilentlyContinue
    Write-ParallelLog $logFile "saves backed up; games $(if ($gameArgs) { "windowed $Window" } else { 'as set in the game' })"

    # ------------------------------------------------------------------ workers
    $worker = {
        param($Slot, $Repo, $QueueFile, $LogFile, $RunsFile, $StallSeconds, $Timeout, $GameArgs, $Delay, $Window, $FreshAfter)
        $ErrorActionPreference = 'Continue'
        . (Join-Path $Repo 'scripts\game\_game.ps1')
        . (Join-Path $Repo 'scripts\game\_parallel.ps1')
        $env:GGBH_SLOT = if ($Slot -ge 2) { "$Slot" } else { $null }
        $paths = Get-GamePaths
        function Log([string]$t) { Write-ParallelLog $LogFile "[slot $Slot] $t" }
        # -Attach: the scenarios given to the game of the slot that waits in its save (no restart); else the game is
        # started (-KeepAlive: it stays open after the run, waiting for the next scenarios)
        function Invoke-Run([string[]]$Ids, [switch]$Attach) {
            $mode = if ($Attach) { '-Attach' } else { '-KeepAlive' }
            $out = & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $Repo 'scripts\game\run-autotest.ps1') `
                -Scenario ($Ids -join ',') -Slot $Slot -SlotCharacter -NoSaveBackup -NoViewerUpdate -Timeout $Timeout `
                -StallSeconds $StallSeconds -GameArgs $GameArgs -Window $Window $mode 2>&1 | ForEach-Object { "$_" }
            $line = $out | Where-Object { $_ -match 'Run:\s+(\S+)' } | Select-Object -First 1
            $runId = if ($line -match 'Run:\s+(\S+)') { $Matches[1] } else { $null }
            if ($runId) { for ($k = 0; $k -lt 20; $k++) { try { [IO.File]::AppendAllText($RunsFile, "$runId`r`n"); break } catch { Start-Sleep -Milliseconds 100 } } }
            $counts = ($out | Where-Object { $_ -match 'OK \d+\s+NG \d+' } | Select-Object -Last 1) -replace '^\S+\s+', ''
            # the scenarios with a result (a game closed or stopped in the middle leaves the others without one)
            $done = @()
            $rf = if ($runId) { Join-Path $Repo "logs\autotest\$runId\results.json" } else { $null }
            if ($rf -and (Test-Path -LiteralPath $rf)) {
                try { $done = @((Get-Content -LiteralPath $rf -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios | Where-Object { $_.status -and $_.status -ne 'RUNNING' } | ForEach-Object { ($_.id -split '#')[0] }) } catch { }
            }
            return [pscustomobject]@{
                RunId = $runId; Counts = $counts; Done = $done
                Finished = @($out | Where-Object { $_ -match 'Runner status: (DONE|STOPPED)' }).Count -gt 0
                Stalled = @($out | Where-Object { $_ -match 'stalled' }).Count -gt 0
            }
        }
        Start-Sleep -Seconds $Delay
        # the slot's character: found or created (TP-01), then its snapshot; the game of the slot started again (twice
        # at most) when it was closed in the middle
        $snap = Get-SlotSnapshotInfo $paths $Slot
        $ch = $null
        for ($try = 1; $try -le 3 -and -not $ch; $try++) {
            if ($snap) { [void](Restore-SlotSnapshot $paths $Slot) }
            $r = Invoke-Run @('TP-01')
            $ch = Get-SlotCharacterOfRun $Repo $r.RunId
            if (-not $ch) { Log "no test character (run $($r.RunId)$(if (-not $r.Finished) { ': the game stopped in the middle' })) - try $try of 3" }
        }
        if (-not $ch) { Log "!!! no test character: this slot stops"; return }
        if ($ch.Created -or -not $snap -or $snap.folder -ne $ch.Folder) {
            Save-SlotSnapshot $paths $Slot $ch.Folder $ch.PlayerId
            Log "test character $($ch.PlayerId) [$($ch.Folder)]$(if ($ch.Created) { ' created' }): snapshot taken"
        }
        else { Log "test character $($ch.PlayerId) [$($ch.Folder)]" }
        while ($true) {
            $item = Use-Queue $QueueFile { param($l) if ($l.Count -eq 0) { return $null }; $x = $l[0]; $l.RemoveAt(0); return $x }
            if (-not $item) { break }
            # the game stays open between the scenarios (each one resets the player to the loaded save and sets its
            # precondition); a game that closed starts again from the slot's snapshot
            $alive = [bool](Get-GameProcess)
            if (-not $alive) { [void](Restore-SlotSnapshot $paths $Slot) }
            Log "start $($item.ids -join ',') (~$($item.seconds) s)$(if (-not $alive) { ' - game started' })"
            $r = Invoke-Run @($item.ids) -Attach:$alive
            Log "done $($item.ids -join ','): run $($r.RunId) $($r.Counts)$(if ($r.Stalled) { ' - STALLED' })"
            if (@($item.ids | Where-Object { @($FreshAfter) -contains $_ }).Count -gt 0 -and (Get-GameProcess)) {
                Stop-Game
                Log "fresh game after $($item.ids -join ','): closed, the next scenario starts from the snapshot"
            }
            # the game closed / stopped in the middle: the scenarios without a result back to the queue (twice at most;
            # a stalled one is not run again - it would stop the game again)
            $left = @($item.ids | Where-Object { $r.Done -notcontains $_ })
            if (-not $r.Finished -and $left.Count -gt 0) {
                $tries = if ($item.PSObject.Properties['tries']) { [int]$item.tries + 1 } else { 1 }
                if ($r.Stalled) { $left = @($left | Select-Object -Skip 1) }
                if ($tries -gt 2) { Log "!!! not run (the game stopped $tries times): $($left -join ',')" }
                elseif ($left.Count -gt 0) {
                    $rest = [pscustomobject]@{ ids = $left; seconds = $item.seconds; tries = $tries }
                    Use-Queue $QueueFile { param($l) $l.Insert(0, $rest); return $null } | Out-Null
                    Log "the game stopped in the middle: $($left -join ',') queued again (try $tries)"
                }
            }
        }
        # the game of the slot closed at the end, then its character back to the snapshot
        Stop-Game
        [void](Restore-SlotSnapshot $paths $Slot)
        Log 'queue empty: done'
    }
    $jobs = foreach ($s in $slotIds) {
        Start-Job -Name "autotest-slot$s" -ScriptBlock $worker -ArgumentList $s, $repo, $queueFile, $logFile, $runsFile, $StallSeconds, $Timeout, $gameArgs, (($s - 2) * 45), $Window, $freshAfter
    }
    while (@($jobs | Where-Object { $_.State -eq 'Running' }).Count -gt 0) {
        $jobs | Receive-Job -ErrorAction Continue 2>&1 | Out-Null
        Start-Sleep -Seconds 5
    }
    $jobs | Remove-Job -Force -ErrorAction SilentlyContinue

    # ------------------------------------------------------------------ exclusive scenarios: alone, first slot
    if ($exclusive.Count -gt 0) {
        $first = $slotIds[0]
        $env:GGBH_SLOT = "$first"
        $slotPaths = Get-GamePaths
        Stop-Game
        [void](Restore-SlotSnapshot $slotPaths $first)
        Write-ParallelLog $logFile "[slot $first] exclusive: $($exclusive -join ',')"
        $out = & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'run-autotest.ps1') -Scenario ($exclusive -join ',') `
            -Slot $first -SlotCharacter -NoSaveBackup -NoViewerUpdate -Timeout $Timeout -StallSeconds $StallSeconds -GameArgs $gameArgs -Window $Window 2>&1 | ForEach-Object { "$_" }
        $line = $out | Where-Object { $_ -match 'Run:\s+(\S+)' } | Select-Object -First 1
        if ($line -match 'Run:\s+(\S+)') { Add-Content -LiteralPath $runsFile -Value $Matches[1] }
        [void](Restore-SlotSnapshot $slotPaths $first)
        $env:GGBH_SLOT = $null
        Write-ParallelLog $logFile "[slot $first] exclusive done: $(($out | Where-Object { $_ -match 'OK \d+\s+NG \d+' } | Select-Object -Last 1) -replace '^\S+\s+', '')"
    }
}
finally {
    Stop-ParallelChildren $slotIds
    if ($backups.Count -gt 0) { Restore-ModGlobals $backups }
    Exit-ParallelLock $repo
    Write-ParallelLog $logFile 'games closed; global settings of the mods restored (saves and window kept)'
}

# ------------------------------------------------------------------ merged summary
$rows = @()
$runIds = @(Get-Content -LiteralPath $runsFile -ErrorAction SilentlyContinue | ForEach-Object { "$_".Trim() } | Where-Object { $_ })
foreach ($runId in $runIds) {
    $rf = Join-Path $repo "logs\autotest\$runId\report.json"
    if (-not (Test-Path -LiteralPath $rf)) { $rf = Join-Path $repo "logs\autotest\$runId\results.json" }
    if (-not (Test-Path -LiteralPath $rf)) { continue }
    $res = Get-Content -LiteralPath $rf -Raw -Encoding UTF8 | ConvertFrom-Json
    foreach ($s in @($res.scenarios)) {
        # TP-01 of a slot that had its character (SKIP) or of an interrupted preparation (tried again) is no result
        if ($s.id -like 'TP-*' -and $s.status -in 'SKIP', 'RUNNING') { continue }
        $rows += [pscustomobject]@{ id = $s.id; status = $s.status; run = $runId; title = $s.title }
    }
}
$ng = @($rows | Where-Object { $_.status -in 'FAIL', 'ERROR', 'RUNNING' })
$missing = @($chosen | Where-Object { $rows.id -notcontains $_.id } | ForEach-Object { $_.id })
$summary = [ordered]@{
    time = $stamp; slots = $Slots; scenarios = $chosen.Count
    ok = @($rows | Where-Object status -eq 'PASS').Count; ng = $ng.Count; skip = @($rows | Where-Object status -eq 'SKIP').Count
    notRun = $missing; ngIds = @($ng | ForEach-Object { $_.id }); runs = $runIds
    results = $rows
}
[IO.File]::WriteAllText((Join-Path $work 'summary.json'), (ConvertTo-Json $summary -Depth 5), (New-Object Text.UTF8Encoding($false)))
& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot '..\..\test-viewer\update-data.ps1') -RepoRoot $repo | Out-Null
Write-ParallelLog $logFile ("OK {0}  NG {1}  SKIP {2}  NOT RUN {3}" -f $summary.ok, $summary.ng, $summary.skip, $missing.Count)
if ($ng.Count -gt 0) { Write-ParallelLog $logFile "NG: $(($ng | ForEach-Object { "$($_.id) ($($_.run))" }) -join ', ')"; $exitCode = 3 }
if ($missing.Count -gt 0) { Write-ParallelLog $logFile "NOT RUN: $($missing -join ', ')"; if ($exitCode -eq 0) { $exitCode = 2 } }
Write-ParallelLog $logFile "summary: $(Join-Path $work 'summary.json')"
# the runs of every scenario as ONE run of the test viewer (logs\autotest\<this time>), pinned
if (-not $NoMerge) { try { & (Join-Path $PSScriptRoot 'merge-parallel-run.ps1') -Batch $work | Out-Host; Write-ParallelLog $logFile "merged into one run: logs\autotest\$stamp (pinned)" }
catch { Write-ParallelLog $logFile "!!! merge of the runs failed: $($_.Exception.Message)" } }
exit $exitCode
