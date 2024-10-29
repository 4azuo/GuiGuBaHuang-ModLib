using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class CacheAttribute : Attribute
{
    public string CacheId { get; private set; }
    public bool IsGlobal { get; set; } = false;

    public CacheAttribute(string cacheId)
    {
        CacheId = cacheId;
    }
}