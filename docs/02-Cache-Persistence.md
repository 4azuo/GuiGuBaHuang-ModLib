# ModLib - Cache and persistence

Every `ModChild`/`ModEvent` is a `CachableObject`: ModLib discovers it by reflection, creates (or deserializes) it, keeps it in `CacheHelper.CacheData` and saves its **public properties** as JSON. Mark runtime-only members `[JsonIgnore]` (or keep them private fields).

## Declaring

```csharp
[Cache("MyFeature", OrderIndex = 2000, CacheType = CacheAttribute.CType.Local, WorkOn = CacheAttribute.WType.Local)]
public class MyFeatureEvent : ModEvent
{
    public static MyFeatureEvent Instance { get; set; }      // assigned automatically in OnLoadClass
    public Dictionary<string, int> Counter { get; set; } = new Dictionary<string, int>(); // saved
    [JsonIgnore] public List<UnitCtrlBase> Temp { get; } = new List<UnitCtrlBase>();     // not saved
}
```

| Property | Values | Meaning |
|---|---|---|
| `CacheId` | string | Unique per mod; file name of the cache |
| `CacheType` | `Local` (default) / `Global` | Local: created when a save is loaded, saved per save. Global: created on the title screen, saved globally |
| `WorkOn` | `All` (default) / `Global` / `Local` | Global: removed when a save is loaded (title screen only). Local: in save only |
| `OrderIndex` | int > 0 | Execution order. 0 = automatic (9000+). Overridden by the mod's `[ModOrder("file.json")]` mapping `CacheId -> index` |

ModChild rules (checked in `CacheHelper.GetModChildCacheTypes`, the mod is ignored with a log line otherwise): exactly one ModChild per mod, its CacheId equals the mod id, no duplicated CacheIds, no negative OrderIndex.

## Discovery and order

`CacheHelper.GetCacheTypes()` = ModLib types (ordered by OrderIndex) + for each loaded mod (`g.mod.allModPaths`) its ModChild and events. `CacheHelper.Order()` sorts the loaded objects by that list, which is the dispatch order of `RunMinorEvents`.

## Files

| Kind | Path (`%USERPROFILE%\AppData\LocalLow\guigugame\guigubahuang\mod\<modId>\`) |
|---|---|
| Global | `globals\<CacheId>.json` |
| Local | `saves\<playerUnitId>\<CacheId>.json` |
| Skill (`[Skill(IsCached = true)]`) | skill cache folder (`CacheHelper.GetSkillCacheFilePath`) |
| Game variables | `Gamevar` (load flags, `CurMonth`) |

Serializer: `CacheHelper.JSON_SETTINGS` (type names, error tolerant). `EnumObject` values are written as `"TypeFullName|Name"` by `EnumObjectConverter` and resolved back to the singleton.

### Writing (2.2.36)

`CacheHelper.Save()` (every game save, also the monthly one) serializes every object on the game thread, then hands the text to `CacheHelper.Writer` (`FileWriteQueue`, `Helper/FileWriteQueue.cs`):

* **Unchanged files skipped**: a file whose JSON equals the one last written to it, and still on disk as written (same size and time), is not written again. A file changed or deleted by someone else is written again. The log gets one line: `Save caches: <n> changed (written in the background), <m> unchanged`.
* **Background write**: the changed files are written by a worker thread (only the disk write: reading game objects off the game thread is not safe). Only the newest text of a file waits in the queue.
* **Safe write**: each file goes to `<file>.tmp` first, then replaces the file (`File.Replace`, `File.Copy` when Replace fails): a failed or interrupted write never leaves a half-written file.
* **Failure**: logged (`Save cache failed: ...`) and forgotten, so the next save writes the file again.
* **Waiting**: `CacheHelper.FlushWrites()` waits for the queue; done before loading the caches (`LoadGlobalCaches`, `LoadGameCaches`) and reading a cache file (`ReadGlobalCacheFile`, `ReadGameCacheFile`). The worker is a foreground thread: the game process does not exit before the queue is written. Left risk: the game killed during the write (the files then hold the previous save; never a half file).
* `SaveGlobalCache(obj)` / `SaveGameCache(obj)` (one object, on demand) always write and wait for the write.

## Useful calls

| Call | Purpose |
|---|---|
| `EventHelper.GetEvent<T>(cacheId)` / `CacheHelper.GetCachableObject<T>(id)` | Find a loaded object |
| `CacheHelper.ReadGlobalCacheFile<T>(modId, cacheId)` | Read a Global object's file from a Local object (Taoist copies S&M configs this way) |
| `CacheHelper.SaveGlobalCache(obj)` | Save a Global object immediately (e.g. after a title screen setting change; waits for the write) |
| `CacheHelper.FlushWrites()` | Wait for the cache files of the last save (before reading or copying them yourself) |
| `ModMaster.ModObj.ParameterStore` / `GetModChildParameterStore()` | Units/buildings snapshot of the month |

## Data of deleted saves

The game names a save folder `<game data>\<platform>\CacheData\<slot>_<key>`; the key is **not** the player unit id that ModLib and the mods use (`mod\<modId>\saves\<id>\`, `mod\<modId>\<id>_configs.json`, `mod\<modId>\<id>_debug-*.log`). So:

- `OnLoadGame` records the save folder of the loaded save (`g.cache.curCache.data.folder`) and the player unit id in `mod\flQrxq\save-index.json` (`SaveCleanupHelper.Register`).
- At startup (`OnLoadGlobals`) the folders of `g.cache.allCache` are compared with the index: a player id whose recorded folders are all gone loses its data in every mod folder (saves folder, configs, debug logs) and leaves the index (`SaveCleanupHelper.CleanDeletedSaves`). Ids never recorded (saves not loaded since this version) and global files are kept; with an empty save list nothing is removed.
- Off with `"CleanOrphanSaves": false` in ModLib `ModConf/mod_configs.json`. Unit tests: `tests/UnitTests/ModLib/SaveCleanupHelperTests.cs`; in game: TP-01 checks the index entry of the new save.