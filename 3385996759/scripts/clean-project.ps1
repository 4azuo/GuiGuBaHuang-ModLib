# Shared script: scripts\project\ of this repository: clean-project.ps1 (this project: settings.json of this folder)
& (Join-Path $PSScriptRoot '../../scripts/project/clean-project.ps1') -ProjectScripts $PSScriptRoot @args
exit $LASTEXITCODE
