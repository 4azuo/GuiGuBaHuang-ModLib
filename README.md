# Fouru's ModLib

[![Steam Workshop](https://img.shields.io/badge/Steam%20Workshop-Fouru's%20ModLib-1b2838?logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=3385996759)
[![License: MIT](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Ko-fi](https://img.shields.io/badge/Ko--fi-support-ff5e5b?logo=ko-fi)](https://ko-fi.com/fouru)

A framework for code mods of Gui Gu Ba Huang (鬼谷八荒 / Tale of Immortal): C# (.NET Framework 4.7.2) +
MelonLoader/Harmony. Write a mod as a set of small events; ModLib takes care of the game's timers, the save data, the
configs, the translations and the UI.

ModLib sits between the game (Unity Il2Cpp + MelonLoader/Harmony) and content mods such as
[Taoist](https://steamcommunity.com/sharedfiles/filedetails/?id=3161035078), a hardcore rework of the game built
entirely on it.

## For players

- Subscribe on the [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3385996759), then **move
  ModLib to the TOP of your mod list** (above every mod that uses it) and **restart the game**. Mods that need a newer
  ModLib tell you so on the title screen.
- **Save-safe**: mod data is stored in separate JSON files and never written into your save file, so removing a mod
  will not break your save. The data of the saves you delete in the game is cleaned up automatically.
- **Fast**: the monthly events of the mods are dispatched quickly, and the mod data is saved in the background (only
  the files that changed).
- Child mods load and update in a fixed order, so they do not conflict with each other.
- Error reports to the author of a mod (BugSplat) are sent **only if you allow it** (title screen).

## For modders

| | |
|---|---|
| **Event system** | One ordered event model for game, map, battle, UI and timers (including a monthly tick), with conditions and delayed execution |
| **Persistence** | Global and per-save caches as plain JSON (no encryption): easy to inspect and debug |
| **Data sharing** | Child mods exchange data; each mod gets its own Parameter Store |
| **Configs** | Create or replace game configs from JSON, with a sample of every config type ([docs/ModConf](docs/ModConf/00-SampleConfs-List.md)) |
| **Translations** | `*_localText.json` per feature, 4 languages of the game + 8 locale folders (de, es, fr, ja, la, ru, th, vi) |
| **UI framework** | Build windows, buttons, inputs and other controls in code, or cover a window of the game |
| **Icons in texts** | Write the game's icons (spirit stones, herbs, ores, stats...) in any text with `[icon:NAME]` ([gallery](docs/GameAssets/index.html)) |
| **Dramas and portraits** | Dialogs with options and backgrounds; characters drawn with the game's portrait art |
| **Battle helpers** | Damage, skills, the game's visual effects ([effect viewer](effect-viewer/index.html)), the battle scene and camera |
| **Helpers** | Units, items, async and delay functions, Workshop descriptions as rich text, UI automation for tests, and more |
| **Debug** | Log levels, error dialogs, a monthly profile of the slowest handlers, error reports to your own BugSplat database |

## Create a mod with ModLib

This repository holds the ModLib sources (`ModLib/`), the ModLib Workshop project (`3385996759/`: ModMain, configs,
SampleConfs, build output) and the tools shared by the mod repositories ([docs/06-Shared-Tools.md](docs/06-Shared-Tools.md)).
The complete example mod is [GuiGuBaHuang-Taoist](https://github.com/4azuo/GuiGuBaHuang-Taoist) (it holds this repository
as its `GuiGuBaHuang-ModLib` submodule). Step-by-step guide:
[Getting started](docs/guide/00-Getting-Started.md) →
[ModMain](docs/guide/01-ModMain.md) →
[Events](docs/guide/02-Events.md) →
[Configs](docs/guide/03-Configs.md) →
[LocalText](docs/guide/04-LocalText.md) →
[Helpers](docs/guide/05-Helpers.md) →
[Debug](docs/guide/06-Debug.md) →
[Build and deploy](docs/guide/07-Build-Deploy.md).

![image](https://github.com/4azuo/GuiGuBaHuang-Taoist/assets/11677054/91921f05-251a-4f1e-a2e9-d1e5bdd853d3)

### 1. Reference ModLib.dll

- Build ModLib (see [Build](#build)) or take `ModLib.dll` from the ModLib Workshop folder (`ModCode/dll/`), and
  reference it in your mod project together with the game DLLs of `<game>\MelonLoader\Managed\`.
- Change the MelonLoader folder in the `.csproj` files if the game is installed elsewhere. Build in **Release**.
- The output of your mod (`Mod_<your mod id>/ModCode/dll/`) holds your dll and `ModLib.dll`; the main repository's
  scripts (`build-project.ps1`, `rebuild-and-deploy.ps1`) copy it, with `ModConf/` and `ModImg/`, to the Workshop folder.

![image](https://github.com/user-attachments/assets/9554b89d-61c6-4ef5-9bad-97c3c62107ab)

### 2. ModMain and events

![image](https://github.com/user-attachments/assets/34a75a28-38df-4922-aa43-732b8ab3b035)

1. Inherit **ModChild** to configure your mod (its cache id must be your mod id); ModLib calls its on-events.
2. Write the features as **ModEvent** classes; use **ModMain** only to configure the mod.

   ![image](https://github.com/4azuo/GuiGuBaHuang-Taoist/assets/11677054/e8da9fd1-89d0-4870-ace4-b7153dace9f2)

3. Mark an event with `[Cache]` to register it:
   - `CacheType = Global`: created when the game starts; `CacheType = Local`: created when a save is loaded.
   - `WorkOn = Global`: deleted when a save is loaded.
   - Every property of a ModEvent is cached as JSON; skip one with `[JsonIgnore]`.

   ![image](https://github.com/user-attachments/assets/42549f20-dcd7-49b0-b624-5caae73ae1d6)

4. Add your events to `_EventOrderIndex.json` (the order of the dispatch).

   ![image](https://github.com/user-attachments/assets/4b338668-a2a0-4a1c-8fcf-fb261e5de7e7)

Every event and when it is called: [docs/01-Event-Reference.md](docs/01-Event-Reference.md); the events ModLib ships:
[docs/03-Builtin-Events.md](docs/03-Builtin-Events.md); examples of the game's API (timers, events, world, battle, resources): [Example/Example.cs](Example/Example.cs).

### 3. Game configs as JSON

Put JSON files into `ModConf/` of your mod; ModLib loads them into the game's configs.

1. The file name is split by `_`: the last part is the name of the config (`mymod_taskBase.json` → `taskBase`).
2. Write only the properties you change. `"__name"` is a comment.
3. `|` gives the same value to several properties; for an array, `@` then `|` as separator.
4. A row whose `"id"` exists replaces that row; a new id adds a row.

Every config type and its fields: [docs/ModConf/00-SampleConfs-List.md](docs/ModConf/00-SampleConfs-List.md).

![image](https://github.com/4azuo/GuiGuBaHuang-Taoist/assets/11677054/7f9de987-89a8-441d-addc-a894d37ba746)

### 4. Texts

- No hardcoded text: every text shown to the player is a localText key, read with `GameTool.LS(key)` (never a language
  field such as `allText[key].en`).
- One `<feature>_localText.json` per feature, with `en`, `ch`, `tc`, `kr`; the locale folders `de es fr ja la ru th vi`
  hold the same keys with one text each (`"en|ch|tc|kr"`).
- Numbers: `{0:#,##0}` for amounts. Icons: `[icon:NAME]`, e.g. `{0:#,##0} [icon:zm_lingshi]`; set such texts with
  `Text.Set(...)` so the icon is drawn.

### 5. Logs, caches, error reports

- Logs (`DebugHelper.WriteLine`): `%USERPROFILE%\AppData\LocalLow\guigugame\guigubahuang\mod\flQrxq\`
  (every log is written by ModLib, mod id `flQrxq`). `ModConf/mod_configs.json` of ModLib sets the level
  (`Info`, `Fine`, `Finest`).
- Caches of your mod: `...\guigubahuang\mod\<your mod id>\` (`globals\` and `saves\<player id>\`).
- Error reports: create a free [BugSplat](https://www.bugsplat.com) database (letters, digits, `-` and `_`, not your
  e-mail or real name) and add `ModConf/crash_reports.json` to your mod:

  ```json
  { "Database": "mymod-ggbh", "ModName": "My mod", "LogLines": 200 }
  ```

  Every `DebugHelper.WriteLine(exception)` (also the errors ModLib catches in your events) goes to the database of the
  mod found in the stack trace, once per error and at most 5 per game session, and only if the player allowed it. Set
  the version of the reports with `CrashReportHelper.SetModVersion(modId, "1.0.0")` in `OnLoadClass`. Details:
  [docs/06-Debug.md](docs/guide/06-Debug.md#error-reports-bugsplat).

## Documentation

[docs/](docs/README.md): ModLib internals ([architecture](docs/00-Architecture.md), event reference, caches, built-in
events, [UI framework](docs/04-UI-Framework.md), attributes / objects), the game's config samples
([docs/ModConf](docs/ModConf/00-SampleConfs-List.md)), the effects of the game recorded in battle (`docs/Effects/`,
browse them with [effect-viewer/index.html](effect-viewer/index.html) after `python effect-viewer/update-data.py`) and
the pictures extracted from the game (`docs/GameAssets/`, gallery [index.html](docs/GameAssets/index.html)). The
pictures are kept with Git LFS: run `git lfs install` before cloning.

## Build

Setup of a machine, build, tests and release of ModLib: [INSTALLATION.md](INSTALLATION.md) (VS Code tasks in
`.vscode/tasks.json`).

- Visual Studio 2022 / `dotnet build ModLib/ModLib.csproj -c Release` (.NET Framework 4.7.2 targeting pack).
- The game references point to `C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒\MelonLoader\...`: change the
  paths in `ModLib/ModLib.csproj` if the game is installed elsewhere (the game must have been started once with MelonLoader).
- `lib/Newtonsoft.Json.dll`: the Newtonsoft.Json shipped with the mod.

## Support

ModLib is free and stays free. If it helps you, you can buy me a coffee on [Ko-fi](https://ko-fi.com/fouru).
Feedback: [GuiGuBaHuang-Taoist issues](https://github.com/4azuo/GuiGuBaHuang-Taoist/issues).

## License

MIT, see [LICENSE](LICENSE).
