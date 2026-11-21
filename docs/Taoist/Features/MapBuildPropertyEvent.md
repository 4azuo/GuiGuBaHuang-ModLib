# MapBuildPropertyEvent

`3161035078/ModProject/ModCode/ModMain/Mod/MapBuildPropertyEvent.cs` - cache key `ModConst.MAP_BUILD_PROPERTY_EVENT` - group: Economy

Town and sect economy and government: budgets (spirit stones of a town / sect), taxes (NPCs monthly, the player when entering), town masters and guardians (salaries, elections, hiring), deposit / withdraw, building, upgrade to city, the sneaky-catch battle for tax evaders. Most economy features pay into or out of these budgets.

## Saved data

| Property | Meaning |
|---|---|
| `Budget` | town id -> budget (sect budgets are the game's `school.buildData.money`) |
| `TaxRate` | town / sect id -> tax rate multiplier (default 1) |
| `TaxStats` | town id -> `TaxStat`: income, payers, leavers of the month and of the previous month |
| `TownMasters` | town id -> unit ids of its council (the master and the guardians) |

Not saved: `PayTaxTown` (towns / sects the player paid this month; cleared monthly), UI flags.

Roles are also lucks: master `420041111`, guardian `420041112` (sky tip replaced by the town name).

## Switches (S&M)

`NoTaxing` (the player never pays when entering), `AddTaxRate`.

## Constants

| Name | Value |
|---|---|
| `INIT_BUDGET` | first budget = base tax x 1,000 |
| `TOWN_YEARLY_BUDGET` / `AUCTION_YEARLY_BUDGET` | + base tax x 300 / x 150 a year (auction house) |
| `SCHOOL_YEARLY_BUDGET` | sects + base tax x 500 a year |
| `FIXING_RATE` | 5 |
| `CATCH_SNEAKY_DUNGEON_ID` | 480110993 (also used for collectors / hunters battles) |

## Formulas

```
base tax  = Calculate(CalculateInflationary(2^area x 5 x TaxRate), AddTaxRate)
            x number of sect effects (sects), x 2 (cities)
unit tax  = base tax x (1 + 0.10 if Merchant role) x (1 + merchant rank x 0.003)
            x 3 for the player in the town of a bank blacklist (BankLoanEvent)
guardian salary (year) = CalculateInflationary(3^grade x 500 x (council size + 1))
guardian reputation    = 2^(council size + 1) x 100 + 3^grade x 10
master pay (year)      = salary + budget / 100
upgrade to city        = 3^area x 1,000,000
```

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame` | every town: TaxRate 1, first budget, a council with a master (the most famous suitable unit within 4 of the town); every sect: TaxRate 1 |
| `OnOpenUIEnd(Town)` | player not paid this month, not a guardian and without a house in the town ([EstateEvent](EstateEvent.md): the house tax replaces it): `CheckIn` with a random guardian |
| `OnOpenUIEnd(School)` | player not a member: `CheckIn` with a random sect member nearby |
| `OnMonthly` | `PayTaxTown` cleared |
| `OnMonthlyForEachWUnit` | repairs role lucks out of sync with `TownMasters`; in a town: council members are fully healed; other NPCs pay `tax x MonthsPassed` (healed: the inn; tax > 1 % of their money: +1 hate for the master, < 0.1 %: +1 intimacy) or, unable, leave the town (+5 hate, moved); `RecordTax`. NPCs in a sect they are not part of pay its tax to the sect |
| `OnYearly` (x years) | per town: dead / missing master -> the most famous guardian becomes master (drama `420041113` for the player); NPC master: random TaxRate 0.5-10, and if budget > 1.5 x a salary, hires the most famous suitable unit within 4 (the player gets drama `420041114` to accept); budget + yearly income (+ auction); guardians paid (budget too low -> they leave, +100 hate for the master), NPC guardians return to the town; NPC master paid and returns. Sects: + yearly budget |
| `OnBattleUnitDie` | real battle, world units: a killer standing in a town not part of its council -> council members +50 hate; a council member killed -> removed, the others +200 hate for the killer |
| `OnTimeUpdate1000ms` (battle) | sneaky-catch battle: ends when the player or all enemies are dead |

## Player taxes (`CheckIn`)

Enough money: pays the town / sect, `PayTaxTown`, log `log_tax_paid`. Otherwise drama `501020027` (tax): option 1 sneak in: a guard catches the player with chance `1 - (max(grade - area, 0) + 1) x 40 %` -> battle (sneaky-catch dungeon); option 2: leave (the town window closes).

## Town UI

Buttons from [UIEvent](UIEvent.md) on the town window:

- **Guardian / master**: "Manage town" -> `OpenUITownManage`: NPC search with the council; town and master, budget, monthly salaries, tax rate slider 0.4-20 (live base tax, help `town_tax_help`), tax statistics of last month (`TaxStatText`), the mortals (population -> target, mortal tax) and a "Plots" button ([EstateEvent](EstateEvent.md)); a "Build" button per missing sub-building it may rebuild ([BuildingArrangeEvent](BuildingArrangeEvent.md)); "Upgrade to city" (master, not yet a city; budget > cost), "Deposit", "Withdraw" (`UIPropSelectCount`), "Hire guardians" (NPC search with suitable units in the town within 4, grade < player + 2), "Leave".
- **Others**: "Tax: N" -> `OpenUITownGuardians`: the council; a town master of another town gets "Declare war" -> `MapBuildBattleEvent.TownWar(town, own town, true)` ([MapBuildBattleEvent](MapBuildBattleEvent.md)).

NPC info of a council member: salary (shown to the master), town; "Dismiss" in the manage window. NPC info from the hire window: salary, reputation, "Hire" (paid from the town budget).

## API

`GetBuildProperty` / `AddBuildProperty` (budgets of towns and sects), `GetBaseTax`, `GetTax`, `IsTownGuardian(unit)` / `(town, unit)`, `IsTownMaster(...)`, `IsSchoolMember(...)`, `GetTownGuardians`, `GetTownMaster`, `GetGuardTown`, `Hire`, `Leave`, `RemoveFromTownGuardians`, `RecordTax`, `TaxStatText`, `Upgrade2City`, `Deposit`, `Withdraw`.

## Tests

TG-10, TM-11, TG-40, TG-58.

## Town management: buildings and banknotes (2026-11-20)

The management window lists the buildings of the town (`BuildingCostEnum`, not the sects'): the game's picture of each
(`BuildingArrangeEvent.Sprite`: TownCommon `Build<sub type id>`, e.g. Build2003 the portal, Build2008 the factory), its
cost from the budget (spirit stones: the budget is a long), then "Built", a "Build" button, or "Not in this town" (no
such building at the start of the world). The buttons on the right are one column of the same size.
Test: TG-40 (screenshot of the window, a building taken down for its "Build" button).
