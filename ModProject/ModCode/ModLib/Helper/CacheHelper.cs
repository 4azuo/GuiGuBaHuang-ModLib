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

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for managing persistent cache data across game sessions.
    /// Supports skill-level, game-level (per save), and global caches.
    /// Handles serialization and file operations for cachable objects.
    /// </summary>
    [ActionCat("Cache")]
    public static class CacheHelper
    {
        /// <summary>
        /// JSON serializer settings for cache data with type handling and error tolerance.
        /// </summary>
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
        
        /// <summary>
        /// Gets or sets the list of all cache types discovered in loaded mods.
        /// </summary>
        public static List<Tuple<string, CacheAttribute, Type>> CacheTypes { get; private set; }
        
        /// <summary>
        /// Gets the dictionary of all currently loaded cache data objects.
        /// </summary>
        public static Dictionary<string, CachableObject> CacheData { get; private set; } = new Dictionary<string, CachableObject>();

        /// <summary>
        /// Gets the filename for a skill-level cache. Requires an active game.
        /// </summary>
        /// <param name="cacheId">The cache identifier</param>
        /// <returns>The cache filename</returns>
        /// <exception cref="FileNotFoundException">Thrown if not in game</exception>
        public static string GetSkillCacheFileName(string cacheId)
        {
            if (GameHelper.IsInGame())
                return $"{cacheId}_data.json";
            throw new FileNotFoundException();
        }

        /// <summary>
        /// Gets the filename for a game-level cache. Requires an active game.
        /// </summary>
        /// <param name="cacheId">The cache identifier</param>
        /// <returns>The cache filename</returns>
        /// <exception cref="FileNotFoundException">Thrown if not in game</exception>
        public static string GetGameCacheFileName(string cacheId)
        {
            if (GameHelper.IsInGame())
                return $"{cacheId}_data.json";
            throw new FileNotFoundException();
        }

        /// <summary>
        /// Gets the filename for a global cache (not tied to a specific save).
        /// </summary>
        /// <param name="cacheId">The cache identifier</param>
        /// <returns>The cache filename</returns>
        public static string GetGlobalCacheFileName(string cacheId)
        {
            return $"{cacheId}_data.json";
        }

        /// <summary>
        /// Gets the folder path for skill-level caches. Creates directory if it doesn't exist.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>Path to the skill cache folder</returns>
        public static string GetSkillCacheFolderName(string modId)
        {
            var p = $"{GetGameCacheFolderName(modId)}\\skills\\";
            if (!Directory.Exists(p))
                Directory.CreateDirectory(p);
            return p;
        }

        /// <summary>
        /// Gets the folder path for game-level caches (per player save). Creates directory if it doesn't exist.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>Path to the game cache folder for the current player</returns>
        public static string GetGameCacheFolderName(string modId)
        {
            var p = $"{GetCacheFolderName(modId)}\\saves\\{g.world.playerUnit.GetUnitId()}\\";
            if (!Directory.Exists(p))
                Directory.CreateDirectory(p);
            return p;
        }

        /// <summary>
        /// Gets the folder path for global caches (shared across saves). Creates directory if it doesn't exist.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>Path to the global cache folder</returns>
        public static string GetGlobalCacheFolderName(string modId)
        {
            var p = $"{GetCacheFolderName(modId)}\\globals\\";
            if (!Directory.Exists(p))
                Directory.CreateDirectory(p);
            return p;
        }

        /// <summary>
        /// Gets the root cache folder for a mod in the user's LocalApplicationData. Creates directory if it doesn't exist.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <returns>Path to the mod's cache root folder</returns>
        public static string GetCacheFolderName(string modId)
        {
            var p = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang\\mod\\{modId}\\";
            if (!Directory.Exists(p))
                Directory.CreateDirectory(p);
            return p;
        }

        /// <summary>
        /// Gets the full file path for a skill-level cache.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <param name="cacheId">The cache identifier</param>
        /// <returns>Complete path to the skill cache file</returns>
        public static string GetSkillCacheFilePath(string modId, string cacheId)
        {
            return Path.Combine(GetSkillCacheFolderName(modId), GetSkillCacheFileName(cacheId));
        }

        /// <summary>
        /// Gets the full file path for a game-level cache.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <param name="cacheId">The cache identifier</param>
        /// <returns>Complete path to the game cache file</returns>
        public static string GetGameCacheFilePath(string modId, string cacheId)
        {
            return Path.Combine(GetGameCacheFolderName(modId), GetGameCacheFileName(cacheId));
        }

        /// <summary>
        /// Gets the full file path for a global cache.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <param name="cacheId">The cache identifier</param>
        /// <returns>Complete path to the global cache file</returns>
        public static string GetGlobalCacheFilePath(string modId, string cacheId)
        {
            return Path.Combine(GetGlobalCacheFolderName(modId), GetGlobalCacheFileName(cacheId));
        }

        /// <summary>
        /// Adds a cachable object to the cache data dictionary.
        /// Logs the cache loading information.
        /// </summary>
        /// <param name="c">The cachable object to add</param>
        public static void AddCachableObject(CachableObject c)
        {
            if (!CacheData.ContainsKey(c.CacheId))
            {
                DebugHelper.WriteLine($"Load Cache: Mod={c.ModId}, Type={c.GetType().FullName}, Id={c.CacheId}, CacheType={c.CacheType}, WorkOn={c.WorkOn}");
                CacheData.Add(c.CacheId, c);
            }
        }

        /// <summary>
        /// Removes a cachable object from the cache data dictionary.
        /// Logs the cache unloading information.
        /// </summary>
        /// <param name="c">The cachable object to remove</param>
        public static void RemoveCachableObject(CachableObject c)
        {
            DebugHelper.WriteLine($"Unload Cache: Mod={c.ModId}, Type={c.GetType().FullName}, Id={c.CacheId}, CacheType={c.CacheType}, WorkOn={c.WorkOn}");
            CacheData.Remove(c.CacheId);
        }

        /// <summary>
        /// Reloads a game-level cachable object from its cache file.
        /// Maps the loaded data back onto the existing object.
        /// </summary>
        /// <typeparam name="T">The cachable object type</typeparam>
        /// <param name="c">The object to reload</param>
        public static void ReloadGameCachableObject<T>(T c) where T : CachableObject
        {
            ObjectHelper.Map<T>(ReadGameCacheFile<T>(c.ModId, c.CacheId), c);
        }

        /// <summary>
        /// Reloads a global cachable object from its cache file.
        /// Maps the loaded data back onto the existing object.
        /// </summary>
        /// <typeparam name="T">The cachable object type</typeparam>
        /// <param name="c">The object to reload</param>
        public static void ReloadGlobalCachableObject<T>(T c) where T : CachableObject
        {
            ObjectHelper.Map<T>(ReadGlobalCacheFile<T>(c.ModId, c.CacheId), c);
        }

        /// <summary>
        /// Gets all global-level cached objects.
        /// </summary>
        /// <returns>List of global cache objects</returns>
        public static List<CachableObject> GetGlobalCaches()
        {
            return GetAllCachableObjects().Where(x => x.CacheType == CacheAttribute.CType.Global).ToList();
        }

        /// <summary>
        /// Gets all game-level (per-save) cached objects.
        /// </summary>
        /// <returns>List of game cache objects</returns>
        public static List<CachableObject> GetGameCaches()
        {
            return GetAllCachableObjects().Where(x => x.CacheType == CacheAttribute.CType.Local).ToList();
        }

        /// <summary>
        /// Gets a specific cachable object by key and type.
        /// </summary>
        /// <typeparam name="T">The cachable object type</typeparam>
        /// <param name="key">The cache ID to search for</param>
        /// <returns>The cached object, or null if not found</returns>
        public static T GetCachableObject<T>(string key) where T : CachableObject
        {
            return GetAllCachableObjects<T>().FirstOrDefault(x => x.CacheId == key);
        }

        /// <summary>
        /// Gets a specific cachable object by type and key.
        /// </summary>
        /// <param name="t">The type of cachable object</param>
        /// <param name="key">The cache ID to search for</param>
        /// <returns>The cached object, or null if not found</returns>
        public static CachableObject GetCachableObject(Type t, string key)
        {
            return GetAllCachableObjects(t).FirstOrDefault(x => x.CacheId == key);
        }

        /// <summary>
        /// Gets all cached objects of a specific type.
        /// </summary>
        /// <typeparam name="T">The cachable object type to filter by</typeparam>
        /// <returns>List of cached objects of the specified type</returns>
        public static List<T> GetAllCachableObjects<T>() where T : CachableObject
        {
            return GetAllCachableObjects().Where(x => typeof(T).IsAssignableFrom(x.GetType())).Cast<T>().ToList();
        }

        /// <summary>
        /// Gets all cached objects of a specific type.
        /// </summary>
        /// <param name="t">The type to filter by</param>
        /// <returns>List of cached objects of the specified type</returns>
        public static List<CachableObject> GetAllCachableObjects(Type t)
        {
            return GetAllCachableObjects().Where(x => t.IsAssignableFrom(x.GetType())).ToList();
        }

        /// <summary>
        /// Gets all currently loaded cached objects.
        /// </summary>
        /// <returns>List of all cached objects</returns>
        public static List<CachableObject> GetAllCachableObjects()
        {
            return CacheData.Values.ToList();
        }

        /// <summary>
        /// Checks if a cache file is readable and can be deserialized.
        /// </summary>
        /// <typeparam name="T">The cachable object type</typeparam>
        /// <param name="modId">The mod identifier</param>
        /// <param name="cacheId">The cache identifier</param>
        /// <returns>True if the file is readable, false otherwise</returns>
        public static bool IsReadable<T>(string modId, string cacheId) where T : CachableObject
        {
            return FileHelper.IsReadable<T>(GetGlobalCacheFilePath(modId, cacheId));
        }

        /// <summary>
        /// Reads a global cache file and deserializes it.
        /// </summary>
        /// <typeparam name="T">The cachable object type</typeparam>
        /// <param name="modId">The mod identifier</param>
        /// <param name="cacheId">The cache identifier</param>
        /// <returns>The deserialized cached object, or null if not found</returns>
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

        /// <summary>
        /// Reads a game-level cache file and deserializes it.
        /// </summary>
        /// <typeparam name="T">The cachable object type</typeparam>
        /// <param name="modId">The mod identifier</param>
        /// <param name="cacheId">The cache identifier</param>
        /// <returns>The deserialized cached object, or null if not found</returns>
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

        /// <summary>
        /// Loads all global cache objects from disk or creates new instances if files don't exist.
        /// Initializes ModLib and ModChild caches.
        /// </summary>
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

        /// <summary>
        /// Loads all game-level cache objects from disk for the current save.
        /// Creates new instances if cache files don't exist.
        /// </summary>
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

        /// <summary>
        /// Removes global cache objects that are only meant for global scope.
        /// Called when entering a game to clean up global-only caches.
        /// </summary>
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

        /// <summary>
        /// Saves a single global cache object to disk.
        /// </summary>
        /// <param name="c">The cache object to save</param>
        public static void SaveGlobalCache(CachableObject c)
        {
            DebugHelper.WriteLine($"Save global-caches: Mod={c.ModId}, Type={c.GetType().FullName}, Id={c.CacheId}, OrderIndex={c.OrderIndex}, CacheType={c.CacheType}, WorkOn={c.WorkOn}");
            File.WriteAllText(GetGlobalCacheFilePath(c.ModId, c.CacheId), JsonConvert.SerializeObject(c, JSON_SETTINGS));
        }

        /// <summary>
        /// Saves all global cache objects to disk.
        /// </summary>
        public static void SaveGlobalCaches()
        {
            foreach (var item in GetGlobalCaches())
            {
                SaveGlobalCache(item);
            }
        }

        /// <summary>
        /// Saves a single game-level cache object to disk.
        /// </summary>
        /// <param name="c">The cache object to save</param>
        public static void SaveGameCache(CachableObject c)
        {
            DebugHelper.WriteLine($"Save game-caches: Mod={c.ModId}, Type={c.GetType().FullName}, Id={c.CacheId}, OrderIndex={c.OrderIndex}, CacheType={c.CacheType}, WorkOn={c.WorkOn}");
            File.WriteAllText(GetGameCacheFilePath(c.ModId, c.CacheId), JsonConvert.SerializeObject(c, JSON_SETTINGS));
        }

        /// <summary>
        /// Saves all game-level cache objects to disk.
        /// </summary>
        public static void SaveGameCaches()
        {
            foreach (var item in GetGameCaches())
            {
                SaveGameCache(item);
            }
        }

        /// <summary>
        /// Saves all cache objects (both global and game-level) to disk.
        /// </summary>
        public static void Save()
        {
            SaveGlobalCaches();
            if (GameHelper.IsInGame())
                SaveGameCaches();
        }

        /// <summary>
        /// Clears all global cache objects from memory and calls their unload handlers.
        /// </summary>
        public static void ClearGlobalCaches()
        {
            foreach (var e in GetGlobalCaches())
            {
                DebugHelper.WriteLine($"Unload GlobalCache: Mod={e.ModId}, Type={e.GetType().FullName}, Id={e.CacheId}");
                e.OnUnloadClass();
                CacheData.Remove(e.CacheId);
            }
        }

        /// <summary>
        /// Clears all game-level cache objects from memory and calls their unload handlers.
        /// </summary>
        public static void ClearGameCaches()
        {
            foreach (var e in GetGameCaches())
            {
                DebugHelper.WriteLine($"Unload GameCache: Mod={e.ModId}, Type={e.GetType().FullName}, Id={e.CacheId}");
                e.OnUnloadClass();
                CacheData.Remove(e.CacheId);
            }
        }

        /// <summary>
        /// Clears all cache objects and resets the cache system.
        /// </summary>
        public static void Clear()
        {
            ClearGlobalCaches();
            ClearGameCaches();
            CacheData.Clear();
            CacheTypes = null;
        }

        /// <summary>
        /// Gets all cache types from loaded mods (both ModLib and mod children).
        /// Caches the result for performance.
        /// </summary>
        /// <returns>List of tuples containing modId, cache attribute, and type</returns>
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

        /// <summary>
        /// Discovers cache types from ModLib assembly.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <param name="ass">The assembly to scan</param>
        /// <returns>List of cache types ordered by OrderIndex</returns>
        private static List<Tuple<string, CacheAttribute, Type>> GetModLibCacheTypes(string modId, Assembly ass)
        {
            DebugHelper.WriteLine($"{AssemblyHelper.GetModPathRootAssembly(modId)}\\{ass?.FullName}");
            var rs = ass.GetLoadableTypes().Where(x => x.IsClass && x.IsSubclassOf(typeof(CachableObject)) && x.GetCustomAttribute<CacheAttribute>() != null)
                .Select(x => Tuple.Create(modId, x.GetCustomAttribute<CacheAttribute>(), x)).ToList();
            DebugHelper.WriteLine($"{modId}: Loaded!!!");
            return rs.OrderBy(x => x.Item2.OrderIndex).ToList();
        }

        /// <summary>
        /// Discovers cache types from a mod child assembly.
        /// Validates that exactly one ModChild exists and loads all cache types.
        /// </summary>
        /// <param name="modId">The mod identifier</param>
        /// <param name="ass">The assembly to scan</param>
        /// <returns>List of cache types ordered by OrderIndex, or empty if validation fails</returns>
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

        /// <summary>
        /// Re-orders all cached objects according to their OrderIndex defined in cache types.
        /// </summary>
        public static void Order()
        {
            var orderList = GetCacheTypes();
            CacheData = CacheData.OrderBy(x =>
            {
                return orderList.IndexOf(y => y.Item1 == x.Key && y.Item2.CacheId == x.Value.CacheId);
            }).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}