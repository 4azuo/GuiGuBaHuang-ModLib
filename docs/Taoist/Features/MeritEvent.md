# MeritEvent

`3161035078/ModProject/ModCode/ModMain/Mod/MeritEvent.cs` - cache key `ModConst.MERIT_EVENT` - group: World and NPCs

Merit (công đức): a path of cultivation by good deeds, for the player and the NPCs. Off with the S&M switch `NoMerit`
(+500 points; `OnCacheHandler` false, `Enabled` false, every bonus 0).

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `Merits` | `Dictionary<unitId, MeritData>` | `Merit` (points, below zero = karma, from -10,000 to 40,000), `Essence` (points above the top), `LuckGiven` (the Luck the merit gave, kept in step), `LastDonateMonth` |
| `LastBountyYear` | int | last year a karma bounty was put on the player |

## Levels

| Level | Name | Points | Attack | Defense | Luck |
|---|---|---|---|---|---|
| 6 | Boundless Merit (Vô lượng công đức) | 40,000 | +6 % | +9 % | +3 |
| 5 | Merit Saint (Công đức thánh giả) | 15,000 | +5 % | +7.5 % | +2 |
| 4 | Golden Light (Công đức kim quang) | 6,000 | +4 % | +6 % | +2 |
| 3 | Blessed Virtue (Phúc trạch) | 2,000 | +3 % | +4.5 % | +1 |
| 2 | Accumulated Virtue (Tích đức) | 500 | +2 % | +3 % | +1 |
| 1 | Good Karma (Thiện duyên) | 100 | +1 % | +1.5 % | 0 |
| 0 | No merit | 0 | - | - | - |
| -1 | Sinful (Tội nghiệp) | -100 | +2 % | -2 % | -1 |
| -2 | Wicked (Ác nghiệp) | -500 | +4 % | -4 % | -2 |
| -3 | Damned (Nghiệp chướng) | -2,000 | +6 % | -6 % | -3 |

Attack and defense are shares of the base value (`UnitModifyEvent.GetAdjustAtk / GetAdjustDef`) **plus a flat part** for
the low grades (`FlatAtk / FlatDef`: attack +1 and defense +1 a level; karma attack +2, defense -1 a level); Luck is a
property moved by `Sync` when the level changes (`LuckGiven`).

**Essence**: at the top the points above 40,000 become essence, without a limit: each 10,000 is a step of +0.5 % attack,
+0.5 % defense, and every 5 steps +1 Luck (as the essence of the body, [BodyCulEvent](BodyCulEvent.md)).

## Sources (merit points)

| Deed | Points |
|---|---|
| a beast killed by the player (not in a spar, the arena, a trial) | +2 x its grade |
| an evil cultivator killed (StandDown above StandUp) | +5 x its grade |
| a cultivator of no side killed | -10 x its grade |
| a righteous cultivator killed | -20 x its grade |
| a foe killed in a town war: defending the town / attacking it | +3 / -3 x its grade |
| a donation to the town (merit window, once a month, `Cost(5)` to the town budget) | +10 x the player's grade |
| encounter: wounded cultivator healed / robbed | +30 / -60 |
| encounter: beast cub fed / town festival | +10 / +5 |
| encounter **Flood relief** (in a town: `Cost(8)` to the town budget) | +20 x grade |
| encounter **Plague** (in a town: 5 herbs of the bag, the cheapest) | +15 x grade |
| encounter **Beast raid** (outside towns): fight (60 %: -20 % HP, +25 x grade; else -40 % HP, +5 x grade) / walk away | / -5 x grade |
| NPCs every month by their side | righteous +0..3, evil -0..3 |

Taoist log: `log_merit_gain`, `log_merit_loss` (with the deed), `log_merit_level`, `log_merit_bounty`.

## World

- **NPCs around the player** (8 cells, every month): same side as the merit (righteous NPCs for merit, evil ones for karma)
  +2 intimacy a level; the other side -1 a level (as hatred).
- **Lightning**: the trial of lightning ([RealTrialEvent](RealTrialEvent.md)) and the body tribulation: -5 % damage a
  merit level (at most -30 %), +10 % a karma level; the NPC body tribulation chance +3 % a merit level.
- **Towns**: donations and flood relief go to the town budget ([MapBuildPropertyEvent](MapBuildPropertyEvent.md)).
- **Karma bounty**: from karma -2, once a year the highest righteous cultivator puts a bounty on the player
  ([WantedEvent](WantedEvent.md), reward `MinReward x karma level`).
- NPCs gather merit or karma by their side, so the old righteous cultivators of the world have merit levels (and their
  bonuses); the NPC info shows the level.
- **The world answers** (`MonthlyDeeds`, player): merit 2+: a gift of the grateful (1 % a month a level above 1: a herb
  or a pill of the grade); karma -2 or below: a revenge of the kin of the victims (1.5 % a month a level below -1: HP -15 %,
  Mood -10).
- **Town growth**: the population target of a town where the player has a house is +3 % a merit level (EstateEvent).

## Massacre of a town (đồ thành) - the evil way

`MeritEvent.Massacre.cs`. With karma -1 or below, in a town the player is not the master of, the merit window offers
"Massacre <town>". The drama tells who would stand and who would flee, then a **real battle** (dungeon 11, the town
master as the opponent; the player can die):

- **Defenders**: the town master, the cultivators of the town (6 cells) who are not evil, not the player's family or
  hired, of grade at least the player's - 1 (`Defends`, the strongest 8), and the town guards (2 + 2 a palace level,
  monsters of the area). The others **flee** (righteous ones hate the player more after a massacre, evil ones like it).
- **Won** (the player stands, the town master fell; no master: every defender fell): the town master and the fallen
  defenders die, 1 % of the mortals survive (`Survivors`), the town is a **ruin** for 10 years (population target x0.2,
  `Ruins`), half of the town budget is looted, karma -3,000 (Damned) and a bounty; the result shows on the map.
- **Lost**: karma -300.
- A save loaded during the battle drops the massacre.

## UI

"Merit" button on the cultivation screen, left of the figure (the level under it) -> window: level, points / next,
essence, bonuses, lightning, NPC attitude, karma bounty warning, the table of the levels, "Donate" in a town, "Massacre" with karma.

## Tests

TG-80 (autotest), TG-82 (massacre), `FormulaTests.Merit_Formulas` (unit test).
