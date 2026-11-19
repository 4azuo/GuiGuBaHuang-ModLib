# MonstStrongerEvent

`3161035078/ModProject/ModCode/ModMain/Mod/MonstStrongerEvent.cs` - cache key `ModConst.MONST_STRONGER_EVENT` - group: Battle

Monsters scale up: with the S&M rates, the area, time, the number of kills, the player's attack, rebirths and the game level. Summons scale with the player; sect guardians with the strongest cultivator of the area. Devil Demon pets grow every month.

## Saved data

| Property | Meaning |
|---|---|
| `Counter` | Growth ticks since the start (+1 every `GrowUpSpeed` months) |
| `KillCounter` | Kills per monster type (Common, Elite, BOSS, NPC) |
| `Additional` | Per type; kept for old saves, no longer used (the "additional" step is commented out) |

## Switches

S&M: `AddAtkRate`, `AddDefRate`, `AddHpRate`, `AddBasisRate`, `MonsterPropertiesDependOnPlayerProperties`, `GrowUpSpeed`.

`mod_configs.json` > `MonstStrongerConfigs`:

| Key | Value |
|---|---|
| `AtkR` / `DefR` / `MHpR` | 1.05 / 0.5 / 1.5 (weight of every bonus on Atk / Def / HP) |
| `RebirthAffect` | 0.04 |
| `GrowRate` (per tick) | Common 0.004, Elite 0.002, BOSS 0.001, NPC 0.002 |
| `KillGrowRate` (per kill) | Common 0.000005, Elite 0.00005, BOSS 0.0001, NPC 0.00005 |
| `PlayerAtk2Hp` | Common 1, Elite 10, BOSS 100, NPC 50 |
| `AreaBonus` (area 1..13) | 0.001, 0.005, 0.01, 0.015, 0.02, 0.03, 0.045, 0.06, 0.08, 0.10, 0.13, 0.16, 0.20 |
| `PlayerAffectSummon` | Atk 0.05, Def 0.01, MHp 0.10, Bas 0.01 |
| `Additionnal` | 0.02 (unused) |

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame` | Creates missing counters. |
| `OnMonthly` | Per tick: `Counter + 1`; every Devil Demon pet: atk / def / hp scales +1 %. |
| `OnBattleUnitDie` | Monster of a configured type died: `KillCounter[type] + 1` (any killer). |
| `OnBattleUnitInto` | Monsters: the steps below, then HP filled to max. |

## Steps of `OnBattleUnitInto`

`gameLvl` = game level, `grade` = monster grade. All additions clamp to int.

1. **S&M**: Atk, Def, max HP and the 12 bases `x (1 + Add*Rate)`.
2. **Summons**: `ratio = 1 + SummonPower adjustment / 100` of the summoner ([CustomRefineEvent](CustomRefineEvent.md)). Adds player Atk x 0.05 x ratio, Def x 0.01 x ratio, **current HP** x 0.10 x ratio, every basis x 0.01 x ratio.
3. **Sect guardians**: adds the base Atk / Def / max HP / bases of the strongest unit in the sect's area.
4. `atk, def, mhp` = values after steps 1-3 (the base of the % below).
5. **Area** (area of the player): `r = AreaBonus[area] x gameLvl`.
6. **Growth**: `r = (Counter x GrowRate[type] + KillCounter[type] x KillGrowRate[type]) x grade x gameLvl`.
7. **Player attack**: if `MonsterPropertiesDependOnPlayerProperties`: max HP += player Atk x `PlayerAtk2Hp[type]` x gameLvl.
8. **Rebirth**: `r = TotalGradeLvl x 0.04 x gameLvl`.
9. **Bases**: every basis x `grade x gameLvl`.
10. **Bases to stats**: Def += `avg(Fist, Palm, Finger) / 1000 x Def` + `Earth / 1000 x Def`; move speed += `Wind / 100`.

Steps 5, 6, 8 each add `atk x r x 1.05`, `def x r x 0.5`, `mhp x r x 1.5`.

Example of growth: a grade-3 common monster, game level 2, after 120 ticks (10 years at GrowUpSpeed 1) and 2,000 common kills: `r = (0.48 + 0.01) x 3 x 2 = 2.94` -> +309 % Atk, +147 % Def, +441 % HP.

## Notes

- Everything is computed when the monster enters the battle; nothing changes during the battle.
- Pet scales are strings `"type|value"` in the game data; the value is raised by 1 % per tick, compounded.

## Tests

TG-20, TB-01, TB-06.
