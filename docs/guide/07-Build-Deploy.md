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
| `build-project.ps1` | compiles the project (Release), copies `CodeArtifacts` and the DLLs to `debug/Mod_xxxxx/ModCode/dll/`, then ModConf/ModImg/SteamImg to `debug/Mod_xxxxx/`; `-ResourcesOnly`: only the copy (the CI); then sets the version of the mod editor's project (`ModProject/ModProject.cache` `"ver"` and the update note `"curUpdateDesc"`: `v<version>`) to the version of the source code (`versionFile` / `versionPattern` of `settings.json`, as the release tag; not with `-ResourcesOnly`) |
| `rebuild-project.ps1` | clean + build |
| `start-auto-build.ps1` | watches ModCode/ModConf/ModImg and rebuilds + deploys on change (Python + watchdog); errors in `scripts/auto-built-errors.log` |
| `copy-to-steam.ps1` / `copy-from-steam.ps1` | copies the project to / from its Steam Workshop folder `<workshop>/<project id>`: the content of `debug/Mod_<id>` (the build output the game loads: `ModCode/dll`, `ModConf`, `ModImg`, `ModExportData.cache`...) and, in `ModProject/`, the 3 project files of the mod editor (`ModProject.cache`: name, author, description, version, tags, Workshop item; `ModData.cache`: the content made with the editor's tables; `ModProjectPreview.png`); no source, scripts or tests (`_workshop.ps1`): the Workshop item stays small |
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
| `copy-both-to-steam.ps1` / `copy-both-from-steam.ps1` | copies the dependencies + projects to Steam / the projects back (as `copy-to-steam` / `copy-from-steam`) |
| `compress-mods.ps1` | runs `compress.ps1` of every project |
| `update-repo.ps1` | pulls and updates the submodules (`-Force` stashes first, `-NoSubmodules`) |
| `open-folder.ps1` | opens (or `-Print`s) a game / Workshop / build / log folder |
| `run-unittests.ps1`, `run-all-tests.ps1` | unit tests, unit tests + autotest (see [06-Shared-Tools](../06-Shared-Tools.md)) |
| `setup/` | setup of a machine: `install.ps1` (ModLib alone), `00-install-dependencies`, `02-configure-settings`, `04-verify-environment`, `setup-nexus` (a mod repository has its own `install.ps1` that clones it with the submodule, then runs these steps) |

## Private mods

A mod that must not be uploaded from the game's mod editor (personal, test or in-house mod, or a mod released by its repository) sets a constant in its entry class:

```csharp
namespace MOD_<id>
{
    public class ModMain : ModChild
    {
        /// <summary>No Steam Workshop upload from the game's mod editor (ModLib PrivateModHelper).</summary>
        public const bool PRIVATE_MOD = true;
    }
}
```

In the mod editor (title screen → Mod → project), ModLib (`PrivateModHelper`, `ModUIEvent`) hides the **Workshop** button of that project and closes the upload window if it opens anyway, with a message. The constant is read from the loaded assembly `MOD_<id>`, else from its DLL (the installed mod, then `debug/Mod_<id>/ModCode/dll` of the project): a constant is metadata, no code of the mod runs. No constant or `false`: public. Rebuild the mod after changing it.

It guards against an upload by mistake, not against a copy: the DLL can be changed by anyone. What is shipped holds no source code: the release zip (CI) is `debug/<mod>`, and `copy-to-steam` puts only the content of `debug/<mod>` and the 3 project files of `ModProject` in the Workshop folder (the editor uploads that whole folder). Only the editor's upload is blocked: the deploy scripts of the repository copy files, they upload nothing.

## Support page (Ko-fi...)

A mod gives its support page with constants of its entry class (ModLib 2.3.07+):

```csharp
public const string SUPPORT_URL = "https://ko-fi.com/me";   // the page
public const string SUPPORT_QR = "kofi-qr.png";             // optional: image of ModImg of the mod: QR code of the page
public const string SUPPORT_NAME = "My Mod";                // optional: name shown (else LICENSE_NAME, else the mod id)
public const string SUPPORT_INFO = "mymod_support_info";    // optional (2.3.08+): localText key: a few words about the mod / what the support gives
```

`SupportHelper` reads them when ModLib lists the objects of the mods (`CacheHelper.GetModChildCacheTypes`, locked mods too); ModLib gives its own page too (its entry class `MOD_flQrxq.ModMain`). When at least one mod gives a page, the title screen gets one **Support** button in its bottom-right corner, under the column of the game's buttons (`ModTitleEvent` → `SupportHelper.AddTitleButton` → `TitleModHelper.AddCornerButton`); its window shows **one QR code at a time** (side by side they are hard to scan: the phone reads the wrong one): the mods on the left (only when several) in the order of the mod list of the game, the name, the words of `SUPPORT_INFO`, QR code, address and a button to the page of the chosen one on the right. Nothing opens by itself. A licensed mod without `LICENSE_SUPPORT_URL` / `LICENSE_QR` shows `SUPPORT_URL` / `SUPPORT_QR` in its key window. Each mod keeps its own QR image in its own ModImg.

## Licensed mods (supporter keys)

A mod that runs only with a key its author signed (Ko-fi supporters...) sets constants in its entry class (ModLib 2.3.05+):

```csharp
public const string LICENSE_PUBLIC_KEY = "<base64url RSA-2048 modulus>"; // the "n" of the JWK of an RSA-2048 key pair (exponent 65537)
public const string LICENSE_NAME = "My Mod";                             // optional: name on the title screen
public const string LICENSE_URL = "https://.../claim";                   // optional: "Get a key" of the key window
public const string LICENSE_SUPPORT_URL = "https://ko-fi.com/me";         // optional: "Support" of the key window
public const string LICENSE_INFO = "mymod_license_info";                // optional: localText key of the mod: what support gets a key
public const string LICENSE_QR = "kofi-qr.png";                         // optional: image of ModImg: QR code of the support page
public const string LICENSE_RENEW_URL = "https://.../renew";            // optional: renewal of the key, share detection (server of the mod)
```

`LicenseHelper` checks the key file `mod/<id>/globals/license.key` (game data, outside the Workshop folder) when ModLib lists the objects of the mods (`CacheHelper.GetModChildCacheTypes`): the RSASSA-PKCS1-v1_5 SHA-256 signature (BigInteger only, offline), the mod id and the expiry of the payload. Without a valid key **nothing of the mod is loaded** (no ModChild, no event, no ModSkill (`ModSkillEvent`), so no Harmony patch made by the mod; of its ModConf only the `*localText.json` files: `OnInitConf` and `ModTitleEvent.LoadLocalTexts` use `LicenseHelper.IsModUnlocked`) and the title screen gets a button "<name>: enter key" (`ModTitleEvent` → `LicenseHelper.AddTitleButtons`; the window also opens by itself the first time the title screen shows in a game start): the state of the key, `LICENSE_INFO`, an input to paste the key (Ctrl+V or Paste), buttons Get a key (`LICENSE_URL`) and Support (`LICENSE_SUPPORT_URL`), the QR code `LICENSE_QR` on the right (with the support address under it); Save keeps a valid key and asks for a restart. The texts (localText) and ModImg of a locked mod are loaded as usual: the key window shows them; its game data (items, skills... of ModConf) are not. A key: `<payload>.<signature>` (base64url), payload `{"m": mod id, "n": supporter, "e": e-mail, "i": payment id, "t": tier, "iat", "exp" (0 = never)}`. Renewal (`LICENSE_RENEW_URL`): a key ending within `LicenseHelper.RENEW_DAYS` (7) is sent to the server of the mod in the background (the new one counts from the next start), an ended one at once (`RENEW_TIMEOUT_MS` at most, so the mod still loads); the answer is kept only when it is valid for the mod and ends later (`IsBetterKey`). `LicenseHelper.Get(modId)` gives the key of a mod (supporter, e-mail, expiry) to show it. The same call is made at each start in the background (`renew: false` outside the renewal days) for the share detection of the server: `GetShare(modId)`, and a small notice on the title screen (`ShowShareNotices`, `ModTitleEvent`) when the key was used from too many networks.

**How keys are issued is up to each mod** (its own key pair, server, payment platform, rules); ModLib only checks them. What a mod must provide:

- **Signing**: RSASSA-PKCS1-v1_5 with SHA-256 ("RS256", e.g. Web Crypto `RSASSA-PKCS1-v1_5` / `SHA-256`) of the ASCII text of the base64url payload, with an RSA-2048 key whose exponent is 65537; the private key stays on the server of the mod, never in the repository.
- **Payload**: JSON with `m` = the mod id (soleID), `exp` = unix seconds (0: never ends); `n` (supporter), `e` (e-mail: renewal, shown by the mod), `i`, `t`, `iat` are shown or logged.
- **Renewal** (optional, `LICENSE_RENEW_URL`): `POST` JSON `{"key": <current key>, "renew": true|false}` (at each start, `renew` true within the renewal days or when the key ended) -> JSON `{"key": <newer key or null>, "ips": n, "warn": bool, "blocked": bool}` (`ips`/`warn`/`blocked`: optional share detection, `warn` shows the notice `libtxt999990062`, `blocked` `libtxt999990063`). The answered key is checked like any key and kept only when it ends later; `{"revoked": true}` removes the key (`LicenseHelper.Revoke`: locked from the next start, notice `libtxt999990064`). The `Date` header of the answer is kept as a time seen.
- **Clock**: the end of a key is checked against the latest time seen (`LicenseHelper.Now` / `EffectiveNow`: the clock, the file `license.time` next to the key, the `Date` of the server, the issue time `iat` of the key): a clock set back does not bring an ended key back.

Dao of Creation does it with a Cloudflare Worker fed by the Ko-fi webhook (in its own repository: `license-worker/`). An older ModLib (before 2.3.05) knows no key and loads the whole mod: the mod checks its key itself too (Dao of Creation: `ModMain.CheckLicense` calls `LicenseHelper.IsUnlocked` by reflection, not active without it; its events refuse to load in `OnCacheHandler`). Like `PRIVATE_MOD`, it keeps the honest players honest: the check can be removed from a DLL.

## Release

Each repository releases its own projects (`<ModLib>/scripts/release/`):

- `create-release-tag.ps1 [-Target <releaseName>] [-RepoRoot <repository>]` - creates the tag `<releaseName>-<version>`
  on HEAD (version read from the source code: `versionFile` / `versionPattern` of `<ProjectId>/scripts/settings.json`)
  and pushes it, which starts the CI of the repository. Checks: tag not existing yet (local + origin), uncommitted
  changes of the mod (asks), HEAD pushed (offers to push the branch); `-Yes` skips the questions, `-NoPush` only tags.
- `package-release.ps1` - zips `<ProjectId>/debug/<modName>` into `dist/<releaseName>-<version>.zip` +
  `release-notes.md` (downloads, install, `releaseRequires` of the project settings such as "needs ModLib 2.2.40 or newer" with a link to that release of the submodule, SHA-256). `create-release-tag.ps1` stops when that release does not exist yet: release ModLib first.
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
