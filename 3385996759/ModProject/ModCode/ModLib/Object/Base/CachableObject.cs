using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;

namespace ModLib.Object
{
    public abstract class CachableObject
    {
        public string ModId { get; set; }
        public string CacheId { get; set; } //ModChild's CacheId = ModId
        public int OrderIndex { get; set; } //ModChild always 0
        public CacheAttribute.CType CacheType { get; set; } //ModChild always Global
        public CacheAttribute.WType WorkOn { get; set; }

        public ModChild GetParent() => CacheHelper.GetAllCachableObjects<ModChild>().FirstOrDefault(x => x.CacheId == ModId);
        public List<ModEvent> GetChildren() => CacheHelper.GetAllCachableObjects<ModEvent>().Where(x => x.ModId == CacheId).ToList();

        public virtual void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            ModId = modId;
            CacheId = attr.CacheId;
            OrderIndex = attr.OrderIndex;
            CacheType = attr.CacheType;
            WorkOn = attr.WorkOn;
        }

        public virtual bool OnCacheHandler() { return true; }

        public virtual void OnUnloadClass() { }
    }
}
