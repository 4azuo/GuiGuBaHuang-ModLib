using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EventConditionAttribute : Attribute
{
    public string CustomCondition { get; set; } = null;
    public bool IsInGame { get; set; } = true;
    public bool IsInBattle { get; set; } = false;
    public bool IsWorldRunning { get; set; } = false;
}