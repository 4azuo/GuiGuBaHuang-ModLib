using EBattleTypeData;
using EGameTypeData;
using ModLib.Object;
using Newtonsoft.Json;

namespace ModLib.Mod
{
    [Cache("$SKILL_MASTER$", OrderIndex = 200, CacheType = CacheAttribute.CType.Local, WorkOn = CacheAttribute.WType.Local)]
    public abstract class BattleEvent : ModEvent
    {
    }
}
