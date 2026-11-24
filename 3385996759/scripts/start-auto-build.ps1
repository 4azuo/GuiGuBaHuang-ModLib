# Shared script: scripts\project\ of this repository: start-auto-build.ps1 (this project: settings.json of this folder)
& (Join-Path $PSScriptRoot '../../scripts/project/start-auto-build.ps1') -ProjectScripts $PSScriptRoot @args
exit $LASTEXITCODE
