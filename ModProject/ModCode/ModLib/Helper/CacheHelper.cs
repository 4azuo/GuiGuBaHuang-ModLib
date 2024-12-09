using ModLib.Converter;
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

    public static string GetGameCacheFileName()
    {
        if (GameHelper.IsInGame())
        {
            return $"{g.world.playerUnit.GetUnitId()}_data.json";
        }
        throw new FileNotFoundException();
    }

    public static string GetGlobalCacheFileName(string cacheId)
    {
        return $"{cacheId}_data.json";
    }

    public static string GetCacheFolderName()
    {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{ModMaster.ModObj.ModName}\\";
    }

    public static string GetGameCacheFilePath()
    {
        return Path.Combine(GetCacheFolderName(), GetGameCacheFileName());
    }

    public static string GetGlobalCacheFilePath(string cacheId)
    {
        return Path.Combine(GetCacheFolderName(), GetGlobalCacheFileName(cacheId));
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

    public static T ReadGlobalCacheFile<T>(string cacheId) where T : CachableObject
    {
        var attr = typeof(T).GetCustomAttributes<CacheAttribute>().FirstOrDefault(x => x.CacheId == cacheId);
        if (attr == null)
            return null;
        var cacheFile = GetGlobalCacheFilePath(attr.CacheId);
        if (!File.Exists(cacheFile))
            return null;
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(cacheFile), JSON_SETTINGS);
    }

    public static List<CachableObject> LoadGlobalCaches()
    {
        var rs = new List<CachableObject>();
        foreach (var t in GetCacheTypes())
        {
            foreach (var attr in t.GetCustomAttributes<CacheAttribute>().Where(x => x.CacheType == CacheAttribute.CType.Global))
            {
                var cacheFile = GetGlobalCacheFilePath(attr.CacheId);
                if (File.Exists(cacheFile))
                {
                    var e = (CachableObject)JsonConvert.DeserializeObject(File.ReadAllText(cacheFile), t, JSON_SETTINGS);
                    DebugHelper.WriteLine($"Load GlobalCache: Type={t.FullName}, Id={e.CacheId}");
                    e.OnLoadClass(false);
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
            foreach (var attr in t.GetCustomAttributes<CacheAttribute>().Where(x => x.CacheType == CacheAttribute.CType.Global))
            {
                if (!CacheData.ContainsKey(attr.CacheId))
                {
                    var e = CreateCachableObject(t, attr);
                    DebugHelper.WriteLine($"Create GlobalCache: Type={t.FullName}, Id={attr.CacheId}");
                    e.OnLoadClass(true);
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
        if (File.Exists(GetGameCacheFilePath()))
        {
            var caches = (List<CachableObject>)JsonConvert.DeserializeObject(File.ReadAllText(GetGameCacheFilePath()), JSON_SETTINGS);
            rs.AddRange(caches);
            foreach (var e in caches)
            {
                var x = e as CachableObject;
                DebugHelper.WriteLine($"Load GameCache: Type={x.GetType().FullName}, Id={x.CacheId}");
                x.OnLoadClass(false);
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
            foreach (var attr in t.GetCustomAttributes<CacheAttribute>().Where(x => x.CacheType == CacheAttribute.CType.Local))
            {
                if (!CacheData.ContainsKey(attr.CacheId))
                {
                    var e = CreateCachableObject(t, attr);
                    DebugHelper.WriteLine($"Create GameCache: Type={t.FullName}, Id={attr.CacheId}");
                    e.OnLoadClass(true);
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
                DebugHelper.WriteLine($"Unload Cache: Type={c.GetType().FullName}, Id={c.CacheId}");
                c.OnUnloadClass();
                RemoveCachableObject(c);
            }
        }
    }

    public static void SaveGlobalCache(CachableObject item)
    {
        DebugHelper.WriteLine($"Save global-caches: {item.CacheId}");
        File.WriteAllText(GetGlobalCacheFilePath(item.CacheId), JsonConvert.SerializeObject(item, JSON_SETTINGS));
    }

    public static void SaveGlobalCaches()
    {
        foreach (var item in GetGlobalCaches())
        {
            SaveGlobalCache(item);
        }
    }

    public static void SaveGameCaches()
    {
        DebugHelper.WriteLine("Save game-caches");
        File.WriteAllText(GetGameCacheFilePath(), JsonConvert.SerializeObject(GetGameCaches(), JSON_SETTINGS));
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
            DebugHelper.WriteLine($"Unload GlobalCache: Type={e.GetType().FullName}, Id={e.CacheId}");
            e.OnUnloadClass();
            CacheData.Remove(e.CacheId);
        }
    }

    public static void ClearGameCaches()
    {
        foreach (var e in GetGameCaches())
        {
            DebugHelper.WriteLine($"Unload GameCache: Type={e.GetType().FullName}, Id={e.CacheId}");
            e.OnUnloadClass();
            CacheData.Remove(e.CacheId);
        }
        CacheData.Clear();
    }

    public static void Clear()
    {
        ClearGlobalCaches();
        ClearGameCaches();
    }

    public static List<Type> GetCacheTypes()
    {
        var rs = new List<Type>();
        rs.AddRange(GetCacheTypes(GameHelper.GetModMasterAssembly()));
        rs.AddRange(GetCacheTypes(GameHelper.GetModMainAssembly()));
        return rs;
    }

    public static List<Type> GetCacheTypes(Assembly ass)
    {
        return ass.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(CachableObject))).ToList();
    }

    public static CachableObject CreateCachableObject(Type t, CacheAttribute attr)
    {
        var e = (CachableObject)Activator.CreateInstance(t);
        e.CacheId = attr.CacheId;
        e.CacheType = attr.CacheType;
        e.WorkOn = attr.WorkOn;
        return e;
    }

    public static void Order(Dictionary<string, int> orderList)
    {
        var defaultIndex = 9000;
        CacheData = CacheData.OrderBy(x => orderList.ContainsKey(x.Key) ? orderList[x.Key] : defaultIndex++).ToDictionary(x => x.Key, x => x.Value);
    }

    public static void Order()
    {
        var orderCfg = ModMaster.ModObj.GetType().GetCustomAttribute<ModOrderAttribute>();
        if (orderCfg != null)
        {
            var orderList = JsonConvert.DeserializeObject<Dictionary<string, int>>(ConfHelper.ReadConfData(orderCfg.OrderFile));
            Order(orderList);
        }
    }
}