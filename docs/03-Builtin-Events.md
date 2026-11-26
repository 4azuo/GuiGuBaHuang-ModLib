# ModLib - Built-in events

ModLib ships its own `ModEvent`s. They run before/among mod events according to their OrderIndex.

| Class | CacheId | Order | Type / WorkOn | Purpose |
|---|---|---|---|---|
| `ModDelayEvent` | `$DELAY$` | 1 | Global / All | Delayed calls. Every 10 ms executes queued `DelayInfo`s whose delay elapsed. API: `DelayEvent(ev, methodName/MethodInfo, param, ms)`, `DelayEvent(ev, Action, ms)`. Used by `[EventCondition(DelayMsec)]` |
| `ModUIEvent` | `$UI$` | 10 | Global / All | Every 100 ms refreshes custom UIs with `IsAutoUpdate = true` (`UIHelper.UpdateAllUI`); mod editor: hides the Workshop button of a private project and closes its upload window (`PrivateModHelper`) |
| `ModSkillEvent` | `$SKILL$` | 80 | Local / Local | Hosts `ModSkill` classes of all mods: loads them (from skill cache when `IsCached`), `OnLoadBattleStart` at battle start, `OnCast(unit, skill, step, prop)` on every skill/step/prop use (parallel), `OnUnloadBattleEnd` at battle end |
| `ModTitleEvent` | `$TITLES$` | 90 | Global / Global | Title screen: "Powered by ModLib" button and the language panel in the frame of the game's DLC buttons (a copy without the DLC icon, the "Claimed" mark and the game's scripts: `GetTitleFrameButton`), mod wallpapers (`WallpaperHelper`, one file `ModConf/*_wallpaper.json` per wallpaper: added to the Change Theme window with the toggle "hide the game's title" above the list, the chosen one or the startup draw shown on the title screen; [guide/08-Wallpapers](guide/08-Wallpapers.md)), translation language selector (de, es, fr, ja, la, ru, vi, th; reloads LocalText of all mods), ModCreator tools window (duplicated config ids: red = defined by several mods, blue = replaces a base id), screen ratio warning (16:9 required), a key button per licensed mod without a valid key (`LicenseHelper`, [guide/07](guide/07-Build-Deploy.md#licensed-mods-supporter-keys)) |
| `ModBattleEvent` | `$BATTLE$` | 100 | Local / Local | Battle state for everyone: `SceneBattle`, `PlayerUnit`, `BattleUnits`, `BattleMonsters`, `Town`/`School` at the player position, `AttackingUnit`/`HitUnit` (+ world units) of the current hit, `KillList`/`GetKiller`, battle/stay/room time, damage statistics |

## ModBattleEvent damage statistics

Keys `"<DmgEnum>_<DmgTypeEnum>"`, e.g. `DmgDealt_Damage`, `DmgRecv_Fire`:

* `DmgEnum`: `DmgDealt` (player hits), `DmgRecv` (player is hit).
* `DmgTypeEnum`: `Damage` (total), `Physic`/`Magic`, weapon bases `Blade, Spear, Sword, Fist, Palm, Finger`, elements `Fire, Frozen, Thunder, Wind, Earth, Wood`.
* `GameDmg` (saved, whole save) and `BattleDmg` (current battle). Read with `ModBattleEvent.sGetDmg(DmgSaveEnum.Global|Local, key)`.
* `GetHighestDealtDmgTypeEnum()` / `GetHighestRecvDmgTypeEnum()`: basis with the most damage this battle; `GetDmgPropertyEnum(type)` maps it to `UnitDynPropertyEnum.BasisXxx`.

## ModSkill

```csharp
[Skill("MySkill", IsCached = true)]
public class MySkill : ModSkill
{
    public int Casts { get; set; }                       // saved when IsCached
    public override void OnCast(UnitCtrlBase cunit, SkillBase skill, StepBase step, PropItemBase prop)
    {
        if (skill != null && cunit.IsPlayer()) Casts++;  // called in parallel: keep it thread-safe
    }
}
```
