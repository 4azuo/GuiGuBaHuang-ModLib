# NpcAutoEvent

`3161035078/ModProject/ModCode/ModMain/Mod/NpcAutoEvent.cs` - cache key `ModConst.NPC_AUTO_EVENT` - group: World and NPCs

NPCs cultivate by themselves: monthly exp, then a breakthrough chance that grows each month while exp is full. One new NPC is added to the world each year.

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `NpcUpGradeRate` | `Dictionary<unitId, float>` | Accumulated breakthrough chance (%) of each NPC waiting with full exp |

## Switch

`AddNpcGrowRate` (S&M): multiplies the monthly exp through `SMLocalConfigsEvent.Calculate`.

## Flow

| Hook | What happens |
|---|---|
| `OnMonthlyForEachWUnit` | NPCs only (not the player). Full exp -> breakthrough roll; otherwise -> exp gain. |
| `OnYearly` | `g.world.run.npc.AddUnit(random grade 1..player grade, 1)` once per year passed (at least 1; a 10-year skip adds 10). |

## Formulas

**Exp (exp not full)**

```
exp = Random(10, 200) x gradeLvl x Talent / 100
exp = exp + exp x AddNpcGrowRate
Hero units: exp x Random(0.5, 2)
added = exp x MonthsPassed
```

**Breakthrough (exp full, a next phase exists)**

```
supportRate = 0.001 + sum over past phases of 0.001 x 2^quality      (quality of each earlier breakthrough)
NpcUpGradeRate[unit] += supportRate x MonthsPassed
success when Random(0, 100) < NpcUpGradeRate[unit] / 2^nextGrade
```

On success: `GradeID` = id of the next phase, exp cleared. The accumulated rate is **not** reset, so later phases start with the chance already built up.

## Notes

- Higher grades need exponentially longer (divisor `2^grade`); NPCs with good past breakthroughs (high quality) progress faster.
- `MonthsPassed` makes a time-skip month (that stands for N months) count N times.

## Tests

TG-06, TM-34.
