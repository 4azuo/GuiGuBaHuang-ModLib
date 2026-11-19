# RealMarketEvent2 (buying)

`3161035078/ModProject/ModCode/ModMain/Mod/RealMarketEvent2.cs` - cache key `ModConst.REAL_MARKET_EVENT2` - group: Economy

Buying in a town shop: unit prices follow a monthly buy rate; the money paid goes into the town budget. Selling is [RealMarketEvent](RealMarketEvent.md).

## Saved data

| Property | Meaning |
|---|---|
| `MarketPriceRate` | `Dictionary<town id, float>`: buy rate (%) of every market town |

## Switch

`mod_configs.json` > `RealMarketConfigs`: `MinBuyRate` 90, `MaxBuyRate` 115, `TownGReduceBuyRate` -5, `TownMReduceBuyRate` -10, and the market events (HappyNewYear buy +20 in month 1, HappyMidYear buy -25 in month 6).

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame` | Every market town without a rate: 100 %. |
| `OnMonthly` | `rate = Random(90 + event, 115 + event)` per town. |
| `OnOpenUIEnd(TownMarketBuy)` | Adds the town budget (live, `ui_market_budget_fmt`) and the rate of the player (`ui_price_rate`, red). |
| `OnOpenUIEnd(PropSelectCount)` while the shop is open | Unit price `oneCost = oneCost x rate / 100`, count UI refreshed, rate shown above the info. A listener inserted **before** the game's OK action adds `count x oneCost` to the town budget. |

## Buy rate of a unit (`GetBuyRate`)

```
rate = town rate
     - 10          Merchant role (BuyCost -0.10 x 100; was -0.1 % before 2026-11-19)
     - 10          town master of this town
     - 5           town guardian (not master)
```

## Notes

- The price is changed in the quantity window, so it applies to every item bought from the town shop.
- The town budget gains even when the purchase then fails in the game (e.g. not enough money): the listener runs first.

## Tests

TG-13, TG-45.
