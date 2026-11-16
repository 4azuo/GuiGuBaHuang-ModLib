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

- Translations of the new texts into `de/es/fr/ja/la/ru/th/vi` (see `docs/04-LocalText.md`): Google Translate refused
  the requests (HTTP 429) on 2026-11-16 and was still blocked 3 hours later: 736 keys / 24 files missing in the 8 folders; run the translate command again.
- NPC storylines: outline of 5 lines / 8 characters (casting by sex, age, looks, traits) in `06-Storylines-Draft.md`, to be approved; then the dialogue draft, then the code.
- Seclusion "stop early" button, "coma instead of exp loss" and a manual seclusion (Taoist Settings) were proposed; not decided.
- Time skip: a month of the mods takes ~3 s, a month run of the game 15-60 s (4 of them per skip): 1 year ~2 min (TX-08: 110 s). The run end call of the game (`AddRunEndCall`) never came in tests: the skip goes on when `g.world.run.isRunning` turns false (`WatchRun`). Between two runs of the game the NPCs only grow by the mod events (Taoist NPC growth every GrowUpSpeed months); the runs of the game do the rest (breakthroughs, deaths of old age after the aging catch-up).

- A month run of the game during a time skip logged once `NullReferenceException at WorldUnitMgr.AddAreaUnit` (UpdateAreaUnits) after a "continue as a descendant" (heir removed with `DelUnitSecret`) and grown-up children (`npc.AddUnit`) in TX-08; the run went on. To check whether the removed heir stays in an area list.

## Ideas for later (to discuss before coding)

- **Custom skills**: let the player create / edit skills (`CustomSkillEvent` is a start).
- **Demonic possession (đoạt xá)**: a demonic art to take over the body of another cultivator; usable by the player and by NPCs; can be discovered, then the whole world hunts the possessor (bounties in every town, sects and families hostile). Conditions to use it (art, realm, target weaker / dying, cost...) to be discussed. Technically close to "continue as a descendant" (`FamilyEvent.Inherit` copies the base properties of a unit onto the player).