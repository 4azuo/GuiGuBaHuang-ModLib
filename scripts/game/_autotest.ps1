<#
.SYNOPSIS
    Scenario status (OK/NG per id) shared by run-autotest.ps1 and set-scenario-status.ps1
    (dot-source: . (Join-Path $PSScriptRoot '_autotest.ps1')).
.DESCRIPTION
    3161035078\tests\scenario-status.json (committed, the only status file) keeps the latest
    result of every scenario id:
      result  OK | NG | SKIP | NOT RUN | - (never tested)
      checks  { "<scenario id>-<nn>": "OK" | "NG" }   (automatic scenarios)
      lastRun, evidence (report of the run), tester, note
    View it in test-viewer\index.html (tab "Scenario status"), data from scripts\update-test-viewer.ps1.
    Keep ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI).
#>

function Get-ScenarioPaths([string]$Repo) {
    $tests = Join-Path $Repo '3161035078\tests'
    return [pscustomobject]@{
        Definitions = Join-Path $tests 'scenarios.json'
        StatusJson  = Join-Path $tests 'scenario-status.json'
    }
}

# JSON with 2-space indentation (ConvertTo-Json of Windows PowerShell 5.1 has irregular indentation).
function ConvertTo-Json2 {
    param([Parameter(Mandatory, ValueFromPipeline)]$InputObject, [int]$Depth = 10)
    process {
        $compact = ConvertTo-Json -InputObject $InputObject -Depth $Depth -Compress
        # PS 5.1 escapes these characters needlessly
        $compact = $compact.Replace('\u0026', '&').Replace('\u0027', "'").Replace('\u003c', '<').Replace('\u003e', '>')
        $sb = New-Object Text.StringBuilder
        $level = 0
        $inString = $false
        $escape = $false
        for ($i = 0; $i -lt $compact.Length; $i++) {
            $ch = $compact[$i]
            if ($inString) {
                [void]$sb.Append($ch)
                if ($escape) { $escape = $false }
                elseif ($ch -eq '\') { $escape = $true }
                elseif ($ch -eq '"') { $inString = $false }
                continue
            }
            switch ($ch) {
                '"' { $inString = $true; [void]$sb.Append($ch) }
                { $_ -eq '{' -or $_ -eq '[' } {
                    $close = if ($ch -eq '{') { '}' } else { ']' }
                    if ($i + 1 -lt $compact.Length -and $compact[$i + 1] -eq $close) {
                        [void]$sb.Append($ch).Append($close); $i++
                    }
                    else {
                        $level++
                        [void]$sb.Append($ch).Append("`n").Append(' ' * (2 * $level))
                    }
                }
                { $_ -eq '}' -or $_ -eq ']' } {
                    $level--
                    [void]$sb.Append("`n").Append(' ' * (2 * $level)).Append($ch)
                }
                ',' { [void]$sb.Append(",`n").Append(' ' * (2 * $level)) }
                ':' { [void]$sb.Append(': ') }
                default { [void]$sb.Append($ch) }
            }
        }
        return $sb.ToString()
    }
}

function Write-Json2File([string]$Path, $Object, [int]$Depth = 10) {
    $utf8 = New-Object Text.UTF8Encoding($false)
    [IO.File]::WriteAllText($Path, ((ConvertTo-Json2 -InputObject $Object -Depth $Depth) + "`n"), $utf8)
}

function Read-ScenarioDefinitions([string]$Repo) {
    $p = Get-ScenarioPaths $Repo
    return @((Get-Content -LiteralPath $p.Definitions -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios)
}

# Returns an ordered hashtable id -> status entry (hashtable), with one entry per defined scenario.
function Read-ScenarioStatus([string]$Repo) {
    $p = Get-ScenarioPaths $Repo
    $old = @{}
    if (Test-Path -LiteralPath $p.StatusJson) {
        foreach ($e in @((Get-Content -LiteralPath $p.StatusJson -Raw -Encoding UTF8 | ConvertFrom-Json).scenarios)) { $old[$e.id] = $e }
    }
    $status = [ordered]@{}
    foreach ($d in (Read-ScenarioDefinitions $Repo)) {
        $o = $old[$d.id]
        $checks = [ordered]@{}
        if ($o -and $o.checks) { foreach ($pr in $o.checks.PSObject.Properties) { $checks[$pr.Name] = $pr.Value } }
        $status[$d.id] = [ordered]@{
            id       = $d.id
            kind     = $d.kind
            feature  = $d.feature
            title    = $d.title
            result   = if ($o) { $o.result } else { '-' }
            lastRun  = if ($o) { $o.lastRun } else { $null }
            evidence = if ($o) { $o.evidence } else { $null }
            tester   = if ($o) { $o.tester } else { $null }
            note     = if ($o) { $o.note } else { $null }
            checks   = $checks
        }
    }
    return $status
}

function Get-StatusCounts($List) {
    $count = [ordered]@{}
    foreach ($r in 'OK', 'NG', 'SKIP', 'NOT RUN', '-') { $count[$r] = @($List | Where-Object { $_.result -eq $r }).Count }
    return $count
}

# Saves the JSON (2-space indent).
function Save-ScenarioStatus([string]$Repo, $Status) {
    $p = Get-ScenarioPaths $Repo
    $list = @($Status.Values)
    Write-Json2File $p.StatusJson ([ordered]@{ updatedAt = (Get-Date).ToString('s'); counts = (Get-StatusCounts $list); scenarios = $list })
}
