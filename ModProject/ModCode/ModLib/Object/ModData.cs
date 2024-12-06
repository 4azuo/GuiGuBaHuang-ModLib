using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModLib.Object
{
    public class ModData
    {
        public Dictionary<string, CachableObject> Data { get; } = new Dictionary<string, CachableObject>();

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

        public List<T> GetDatas<T>() where T : CachableObject
        {
            return GetDatas(typeof(T)).Cast<T>().ToList();
        }

        public List<CachableObject> GetDatas(Type dataType)
        {
            if (Data == null)
                return new List<CachableObject>();
            return Data.Values.Where(x => dataType.IsAssignableFrom(x.GetType())).ToList();
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