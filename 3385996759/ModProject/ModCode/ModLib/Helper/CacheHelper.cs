using ModLib.Attributes;
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

    private static readonly List<Tuple<string, CacheAttribute, Type>> EMPTY = new List<Tuple<string, CacheAttribute, Type>>();
    public static List<Tuple<string, CacheAttribute, Type>> CacheTypes { get; private set; }
    public static Dictionary<string, CachableObject> CacheData { get; private set; } = new Dictionary<string, CachableObject>();

    public static string GetSkillCacheFileName(string cacheId)
    {
        if (GameHelper.IsInGame())
            return $"{cacheId}_data.json";
        throw new FileNotFoundException();
    }

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

    public static string GetSkillCacheFolderName(string modId)
    {
        var p = $"{GetGameCacheFolderName(modId)}\\skills\\";
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

    public static string GetCacheFolderName(string modId)
    {
        var p = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{modId}\\";
        if (!Directory.Exists(p))
            Directory.CreateDirectory(p);
        return p;
    }

    public static string GetSkillCacheFilePath(string modId, string cacheId)
    {
        return Path.Combine(GetSkillCacheFolderName(modId), GetSkillCacheFileName(cacheId));
    }

    public static string GetGameCacheFilePath(string modId, string cacheId)
    {
        return Path.Combine(GetGameCacheFolderName(modId), GetGameCacheFileName(cacheId));
    }

    public static string GetGlobalCacheFilePath(string modId, string cacheId)
    {
        return Path.Combine(GetGlobalCacheFolderName(modId), GetGlobalCacheFileName(cacheId));
    }

    public static void AddCachableObject(CachableObject c)
    {
        if (!CacheData.ContainsKey(c.CacheId))
        {
            DebugHelper.WriteLine($"Load Cache: Mod={c.ModId}, Type={c.GetType().FullName}, Id={c.CacheId}, CacheType={c.CacheType}, WorkOn={c.WorkOn}");
            CacheData.Add(c.CacheId, c);
        }
    }

    public static void RemoveCachableObject(CachableObject c)
    {
        DebugHelper.WriteLine($"Unload Cache: Mod={c.ModId}, Type={c.GetType().FullName}, Id={c.CacheId}, CacheType={c.CacheType}, WorkOn={c.WorkOn}");
        CacheData.Remove(c.CacheId);
    }

    public static void ReloadGameCachableObject<T>(T c) where T : CachableObject
    {
        ObjectHelper.Map<T>(ReadGameCacheFile<T>(c.ModId, c.CacheId), c);
    }

    public static void ReloadGlobalCachableObject<T>(T c) where T : CachableObject
    {
        ObjectHelper.Map<T>(ReadGlobalCacheFile<T>(c.ModId, c.CacheId), c);
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

    public static bool IsReadable<T>(string modId, string cacheId) where T : CachableObject
    {
        return FileHelper.IsReadable<T>(GetGlobalCacheFilePath(modId, cacheId));
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

    public static void LoadGlobalCaches()
    {
        foreach (var t in GetCacheTypes())
        {
            if (t.Item2.CacheType == CacheAttribute.CType.Global || t.Item3.IsSubclassOf(typeof(ModChild)))
            {
                var cacheFile = GetGlobalCacheFilePath(t.Item1, t.Item2.CacheId);
                if (File.Exists(cacheFile))
                {
                    var e = (CachableObject)JsonConvert.DeserializeObject(File.ReadAllText(cacheFile), t.Item3, JSON_SETTINGS);
                    e.OnLoadClass(false, t.Item1, t.Item2);
                    if (e.OnCacheHandler())
                        AddCachableObject(e);
                }
                else
                {
                    if (!CacheData.ContainsKey(t.Item2.CacheId))
                    {
                        var e = (CachableObject)Activator.CreateInstance(t.Item3);
                        e.OnLoadClass(true, t.Item1, t.Item2);
                        if (e.OnCacheHandler())
                            AddCachableObject(e);
                    }
                }
            }
        }
    }

    public static void LoadGameCaches()
    {
        foreach (var t in GetCacheTypes())
        {
            if (t.Item2.CacheType == CacheAttribute.CType.Local)
            {
                var cacheFile = GetGameCacheFilePath(t.Item1, t.Item2.CacheId);
                if (File.Exists(cacheFile))
                {
                    var e = (CachableObject)JsonConvert.DeserializeObject(File.ReadAllText(cacheFile), t.Item3, JSON_SETTINGS);
                    e.OnLoadClass(false, t.Item1, t.Item2);
                    if (e.OnCacheHandler())
                        AddCachableObject(e);
                }
                else
                {
                    if (!CacheData.ContainsKey(t.Item2.CacheId))
                    {
                        var e = (CachableObject)Activator.CreateInstance(t.Item3);
                        e.OnLoadClass(true, t.Item1, t.Item2);
                        if (e.OnCacheHandler())
                            AddCachableObject(e);
                    }
                }
            }
        }
    }

    public static void RemoveUnuseGlobalCaches()
    {
        foreach (var c in GetGlobalCaches())
        {
            if (c.WorkOn == CacheAttribute.WType.Global)
            {
                c.OnUnloadClass();
                RemoveCachableObject(c);
            }
        }
    }

    public static void SaveGlobalCache(CachableObject c)
    {
        DebugHelper.WriteLine($"Save global-caches: Mod={c.ModId}, Type={c.GetType().FullName}, Id={c.CacheId}, OrderIndex={c.OrderIndex}, CacheType={c.CacheType}, WorkOn={c.WorkOn}");
        File.WriteAllText(GetGlobalCacheFilePath(c.ModId, c.CacheId), JsonConvert.SerializeObject(c, JSON_SETTINGS));
    }

    public static void SaveGlobalCaches()
    {
        foreach (var item in GetGlobalCaches())
        {
            SaveGlobalCache(item);
        }
    }

    public static void SaveGameCache(CachableObject c)
    {
        DebugHelper.WriteLine($"Save game-caches: Mod={c.ModId}, Type={c.GetType().FullName}, Id={c.CacheId}, OrderIndex={c.OrderIndex}, CacheType={c.CacheType}, WorkOn={c.WorkOn}");
        File.WriteAllText(GetGameCacheFilePath(c.ModId, c.CacheId), JsonConvert.SerializeObject(c, JSON_SETTINGS));
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
        CacheTypes = null;
    }

    public static List<Tuple<string, CacheAttribute, Type>> GetCacheTypes()
    {
        if (CacheTypes == null)
        {
            var rs = new List<Tuple<string, CacheAttribute, Type>>();
            rs.AddRange(GetModLibCacheTypes(ModMaster.ModObj.ModId, AssemblyHelper.GetModLibAssembly()));

            foreach (var mod in g.mod.allModPaths)
            {
                if (g.mod.IsLoadMod(mod.t1) && mod.t1 != ModMaster.ModObj.ModId)
                {
                    rs.AddRange(GetModChildCacheTypes(mod.t1, AssemblyHelper.GetModRootAssembly(mod.t1)));
                }
            }
            CacheTypes = rs;
        }
        return CacheTypes;
    }

    private static List<Tuple<string, CacheAttribute, Type>> GetModLibCacheTypes(string modId, Assembly ass)
    {
        DebugHelper.WriteLine($"{AssemblyHelper.GetModPathRootAssembly(modId)}\\{ass?.FullName}");
        var rs = ass.GetLoadableTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(CachableObject)) && x.GetCustomAttribute<CacheAttribute>() != null)
            .Select(x => Tuple.Create(modId, x.GetCustomAttribute<CacheAttribute>(), x)).ToList();
        DebugHelper.WriteLine($"{modId}: Loaded!!!");
        return rs.OrderBy(x => x.Item2.OrderIndex).ToList();
    }

    private static List<Tuple<string, CacheAttribute, Type>> GetModChildCacheTypes(string modId, Assembly ass)
    {
        DebugHelper.WriteLine($"{AssemblyHelper.GetModPathRootAssembly(modId)}\\{ass?.FullName}");
        if (ass == null)
        {
            DebugHelper.WriteLine($"{modId}: DLL file is not exists!!!");
            return EMPTY;
        }
        var rs = ass.GetLoadableTypes()
            .Where(x => 
                x.IsClass && 
                x.IsSubclassOf(typeof(CachableObject)) && 
                x.GetCustomAttribute<CacheAttribute>() != null)
            .Select(x => Tuple.Create(modId, x.GetCustomAttribute<CacheAttribute>(), x)).ToList();
        if (rs.Count(x => x.Item3.IsSubclassOf(typeof(ModChild))) > 1)
        {
            DebugHelper.WriteLine($"{modId}: Only 1 ModChild in mod!!!");
            return EMPTY;
        }
        var child = rs.FirstOrDefault(x => x.Item3.IsSubclassOf(typeof(ModChild)));
        if (child == null)
        {
            DebugHelper.WriteLine($"{modId}: This mod is not powered by ModLib or you have to declare a ModChild!!!");
            return EMPTY;
        }
        else
        {
            child.Item2.OrderIndex = -1;
            child.Item2.CacheType = CacheAttribute.CType.Global;
            child.Item2.WorkOn = CacheAttribute.WType.All;
        }
        if (child.Item1 != child.Item2.CacheId)
        {
            DebugHelper.WriteLine($"{modId}: ModChild's CacheId must be same ModId!!! ({child.Item1})");
            return EMPTY;
        }
        var dupCheck = rs.FirstOrDefault(x => rs.Any(y => x != y && x.Item2.CacheId == y.Item2.CacheId));
        if (dupCheck != null)
        {
            DebugHelper.WriteLine($"{modId}-{dupCheck.Item2.CacheId}: Exists another CachableObject has same CacheId!!!");
            return EMPTY;
        }
        var orderCfg = child.Item3.GetCustomAttribute<ModOrderAttribute>();
        if (orderCfg != null && File.Exists(ConfHelper.GetConfFilePath(modId, orderCfg.OrderFile)))
        {
            var orderList = JsonConvert.DeserializeObject<Dictionary<string, int>>(ConfHelper.ReadConfData(modId, orderCfg.OrderFile));
            foreach (var r in rs)
            {
                if (orderList.ContainsKey(r.Item2.CacheId))
                    r.Item2.OrderIndex = orderList[r.Item2.CacheId];
            }
        }
        var defOrderIndex = 9000;
        foreach (var r in rs.Where(x => x.Item2.OrderIndex == 0))
        {
            r.Item2.OrderIndex = defOrderIndex++;
        }
        var orderCheck = rs.FirstOrDefault(x => !x.Item3.IsSubclassOf(typeof(ModChild)) && x.Item2.OrderIndex < 0);
        if (orderCheck != null)
        {
            DebugHelper.WriteLine($"{modId}-{orderCheck.Item2.CacheId}: OrderIndex must greater than 0!!!");
            return EMPTY;
        }
        DebugHelper.WriteLine($"{modId}: Loaded!!!");
        return rs.OrderBy(x => x.Item2.OrderIndex).ToList();
    }

    public static void Order()
    {
        var orderList = GetCacheTypes();
        CacheData = CacheData.OrderBy(x =>
        {
            return orderList.IndexOf(y => y.Item1 == x.Key && y.Item2.CacheId == x.Value.CacheId);
        }).ToDictionary(x => x.Key, x => x.Value);
    }
}