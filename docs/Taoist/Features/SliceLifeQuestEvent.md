# SliceLifeQuestEvent

`3161035078/ModProject/ModCode/ModMain/Mod/SliceLifeQuestEvent.cs` - cache key `ModConst.SLICE_LIFE_QUEST_EVENT` - group: World and NPCs

"Slice of Life" quest chain: live a number of months without any battle. Each level gives a permanent luck (HP / MP / Def). Any battle resets the counter.

## Saved data

| Property | Meaning |
|---|---|
| `TaskLevel` | Levels completed (0..6) |
| `LastCount` | Peaceful months counted for the current level (kept when the quest is recreated) |

## Configs

- `slicelife_taskBase.json`, `slicelife_task104.json`: quests `444443001..006` (counter type 104, `requireNumber`).
- `slicelife_roleCreateFeature.json`: lucks `444441001..006`.
- `slicelife_localText.json`: names and descriptions.

## Levels

| Level | Unlocked from game year | Months without battle | Reward luck |
|---|---|---|---|
| 1 | 2 | 60 (5 years) | +25 % max HP, +6 % max MP |
| 2 | 10 | 120 (10 y) | +50 % HP, +12 % MP |
| 3 | 30 | 240 (20 y) | +80 % HP, +20 % MP |
| 4 | 70 | 480 (40 y) | +120 % HP, +30 % MP, +2 % Def |
| 5 | 150 | 1,200 (100 y) | +160 % HP, +40 % MP, +5 % Def |
| 6 | 400 | 3,600 (300 y) | +220 % HP, +60 % MP, +10 % Def |

Only the luck of the last level is kept (the previous one is removed).

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame` | `AddTask()`: the quest of level `TaskLevel + 1` is created (with `curCount = LastCount`) once its year is reached. |
| `OnMonthly` | Quest exists: `curCount += MonthsPassed`; reached `requireNumber` -> `TaskLevel + 1`, count 0, new luck, quest removed. No quest (locked / all done): `LastCount = 0`. |
| `OnBattleEndHandler` | Any battle end: `curCount = 0`. |

## Notes

- A time skip / seclusion counts every month passed, so long seclusions without interruptions help a lot.
- The counter resets on any battle, also the ones the player did not start (ambushes, stalkers, hunters).

## Tests

TG-21, TB-02, TX-04.
