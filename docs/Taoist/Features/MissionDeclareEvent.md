# MissionDeclareEvent

`3161035078/ModProject/ModCode/ModMain/Mod/MissionDeclareEvent.cs` - cache key `ModConst.MISSION_DECLARE_EVENT` - group: World and NPCs

Commissions: at the bounty board the player orders herbs, minerals, rings or breakthrough items, pays up front, and receives them some months later, unless the commission fails on the way. One commission per month.

Related object: `Object/CommissionTask.cs`.

## Saved data

| Property | Meaning |
|---|---|
| `CommissionTasks` | `List<CommissionTask>` running (items, total, fee, degree cost, months, success rate, months passed, status) |
| `LastMonthCommission` | `roundMonth` of the last commission |

## Switch

`mod_configs.json` > `MissionDeclareConfigs`:

| Key | Value |
|---|---|
| `FeeRate` / `FeeMinCost` | 0.2 / 100 |
| `DegreeCostRate` / `DegreeMinCost` | 0.001 / 1 |
| `CostTime` (months, by item level 1..6) | 1, 2, 3, 5, 8, 12 |
| `SuccessRate` (% per month, by item level 1..6) | 100, 99, 98, 96, 94, 91 |

## Which items

Built at `OnLoadGame` from every item config that is (a breakthrough item of grade <= 4, or of grade > 4 with level <= 4) or a herb, mineral or ring, **and not** sold in the town market. Grade = max breakthrough grade, else ring grade, else 1 (clamped 1..10).

In a town of level `A` (`areaBaseID`), the selector shows the items with grade in `A-1..A+1`; herbs and minerals also need `level + 1 <= A`. Stackable non-breakthrough items are offered as 100, others as 1.

## Flow

| Hook | What happens |
|---|---|
| `OnOpenUIEnd(TownBounty)` | Button "Commission" (`other500020003`) right of "Post task", shown only if no commission this month; a live list "Your commissions:" of the running ones (item x count: months left), hidden while a task info is open. |
| Button -> item selector | Shows money and mayor degree (`ui_mission_owned`); for the selection: total + fee (+ degree cost) (`commission480030400` / `500`), months and success rate (`commission480030600`); OK hidden and text red when not affordable. |
| OK (`Commission`) | Town guardian: pays `Total` only. Others: pays `Total + Fee` and `CostDegree` mayor degree. Commission added, `LastMonthCommission` set. |
| `OnMonthly` | For each running commission, month by month (`MonthsPassed`): `PassTime + 1`; above `CostTime` -> Success; else roll `Random(0,100) < SuccessRate` or Failed. Then, **if the player stands in a town** and some are finished: a reward window lists them with their status; successful items are added to the bag. Failed ones are removed with no refund. |

## Formulas (`CommissionTask`)

```
Total       = sum of count x worth
Fee         = Total x 0.2 + 100
CostDegree  = Total x 0.001 + 1
CostTime    = sum over items of CostTime[item level]        (months)
SuccessRate = min over items of SuccessRate[item level] + player Luck / 100      (% per month)
```

Chance to receive everything = `SuccessRate^CostTime` (e.g. 3 level-4 items: 15 months at 96 % -> about 54 %).

## Notes

- Finished commissions wait until the player is in a town at month end.
- The list text "Your commissions:" is hard-coded English.

## Tests

TG-23, TM-09.
