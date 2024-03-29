﻿using Harmony;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ModLib.Mod
{
    public sealed class ModData
    {
        public string SaveTime { get; set; }
        public IDictionary<string, CachableObject> Data { get; set; }

        public ModData()
        {
            //loaded by json
            //Data = new Dictionary<string, CachableObject>();
        }

        public ModData(bool createEvent, bool isGlobal)
        {
            Data = new Dictionary<string, CachableObject>();
            if (createEvent)
            {
                LoadEvents(Assembly.GetAssembly(typeof(ModMaster)), isGlobal);
                LoadEvents(Assembly.GetAssembly(ModMaster.ModObj.GetType()), isGlobal);
            }
        }

        private void LoadEvents(Assembly ass, bool isGlobal)
        {
            var eventTypes = ass.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(CachableObject)) && x.GetCustomAttributes<CacheAttribute>().Count() > 0).ToList();
            foreach (var t in eventTypes)
            {
                foreach (var attr in t.GetCustomAttributes<CacheAttribute>().Where(x => x.IsGlobal == isGlobal))
                {
                    DebugHelper.WriteLine($"Create {(isGlobal ? "Global" : "Game")}Cache: Type={t.FullName}, Id={attr.CacheId}");
                    var e = (CachableObject)Activator.CreateInstance(t);
                    e.CacheId = attr.CacheId;
                    Data.Add(attr.CacheId, e);
                }
            }
        }

        public T CreateIfNotExists<T>(string key, T defaultValue = null) where T : CachableObject
        {
            return (T)CreateIfNotExists(typeof(T) , key, defaultValue);
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
    }
}
