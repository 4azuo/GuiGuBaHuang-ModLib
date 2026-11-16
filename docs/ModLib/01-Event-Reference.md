# ModLib - Event reference

All methods are `public virtual` on `ModEvent` (override them in your event, call `base.` first). Parameters are the concrete Il2Cpp types of `EGameTypeData` / `EBattleTypeData`; the last parameter is also stored in `EventParameter`.

Legend - **Where**: T = title screen, G = in a save (map), B = in battle. Actual execution also depends on the event's `WorkOn` and `[EventCondition]`.

## Lifecycle (dispatched by ModMaster)

| Method | When | Where |
|---|---|---|
| `OnLoadClass(isNew, modId, attr)` | Object created/deserialized (from `CachableObject`) | T/G |
| `OnCacheHandler()` | Right after `OnLoadClass`; return `false` to not load the object | T/G |
| `OnUnloadClass()` | Object removed from the cache | T/G |
| `OnLoadNewGame()` | First MapMain of a brand new save | G |
| `OnLoadGameBefore()` / `OnLoadGame()` / `OnLoadGameAfter()` | First MapMain after loading a save (in this order) | G |
| `OnRefreshParameterStoreOnMonthly()` | New month, after the ParameterStore refresh | G |
| `OnFirstMonth()` | First month change of the save (once) | G |
| `OnMonthly()` | Every month | G |
| `OnMonthlyForEachWUnit(WorldUnitBase)` | Every month, once per unit of the ParameterStore | G |
| `OnYearly()` | Every 12 months | G |
| `OnSave(ETypeData)` | Game save (in a save only) | G |

## Timers

| Method | Period |
|---|---|
| `OnTimeUpdate10ms` / `100ms` / `200ms` / `500ms` / `1000ms` | 10 ms ... 1 s (game timers, also on the title screen) |
| `OnMonoUpdate()` | Every Unity frame (`MonoUpdater.Update`) - use for keyboard input |

## Game (`EGameType`)

| Method | Event |
|---|---|
| `OnOpenUIStart(OpenUIStart)` / `OnOpenUIEnd(OpenUIEnd)` | A UI starts/finished opening (`e.uiType.uiName`, `e.ui`) - main hook to inject UI |
| `OnCloseUIStart(CloseUIStart)` / `OnCloseUIEnd(CloseUIEnd)` | A UI starts/finished closing |
| `OnInitWorld(ETypeData)` | World creation |
| `OnLoadSceneStart(LoadSceneStart)` / `OnLoadScene(LoadScene)` | Scene loading |
| `OnIntoWorld(ETypeData)` | Entering the world |
| `OnOpenDrama(OpenDrama)` / `OnOpenNPCInfoUI(OpenNPCInfoUI)` | Drama / NPC info opened |
| `OnTaskAdd` / `OnTaskComplete` / `OnTaskFail` / `OnTaskGive` / `OnTaskOverl` | Task changes |
| `OnUnitSetGrade(ETypeData)` / `OnUnitSetHeartState(ETypeData)` | Unit grade / heart state changed |
| `OnWorldRunStart(WorldRunStart)` / `OnWorldRunEnd(WorldRunEnd)` | World step start/end (month tick is computed at the end) |

## Map (`EMapType`)

`OnPlayerOpenTreeVault` (town/sect storage), `OnPlayerEquipCloth`, `OnPlayerInMonstArea`, `OnPlayerRoleEscapeInMap`, `OnPlayerRoleUpGradeBig` (major breakthrough), `OnUpGradeAndCloseFateFeatureUI`, `OnUseHobbyProps` - all `(ETypeData e)`.

## Battle (`EBattleType`)

| Method | Event |
|---|---|
| `OnBattleStart(ETypeData)` | Battle started (ModMaster logs dungeon/room details) |
| `OnBattleUnitInit(UnitInit)` | Battle unit initialized |
| `OnBattleSetUnitType(SetUnitType)` | Unit type set |
| `OnBattleUnitInto(UnitCtrlBase)` | **ModLib event**: first `SetUnitType` of a unit in this battle = the unit entered the battle. Best place to change unit stats |
| `OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler)` | Before damage is applied - change `e.dynV.baseValue` to change the damage |
| `OnBattleUnitShieldHitDynIntHandler(UnitShieldHitDynIntHandler)` | Same for shield damage |
| `OnBattleUnitHit(UnitHit)` | Damage applied |
| `OnBattleUnitUseSkill` / `OnBattleUnitUseStep` / `OnBattleUnitUseProp` | Skill / movement step / item used |
| `OnBattleUnitAddEffectStart` / `OnBattleUnitAddEffect` | Effect being added / added |
| `OnBattleUnitAddHP(UnitAddHP)` / `OnBattleUnitUpdateProperty(UnitUpdateProperty)` | HP change / property update |
| `OnBattleUnitDie(UnitDie)` / `OnBattleUnitDieEnd(ETypeData)` | Unit died / death processed |
| `OnBattleEndFront(ETypeData)` | Before end processing |
| `OnBattleEndHandler(BattleEndHandler)` | End processing (`e.isWin`, drops still editable) |
| `OnBattleEnd(BattleEnd)` | Battle ended |
| `OnBattleEscapeFailed(ETypeData)` / `OnBattleExit(ETypeData)` | Escape failed / battle scene exited |

## Conditions, delays and errors

```csharp
[EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False, DelayMsec = 500)]
public override void OnOpenUIEnd(OpenUIEnd e) { ... }   // map UIs only, 0.5 s after opening

[ErrorIgnore]   // no exception dialog, each distinct error logged once (polling handlers that may hit closed UIs)
public override void OnTimeUpdate200ms() { ... }
```

Ad-hoc delays: `ModDelayEvent.Instance.DelayEvent(this, () => ..., 1000)`.
