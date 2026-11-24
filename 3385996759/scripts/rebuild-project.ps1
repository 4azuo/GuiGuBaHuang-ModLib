# Shared script: scripts\project\ of this repository: rebuild-project.ps1 (this project: settings.json of this folder)
& (Join-Path $PSScriptRoot '../../scripts/project/rebuild-project.ps1') -ProjectScripts $PSScriptRoot @args
exit $LASTEXITCODE
