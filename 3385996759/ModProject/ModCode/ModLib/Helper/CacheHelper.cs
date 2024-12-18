﻿using ModLib.Converter;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public static class CacheHelper
{
    public static readonly JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        TypeNameHandling = TypeNameHandling.All,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        Converters = new List<JsonConverter>() { new EnumObjectConverter() },
        Error = (sender, args) =>
        {
            args.ErrorContext.Handled = true;
        },
    };

    public static Dictionary<string, CachableObject> CacheData { get; private set; } = new Dictionary<string, CachableObject>();

    public static string GetGameCacheFileName(string cacheId)
    {
        if (GameHelper.IsInGame())
            return $"{cacheId}_data.json";
        throw new FileNotFoundException();
    }

    public static string GetGlobalCacheFileName(string cacheId)
    {
        return $"{cacheId}_data.json";
    }

    public static string GetCacheFolderName(string modId)
    {
        var p = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{modId}\\";
        if (!Directory.Exists(p))
            Directory.CreateDirectory(p);
        return p;
    }

    public static string GetGameCacheFolderName(string modId)
    {
        var p = $"{GetCacheFolderName(modId)}\\saves\\{g.world.playerUnit.GetUnitId()}\\";
        if (!Directory.Exists(p))
            Directory.CreateDirectory(p);
        return p;
    }

    public static string GetGlobalCacheFolderName(string modId)
    {
        var p = $"{GetCacheFolderName(modId)}\\globals\\";
        if (!Directory.Exists(p))
            Directory.CreateDirectory(p);
        return p;
    }

    public static string GetGameCacheFilePath(string modId, string cacheId)
    {
        return Path.Combine(GetGameCacheFolderName(modId), GetGameCacheFileName(cacheId));
    }

    public static string GetGlobalCacheFilePath(string modId, string cacheId)
    {
        return Path.Combine(GetGlobalCacheFolderName(modId), GetGlobalCacheFileName(cacheId));
    }

    public static void AddCachableObject(CachableObject item)
    {
        if (!CacheData.ContainsKey(item.CacheId))
            CacheData.Add(item.CacheId, item);
    }

    public static void AddCachableObjects(List<CachableObject> items)
    {
        foreach (var item in items)
        {
            AddCachableObject(item);
        }
    }

    public static void RemoveCachableObject(CachableObject item)
    {
        CacheData.Remove(item.CacheId);
    }

    public static List<CachableObject> GetGlobalCaches()
    {
        return GetAllCachableObjects().Where(x => x.CacheType == CacheAttribute.CType.Global).ToList();
    }

    public static List<CachableObject> GetGameCaches()
    {
        return GetAllCachableObjects().Where(x => x.CacheType == CacheAttribute.CType.Local).ToList();
    }

    public static T GetCachableObject<T>(string key) where T : CachableObject
    {
        return GetAllCachableObjects<T>().FirstOrDefault(x => x.CacheId == key);
    }

    public static CachableObject GetCachableObject(Type t, string key)
    {
        return GetAllCachableObjects(t).FirstOrDefault(x => x.CacheId == key);
    }

    public static List<T> GetAllCachableObjects<T>() where T : CachableObject
    {
        return GetAllCachableObjects().Where(x => typeof(T).IsAssignableFrom(x.GetType())).Cast<T>().ToList();
    }

    public static List<CachableObject> GetAllCachableObjects(Type t)
    {
        return GetAllCachableObjects().Where(x => t.IsAssignableFrom(x.GetType())).ToList();
    }

    public static List<CachableObject> GetAllCachableObjects()
    {
        return CacheData.Values.ToList();
    }

    public static T ReadGlobalCacheFile<T>(string modId, string cacheId) where T : CachableObject
    {
        var attr = typeof(T).GetCustomAttributes<CacheAttribute>().FirstOrDefault(x => x.CacheId == cacheId && x.CacheType == CacheAttribute.CType.Global);
        if (attr == null)
            return null;
        var cacheFile = GetGlobalCacheFilePath(modId, attr.CacheId);
        if (!File.Exists(cacheFile))
            return null;
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(cacheFile), JSON_SETTINGS);
    }

    public static T ReadGameCacheFile<T>(string modId, string cacheId) where T : CachableObject
    {
        var attr = typeof(T).GetCustomAttributes<CacheAttribute>().FirstOrDefault(x => x.CacheId == cacheId && x.CacheType == CacheAttribute.CType.Local);
        if (attr == null)
            return null;
        var cacheFile = GetGameCacheFilePath(modId, attr.CacheId);
        if (!File.Exists(cacheFile))
            return null;
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(cacheFile), JSON_SETTINGS);
    }

    public static List<CachableObject> LoadGlobalCaches()
    {
        var rs = new List<CachableObject>();
        foreach (var t in GetCacheTypes())
        {
            foreach (var attr in t.Value.GetCustomAttributes<CacheAttribute>().Where(x => x.CacheType == CacheAttribute.CType.Global))
            {
                var cacheFile = GetGlobalCacheFilePath(t.Key, attr.CacheId);
                if (File.Exists(cacheFile))
                {
                    var e = (CachableObject)JsonConvert.DeserializeObject(File.ReadAllText(cacheFile), t.Value, JSON_SETTINGS);
                    DebugHelper.WriteLine($"Load GlobalCache: Mod={t.Key}, Type={t.Value.FullName}, Id={e.CacheId}");
                    e.OnLoadClass(false, t.Key, attr);
                    rs.Add(e);
                }
            }
        }
        AddCachableObjects(rs);
        return rs;
    }

    public static List<CachableObject> LoadNewGlobalCaches()
    {
        var rs = new List<CachableObject>();
        foreach (var t in GetCacheTypes())
        {
            foreach (var attr in t.Value.GetCustomAttributes<CacheAttribute>().Where(x => x.CacheType == CacheAttribute.CType.Global))
            {
                if (!CacheData.ContainsKey(attr.CacheId))
                {
                    var e = (CachableObject)Activator.CreateInstance(t.Value);
                    DebugHelper.WriteLine($"Create GlobalCache: Mod={t.Key}, Type={t.Value.FullName}, Id={attr.CacheId}");
                    e.OnLoadClass(true, t.Key, attr);
                    rs.Add(e);
                }
            }
        }
        AddCachableObjects(rs);
        return rs;
    }

    public static List<CachableObject> LoadGameCaches()
    {
        var rs = new List<CachableObject>();
        foreach (var t in GetCacheTypes())
        {
            foreach (var attr in t.Value.GetCustomAttributes<CacheAttribute>().Where(x => x.CacheType == CacheAttribute.CType.Local))
            {
                var cacheFile = GetGameCacheFilePath(t.Key, attr.CacheId);
                if (File.Exists(cacheFile))
                {
                    var e = (CachableObject)JsonConvert.DeserializeObject(File.ReadAllText(cacheFile), t.Value, JSON_SETTINGS);
                    DebugHelper.WriteLine($"Load GameCache: Mod={t.Key}, Type={t.Value.FullName}, Id={e.CacheId}");
                    e.OnLoadClass(false, t.Key, attr);
                    rs.Add(e);
                }
            }
        }
        AddCachableObjects(rs);
        return rs;
    }

    public static List<CachableObject> LoadNewGameCaches()
    {
        var rs = new List<CachableObject>();
        foreach (var t in GetCacheTypes())
        {
            foreach (var attr in t.Value.GetCustomAttributes<CacheAttribute>().Where(x => x.CacheType == CacheAttribute.CType.Local))
            {
                if (!CacheData.ContainsKey(attr.CacheId))
                {
                    var e = (CachableObject)Activator.CreateInstance(t.Value);
                    DebugHelper.WriteLine($"Create GameCache: Mod={t.Key}, Type={t.Value.FullName}, Id={attr.CacheId}");
                    e.OnLoadClass(true, t.Key, attr);
                    rs.Add(e);
                }
            }
        }
        AddCachableObjects(rs);
        return rs;
    }

    public static void RemoveUnuseGlobalCaches()
    {
        foreach (var c in GetGlobalCaches())
        {
            if (c.WorkOn == CacheAttribute.WType.Global)
            {
                DebugHelper.WriteLine($"Unload Cache: Mod={c.ModId}, Type={c.GetType().FullName}, Id={c.CacheId}");
                c.OnUnloadClass();
                RemoveCachableObject(c);
            }
        }
    }

    public static void SaveGlobalCache(CachableObject item)
    {
        DebugHelper.WriteLine($"Save global-caches: Mod={item.ModId}, Type={item.GetType().FullName}, Id={item.CacheId}, OrderIndex={item.OrderIndex}({item.InModOrderIndex()})");
        File.WriteAllText(GetGlobalCacheFilePath(item.ModId, item.CacheId), JsonConvert.SerializeObject(item, JSON_SETTINGS));
    }

    public static void SaveGlobalCaches()
    {
        foreach (var item in GetGlobalCaches())
        {
            SaveGlobalCache(item);
        }
    }

    public static void SaveGameCache(CachableObject item)
    {
        DebugHelper.WriteLine($"Save game-caches: Mod={item.ModId}, Type={item.GetType().FullName}, Id={item.CacheId}, OrderIndex={item.OrderIndex}({item.InModOrderIndex()})");
        File.WriteAllText(GetGameCacheFilePath(item.ModId, item.CacheId), JsonConvert.SerializeObject(item, JSON_SETTINGS));
    }

    public static void SaveGameCaches()
    {
        foreach (var item in GetGameCaches())
        {
            SaveGameCache(item);
        }
    }

    public static void Save()
    {
        SaveGlobalCaches();
        if (GameHelper.IsInGame())
            SaveGameCaches();
    }

    public static void ClearGlobalCaches()
    {
        foreach (var e in GetGlobalCaches())
        {
            DebugHelper.WriteLine($"Unload GlobalCache: Mod={e.ModId}, Type={e.GetType().FullName}, Id={e.CacheId}");
            e.OnUnloadClass();
            CacheData.Remove(e.CacheId);
        }
    }

    public static void ClearGameCaches()
    {
        foreach (var e in GetGameCaches())
        {
            DebugHelper.WriteLine($"Unload GameCache: Mod={e.ModId}, Type={e.GetType().FullName}, Id={e.CacheId}");
            e.OnUnloadClass();
            CacheData.Remove(e.CacheId);
        }
    }

    public static void Clear()
    {
        ClearGlobalCaches();
        ClearGameCaches();
        CacheData.Clear();
    }

    public static List<KeyValuePair<string, Type>> GetCacheTypes(bool includeInactive = false)
    {
        var rs = new List<KeyValuePair<string, Type>>();
        rs.AddRange(GetCacheTypes(ModMaster.ModObj.ModId, GameHelper.GetModLibAssembly()));
        rs.AddRange(GetCacheTypes(ModMaster.ModObj.ModId, GameHelper.GetModLibMainAssembly()));

        foreach (var mod in g.mod.allModPaths)
        {
            if (g.mod.IsLoadMod(mod.t1) || includeInactive)
            {
                rs.AddRange(GetCacheTypes(mod.t1, GameHelper.GetModChildAssembly(mod.t1)));
            }
        }
        return rs;
    }

    public static List<KeyValuePair<string, Type>> GetCacheTypes(string modId, Assembly ass)
    {
        if (ass == null)
            return new List<KeyValuePair<string, Type>>();
        return ass.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(CachableObject))).Select(x => new KeyValuePair<string, Type>(modId, x)).ToList();
    }

    public static void Order()
    {
        foreach (var modChild in GetAllCachableObjects<ModChild>())
        {
            var orderCfg = modChild.GetType().GetCustomAttribute<ModOrderAttribute>();
            if (orderCfg != null)
            {
                var orderList = JsonConvert.DeserializeObject<Dictionary<string, int>>(ConfHelper.ReadConfData(modChild.ModId, orderCfg.OrderFile));
                var defaultIndex = 9000;
                foreach (var e in modChild.GetChildren())
                {
                    if (orderList.ContainsKey(e.CacheId))
                        e.OrderIndex = orderList[e.CacheId];
                    else if (e.OrderIndex < 0)
                        e.OrderIndex = defaultIndex++;
                }
            }
        }
        CacheData = CacheData.OrderBy(x => x.Value.InModOrderIndex()).ToDictionary(x => x.Key, x => x.Value);
    }
}