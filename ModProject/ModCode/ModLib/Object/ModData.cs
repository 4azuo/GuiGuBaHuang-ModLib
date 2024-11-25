using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModLib.Object
{
    public class ModData
    {
        public string SaveTime { get; set; }
        public IDictionary<string, CachableObject> Data { get; set; }

        [Obsolete]
        public ModData()
        {
            //loaded by json
            //Data = new Dictionary<string, CachableObject>();
        }

        public ModData(bool isGlobal)
        {
            Data = new Dictionary<string, CachableObject>();
            Init(isGlobal);
        }

        public void Init(bool isGlobal)
        {
            LoadEvents(Assembly.GetAssembly(typeof(ModMaster)), isGlobal);
            LoadEvents(Assembly.GetAssembly(ModMaster.ModObj.GetType()), isGlobal);
        }

        public void LoadEvents(Assembly ass, bool isGlobal)
        {
            var eventTypes = ass.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(CachableObject)) && x.GetCustomAttributes<CacheAttribute>().Count() > 0).ToList();
            foreach (var t in eventTypes)
            {
                foreach (var attr in t.GetCustomAttributes<CacheAttribute>().Where(x => x.IsGlobal == isGlobal))
                {
                    if (!Data.ContainsKey(attr.CacheId))
                    {
                        DebugHelper.WriteLine($"Create {(isGlobal ? "Global" : "Game")}Cache: Type={t.FullName}, Id={attr.CacheId}");
                        var e = (CachableObject)Activator.CreateInstance(t);
                        e.CacheId = attr.CacheId;
                        Data.Add(attr.CacheId, e);
                    }
                }
            }
        }

        public T CreateIfNotExists<T>(string key, T defaultValue = null) where T : CachableObject
        {
            return (T)CreateIfNotExists(typeof(T), key, defaultValue);
        }

        public CachableObject CreateIfNotExists(Type dataType, string key, CachableObject defaultValue = null)
        {
            if (Data.ContainsKey(key))
                return Data[key];
            CachableObject rs;
            if (defaultValue == null)
            {
                rs = (CachableObject)Activator.CreateInstance(dataType);
            }
            else
            {
                rs = defaultValue;
            }
            rs.CacheId = key;
            Data.Add(key, rs);
            return rs;
        }

        public T GetData<T>(string key, T defaultValue = null) where T : CachableObject
        {
            return CreateIfNotExists<T>(key, defaultValue);
        }

        public CachableObject GetData(Type dataType, string key, CachableObject defaultValue = null)
        {
            return CreateIfNotExists(dataType, key, defaultValue);
        }

        public IList<T> GetDatas<T>() where T : CachableObject
        {
            return GetDatas(typeof(T)).Cast<T>().ToList();
        }

        public IList<CachableObject> GetDatas(Type dataType)
        {
            return Data.Values.Where(x => dataType.IsAssignableFrom(x.GetType())).Select(x => x).ToList();
        }

        public void SetData(string key, CachableObject replacementValue)
        {
            if (!Data.ContainsKey(key))
                Data.Add(key, replacementValue);
            else
                Data[key] = replacementValue;
        }

        public void ClearData(string key)
        {
            if (Data.ContainsKey(key))
                Data.Remove(key);
        }
    }
}