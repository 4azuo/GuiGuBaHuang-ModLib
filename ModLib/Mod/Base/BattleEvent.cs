using EBattleTypeData;
using EGameTypeData;
using ModLib.Object;
using Newtonsoft.Json;

namespace ModLib.Mod
{
    /// <summary>
    /// Base class for battle-only events (semantic marker, same API as ModEvent).
    /// </summary>
    public abstract class BattleEvent : ModEvent
    {
    }
}
