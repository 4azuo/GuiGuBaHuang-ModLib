# EncounterEvent

`3161035078/ModProject/ModCode/ModMain/Mod/EncounterEvent.cs` - cache key `ModConst.ENCOUNTER_EVENT` - group: World

Random encounters of the player (at most one a month) as dramas with choices, and the yearly fortunes of NPCs. Every result is written in the Taoist log.

## Saved data

| Property | Meaning |
|---|---|
| `Enabled` | encounters on / off (Taoist Settings, [UISettingScreenEvent](UISettingScreenEvent.md)) |
| `Count` | encounters seen |

## Switches

- `NoEncounter` (S&M): none at all (player and NPCs).
- `EncounterRate` (S&M, default 3 %): monthly chance.

## Player encounters (`OnMonthly`)

Not when disabled, in battle or during a time skip. Roll `< EncounterRate` -> one of the possible encounters at random:

| Where | Possible |
|---|---|
| anywhere | Chess hermit |
| in a town | Hooded peddler, Town festival |
| outside towns | Collapsed cave, Spirit beast cub, Wounded cultivator (if an NPC not hired is within 8) |
| in a town / outside towns, merit on ([MeritEvent](MeritEvent.md)) | Flood relief, Plague / Beast raid |
| outside towns, a body refiner ([BodyCulEvent](BodyCulEvent.md)) | Waterfall (Copper Skin+), Lava pool (Iron Bone+), Thunder pool (Golden Body+) |
| S&M score tier >= Ascetic (30k P) | Heavenly trial |

`Cost(n) = CalculateInflationary(100 x grade x n)`. Paying in a town puts the money in the town budget (festival).

| Encounter | Options -> results |
|---|---|
| **Wounded cultivator** (an NPC, portrait drama) | 1. pay `Cost(2)` for medicine: +100 intimacy both ways, a herb of the player's grade. 2. rob: take 15 % of its money (min 1), +300 hate. 3. walk on. |
| **Chess hermit** (stake `Cost(3)`) | 1. play (needs the stake): win chance `clamp(25 + Talent / 4, 25, 75)` %: +`100 x grade` exp and + stake; else - stake. 2. decline. |
| **Hooded peddler** (box `Cost(5)`) | 1. buy: 10 % empty; else a herb / mineral / pill of the grade (grade + 1 when roll < 15). 2. haggle: 50 % he leaves, else 30 % off and the box (the roll x 2 reused). 3. ignore. |
| **Collapsed cave** | 1. explore: < 30: 1 herb / mineral (2 when < 10); < 55: +`Cost(2)` stones; else -30 % max HP (keeps 1 HP). 2. leave. |
| **Spirit beast cub** | 1. feed a herb from the bag: +1 Luck (+2 when < 10); no herb -> `enc_cub_noherb`. 2. leave. |
| **Town festival** (`Cost(1)`) | 1. join: Mood + up to 20, 5 townspeople near the town center +20 intimacy. 2. skip. |
| **Waterfall / Lava pool / Thunder pool** (`BodyCulEvent.OfferPlace`) | 1-3. stay 1 / 3 / 5 years: a seclusion (`SeclusionEvent`), each month the body exp of the current level / 24 / 14 / 8. 4. leave. |
| **Flood relief** (`Cost(8)`) | 1. donate (to the town budget): merit +20 x grade. 2. walk on. |
| **Plague** | 1. give 5 herbs (the cheapest): merit +15 x grade; too few -> `enc_plague_noherb`. 2. walk on. |
| **Beast raid** | 1. fight: < 60: -20 % HP, merit +25 x grade; else -40 % HP, merit +5 x grade. 2. walk away: merit -5 x grade. |
| **Heavenly trial** | 1. accept: 60 % +`1000 x grade` exp and an item of grade + 1; else -30 % HP. 2. refuse. |

Not enough money -> `enc_nomoney`. Results open a drama 300 ms later (wounded: with the NPC portrait; others: the encounter background: chess / heaven `xianrencijiu`, peddler `hundengshangren`, cave `dixiamushi`, cub `xingxingqiyu`, festival `shimengzhai_denglong`).

`GiveItem(kind, grade)` / `PickItem`: a random sellable item of the kind at the nearest available grade.

## NPC fortunes (`OnYearly`)

For each living NPC (at most `10 x YearsPassed` a year):

| Kind | Chance a year | Effect |
|---|---|---|
| Inheritance | 0.02 % | + one full phase of exp (min 50), Talent +3, Luck +1 |
| Fortune | 0.2 % | + a tenth of the phase exp |

`ChanceOver` applies the chance over skipped years. Logged (`log_npc_inheritance` / `log_npc_fortune`) only for the player's friends and family.

## Tests

TG-55, TX-08.
