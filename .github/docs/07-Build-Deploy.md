# Build and Deploy

## Build Scripts

Located in `3161035078/.vscode/`:

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
