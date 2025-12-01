using System;

namespace ModLib.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SkillAttribute : Attribute
    {
        public string CacheId { get; set; }
        public bool IsCached { get; set; }

        public SkillAttribute(string cacheId)
        {
            CacheId = cacheId;
        }
    }
}