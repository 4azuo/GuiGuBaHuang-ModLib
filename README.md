# About my mod

※Pls, give a like to let me know it useful, tks.

※If you want to support the work: [Ko-fi](https://ko-fi.com/fouru)

※Download: Steam Workshop (automatic updates), or the zips of the [latest GitHub release](https://github.com/4azuo/GuiGuBaHuang-ModLib/releases/latest) for a manual install (copy the `Mod_xxx` folder of the zip into the `ModExportData` folder of the game).

※Error reports: ModLib can send the errors of a mod to its author (BugSplat) **only if you allow it** on the title screen (change it in ModCreator → Error reports). Sent: the error and its stack trace, the last lines of the mod log (your user folder, Windows account and player id removed), versions, loaded mod ids, language, an anonymous id. Never your saves or Steam account. Mod authors: set your own BugSplat database in `ModConf/crash_reports.json` (see [Error reports (BugSplat)](#error-reports-bugsplat)).

※If you have any problem about creating project. Feel free for contacting to me.

※If you wanna contribute to my project. Please contact to me.





[Fouru's ModLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3385996759)

※This a library for many mods.
- Sync from children mods.
- Data flow between children mods.
- Mod data wont be saved in save file.
- Use json to cache everythings (no encrypt)
- Debug and log
- Support many minor libraries: UI, Battle damage, ...
- Create and replace game configs
- Translation system (replace LocalText.json)
- Async and Delay functions
- Support Parameter Store for each ModChild
- etc...

[Taoist](https://steamcommunity.com/sharedfiles/filedetails/?id=3161035078)

Hardcore rework of the base game. Most features can be tuned or turned off in **S&M Configs** (title screen; page 3 holds the optional features).

**New game**
- Starting divine artifacts at character creation (Bagua Jade, Eye of Providence, Devil Demon pot; any combination) and "skip the intro and the first battle"
- Roles when creating a character/NPC: Power, Speed, Tao, Atk, Def, Angel, Evil, Merchant (each with its own advantages)
- Prioritized destiny level, S&M difficulty presets, one-life mode, in-game trainer

**Divine artifacts**
- Several artifacts at once: their buttons are laid out side by side on the map and in battles
- A hotkey per artifact (Taoist Settings, same binding rows as the game): opens the artifact window on the map, uses its skill in a battle
- Repair a damaged artifact

**World and NPCs**
- NPCs grow, level up skills, craft artifacts, get roles and titles
- Teams: hire NPCs (monthly pay), friends join for free, members who fight beside you grow closer; members may betray you
- NPCs join your battles (enemies, friends, sect members, town guards); stalkers follow you after you show rare items — fight them or pay them off
- Random encounters with choices and rewards: wounded cultivator, chess hermit, hooded peddler, collapsed cave, spirit beast cub, town festival
- Reworked dramas (several lines per situation, own choices) in English, Chinese (simplified/traditional) and Korean
- Quests: slice-of-life, challenge master, commissions (ask NPCs to find items)
- Rebirth to level 1 to cultivate again with better basic stats

**Economy**
- Prices rise over time (inflation, currency reform), sale/price events in June and December
- Town budgets and taxes (tax statistics: income, residents who could not pay and left)
- Bank accounts and storage fees (unpaid fees lock the storage)
- Bank loans: 3 %/month, due in 12 months; overdue debts bring reminder letters, then debt collectors (pay / hand over goods / fight); beat them twice and the bank writes the debt off but blacklists you (no loans, account closed, triple taxes in that town)
- Player market with NPCs: list items, negotiate (spirit stones and barter), VIP market, recent deal prices, offer notifications, buy orders filled by the townspeople
- Exchange items for sect contribution

**Towns**
- Become town master/guardian, manage the budget, tax rate, buildings, upgrade to a city
- Town wars and monster waves; when your town is attacked far away you are called back to defend it
- Bounties: town masters post wanted notices paid from the town budget; hunters chase the targets (you included — fight or clear your name); hunt the wanted yourself from the bounty board

**World and family**
- Families: NPCs take dao companions (same-sex option), have children who join the world at 16 (surname of the father, the mother or random); family window in the NPC info; a family avenges its members
- Play on as your grown-up child or grandchild when your character dies (S&M option): the legacy (items, spirit stones, techniques) is kept
- Seclusion and enlightenment lasting years (spirit veins, ancient inheritances, heart demons, time rifts...) while the world keeps living; a forced one never ends your life
- NPCs find their own fortunes and ancient inheritances (rarely)
- Time skips run every month of the world and save only 4 times, with a progress window

**Battle and growth**
- Mana shields, special monsters, monsters growing stronger, reworked damage/defense, block/evade/crit rules
- Exp from battles, stats grow through battles, harder level-ups, expertise of skills and artifacts
- Losing a battle can cost exp, items, spirit stones, lifespan or the character (one-life) — Resurrection Pill and rings protect
- Custom refine of rings/outfits/artifacts/mounts: refine levels, random adjustment slots with qualities (slot n unlocks at level n x 100), progress shown at the forge, reroll unlocked slots
- Qi cultivation, cultivation with spirit stones, natural breakthroughs

**Interface**
- Taoist Settings (game settings): hide damage multiplier, notifications, encounters, artifact hotkeys, the **Taoist log** (taxes, fees, deals, loans, bounties, encounters... month by month)
- Hide map / unit number in battle, hide training and skill previews
- Languages: English, Chinese, Vietnamese (+ custom language system)




# How to create a mod

![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/91921f05-251a-4f1e-a2e9-d1e5bdd853d3)





# Configuration

**Add ModLib.dll to your project**

- Download ModLib, build and reference .dll to your project.

![image](https://github.com/user-attachments/assets/9554b89d-61c6-4ef5-9bad-97c3c62107ab)

**Add ModMain-BuildEvent**

![image](https://github.com/user-attachments/assets/c9fec3e0-7dfd-4e8c-8c0e-d740ce400e35)

- `rd "$(ProjectDir)obj" /s /q`
- `rd "$(ProjectDir)\..\..\..\debug\Mod_nE7UL2\ModCode\dll" /s /q`
- `rd "$(ProjectDir)\..\..\..\debug\Mod_nE7UL2\ModConf" /s /q`
- `rd "$(ProjectDir)\..\..\..\debug\Mod_nE7UL2\ModImg" /s /q`
- `xcopy "$(ProjectDir)\..\..\..\..\3385996759\CodeArtifacts\*.dll" "$(ProjectDir)\..\..\..\debug\Mod_nE7UL2\ModCode\dll\" /y /i`
- `xcopy "$(ProjectDir)\bin\Release\*.dll" "$(ProjectDir)\..\..\..\debug\Mod_nE7UL2\ModCode\dll\" /y /i`
- `xcopy "$(ProjectDir)\..\..\ModConf\*.*" "$(ProjectDir)\..\..\..\debug\Mod_nE7UL2\ModConf\" /y /i /e /h /c`
- `xcopy "$(ProjectDir)\..\..\ModImg\*.*" "$(ProjectDir)\..\..\..\debug\Mod_nE7UL2\ModImg\" /y /i /e /h /c`

Add BuildEvent for copying dll to "debug" folder. 

※"nE7UL2" is my mod-id. You should change "nE7UL2" to your mod-id. And build by "Release" mode.

※You should change MelonLoader folder path in `ModLib.csproj` and `ModMain.csproj` to use game dll.





# How to use

![image](https://github.com/user-attachments/assets/34a75a28-38df-4922-aa43-732b8ab3b035)

![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/84f35501-d6f2-4b8d-9cd6-2606bf397e59)

１．Just inherit **ModChild** and config your mod. And then, you can use declared on-events.

※Cache id of ModChild must be same Mod id.

２．You should use **ModEvent**↓↓↓ to process your mod. And use **ModMain**↑↑↑ to config your mod.

![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/e8da9fd1-89d0-4870-ace4-b7153dace9f2)

![image](https://github.com/user-attachments/assets/5c1c364b-daf4-41c6-aa78-f39b2fcf9d6e)

３．`Cache` and register a event.

![image](https://github.com/user-attachments/assets/42549f20-dcd7-49b0-b624-5caae73ae1d6)

If **CacheType**=Global then, the event will be created when start game (application).

If **CacheType**=Local then, the event will be created when load save.

If **WorkOn**=Global then, the event will be deleted when load save.

Last, All properties of ModEvent will be cached (.json). You can ignore cache and reconstruct by **JsonIgnoreAttribute**

※I wrote some events which i need. You can add more to ModMaster.

４．Final, you should add your mod index to `_EventOrderIndex.json`

![image](https://github.com/user-attachments/assets/4b338668-a2a0-4a1c-8fcf-fb261e5de7e7)





# Debug/Log

You can use DebugHelper to write a log which will be saved to ↓ folder.

![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/7cf4688f-8890-41e7-bf0a-aa6519bbf325)

`xxx\AppData\LocalLow\guigugame\guigubahuang\mod\flQrxq`

※All logs are created in ModLib with Mod-Id = `flQrxq`

Also, you can use **TraceAttribute**/**TraceIgnoreAttribute** which catch calling method.





# Error reports (BugSplat)

ModLib can send the errors of your mod to your own [BugSplat](https://www.bugsplat.com) database, so you see the bugs of the players without asking them for logs. Nothing is sent before the player allows it (a Yes/No dialog on the title screen, changed later in **ModCreator → Error reports**).

**1. Create a database**

- Sign up at [bugsplat.com](https://www.bugsplat.com) (the free plan is enough for a mod).
- Create a database in the BugSplat web app (Settings → Database). Its name goes into the upload URL (`https://<database>.bugsplat.com`): use letters, digits, `-` and `_` only, and **not your e-mail or real name** (e.g. `mymod-ggbh`). Taoist uses `taoist-ggbh`.

**2. Add `ModConf/crash_reports.json` to your mod**

```json
{
	"Database": "mymod-ggbh",
	"ModName": "My mod",
	"LogLines": 200
}
```

- `Database`: the name of step 1. No file or an invalid name = the errors of your mod are never sent.
- `ModName`: the name shown to the player in the consent dialog, and the application name in BugSplat.
- `LogLines`: the last lines of the ModLib log sent with each error (0 = no log).

The build copies `ModConf` to `debug/Mod_<id>/ModConf`, so the file ships with the mod. It is not a game config (ModLib skips it when loading the configs).

**3. Set the version of your reports** (optional, else the ModLib version is used), in your `ModChild`:

```csharp
public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
{
    base.OnLoadClass(isNew, modId, attr);
    CrashReportHelper.SetModVersion(modId, "1.0.0");
}
```

**4. Log your errors with `DebugHelper`**

Every `DebugHelper.WriteLine(exception)` (also the errors ModLib catches in your events) is reported to the database of the mod found in the stack trace, so each mod gets only its own errors. The same error is sent once per game session, at most 5 per session.

**5. Check the database**

- Without the game: `$env:BUGSPLAT_TEST_DB="mymod-ggbh"; .\scripts\run-unittests.ps1` sends one test report (`CrashReportHelperTests.Live_SendToBugSplat`).
- In the game: start it, answer **Yes** in the dialog; the MelonLoader log shows `[CrashReport] sent` / `not sent` for each error.
- See the reports at `https://app.bugsplat.com/v2/crashes?database=<database>`: each one has `Callstack.txt` (the exception), `log.txt` (the end of the log, with the user folder, the Windows account and the player id removed) and the attributes `modId`, `modLibVersion`, `caller`, `gameVersion`, `mods`, `language`.

The player is asked again when a new mod with a database is installed. Details (routing, consent key, protocol, autotest TT-06): [docs/06-Debug.md](docs/06-Debug.md#error-reports-bugsplat).





# Cache/Save

`xxx\AppData\LocalLow\guigugame\guigubahuang\mod\{MOD_ID}`

Your mod data will be saved here.





# ModConf

I hate to use **.cache**. It could not be read/write directly.

So, i wrote a snipet which i can use **.json** to edit game's conf.

Create a folder **ModConf** in **ModProject**, copy samples from below folder to **ModConf**

`Steam\steamapps\common\鬼谷八荒\Mod\modFQA\配置修改教程\配置（只读）Json格式`

## ModConf - Notes

1. About filename, split by '**_**'. The last part is the name of conf.

![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/a1ff96de-6850-466d-a3fd-682faf38a7f3)

![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/81548d72-7c44-4fe7-abed-094ff507a38e)

2. You dont need rewrite all properties of conf. Just edit which you want.

![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/7f9de987-89a8-441d-addc-a894d37ba746)

※**"__name"** is my comment.

3. Use **|** to set same value to the properties.

![image](https://github.com/4azuo/GuiGuBaHuang-ModLib/assets/11677054/cb95e58a-daf2-45f4-88d1-6ee1cb6cca1b)

4. If **"id"** exists, conf will be override. If not exists, then will be created a new one.

5. If its a array, use **@** then **|** (separate char)

![image](https://github.com/user-attachments/assets/a4d5566c-35de-4b38-b681-cd4ab997cc62)

## LocalText - Rules

1. **No hardcoded text** in the code: every text shown to the player is a localText key.
   - Code: `GameTool.LS(key)`; with format items `string.Format(GameTool.LS(key), args)` (Taoist: `ModTextHelper.LSF(key, args)`).
   - Never read a language field directly (`allText[key].en`): it shows English whatever the language.
2. **Which file**: one `<feature>_localText.json` per feature, key prefix = feature, one block of ids per file.

   | Project | File | Keys | Ids |
   |---|---|---|---|
   | ModLib (3385996759) | `localText.json` | `libtxt<id>` | `999990000`+ |
   | Taoist (3161035078) | `ui_smcfgs_localText.json` | `smcfgs…` | `-1000001000`… |
   | Taoist | `ui_trainer_localText.json` | `trainer…` (window labels `trainer_lbl_…`) | `-1000002000`… |
   | Taoist | `ui_setting_localText.json` | `setting…` | `-1000003000`… |
   | Taoist | `newgame_localText.json` | `newgame…` | `-1000005000`… |
   | Taoist | `features_localText.json` | `log_`, `secl_`, `enc_`, `loan_`, `wanted_`, `family_`… (features of `Mod/`) | `-1000006000`… |
   | Taoist | `story_localText.json` | `story_` (storylines) | `-1000007000`… |
   | Taoist | `ui_localText.json` | `ui_` (labels of the UIs: market, storage, team, town, stats…) | `-1000008000`… |
   | Taoist | `dmginfo_localText.json` | `dmginfo_` (damage calculation window) | `-1000009000`… |
   | Taoist | `body_localText.json` | `body_`, `log_body_` (body refining) | `-1000010000`… |
   | Taoist | `estate_localText.json` | `estate_`, `log_estate_` (town estates) | `-1000011000`… |
   | Taoist | `note_localText.json` | `note_` (spirit banknotes) | `-1000012000`… |
   | Taoist | `merit_localText.json` | `merit_`, `log_merit_`, merit encounters `enc_flood/plague/raid` | `-1000013000`… |
   | Taoist | `artisan_localText.json` | `artisan_`, `log_artisan_` (Artisan Dao) | `-1000014000`… |
   | Taoist | `mind_localText.json` | `heart_`, `sense_`, `log_heart_`, `log_sense_` (Dao heart, spirit sense) | `-1000015000`… |
   | Taoist | older files (`other_`, `team_`, `townmaster_`…) | their prefix | positive ids of the game's style |

   A new feature with many texts gets a new file and the next free block (`-1000016000`…); the ids of a block go down (-1000008000, -1000008001…).
3. **Languages**: the main file has `en`, `ch`, `tc`, `kr`; each of the 8 folders `de es fr ja la ru th vi` has the same file with every key
   (`"en|ch|tc|kr"`). Add a key to the 9 files at once (see `docs/04-LocalText.md` for the translation tools).
4. **Numbers**: `{0:#,##0}` for amounts (`{0:0.00}` for rates).
5. **Icons**: `[icon:NAME]` shows the game's sprite NAME, e.g. `{0:#,##0} [icon:zm_lingshi]` (spirit stones). NAME is the name of a
   sprite of the game's TextMeshPro sprite asset (`zm_lingshi`, `zm_yaocai`, `zm_kuangshi`, `lingli`, `daodian`, `jinenglengque`…; no
   required prefix). ModLib turns the tokens into `<sprite name=NAME>` when the mod loads (and after a language change); dramas and
   TextInfo show them as is, the other texts (custom UI items, message boxes, drama options) through a TextMeshPro twin: set texts with
   `Text.Set(...)` (CompHelper), not `text.text = ...`.







# Known issues (developers)

Setting up a machine: see [INSTALLATION.md](INSTALLATION.md). Run `scripts\setup\04-verify-environment.ps1` to detect the problems below.

**1. The game closes ~2 seconds after starting / code mods never load (non-CJK Windows)**

MelonLoader's `version.dll` refuses to start when the game folder path cannot be written in the Windows ANSI code page. Steam installs the game to `...\steamapps\common\鬼谷八荒`. That path works on Chinese/Japanese Windows, but on English/European/Vietnamese locales (code page 1252, 1258...) the game exits immediately (exit code 0, no MelonLoader log). If `version.dll` was deleted instead, the game runs but only mod *configs* load, and no mod DLL runs.

Fix (pick one):
- **Start the game through an ASCII junction** (no reboot):
  ```bat
  mklink /J "%LOCALAPPDATA%\ggbh-game-link" "C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒"
  ```
  Then in Steam → game → Properties → General → **Launch Options**:
  ```
  "C:\Users\<you>\AppData\Local\ggbh-game-link\guigubahuang.exe" %command%
  ```
  (If your Windows user name is not ASCII, put the junction somewhere like `C:\ggbh` from an Admin prompt.) `scripts\game\run-game.ps1` creates and uses this junction automatically.
- **Or change the system locale**: Settings → Time & language → Language & region → Administrative language settings → *Change system locale* → Chinese (Simplified) or Japanese, then reboot. This affects other non-Unicode programs.

Keep the junction path short: MelonLoader's assembly generator fails past 260-character paths.

**2. Hundreds of "does not contain a definition" / "type not found" build errors after a game update**

The code is usually fine. The projects compile against `<game>\MelonLoader\Managed\*.dll`, which MelonLoader regenerates only when it runs after a game update (see issue 1). Run it once and rebuild:
```powershell
.\scripts\game\run-game.ps1 -UntilModLoaded -StopGame -Duration 900
```

**3. Testing in game**

`scripts\game\run-game.ps1` starts the game and follows Player.log, MelonLoader and ModLib logs live, then saves them to `logs\game-runs\<time>\`. Details: [docs/06-Debug.md](docs/06-Debug.md).

`scripts\game\run-autotest.ps1 -Save <test save>` runs the Taoist test scenarios in the game and writes the evidence (screenshots of every step, logs) to `logs\autotest\<time>\`. Open `test-viewer\index.html` to browse the results of every autotest and unit test run. Details: [docs/Taoist/04-Autotest.md](docs/Taoist/04-Autotest.md).

**4. Documentation**

- ModLib: [docs/ModLib/00-Architecture.md](docs/ModLib/00-Architecture.md)
- Taoist: [docs/Taoist/00-Overview.md](docs/Taoist/00-Overview.md)
- Getting started / configs / helpers: [docs/](docs/)





# Credits

Thanks [vuphuong878](https://steamcommunity.com/id/vuphuong878/) for translating to Chinese and Vietnamese.

Thanks [Korland](nikbraun2003@gmail.com) for translating to Russian.





# _

※Sorry for my english.

Hashtag: 鬼谷八荒, guigubahuang, mod, unity, harmony
