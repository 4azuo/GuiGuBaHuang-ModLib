using ModLib.Converter;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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

    [Obsolete]
    public static string GetGameCacheFileName()
    {
        if (GameHelper.IsInGame())
        {
            return $"{g.world.playerUnit.GetUnitId()}_data.json";
        }
        throw new FileNotFoundException();
    }

    [Obsolete]
    public static string GetGlobalCacheFileName()
    {
        return $"{ModMaster.ModObj.ModId}_data.json";
    }

    public static string GetGameCacheFileName(string whichData)
    {
        if (GameHelper.IsInGame())
        {
            return $"{whichData}_gamedata.json";
        }
        throw new FileNotFoundException();
    }

    public static string GetGlobalCacheFileName(string whichData)
    {
        return $"{whichData}_globaldata.json";
    }

    [Obsolete]
    public static string GetOldCacheFolderName()
    {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{ModMaster.ModObj.ModName}\\";
    }

    public static string GetCacheFolderName(bool isGlobal)
    {
        var sub = isGlobal ? ModMaster.ModObj.ModId : g.world.playerUnit.GetUnitId();
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{ModMaster.ModObj.ModName}\\data\\{sub}\\";
    }

    [Obsolete]
    public static string GetGameCacheFilePath()
    {
        return Path.Combine(GetOldCacheFolderName(), GetGameCacheFileName());
    }

    [Obsolete]
    public static string GetGlobalCacheFilePath()
    {
        return Path.Combine(GetOldCacheFolderName(), GetGlobalCacheFileName());
    }

    public static string GetGameCacheFilePath(string whichData)
    {
        return Path.Combine(GetCacheFolderName(false), GetGameCacheFileName(whichData));
    }

    public static string GetGlobalCacheFilePath(string whichData)
    {
        return Path.Combine(GetCacheFolderName(true), GetGlobalCacheFileName(whichData));
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

    public static IList<CachableObject> GetDatas(Type dataType)
    {
        var rs = GetGlobalCache().GetDatas(dataType).ToList();
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
            var cacheOldFolderPath = GetOldCacheFolderName();
            var cacheFolderPath = GetCacheFolderName(true);
            Directory.CreateDirectory(cacheOldFolderPath);
            Directory.CreateDirectory(cacheFolderPath);

            var files = Directory.GetFiles(cacheFolderPath, $"{ModMaster.ModObj.ModId}_*_globaldata.json");
            var cacheOldFilePath = GetGlobalCacheFilePath();
            if (files.Length > 0)
            {
                GlobalCacheData = new ModLib.Object.ModData();
                foreach (var cacheFilePath in files)
                {
                    var id = cacheFilePath.Split('_')[1];
                    DebugHelper.WriteLine($"Load: GlobalCache: File={cacheFilePath}");
                    GlobalCacheData.Data.Add(id, (CachableObject)Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(cacheFilePath), GetCacheType(id, true), CACHE_JSON_SETTINGS));
                }
                GlobalCacheData.Init(true);
            }
            else if (File.Exists(cacheOldFilePath))
            {
                DebugHelper.WriteLine($"Load: GlobalCache: File={cacheOldFilePath}");
                GlobalCacheData = Newtonsoft.Json.JsonConvert.DeserializeObject<ModLib.Object.ModData>(File.ReadAllText(cacheOldFilePath), CACHE_JSON_SETTINGS);
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
            var cacheOldFolderPath = GetOldCacheFolderName();
            var cacheFolderPath = GetCacheFolderName(false);
            Directory.CreateDirectory(cacheOldFolderPath);
            Directory.CreateDirectory(cacheFolderPath);

            var files = Directory.GetFiles(cacheFolderPath, $"{g.world.playerUnit.GetUnitId()}_*_gamedata.json");
            var cacheOldFilePath = GetGameCacheFilePath();
            if (files.Length > 0)
            {
                GameCacheData = new ModLib.Object.ModData();
                foreach (var cacheFilePath in files)
                {
                    var id = cacheFilePath.Split('_')[1];
                    DebugHelper.WriteLine($"Load: GameCache: File={cacheFilePath}");
                    GameCacheData.Data.Add(id, (CachableObject)Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(cacheFilePath), GetCacheType(id, false), CACHE_JSON_SETTINGS));
                }
                GameCacheData.Init(false);
            }
            else if (File.Exists(cacheOldFilePath))
            {
                DebugHelper.WriteLine($"Load: GameCache: File={cacheOldFilePath}");
                GameCacheData = Newtonsoft.Json.JsonConvert.DeserializeObject<ModLib.Object.ModData>(File.ReadAllText(cacheOldFilePath), CACHE_JSON_SETTINGS);
                GameCacheData.Init(false);
            }
            else
            {
                GameCacheData = new ModLib.Object.ModData(false);
            }
            #region old
            const string oldSttId = "*ModSettings";
            if (GameCacheData.Data.ContainsKey(oldSttId))
            {
                GameCacheData.Data[InGameSettings.MOD_SETTINGS_KEY] = GameCacheData.Data[oldSttId];
                GameCacheData.Data.Remove(oldSttId);
            }
            #endregion
        }
        return GameCacheData;
    }

    public static List<Type> GetCacheTypes()
    {
        var rs = new List<Type>();
        rs.AddRange(Assembly.GetAssembly(typeof(ModMaster)).GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(CachableObject)) && x.GetCustomAttributes<CacheAttribute>().Count() > 0));
        rs.AddRange(Assembly.GetAssembly(ModMaster.ModObj.GetType()).GetTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(CachableObject)) && x.GetCustomAttributes<CacheAttribute>().Count() > 0));
        return rs;
    }

    public static Type GetCacheType(string cacheId, bool isGlobal)
    {
        foreach (var t in GetCacheTypes())
        {
            if (t.GetCustomAttributes<CacheAttribute>().Any(x => x.CacheId == cacheId && x.IsGlobal == isGlobal))
                return t;
        }
        return null;
    }

    public static void Save()
    {
        if (IsGlobalCacheLoaded())
        {
            //GlobalCacheData.SaveTime = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} ({(g.world.run.roundMonth / 12) + 1:0000}/{(g.world.run.roundMonth % 12) + 1:00}/{g.world.run.roundDay + 1:00})";
            Save(true, GlobalCacheData);
            File.WriteAllText($"{GetGlobalCacheFilePath()}.old", Newtonsoft.Json.JsonConvert.SerializeObject(GlobalCacheData, CACHE_JSON_SETTINGS));
        }
        if (IsGameCacheLoaded())
        {
            //GameCacheData.SaveTime = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss.fff} ({(g.world.run.roundMonth / 12) + 1:0000}/{(g.world.run.roundMonth % 12) + 1:00}/{g.world.run.roundDay + 1:00})";
            Save(false, GameCacheData);
            File.WriteAllText($"{GetGameCacheFilePath()}.old", Newtonsoft.Json.JsonConvert.SerializeObject(GameCacheData, CACHE_JSON_SETTINGS));
        }
    }

    private static async void Save(bool isGlobal, ModLib.Object.ModData data)
    {
        foreach (var d in data.Data)
        {
            var filePath = isGlobal ? GetGlobalCacheFilePath(d.Key) : GetGameCacheFilePath(d.Key);
            if (File.Exists(filePath))
                File.Copy(filePath, $"{filePath}.bk", true);
            File.WriteAllText(filePath, Newtonsoft.Json.JsonConvert.SerializeObject(d.Value, CACHE_JSON_SETTINGS));
        }
    }

    public static void ClearGameCache()
    {
        GameCacheData = null;
    }

    public static bool IsGameCacheLoaded()
    {
        return GameCacheData != null;
    }

    public static bool IsGlobalCacheLoaded()
    {
        return GlobalCacheData != null;
    }
}