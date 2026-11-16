using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ModLib.Helper
{
    /// <summary>
    /// Removes the mod data of saves deleted in the game, at startup.
    /// The game names a save folder "&lt;slot&gt;_&lt;key&gt;" (CacheData), unrelated to the player unit id that ModLib and its mods
    /// use: "mod\&lt;modId&gt;\saves\&lt;id&gt;\", "mod\&lt;modId&gt;\&lt;id&gt;_configs.json", "mod\&lt;modId&gt;\&lt;id&gt;_debug-*.log".
    /// So each loaded save is recorded (save folder -> player unit id, mod\flQrxq\save-index.json). At startup, an id is
    /// removed only when every folder recorded for it is gone from the save list. Ids never recorded (saves not loaded
    /// since this version) are kept.
    /// </summary>
    public static class SaveCleanupHelper
    {
        public const string INDEX_FILE = "save-index.json";
        private static readonly Regex ID_FILE = new Regex(@"^(?<id>[A-Za-z0-9]{6})_(configs\.json|debug-\d{8}\.log)$");

        /// <summary>Game data folder (LocalLow\guigugame\guigubahuang).</summary>
        public static string GameDataPath => $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}Low\\guigugame\\guigubahuang";

        public static string IndexPath(string gameDataPath, string modLibId = "flQrxq") => Path.Combine(gameDataPath, "mod", modLibId, INDEX_FILE);

        /// <summary>Save folder -> player unit id of the saves loaded so far.</summary>
        public static Dictionary<string, string> ReadIndex(string indexPath)
        {
            try
            {
                return File.Exists(indexPath) ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(indexPath)) ?? new Dictionary<string, string>() : new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        public static void WriteIndex(string indexPath, Dictionary<string, string> index)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(indexPath));
            File.WriteAllText(indexPath, JsonConvert.SerializeObject(index, Formatting.Indented));
        }

        /// <summary>Records the save folder of the loaded save.</summary>
        public static void Register(string indexPath, string folder, string playerUnitId)
        {
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(playerUnitId))
                return;
            var index = ReadIndex(indexPath);
            if (index.TryGetValue(folder, out var cur) && cur == playerUnitId)
                return;
            index[folder] = playerUnitId;
            WriteIndex(indexPath, index);
        }

        /// <summary>Player unit ids whose recorded save folders are all gone from the existing save folders.</summary>
        public static List<string> DeletedIds(Dictionary<string, string> index, ICollection<string> existingFolders)
        {
            var alive = new HashSet<string>(index.Where(x => existingFolders.Contains(x.Key)).Select(x => x.Value));
            return index.Where(x => !existingFolders.Contains(x.Key) && !alive.Contains(x.Value)).Select(x => x.Value).Distinct().ToList();
        }

        /// <summary>Data of the ids in the mod folders (saves folders, configs, debug logs).</summary>
        public static List<string> FindData(string gameDataPath, ICollection<string> ids)
        {
            var rs = new List<string>();
            var modRoot = Path.Combine(gameDataPath, "mod");
            if (ids.Count == 0 || !Directory.Exists(modRoot))
                return rs;
            foreach (var mod in Directory.GetDirectories(modRoot))
            {
                var saves = Path.Combine(mod, "saves");
                if (Directory.Exists(saves))
                    rs.AddRange(Directory.GetDirectories(saves).Where(x => ids.Contains(Path.GetFileName(x))));
                foreach (var file in Directory.GetFiles(mod))
                {
                    var m = ID_FILE.Match(Path.GetFileName(file));
                    if (m.Success && ids.Contains(m.Groups["id"].Value))
                        rs.Add(file);
                }
            }
            return rs;
        }

        /// <summary>
        /// Removes the data of the deleted saves (existingFolders: the save folders of the game now; null or empty: nothing
        /// is done) and forgets them in the index. Returns the removed paths; a locked file is skipped.
        /// </summary>
        public static List<string> CleanDeletedSaves(string gameDataPath, ICollection<string> existingFolders)
        {
            var removed = new List<string>();
            if (existingFolders == null || existingFolders.Count == 0)
                return removed;
            var indexPath = IndexPath(gameDataPath);
            var index = ReadIndex(indexPath);
            var ids = DeletedIds(index, existingFolders);
            foreach (var path in FindData(gameDataPath, ids))
            {
                try
                {
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                    else File.Delete(path);
                    removed.Add(path);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            foreach (var folder in index.Keys.Where(x => !existingFolders.Contains(x)).ToList())
                index.Remove(folder);
            WriteIndex(indexPath, index);
            return removed;
        }
    }
}
