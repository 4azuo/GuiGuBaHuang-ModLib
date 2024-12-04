using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EventConditionAttribute : Attribute
{
    public string CustomCondition { get; set; } = null;
    public int IsInGame { get; set; } = 1; //-1: ignore, 0: false, 1: true
    public int IsInBattle { get; set; } = 0; //-1: ignore, 0: false, 1: true
    public int IsWorldRunning { get; set; } = 0; //-1: ignore, 0: false, 1: true
}