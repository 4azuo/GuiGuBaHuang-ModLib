# Shared script: scripts\project\ of this repository: build-project.ps1 (this project: settings.json of this folder)
& (Join-Path $PSScriptRoot '../../scripts/project/build-project.ps1') -ProjectScripts $PSScriptRoot @args
exit $LASTEXITCODE
