# Shared script: scripts\project\ of this repository: copy-to-steam.ps1 (this project: settings.json of this folder)
& (Join-Path $PSScriptRoot '../../scripts/project/copy-to-steam.ps1') -ProjectScripts $PSScriptRoot @args
exit $LASTEXITCODE
