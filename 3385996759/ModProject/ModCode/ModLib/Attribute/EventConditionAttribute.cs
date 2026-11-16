using ModLib.Enum;
using System;

namespace ModLib.Attributes
{
    /// <summary>
    /// Run conditions for an event method: only in game / outside game, only in battle / outside battle,
    /// and an optional delay (DelayMsec) before execution through ModDelayEvent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class EventConditionAttribute : Attribute
    {
        public HandleEnum IsInGame { get; set; } = HandleEnum.Ignore;
        public HandleEnum IsInBattle { get; set; } = HandleEnum.Ignore;
        public int DelayMsec { get; set; } = 0;
    }
}