namespace ModLib.Object
{
    public abstract class CachableObject
    {
        public string CacheId { get; set; }

        public CachableObject(string cacheId)
        {
            CacheId = cacheId;
        }

        public CachableObject()
        {
        }
    }
}
