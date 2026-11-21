# AutoTestEvent

`3161035078/ModProject/ModCode/ModMain/Mod/AutoTestEvent.cs` - cache key `ModConst.AUTOTEST_EVENT` (Global cache, works everywhere) - group: Core (tests)

The in-game test runner driven by `scripts/game/run-autotest.ps1`. It is **never loaded for players**: `OnCacheHandler` loads it only when `<mod cache>\autotest\request.json` exists (slot N: `autotest-slotN\`) and the `run-autotest.ps1` that wrote it still runs (`ReadActiveRequest`: `OwnerPid` / `OwnerStart`; a stale request is moved to `request.stale.json`, also when a game waiting in its save reads the next one). The full guide (requests, scenarios, evidence, slots, viewers) is [04-Autotest](../04-Autotest.md); the scenarios are in `ModMain/AutoTest/Scenarios/` and listed in [03-Test-Scenarios](../03-Test-Scenarios.md).

## Files

| File | Meaning |
|---|---|
| `request.json` | `AutoTestRequest`: RunId, Save, AutoLoad, Scenarios (empty = all), NewCharacter, OutDir, window size, OwnerPid / OwnerStart (the writing script)... |
| `request.stale.json` | a request whose script no longer ran when the game read it (never read again) |
| `stop.flag` | asks the runner to stop |
| `results.json` | results written to `OutDir` (with `screens\`, `event-trace.log`) |

## Phases

`WaitTitle` -> `Title` (title screen scenarios, after `TITLE_SETTLE_MS` 8 s) -> `Loading` (load the save, automatically or by the tester; `LOAD_TIMEOUT_SEC` 300; confirms blocking dialogs) -> `WaitGame` -> `Game` (scenarios step by step) -> `Watch` (optional battle watching) -> `Done`.

## What it does

| Hook | What happens |
|---|---|
| `OnLoadClass` | reads the request, creates `OutDir\screens`, starts `EventTraceHelper` (hang finder), turns exception dialogs off, `StoryEvent.NoAutoOpen`, mutes the game |
| `OnMonoUpdate` | every second: exception dialogs kept off; a drama left open more than `DramaTimeoutSec` (5 s) is closed (dramas pause the game timers, so the step runner would stop) |
| `OnTimeUpdate100ms` | the phase machine and the step runner |
| `OnBattleStart`, `OnBattleEndHandler` | battle counting / watching for the scenarios |

Before a step, message boxes left by the previous one are answered; UIs a scenario opened and left are closed; a step that left only the base screen shows its checks in a text panel for the screenshot. Screenshots (`Shot`) and recordings (`Record`) are requests to `run-autotest.ps1` (`ScreenCapture` is stripped from the game); the runner waits `SHOT_WAIT_MS` 900 ms after each.

Slots (`Slot`, from `%LOCALAPPDATA%\ggbh-slotN`): several games can run tests side by side; TP-01 creates the test character "AutoTest Slot<n>" once per slot (one creation at a time).

The runner never saves the game; use a throwaway test save.

## Related

`TimeSkipEvent.NoInterrupt` / `HidePlayer`, `AnimaWeaponEvent.LastUse` / `MonoTicksInBattle`, `BattleModifyEvent.TraceDamage`, `SMGlobalConfigsEvent.ShowTrainer`: hooks used by scenarios.
