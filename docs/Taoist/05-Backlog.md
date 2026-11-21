# Taoist - Backlog

Open work collected from the TODO notes of the code (the notes were removed, this file replaces them) and from the
features not finished yet. Remove an item once it is done.

## From TODO notes

### Market: anonymous listings (`RealMarketEvent3`, note "Todo: IsHidden, xử lý drama")

A listing registered with "Hidden" (`MarketItem.IsHidden`, 10 % fee, also `HiddenRate` for NPC listings) is shown only
in the VIP market, but nothing else hides the seller:

- the deal dramas (`NG_Seller1`, `NG_Seller2`, `NG_Buyer1`, `NG_Buyer2`, `NG_Buyer3`, `OK`) open `DramaHelper.OpenDrama1`
  with the seller as speaker: the buyer sees who sold the item. For a hidden listing the buyer side should get an
  `OpenDrama2` text with a background ("an anonymous seller...") instead of the seller's portrait;
- intimacy / hatred of the deal (`AddIntim` / `AddHate` between seller and buyer) still targets the seller: for a
  hidden listing the buyer should not gain hatred or friendship toward a seller they do not know (the seller still
  knows the buyer);
- the selling list / negotiating list should show "Anonymous" instead of the seller name;
- the commented hint "Anonymous fee = 10% of selling price" in `OpenRegister` (key `other500020097`) can be shown
  under the Hidden toggle (`UIItemWork.UpdateAct` with the toggle state).

Test: extend TG-46 (register a hidden item, NPC buys it, check the drama has no seller portrait and no relation change).

## Features not finished

- NPC storylines: coded in `ModMain/Story/` (lines A-L, tests TS-01..TS-12). Simplified against the design (`06-Storylines-Draft.md`), to extend later:
  - A: the sect-master perk is a yearly pill; the duel with the elder is a single real battle.
  - B: the siege of the town is a single battle (no town damage / budget loss modelled).
  - D: the "Ox Fist" reward is +10 fist basis (no new technique item).
  - E: "visit A Li within a year" is not enforced (moments come by time only); A Li becomes an NPC at the adult age of `FamilyEvent.ADULT_YEARS` (`GrowUpALi`).
  - No "Stories" page in Taoist Settings (only the `NoStory` toggle); the story log uses the Taoist log.
- Seclusion "stop early" button, "coma instead of exp loss" and a manual seclusion (Taoist Settings) were proposed; not decided.
- Time skip (measured, TX-09): the mods run every N months for N years (12 runs + 4 month runs of the game whatever the length): 1 year ~1.5 min, 10 years ~2.3 min; 100 years not measured (a run of the mods gets heavier with the months it stands for: 3.6 s for 1 month, 6.5 s for 10). The run end call of the game (`AddRunEndCall`) never came in tests: `WatchRun` goes on when `g.world.run.isRunning` turns false, 20 s after the save of the run, or after 2 min (a run stuck by an error of the game's AI).

- A month run of the game during a time skip logged once `NullReferenceException at WorldUnitMgr.AddAreaUnit` (UpdateAreaUnits) after a "continue as a descendant" (heir removed with `DelUnitSecret`) and grown-up children (`npc.AddUnit`) in TX-08; the run went on. To check whether the removed heir stays in an area list.

- Continue as a descendant **by switching the player unit** (instead of copying the heir onto the player, the current way). Possible with the game data, not done because of the risks:
  - the player unit is `g.data.world.playerUnitID` (`DataWorld.World`) and `g.world.playerUnit` (`WorldMgr`, settable), `WorldUnitMgr.InitPlayer()` builds the player; set both to the heir, save, then reload the save (many systems cache the player);
  - ModLib keys the mod data of a save by the player unit id (`saves/<id>`, `<id>_configs.json`, save index): copy them to the new id before the reload;
  - an NPC lacks the data only the player has (tasks, player log, map fog, sect contributions, school tasks...): to be checked one by one; the old player unit stays as a dead unit;
  - then apply the legacy on the heir (items, spirit stones, debts, techniques option, intimacies of the old player halved / enmities in full) instead of copying the heir's base properties.

## Ideas for later (to discuss before coding)

- **Custom skills**: let the player create / edit skills.
- **Demonic possession (đoạt xá)**: a demonic art to take over the body of another cultivator; usable by the player and by NPCs; can be discovered, then the whole world hunts the possessor (bounties in every town, sects and families hostile). Conditions to use it (art, realm, target weaker / dying, cost...) to be discussed. Technically close to "continue as a descendant" (`FamilyEvent.Inherit` copies the base properties of a unit onto the player).

## Cultivation paths (decided 2026-11-20: body refining first, the rest later)

- **Body refining** (`BodyCulEvent`, done: realms, exp from hits / training / bath (treasures and herbs, doses),
  breakthrough by chance or by the body tribulation battle, tempering places (waterfall / lava / thunder pool encounters of the world map, then a seclusion),
  sects that teach it, S&M `NoBodyCultivation`, the training ground room, NPCs). NPCs use the tempering places and the
  tribulation; the body refining sects are chosen by their style every year.
- **Merit** (`MeritEvent`, done 2026-11-20: 6 levels + 3 of karma, essence, flat bonuses, encounters, NPC intimacy,
  lightning, karma bounty, gifts / revenge, town wars, town growth, the massacre of a town).
- **Artisan Dao** (`ArtisanEvent`, done 2026-11-20: the 6 crafts (herbs and ores gathered on the map); craft exp, 5 ranks by exam,
  masterpieces, furnace accidents, NPC crafters, a Saint's essence and the return to simplicity, NPC commissions,
  apprentices, market price of the title, a return-to-simplicity destiny a craft). A masterpiece used at a breakthrough of
  the game: dropped (2026-11-20).
- **Dao heart** (`DaoHeartEvent`) and **spirit sense** (`SpiritSenseEvent`), done 2026-11-20: mainly max SP / MP. Left: the
  game's own breakthrough chance from the Dao heart and a wider map view from the spirit sense (both need a Harmony patch).
- Feasibility of the paths below: `07-Paths-Feasibility.md` (merit first, then Artisan Dao, Dao heart, spirit sense).
- **Artisan Dao (luyện kỹ nghệ)**: the 6 crafts of the game as a path: crafting a masterpiece gives cultivation exp (a
  breakthrough by a masterpiece: dropped); craft titles (Apprentice -> Master -> Grandmaster) by a yearly exam in a
  city, with price bonuses at the market; named masterpieces (maker, fame) and NPC commissions (`MissionDeclareEvent` the
  other way round); furnace accidents above one's level; apprentices who craft for the player; a craft passed down to the
  heirs (`FamilyEvent.Inherit`).
- **Other paths**: spirit sense (map view, ambushes of hunters seen, heart demon resistance), Dao heart (up with kept
  promises and beaten heart demons, down with betrayal; breakthrough chance), merit (helping mortals, killing beasts that
  harm them; lowers the tribulation), beast taming, formations (protect the estate).
- A shared "Paths" frame (levels, exp sources, bottleneck, bonuses) and at most 1-2 side paths a character.

## Town estates, mortals and the clan (decided 2026-11-20, step by step with autotests)

- **B1 town mortals and plots**: done (`EstateEvent`, TG-63): town map with districts and roads, the town master's palace
  (5 levels, TG-64). To extend: NPCs buying houses too, selling / renting the house, the town master setting plot
  prices, a house upgrade taking months (building time) instead of at once.
- **District effects** (proposed 2026-11-20, to decide): market - a house there earns rent / shop income, more NPC
  visitors, thieves; noble quarter - reputation every month, NPC nobles visit with gifts / marriage proposals, higher
  NPC grades nearby; residential - neutral, cheap upkeep of the family living there; craftsmen - discounts at the forge
  / alchemy of the town, crafting exp (Artisan Dao); outskirts - cheap, beast and bandit raids (B3), quiet seclusion
  bonus. District events: a market fire, a festival in the market, the noble quarter's banquet.
- **B2 house interior**: done (`EstateEvent.Rooms.cs`, TG-64): quiet room, training ground, herb garden, spirit
  gathering array, ancestral hall. Still to add: alchemy / forge room, guest rooms (team), protective formation (B3
  raids) and:
  - Private storage (decided 2026-11-20): its own items, kept apart from the bank storage (not a bank without fees:
    the bank always takes its fee).
  - Other ideas: exp bonus of a seclusion at home by house level, reputation per house level, the family living there
    (lower upkeep), friends visiting with gifts, auction priority / market discount in that town, a big house helps to
    become town master, renting the house out.
- **B3 mortals of the estate**: groups (farmers, craftsmen, servants, guards, merchants) with loyalty, wages and food;
  output (food, herbs, ores, rent, building speed); events: a child with spiritual roots (a new NPC, disciple / adopted
  child), bandits, famine, plague, too high taxes (they leave or rise up), merit when treated well; generations over time
  skips.
- **B4 clan**: clan rank (small clan -> notable family -> great family) by assets, members and grades; clan rules
  (buffs / maluses); descendants managing estates; NPC clans buying land, land disputes (bounties), marriages for
  alliances; the heir inherits the estates.
- **Land outside towns**: a big tract in the wilds (map point), cleared by battles, roads / wells / walls / formation
  raise its price, cut into plots sold to NPCs; risks: beast tides, confiscation for unpaid taxes, mortgage at the bank
  (`BankLoanEvent`).

## Autotest: effect galleries (TE-xx)

- **TE-01 recordings: camera jumps and choppy frames** (seen 2026-11-18). Many skill effects still shake / jump the
  camera in the webp although the camera is held every frame (`BattleSceneHelper.HoldCamera`, `StopCameraShake`,
  `BattleEffectHelper.RemoveCameraControls`): to find which component still moves it (other camera scripts of the
  effect prefab, the battle's camera follow of the player, shakes started by the effect's sub-objects) and hold it.
  The recording step is ~110 ms instead of 100 ms (`Invoke-Recording` of `run-autotest.ps1`: the window capture +
  JPEG save take longer than the 100 ms period, so the loop never waits): bring it down to a steady 100 ms
  (capture and save in separate threads, a smaller capture of the game area only, or a lower JPEG quality) so the
  effects look smoother and clearer; then `effect_strips.py` ("the fixed step, ~110 ms") and the frame count of the
  webp follow.

## Promotion pictures (SteamImg, run-promo.ps1)

- TR-02 (2026-11-18) develops the character for the pictures (restored after the run): ring / outfit / weapon with
  random refine levels 120-360, 4 techniques of grade ~5 moved from cultivators of the world (the game's book study
  `StudyActionMartialProp` throws outside its window) with random mastery, sample recent foes for the damage
  calculation. The realm stays Qi Refining (ATK 30): a trainer level-up before the pictures is still possible.
- TR-01 turns the trainer toggle off in the window before the S&M pictures (score without the -1,000,000 P of the
  trainer the test settings use); Cancel at the end, nothing saved.
- Fixed 2026-11-19: the S&M sliders showed "(0P)" and did not count in the total: `ActionHelper.TracedFunc(null, def)`
  returned default(T) (false) instead of def (ModLib).
- `family-fund` is skipped when the character has no family; `taoist-log` when it has fewer than 5 entries.
- Battle pictures (NPCs joining, monster waves, trial of lightning) are not in TR-02: a staged battle (camera centred
  on the fight, `PlayerMask` off, a hand-picked moment) to add as TM-03.
- `promo_story_cold_sword`: the drama reads "Disciples of ? follow" (the `{S}` sect of line A not found when TS-01
  forces the opening, `StoryLine` place token `S`).

## Damage calculation window (DamageInfo, 2026-11-18)

- Seen in a battle by TB-17 (button under the damage multiplier, hotkey, pause, last hits, recent foes) and outside
  by TG-61. The autotest runner works with the game time: TB-17 lets the game run again after checking the pause
  (the window stays open), a runner step cannot wait while the game is paused.
- Fixed 2026-11-19: "Skill bonus (-)" (the name of a technique is in its martial info); monster pictures cut at the
  top (`MonsterOffsetY`, world units, found by an experiment).
- The example hit uses ATK as the base damage of a skill (the game's own skill formula is not reproduced) and the
  strongest basis of the attacker as the damage type. Sample monsters of TR-02 use the base numbers of their config
  x the player's grade, not a real battle.

## Storylines G-L

- Coded 2026-11-19 (lines, links, 12 languages, TS-07..TS-12). Simplified from the design, to extend later: the
  lightning trials of G5 / K are beast battles (not the `RealTrialEvent` dungeon), the heart demon's copy is a foe of
  the player's grade (not a copy of the player's looks and arts), the son of Mei Niang is a young NPC cast (not a
  child record born of her), Eternal Rival's "duel every 10 years" is a letter with exp, Guided is a one-time exp gift
  (no tribulation damage -30 %).

## Autotest: games side by side

* Game slots have their own game data folder (tools/SlotDataPath, 2026-11-18). Left: the save index read and written
  back once at the start of a slot game, before the plugin moves the paths (a native hook of the game's path code
  would remove it); the extra save folders left in a slot by interrupted character creations are not cleaned.
* TI-01 (investigation): start again from a save without closing the game. "Reload save" of the game menu
  (`UIGameMemu.ReloadCache`) left the runner waiting (first run, 2026-11-18): to look at again with the logs of
  `logs/autotest/20261118-005106`; the "return to main menu" path was not reached.


## Release CI, Nexus Mods and error reports (2026-11-19)

- **Manual install path**: the release notes say to copy `Mod_xxx` into `<game>\ModExportData\`.
  Not checked in the game yet: unsubscribe (or rename the Workshop folders), copy both zips' folders there, run
  `scripts/game/run-game.ps1 -UntilModLoaded -StopGame` and look for `加载模组：...ModExportData...` in the log.
- **Nexus Mods**: create the ModLib and Taoist pages and a first file on Nexus by hand, then run the VS Code task
  "Setup: Nexus Mods (CI)" (the CI only adds versions to an existing file). The game domain of Tale of Immortal on
  Nexus was not checked.
