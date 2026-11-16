# ModLib - Architecture

ModLib (Workshop `3385996759`, mod id `flQrxq`, namespace `ModLib` + `MOD_flQrxq`) is a framework that sits between the game (Unity Il2Cpp + MelonLoader/Harmony) and content mods such as Taoist. It turns the game's native timers and events into a single, ordered, persisted event model.

Related docs: [01-Event-Reference](01-Event-Reference.md) · [02-Cache-Persistence](02-Cache-Persistence.md) · [03-Builtin-Events](03-Builtin-Events.md) · [04-UI-Framework](04-UI-Framework.md) · [05-Attributes-Objects](05-Attributes-Objects.md) · [../05-Helpers.md](../05-Helpers.md)

## Source layout

| Folder (`3385996759/ModProject/ModCode/`) | Content |
|---|---|
| `ModMain/ModMain.cs` | `MOD_flQrxq.ModMain : ModMaster` - the concrete core object (name, id, version) |
| `ModLib/Mod/Base/` | `ModMaster` (partial: `ModMaster.cs`, `_GameEvent`, `_CommonEvent`, `_BattleEvent`, `_MapEvent`, `_TimerEvent`), `ModEvent`, `ModChild`, `ModSkill`, marker bases `BattleEvent`, `SkillEvent` |
| `ModLib/Mod/` | Built-in events: `ModDelayEvent`, `ModUIEvent`, `ModSkillEvent`, `ModTitleEvent`, `ModBattleEvent` |
| `ModLib/Object/` | `CachableObject`, `EnumObject`, `Gamevar`, `ParameterStore`, `ModConfigs`, `MultiValue`, `DelayInfo`, `MonoUpdater`, `UI/*` |
| `ModLib/Helper/` | Extension/helper classes (units, items, configs, cache, UI, debug...) - see `05-Helpers.md` |
| `ModLib/Attribute/` | `Cache`, `ModOrder`, `EventCondition`, `ErrorIgnore`, `Skill`, `EventCat`/`ActionCat` (+ `Ign`) |
| `ModLib/Enum/` | `EnumObject` based enums (`UnitPropertyEnum`, `UnitDynPropertyEnum`, `GradePhaseEnum`...) and plain enums |
| `ModLib/Converter/` | `EnumObjectConverter` (Json.NET) |

## Runtime objects

```mermaid
flowchart LR
    Game["Game engine<br/>g.timer / g.events / Update()"] -->|native callbacks| MM["ModMaster (ModLib)<br/>_OnXxx -> CallEvents -> OnXxx"]
    MM -->|EventHelper.RunMinorEvents| EV["ModEvent instances<br/>(ModLib built-ins + every mod)"]
    MM --> CH["CacheHelper.CacheData<br/>(ordered CachableObjects)"]
    CH --> EV
    CH --> MC["ModChild of each mod<br/>(e.g. Taoist ModMain)"]
    EV -->|EventCondition delay| DE["ModDelayEvent queue"]
    CH <-->|JSON| Files["mod/<modId>/globals/*.json<br/>mod/<modId>/saves/<playerId>/*.json"]
```

* **ModMaster** - one per game process. `Init()` patches Harmony, registers 5 timers (10/100/200/500/1000 ms), the map/game/battle events (`EMapType`, `EGameType`, `EBattleType`) and a `MonoUpdater` (per-frame `OnMonoUpdate`).
* **CachableObject** - anything ModLib creates by reflection and persists as JSON: every `ModChild` and `ModEvent` declared with `[Cache("id")]` in ModLib and in all loaded mods.
* **ModChild** - exactly one per mod (CacheId = mod id). Created first (order -1, Global, WorkOn All).
* **ModEvent** - feature classes; override `OnXxx` methods. Execution order = `OrderIndex` (from `[Cache]` or the mod's `[ModOrder]` file).

## Two-layer dispatch

1. Native callback `ModMaster._OnXxx(ETypeData e)` (registered in `Init`) calls `CallEvents<T>("OnXxx", e)`: reflection call of `ModMaster.OnXxx`, casting the Il2Cpp event data to the concrete type `T`, measuring time (`DebugMode >= Finest`) and logging exceptions.
2. `ModMaster.OnXxx` does framework work when needed (load sequence, save, monthly tick...) and then calls `EventHelper.RunMinorEvents("OnXxx", e)`, which iterates the cached `ModEvent`s **that declare** the method, in cache order, and for each:
   * checks `[EventCondition(IsInGame, IsInBattle)]`,
   * defers through `ModDelayEvent` when `DelayMsec > 0`,
   * invokes the method and logs exceptions (with the in-game dialog in debug mode; `[ErrorIgnore]`: logged once per distinct error, no dialog).

`WorkOn` filtering is done by the cache itself: Global-only objects are removed when a save is loaded (`CacheHelper.RemoveUnuseGlobalCaches`), Local objects only exist inside a save.

## Load sequence

```mermaid
sequenceDiagram
    participant G as Game
    participant M as ModMaster
    participant C as CacheHelper
    participant E as ModEvents
    G->>M: Init() (mod DLL loaded)
    G->>M: first OpenUIEnd (any UI)
    M->>M: OnInitMod (mod_configs.json, UI samples)
    M->>M: OnInitConf (copy imgs/confs, load ModConf of every mod)
    M->>M: OnInitEObj (static ctors of EnumObject types)
    M->>C: OnLoadGlobals -> LoadGlobalCaches (ModChild + Global events, OnLoadClass)
    Note over E: title screen events run (WorkOn All/Global)
    G->>M: OpenUIEnd(MapMain) in a save
    M->>M: OnLoadGameVariables (Gamevar)
    M->>C: OnLoadGameCaches: ParameterStore, LoadGameCaches (Local), remove Global-only, Order()
    M->>E: OnLoadNewGame (new save only) -> OnLoadGameBefore -> OnLoadGame -> OnLoadGameAfter
```

`OnLoadClass(isNew, modId, attr)` runs right after an object is created (`isNew = true`) or deserialized from its cache file; returning `false` from `OnCacheHandler()` skips the object entirely (used for features switched off by configuration).

## Monthly tick and save

`_OnWorldRunEnd` (end of each world step) detects a month change through `Gamevar.CurMonth` and then:

1. `RefreshParameterStore()` - snapshot of units (with a valid area), buildings, towns, schools; ModChild overrides cleared.
2. `OnRefreshParameterStoreOnMonthly` - mods can narrow the snapshot (`ModChild.SetParameterStore`, e.g. Taoist `PrepareEvent`).
3. `OnFirstMonth` (once per save), `OnMonthly`, `OnMonthlyForEachWUnit(wunit)` for every unit of the store, `OnYearly` every 12 months.
4. `OnWorldRunEnd`, then `GameHelper.SaveGame()`.

`OnSave` (game save) dispatches to events, then writes `Gamevar`, all caches (`CacheHelper.Save`) and the log (`DebugHelper.Save`).

## Logs and files

* Logs: `%USERPROFILE%\AppData\LocalLow\guigugame\guigubahuang\mod\flQrxq\debug-YYYYMMDD.log` and `<playerId>_debug-YYYYMMDD.log` (see [06-Debug](../06-Debug.md)).
* Cache files: `mod\<modId>\globals\<CacheId>.json` (Global) and `mod\<modId>\saves\<playerId>\<CacheId>.json` (Local); skills in the skill cache folder.
* `ModConf/mod_configs.json` of ModLib: `{ "DebugMode": "Info" }` (`Info`, `Fine` = log each event start, `Finest` = also durations).
