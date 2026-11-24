# Shared script: GuiGuBaHuang-ModLib\scripts\project\rebuild-and-deploy.ps1 (this project: settings.json of this folder)
& (Join-Path $PSScriptRoot '../../GuiGuBaHuang-ModLib/scripts/project/rebuild-and-deploy.ps1') -ProjectScripts $PSScriptRoot @args
exit $LASTEXITCODE
