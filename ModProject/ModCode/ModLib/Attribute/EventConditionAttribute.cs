using MethodBoundaryAspect.Fody.Attributes;
using ModLib.Enum;
using System;
using System.Linq;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EventConditionAttribute : Attribute
{
    public EvCondLoadEnum WithLoadState { get; set; } = EvCondLoadEnum.None;
    public bool NeedFlgUpdate { get; set; } = false;
    public string CustomCondition { get; set; } = null;
}