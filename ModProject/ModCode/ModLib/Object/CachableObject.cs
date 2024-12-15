namespace ModLib.Object
{
    public abstract class CachableObject
    {
        public string CacheId { get; set; }
        public int OrderIndex { get; set; }
        public CacheAttribute.CType CacheType { get; set; }
        public CacheAttribute.WType WorkOn { get; set; }

        public virtual void OnLoadClass(bool isNew, CacheAttribute attr)
        {
            CacheId = attr.CacheId;
            OrderIndex = attr.OrderIndex;
            CacheType = attr.CacheType;
            WorkOn = attr.WorkOn;
        }

        public virtual void OnUnloadClass() { }
    }
}
