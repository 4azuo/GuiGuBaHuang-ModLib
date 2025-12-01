using ModLib.Enum;
using System;

namespace ModLib.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class EventConditionAttribute : Attribute
    {
        public HandleEnum IsInGame { get; set; } = HandleEnum.Ignore;
        public HandleEnum IsInBattle { get; set; } = HandleEnum.Ignore;
        public int DelayMsec { get; set; } = 0;
    }
}