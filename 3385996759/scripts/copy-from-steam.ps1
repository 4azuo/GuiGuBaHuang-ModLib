# Shared script: scripts\project\ of this repository: copy-from-steam.ps1 (this project: settings.json of this folder)
& (Join-Path $PSScriptRoot '../../scripts/project/copy-from-steam.ps1') -ProjectScripts $PSScriptRoot @args
exit $LASTEXITCODE
