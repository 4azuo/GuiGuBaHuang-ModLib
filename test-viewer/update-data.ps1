<#
.SYNOPSIS
    Collect every autotest and unit test run of a mod repository into <repo>\logs\test-viewer\data\runs.js (test viewer data).
.DESCRIPTION
    Shared by the mods using ModLib: the viewer (index.html template, app.js, style.css) lives in ModLib, the data in
    the mod repository (<repo> = -RepoRoot, default: the folder holding this ModLib submodule). The mod is named in
    <repo>\scripts\settings.json: "modProject" (project id; default "releaseAllVersionProject") and "modName".
    Sources (in <repo>):
      logs\autotest\<run>\report.json (or results.json)  autotest runs (scenarios, checks, screenshots)
      logs\autotest\<run>\summary.json, game-run\summary.json
      logs\unittests\<run>\unittests.trx                  unit test runs
      <modProject>\tests\scenarios.json, scenario-status.json  definitions and latest OK/NG per id
    Output: <repo>\logs\test-viewer\index.html (from the template: loads app.js/style.css of ModLib) and
    <repo>\logs\test-viewer\data\runs.js (window.TEST_DATA = {...}), git-ignored with logs\. Open logs\test-viewer\index.html
    (works from file://). Called automatically by run-autotest.ps1 and run-unittests.ps1.
    Only the newest -Keep runs (default 10) of logs\autotest, logs\unittests and logs\game-runs are kept:
    older run folders (screenshots, save backups, logs) are deleted to save disk space. Pinned runs (pinned.json in
    the folder: scripts\pin-test-run.ps1) are never deleted and do not count in -Keep. An autotest run that holds
    the latest result of a scenario is kept too (the games side by side make one run per scenario).
    Keep ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI).
.PARAMETER Keep
    Number of runs kept per folder (0 = keep everything).
.EXAMPLE
    .\GuiGuBaHuang-ModLib\test-viewer\update-data.ps1 -Open
    .\GuiGuBaHuang-ModLib\test-viewer\update-data.ps1 -Keep 5 -RepoRoot C:\GitHub\MyMod
#>
[CmdletBinding()]
param([switch]$Open, [int]$Keep = 10, [string]$RepoRoot)

$ErrorActionPreference = 'Stop'
$modLib = Split-Path $PSScriptRoot -Parent
$repo = if ($RepoRoot) { (Resolve-Path -LiteralPath $RepoRoot).Path } else { Split-Path $modLib -Parent }
$viewer = Join-Path $repo 'logs\test-viewer'   # next to the runs (logs\autotest...)
$dataDir = Join-Path $viewer 'data'
New-Item -ItemType Directory -Path $dataDir -Force | Out-Null

function Read-Json([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return $null }
    try { return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json } catch { Write-Warning "Cannot read $Path : $($_.Exception.Message)"; return $null }
}

# ------------------------------------------------------------------ the mod (scripts\settings.json) and the page
$settings = Read-Json (Join-Path $repo 'scripts\settings.json')
$modProject = if ($settings -and $settings.modProject) { [string]$settings.modProject } elseif ($settings -and $settings.releaseAllVersionProject) { [string]$settings.releaseAllVersionProject } else { '' }
$modName = if ($settings -and $settings.modName) { [string]$settings.modName } else { 'Mod' }
if (-not $modProject) { Write-Warning "No modProject in $repo\scripts\settings.json: no scenario definitions" }
# path of ModLib from <repo>\logs\test-viewer, with / (works from file:// and from a local server rooted at <repo>)
$modLibRel = ([uri]($viewer.TrimEnd('\') + '\')).MakeRelativeUri([uri]($modLib.TrimEnd('\') + '\')).ToString().TrimEnd('/')
$page = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'index.html'), [Text.Encoding]::UTF8)
$page = $page.Replace('{{MODLIB}}', $modLibRel).Replace('{{MOD_NAME}}', [Net.WebUtility]::HtmlEncode($modName))
[IO.File]::WriteAllText((Join-Path $viewer 'index.html'), $page, (New-Object Text.UTF8Encoding($false)))

function Get-RunTime([string]$Id) {
    try { return [datetime]::ParseExact($Id, 'yyyyMMdd-HHmmss', $null).ToString('s') } catch { return $null }
}

# ------------------------------------------------------------------ retention: newest $Keep runs per folder
# + (autotest) every run holding the latest result of a scenario: the games side by side make a run of every scenario
# (~130 runs a full run), a count alone deleted the full run and "latest result of every scenario" lost a third of them
function Get-LatestRunOfEveryScenario([string]$Root) {
    $keep = @{}
    $seen = @{}
    foreach ($dir in @(Get-ChildItem -LiteralPath $Root -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^\d{8}-\d{6}(-s\d+)?$' } | Sort-Object Name -Descending)) {
        $res = Read-Json (Join-Path $dir.FullName 'results.json')
        if (-not $res) { continue }
        foreach ($sc in @($res.scenarios)) {
            if (-not $sc -or -not $sc.id -or $sc.status -eq 'RUNNING') { continue }
            if ($sc.id -like 'TP-*' -and $sc.status -eq 'SKIP') { continue }   # the pre-test of a slot that had its character
            $id = ($sc.id -split '#')[0]
            if ($seen.ContainsKey($id)) { continue }
            $seen[$id] = $true
            $keep[$dir.Name] = $true
        }
    }
    return $keep
}
if ($Keep -gt 0) {
    foreach ($sub in 'logs\autotest', 'logs\unittests', 'logs\game-runs') {
        $root = Join-Path $repo $sub
        $latest = if ($sub -eq 'logs\autotest') { Get-LatestRunOfEveryScenario $root } else { @{} }
        $old = @(Get-ChildItem -LiteralPath $root -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match '^\d{8}-\d{6}(-s\d+)?' -and -not (Test-Path -LiteralPath (Join-Path $_.FullName 'pinned.json')) } |
            Sort-Object Name -Descending | Select-Object -Skip $Keep | Where-Object { -not $latest.ContainsKey($_.Name) })
        foreach ($d in $old) {
            try { Remove-Item -LiteralPath $d.FullName -Recurse -Force -ErrorAction Stop; Write-Host "Removed old run $sub\$($d.Name)" -ForegroundColor DarkGray }
            catch { Write-Warning "Cannot remove $($d.FullName): $($_.Exception.Message)" }
        }
    }
}

# ------------------------------------------------------------------ autotest
$autotest = @()
$atRoot = Join-Path $repo 'logs\autotest'
foreach ($dir in @(Get-ChildItem -LiteralPath $atRoot -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^\d{8}-\d{6}(-s\d+)?$' } | Sort-Object Name)) {
    $res = Read-Json (Join-Path $dir.FullName 'report.json')
    if (-not $res) { $res = Read-Json (Join-Path $dir.FullName 'results.json') }
    if (-not $res) { continue }
    $sum = Read-Json (Join-Path $dir.FullName 'summary.json')
    $game = Read-Json (Join-Path $dir.FullName 'game-run\summary.json')
    $base = "../autotest/$($dir.Name)/"
    $scenarios = @(foreach ($s in @($res.scenarios)) {
        $shots = @(foreach ($sh in @($s.Screenshots)) {
            if ($null -eq $sh) { continue }
            if ($sh -is [string]) { [ordered]@{ file = $base + $sh; step = ''; stepNo = 0; kind = 'extra' } }
            else { [ordered]@{ file = $base + $sh.File; step = $sh.Step; stepNo = $sh.StepNo; kind = $sh.Kind } }
        })
        [ordered]@{
            id = $s.Id; feature = $s.Feature; title = $s.Title; kind = $s.Kind; status = $s.Status
            startedAt = $s.StartedAt; endedAt = $s.EndedAt; error = $s.Error
            checks = @(foreach ($c in @($s.Checks)) { [ordered]@{ id = $c.Id; step = $c.Step; status = $c.Status; message = $c.Message; time = $c.Time } })
            screenshots = $shots
        }
    })
    # logs of the run: ModLib/mod logs (incl. per save game), MelonLoader, Unity player (last 6000 lines each)
    $logs = @(foreach ($f in @(Get-ChildItem -LiteralPath (Join-Path $dir.FullName 'game-run') -Filter *.log -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -ne 'timeline.log' } | Sort-Object { if ($_.Name -like 'mod-*') { 0 } else { 1 } }, Name)) {
        $lines = @(Get-Content -LiteralPath $f.FullName -Encoding UTF8 -Tail 6000)
        [ordered]@{ name = $f.BaseName; lines = $lines }
    })
    $autotest += [ordered]@{
        logs = $logs
        id = $dir.Name; time = Get-RunTime $dir.Name; status = $res.status; exitCode = if ($sum) { $sum.exitCode } else { $null }
        pin = Read-Json (Join-Path $dir.FullName 'pinned.json')
        modVersion = $(if ($res.modVersion) { $res.modVersion } else { $res.taoistVersion }); modLibVersion = $res.modLibVersion; player = $res.player
        startedAt = $res.startedAt; updatedAt = $res.updatedAt
        logErrors = if ($game) { @($game.errors | Select-Object -First 30) } else { @() }
        log = @($res.log)
        scenarios = $scenarios
    }
}

# ------------------------------------------------------------------ unit tests
$unittest = @()
$utRoot = Join-Path $repo 'logs\unittests'
# [Description("...")] of the test methods: the trx logger does not write it, read it from the sources
# (tests of the mod repository + shared tests of ModLib)
$utDesc = @{}
foreach ($cs in @(Get-ChildItem -LiteralPath (Join-Path $repo 'tests\UnitTests'), (Join-Path $modLib 'tests') -Recurse -Filter *.cs -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '\\(obj|bin)\\' })) {
    $src = [IO.File]::ReadAllText($cs.FullName)
    foreach ($m in [regex]::Matches($src, 'Description\("((?:[^"\\]|\\.)*)"\)\]\s*public\s+void\s+(\w+)')) {
        $utDesc[$m.Groups[2].Value] = $m.Groups[1].Value -replace '\\"', '"'
    }
}
foreach ($trx in @(Get-ChildItem -LiteralPath $utRoot -Recurse -Filter *.trx -ErrorAction SilentlyContinue | Sort-Object LastWriteTime)) {
    try { [xml]$x = Get-Content -LiteralPath $trx.FullName -Raw -Encoding UTF8 } catch { continue }
    $defs = @{}
    foreach ($d in @($x.TestRun.TestDefinitions.UnitTest)) {
        $cats = @($d.TestCategory.TestCategoryItem | ForEach-Object { $_.TestCategory })
        $desc = if ($d.Description) { [string]$d.Description } else { $null }
        $defs[$d.id] = @{ className = $d.TestMethod.className; categories = $cats; description = $desc }
    }
    $tests = @(foreach ($r in @($x.TestRun.Results.UnitTestResult)) {
        $def = $defs[$r.testId]
        $cls = if ($def) { ($def.className -split ',')[0] } else { '' }
        [ordered]@{
            name = $r.testName; className = $cls; outcome = $r.outcome; duration = $r.duration
            categories = if ($def) { $def.categories } else { @() }
            description = if ($def -and $def.description) { $def.description } elseif ($utDesc.ContainsKey([string]$r.testName)) { $utDesc[[string]$r.testName] } else { $null }
            output = if ($r.Output.StdOut) { [string]$r.Output.StdOut } else { $null }
            message = if ($r.Output.ErrorInfo) { [string]$r.Output.ErrorInfo.Message } else { $null }
            stack = if ($r.Output.ErrorInfo) { [string]$r.Output.ErrorInfo.StackTrace } else { $null }
        }
    })
    $id = if ($trx.Directory.Name -match '^\d{8}-\d{6}(-s\d+)?$') { $trx.Directory.Name } else { $trx.LastWriteTime.ToString('yyyyMMdd-HHmmss') }
    $unittest += [ordered]@{
        id = $id; time = $trx.LastWriteTime.ToString('s'); file = $trx.FullName.Substring($repo.Length + 1)
        pin = Read-Json (Join-Path $trx.DirectoryName 'pinned.json')
        counts = [ordered]@{
            total = $tests.Count
            passed = @($tests | Where-Object outcome -eq 'Passed').Count
            failed = @($tests | Where-Object outcome -eq 'Failed').Count
            skipped = @($tests | Where-Object { $_.outcome -notin 'Passed', 'Failed' }).Count
        }
        tests = $tests
    }
}

# ------------------------------------------------------------------ logs of the game mod folder
# %USERPROFILE%\AppData\LocalLow\guigugame\guigubahuang\mod\<mod id>\[<save>_]debug-<date>.log (ModLib/mod logs),
# newest 30 files, last 6000 lines each
$modLogs = @()
$modDir = Join-Path $env:USERPROFILE 'AppData\LocalLow\guigugame\guigubahuang\mod'
if (Test-Path -LiteralPath $modDir) {
    foreach ($f in @(Get-ChildItem -LiteralPath $modDir -Recurse -File -Filter *.log -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 30)) {
        $modLogs += [ordered]@{
            name = $f.FullName.Substring($modDir.Length + 1); time = $f.LastWriteTime.ToString('s'); size = $f.Length
            lines = @(Get-Content -LiteralPath $f.FullName -Tail 6000 -Encoding UTF8 -ErrorAction SilentlyContinue)
        }
    }
}

# ------------------------------------------------------------------ scenario definitions and status
$defsJson = if ($modProject) { Read-Json (Join-Path $repo "$modProject\tests\scenarios.json") } else { $null }
$statusJson = if ($modProject) { Read-Json (Join-Path $repo "$modProject\tests\scenario-status.json") } else { $null }
# ConvertTo-Json of Windows PowerShell 5.1 is very slow on large data (logs): the runs (plain dictionaries,
# arrays and strings) go through JavaScriptSerializer, the small PSObject parts through ConvertTo-Json.
Add-Type -AssemblyName System.Web.Extensions
$ser = New-Object Web.Script.Serialization.JavaScriptSerializer
$ser.MaxJsonLength = [int]::MaxValue
$ser.RecursionLimit = 100
function To-Plain($o) {
    # PSObjects (from ConvertFrom-Json) -> dictionaries/arrays understood by JavaScriptSerializer
    if ($null -eq $o) { return $null }
    if ($o -is [string]) { return [string]$o.ToString() }   # strings from Get-Content carry PS note properties
    if ($o -is [ValueType]) { return $o.PSObject.BaseObject }
    if ($o -is [Collections.IDictionary]) { $d = [ordered]@{}; foreach ($k in @($o.Keys)) { $d[[string]$k] = To-Plain $o[$k] }; return $d }
    if ($o -is [Management.Automation.PSCustomObject]) { $d = [ordered]@{}; foreach ($p in $o.PSObject.Properties) { $d[$p.Name] = To-Plain $p.Value }; return $d }
    if ($o -is [Collections.IEnumerable]) { return ,@(foreach ($x in $o) { To-Plain $x }) }
    return [string]$o
}
$generatedAt = (Get-Date).ToString('s')
$parts = @(
    ('"generatedAt":"' + $generatedAt + '"')
    ('"modName":' + $ser.Serialize($modName))
    ('"modProject":' + $ser.Serialize($modProject))
    '"autotest":' + $ser.Serialize((To-Plain $autotest))
    '"unittest":' + $ser.Serialize((To-Plain $unittest))
    '"modLogs":' + $ser.Serialize((To-Plain $modLogs))
    ('"modLogDir":' + $ser.Serialize([string]$modDir))
    '"definitions":' + $(if ($defsJson) { ConvertTo-Json -InputObject @($defsJson.scenarios) -Depth 6 -Compress } else { '[]' })
    '"status":' + $(if ($statusJson) { ConvertTo-Json -InputObject @($statusJson.scenarios) -Depth 6 -Compress } else { '[]' })
)
$json = '{' + ($parts -join ',') + '}'
$utf8 = New-Object Text.UTF8Encoding($false)
[IO.File]::WriteAllText((Join-Path $dataDir 'runs.js'), "window.TEST_DATA = $json;`n", $utf8)
Write-Host "Test viewer data: $($autotest.Count) autotest run(s), $($unittest.Count) unit test run(s), $($modLogs.Count) mod log file(s) -> $(Join-Path $dataDir 'runs.js')" -ForegroundColor Cyan
if ($Open) { Start-Process (Join-Path $viewer 'index.html') }
