using ModLib.Enum;
using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EventConditionAttribute : Attribute
{
    public HandleEnum IsInGame { get; set; } = HandleEnum.True;
    public HandleEnum IsInBattle { get; set; } = HandleEnum.False;
}