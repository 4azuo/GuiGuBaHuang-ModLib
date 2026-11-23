# Fouru's ModLib

Framework for mods of Gui Gu Ba Huang (鬼谷八荒): C# (.NET Framework 4.7.2) + MelonLoader/Harmony.
Steam Workshop: [Fouru's ModLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3385996759).

ModLib sits between the game (Unity Il2Cpp + MelonLoader/Harmony) and content mods such as
[Taoist](https://steamcommunity.com/sharedfiles/filedetails/?id=3161035078): it turns the game's timers and events
into one ordered event model, keeps the data of the mods out of the save file (json cache), and offers helpers.

- Events dispatched to every child mod in order (`ModEvent`, `ModChild`), data flow between child mods
- Mod data kept in json caches next to the save (not in the save file), per save game or global
- Game configs created / replaced from json, translation system (replaces LocalText.json)
- Helpers: UI (custom windows, covers over game windows), battles, units, items, async and delayed actions
- Debug logs, error reports (BugSplat, only if the player allows it)

## Use it in a mod

ModLib is built into `ModLib.dll`, which a mod references. The mod projects, the documentation and the build scripts
live in the main repository, where this repository is the `GuiGuBaHuang-ModLib` submodule:
[GuiGuBaHuang-Taoist](https://github.com/4azuo/GuiGuBaHuang-Taoist) (docs: `docs/ModLib/`, `docs/01-ModMain.md` ...,
the ModLib mod project: `3385996759/`, the ModCreator tool: `GuiGuBaHuang-ModCreator`).

## Build

- Visual Studio 2022 / `dotnet build ModLib.csproj -c Release` (.NET Framework 4.7.2 targeting pack).
- The game references point to `C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒\MelonLoader\...`: change the
  paths in `ModLib.csproj` if the game is installed elsewhere (the game must have been started once with MelonLoader).
- `lib/Newtonsoft.Json.dll`: the Newtonsoft.Json shipped with the mod.

## License

MIT, see [LICENSE](LICENSE).
