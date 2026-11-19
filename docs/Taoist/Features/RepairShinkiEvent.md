# RepairShinkiEvent

`3161035078/ModProject/ModCode/ModMain/Mod/RepairShinkiEvent.cs` - cache key `ModConst.REPAIR_SHINKI_EVENT` - group: World and NPCs

Pay spirit stones to repair the damaged divine artifacts (Devil Demon pot, God Eye) or to add a fish jade to the Pisces Pendant.

## Saved data

None.

## Switch

`AllowRepairGodWeapon` (S&M). Off: the event is not loaded (`OnCacheHandler`).

## Flow

| Hook / UI | Condition | Button |
|---|---|---|
| `OnOpenUIEnd(DevilDemon)` | `GodArtifactHelper.IsPotmonDamaged()` | "Repair" (`other500020113`) -> `RepairPotmonDamaged(1)` |
| `OnOpenUIEnd(GodEye)` | `GodArtifactHelper.IsGodEyeDamaged()` | "Repair" -> `RepairGodEyeDamaged(1)` |
| `OnOpenUIEnd(PiscesPendant)` | always | "Add fish jade" (`other500020114`) -> `AddFishJade(1)` |

Each button asks Yes/No with the cost (`other500020115`). Yes with enough money: pays, applies, closes the window. Not enough: `other500020116`.

## Cost

```
Devil Demon / God Eye : (grade + RebirthEvent.TotalGradeLvl) x 100 x 2^grade
Fish jade             : (grade + grade of the Pisces Pendant NPC, 0 if none) x 100 x 2^grade
then x world price index (InflationaryEvent.CalculateInflationary)
```

`grade` = player grade. Example: grade 5, no rebirth -> 5 x 100 x 32 = 16,000 before inflation.

## Notes

- One repair step per click (level 1 of damage).
- BUG-05 (see Known-Issues) concerns the fish jade payment.

## Tests

TG-49.
