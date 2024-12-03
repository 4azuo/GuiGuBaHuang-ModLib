using ModLib.Converter;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public static class CacheHelper
{
    public static readonly Newtonsoft.Json.JsonSerializerSettings CACHE_JSON_SETTINGS = new Newtonsoft.Json.JsonSerializerSettings
    {
        Formatting = Newtonsoft.Json.Formatting.Indented,
        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
        PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
        Converters = new List<Newtonsoft.Json.JsonConverter>() { new EnumObjectConverter() },
        Error = (sender, args) =>
        {
            args.ErrorContext.Handled = true;
        },
    };

    private static ModLib.Object.ModData GlobalCacheData;
    private static ModLib.Object.ModData GameCacheData;

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

    public static T GetData<T>(string key) where T : CachableObject
    {
        return GetDatas<T>().FirstOrDefault(x => x.CacheId == key);
    }

    public static CachableObject GetData(Type dataType, string key)
    {
        return GetDatas(dataType).FirstOrDefault(x => x.CacheId == key);
    }

    public static IList<T> GetDatas<T>() where T : CachableObject
    {
        return (IList<T>)GetDatas(typeof(T));
    }

    public static List<CachableObject> GetDatas(Type dataType)
    {
        var rs = GetGlobalCache().GetDatas(dataType);
        if (IsGameCacheLoaded())
        {
            rs.AddRange(GetGameCache().GetDatas(dataType));
        }
        return rs;
    }

    public static ModLib.Object.ModData GetGlobalCache()
    {
        if (!IsGlobalCacheLoaded())
        {
            var cacheFolderPath = GetCacheFolderName();
            var cacheFilePath = GetGlobalCacheFilePath();
            Directory.CreateDirectory(cacheFolderPath);
            if (File.Exists(cacheFilePath))
            {
                DebugHelper.WriteLine($"Load: GlobalCache: File={cacheFilePath}");
                GlobalCacheData = Newtonsoft.Json.JsonConvert.DeserializeObject<ModLib.Object.ModData>(File.ReadAllText(cacheFilePath), CACHE_JSON_SETTINGS);
                GlobalCacheData.Init(true);
            }
            else
            {
                GlobalCacheData = new ModLib.Object.ModData(true);
            }
        }
        return GlobalCacheData;
    }

    public static ModLib.Object.ModData GetGameCache()
    {
        if (!GameHelper.IsInGame())
        {
            ClearGameCache();
            return GameCacheData;
        }
        if (!IsGameCacheLoaded())
        {
            var cacheFolderPath = GetCacheFolderName();
            var cacheFilePath = GetGameCacheFilePath();
            Directory.CreateDirectory(cacheFolderPath);
            if (File.Exists(cacheFilePath))
            {
                DebugHelper.WriteLine($"Load: GameCache: File={cacheFilePath}");
                GameCacheData = Newtonsoft.Json.JsonConvert.DeserializeObject<ModLib.Object.ModData>(File.ReadAllText(cacheFilePath), CACHE_JSON_SETTINGS);
                GameCacheData.Init(false);
            }
            else
            {
                GameCacheData = new ModLib.Object.ModData(false);
            }
        }
        return GameCacheData;
    }

    public static void Save()
    {
        if (IsGlobalCacheLoaded())
        {
            GlobalCacheData.SaveTime = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} ({(g.world.run.roundMonth / 12) + 1:0000}/{(g.world.run.roundMonth % 12) + 1:00}/{g.world.run.roundDay + 1:00})";
            File.WriteAllText(GetGlobalCacheFilePath(), Newtonsoft.Json.JsonConvert.SerializeObject(GlobalCacheData, CACHE_JSON_SETTINGS));
        }
        if (IsGameCacheLoaded())
        {
            GameCacheData.SaveTime = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} ({(g.world.run.roundMonth / 12) + 1:0000}/{(g.world.run.roundMonth % 12) + 1:00}/{g.world.run.roundDay + 1:00})";
            File.WriteAllText(GetGameCacheFilePath(), Newtonsoft.Json.JsonConvert.SerializeObject(GameCacheData, CACHE_JSON_SETTINGS));
        }
    }

    public static void ClearGameCache()
    {
        GameCacheData = null;
    }

    public static bool IsGameCacheLoaded()
    {
        if (!GameHelper.IsInGame() || GameHelper.IsModScreen())
            ClearGameCache();
        return GameCacheData != null;
    }

    public static bool IsGlobalCacheLoaded()
    {
        return GlobalCacheData != null;
    }
}