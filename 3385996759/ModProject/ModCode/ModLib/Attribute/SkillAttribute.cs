using System;

namespace ModLib.Attributes
{
    /// <summary>
    /// Registers a ModSkill. IsCached = true saves the skill state to a skill cache file after each battle.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SkillAttribute : Attribute
    {
        public string CacheId { get; set; }
        public bool IsCached { get; set; }

        /// <summary>
        /// Creates the attribute with the cache id.
        /// </summary>
        public SkillAttribute(string cacheId)
        {
            CacheId = cacheId;
        }
    }
}