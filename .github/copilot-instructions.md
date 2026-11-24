## Project Context

GuiGuBaHuang-ModLib: ModLib, the framework of the Gui Gu Ba Huang mods (C# .NET Framework 4.7.2 + MelonLoader/Harmony),
and the tools shared by the mod repositories (they hold this repository as the `GuiGuBaHuang-ModLib` submodule).

- **ModLib/** - C# sources (`ModLib.csproj`)
- **3385996759/** - the ModLib Workshop project (ModMain, ModConf, SampleConfs: templates of every game config type, build output `debug/Mod_flQrxq`)
- **scripts/**, **tests/**, viewers, **Translate/** - shared tools

## Working Principles

- Read the instruction files before modifying matching code
- Use existing Helper utilities before creating new ones; search for existing patterns first
- Check SampleConfs when working with configs; LocalText for every user-facing text
- Keep the public API compatible: every mod builds against ModLib
- Build: `3385996759/scripts/build-project.ps1` (Release), output `3385996759/debug/Mod_flQrxq/ModCode/dll/` (committed)

## Documentation

`docs/README.md` (map), `docs/guide/` (writing a mod with ModLib), `docs/00-Architecture.md` .. `05` (internals),
`docs/06-Shared-Tools.md` (shared tools), `docs/ModConf/00-SampleConfs-List.md` (config samples). Setup: `INSTALLATION.md`.
