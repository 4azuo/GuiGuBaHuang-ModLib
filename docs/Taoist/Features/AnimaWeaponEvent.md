# AnimaWeaponEvent

`3161035078/ModProject/ModCode/ModMain/Mod/AnimaWeaponEvent.cs` - cache key `ModConst.ANIMA_WEAPON_EVENT` (Global cache, works everywhere) - group: UI

Several divine artifacts at once (from [NewGameEvent](NewGameEvent.md), the trainer...): the game draws all their buttons at the same place and has one artifact key only. This event lays the buttons side by side and gives each artifact its own hotkey. It also holds the hotkey of the damage calculation window ([DamageInfo](DamageInfo.md)).

## Saved data (global, all saves)

| Property | Meaning |
|---|---|
| `PiscesPendantKeyCode` | Hotkey of the Bagua Jade (Pisces Pendant) |
| `GodEyeKeyCode` | Hotkey of the God Eye |
| `DevilDemonKeyCode` | Hotkey of the Devil Demon pot |
| `DamageInfoKeyCode` | Hotkey of the damage window (default `DamageInfo.DEFAULT_KEY` = F9) |

Stored as int `KeyCode` values (the class must load outside the game for unit tests).

## Bindings

`BINDINGS` = the 3 artifacts + a pseudo row `DamageInfoRow` (`(GameAnimaWeapon)1000`).

Rows are added to a Taoist window by `AddBindingRows(ui, row, bindings)` ([UISettingScreenEvent](UISettingScreenEvent.md): Settings > Bindings, combat / map lists; Taoist Settings > Other hotkeys). With the game settings open, a row is a copy of a game key-binding row (same look); otherwise a plain button "Name: key".

Capture: click a row -> `Capturing = w` ("..." shown, toggle on) -> next key press (`OnMonoUpdate`, any key except mouse 0-2 and joystick):

| Key | Result |
|---|---|
| Esc | cancelled |
| Backspace / Delete | cleared (None) |
| a key bound by the game (map or battle, `GameKeyUsing`) | refused, message `setting008` |
| a key of another Taoist binding | moved to this one |

Saved at once (`SaveGlobalCache`). Closing `GameSetting` / `TextInfoLong` cancels the capture.

## Flow

| Hook | What happens |
|---|---|
| `OnOpenUIEnd(MapMain / BattleInfo)` | `Arrange()` 200 ms later. |
| `OnTimeUpdate1000ms` | `Arrange()` (the game shows/hides buttons by itself). |
| `OnMonoUpdate` | Capture, then (in game, not typing in an input field) the hotkeys: `DamageInfo.Watch()` every frame, `Trigger(w)` on key down. |

`Trigger(w)`:

- **Damage row**: only in battle -> `DamageInfo.Toggle()`.
- **Battle**: the battle has started, the player is alive, the artifact's battle button is active -> `inputCtrl.UsePiscesPendant / UseGodEye / UseDevilDemon` (the game checks cooldown and charges).
- **Map** (not loading / mask screen): clicks the artifact's map button (opens its window).

`LastUse` ("map:X" / "battle:X") and `MonoTicksInBattle` are read by the autotest.

## Layout (`Arrange`)

Map (`uiPlayerInfo.btnPiscesPendant / btnGodEye / btnDevilDemon`) and battle (`uiInfo.goPiscesPendant / goGodEye / goDevilDemon`): the original position of each button is remembered; active buttons are placed from there to the right, `1.05 x` the widest button apart (60 if unknown); inactive ones go back. Each button's key label shows its own hotkey (kept as the game's when none is set).

## Notes

The game's own artifact key keeps working.

## Tests

TB-16.
