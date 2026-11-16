<#
.SYNOPSIS
    Collect every autotest and unit test run into test-viewer\data\runs.js (data of the test viewer).
.DESCRIPTION
    Sources:
      logs\autotest\<run>\report.json (or results.json)  autotest runs (scenarios, checks, screenshots)
      logs\autotest\<run>\summary.json, game-run\summary.json
      logs\unittests\<run>\unittests.trx                  unit test runs
      3161035078\tests\scenarios.json, scenario-status.json  definitions and latest OK/NG per id
    Output: test-viewer\data\runs.js (window.TEST_DATA = {...}), git-ignored. Open test-viewer\index.html
    (works from file://). Called automatically by run-autotest.ps1 and run-unittests.ps1.
    Only the newest -Keep runs (default 10) of logs\autotest, logs\unittests and logs\game-runs are kept:
    older run folders (screenshots, save backups, logs) are deleted to save disk space.
    Keep ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI).
.PARAMETER Keep
    Number of runs kept per folder (0 = keep everything).
.EXAMPLE
    .\scripts\update-test-viewer.ps1 -Open
    .\scripts\update-test-viewer.ps1 -Keep 5
#>
[CmdletBinding()]
param([switch]$Open, [int]$Keep = 10)

$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$viewer = Join-Path $repo 'test-viewer'
$dataDir = Join-Path $viewer 'data'
New-Item -ItemType Directory -Path $dataDir -Force | Out-Null

function Read-Json([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return $null }
    try { return Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json } catch { Write-Warning "Cannot read $Path : $($_.Exception.Message)"; return $null }
}

function Get-RunTime([string]$Id) {
    try { return [datetime]::ParseExact($Id, 'yyyyMMdd-HHmmss', $null).ToString('s') } catch { return $null }
}

# ------------------------------------------------------------------ retention: newest $Keep runs per folder
if ($Keep -gt 0) {
    foreach ($sub in 'logs\autotest', 'logs\unittests', 'logs\game-runs') {
        $root = Join-Path $repo $sub
        $old = @(Get-ChildItem -LiteralPath $root -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match '^\d{8}-\d{6}' } | Sort-Object Name -Descending | Select-Object -Skip $Keep)
        foreach ($d in $old) {
            try { Remove-Item -LiteralPath $d.FullName -Recurse -Force -ErrorAction Stop; Write-Host "Removed old run $sub\$($d.Name)" -ForegroundColor DarkGray }
            catch { Write-Warning "Cannot remove $($d.FullName): $($_.Exception.Message)" }
        }
    }
}

# ------------------------------------------------------------------ autotest
$autotest = @()
$atRoot = Join-Path $repo 'logs\autotest'
foreach ($dir in @(Get-ChildItem -LiteralPath $atRoot -Directory -ErrorAction SilentlyContinue | Where-Object { $_.Name -match '^\d{8}-\d{6}$' } | Sort-Object Name)) {
    $res = Read-Json (Join-Path $dir.FullName 'report.json')
    if (-not $res) { $res = Read-Json (Join-Path $dir.FullName 'results.json') }
    if (-not $res) { continue }
    $sum = Read-Json (Join-Path $dir.FullName 'summary.json')
    $game = Read-Json (Join-Path $dir.FullName 'game-run\summary.json')
    $base = "../logs/autotest/$($dir.Name)/"
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
    # logs of the run: ModLib/Taoist logs (incl. per save game), MelonLoader, Unity player (last 6000 lines each)
    $logs = @(foreach ($f in @(Get-ChildItem -LiteralPath (Join-Path $dir.FullName 'game-run') -Filter *.log -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -ne 'timeline.log' } | Sort-Object { if ($_.Name -like 'mod-*') { 0 } else { 1 } }, Name)) {
        $lines = @(Get-Content -LiteralPath $f.FullName -Encoding UTF8 -Tail 6000)
        [ordered]@{ name = $f.BaseName; lines = $lines }
    })
    $autotest += [ordered]@{
        logs = $logs
        id = $dir.Name; time = Get-RunTime $dir.Name; status = $res.status; exitCode = if ($sum) { $sum.exitCode } else { $null }
        taoistVersion = $res.taoistVersion; modLibVersion = $res.modLibVersion; player = $res.player
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
$utDesc = @{}
foreach ($cs in @(Get-ChildItem -LiteralPath (Join-Path $repo 'tests\UnitTests') -Recurse -Filter *.cs -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notmatch '\\(obj|bin)\\' })) {
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
    $id = if ($trx.Directory.Name -match '^\d{8}-\d{6}$') { $trx.Directory.Name } else { $trx.LastWriteTime.ToString('yyyyMMdd-HHmmss') }
    $unittest += [ordered]@{
        id = $id; time = $trx.LastWriteTime.ToString('s'); file = $trx.FullName.Substring($repo.Length + 1)
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
# %USERPROFILE%\AppData\LocalLow\guigugame\guigubahuang\mod\<mod id>\[<save>_]debug-<date>.log (ModLib/Taoist logs),
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
$defsJson = Read-Json (Join-Path $repo '3161035078\tests\scenarios.json')
$statusJson = Read-Json (Join-Path $repo '3161035078\tests\scenario-status.json')
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
