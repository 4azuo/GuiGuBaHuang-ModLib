# UISettingScreenEvent

`3161035078/ModProject/ModCode/ModMain/Mod/UISettingScreenEvent.cs` - cache key `ModConst.UI_SETTING_SCREEN_EVENT` (per save) - group: UI

The "Taoist Settings" window, opened from a button of the game settings.

## Saved data

| Property | Default | Meaning |
|---|---|---|
| `HideDamageMultiplier` | false | hide the damage multiplier text in battles ([BattleModifyEvent](BattleModifyEvent.md)) |
| `HideExceptionLogs` | false | `DebugHelper.IsDebugMode = !HideExceptionLogs` (no exception log lines) |
| `NotifyMarketOffers` | true | message for new offers / filled buy orders ([RealMarketEvent3](RealMarketEvent3.md)) |

## Flow

| Hook | What happens |
|---|---|
| `OnLoadGame` | applies `HideExceptionLogs` |
| `OnOpenUIEnd(GameSetting)` | button "Taoist Settings" (`setting000`, styled like the system OK button) -> `OpenTaoistSettings()` |

## Window (`UICustom1`)

| Section | Content |
|---|---|
| Display (`setting010`) | toggle hide damage multiplier (`setting002`); toggle hide exception logs (`setting003`); note `setting004` |
| Game (`setting011`) | toggle market notifications (`setting012`); toggle random encounters (`setting014`, `EncounterEvent.Enabled`); buttons "Taoist log" (`setting013` -> `TaoistLogEvent.OpenLog`) and "Score records" (`setting016` -> `SMScoreEvent.OpenRecords`) |
| Divine artifact hotkeys (`setting005`) | help text `setting006` (wrapped at 110 characters), then a binding row per artifact (`AnimaWeaponEvent.AddBindingRows`) |
| Other hotkeys (`setting015`) | the damage calculation row (default F9) |
| footer | red note `setting001` |

Toggles write the value at once (saved with the save). The hotkeys are global ([AnimaWeaponEvent](AnimaWeaponEvent.md)).

## Tests

TG-27, TM-27.
