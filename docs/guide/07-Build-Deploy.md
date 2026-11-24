# Build and Deploy

The scripts are shared by the mod repositories and live in this repository (`scripts/`); a mod repository holds it as
the `GuiGuBaHuang-ModLib` submodule. Paths below: `<ModLib>` = `GuiGuBaHuang-ModLib/` in a mod repository, the root
here. Every script works on a repository: `-RepoRoot`, else the git repository of the current folder.

## Project scripts

`<ProjectId>/scripts/` of a project holds `settings.json` and 3-line wrappers of `<ModLib>/scripts/project/`
(`-ProjectScripts $PSScriptRoot`):

| Script | Does |
|---|---|
| `clean-project.ps1` | cleans the build artifacts and output folders |
| `build-project.ps1` | compiles the project (Release), copies `CodeArtifacts` and the DLLs to `debug/Mod_xxxxx/ModCode/dll/`, then ModConf/ModImg/SteamImg to `debug/Mod_xxxxx/`; `-ResourcesOnly`: only the copy (the CI) |
| `rebuild-project.ps1` | clean + build |
| `start-auto-build.ps1` | watches ModCode/ModConf/ModImg and rebuilds + deploys on change (Python + watchdog); errors in `scripts/auto-built-errors.log` |
| `copy-to-steam.ps1` / `copy-from-steam.ps1` | copies the project folder to / from the Steam Workshop folder |
| `rebuild-and-deploy.ps1` | rebuild + copy to Steam |
| `compress.ps1` | `debug/Mod_xxxxx/` to `debug/Mod_xxxxx.rar` (WinRAR) |

A mod references the `ModLib.dll` committed in the submodule (`<ModLib>/3385996759/debug/Mod_flQrxq/ModCode/dll/`):
ModLib is built and released in its own repository (`3385996759/scripts/` there).

## Repository scripts (`<ModLib>/scripts/`)

`scripts/settings.json` of the repository: `"projects"` (built, deployed and released by the repository, in this
order), `"dependencies"` (projects of other repositories copied to the Workshop folder before them, not built: the
ModLib of the submodule, `GuiGuBaHuang-ModLib/3385996759`), `"modProject"`, `"modName"`, `"modId"` (the mod for the game
tools), `"steamWorkshopPath"`.

| Script | Does |
|---|---|
| `rebuild-and-deploy-all.ps1` | rebuilds every project, then copies the dependencies and the projects to Steam Workshop |
| `copy-both-to-steam.ps1` / `copy-both-from-steam.ps1` | copies the dependencies + projects to Steam / the projects back |
| `compress-mods.ps1` | runs `compress.ps1` of every project |
| `update-repo.ps1` | pulls and updates the submodules (`-Force` stashes first, `-NoSubmodules`) |
| `open-folder.ps1` | opens (or `-Print`s) a game / Workshop / build / log folder |
| `run-unittests.ps1`, `run-all-tests.ps1` | unit tests, unit tests + autotest (see [06-Shared-Tools](../06-Shared-Tools.md)) |
| `setup/` | setup of a machine: `install.ps1` (ModLib alone), `00-install-dependencies`, `02-configure-settings`, `04-verify-environment`, `setup-nexus` (a mod repository has its own `install.ps1` that clones it with the submodule, then runs these steps) |

## Release

Each repository releases its own projects (`<ModLib>/scripts/release/`):

- `create-release-tag.ps1 [-Target <releaseName>] [-RepoRoot <repository>]` - creates the tag `<releaseName>-<version>`
  on HEAD (version read from the source code: `versionFile` / `versionPattern` of `<ProjectId>/scripts/settings.json`)
  and pushes it, which starts the CI of the repository. Checks: tag not existing yet (local + origin), uncommitted
  changes of the mod (asks), HEAD pushed (offers to push the branch); `-Yes` skips the questions, `-NoPush` only tags.
- `package-release.ps1` - zips `<ProjectId>/debug/<modName>` into `dist/<releaseName>-<version>.zip` +
  `release-notes.md` (downloads, install, `releaseRequires` of the project settings such as "needs ModLib", SHA-256).
  `-Tag <tag>` (the CI) fails when the tag does not match the version of the source code; `-PrepareResources` (the
  CI) first runs `build-project.ps1 -ResourcesOnly`.

The CI (`.github/workflows/release.yml` of the repository) runs only when its tag is pushed: checkout (Git LFS, the
submodule for the shared scripts) → `package-release.ps1 -Tag <tag> -PrepareResources` (the CI cannot build: the DLLs
of `debug/` are the committed ones) → GitHub Release `<tag>` → Nexus Mods (optional): the zip as a new version of an
existing Nexus file ([`Nexus-Mods/upload-action`](https://github.com/Nexus-Mods/upload-action), previous version
archived).

| Repository | Tag | Zip |
|---|---|---|
| GuiGuBaHuang-ModLib | `ModLib-<Version of 3385996759 ModMain.cs>` | `ModLib-<ver>.zip` |
| a mod repository | `<releaseName>-<version>` (e.g. `Taoist-<TAOIST_VERSION>`) | `<releaseName>-<ver>.zip` |

A mod reuses the ModLib DLL of its submodule: release ModLib first (in its repository) when the mod needs a new
ModLib, then update the submodule in the mod repository.

Publishing a version: bump the version → build in Release → commit (the DLLs of `debug/` included) and push → task
**Release: Tag ...**.

Nexus (`<ModLib>/scripts/setup/setup-nexus.ps1 [-RepoPath <repository>]`, task **Setup: Nexus Mods (CI)**, or the
GitHub repository → Settings → Secrets and variables → Actions); the mod page and a first file are created by hand on
Nexus first:

| Name | Kind | Value |
|---|---|---|
| `NEXUSMODS_API_KEY` | secret (in each repository) | API key of https://www.nexusmods.com/settings/api-keys |
| `NEXUS_<RELEASENAME>_FILE_ID` | variable | File id (Files tab of the mod page → "Advanced", or Manage Files). Not set: no Nexus upload |
| `NEXUS_<RELEASENAME>_URL` | variable | Mod page URL, linked in the release notes |

## VS Code tasks

`.vscode/tasks.json` of each repository (**Terminal → Run Task...**): this repository has the tasks of ModLib alone;
a mod repository has its own (build, deploy, release, tests, setup, game, viewers) for its mod only: ModLib is built and
released from this repository.
