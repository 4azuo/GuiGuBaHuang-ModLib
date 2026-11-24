# Shared script: GuiGuBaHuang-ModLib\scripts\project\copy-to-steam.ps1 (this project: settings.json of this folder)
& (Join-Path $PSScriptRoot '../../GuiGuBaHuang-ModLib/scripts/project/copy-to-steam.ps1') -ProjectScripts $PSScriptRoot @args
exit $LASTEXITCODE
