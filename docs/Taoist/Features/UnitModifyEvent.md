# UnitModifyEvent

`3161035078/ModProject/ModCode/ModMain/Mod/UnitModifyEvent.cs` - cache key `ModConst.UNIT_MODIFY_EVENT` - group: Unit properties

The place where every Taoist stat bonus of a unit is added to the game: one add-handler per property on the unit's `DynInt`s (attack, defense, max HP/MP/SP/DP, move speed, the 12 bases), and cooldown handlers on battle units. The formulas are in `Helper/UnitModifyHelper.cs`.

## Saved data

None. Runtime only (`[JsonIgnore]`):

| Field | Meaning |
|---|---|
| `CachedUnits` | Units already hooked (a handler is added once per unit) |
| `CachedValues` | `"{unitId}_{key}"` -> bonus, cleared every month |

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame` | Hooks every unit (`AddWUnitModifier`). |
| `OnMonthly` | Clears `CachedValues`. |
| `OnMonthlyForEachWUnit` | Hooks new units. |
| `OnBattleUnitInto` | World units in battle: handlers on `artifactCDRate`, `itemCDRate` (`GetItemCD`) and `skillCDRate`, `stepCDRate`, `ultimateCDRate` (`GetSkillCD`). |

## Caching

Each `GetAdjust*`:

- in battle: returns the cached value (0 if none) - never recomputed during a battle;
- outside battle: computed when not cached, and **always** for the player (so equipment changes show at once); NPC values are fixed for the month.

Every handler is wrapped in try/catch: an error logs and returns 0.

## What each property gets

| Property (key) | Sources |
|---|---|
| Attack (`atk`) | per equipped artifact with durability > 0: basic bonus + expertise bonus + refine bonus; active abilities: expertise bonus; refine adjustment `Atk`; Qi (`Qi / (10,000 x grade)`) |
| Defense (`def`) | artifacts: basic + expertise + refine; abilities: expertise; outfit refine; adjustment `Def` |
| Max HP (`mhp`) | artifacts: basic; abilities: expertise; ability points `x 10 x grade`; physical bases sum `x grade`; ring refine; outfit refine; adjustment `MHp`; Qi (`Qi / (1,000 x grade)`) |
| Max MP (`mmp`) | abilities: expertise; ability points; magic bases sum; mount refine; adjustment `MMp`; Qi (`Qi / (4,000 x grade)`) |
| Max SP (`msp`) | abilities: expertise; artisanship sum; adjustment `MSp` |
| Max DP (`mdp`) | adjustment `MDp` |
| Move speed (`speed`) | equipped steps that are active: expertise `L x (6G + 2l)`; adjustment `Speed` |
| Blade, Earth, Finger, Fire, Fist, Froze, Palm, Spear, Sword, Thunder, Wind, Wood | adjustment of that basis |

"Adjustment" = `CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.X)`, the random refine slots of the equipment ([CustomRefineEvent](CustomRefineEvent.md)). Expertise formulas: [ExpertEvent](ExpertEvent.md).

## Equipment formulas (`UnitModifyHelper`)

`a` = artifact data, `conf` = artifact config, `G` / `l` = grade / level, `R` = refine level, `base` = base value of the unit.

| Bonus | Formula |
|---|---|
| Artifact basic Atk | `(0.01 + 0.001 x 2^l + 0.02G) x base + (4 + 0.006 x 3^l + G) / 100 x conf.atk` |
| Artifact basic Def | `(0.01 + 0.001 x 2^l + 0.02G) x base + (3 + 0.005 x 3^l + 0.8G) / 100 x conf.def` |
| Artifact basic HP | `(0.01 + 0.01l + 0.03G) x base + l x G x conf.hp / 10` |
| Artifact refine Atk / Def | `R x (0.004G + 0.001l) x conf.atk` / `R x (0.001G + 0.0002l) x conf.def` |
| Outfit refine HP / Def | `(0.04l + 0.001R) x base` / `(0.01l + 0.0001R) x base` |
| Mount refine MP | `(0.04l + 0.001R) x base` |
| Ring refine HP | `(0.04G + 0.007l + 0.001R) x base` |

## Cooldowns

```
GetItemCD  = adjustment ItemCD  / 100 x current rate
GetSkillCD = adjustment SkillCD / 100 x current rate
```

The adjustment is a percent of the game's cooldown rate (`ItemCD` base = grade, `SkillCD` base = grade, see [CustomRefineEvent](CustomRefineEvent.md)).

## Tests

TG-17.
