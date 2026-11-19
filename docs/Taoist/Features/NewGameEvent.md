# NewGameEvent

`3161035078/ModProject/ModCode/ModMain/Mod/NewGameEvent.cs` - cache key `ModConst.NEW_GAME_EVENT` (Global cache, works everywhere) - group: UI

New game options on the character creation screen: which divine artifacts to start with (Bagua Jade / Pisces Pendant, God Eye, Devil Demon pot: any combination) and "skip the intro and the first battle". The choices are remembered globally for the next new game.

## Saved data (global)

| Property | Meaning |
|---|---|
| `PiscesPendant`, `GodEye`, `DevilDemon` | Start with that artifact |
| `SkipIntro` | Close the intro screens (`IntoGameInfo`, `IntoGameDrama`), so the first battle never starts |

Saved with `CacheHelper.SaveGlobalCache` on every checkbox change.

## UI (`OnOpenUIEnd(CreatePlayer)`)

One line at the height of the START GAME button (`btnOK_En` when shown, else `btnOK`), starting right of the left pillar: header `newgame000`, checkboxes `newgame001` (Bagua Jade), `newgame002` (God Eye), `newgame003` (Devil Demon), `newgame004` (skip intro); hint `newgame005` just below. Items are placed from their measured widths (8 between a box and its label, 36 between items).

## Applying (state machine)

`stage`: `None` -> `NewGame` (set by `OnLoadNewGame`) -> `IntroEnded` -> `None`.

| Event | Stage | What happens |
|---|---|---|
| `OnOpenUIEnd(MapMain)` | not None | `ApplyWeapons()`; if `IntroEnded` -> `None` |
| `OnOpenUIEnd(IntoGameInfo / IntoGameDrama)` | not None, `SkipIntro` | closes the screen 100 ms later (`ModDelayEvent`; closing inside its own open event is unsafe); records the time |
| `OnOpenUIEnd(GodEye / PiscesPendant / DevilDemon)` | `SkipIntro`, < 10 s after a skip | closes it too (the game opens one after the skipped intro) |
| `OnCloseUIEnd(IntoGameDrama)` | not None | `ApplyWeapons()` again (the intro drama may set its own artifact); then `None` if skipped, else `IntroEnded` (applied a last time on the map after the first battle) |

`ApplyWeapons()`: nothing when no box is checked (the game's choice stays); else `GodArtifactHelper.SetAnimaWeapon(selected)` + `UpdateUI()`.

## Notes

- Later map openings (after battles, trainer...) do not touch the artifacts.
- With several artifacts, their map / battle buttons and hotkeys come from [AnimaWeaponEvent](AnimaWeaponEvent.md).

## Tests

TP-01.
