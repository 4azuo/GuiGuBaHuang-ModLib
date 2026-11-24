# Shared script: scripts\project\ of this repository: compress.ps1 (this project: settings.json of this folder)
& (Join-Path $PSScriptRoot '../../scripts/project/compress.ps1') -ProjectScripts $PSScriptRoot @args
exit $LASTEXITCODE
