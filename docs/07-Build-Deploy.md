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

## VS Code Tasks

All scripts above are available in `.vscode/tasks.json`: **Terminal → Run Task...** (or `Ctrl+Shift+B` for the default build task "All: Rebuild & Deploy").
