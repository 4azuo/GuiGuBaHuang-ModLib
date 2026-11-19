# UseSpiritStoneCulEvent

`3161035078/ModProject/ModCode/ModMain/Mod/UseSpiritStoneCulEvent.cs` - cache key `ModConst.USE_SPIRIT_STONE_CUL_EVENT` - group: World and NPCs

Cultivate with spirit stones: pay money for exp from the cultivation screen.

## Saved data

None.

## Flow

| Hook | What happens |
|---|---|
| `OnOpenUIEnd(FateFeature)` | If exp is not full: tooltip (`other500020073`) and button (`other500020068`). |

Click:

```
cost  = 10,000 x gradeLvl
exp   = 100 x gradeLvl x (Talent / 100) x (sum of all bases / 100 / number of bases) x Random(0.8, 1.2)
```

- Money > cost: pays, adds exp, refreshes the screen (only when a grade is selected: `UIFateFeature.UpdateUI` throws with `lastSelect == null`), message `other500020070` (cost, exp).
- Otherwise: message `other500020069` (not enough money).

## Notes

- Can be clicked many times; the button disappears only when the screen is reopened with full exp.
- Exp grows with the average of the bases, so a unit with high bases gets much more per stone.

## Tests

TG-25, TM-17.
