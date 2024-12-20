using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SkillAttribute : Attribute
{
    public bool IsCached { get; set; }

    public SkillAttribute(bool isCached)
    {
        IsCached = isCached;
    }
}