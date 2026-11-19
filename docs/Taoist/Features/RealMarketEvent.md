# RealMarketEvent (selling)

`3161035078/ModProject/ModCode/ModMain/Mod/RealMarketEvent.cs` - cache key `ModConst.REAL_MARKET_EVENT` - group: Economy

Selling items to a town shop: the price depends on a sell rate that changes every month, and the town pays from its budget, so a poor town cannot buy much. Buying is [RealMarketEvent2](RealMarketEvent2.md); the player-to-NPC market is [RealMarketEvent3](RealMarketEvent3.md).

## Saved data

| Property | Meaning |
|---|---|
| `MarketPriceRate` | `Dictionary<town id, float>`: sell rate (%) of every town with a market (`MapBuildSubType.TownMarketPill`) |

## Switch

`mod_configs.json` > `RealMarketConfigs`:

| Key | Value | Meaning |
|---|---|---|
| `MinSellRate` / `MaxSellRate` | 85 / 110 | monthly random range (%) |
| `Events.HappyNewYear` | month 1, 1 month, every 12: sell +25, buy +20 | added to the range |
| `Events.HappyMidYear` | month 6, 1 month, every 12: sell -20, buy -25 | |

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame` | Every market town without a rate gets 100 %. |
| `OnMonthly` | Every market town: `rate = Random(85 + event, 110 + event)`. |
| `OnOpenUIEnd(PropSell)` (in game) | Adds: rate text (`ui_price_rate`, red), budget text, a second price line, a warning `ui_over_price` (hidden). Replaces the OK button action by `SellEvent`. |
| `OnTimeUpdate100ms` | While the sell window is open: money owned, total `price + cashback`, town budget; the OK button is hidden and the warning shown when `total price > town budget`. |

## Selling (`SellEvent`)

```
price    = sum of item sale value x count           (game base price)
cashback = price x (sellRate - 100) / 100           (negative below 100 %)
player  += price + cashback
town budget -= price
```

The items are removed from the bag, the lists refreshed.

## Sell rate of a unit (`GetSellRate`)

```
rate = town rate
     + 10                           if the unit has the Merchant role (SellValue 0.10 x 100)
     + merchantRank x 0.3           (MerchantLuckEnum IncSellValueEachLvl 0.003 x 100)
```

The bonuses are stored as fractions and converted to percent (before 2026-11-19 they were added as is: +0.1 % and +0.003 % per rank).

## Notes

- The town loses the base price, not the price with the cashback.
- Town budgets are managed by [MapBuildPropertyEvent](MapBuildPropertyEvent.md).

## Tests

TG-13, TM-05.
