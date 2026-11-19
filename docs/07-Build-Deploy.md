# Build and Deploy

## Build Scripts

Located in `3161035078/scripts/` (same for `3385996759/scripts/`):

**clean-project.ps1**
- Cleans build artifacts and output folders

**build-project.ps1**
- Compiles the project in Release mode
- Copies dependencies (ModLib.dll)
- Outputs to `debug/Mod_xxxxx/ModCode/dll/`

**rebuild-project.ps1**
- Runs clean-project.ps1
- Runs build-project.ps1

**start-auto-build.ps1**
- Watches for file changes
- Automatically rebuilds on save
- Requires Python and watchdog module

**copy-to-steam.ps1**
- Copies built mod to Steam Workshop folder
- Used for testing and publishing

**copy-from-steam.ps1**
- Copies mod from Steam Workshop back to Git repository

**rebuild-and-deploy.ps1**
- Runs rebuild-project.ps1
- Runs copy-to-steam.ps1
- Complete workflow for testing

**compress.ps1**
- Compresses `debug/Mod_xxxxx/` to `debug/Mod_xxxxx.rar` (requires WinRAR)

## Root Scripts

Located in `scripts/` (settings in `scripts/settings.json`):

**rebuild-and-deploy-all.ps1** - Rebuilds every project in `settings.json`, then copies all to Steam Workshop

**copy-both-to-steam.ps1** / **copy-both-from-steam.ps1** - Copies all projects to/from Steam Workshop

**compress-mods.ps1** - Runs each project's `compress.ps1`

**update-repo.ps1** - Pulls latest changes and updates submodules (`-Force` to auto-stash, `-NoSubmodules` to skip submodules)

**setup/** - One-time environment setup: `install.ps1` runs `00-install-dependencies.ps1` → `01-clone-repository.ps1` → `03-extract-resources.ps1` → `02-configure-settings.ps1` → `04-verify-environment.ps1` (shared helpers in `_common.ps1`). Every step is re-runnable and works from any repo folder; see [INSTALLATION.md](../../INSTALLATION.md)

**package-release.ps1** - Zips the folder `<ProjectId>/debug/<modName>` (as made by the build task: DLLs, ModConf, ModImg, SteamImg, ModExportData.cache) of each project of a target into `dist/` (`ModLib-<ver>.zip`, `Taoist-<ver>.zip`) + `release-notes.md` (downloads, install, SHA-256). `-Target ModLib|Taoist|All`; `-Tag <tag>` (CI) takes the target from the tag and fails when the tag does not match the version of the source code; `-PrepareResources` (CI) first runs `build-project.ps1 -ResourcesOnly`. VS Code task **Release: Package Zips (local)**

**create-release-tag.ps1** `-Target ModLib|Taoist|All` - Creates the release tag on HEAD and pushes it (starts the CI). Checks: tag not existing yet (local + origin), uncommitted changes of the mods (asks), HEAD pushed (offers to push the branch); asks a confirmation (`-Yes` skips, `-NoPush` only tags). VS Code tasks **Release: Tag ModLib / Taoist / All**

**_release.ps1** - Shared: projects, versions (`versionFile` / `versionPattern` of each `<ProjectId>/scripts/settings.json`), targets and tags

**setup/setup-nexus.ps1** - Nexus Mods setup of the CI on GitHub with `gh`: secret `NEXUSMODS_API_KEY` (checked with Nexus, given to gh on stdin) and variables `NEXUS_<MOD>_URL` / `NEXUS_<MOD>_FILE_ID`. Shows what is set; Enter keeps a value, `-` removes it. VS Code task **Setup: Nexus Mods (CI)**

`build-project.ps1 -ResourcesOnly` (each project) - only the copy of ModConf/ModImg/SteamImg to `debug/` (no dotnet build)

## GitHub Release and Nexus Mods (CI)

`.github/workflows/release.yml` runs **only when a release tag is pushed** (never on a normal push):

| Target | Tag (version from the source code) | Zips |
|---|---|---|
| ModLib | `ModLib-<Version of 3385996759 ModMain.cs>` | `ModLib-<ver>.zip` |
| Taoist | `Taoist-<TAOIST_VERSION>` | `Taoist-<ver>.zip` |
| All | `Release-<TAOIST_VERSION>` | both |

Steps: checkout (with Git LFS: SteamImg) → `package-release.ps1 -Tag <tag> -PrepareResources` (the CI cannot build: the DLLs of `debug/` are the committed ones, ModConf/ModImg/SteamImg are copied by `build-project.ps1 -ResourcesOnly`) → GitHub Release `<tag>` with the zips and the notes → Nexus Mods (optional): each zip as a new version of an existing Nexus file with [`Nexus-Mods/upload-action`](https://github.com/Nexus-Mods/upload-action) (previous version archived).

Publishing a version: bump the version → build in Release (task) → commit (the DLLs of `debug/` included) and push → task **Release: Tag ...**.

Nexus (task **Setup: Nexus Mods (CI)**, or Settings → Secrets and variables → Actions); the mod page and a first file must be created by hand on Nexus first:

| Name | Kind | Value |
|---|---|---|
| `NEXUSMODS_API_KEY` | secret | API key of https://www.nexusmods.com/settings/api-keys |
| `NEXUS_TAOIST_FILE_ID`, `NEXUS_MODLIB_FILE_ID` | variable | File id (Files tab of the mod page → "Advanced", or Manage Files). Not set: no Nexus upload |
| `NEXUS_TAOIST_URL`, `NEXUS_MODLIB_URL` | variable | Mod page URL, linked in the release notes |

## VS Code Tasks

All scripts above are available in `.vscode/tasks.json`: **Terminal → Run Task...** (or `Ctrl+Shift+B` for the default build task "All: Rebuild & Deploy").
