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

- NPC storylines: coded in `ModMain/Story/` (lines A-F, tests TS-01..TS-06). Simplified against the design (`06-Storylines-Draft.md`), to extend later:
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