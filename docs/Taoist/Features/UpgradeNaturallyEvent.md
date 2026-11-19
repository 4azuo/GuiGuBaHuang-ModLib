# UpgradeNaturallyEvent

`3161035078/ModProject/ModCode/ModMain/Mod/UpgradeNaturallyEvent.cs` - cache key `ModConst.UPGRADE_NATURALLY_EVENT` - group: World and NPCs

Lets the player break through to the next phase without breakthrough items once exp is full.

## Saved data

None.

## Switch

`AllowUpgradeNaturally` (S&M). Off: `OnCacheHandler` returns false and the event is not loaded.

## Flow

| Hook | What happens |
|---|---|
| `OnOpenUIEnd(FateFeature)` | If exp is full and a next phase exists: button "Upgrade" (`other500020006`) and a tooltip (`other500020018`). Click: `GradeID` = next phase id, exp cleared. |

## Notes

- No cost, no failure chance, no breakthrough drama: the quality of the breakthrough is not rolled (the game's own breakthrough gives a quality the mod skips).
- NPCs break through on their own in [NpcAutoEvent](NpcAutoEvent.md).

## Tests

TG-25, TG-50.
