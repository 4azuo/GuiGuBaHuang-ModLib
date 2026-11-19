# ChallengeQuestEvent

`3161035078/ModProject/ModCode/ModMain/Mod/ChallengeQuestEvent.cs` - cache key `ModConst.CHALLENGE_QUEST_EVENT` - group: World and NPCs

"Challenge Master" quest chain: kill distinct cultivators of a higher grade than the player. Each level gives a permanent Attack luck.

## Saved data

| Property | Meaning |
|---|---|
| `TaskLevel` | Levels completed (0..10) |
| `CurrentWins` | Wins counted for the current level |
| `DefeatedOpponents` | Unit ids already counted (the same opponent counts once) |
| `PendingLuck` | A level completed during a battle: its luck is given on the map |

## Configs

`challenge_taskBase.json`, `challenge_task104.json` (quests `444446001..010`), `challenge_roleCreateFeature.json` (lucks `444445001..010`), `challenge_localText.json`.

## Levels

| Level | Wins | At least N grades above the player | Reward |
|---|---|---|---|
| 1 | 1 | 1 | +1 % Attack |
| 2 | 2 | 1 | +2 % |
| 3 | 2 | 2 | +4 % |
| 4 | 4 | 2 | +7 % |
| 5 | 3 | 3 | +11 % |
| 6 (Grandmaster) | 9 | 3 | +16 % |
| 7 | 5 | 4 | +22 % |
| 8 | 20 | 4 | +29 % |
| 9 | 3 | 5 | +37 % |
| 10 (Legend) | 3 | 6 | +46 % |

Only the luck of the last level is kept.

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame`, `OnMonthly` | `AddTask()`: creates the quest of the next level if missing (`curCount = CurrentWins`). |
| `OnBattleUnitDie` | The dead unit is a world unit, the killer is the player, `dead grade - player grade >= MinLevelDifference`, not counted before -> `CurrentWins + 1`, quest count updated; enough wins -> `CompleteTask`. |
| `CompleteTask` | `TaskLevel + 1`, wins and list reset, quest removed, `AddLuck()`. |
| `AddLuck` | In battle: `PendingLuck = true` (the game throws when a luck is added in a battle scene). Else: removes all challenge lucks, adds the one of `TaskLevel`. |
| `OnOpenUIEnd(MapMain)` | `PendingLuck` and not in battle: 2 s later (the map is not ready right after a battle, TB-07), adds the luck; on error it stays pending for the next map opening. |
| `OnLoadNewGame` | Resets level, wins and list. |

## Notes

Monsters do not count (only world units, i.e. cultivators).

## Tests

TG-21, TB-07.
