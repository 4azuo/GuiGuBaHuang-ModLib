# SMLocalConfigsEvent

`3161035078/ModProject/ModCode/ModMain/Mod/SMLocalConfigsEvent.cs` - cache key `ModConst.SM_LOCAL_CONFIGS_EVENT` - group: Core

Per-save copy of the S&M configs. Almost every Taoist feature reads its options from `SMLocalConfigsEvent.Instance.Configs`, never from the global configs, so changing the S&M window on the title screen does not change existing saves.

## Saved data

| Property | Type | Meaning |
|---|---|---|
| `Configs` | `SMGlobalConfigsEvent` | Snapshot of every S&M option (see [SMGlobalConfigsEvent](SMGlobalConfigsEvent.md)) |

## Flow

| Hook | What happens |
|---|---|
| `OnLoadClass(isNew, ...)` | New save, or a save without a copy (older save, broken file): reads the global cache file `SM_GLOBAL_CONFIGS_EVENT` with `CacheHelper.ReadGlobalCacheFile`. A read error is logged and replaced by default configs (`new SMGlobalConfigsEvent()`). |
| `OnLoadGame` | Applies `AddLevelupExpRate` to the level-up exp table: every `g.conf.roleGrade` row gets `exp = Calculate(exp, AddLevelupExpRate)`. |

## API

- `Calculate(bas, addRate) = bas + bas x addRate`: the helper every feature uses to apply an S&M rate (e.g. `AddNpcGrowRate`, `AddTaxRate`, `AddBankFee`).

## Notes

- The level-up exp is changed in the game config table in memory, once per game load. Because the table is reloaded with the game, the rate is not applied twice.
- A save created before an option existed gets that option's default value from the class initializer.

## Tests

TG-02.
