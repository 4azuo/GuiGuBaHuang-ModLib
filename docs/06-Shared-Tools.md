# Shared tools of the mod repositories

This repository: `ModLib/` (C# sources, `ModLib.csproj`), `3385996759/` (the ModLib Workshop project: ModMain, configs,
SampleConfs, build output `debug/Mod_flQrxq` committed and reused by the mods), the tools below. ModLib is built and
released here (`3385996759/scripts/`, `scripts/release/`, `.github/workflows/release.yml`: tags `ModLib-<version>`).

The tools used by every mod repository built on ModLib live here, so a fix reaches every mod with the submodule
update. The **data stays in the mod repository** (logs, scenarios, configs, pictures, translation cache, generated
viewer pages). A mod repository holds this repository as the `GuiGuBaHuang-ModLib` submodule at its root; the tools
find it as the parent folder of ModLib (or `$env:MOD_REPO`).

## What a mod repository provides

| File | Content |
|---|---|
| `scripts/settings.json` | `"projects"` (built / deployed / released by this repository, in this order), `"dependencies"` (projects of other repositories copied to the Workshop folder, not built, before them: `GuiGuBaHuang-ModLib/3385996759`, the ModLib build committed in the submodule), `"modProject"` (project id of the mod), `"modName"` (shown by the viewers and the autotest), `"modId"` (folder of the mod in `<game data>\mod`, e.g. `nE7UL2`: autotest requests and snapshots), `"steamWorkshopPath"`, `"repositoryPath"` |
| `scripts/setup/install.ps1`, `01-clone-repository.ps1` | the setup of a new machine: clones the repository with this submodule, then runs the shared steps of `scripts/setup/` here (downloaded from GitHub before the clone) |
| `<ProjectId>/scripts/settings.json` | `projectId`, `modName` (`Mod_<id>`), `releaseName`, `releaseRequires` (e.g. `ModLib`: the release notes link to the release of the version in the submodule, `ModLib-<version>`, and `create-release-tag.ps1` stops while it is not released), version file, auto-build delays |
| `<ProjectId>/scripts/*.ps1` | 2-line wrappers of `scripts/project/*.ps1` (`-ProjectScripts $PSScriptRoot @args`) |
| `<modProject>/tests/scenarios.json`, `scenario-status.json` | autotest scenarios and the latest OK/NG per id |
| `tests/UnitTests/UnitTests.csproj` | imports `GuiGuBaHuang-ModLib/tests/UnitTests.props`, references its build output, holds the mod tests |

## Tools

| Folder | Tools | Writes into the mod repository |
|---|---|---|
| `scripts/setup/` | `install` (ModLib alone), `00-install-dependencies`, `02-configure-settings`, `04-verify-environment`, `setup-nexus` (`-RepoPath`: ModLib or a mod repository), `_common.ps1` (helpers, also of the game tools) | the paths of `scripts/settings.json` and `<ProjectId>/scripts/settings.json` |
| `scripts/run-unittests.ps1` | builds the projects and runs `tests/UnitTests/UnitTests.csproj` of the repository (ModLib alone: `tests/ModLibTests.csproj`) | `logs/unittests/` |
| `scripts/release/` | `create-release-tag`, `package-release` (+ `_release.ps1`): each repository releases its own projects (repository: `-RepoRoot` or the git repository of the current folder) | `dist/` |
| `scripts/project/` | `build-project`, `clean-project`, `rebuild-project`, `rebuild-and-deploy`, `copy-to-steam`, `copy-from-steam`, `compress`, `start-auto-build` (+ `auto-build-and-deploy.py`) | `<ProjectId>/debug/`, `<ProjectId>/scripts/auto-built-errors.log` |
| `scripts/game/` | `run-game`, `run-autotest` (+ `-parallel`, `merge-parallel-run`), `new-game-slot`, `set-scenario-status`, `run-promo` / `collect-promo`, effect galleries (`run-effect-gallery`, `collect-effects`, `*_effects.py`) | `logs/game-runs/`, `logs/autotest/`, `<modProject>/tests/scenario-status.json`, `<modProject>/ModProject/SteamImg/promo-*/`, `logs/promo-clips/` (video clips of `AutoTestContext.Clip`, `"clip": true` promo entries) (effects go to `docs/Effects/` here, their lists to `3385996759/ModProject/ModConf/_Def_Res_Efx_*.txt`) |
| `scripts/` | `pin-test-run.ps1`, `effect-viewer-server.py` (viewer server with saving), `extract-game-assets.py` (writes `docs/GameAssets/` here) | `logs/*/<run>/pinned.json` |
| `test-viewer/` | page template, `app.js`, `style.css`, `update-data.ps1 [-RepoRoot]` | `logs/test-viewer/index.html` + `data/runs.js` (git-ignored with `logs/`) |
| `promo-viewer/` | page template, `app.js`, `style.css`, `update-data.py [--repo]` | `logs/promo-viewer/index.html` + `data/promo.js` (git-ignored with `logs/`) |
| `Translate/` | `run.py`, `ai_bridge.py` (see `Translate/README.md`) | `<ProjectId>/ModProject/ModConf/**`, `.translate-cache/` (in git: a text is never translated twice) |
| `tests/` | `UnitTests.props` (framework, MSTest, game assemblies), `Shared/` (`TestSetup`, `Check`), `ModLib/` (tests of ModLib, `IncludeModLibTests=true`), `ModLibTests.csproj` (ModLib alone) | - |
| `tools/SlotDataPath/` | MelonLoader plugin of the game slots (installed by `new-game-slot.ps1`) | - |

## ModLib alone

A clone of this repository alone has its own `scripts/settings.json` (`"projects": ["3385996759"]`), setup
(`scripts/setup/install.ps1`, guide: `INSTALLATION.md`), tests and VS Code tasks (`.vscode/tasks.json`: build / deploy / compress / auto-build of
3385996759, release, unit tests, viewers, game pictures, Translate with its cache `.translate-cache/`, setup, Nexus).
As a submodule, use the tasks of the mod repository: they offer the ModLib project too.

## Autotest in the game

The runner is ModLib's: `ModLib/AutoTest/` (`AutoTestRunner`: request, phases, steps, screenshots, game slots, the player
put back between scenarios; `AutoTestContext` / `Scenario` / `AutoTestRequest`; `ScenarioRegistry`; `MockBugSplatServer`;
`Scenarios/EffectGallery.cs`: the effect galleries TE-01..03, tools run only when asked by id). A mod adds:

- its runner, a subclass with a Cache attribute (`[Cache("AutoTestEvent", CacheType = CacheAttribute.CType.Global, WorkOn =
  CacheAttribute.WType.All)] public class AutoTestEvent : AutoTestRunner`) and its hooks: `ModVersion`, `OnRequestLoaded`,
  `OnNewCharacterRequested`, `CaptureModState` / `RestoreModState` (its data put back between scenarios), `CanFightInTests`;
- its scenarios: `IScenarioDefinition` classes of its assembly (`CreateScenarios` finds them with ModLib's), their ids in
  `<modProject>/tests/scenarios.json` (the TE-xx ids too when the mod checks that every scenario class has an entry);
- `modId` in `scripts/settings.json` (the folder of the requests: `<game data>\mod\<modId>\autotest\`).

The reference: `3161035078/ModProject/ModCode/ModMain/Mod/AutoTestEvent.cs` and `AutoTest/` of GuiGuBaHuang-Taoist.
