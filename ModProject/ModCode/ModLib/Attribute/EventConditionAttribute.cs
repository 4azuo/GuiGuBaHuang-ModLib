using ModLib.Enum;
using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EventConditionAttribute : Attribute
{
    public EvCondLoadEnum WithLoadState { get; set; } = EvCondLoadEnum.None;
    public bool NeedFlgUpdate { get; set; } = false;
    public string CustomCondition { get; set; } = null;
    public bool IsInGame { get; set; } = true;
    public bool IsInBattle { get; set; } = false;
}