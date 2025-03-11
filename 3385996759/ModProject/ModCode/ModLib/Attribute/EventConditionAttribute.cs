using ModLib.Enum;
using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EventConditionAttribute : Attribute
{
    public HandleEnum IsInGame { get; set; } = HandleEnum.Ignore;
    public HandleEnum IsInBattle { get; set; } = HandleEnum.Ignore;
    public int DelayMsec { get; set; } = 0;
}