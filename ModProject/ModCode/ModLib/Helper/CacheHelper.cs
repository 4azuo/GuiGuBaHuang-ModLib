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

    public static string GetGlobalCacheFileName()
    {
        return $"{ModMaster.ModObj.ModId}_data.json";
    }

    public static string GetCacheFolderName()
    {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{ModMaster.ModObj.ModName}\\";
    }

    public static string GetGameCacheFilePath()
    {
        return Path.Combine(GetCacheFolderName(), GetGameCacheFileName());
    }

    public static string GetGlobalCacheFilePath()
    {
        return Path.Combine(GetCacheFolderName(), GetGlobalCacheFileName());
    }

    public static void AddCachableObject(CachableObject item)
    {
        CacheData.Add(item.CacheId, item);
    }

    public static void AddCachableObjects(List<CachableObject> items)
    {
        foreach (var item in items)
            AddCachableObject(item);
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

    public static List<CachableObject> LoadGlobalCaches()
    {
        if (!File.Exists(GetGlobalCacheFilePath()))
            return new List<CachableObject>();
        return (List<CachableObject>)JsonConvert.DeserializeObject(GetGlobalCacheFilePath(), JSON_SETTINGS);
    }

    public static List<CachableObject> LoadGameCaches()
    {
        if (!File.Exists(GetGameCacheFilePath()))
            return new List<CachableObject>();
        return (List<CachableObject>)JsonConvert.DeserializeObject(GetGameCacheFilePath(), JSON_SETTINGS);
    }

    public static void SaveGlobalCaches()
    {
        File.WriteAllText(GetGlobalCacheFilePath(), JsonConvert.SerializeObject(GetGlobalCaches(), JSON_SETTINGS));
    }

    public static void SaveGameCaches()
    {
        File.WriteAllText(GetGameCacheFilePath(), JsonConvert.SerializeObject(GetGameCaches(), JSON_SETTINGS));
    }

    public static void Save()
    {
        SaveGlobalCaches();
        if (GameHelper.IsInGame())
            SaveGameCaches();
    }

    public static void Clear()
    {
        CacheData.Clear();
    }

    public static List<Type> GetCacheTypes(CacheAttribute.CType ctype)
    {
        var rs = new List<Type>();
        rs.AddRange(GetCacheTypes(GameHelper.GetModMasterAssembly(), ctype));
        rs.AddRange(GetCacheTypes(GameHelper.GetModMainAssembly(), ctype));
        return rs;
    }

    public static List<Type> GetCacheTypes(Assembly ass, CacheAttribute.CType ctype)
    {
        return ass.GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(CachableObject)) && x.GetCustomAttributes<CacheAttribute>().Any(y => y.CacheType == ctype)).ToList();
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
}