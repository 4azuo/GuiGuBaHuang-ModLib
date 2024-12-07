namespace ModLib.Object
{
    public abstract class CachableObject
    {
        public string CacheId { get; set; }
        public CacheAttribute.CType CacheType { get; set; }
        public CacheAttribute.WType WorkOn { get; set; }

        public CachableObject(string cacheId)
        {
            CacheId = cacheId;
        }

        public CachableObject()
        {
        }

        public virtual void OnLoadClass(bool isNew) { }
        public virtual void OnUnloadClass() { }
    }
}
