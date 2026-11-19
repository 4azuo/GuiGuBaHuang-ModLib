# TimeSkipEvent

`3161035078/ModProject/ModCode/ModMain/Mod/TimeSkipEvent.cs` - cache key `ModConst.TIME_SKIP_EVENT` - group: World

Skip 1 or 10 years from the map; also the engine of seclusions ([SeclusionEvent](SeclusionEvent.md)) and of the "month passes after a lost battle" ([BattleRewardEventV2](BattleRewardEventV2.md)). A skip does not run the whole game month after month (too slow): it runs the month of the mods by strides and the month of the game itself only at a few save points.

## Saved data

None (all `[JsonIgnore]`): `IsProcessing`, `Total`, `Done`, `Interrupted`, `LastTiming`. Static test switches: `FullSimulation`, `NoInterrupt`, `HidePlayer`.

## Switches

`GrowUpSpeed` and the other monthly features scale by what the dispatch stands for (`ModMaster.MonthsPassed`, `YearsPassed`, `Ticks`, `ChanceOver`).

## UI

`OnOpenUIEnd(MapMain)`: two buttons bottom left, "Skip 1 year" / "Skip 10 years" (`other500020030`), each with a Yes/No confirmation (`other500020029`) -> `SkipTime(12)` / `SkipTime(120)`.

While skipping, the game's loading window shows `skip_progress` ("n / N months", year, month).

## Algorithm (`SkipTime(months, done)`)

| Constant / function | Value |
|---|---|
| `Stride(N)` | `max(1, N / 12)`: months one mod dispatch stands for (12 dispatches whatever the length) |
| `SAVES` | 4 save points, evenly spread, the last at the end (`SaveAfter`, `SaveEvery`) |
| `NextStep(done, N)` | next stop: up to `Stride` months further, stopping on the first save point on the way |
| `MONTH_DELAY` | 200 ms between steps |

Each step (`NextMonth`):

1. The months of the step before its last one only move the calendar (`AddDay` without running).
2. **Save point** (or `FullSimulation`): `RunGameMonth`:
   - every living unit ages by the months passed without a game month run (`AgeUnits`, `CatchUpAge`: at most life - 1, so deaths of old age happen in the game run that follows);
   - the game's own month run (NPCs cultivate, break through, marry, die; the game saves); the end call continues the skip;
   - safety net `WatchRun` (every 2 s): waits while the player is in a drama / battle; considers the run over 20 s after its save (`SAVED_GRACE_MS`) or after 2 min (`RUN_TIMEOUT_MS`), or when the game reports it stopped without calling back.
3. **Other steps**: the month of the mods only: `WorldRunStart` + `WorldRunEnd` raised, no save (`ModMaster.NoSaveOnWorldRunEnd`); ModLib dispatches once with `MonthsPassed` = months of the step.
4. After each step: the player gains `(MP / 10 + SP / 2) x months` exp; then the next step once the save (if any) is written (`WhenSaved`).

Example: 10 years = 120 months, stride 10: 12 steps, 4 of them (months 30, 60, 90, 120) are game month runs.

## Interruptions

A drama (`DramaDialogue`, `DramaFortuitous`, `DramaDialogueBigTexture`, `DramaBigTexture`) opening or a battle starting during the skip sets `Interrupted`: the skip stops after the month in progress so the player can answer; a seclusion ends early. `NoInterrupt` (tests) disables it.

End (done or interrupted): `LastTiming` filled (months, total ms, game runs and their time, mod months and their time; logged), `IsProcessing = false`, progress hidden, `done` callback.

## Other features read it

`IsProcessing`: no storage debt drama, no encounters, etc. during a skip.

## Tests

TG-24, TM-16, TX-07, TX-08, TX-09.
