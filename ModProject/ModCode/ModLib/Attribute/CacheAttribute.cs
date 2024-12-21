using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class CacheAttribute : Attribute
{
    public enum CType
    {
        Local,
        Global,
    }

    public enum WType
    {
        All,
        Local,
        Global,
    }

    public string CacheId { get; set; }
    public int OrderIndex { get; set; }
    public CType CacheType { get; set; } = CType.Local;
    public WType WorkOn { get; set; } = WType.All;

    public CacheAttribute(string cacheId)
    {
        CacheId = cacheId;
    }
}