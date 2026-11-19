# SpecialMonsterEvent

`3161035078/ModProject/ModCode/ModMain/Mod/SpecialMonsterEvent.cs` - cache key `ModConst.SPECIAL_MONST_EVENT` - group: Battle

Monsters may get a shield when they enter the battle, explode when they die, or split into new copies.

## Saved data

None. `MultipliedUnits` (runtime, `[JsonIgnore]`) lists the copies created this battle; cleared at `OnBattleEnd`.

## Switches

- `AddSpecialMonsterRate` (S&M): raises every chance below through `Calculate(chance, rate)`.
- `BossHasShield` (S&M): bosses and NPC-type monsters always get a shield.

## Scope

Monsters only, not the Devil Demon pot (`IsPotmon`), not summons.

## Flow and formulas

`gameLvl` = game level (`GameHelper.GetGameLevel`), `grade` = monster grade. Chances are in %.

| Hook | Monster type | Effect | Chance (%) |
|---|---|---|---|
| `OnBattleUnitInto` | Common, Elite | Shield = max HP (max shield unlimited), via `BattleManashieldEvent.ShieldUp` | `0.6 x grade x gameLvl` |
| `OnBattleUnitInto` | BOSS, NPC | Shield = max HP | 100 % if `BossHasShield`, else none |
| `OnBattleUnitDie` | Common, Elite | **Explosion**: effect `baiyuanshizhen`, a hit of the monster's base attack on every enemy within 2.5 | `0.4 x grade x gameLvl` |
| `OnBattleUnitDie` | Common, Elite, not itself a copy | **Split**: effect `changhenfu_bao`, creates `Random(1, gameLvl)` new monsters of the same kind at its position | `0.2 x grade x gameLvl` |

Example: grade 3 elite at game level 4 -> shield 7.2 %, explosion 4.8 %, split 2.4 %.

## Notes

- Copies cannot split again (they are in `MultipliedUnits`) but can explode and have shields.
- The shield uses the mana shield effect (ids 903151120 / 903151121), see [BattleManashieldEvent](BattleManashieldEvent.md).

## Tests

TB-06.
