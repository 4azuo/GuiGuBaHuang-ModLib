# UIDifficultyEvent

`3161035078/ModProject/ModCode/ModMain/Mod/UIDifficultyEvent.cs` - cache key `ModConst.UI_DIFFICULTY_EVENT` (Global) - group: UI

Hides the descriptions of the game's own difficulty levels, since Taoist replaces their meaning with the S&M configs ([SMGlobalConfigsEvent](SMGlobalConfigsEvent.md)).

## Flow

| Hook | What happens |
|---|---|
| `OnOpenUIEnd(Difficulty)` | For each difficulty item: `goRoot1`, `goRoot1_En`, `goRoot2`, `goRoot2_En` hidden. |

No saved data, no switch.

## Tests

TM-01.
