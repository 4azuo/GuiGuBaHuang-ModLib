# GuiGuBaHuang-ModLib Installation Guide

How to set up a Windows machine to build ModLib (3385996759), the Taoist mod (3161035078) and ModCreator.

There are two ways:

- **[Option A – Automatic](#option-a--automatic-install)**: one script does everything (about 5–15 minutes).
- **[Option B – Manual](#option-b--manual-install)**: every step by hand, no scripts needed.

Both end with the same result. If anything goes wrong, see [Troubleshooting](#troubleshooting).

---

## Before you start

| Requirement | Notes |
|---|---|
| Windows 10/11 64-bit | |
| Internet connection | Downloads tools and the repository |
| ~2 GB free disk | Tools + repository |
| **Gui Gu Ba Huang installed through Steam** | The projects compile against the game's DLLs |
| **MelonLoader installed in the game, and the game started once** | Creates `<game>\MelonLoader\Managed\Assembly-CSharp.dll`, which the build needs |
| Administrator account | Only for installing tools (Option A step 1 / Option B step 1) |

**What gets installed**

| Tool | Needed for | Required? |
|---|---|---|
| .NET Framework **4.7.2** Developer Pack | Building ModLib and the mods (they target .NET Framework 4.7.2) | Yes |
| .NET 9 SDK | `dotnet` command (build scripts) and building ModCreator | Yes |
| Git | Downloading the repository | Yes |
| Python 3 | Auto-build watcher, `Translate` tool | Recommended |
| 7-Zip (or WinRAR) | Extracting `BaseGameResources` | Recommended |
| GitHub CLI | Starring the repo, pull requests | Optional |
| Notepad++ | Editing files | Optional |

---

## Option A – Automatic install

### A1. New machine (repository not downloaded yet)

1. Create a temporary folder, e.g. `C:\temp\modlib-install`.
2. Download **only** [`install.ps1`](https://raw.githubusercontent.com/4azuo/GuiGuBaHuang-ModLib/main/scripts/setup/install.ps1) into it
   (right-click the link → *Save link as…*). The other setup scripts are downloaded automatically.
3. Open **PowerShell as Administrator**: press `Win + X` → **Terminal (Admin)**.
4. Run:

   ```powershell
   cd C:\temp\modlib-install
   powershell -NoProfile -ExecutionPolicy Bypass -File .\install.ps1
   ```

   The repository is cloned to `C:\git\GuiGuBaHuang-ModLib`. To put it somewhere else:

   ```powershell
   powershell -NoProfile -ExecutionPolicy Bypass -File .\install.ps1 -RepoPath "D:\Projects\GuiGuBaHuang-ModLib"
   ```

5. Answer the questions (press **Enter** to accept the suggested value). At the settings step, the script shows the paths it detected (repository, Steam Workshop, game folder, BaseGameResources) and lets you change any of them.
6. At the end you get a **PASS / WARN / FAIL** report. Fix every `FAIL` (each line shows a `fix:` hint), then open a **new** terminal.

### A2. Repository already cloned

Run the installer from inside the repository; it uses that folder and skips cloning:

```powershell
cd <your repo folder>
# Admin terminal: installs missing tools too
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup\install.ps1
# Normal terminal: tools already installed, only set up the repository
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup\install.ps1 -SkipDependencies
```

### install.ps1 parameters

| Parameter | Meaning |
|---|---|
| `-RepoPath <folder>` | Where the repository is (or should be cloned). Default: the repo containing the script, otherwise `C:\git\GuiGuBaHuang-ModLib` |
| `-Yes` | Don't ask anything; accept every detected/default value (unattended install) |
| `-ArchivePath <file>` | Path of the `BaseGameResources *.rar/.7z/.zip` archive, if it is not in a standard place |
| `-SkipDependencies` | Don't install tools (no Administrator needed) |
| `-SkipCheckpoint` | Don't offer to create a System Restore Point |
| `-SkipStar` | Don't offer to star the repository on GitHub |
| `-SkipExtract` / `-SkipConfigure` / `-SkipVerify` | Skip that step |
| `-OpenAll` | After writing the settings, open each settings file in Notepad for review |

Example, fully unattended into a custom folder:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\install.ps1 -RepoPath D:\dev\GuiGuBaHuang-ModLib -Yes -SkipCheckpoint
```

### Running a single step

All scripts live in `scripts\setup\`, can be run again at any time (finished work is detected and skipped), and accept `-Yes` to run without questions. Every step is also available as a VS Code task (**Terminal → Run Task → Setup: …**). For "Install Dependencies", VS Code must run as Administrator.

| Script | What it does | Admin | Useful parameters |
|---|---|---|---|
| `00-install-dependencies.ps1` | Installs missing tools with Chocolatey | Yes | `-SkipPython`, `-SkipGitHubCLI`, `-SkipNotepadPlusPlus`, `-Skip7Zip`, … |
| `01-clone-repository.ps1` | Clones the repo (skipped if it already exists) and makes sure `GuiGuBaHuang-ModCreator` is checked out | No | `-RepoPath`, `-Branch`, `-SkipStar`, `-Force` |
| `03-extract-resources.ps1` | Finds and extracts the BaseGameResources archive | No | `-ArchivePath`, `-Destination`, `-Force` |
| `02-configure-settings.ps1` | Detects paths and writes them into every `settings.json` | No | `-GamePath`, `-WorkshopPath`, `-BaseGameResourcesDir`, `-DryRun`, `-SkipWorktree`, `-OpenFiles` |
| `04-verify-environment.ps1` | Read-only health check (PASS / WARN / FAIL) | No | `-RepoPath` |

Example – see what the configure step would change without writing anything:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup\02-configure-settings.ps1 -DryRun
```

---

## Option B – Manual install

Do the steps in order. Each step ends with a **Check**. Don't go on until it passes.

### Step 1 – Install the tools

Download and install each one (default options are fine unless noted):

| Tool | Download | Note |
|---|---|---|
| .NET Framework 4.7.2 Developer Pack | https://dotnet.microsoft.com/download/dotnet-framework/net472 → **Developer Pack** | Must be **4.7.2**. The 4.8 Developer Pack or the 4.7.2 *Runtime* do **not** include the 4.7.2 reference assemblies |
| .NET 9 SDK | https://dotnet.microsoft.com/download/dotnet/9.0 → **SDK x64** | The SDK, not only the runtime |
| Git | https://git-scm.com/download/win | |
| Python 3 | https://www.python.org/downloads/ | Tick **"Add python.exe to PATH"** on the first installer screen |
| 7-Zip | https://www.7-zip.org/ | Or WinRAR |
| GitHub CLI (optional) | https://cli.github.com/ | |

Alternative with [Chocolatey](https://chocolatey.org/install) in an Admin terminal:

```powershell
choco install netfx-4.7.2-devpack dotnet-9.0-sdk git python 7zip gh -y
```

**Check:** close the terminal and open a **new** one, then:

```powershell
dotnet --list-sdks          # a line starting with 9.
git --version
python --version            # "Python 3.x.x"
Test-Path "${env:ProgramFiles(x86)}\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll"   # True
```

### Step 2 – Download the repository

Pick a folder (examples use `C:\git`) and clone:

```powershell
mkdir C:\git -Force
cd C:\git
git clone --recurse-submodules https://github.com/4azuo/GuiGuBaHuang-ModLib.git
cd GuiGuBaHuang-ModLib
```

`GuiGuBaHuang-ModCreator` is a separate repository. If the folder is empty after cloning (the submodule entry in `.gitmodules` may be missing), fetch it yourself at the commit this repository expects:

```powershell
git ls-tree HEAD GuiGuBaHuang-ModCreator          # prints: 160000 commit <SHA>  GuiGuBaHuang-ModCreator
git clone https://github.com/4azuo/GuiGuBaHuang-ModCreator.git GuiGuBaHuang-ModCreator
git -C GuiGuBaHuang-ModCreator checkout <SHA>     # the SHA from the first command
```

**Check:** `GuiGuBaHuang-ModCreator\ModCreator.sln` exists.

### Step 3 – BaseGameResources (only for ModCreator)

ModCreator's base-game config browser needs the archive `BaseGameResources <version>.rar` (e.g. `BaseGameResources 1.2.111.rar`). It is **not** in git (`*.rar` is ignored), so get it from the project author.

Extract it so the files end up in `GuiGuBaHuang-ModCreator\BaseGameResources <version>\`. With 7-Zip: right-click the archive → **7-Zip → Extract to "BaseGameResources <version>\"**.

You can skip this step. ModLib and the mods build without it.

**Check:** `GuiGuBaHuang-ModCreator\BaseGameResources <version>\` contains files directly (not one more nested folder).

### Step 4 – Configure the settings files

Find your paths first:

- **Game folder**: in Steam, right-click the game → **Manage → Browse local files**. Copy the folder path from Explorer's address bar.
  Usually `C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒`.
- **Workshop folder**: in the same Steam library, `...\steamapps\workshop\content\1468810` (1468810 is the game's Steam app id).
  It may not exist until you subscribe to a Workshop item. That is fine, deploy scripts create it.
- **Repository folder**: where you cloned in step 2, e.g. `C:\git\GuiGuBaHuang-ModLib`.

Open each file below in a text editor (Notepad++, VS Code) and set the values. In JSON, **every backslash must be doubled**: `C:\\git\\GuiGuBaHuang-ModLib`.

**`scripts\settings.json`** and **`3161035078\scripts\settings.json`** and **`3385996759\scripts\settings.json`**

| Key | Value |
|---|---|
| `repositoryPath` | Repository folder |
| `steamWorkshopPath` | Workshop folder |
| `projects` *(root file only)* | Project ids built by the "all" scripts. Keep `3385996759` first (ModLib must build before the mods) |
| `projectId`, `modName`, `autoBuild…` *(project files)* | Leave as they are |

**`GuiGuBaHuang-ModCreator\ModCreator\Resources\settings.json`**

| Key | Value |
|---|---|
| `rootDir` | Repository folder |
| `modCreatorDir` | `<repository>\GuiGuBaHuang-ModCreator` |
| `baseGameResourcesDir` | Folder extracted in step 3 (leave as is if you skipped it) |
| `translateScriptPath` | `<repository>\Translate\run.py` |
| `steamWorkshopDir` | Workshop folder |
| `gameFolderPath` | Game folder |

Example (`3385996759\scripts\settings.json`):

```json
{
    "projectId": "3385996759",
    "modName": "Mod_flQrxq",
    "steamWorkshopPath": "C:\\Program Files (x86)\\Steam\\steamapps\\workshop\\content\\1468810",
    "repositoryPath": "D:\\Projects\\GuiGuBaHuang-ModLib",
    "autoBuildDebounceSeconds": 300,
    "rapidChangeDebounceSeconds": 5
}
```

> These files are tracked by git. If your paths differ from the committed ones, the files show up as changed.
> To keep your local paths out of commits:
> `git update-index --skip-worktree scripts/settings.json 3161035078/scripts/settings.json 3385996759/scripts/settings.json`
> (undo with `--no-skip-worktree`). Remember to undo it before you intentionally change other keys in those files.

**Check:** each file still opens as valid JSON, e.g. `Get-Content scripts\settings.json -Raw | ConvertFrom-Json` prints the values without an error.

### Step 5 – Verify

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup\04-verify-environment.ps1
```

Without the script, check by hand:

- [ ] Step 1 checks pass
- [ ] `GuiGuBaHuang-ModCreator\ModCreator.sln` exists
- [ ] `repositoryPath` in the 3 settings files is your repository folder
- [ ] `<game folder>\MelonLoader\Managed\Assembly-CSharp.dll` exists
- [ ] The game is at `C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒`, or you created the junction described in [Game installed in another folder](#game-installed-in-another-folder)

---

## First build

Always build ModLib first. The other mods reference `ModLib.dll` from `3385996759\CodeArtifacts\`.

```powershell
cd <repository>
powershell -NoProfile -ExecutionPolicy Bypass -File .\3385996759\scripts\build-project.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File .\3161035078\scripts\build-project.ps1
# ModCreator (WPF tool)
powershell -NoProfile -ExecutionPolicy Bypass -File .\GuiGuBaHuang-ModCreator\build.ps1
```

Output goes to `<ProjectId>\debug\Mod_xxxxx\`. To build **and** copy everything to the Steam Workshop folder: `.\scripts\rebuild-and-deploy-all.ps1`.
In VS Code: **Terminal → Run Task…** (`Ctrl+Shift+B` = "All: Rebuild & Deploy"). More in [docs/07-Build-Deploy.md](docs/07-Build-Deploy.md).

---

## Troubleshooting

### "running scripts is disabled on this system"
Start scripts through `powershell -NoProfile -ExecutionPolicy Bypass -File <script>` (as shown everywhere in this guide), or allow scripts for the current window only: `Set-ExecutionPolicy -Scope Process Bypass`.

### "This script needs Administrator privileges"
Only `00-install-dependencies.ps1` (and `install.ps1` without `-SkipDependencies`) need it. Open `Win + X` → **Terminal (Admin)**. You can also install the tools by hand ([Option B step 1](#step-1--install-the-tools)) and then run `install.ps1 -SkipDependencies`.

### A tool is "not recognized" right after installing it
PATH only refreshes in new terminals. Close every terminal (and VS Code) and open again.

### `python` opens the Microsoft Store / "Python was not found"
Windows ships a fake `python` alias. Install Python from python.org with **Add to PATH**, or turn off the alias in **Settings → Apps → Advanced app settings → App execution aliases** (python.exe / python3.exe).

### `GuiGuBaHuang-ModCreator` folder is empty / "no submodule mapping found in .gitmodules"
Run `.\scripts\setup\01-clone-repository.ps1 -RepoPath <repository>`, or follow the manual commands in [step 2](#step-2--download-the-repository).

### Build error MSB3644: "reference assemblies for .NETFramework,Version=v4.7.2 were not found"
The 4.7.2 **Developer Pack** is missing. Having .NET Framework 4.8 installed is not enough. Install `netfx-4.7.2-devpack` ([step 1](#step-1--install-the-tools)).

### Build errors about `Assembly-CSharp`, `MelonLoader`, `UnityEngine`… (CS0246 / missing references)
- Install MelonLoader in the game and **start the game once**, so `MelonLoader\Managed\` is generated.
- Check the game location (next section).

### Many "does not contain a definition" / "type could not be found" errors after a game update
`<game>\MelonLoader\Managed\*.dll` (what the projects compile against) is generated by MelonLoader from the game. After a game update it is stale until MelonLoader regenerates it, so types and members that exist in the game look missing, even though the code is fine.
`04-verify-environment.ps1` detects this ("MelonLoader assemblies match game version" = FAIL).
Fix: let MelonLoader run once so it regenerates them (2–5 minutes), then build again:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\game\run-game.ps1 -UntilModLoaded -StopGame -Duration 900
```

Look for `Assembly Generation Successful!` in the output and `"assembliesRegenerated": true` in the run's `summary.json`.

### MelonLoader never starts (game runs, but code mods don't load)
- **`version.dll` missing in the game folder.** It is MelonLoader's loader. Without it the game still runs and still loads mod configs, but no mod DLL runs. Restore it with Steam → game → Properties → Installed Files → *Verify integrity of game files*.
- **Game path contains non-ASCII characters.** Steam installs to `...\common\鬼谷八荒`. MelonLoader's `version.dll` stops the game in about 2 seconds (exit code 0, no log written) when the game path is not ASCII. The real condition is that the path must be representable in the Windows ANSI code page. It works on Chinese/Japanese Windows and fails on English/European/Vietnamese locales. `scripts\game\run-game.ps1` works around it: it creates a directory junction with an ASCII name (`%LOCALAPPDATA%\ggbh-game-link`) and starts the game through it. Keep that path short, because MelonLoader's assembly generator fails when paths exceed 260 characters.
  To start from Steam too, set Steam → game → Properties → General → **Launch Options** to
  `"C:\Users\<you>\AppData\Local\ggbh-game-link\guigubahuang.exe" %command%`
  (run `run-game.ps1` once first so the junction exists). Alternatively, change the Windows system locale to Chinese/Japanese (*Change system locale*, reboot). See also the README, section "Known issues".

### Game installed in another folder
The `.csproj` files reference the game DLLs at the fixed path `C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒\MelonLoader\...`. If your game is elsewhere (another Steam library or drive), make that path point to your real folder with a directory junction (Admin terminal, the path must not already exist):

```powershell
New-Item -ItemType Junction `
  -Path   "C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒" `
  -Target "D:\SteamLibrary\steamapps\common\鬼谷八荒"
```

`04-verify-environment.ps1` reports this as **".csproj game references – N DLLs missing"** and shows the path it expects.

### Warning "ModLib.csproj DocumentationFile … does not exist"
`ModLib.csproj` writes `ModLib.xml` (IntelliSense docs for ModCreator) to `C:\git\GuiGuBaHuang-ModLib\…`. If your repository is elsewhere, that file just isn't refreshed. The build still works.

### Settings files show as modified in git
They now hold your local paths. See the skip-worktree note in [step 4](#step-4--configure-the-settings-files), or use `02-configure-settings.ps1 -SkipWorktree`.

### BaseGameResources archive not found
It isn't in the repository. Put `BaseGameResources <version>.rar` in `GuiGuBaHuang-ModCreator\`, your `Downloads` or your `Desktop` folder and run `03-extract-resources.ps1` again, or pass `-ArchivePath <file>`.

### Creating a restore point fails
Optional. System Protection may be disabled, or a restore point was created within the last 24 hours. Continue without it, or use `-SkipCheckpoint`.

---

## What the setup scripts change

| Script | Changes |
|---|---|
| `00-install-dependencies.ps1` | Installs Chocolatey (if missing) and the missing tools system-wide |
| `01-clone-repository.ps1` | Creates the repository folder; clones ModCreator into `GuiGuBaHuang-ModCreator\` |
| `03-extract-resources.ps1` | Creates `GuiGuBaHuang-ModCreator\BaseGameResources <version>\` |
| `02-configure-settings.ps1` | Replaces only the path values listed in [step 4](#step-4--configure-the-settings-files). Formatting and other keys stay as they are |
| `04-verify-environment.ps1` | Nothing (read-only) |

---

## Support

- **Repository:** https://github.com/4azuo/GuiGuBaHuang-ModLib
- **Issues:** https://github.com/4azuo/GuiGuBaHuang-ModLib/issues
- **Author:** 4azuo
