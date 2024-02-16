using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class EnumObjectIndexAttribute : Attribute
{
    public int Index { get; set; }

    public EnumObjectIndexAttribute(int i)
    {
        Index = i;
    }
}