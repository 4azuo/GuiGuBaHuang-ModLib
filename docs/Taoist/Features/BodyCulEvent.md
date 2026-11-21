# BodyCulEvent

`3161035078/ModProject/ModCode/ModMain/Mod/BodyCulEvent.cs` - cache key `ModConst.BODY_CUL_EVENT` - group: World and NPCs

Body refining (thể tu): a second path of cultivation beside the Qi ([QiCulEvent](QiCulEvent.md)). The flesh is tempered by
the blows taken in battle, monthly training and medicinal baths; it gives max HP, defense and the fist / palm / finger basis.

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `Bodies` | `Dictionary<unitId, BodyData>` | `Level` (0 = not a body refiner, 1-15), `Exp` (of the current level), `InjuryMonths`, `LastBathMonth`; cultivation tracking `TrackPhase/Exp/Hp/Def/Atk`, `BonusHp/Def/Atk` (breakthroughs), `Absorbed` |

## Realms

5 realms of 3 layers (Early / Middle / Late): level 1-3 Copper Skin, 4-6 Iron Bone, 7-9 Jade Marrow, 10-12 Golden Body,
13-15 Undying Body. Grade of cultivation needed to enter a realm: 1, 3, 5, 7, 9 (`REALM_GRADE`).

## Formulas

```
exp of a level      = 200 x level²
hit exp             = min(1, damage / max HP) x 100          (a hit counts at most one max HP)
training (monthly)  = 10 (player) / 30 (NPC) x months         (not while injured)
medicinal bath      = exp of the level / 5, once a month; uses natural treasures (天材地宝, item class 508) of the bag,
                      the cheapest first: realm 1: 1 of item level 4+, 2: 2 of 4+, 3: 1 of 5+, 4: 2 of 5+, 5: 1 of 6
breakthrough chance = max(20, 70 - 10 x (realm - 1)) %       (70 % from Copper Skin, 40 % from Golden Body)
failure             = exp / 2, injured 3 months
bonuses (share = level, injured: level / 2)
  max HP            = base max HP x 4 % x share
  defense           = base defense x 3 % x share + share
  fist, palm, finger= 4 x share
Qi gain by realm    = x 1.00 / 0.90 / 0.85 / 0.80 / 0.75 / 0.70 (none, Copper Skin ... Undying Body; QiCulEvent)

cultivation (Track: player every second outside battles and monthly, NPCs monthly)
  exp needed        = x (1 + 0.1 x realm): of the exp gained in a phase the unit keeps gained / rate, the body absorbs
                      the rest (Absorbed)
  breakthrough      = a new phase adds, of the base stats it gave: HP 5 % x realm, defense 3 % x realm,
                      attack 1 % x realm (BonusHp / BonusDef / BonusAtk, permanent, by the realm at that time)
```

| Realm | Exp needed | Breakthrough bonus HP / def / atk | Qi |
|---|---|---|---|
| none | x1.00 | - | -0 % |
| Copper Skin | x1.10 | 5 % / 3 % / 1 % | -10 % |
| Iron Bone | x1.20 | 10 % / 6 % / 2 % | -15 % |
| Jade Marrow | x1.30 | 15 % / 9 % / 3 % | -20 % |
| Golden Body | x1.40 | 20 % / 12 % / 4 % | -25 % |
| Undying Body | x1.50 | 25 % / 15 % / 5 % | -30 % |

Layers inside a realm go up by themselves; at the last layer of a realm (and at level 15) the exp stops at the bottleneck
(`IsBottleneck`): no bath, only a breakthrough (`Breakthrough(unit, roll)`: `NotReady`, `GradeTooLow`, `Success`, `Failure`).

## Flow

| Hook | What happens |
|---|---|
| `OnMonthlyForEachWUnit` | NPC not a body refiner: takes up the path with 0.5 % a month; body refiners: injury months go down, training exp; NPCs at a bottleneck try a breakthrough |
| `OnBattleUnitHit` | the hit world unit is a body refiner (not injured): hit exp |
| `OnOpenUIEnd(FateFeature)` | button "Body Refining" left of the Qi lines, the realm under it -> `OpenWindow` |
| `OnOpenUIEnd(NPCInfo)` | an NPC body refiner: "Body: realm - layer" over the Qi lines |

Window (`OpenWindow`, `UICustom1`): not started -> intro and "Take up body refining" (confirm: Qi -20 %); else realm,
exp or bottleneck, injury, bonuses, "Medicinal bath" button (treasures, exp) with the icon of the treasure it uses and the count in the bag, "Break through (chance)" at a bottleneck.

Bonuses are added by [UnitModifyEvent](UnitModifyEvent.md) (`GetAdjustMaxHp`, `GetAdjustDef`, `GetAdjustFist/Palm/Finger`).
Taoist log: `log_body_start`, `log_body_layer`, `log_body_realm`, `log_body_fail`. Texts: `body_localText.json`.

## Tests

TG-62 (autotest), `FormulaTests.BodyCul_Formulas` (unit test).

## Materials, doses and risks (2026-11-20)

Baths and breakthroughs use the bag (the player only; the NPCs refine their body without), the cheapest first:

- **Natural treasures** (天材地宝 508, divine stones 506, immortal dews 531, orbs 532), item level 4 / 4 / 5 / 5 / 6 by realm:
  a bath 2 / 4 / 2 / 4 / 2, a breakthrough into realm 2-5: 6 / 10 / 16 / 24.
- **Herbs** (药材 503, spirit materials 502, dews 104; an elixir 101 counts double), by tier against the floor of the
  cultivator's grade (`HerbFloor` = 1 + (grade - 1) / 2: below it an herb is useless): low = the floor, middle = +1,
  high = +2 or more. A bath takes 50 / 100 / 200 / 300 / 500 low herbs (or 20 / 40 / 80 / 120 / 200 middle, or 10 / 20 / 40 /
  60 / 100 high, or a mix: low 2, middle 5, high 10 a unit); a breakthrough 250 / 500 / 1,000 / 2,000 low (or 2/5 of that
  middle, 1/5 high).

Three doses, a column each in the window (button, risk, the icons and counts of its materials):

| | x1 | x2 | x3 |
|---|---|---|---|
| materials | x1 | x2 | x3 |
| gain (bath exp; a breakthrough's tempering) | +0 % | +20 % | +50 % |
| backlash of a bath | 5 % | 15 % | 30 % |
| breakthrough chance | realm | -10 | -20 |

- **Toughness** (`Toughness`: max HP / 200,000 + defense / 20,000 + fist + palm + finger / 3,000, at most 1) lowers the
  risks by up to 30 % and the lifespan lost by up to 50 %.
- A **backlash**: no exp, injured a month, 6 / 24 / 72 months of lifespan (x 0.5-2). A **failed breakthrough**: half the
  exp, injured 3 months, dose x 12 x realm months (x 0.5-2). When no lifespan is left the cultivator dies of old age, as
  in the game (the window shows the lifespan left and warns; Taoist log `log_body_deadly`). No sudden death by chance.
- **Tempering** (`BodyData.Temper`): a successful x2 / x3 breakthrough adds 0.2 / 0.5 of a realm's bonuses (max HP 12 %,
  defense 9 % a realm) for good.
- **Body essence** (`BodyData.Essence`): at every bottleneck (a realm end or the top level, exp full) the exp gained becomes essence:
  each 45,000 (one level's exp) gives +1 % max HP, +0.75 % defense, +1 fist / palm / finger, without a limit; baths go on.

## Extensions (2026-11-20)

- **S&M `NoBodyCultivation`** (+1,000 points): `OnCacheHandler` false, the event is not loaded; `Enabled` is false and
  `Get` returns null, so every bonus is 0 (EstateEvent's training ground does nothing).
- **Sects by style** (`BodySects`, saved; `RefreshBodySects` every year and at load when empty): the sects of 3 members or
  more are ranked by the average share of fist + palm + finger in the basis of their members (`HandShare`); the first
  third (at least one) teach body refining (`PickBodySects`). Their members take up the path x4 more often (NPCs, 2 % a
  month) and train x2 (`TrainExp`). The sect window says so; the body window of a member too.
- **NPCs** (`OnMonthlyForEachWUnit`): a body refiner not at a bottleneck goes, 1 % a month, to the best tempering place it
  can stand for 1, 3 or 5 years (`NpcGoToPlace`, `BodyData.PlaceKind / PlaceMonths`); there it gets the exp of the place
  instead of the training (`NpcAtPlace`). At a bottleneck it breaks through by a tribulation half of the time
  (`NpcBreak`: chance `TribulationChance` = 45 + 40 x toughness + 3 x merit level, 15-90 %; success = tempering of a x2
  dose, failure = a failed x1 breakthrough), else by chance. Friends and family are written in the Taoist log
  (`log_npc_body_place`, `log_npc_body_place_end`, `log_npc_body_trib_ok / _fail`).
- **Tempering places** (`Place`): encounters of the world map ([EncounterEvent](EncounterEvent.md) `Encounter.Waterfall /
  Lava / Thunder`, outside towns, only for a body refiner who can stand the place, `PlacesFor`); the drama
  (`OfferPlace`) offers to stay 1, 3 or 5 years (a seclusion `SeclusionEvent.Kind.Waterfall / Lava / Thunder`) or to
  leave (Taoist log `log_body_place_left`): waterfall from Copper Skin, lava pool from Iron Bone, thunder pool from Golden Body; a month gives the
  exp of the current level / 24, 14, 8 (`PlaceExp`; a bath gives / 5 with materials), month by month at the end
  (`Temper`; at a bottleneck it becomes essence). No cultivation exp. The time passes as in every seclusion.
- **Body tribulation** (at a bottleneck, button under the breakthrough doses): the materials of a x1 breakthrough are
  used, the player enters the trial of lightning (dungeon 110081, level grade x 5) with the MP held under 20 %
  (`OnTimeUpdate200ms`); survived = the next realm for sure with the tempering of a x2 dose; fallen = a failed
  breakthrough of a x1 dose. The result shows on the map; the trial of lightning gives no trial properties then
  (`TribulationEnded`, RealTrialEvent). A save loaded during the battle drops the tribulation.

Window: the "Body Refining" button is in the bottom left corner of the cultivation screen. Tests: TG-62, TG-65 (a real
breakthrough of the cultivation), TG-71 (a year at the waterfall, the body tribulation), TG-81 (NPCs: sects by style, tribulation, tempering place), TG-68 (gallery of every realm at its cultivation grade, and grade 10, with its materials); `FormulaTests.BodyCul_Bath`, `FormulaTests.BodyCul_PlacesAndSects`.