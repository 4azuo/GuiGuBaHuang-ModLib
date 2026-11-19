<#
.SYNOPSIS
  Sets up the Nexus Mods upload of the release CI (.github/workflows/release.yml) on the GitHub repository:
  secret NEXUSMODS_API_KEY and variables NEXUS_<MOD>_FILE_ID / NEXUS_<MOD>_URL for ModLib and Taoist.

.DESCRIPTION
  Needs the GitHub CLI (gh) signed in with access to the repository ("gh auth login").
  Shows what is set, then asks each value: Enter keeps it, "-" removes it.
  Before: create the mod page on Nexus Mods and upload a first file by hand (the CI only adds new versions to an
  existing file). File id: Files tab of the mod page -> "Advanced", or Manage Files.
  The API key (https://www.nexusmods.com/settings/api-keys) is checked with Nexus (users/validate) unless
  -SkipValidate, then given to gh through stdin (never on the command line).

.EXAMPLE
  powershell -NoProfile -ExecutionPolicy Bypass -File scripts/setup/setup-nexus.ps1
#>
param(
    [string]$Repo,
    [switch]$SkipValidate
)

$ErrorActionPreference = "Continue"
$root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) {
    Write-Host "GitHub CLI (gh) not found: https://cli.github.com (winget install GitHub.cli), then 'gh auth login'." -ForegroundColor Red
    exit 1
}
gh auth status *> $null
if ($LASTEXITCODE -ne 0) { Write-Host "gh is not signed in: run 'gh auth login'." -ForegroundColor Red; exit 1 }

if (-not $Repo) {
    $url = (git -C $root remote get-url origin).Trim()
    if ($url -match 'github\.com[:/]([^/]+/[^/.]+)') { $Repo = $Matches[1] } else { Write-Host "Repository not found from origin ($url): pass -Repo owner/name." -ForegroundColor Red; exit 1 }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Nexus Mods setup of the release CI ($Repo)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$vars = @{}
$json = gh variable list --repo $Repo --json name,value 2>$null
if ($LASTEXITCODE -eq 0 -and $json) { ($json | ConvertFrom-Json) | ForEach-Object { $vars[$_.name] = $_.value } }
$secrets = @()
$json = gh secret list --repo $Repo --json name 2>$null
if ($LASTEXITCODE -eq 0 -and $json) { $secrets = @(($json | ConvertFrom-Json).name) }

function Set-RepoVariable([string]$Name, [string]$Label, [scriptblock]$Validate) {
    $cur = $vars[$Name]
    $shown = if ($cur) { $cur } else { "(not set)" }
    while ($true) {
        $v = (Read-Host "$Label [$shown]").Trim()
        if ($v -eq "") { Write-Host "  kept" -ForegroundColor DarkGray; return }
        if ($v -eq "-") {
            if ($cur) { gh variable delete $Name --repo $Repo | Out-Null; Write-Host "  $Name removed" -ForegroundColor Yellow }
            return
        }
        $err = & $Validate $v
        if ($err) { Write-Host "  $err" -ForegroundColor Red; continue }
        gh variable set $Name --repo $Repo --body $v | Out-Null
        if ($LASTEXITCODE -eq 0) { Write-Host "  $Name = $v" -ForegroundColor Green } else { Write-Host "  gh variable set failed" -ForegroundColor Red }
        return
    }
}

$isId = { param($v) if ($v -notmatch '^\d+$') { "a number (file id)" } }
$isUrl = { param($v) if ($v -notmatch '^https://(www\.)?nexusmods\.com/') { "a Nexus Mods URL (https://www.nexusmods.com/...)" } }

# API key
$hasKey = $secrets -contains "NEXUSMODS_API_KEY"
Write-Host ""
Write-Host "API key: https://www.nexusmods.com/settings/api-keys" -ForegroundColor Cyan
$secure = Read-Host "NEXUSMODS_API_KEY [$(if ($hasKey) { 'set' } else { 'not set' }), Enter = keep, '-' = remove]" -AsSecureString
$key = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
if ($key -eq "-") {
    if ($hasKey) { gh secret delete NEXUSMODS_API_KEY --repo $Repo | Out-Null; Write-Host "  removed" -ForegroundColor Yellow }
} elseif ($key) {
    $ok = $true
    if (-not $SkipValidate) {
        try {
            $user = Invoke-RestMethod -Uri "https://api.nexusmods.com/v1/users/validate.json" -Headers @{ apikey = $key; "Application-Name" = "GuiGuBaHuang-ModLib-setup"; "Application-Version" = "1.0" } -TimeoutSec 20
            Write-Host "  key valid (Nexus user: $($user.name))" -ForegroundColor Green
        } catch {
            Write-Host "  Nexus refused the key: $($_.Exception.Message)" -ForegroundColor Red
            $ok = $false
        }
    }
    if ($ok) {
        $key | gh secret set NEXUSMODS_API_KEY --repo $Repo | Out-Null
        if ($LASTEXITCODE -eq 0) { Write-Host "  NEXUSMODS_API_KEY saved" -ForegroundColor Green } else { Write-Host "  gh secret set failed" -ForegroundColor Red }
    }
} else { Write-Host "  kept" -ForegroundColor DarkGray }
$key = $null

foreach ($mod in @("ModLib", "Taoist")) {
    $u = $mod.ToUpper()
    Write-Host ""
    Write-Host "$mod (Enter = keep, '-' = remove; no file id = no Nexus upload of $mod)" -ForegroundColor Cyan
    Set-RepoVariable "NEXUS_${u}_URL" "  Mod page URL" $isUrl
    Set-RepoVariable "NEXUS_${u}_FILE_ID" "  File id" $isId
}

Write-Host ""
Write-Host "Done. Next release: VS Code task 'Release: Tag ...' (scripts/create-release-tag.ps1)." -ForegroundColor Green
