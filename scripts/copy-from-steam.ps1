# Shared script: GuiGuBaHuang-ModLib\scripts\project\copy-from-steam.ps1 (this project: settings.json of this folder)
& (Join-Path $PSScriptRoot '../../GuiGuBaHuang-ModLib/scripts/project/copy-from-steam.ps1') -ProjectScripts $PSScriptRoot @args
exit $LASTEXITCODE
