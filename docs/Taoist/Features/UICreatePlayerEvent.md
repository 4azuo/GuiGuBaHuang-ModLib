# UICreatePlayerEvent

`3161035078/ModProject/ModCode/ModMain/Mod/UICreatePlayerEvent.cs` - cache key `ModConst.UI_CREATE_PLAYER_EVENT` (Global, works on the title screen) - group: UI

Character creation: the born destinies of one chosen level (S&M "priority destiny level") come up much more often.

## Saved data

None (reads `SMGlobalConfigsEvent.Instance.PriorityDestinyLevel`).

## Flow

| Hook | What happens |
|---|---|
| `OnOpenUIStart(CreatePlayer)` | If `PriorityDestinyLevel != 0`: every born destiny config (`g.conf.roleCreateFeature`, `type == 1`) of that level gets `weight = 1000` and `lockLuck = 0` (unlocked). |

## Notes

- The config table is changed in memory for the rest of the game session (until restart); choosing level 0 later does not undo it.
- Runs before the window builds its random destinies, so the first roll already uses the new weights.

## Tests

TM-02.
