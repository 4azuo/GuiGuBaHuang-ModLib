# BuildingArrangeEvent

`3161035078/ModProject/ModCode/ModMain/Mod/BuildingArrangeEvent.cs` - cache key `ModConst.BUILDING_ARRANGE_EVENT` - group: Economy

Towns and sects build their sub-buildings (portal, auction, forge, shops, bounty board, training hall) over time, paid from their budget. Options: start with towns stripped of them, or keep portals only in cities and sects.

## Saved data

| Property | Meaning |
|---|---|
| `DefaultBuilding` | `"{buildId}_{name}"` of every sub-building the world had at creation: only these can be (re)built |

## Switches (S&M)

- `AllowTownBuildupOverTime`: at new game, every managed sub-building is removed (from all towns except area 1, unless `AllowTownBuildupOverTime_IncludeFirstTown`).
- `OnlyPortalAtCityAndSect`: small-town portals removed at new game and never rebuilt.
- `AddBuildingCostRate`: cost multiplier.

## Managed sub-buildings (`BuildingCostEnum`)

| Sub-building | Where | Chance / month | Cost by area 1..10 |
|---|---|---|---|
| Training hall | sects | 5 % | 10k, 20k, 50k, 50k, 200k, 200k, 500k, 500k, 1M, 3M |
| Sect portal | sects | 4 % | 15k, 30k, 100k, 100k, 320k, 320k, 1M, 1M, 3M, 10M |
| Town portal | towns | 4 % | 20k, 40k, 150k, 150k, 400k, 400k, 2M, 2M, 8M, 25M |
| Auction | cities, towns | 2 % | 10k, 20k, 60k, 60k, 250k, 250k, 1M, 1M, 3M, 10M |
| Forge | towns | 3 % | 10k, 20k, 50k, 50k, 100k, 100k, 500k, 500k, 2M, 3M |
| Book market | towns | 1 % | 0, 30k, 80k, 80k, 300k, 300k, 1.5M, 1.5M, 6M, 8M |
| Dress market | towns | 3 % | 4k, 8k, 20k, 20k, 80k, 80k, 250k, 250k, 800k, 1M |
| Bounty board | towns | 8 % | 0, 4k, 12k, 12k, 25k, 25k, 70k, 70k, 150k, 200k |

Storage buildings are commented out (not managed). `IsMatchBuildConds`: any town matches; sects only for the sect entries; cities for the auction.

## Flow

| Hook | What happens |
|---|---|
| `OnLoadNewGame` | Records `DefaultBuilding`; then applies the switches above. |
| `OnMonthly` | Every building except towns where the player is town master: for each managed sub-building that existed at creation and is missing, if `budget > cost` and `Random(0,100) < rate` -> built, budget - cost. |

```
cost = Calculate(CalculateInflationary(BuildCosts[area - 1]), AddBuildingCostRate)
```

## Notes

- A town governed by the player is never built up automatically (the player decides).
- Saves created before the mod have an empty `DefaultBuilding`: nothing is built there.

## Tests

TG-16, TM-12.
