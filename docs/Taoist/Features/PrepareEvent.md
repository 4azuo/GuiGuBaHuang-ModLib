# PrepareEvent

`3161035078/ModProject/ModCode/ModMain/Mod/PrepareEvent.cs` - cache key `$PrepareEvent$` - group: Core

Limits the units processed by the monthly Taoist handlers to the areas around the player, for performance.

## Saved data

None.

## Switch

`SMLocalConfigsEvent.Configs.AllFunctionsApplyToNearestUnits` (S&M option). Off: every handler sees all world units.

## Flow

| Hook | What happens |
|---|---|
| `OnRefreshParameterStoreOnMonthly` | Runs before the other monthly handlers. With the switch on, builds a child `ParameterStore` from the **full** ModLib unit list (`ModMaster.ModObj.ParameterStore.WUnits`, not the nearby list of last month) keeping units whose area id is within `player area +- 3`, and sets it with `SetModChildParameterStore`. |

Every `OnMonthlyForEachWUnit` of Taoist (NPC growth, Qi, skills, unit types, artifacts...) then runs only on these units.

## Notes

- Area ids are numbered in progression order, so +-3 keeps the player's area and its neighbours.
- Units far away are frozen for the mods (no exp, no Qi, no artifacts) while the switch is on; the game's own month run still updates them.

## Tests

TG-03.
