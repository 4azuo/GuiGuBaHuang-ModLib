# About my mod

※Pls, give a like to let me know it useful, tks.

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

- Create a team and hire people for combat (NPCs also team up together)
- Allow NPCs to betray under certain conditions
- NPCs track the player and look for a chance to steal when they detect rare items
- Become town master and get tax from people
- Bank needs money to open account and charge monthly fee
- Lock items when not paying
- Prices increase over time
- Towns build slowly over time (depend tax and inflationary)
- Hide map in battle (optional)
- Hide unit number in battle
- Hide training and preview skills
- Player/NPCs/Monsters has manashield, and some special efx
- Modify dmg/def calculation in battle
- Increase exp needed to level up
- Modify item price and stats
- Lose all exp at current lvl + items when dying
- Item [Resurrection Pill] blocks exp loss (high lvl should use)
- [Ring] blocks item loss when dying (depend on security points)
- Add quests
- Add roles when creating new character/npc: Power, Speed, Tao, Atk, Def, Angel, Evil, Merchant (each role has its own advantages)
- Allows exchanging items for sect contribution points
- Buying and selling in town has % price increase, there are sale off events or price increase in December and June
- Ask npc to find items for you (Commission button in the quest board in town)
- Rebirth to level 1 to cultivate again to increase basic stats
- Increase stats through battles
- Have titles corresponding to the profession
- Deeply cultivate skills to increase stats (including artifacts)
- Artifacts/Rings/Clothes, have random stats divided by rarity
- Increase difficulty for monsters
- Add "S&M Configs" 🐧
- Add Trainer in game
- Support language: English, Chinese, Vietnamese
- Add custom language systeem
- Town manage, declare war to others, build your town, change tax rate, etc...
- Monster waves
- Potmon properties auto increase monthly and depend on player properties when summoned
- NPCs auto up level and grade of skills and auto craft new artifact
- Allow NPCs/Player trade items in market
- etc...





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
