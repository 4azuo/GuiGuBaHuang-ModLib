# CustomRefineEvent

`3161035078/ModProject/ModCode/ModMain/Mod/CustomRefineEvent.cs` - cache key `ModConst.CUSTOM_REFINE_EVENT` - group: Unit properties

Custom refine: spend spirit stones on a ring, outfit, artifact or mount to give it refine exp. The refine level raises the item's bonuses and unlocks up to 5 random adjustment slots (extra stats). Slots can be rerolled. NPCs refine their equipment too.

Related object: `Object/CustomRefine.cs` (one slot), enums `AdjTypeEnum` (what a slot gives) and `AdjLevelEnum` (quality).

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `RefineExp` | `Dictionary<soleID, long>` | Refine exp of each item |
| `CustomRefine` | `Dictionary<"soleID_index", CustomRefine>` | Slots 1..5 of each item (created at random on first read) |

`CachedValues` (runtime, `[JsonIgnore]`): slot values, cleared monthly and when the item changes.

## Switches

- `AddRefineCost` (S&M): raises the exp per level.
- `GrowUpSpeed` (S&M): NPC refining runs every `GrowUpSpeed` months.
- `mod_configs.json` > `CustomRefineConfigs.LevelRates`: slot quality chance (%): Common 30, Uncommon 20, Rare 20, Myst 16, Lgendary 8, Beast 4, Antique 2.

## Flow

| Hook | What happens |
|---|---|
| `OnMonthly` | Clears `CachedValues`. |
| `OnMonthlyForEachWUnit` | NPCs in a **small town** (not a main town) with money > 3 x the town tax: for each equipped refinable item, spends `sqrt(money)` (to the town budget) for `spend x RefineWeapon / 40` exp. |
| `OnOpenUIEnd(TownFactoryShapeOption)` | Forge of a **small town**: the upgrade button is replaced by the custom refine. |

## Player refine (forge)

1. **Item selector** (`other500020001`): the refinable items of the bag, one at a time. Under OK: progress text (`refine_progress`: level, exp / exp of next level, next slot at N or all slots) and a "Reroll" button (`refine_reroll`).
2. OK -> **material selector** (`other500020002`): spirit stones only. OK: `value = worth x count`, `exp = value x RefineWeapon / 40` (long), money - value, town budget + value, message `other500020051` (old level, new level, exp).

## Refine level

```
L1    = (grade x 100 + level x 20) x (1 + AddRefineCost)       grade/level of the item
level = sqrt(exp / L1)                                          (GetRefineLvl)
exp for level n = L1 x (n - 0.5)^2                              (ExpForLevel, rounded the same way)
```

Example: grade 4 item level 3 (L1 = 460): level 100 needs about 4.55M exp; with RefineWeapon 200 that is about 910k spirit stones.

The refine level also feeds the equipment formulas of [UnitModifyEvent](UnitModifyEvent.md) (artifact Atk/Def, outfit HP/Def, ring HP, mount MP).

## Adjustment slots

Slot `i` (1..5) unlocks at refine level `i x 100`. Created at random:

- **type**: random among the types allowed for the item kind with `MinGradeLvl <= item grade` (if none, the types of the lowest minimum grade);
- **quality** (`AdjLevelEnum`) by `LevelRates`: multiplier Common 1.0, Uncommon 1.1, Rare 1.22, Myst 1.35, Lgendary 1.5, Beast 1.7, Antique 2.0;
- **random multiplier** 0.6-1.4.

Value:

```
value = base(type, unit) x (0.001 x grade + 0.0002 x level) x (refineLvl - 50 x i) x quality x multiplier
```

Allowed types:

| Item | Types |
|---|---|
| Ring | Def, MHp, MMp, MSp, MDp, Nullify, RHp, RMp, RSp, RDp, SkillDamage, MinDamage, SummonPower, ItemCD, SkillCD, 12 bases |
| Outfit | Def, MHp, MMp, MDp, RHp, RMp, RDp, Nullify, BlockChanceMax, BlockDmg, EvadeChance, EvadeChanceMax, ItemCD, SkillCD, 12 bases |
| Artifact | StealHp/Mp/Sp, InstantKill, Atk, Def, Speed, Manashield, Nullify, SkillDamage, MinDamage, SummonPower, BlockChanceMax, BlockDmg, EvadeChance, EvadeChanceMax, ItemCD, SkillCD, SCritChance, SCritChanceMax, SCritDamage |
| Mount | Atk, Def, Nullify, Manashield, SummonPower, StealHp/Mp/Sp, InstantKill, SkillDamage, MinDamage |

Base value and minimum item grade of each type (`AdjTypeEnum`):

| Type | Min grade | Base |
|---|---|---|
| Atk / Def / MHp | 1 | unit property |
| MMp | 2 | max MP |
| MSp | 4 | max SP |
| MDp | 8 | grade + total rebirth grades |
| RHp / RMp | 1 / 3 | HP / MP regen base (`BattleModifyEvent`) |
| RSp / RDp | 4 / 9 | (grade + rebirth grades) x 1 / x 0.1 |
| 12 bases, Speed | 1 | property x 0.1 |
| BlockChanceMax, BlockDmg, EvadeChance, EvadeChanceMax | 1 | battle bases (`BattleModifyEvent`) |
| Nullify | 2 | grade x 4 |
| Manashield | 1 | mana shield base ([BattleManashieldEvent](BattleManashieldEvent.md)) |
| SCritChance / SCritChanceMax / SCritDamage | 4 / 5 / 5 | battle bases |
| SkillDamage | 1 | 100 + 10 x grade |
| MinDamage | 2 | minimum damage base |
| StealHp / StealMp / StealSp | 3 / 4 / 5 | grade x 0.8 / 0.4 / 0.2 |
| InstantKill | 1 | grade x 0.1 |
| SummonPower | 1 | grade x 0.5 |
| ItemCD / SkillCD | 1 / 3 | grade |

The totals per type over all equipped items (`GetCustomAdjValue`) are read by [UnitModifyEvent](UnitModifyEvent.md) (stats, cooldowns), [BattleModifyEvent](BattleModifyEvent.md) (combat), [BattleManashieldEvent](BattleManashieldEvent.md) and [MonstStrongerEvent](MonstStrongerEvent.md) (summons).

## Reroll

Window `UICustom1` (`refine_reroll`): name, progress, the 5 slots with their value and color (gray while locked, "(Req N)"); a button on each unlocked slot.

```
cost = CalculateInflationary(1,000 x grade^2 x slot index)
```

Paid to the budget of the town / sect where the player stands; the slot gets a new type, quality and multiplier; log `log_refine_reroll` (item, slot, old, new, cost). Locked slot or not enough money: `other500020087`.

## API

`AddRefineExp`, `GetRefineExp`, `GetRefineLvl`, `ExpForLevel`, `GetCustomAdjType(props, i)`, `GetCustomAdjTypes`, `GetCustomAdjValue(wunit, type)`, `Reroll`, `RerollCost`, `CopyAdj(from, to)` (copies slots and exp to another item, e.g. when an item is replaced).

## Tests

TG-08, TG-48.
