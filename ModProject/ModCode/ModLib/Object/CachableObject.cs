using ModLib.Mod;
using System.Collections.Generic;
using System.Linq;

namespace ModLib.Object
{
    public abstract class CachableObject
    {
        public string ModId { get; set; }
        public string CacheId { get; set; }
        public int OrderIndex { get; set; }
        public CacheAttribute.CType CacheType { get; set; }
        public CacheAttribute.WType WorkOn { get; set; }

        public int ModIndex() => g.mod.allModPaths.ToArray().IndexOf(x => x.t1 == ModId);
        public int InModOrderIndex() => OrderIndex + (ModIndex() * 1000000);
        public ModChild GetParent() => CacheHelper.GetAllCachableObjects<ModChild>().FirstOrDefault(x => x.CacheId == ModId);
        public List<ModEvent> GetChildren() => CacheHelper.GetAllCachableObjects<ModEvent>().Where(x => x.ModId == CacheId).ToList();

        public virtual void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            ModId = modId;
            CacheId = attr.CacheId;
            OrderIndex = attr.OrderIndex.FixValue(0, 999999);
            CacheType = attr.CacheType;
            WorkOn = attr.WorkOn;
        }

        public virtual void OnUnloadClass() { }
    }
}
