# BankLoanEvent

`3161035078/ModProject/ModCode/ModMain/Mod/BankLoanEvent.cs` - cache key `ModConst.BANK_LOAN_EVENT` - group: Economy

Bank loans from the budget of a town (button "Bank loan" in the town storage). One loan at a time. Late payment brings penalty interest, reminder letters, then debt collectors; beating two collectors writes the debt off but blacklists the player.

## Saved data

| Property | Meaning |
|---|---|
| `TownId` | town of the loan |
| `Debt` / `Borrowed` | what is owed now / the amount borrowed |
| `DueMonth` | total month the loan is due |
| `OverdueMonths` | months late |
| `CollectorsBeaten` | collectors the player defeated (this loan) |
| `LastCollectorMonth` | total month of the last collector |
| `Blacklisted` / `BlacklistTownId` | blacklisted, and by which town |

`CollectorBattle` (static, not saved): the running battle is against a collector.

## Switch

`NoBankLoan` (S&M): no new loans (the button stays only to repay a running loan).

## Constants

| Name | Value |
|---|---|
| `INTEREST` | 3 % a month (compounded, rounded up) |
| `PENALTY` | +5 % a month when overdue (not while secluded) |
| `TERM_MONTHS` | 12 |
| `REMINDER_MONTHS` | 2 (letters in overdue months 1-2) |
| `COLLECTOR_EVERY` | 2 months |
| `MAX_BEATEN` | 2 |
| `WANTED_TAX_RATE` | 3 (taxes x3 in the blacklisting town, applied by [MapBuildPropertyEvent](MapBuildPropertyEvent.md)) |

`Limit() = CalculateInflationary(2,000 x grade^2)`.

## Borrowing

`CannotBorrow(town, amount)` returns the LocalText key of the reason, or null:

| Key | Reason |
|---|---|
| `loan_err_off` | `NoBankLoan` |
| `loan_err_blacklist` | blacklisted |
| `loan_err_loan` | a loan is running |
| `loan_err_account` | no account in the town ([BankAccountEvent](BankAccountEvent.md)) and not a guardian |
| `loan_err_limit` | amount <= 0 or above the limit |
| `loan_err_budget` | town budget below the amount |

`Borrow`: budget - amount, player + amount, due in 12 months, counters reset, log `log_loan_borrow`.

`Repay(amount)`: pays `min(amount, Debt, money)` into the town budget; log `log_loan_repay`; 0 debt -> closed (`log_loan_closed`).

## Monthly (`OnMonthly`)

For each month passed (`MonthsPassed`): `Debt += ceil(Debt x (3 % + 5 % if late and not secluded))`. If overdue and not secluded: `OverdueMonths` up, log `log_loan_overdue`; not in battle / time skip:

- overdue months 1-2: letter drama `loan_letter` (town, debt, months);
- later: a collector every 2 months (`SendCollector`).

## Debt collectors

`FindCollector`: among the town's guardians and all other living, not hired NPCs, the lowest grade >= `player grade + CollectorsBeaten`; else the strongest. The collector is moved to the player; log `log_loan_collector`; drama with its portrait (`loan_collector`):

| Option | Result |
|---|---|
| 1. Pay all | enough money: `Repay(Debt)`, `loan_paid`; otherwise treated as option 2 |
| 2. Hand over | `Seize()`: money, then the most valuable unequipped sellable items (by sale x count) until the debt is covered; all to the town budget; -100 reputation; `loan_seized` / `loan_seized_part` |
| 3. Fight | a battle (sneaky-catch dungeon) against the collector |

After the fight (`OnBattleEndHandler`, `CollectorBattle`):

- lost: `Seize()`, `loan_lost`;
- won: `CollectorsBeaten + 1` (`log_loan_beaten`); below 2: `loan_beaten` (the next is stronger); at 2: **blacklisted**: the account of that town closed, -300 reputation, the debt written off (`log_loan_blacklisted`, `loan_blacklisted`).

Result dramas use the background `dizizhenjing1`.

## UI

`OnOpenUIEnd(TownStorage)` (in game, not in battle), with an account or as a guardian: button "Bank loan" (`loan_title`) just below the game's "Storage" button, same size (`PlaceBelow`).

`OpenLoan(town)`: `UICustom2` window. Terms (`loan_terms`: 3 %, 12 months, 5 %) and a red warning (`loan_warning`), wrapped at 70 characters (`WrapWords`). Then:

- blacklisted: `loan_state_blacklist` (town);
- loan running: `loan_state` (town, borrowed, debt, months left; red when late), a repay slider (1 .. min(debt, money)) and "Repay";
- no loan: `loan_limit` (limit, town budget), an amount slider (100 .. limit) and "Borrow" (error message box on refusal).

## Tests

TG-57.
