# RealStorageEvent

`3161035078/ModProject/ModCode/ModMain/Mod/RealStorageEvent.cs` - cache key `ModConst.REAL_STORAGE_EVENT` - group: Economy

Storage is not free: every month a fee on the worth of everything stored is charged. Unpaid fees become a debt that locks the storage.

## Saved data

| Property | Meaning |
|---|---|
| `Debt` | Unpaid storage fees |

`count` (not saved): reminders shown since the debt started.

## Switches

- `real_storage_configs.json` > `FeeRate` = 0.001 (0.1 % a month).
- `AddBankFee` (S&M): `FeeRate() = Calculate(FeeRate, AddBankFee)`.

## Free storage

No fee when the player has the **Merchant** role ([UnitTypeEvent](UnitTypeEvent.md)) or is a guardian of the town where they stand (`MapBuildPropertyEvent.IsTownGuardian`).

## Flow

| Hook | What happens |
|---|---|
| `OnMonthly` | Not free: `Debt += storage worth x FeeRate() x MonthsPassed`. If money >= Debt: pays (log `log_storage_fee`), debt 0, reminder count 0. If a debt remains: drama `480020100`, for the first 3 months and then every January; never during a time skip. |
| `OnOpenUIEnd(TownStorageProps)` | Adds two texts (storage worth, fee). With a debt: red warnings (`ui_storage_debt`) next to both search buttons and every item image in the lists stops taking clicks (items cannot be taken out). |
| `OnTimeUpdate200ms` | While the storage is open: text `ui_storage_value` (total worth, of which spirit stones, other items) and the fee `ui_storage_fee` (rate %, amount) or "free" (`other500020004`). |

## Formulas

```
storage worth = sum over stored items of count x worth      (spirit stones included)
monthly fee   = storage worth x 0.1 % x (1 + AddBankFee)
```

## Notes

- The fee is charged on the storage worth whatever the town the player is in; free storage is checked against the town the player is in at month end.
- The debt is paid from the player's money only, all at once.
- `RemoveDebt()` clears it (used when the player continues as a descendant).

## Tests

TG-12, TM-04.
