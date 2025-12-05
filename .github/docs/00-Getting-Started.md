# ModLib - Getting Started Guide

ModLib is a framework for creating mods for Gui Gu Ba Huang (鬼谷八荒).

**Important:** ModLib (3385996759) provides ModLib.dll. Project 3161035078 is an example mod.

## Requirements

**Software:** Visual Studio 2019+, .NET Framework, dnSpy, MelonLoader, Git, WinRAR

**Dependencies:** ModLib.dll, MelonLoader, UnhollowerBaseLib, Il2CppInterop, Game DLLs

## Project Structure

```
<ProjectId>/
├── ModProject/
│   ├── ModCode/ModMain/    # Main code (Mod/, Const/, Enum/, Helper/, Object/)
│   ├── ModConf/            # JSON configs
│   ├── ModImg/             # Assets
│   └── SteamImg/           # Workshop images
├── debug/Mod_xxxxx/ModCode/dll/  # Output (ModMain.dll, ModLib.dll)
├── tasks/                  # Settings & Build scripts
```

## Creating a New Mod

**Step 1:** Create Steam Workshop item, get Workshop ID

**Step 2:** Copy folder structure from 3161035078, create folders: ModMain/, Mod/, Const/, Helper/, ModConf/, ModImg/, debug/

**Step 3:** Configure `tasks/settings.json` (projectId, modName, paths) and add to `tasks/settings.json`

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
