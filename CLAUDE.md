# GuiGuBaHuang-ModLib

ModLib: the framework of the Gui Gu Ba Huang (鬼谷八荒) mods, C# (.NET Framework 4.7.2) + MelonLoader/Harmony, and the
tools shared by the mod repositories. Public repository `4azuo/GuiGuBaHuang-ModLib`; the mods (e.g. GuiGuBaHuang-Taoist)
hold it as their `GuiGuBaHuang-ModLib` submodule.

## Layout

- **ModLib/** - C# sources (`ModLib.csproj`): `Attribute/`, `Const/`, `Converter/`, `Enum/`, `AutoTest/` (the in-game autotest runner of the mods: `AutoTestRunner`, scenarios model, registry, effect galleries), `Example/`, `Helper/`, `Mod/` (`Mod/Base/`: `ModMaster`, `ModChild`, `ModEvent`), `Object/`, `lib/` (Newtonsoft.Json)
- **3385996759/** - the ModLib Workshop project: `ModProject/ModCode/ModMain/` (ModMain of ModLib, builds `ModLib/ModLib.csproj`), `ModProject/ModConf/` (configs, `localText.json`: keys `libtxt<id>`, ids `999990000`+), `SampleConfs/` (templates of every game config type), `debug/Mod_flQrxq/` (build output; `ModCode/dll` committed: the mods build against it), `scripts/` (settings + wrappers)
- **scripts/** - shared tools: `project/` (build / deploy), `game/` (game runs, autotest, game slots, promo, effect galleries), `release/`, `setup/`, `run-unittests.ps1`, repository scripts; `settings.json` of this repository
- **tests/** - `UnitTests.props` (shared test setup), `Shared/`, `ModLib/` (tests of ModLib), `ModLibTests.csproj`
- **test-viewer/**, **promo-viewer/**, **effect-viewer/**, **Translate/**, **tools/SlotDataPath/** - shared viewers and tools (the data stays in the mod repository)
- **docs/** - `guide/` (writing a mod with ModLib), internals `00`-`05`, `06-Shared-Tools.md`, `ModConf/` (config samples), `Effects/`, `GameAssets/` (Git LFS)

## Guidelines

- Use existing Helper utilities before creating new ones; search for existing patterns first
- Check `3385996759/SampleConfs` (and `docs/ModConf/`) when working with configs
- LocalText for every user-facing text (`docs/guide/04-LocalText.md`)
- A change of ModLib is a change for every mod: keep the public API compatible, document new events/helpers in `docs/`
- PowerShell scripts: ASCII-only (Windows PowerShell 5.1 reads BOM-less scripts as ANSI); the shared tools take `-RepoRoot` / `-RepoPath` (default: the git repository of the current folder) and read the mod from `scripts/settings.json` of that repository, never a mod id or path written in the code
- No TODO notes in code

## Build, test, release

Always build in Release. Run scripts with `powershell -NoProfile -ExecutionPolicy Bypass -File <script>`.

- Build: `3385996759/scripts/build-project.ps1` (clean + build: `rebuild-project.ps1`, + deploy: `rebuild-and-deploy.ps1`); then commit the DLLs of `3385996759/debug/Mod_flQrxq/ModCode/dll` (the mods reference them)
- Auto-build watcher failures: `3385996759/scripts/auto-built-errors.log` (`/fix-build-errors`)
- Unit tests: `scripts/run-unittests.ps1` (`tests/ModLibTests.csproj`)
- Release: `scripts/release/create-release-tag.ps1` (tag `ModLib-<version of 3385996759 ModMain.cs>`, CI `.github/workflows/release.yml`)
- Setup / environment check: `scripts/setup/install.ps1`, `scripts/setup/04-verify-environment.ps1` (many "missing type/member" build errors after a game update = stale `MelonLoader\Managed`, not code bugs)
- VS Code tasks: `.vscode/tasks.json`; setup of a machine: `INSTALLATION.md`

## Documentation

Read the relevant doc before answering questions or making non-trivial changes: `docs/README.md` (map), `docs/guide/`
(`00-Getting-Started` … `07-Build-Deploy`), `docs/00-Architecture.md` (dispatch, load sequence, monthly tick; `01` event
reference, `02` cache/persistence, `03` built-in events, `04` UI framework, `05` attributes/objects/enums),
`docs/06-Shared-Tools.md` (what a mod repository provides), `docs/ModConf/00-SampleConfs-List.md`.
