# TaoistLogEvent

`3161035078/ModProject/ModCode/ModMain/Mod/TaoistLogEvent.cs` - cache key `ModConst.TAOIST_LOG_EVENT` - group: UI

The Taoist log of a save: what the mod did to the player month by month (taxes, fees, market deals, buy orders, loans, encounters, stalkers, storylines, refine rerolls...). Entries keep a LocalText key and its arguments, so they are shown in the current language.

## Saved data

| Property | Meaning |
|---|---|
| `Entries` | `List<Entry>`: `Year`, `Month`, `Key` (LocalText key), `Args` (format arguments). At most 300 (`MAX_ENTRIES`): the oldest are dropped. |

## API (used by the other features)

- `Log(key, params args)`: adds an entry dated with the current game month. No-op outside a save or before the event exists.
- `LogIf(cond, key, args)`.
- `OfMonth(year, month)`: entries of a month.
- `Icon(key)`: icon sprite by key prefix:

| Prefix | Icon | Prefix | Icon |
|---|---|---|---|
| `log_tax` | zm_fanrongdu | `log_family` | zm_renshu |
| `log_storage`, `log_loan`, `log_market`, `market_` | zm_lingshi | `log_secl` | wuxing |
| `log_stalker` | shanbiD | `log_story` | daodian |
| `log_buyorder` | yaocai | `log_wanted` | gongji |
| `log_town` | zm_andingdu | `log_team` | shengwang |
| `log_refine` | lianqi | `log_score` | daoli |
| `enc_` and others | xingyun | | |

## Display

- `Entry.RenderText()`: `LS(key)` formatted with the args (a bad format falls back to the plain text). `Render()`: icon + `log_line` ("Year y, Month m: text").
- **Log window** `OpenLog()` (opened from Taoist Settings, [UISettingScreenEvent](UISettingScreenEvent.md)): `UICustom1` titled `log_title`, newest first, 22 rows a page, two columns (date `log_date` in dark red, icon + text), `<` / `>` buttons and "page / pages"; `log_empty` when empty.
- **Month log of the game** (`OnOpenUIEnd(MonthLog)`): a text block to the right of the window's close button (440 x 600, wraps, grows down): bold `log_title` then the Taoist lines of the month the window shows (`curMonthIndex` -> year / month; current month if unknown), or `log_empty`. Refreshed live as the player browses months.

## Tests

TG-27.
