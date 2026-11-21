# ContributionExchangeEvent

`3161035078/ModProject/ModCode/ModMain/Mod/ContributionExchangeEvent.cs` - cache key `ModConst.CONTRIBUTION_EXCHANGE_EVENT` - group: Economy

A sect member can give items to the sect in exchange for sect contribution, at a ratio that changes every month.

## Saved data

| Property | Meaning |
|---|---|
| `CurMonthRatio` | Spirit stones (sale value) per contribution point this month |

## Switches

- `SectNoExchange` (S&M): no button.
- `contribution_exchange_configs.json` > `ExchangeRatio` = 40.
- `AddSectExchangeRate` (S&M).

## Flow

| Hook | What happens |
|---|---|
| `OnMonthly` | `CurMonthRatio = GetExchangeRatio() x Random(0.8, 1.3)`. |
| `OnOpenUIEnd(School)` | Only in the player's own sect: button "Exchange" (`other500020000`) -> opens the item selector (`UIPropSelect`, tag `CONTRIBUTION_EXCHANGE_EVENT`) with the ratio (`other500020057`). |
| `OnTimeUpdate200ms` | While the selector is open: preview `other500020056` (value, contribution). |
| Selector OK (`Exchange`) | The sect gets the value as money (`school.buildData.AddMoney`), the player gets the contribution item. |

## Formulas

```
GetExchangeRatio = Calculate(CalculateInflationary(40), AddSectExchangeRate)
CurMonthRatio    = GetExchangeRatio x Random(0.8, 1.3)
value            = sum of selected items: sale x count
contribution     = max(1, value / CurMonthRatio)     (0 when value is 0)
```

## Notes

- On a new save no month has passed: the ratio is drawn when the selector opens (it was 0 -> division by zero).
- The selected items are removed by the game's selector.

## Tests

TG-15, TG-47.
