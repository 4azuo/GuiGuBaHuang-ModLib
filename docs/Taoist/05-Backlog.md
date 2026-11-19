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

- **Custom skills**: let the player create / edit skills (`CustomSkillEvent` is a start).
- **Demonic possession (đoạt xá)**: a demonic art to take over the body of another cultivator; usable by the player and by NPCs; can be discovered, then the whole world hunts the possessor (bounties in every town, sects and families hostile). Conditions to use it (art, realm, target weaker / dying, cost...) to be discussed. Technically close to "continue as a descendant" (`FamilyEvent.Inherit` copies the base properties of a unit onto the player).

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

