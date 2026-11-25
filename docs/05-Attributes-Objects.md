# ModLib - Attributes, objects and enums

## Attributes

| Attribute | Target | Effect |
|---|---|---|
| `[Cache(id, OrderIndex, CacheType, WorkOn)]` | ModChild / ModEvent | Registers and persists the object (see [02-Cache-Persistence](02-Cache-Persistence.md)) |
| `[ModOrder("_EventOrderIndex.json")]` | ModChild | ModConf JSON `{ "CacheId": index }` overriding OrderIndex of all events of the mod |
| `[EventCondition(IsInGame, IsInBattle, DelayMsec)]` | event method | Run only in/outside a save, in/outside battle (`HandleEnum.True/False/Ignore`), optionally delayed |
| `[ErrorIgnore]` | method/class | No exception dialog; each distinct error is still logged once |
| `[Skill(id, IsCached)]` | ModSkill | Registers a custom skill |
| `[EventCat("Battle")]`, `[ActionCat("Unit")]`, `[EventCatIgn]`, `[ActionCatIgn]` | methods/classes | Categories for the ModCreator event/action editors |

`TraceAttribute`/`TraceIgnoreAttribute` are kept commented out (former Fody method tracing).

## Objects

| Class | Purpose |
|---|---|
| `CachableObject` | Base of persisted objects: `ModId`, `CacheId`, `OrderIndex`, `CacheType`, `WorkOn`, `Parent` (ModChild), `OnLoadClass`, `OnCacheHandler`, `OnUnloadClass` |
| `EnumObject` | "Smart enum": static properties hold instances with a `Value` and extra data; `GetAllEnums<T>()`, `GetEnumByVal/ByName/ByProp<T>()`. Static constructors are run at `OnInitEObj` |
| `Gamevar` | Per-save ModLib variables (load flags, `CurMonth`) |
| `ParameterStore` | Monthly snapshot: `WUnits`, `Buildings`, `Towns`, `Schools` |
| `ModConfigs` | ModLib `mod_configs.json` (`DebugMode`) |
| `MultiValue` | Serializable `object[]` tuple (`Value0..2`, `Get(i)`) used in data tables |
| `DelayInfo` | Queued delayed call |
| `MonoUpdater` | Il2Cpp MonoBehaviour calling `OnMonoUpdate` every frame |
| `WebClientWithTimeout` | WebClient with timeout (`HttpHelper`) |

## Enums

* Unit data: `UnitPropertyEnum` (saved properties: `GradeID`, `Exp`, `Age`, `Talent`, basis...), `UnitDynPropertyEnum` (final DynInt values: `Attack`, `Defense`, `HpMax`, `BasisFire`...), conversion `GetPropertyEnum()` / `GetDynPropertyEnum()`; both are found by their property name: `GetEnumByVal<UnitDynPropertyEnum>("basisFire")` (`UnitDynPropertyEnum` since 2.3.00). Read/write with `wunit.GetProperty<T>(e)`, `SetProperty`, `AddProperty`, `GetDynProperty(e).value`.
* Grades: `GradeEnum`, `GradePhaseEnum`, `GradePhaseUpEnum`, `BottleneckEnum`.
* Lucks: `LuckTypeEnum`, `LuckRarityEnum`, `LuckConcealEnum`, `LuckDurStackTypeEnum`, `AddLuckOptions`.
* Misc: `UnitTraitEnum`, `PillTypeEnum`, `EffectChangeTypeEnum`, `MathLogicOptions` (+ - * / as data), `MsgBoxButtonEnum`, `HandleEnum`, `CheckEnum`, `DebugModeEnum`.
