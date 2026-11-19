# Known issues

Open issues of ModLib and Taoist. Fixed issues are removed from this file (cleared on 2026-11-19: BUG-01 .. BUG-21 and the fixes found while testing; their history is in git, `git log -p -- docs/Known-Issues.md`). Fixes of behaviour found while documenting the features are listed in [Taoist/Features/README.md](Taoist/Features/README.md).

Status: `Open` (to check) · `Confirmed` · `Fixed` · `Won't fix`. Severity: High = wrong behaviour players will see, Medium = feature partly broken, Low = robustness / cosmetic.
`Test` = unit test reproducing the issue (`tests/UnitTests/KnownIssues.cs`, run with `scripts\run-unittests.ps1 -IncludeKnownIssues`); when an issue is fixed, move its test to the normal tests, remove it from the allow-lists in `KnownIssues` and remove the row here. New ids continue from BUG-23.

| Id | Severity | Status | Where | Issue | Suggested fix | Test |
|---|---|---|---|---|---|---|
| BUG-22 | Medium | Open | `3161035078/.../Mod/SeclusionEvent.cs` (`Kind.Garden`) / `TimeSkipEvent.cs` | The game froze in the garden seclusion of storyline E (TS-05 step 09, "Time passes... 1 / 12 months", vitality 0) when TS-05 ran after TR-02 in the same run (promotion run 20261119-095223: the character raised to Origin Spirit, married with children, town master with guardians); TS-05 alone passes (~1 min for the step). Not reproduced yet | Reproduce with `run-autotest.ps1 -Scenario TR-02,TS-05 -ConfirmTools`; check the month run of the skip (`TimeSkipEvent`) with the TR-02 world | - |

## Limitations (not bugs)

* **Games side by side**: every game slot has its own game data folder (`...\LocalLow\guigugame\guigubahuang-slot<n>`,
  plugin `tools/SlotDataPath` installed in the slots only; ModLib reads its data folder from
  `Application.persistentDataPath`), and the parallel scripts use the slots 2.. only, never the normal game (before
  2026-11-18 the slots shared the save index and lost saves). Left: a slot game reads the usual save index once at start
  and writes it back unchanged, before the plugin moves the game's paths (first frame) - harmless while no normal game
  runs at the same time.
* `ModStts` (and every type using Il2Cpp enums such as `MartialType`, `MonstType`) cannot be loaded outside the game (`TypeLoadException: Method in an Enum`), so `mod_configs.json` is validated by structure in unit tests and by `TG-01` in the in-game autotest.
* Battle behaviour (damage pipeline, NPCs joining battles) can only be verified in game: manual scenarios TM-21/TM-27 and the battle watch scenarios (`-WatchBattles`).
