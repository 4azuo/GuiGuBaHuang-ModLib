# GuiGuBaHuang-ModLib

Mods for Gui Gu Ba Huang (鬼谷八荒), C# (.NET Framework 4.7.2) + MelonLoader/Harmony.

## Projects

- **3385996759** - ModLib framework (core library, provides ModLib.dll)
- **3161035078** - Taoist mod (uses ModLib, also the reference example mod)
- **GuiGuBaHuang-ModCreator** - WPF tool for creating/editing mod projects (submodule)

## Structure

- **ModProject/ModConf/** - JSON configs (name as `*_<original>.json`, reference SampleConfs)
- **ModProject/ModCode/** - C# source (solution: `ModProject/ModCode/ModMain/ModMain.sln`)
- **ModProject/ModImg/** - Assets
- **3385996759/SampleConfs/** - Templates for all game config types
- **debug/Mod_xxxxx/** - Build output (`ModCode/dll/`, `ModConf/`, `ModImg/`)
- **<ProjectId>/scripts/** - Per-project build/deploy scripts + `settings.json`
- **scripts/** - Scripts for all projects + `settings.json`; `scripts/setup/` for first-time install

## Guidelines

- Folder-specific rules live in `.claude/rules/` and load automatically for matching files
- Use existing Helper utilities before creating new ones
- Check SampleConfs when working with configs
- Search for existing patterns before implementing new features
- Use LocalText.json for all user-facing text
- No TODO notes in code: write open work in `docs/Taoist/05-Backlog.md`
- Batch independent operations when possible

## Build & Deploy

Always build in Release. Run scripts with `powershell -NoProfile -ExecutionPolicy Bypass -File <script>`.

- Build one project: `<ProjectId>/scripts/build-project.ps1` (clean + build: `rebuild-project.ps1`)
- Build + deploy to Steam Workshop: `<ProjectId>/scripts/rebuild-and-deploy.ps1`
- All projects: `scripts/rebuild-and-deploy-all.ps1`
- Auto-build watcher writes failures to `<ProjectId>/scripts/auto-built-errors.log` (use `/fix-build-errors`)
- 3161035078 depends on ModLib.dll from `3385996759/CodeArtifacts/` — rebuild ModLib first when changing it
- Same scripts are exposed as VS Code tasks in `.vscode/tasks.json`
- Environment check: `scripts/setup/04-verify-environment.ps1` (many "missing type/member" build errors after a game update = stale `MelonLoader\Managed`, not code bugs)

## Testing in game

- Run the game and collect logs: `scripts/game/run-game.ps1` (e.g. `-UntilModLoaded -StopGame -Duration 600`); read `logs/game-runs/<latest>/summary.json` first, then `timeline.log`
- Paths of game / Workshop / log folders: `scripts/open-folder.ps1 <target> -Print`
- Details: `docs/06-Debug.md`
- Unit tests (no game): `scripts/run-unittests.ps1` (`tests/UnitTests`; `-IncludeKnownIssues` runs the tests reproducing open bugs)
- Known bugs to check: `docs/Known-Issues.md`
- Taoist autotest (scenarios + evidence): `scripts/game/run-autotest.ps1 -Save <test save>` -> evidence in `logs/autotest/<time>/`; OK/NG per scenario id in `3161035078/tests/scenario-status.json`. All results (autotest + unit tests, every run, screenshots by step): `test-viewer/index.html` (data from `scripts/update-test-viewer.ps1`). Details: `docs/Taoist/04-Autotest.md`

## Documentation

Read the relevant doc from `docs/` before answering questions or making non-trivial changes:

- `00-Getting-Started.md` - Setup and structure
- `01-ModMain.md` - ModMain class and initialization
- `02-Events.md` - Event system
- `03-Configs.md` - Configuration structure
- `04-LocalText.md` - Localization
- `05-Helpers.md` - Helper functions
- `06-Debug.md` - Debugging, log locations
- `07-Build-Deploy.md` - Build scripts
- `ModConf/00-SampleConfs-List.md` - Index of config samples (per-category files `01`-`12`)
- `ModCreator/00-Overview.md` - ModCreator tool (per-feature files `01`-`05`)
- `ModLib/00-Architecture.md` - ModLib internals: dispatch, load sequence, monthly tick (`01` event reference, `02` cache/persistence, `03` built-in events, `04` UI framework, `05` attributes/objects/enums)
- `Taoist/00-Overview.md` - Taoist structure and event order (`01` features per event + known issues, `02` configs, `03` test scenarios, `04` autotest/evidence/unit tests, `05` backlog: open work and former TODO notes, `06` storyline draft)
