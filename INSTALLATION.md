# GuiGuBaHuang-ModLib Installation Guide

How to set up a Windows machine to build, test and release **ModLib** (Steam Workshop `3385996759`) from this
repository.

A mod built on ModLib (e.g. [GuiGuBaHuang-Taoist](https://github.com/4azuo/GuiGuBaHuang-Taoist)) does not need this
guide: it holds this repository as its `GuiGuBaHuang-ModLib` submodule and uses the `ModLib.dll` committed here; follow
the installation guide of the mod.

---

## Before you start

| Requirement | Notes |
|---|---|
| Windows 10/11 64-bit | |
| **Gui Gu Ba Huang installed through Steam** | ModLib compiles against the game's DLLs |
| **MelonLoader installed in the game, and the game started once** | Creates `<game>\MelonLoader\Managed\Assembly-CSharp.dll`, which the build needs |
| Administrator account | Only for installing tools |

| Tool | Needed for | Required? |
|---|---|---|
| .NET Framework **4.7.2** Developer Pack | Building ModLib (it targets .NET Framework 4.7.2) | Yes |
| .NET 9 SDK | `dotnet` command (build scripts, unit tests) | Yes |
| Git + **Git LFS** | Cloning: the DLLs of `3385996759/debug`, the Steam pictures and the pictures of `docs/` are in Git LFS | Yes |
| Python 3 | `Translate` tool, auto-build watcher, viewers, game picture extraction | Recommended |
| GitHub CLI | Nexus Mods setup of the release CI | Optional |

---

## 1. Clone

```powershell
git lfs install
git clone https://github.com/4azuo/GuiGuBaHuang-ModLib.git C:\git\GuiGuBaHuang-ModLib
cd C:\git\GuiGuBaHuang-ModLib
```

**Check:** `3385996759\debug\Mod_flQrxq\ModCode\dll\ModLib.dll` is about 200 KB (a file of about 130 bytes is a Git LFS
pointer: run `git lfs pull`).

## 2. Setup script

```powershell
# Admin terminal: installs the missing tools too (Chocolatey)
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup\install.ps1
# Normal terminal: tools already installed
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup\install.ps1 -SkipDependencies
```

It runs these steps (each also alone, and as a VS Code task **Setup: …**):

| Script (`scripts\setup\`) | What it does | Admin |
|---|---|---|
| `00-install-dependencies.ps1` | Installs the missing tools with Chocolatey | Yes |
| `02-configure-settings.ps1` | Detects the Steam Workshop folder and writes it into the settings files (`-DryRun` shows the changes only) | No |
| `04-verify-environment.ps1` | Read-only check (PASS / WARN / FAIL): tools, Git LFS, `ModLib.dll`, settings, game + MelonLoader, game paths of `ModLib\ModLib.csproj` | No |

Without the scripts: install the tools by hand, then set `steamWorkshopPath` (the Workshop folder of the game,
`...\steamapps\workshop\content\1468810`, backslashes doubled in JSON) in `scripts\settings.json` and
`3385996759\scripts\settings.json`.

## 3. Build and deploy

| VS Code task | Script | Does |
|---|---|---|
| **ModLib: Build** (`Ctrl+Shift+B`) | `3385996759\scripts\build-project.ps1` | builds `ModLib\ModLib.csproj` + the ModMain of 3385996759 (Release) into `3385996759\debug\Mod_flQrxq` |
| **ModLib: Rebuild & Deploy** | `3385996759\scripts\rebuild-and-deploy.ps1` | clean + build + copy to the Steam Workshop folder |
| **ModLib: Start Auto Build** | `3385996759\scripts\start-auto-build.ps1` | rebuilds and deploys on every change (Python + watchdog) |

The DLLs of `3385996759\debug\Mod_flQrxq\ModCode\dll` are committed: the mods build against them and the release zips
them. Commit them after a change of ModLib, then update the submodule in the mod repositories.

The game references of `ModLib\ModLib.csproj` point to `C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒\`.
If the game is elsewhere, make that path point to it with a junction (Admin terminal):

```powershell
New-Item -ItemType Junction -Path "C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒" -Target "D:\SteamLibrary\steamapps\common\鬼谷八荒"
```

## 4. Test

**Test: Unit Tests** (`scripts\run-unittests.ps1`): builds 3385996759 and runs the ModLib tests
(`tests\ModLibTests.csproj`); results in `logs\test-viewer\index.html`. The tests in game (autotest) run from a mod
repository: see [docs/06-Shared-Tools.md](docs/06-Shared-Tools.md).

## 5. Release

1. Bump `Version` in `3385996759\ModProject\ModCode\ModMain\ModMain.cs`, build (task **ModLib: Build**), commit with the
   DLLs of `debug\` and push.
2. Task **Release: Tag ModLib** (`scripts\release\create-release-tag.ps1`): tags `ModLib-<version>` and pushes it; the
   CI (`.github\workflows\release.yml`) zips `3385996759\debug\Mod_flQrxq` and publishes the GitHub Release.
3. Nexus Mods (optional, once): create the mod page and upload a first file by hand on Nexus, then task
   **Setup: Nexus Mods (CI)** (`scripts\setup\setup-nexus.ps1`, needs `gh auth login`): secret `NEXUSMODS_API_KEY`,
   variables `NEXUS_MODLIB_FILE_ID` and `NEXUS_MODLIB_URL` of this repository. From then on every release uploads a new
   version of that file.

The Steam Workshop item is updated from the game's mod uploader with the deployed folder (`ModLib: Rebuild & Deploy`).

---

## Troubleshooting

- **"running scripts is disabled"**: start scripts with `powershell -NoProfile -ExecutionPolicy Bypass -File <script>`.
- **A tool is "not recognized" right after installing it**: open a new terminal (and restart VS Code).
- **MSB3644 "reference assemblies for .NETFramework,Version=v4.7.2 were not found"**: install the 4.7.2 **Developer
  Pack** (4.8 is not enough).
- **Errors about `Assembly-CSharp`, `MelonLoader`, `UnityEngine`**: install MelonLoader, start the game once; check the
  game path (junction above).
- **Many "does not contain a definition" errors after a game update**: `MelonLoader\Managed` is stale; let MelonLoader
  regenerate it: `powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\game\run-game.ps1 -UntilModLoaded -StopGame -Duration 900`
  (`04-verify-environment.ps1` reports it as "MelonLoader assemblies match game version" = FAIL).
- **The game path is not ASCII (`鬼谷八荒`) and MelonLoader does not start**: start the game with
  `scripts\game\run-game.ps1`, it goes through an ASCII junction.

Support: [issues](https://github.com/4azuo/GuiGuBaHuang-Taoist/issues).
