# BankAccountEvent

`3161035078/ModProject/ModCode/ModMain/Mod/BankAccountEvent.cs` - cache key `ModConst.BANK_ACCOUNT_EVENT` - group: Economy

The storage of a town can only be used with a bank account in that town. Opening one costs a one-time fee paid into the town budget.

## Saved data

| Property | Meaning |
|---|---|
| `RegisterdTown` | Ids (`buildData.id`) of the towns where the player has an account |

## Switches

- `mod_configs.json` > `BankAccountConfigs.OpenFee`: base fee per area: 1: 100, 2-3: 1,000, 4-5: 10,000, 6-7: 100,000, 8-9: 1,000,000, 10: 10M, 11: 20M, 12: 40M, 13: 100M.
- `AddBankAccountCostRate` (S&M).

## Flow

| Hook | What happens |
|---|---|
| `OnOpenUIEnd(TownStorage)` (in game, not in battle) | In a town without account, where the player is not a guardian of the town: the "Props" button is replaced by a wide button "Open account (cost)" (`ui_bank_open`). Click with enough money: pays, adds the fee to the town budget (`MapBuildPropertyEvent.AddBuildProperty`), records the town, closes the window. Not enough: nothing. |

## Cost

```
round = max(1, OpenFee / 100)
cost  = Calculate( floor(CalculateInflationary(OpenFee) / round) x round, AddBankAccountCostRate )
```

The rounding keeps the first two digits (e.g. 10,000 x 1.37 -> 13,700). `max(1, ...)` avoids a division by zero for fees below 100 (BUG-15).

## API

`RemoveAllAccounts()`: closes every account. Called when the player is blacklisted by a bank ([BankLoanEvent](BankLoanEvent.md)) and when the player continues as a descendant ([FamilyEvent](FamilyEvent.md)).

## Notes

- Town guardians / masters never need an account.
- The account only unlocks the storage; the monthly fee is [RealStorageEvent](RealStorageEvent.md).

## Tests

TG-11, TM-03.
