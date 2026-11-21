# UnitTypeEvent

`3161035078/ModProject/ModCode/ModMain/Mod/UnitTypeEvent.cs` - cache key `ModConst.UNIT_TYPE_EVENT` - group: World and NPCs

Every unit gets a role (`UnitTypeEnum`) the first month it is seen. The role decides how its properties grow, and may give a luck (destiny) with special effects. Also: rank lucks for artisans / trainees / merchants, monthly passive income of NPCs, and monthly property growth.

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `UnitTypeDic` | `Dictionary<unitId, UnitTypeEnum>` | Role of each unit |

## Switches

- `unit_type_configs.json` > `UnitTypes` (`UnitTypeConfig`): chance (%) of each random role (`RandomUnitType`): PowerUnit 8, SpeedUnit 8, TaoistUnit 8, AtkUnit 12, DefUnit 12, Angel 2, Evil 4, Merchant 16; the remaining 30 % get `Default`.
- `StartupPlayerRole` (S&M, > 0): fixed role of the player, taken from `SMGlobalConfigsEvent.StartupPlayerRoles`.
- `GrowUpSpeed` (S&M): property growth runs every `GrowUpSpeed` months (`ModMaster.Ticks`).
- `AddNpcGrowRate` (S&M): multiplies NPC property growth.

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame` | For every known unit whose role has a luck: removes all role lucks and adds its own again (repairs lucks lost between versions). |
| `OnMonthlyForEachWUnit` | 1. New unit: role assigned (+ its luck). 2. Rank lucks updated (artisan, trainee, merchant). 3. Passive income. 4. Property growth x ticks. |
| `OnYearly` | `SpeedUnit` NPCs: MoveSpeed + max(1, YearsPassed). |

## Roles

`PropIncRatio` = (property, ratio, minimum). Growth of one property in one tick: `max(value x ratio x inputRatio, minimum)`.

| Role | Luck id | Atk | Def | HP | MP |
|---|---|---|---|---|---|
| Default (all NPCs, added first) | - | 0.1 %, min 1 | 0.02 % | 0.4 %, min 2 | 0.2 %, min 1 |
| Player | - | 0.1 % | 0.01 % | 0.2 % | 0.1 % |
| Hero (added on top for hero NPCs) | - | 0.1 %, 1 | 0.05 % | 0.2 %, 1 | 0.1 %, 1 |
| Trial | - | 2 %, 1 | 2 %, 1 | 5 %, 100 | 2 %, 10 |
| PowerUnit | 420010100 | 0.6 %, 1 | 0.1 % | 0.3 %, 5 | 0.1 %, 1 |
| SpeedUnit | 420010200 | 0.5 %, 1 | - | 0.1 %, 1 | - (+ move speed yearly) |
| TaoistUnit | 420010300 | 0.8 %, 1 | - | 0.4 %, 1 | 0.4 %, 1 |
| AtkUnit | 420010400 | 1.0 %, 1 | - | 0.1 %, 1 | 0.1 % |
| DefUnit | 420010500 | 0.6 %, 1 | 0.6 %, 1 | 0.8 %, 5 | min 1 |
| Angel | 420010600 | 0.6 %, 1 | 0.1 % | 0.5 %, 3 | 0.3 %, 1 |
| Evil | 420010700 | 0.7 %, 1 | 0.2 % | 0.7 %, 3 | min 1 |
| Merchant | 420010800 | 0.1 %, 1 | 0.1 %, 1 | 0.1 %, 2 | 0.1 %, 1 |

`CalType` adds the role row only for roles with a luck (`Value != null`). Default, Player, Hero and Trial have none: Default is always added, Hero is added for hero NPCs, Player is used by `AddPlayerProp`, Trial only by the trial of lightning reward ([RealTrialEvent](RealTrialEvent.md), `CalProp` directly). No unit is ever given the Trial role (it is neither in `unit_type_configs.json` nor in the startup roles).

Merchant luck effects (`UnitTypeLuckEnum.Merchant.CustomEffects`): free storage, buy cost -10 %, sell value +10 %, passive income +3 % of money / month. These are read by the market and storage features.

## Property growth (per tick)

- **Player**: from grade 3 only: `AddPlayerProp(ratio = grade / 3)` with the Player row.
- **NPC**: `AddNpcProp(ratio = 1 + grade x 0.03)`. For each property of its role:
  ```
  r2Player = max(1, player value / NPC value)          (catch-up: NPCs far behind grow faster)
  rate     = Calculate(ratio x r2Player, AddNpcGrowRate)
  new value = value + Default growth + role growth + (Hero growth if hero)
  ```
  Plus, for each of the 6 artisan properties, 8 % chance of +1.

## Rank lucks

Each rank is a luck `base id + level`; when the rank changes, all levels of that kind are removed and the new one added.

| Kind | Base id | Measured on | Thresholds (level 1..10) |
|---|---|---|---|
| Alchemy / Forge / Talismans / FengShui / Herbology / Mining | 420011000 / 420021000 / 420031000 / 420041000 / 420051000 / 420061000 | RefineElixir / RefineWeapon / Symbol / Geomancy / Herbal / Mine | 50, 100, 150, 250, 350, 500, 650, 850, 1050, 1300 |
| Martial / Magic trainee | 420071000 / 420081000 | sum of physical / magic bases | 200, 400, 800 ... 102,400 (x2 each) |
| Merchant | 420091000 | money | 10k, 20k, 40k, 80k, 160k, 480k, 1.44M, 4.32M, 12.96M, 38.88M |

Merchant rank: sell value +0.3 % and passive income +0.1 % of money per level. The merchant rank never goes down (search starts at the current level).

## Passive income (per month)

```
NPC base       = Random(3^grade, 4^grade)          (player: 0)
Merchant role  += money x 3 %
Merchant rank  += money x level x 0.1 %
added          = income x MonthsPassed (capped at int.MaxValue)
```

## API

`GetUnitTypeEnum(wunit)`: role of a unit (null before its first month). `AddRandomUnitType(wunit)` is also used when new units are created by other features.

## Tests

TG-05.
