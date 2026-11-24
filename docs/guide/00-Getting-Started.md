# ModLib - Getting Started Guide

ModLib is a framework for creating mods for Gui Gu Ba Huang (鬼谷八荒).

**Important:** ModLib (Workshop 3385996759, this repository) provides ModLib.dll. The reference example mod is Taoist
(Workshop 3161035078, [GuiGuBaHuang-Taoist](https://github.com/4azuo/GuiGuBaHuang-Taoist)), which holds this repository as
its `GuiGuBaHuang-ModLib` submodule (ModLib.dll, shared scripts).

## Requirements

**Software:** Visual Studio 2019+, .NET Framework, dnSpy, MelonLoader, Git, WinRAR

**Dependencies:** ModLib.dll, MelonLoader, UnhollowerBaseLib, Il2CppInterop, Game DLLs

**Setting up a new machine:** the installation guide of the mod repository (e.g. [Taoist](https://github.com/4azuo/GuiGuBaHuang-Taoist/blob/main/INSTALLATION.md)); for
ModLib itself, [INSTALLATION.md](../../INSTALLATION.md) of this repository

## Project Structure

```
<ProjectId>/
├── ModProject/
│   ├── ModCode/ModMain/    # Main code (Mod/, Const/, Enum/, Helper/, Object/)
│   ├── ModConf/            # JSON configs
│   ├── ModImg/             # Assets
│   └── SteamImg/           # Workshop images
├── debug/Mod_xxxxx/ModCode/dll/  # Output (ModMain.dll, ModLib.dll)
├── scripts/                # Settings & Build scripts
```

## Creating a New Mod

**Step 1:** Create Steam Workshop item, get Workshop ID

**Step 2:** Copy the folder structure of a mod project (e.g. `3161035078` of GuiGuBaHuang-Taoist), create folders: ModMain/, Mod/, Const/, Helper/, ModConf/, ModImg/, debug/

**Step 3:** Configure `<ProjectId>/scripts/settings.json` (projectId, modName, paths) and add the project id to `"projects"` of the root
`scripts/settings.json` (with `"dependencies": ["GuiGuBaHuang-ModLib/3385996759"]`, `modProject`, `modName`, `modId`: [07-Build-Deploy](07-Build-Deploy.md))

---

## Detailed Guides

From step 4 onwards, see detailed guide files:

### Step 4: [Create ModMain.cs](./01-ModMain.md)
Guide for creating the mod entry point

### Step 5: [Create Mod Events](./02-Events.md)
Guide for creating event handlers for game logic

### Step 6: [Create Config Files](./03-Configs.md)
Guide for creating JSON configuration files

### Step 7: [Create LocalText](./04-LocalText.md)
Guide for creating multi-language support

### Step 8: [Setup Helpers](./05-Helpers.md)
Guide for creating helper classes

### Step 9: [Debug and Troubleshooting](./06-Debug.md)
Guide for viewing logs and debugging mods

### Step 10: [Build and Deploy](./07-Build-Deploy.md)
Guide for building, compressing, and deploying mods

---
