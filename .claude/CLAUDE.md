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
- Batch independent operations when possible

## Build & Deploy

Always build in Release. Run scripts with `powershell -NoProfile -ExecutionPolicy Bypass -File <script>`.

- Build one project: `<ProjectId>/scripts/build-project.ps1` (clean + build: `rebuild-project.ps1`)
- Build + deploy to Steam Workshop: `<ProjectId>/scripts/rebuild-and-deploy.ps1`
- All projects: `scripts/rebuild-and-deploy-all.ps1`
- Auto-build watcher writes failures to `<ProjectId>/scripts/auto-built-errors.log` (use `/fix-build-errors`)
- 3161035078 depends on ModLib.dll from `3385996759/CodeArtifacts/` — rebuild ModLib first when changing it
- Same scripts are exposed as VS Code tasks in `.vscode/tasks.json`

## Documentation

Read the relevant doc from `.github/docs/` before answering questions or making non-trivial changes:

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
