using System;

namespace ModLib.Attributes
{
    /// <summary>
    /// Registers a CachableObject (ModChild/ModEvent) in ModLib.
    /// CacheId identifies the object and its cache file; OrderIndex sets execution order;
    /// CacheType chooses when it is loaded/saved (Global = title screen, Local = per save game);
    /// WorkOn limits where its events run (All, Global = title screen only, Local = in save game only).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CacheAttribute : Attribute
    {
        /// <summary>
        /// When the object is created and where it is persisted.
        /// </summary>
        public enum CType
        {
            Local,
            Global,
        }

        /// <summary>
        /// Where the object's events are executed.
        /// </summary>
        public enum WType
        {
            All,
            Local,
            Global,
        }

        public string CacheId { get; private set; }
        public int OrderIndex { get; set; }
        public CType CacheType { get; set; } = CType.Local;
        public WType WorkOn { get; set; } = WType.All;

        /// <summary>
        /// Creates the attribute with the cache id.
        /// </summary>
        public CacheAttribute(string cacheId)
        {
            CacheId = cacheId;
        }
    }
}