# MapBuildBattleEvent

`3161035078/ModProject/ModCode/ModMain/Mod/MapBuildBattleEvent.cs` - cache key `ModConst.MAP_BUILD_BATTLE_EVENT` - group: Battle

Town wars (town vs town) and monster waves (on towns and sects). The player may defend, attack, try to escape or ignore; a player who is master / guardian of an attacked town far away is called back. Battles the player does not join are simulated by comparing battle power. Results change budgets, buildings and relations.

## Saved data

| Property | Meaning |
|---|---|
| `LastYearEventHappen` | town / sect id -> game year of its last war / wave |

Runtime statics: `JoinBattleFlg` (month the player joined one), `InitBattleFlg`, `PlayerSide`, `WinTeamSide`, `BuildBaseA` (defender), `BuildBaseB` (attacker town), `TeamInfo` (per side: max units, units left, world units, battle units), `DieCUnits`, `CalledToDefend`.

## Switches

- `DestroyBuildingAfterTownWar` (S&M): losses destroy sub-buildings ([BuildingArrangeEvent](BuildingArrangeEvent.md)).
- `mod_configs.json` > `MapBuildBattleConfigs.EscapeChance` = 10 %.

## Constants

| Name | Value |
|---|---|
| `MONST_WAVE_RATE` | 0.5 % x years since the last event, a month |
| `TOWN_WAR_RATE` | 0.2 % x years since the last event, a month |
| `JOIN_RANGE` | 4 |
| `MIN_UNIT` / `STP_UNIT` | 4 / 1 (local fighters: `4 + area / 2`) |
| dungeons | town war 480110990, wave on town 480110991, wave on sect 480110992 |
| `HELP_REPUTATION` | +1,000 for helping against a wave |
| `TOWN_DEFENDED_REPUTATION` | +200 for a successful defense after a call back |
| monsters | 480112870 .. 480112879 (by area) |

## Triggers (`OnMonthly`)

- **Monster wave**: every town and sect, chance `ChanceOver(0.5 x years since last, MonthsPassed)`.
- **Town war**: every town with a master, chance `ChanceOver(0.2 x years since last, ...)`; the attacker is another town whose NPC master hates this master (intimacy <= -200) or has the trait Evil / Power-hungry / Glory Hound, whose living guardians are stronger than the units within 4 of the target, and whose area is within 2.

## What the player sees

A war / wave is **skipped** (simulated) when the player already joined one this month or a time skip runs. Otherwise:

| Where the player is | War | Wave |
|---|---|---|
| inside the target | drama `battleevent480112890`: help defend / help attack / try to escape | help / try to escape |
| same area, within 8 | drama `battleevent480112894`: help defend / help attack / don't care | `battleevent480112895`: help / don't care |
| far away, master or guardian of the defending town | `CallToDefend`: drama `town_call` (log `log_town_call`): rush back (moved to the town, joins the defense) / trust the guardians (simulated) | - |
| elsewhere | simulated | simulated |

Choices: helping a side -> its guardians +50 intimacy (`Loved`), the other side's guardians +100 hate (`Hated`); for a sect, members in the area. Helping a wave: +1,000 reputation. Don't care as a guardian of the town: its council hates the player. Escape: 10 % success: moved away, **loses** half of the reputation (a guardian of the attacked town loses a fifth more and is hated by its council); failure: joins the defense and loses a third (town war) / a fifth (wave) of the reputation. The loss is a share of the absolute reputation (`LoseReputation(n)`: `-|rep| / n`), so a negative reputation gets worse too (e.g. -1,000 -> -1,500 after a successful escape). Before 2026-11-19 `rep / n` was added: a positive reputation rose and only a negative one fell.

## The battle (player joined)

`JoinTownWar` / `JoinMonstWave`: side counts computed, custom monster counters shown, battle entered (level `area x 5`).

| Side | Units | "Lives" (`UnitCount`) |
|---|---|---|
| A (defender, town war) | units within 4 of the town (up to `4 + area/2`) + its guardians | `8 + (units x (2 if city) + budget / 1M) x 1.5` |
| B (attacker town) | its guardians | `8 + units x (2 if city) + budget / 1M` |
| A (wave) | as above | `10 + (units x (1.6 if city) + budget / 1M) x 2` |
| B (wave) | 2 monsters per A unit, more spawned as they die | `(month + A lives x gameLevel + 2^area) x Random(0.8, 1.1)` |

`OnBattleStart`: units created on their side (A left, B right of the room center), unable to die (`NotDie`), full HP/MP/SP. A human of a side brought to 1 HP: one life used and revived on its side; with no lives left it is removed. In a wave, each monster killed uses a B life and spawns a new one. `OnTimeUpdate1000ms`: counters updated; a side at 0 lives ends the battle (all units paused).

## Results

| Outcome | Played (`OnBattleEndHandler`) | Simulated (`SkipTownWar` / `SkipMonstWave`) |
|---|---|---|
| A wins | A loses budget in proportion to lives used (one random sub-building destroyed with the switch, town war / town wave); war: B loses 80 % of its budget | war: A -30 % budget (and a building if `A/B < 2`), B -20 %; wave: A -20 % |
| B wins | A loses its whole budget (2 buildings destroyed; wave on a town: every managed building); war: B loses budget by lives used and pillages: half of A's budget shared among B's guardians, the rest to B's budget | same, B -10 % budget |

Simulation: `power(A units) x A lives x 1.5` against `power(B units) x B lives` (wave: `UnitPowerCalHelper.CalMonstBattlePower(gameLevel, area) x B lives`).

Relations: the losers' guardians +100 hate for the winners (+100 more for the winning master), winners +50 for the losers; fighters of a side +50 intimacy with each other. Called back and the defense held: +200 reputation, log `log_town_defended`.

A town master player can also **declare war** on another town from its guardians window ([MapBuildPropertyEvent](MapBuildPropertyEvent.md)): `TownWar(target, own town, proactive = true)` -> the player attacks (side B).

## API

`IsBattleTownWar()`, `IsBattleMonstWave()`, `IsBattleMonstWaveOnTown()`, `IsBattleMonstWaveOnSect()` (read by other battle features to skip these battles), `TownWar`, `MonstWave`, `CallToDefend`.

## Tests

TM-23.
