# InflationaryEvent

`3161035078/ModProject/ModCode/ModMain/Mod/InflationaryEvent.cs` - cache key `ModConst.INFLATIONARY_EVENT` - group: Economy

Inflation: the world price index (the game's difficulty price, `g.conf.gameDifficultyValue.curItem.price`, 100 = x1) grows every year. Game prices follow it, and most Taoist fees go through `CalculateInflationary`. When the index goes above 10,000, a currency reform divides all money by 10,000.

## Saved data

| Property | Meaning |
|---|---|
| `WorldPrice` | Current index (-1 = never computed on this save) |
| `Corruption` | Number of currency reforms |

## Switches

- `inflationary_configs.json` > `InflationaryRate` = 1.02 (+2 % a year).
- `AddInflationRate` (S&M): `rate' = rate + rate x AddInflationRate`.
- `AddItemValueRate` (S&M): raises the starting index.

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame` | First load on this save (`WorldPrice == -1`, new save or mod added to a running save): `WorldPrice = game price x (1 + AddItemValueRate)`, then compounded once per game year already passed. Every load: writes `WorldPrice` into the game. |
| `OnYearly` | `WorldPrice = index x rate'^max(1, YearsPassed)` (compounded over skipped years), then the reform check. |

## Currency reform (`Corrupt`)

When the index > `LIMIT` (10,000):

1. `Corruption + 1`; index reset to 100.
2. Player: money / 10,000; spirit stones in the storage removed; drama `499919998`.
3. Every living NPC: money / 10,000 (the player is skipped there: dividing twice gave 0, autotest TX-02).
4. Every town and sect budget: -80 %.
5. Player market listings cleared (`RealMarketEvent3.MarketStack`).

With the default 2 %/year, from 100 the reform comes after about 233 years.

## API

- `CalculateInflationary(int)`: `value x index / 100`, clamped to int (`ValueHelper.SafeMulInt`; was an `OverflowException` for big prices). Values <= 0 unchanged.
- `CalculateInflationary(long)`: same in long.

Used by: bank account fee, repair costs, storage fees, bank loans, family upkeep, bounties, hire costs, etc.

## Tests

TG-04, TM-20, TX-02.

## Spirit banknotes

The reform converts the spirit banknotes too: 1 stone each in the bags, lost in the storage ([SpiritNoteEvent](SpiritNoteEvent.md)).
