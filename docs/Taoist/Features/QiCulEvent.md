# QiCulEvent

`3161035078/ModProject/ModCode/ModMain/Mod/QiCulEvent.cs` - cache key `ModConst.QI_CUL_EVENT` - group: World and NPCs

Qi cultivation: a unit whose exp is full keeps cultivating and stores Qi; Qi gives Atk, HP and MP. Qi can be passed to another unit.

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `Qi` | `Dictionary<unitId, long>` | Qi of every unit (player and NPCs) |
| `LastYearReceiveQi` | int | Game year the player last received Qi from the master |

## Switches

- `NoQiCultivation` (S&M): `OnCacheHandler` returns false -> the event is not loaded at all (`Instance` null, all Qi bonuses 0; BUG-16).
- `GrowUpSpeed` (S&M): Qi is added every `GrowUpSpeed` months (`ModMaster.Ticks`).

## Flow

| Hook | What happens |
|---|---|
| `OnMonthlyForEachWUnit` | All units (player too). If exp is full and `ticks > 0`: `Qi += Sp x Random(0.8, 1.2) x ticks`. |
| `OnOpenUIEnd(FateFeature)` | Player cultivation screen: Qi, Atk/HP/MP bonuses, a tooltip button. |
| `OnOpenUIEnd(NPCInfo)` | Units with Qi (not while the hire window is open): Qi and bonuses, button "Transfer Qi" (player -> NPC); if the NPC is alive, is the player's **Master** and the player has not received this year: button "Receive Qi" (master -> player, sets `LastYearReceiveQi`). |

## Formulas

Bonuses (in `UnitModifyHelper`, added to the unit by [UnitModifyEvent](UnitModifyEvent.md)):

| Bonus | Formula |
|---|---|
| Atk | `Qi / (10,000 x gradeLvl)` |
| HP | `Qi / (1,000 x gradeLvl)` |
| MP | `Qi / (4,000 x gradeLvl)` |

Bonuses fall after a breakthrough (divided by a bigger grade) until Qi builds up again.

Transfer `QiTransmit(from, to)`:

```
sent     = from.Qi / 10 x Random(0.8, 1.1)
received = sent x to.Talent / 100 x Random(0.8, 1.1)
```

Then the drama `480010100` and a message box with the sender's old / new Qi.

## Tests

TG-17, TG-25, TM-19.
