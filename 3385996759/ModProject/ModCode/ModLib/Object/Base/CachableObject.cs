using ModLib.Attributes;
using ModLib.Mod;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using ModLib.Helper;

namespace ModLib.Object
{
    /// <summary>
    /// Base of everything ModLib persists as JSON (ModChild, ModEvent). Public properties are saved; use [JsonIgnore] for runtime data.
    /// </summary>
    public abstract class CachableObject
    {
        public string ModId { get; set; }
        public string CacheId { get; set; } //ModChild's CacheId = ModId
        public int OrderIndex { get; set; } //ModChild always 0
        public CacheAttribute.CType CacheType { get; set; } //ModChild always Global
        public CacheAttribute.WType WorkOn { get; set; }
        [JsonIgnore]
        public ModChild Parent { get; set; }

        /// <summary>
        /// The ModChild of the owning mod.
        /// </summary>
        public ModChild GetParent() => CacheHelper.GetAllCachableObjects<ModChild>().FirstOrDefault(x => x.CacheId == ModId);
        /// <summary>
        /// Events belonging to this object (when it is a ModChild).
        /// </summary>
        public IList<ModEvent> GetChildren() => CacheHelper.GetAllCachableObjects<ModEvent>().Where(x => x.ModId == CacheId).ToList();

        /// <summary>
        /// Called after creation or deserialization: copies attribute values, sets Parent and assigns the static "Instance" property when declared.
        /// </summary>
        public virtual void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            ModId = modId;
            CacheId = attr.CacheId;
            OrderIndex = attr.OrderIndex;
            CacheType = attr.CacheType;
            WorkOn = attr.WorkOn;

            //set parent
            Parent = GetParent();
            //autowire to [Instance] variable
            this.GetType().GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?.SetValue(null, this);
        }

        /// <summary>
        /// Return false to skip loading this object (feature switched off).
        /// </summary>
        public virtual bool OnCacheHandler() { return true; }

        /// <summary>
        /// Called when the object is removed from the cache.
        /// </summary>
        public virtual void OnUnloadClass() { }
    }
}
