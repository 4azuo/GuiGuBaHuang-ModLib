using HarmonyLib;
using MelonLoader;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

[assembly: MelonInfo(typeof(SlotDataPath.Plugin), "SlotDataPath", "1.0.0", "GuiGuBaHuang-ModLib autotest")]
[assembly: MelonGame(null, null)]

namespace SlotDataPath
{
    /// <summary>
    /// The game data folder of a game slot when the game runs from a "ggbh-slot&lt;n&gt;" folder (nothing changes
    /// otherwise): ...\LocalLow\guigugame\guigubahuang-slot&lt;n&gt; instead of ...\guigubahuang, for
    /// - Application.persistentDataPath (the mods: ModLib reads its data folder from it);
    /// - the save index: DataGloble.CachePath();
    /// - the save manager: CacheMgr keeps its folders in fields (cachePath...) set before the plugins start: they are
    ///   moved on the first frame where g.cache exists (the game reads them at every use); DLCMgr.dlcCachePath.
    /// Harmony patches of Il2Cpp methods (MelonLoader 0.5.4) only catch the calls from managed code (the mods): the game's
    /// own Il2Cpp code calls the native functions directly - hence the fields. Before the first frame the game reads its
    /// save index from the usual folder and writes it back once, unchanged.
    /// </summary>
    public class Plugin : MelonPlugin
    {
        /// <summary>The data folder of this slot (null: not a slot, nothing patched).</summary>
        public static string DataPath { get; private set; }
        /// <summary>The game's own data folder.</summary>
        public static string Original { get; private set; }

        public override void OnApplicationStart()
        {
            var m = Regex.Match(MelonUtils.GameDirectory ?? "", @"ggbh-slot(\d+)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                MelonLogger.Msg("not a game slot: the game data folder is unchanged");
                return;
            }
            Original = UnityEngine.Application.persistentDataPath.TrimEnd('/', '\\');
            DataPath = Original + "-slot" + m.Groups[1].Value;
            Directory.CreateDirectory(DataPath);
            var h = new HarmonyLib.Harmony("SlotDataPath");
            var postfix = new HarmonyMethod(typeof(Plugin), nameof(PathPostfix));
            h.Patch(AccessTools.PropertyGetter(typeof(UnityEngine.Application), nameof(UnityEngine.Application.persistentDataPath)), postfix: postfix);
            h.Patch(AccessTools.Method(typeof(DataGloble), "CachePath"), postfix: postfix);
            foreach (var get in typeof(CacheMgr).GetMethods().Where(x => x.Name == "GetPath" && x.ReturnType == typeof(string)))
                h.Patch(get, postfix: postfix);
            MelonLogger.Msg($"game slot {m.Groups[1].Value}: game data folder {Original} -> {DataPath}");
        }

        private static bool _cacheMoved;
        private bool _dlcMoved;

        /// <summary>The folders of a CacheMgr moved under the slot's data folder (once).</summary>
        private static void MoveCache(CacheMgr cache, string by)
        {
            if (_cacheMoved || DataPath == null || cache == null || string.IsNullOrEmpty(cache.cachePath)) return;
            var before = cache.cachePath;
            cache.cachePath = Fix(cache.cachePath);
            cache.cachePathBack = Fix(cache.cachePathBack);
            cache.skyMonthPathBack = Fix(cache.skyMonthPathBack);
            cache.cachePathNewBackUp = Fix(cache.cachePathNewBackUp);
            // a new slot has no save folder yet: the save list reads it (DirectoryNotFoundException otherwise)
            foreach (var d in new[] { cache.cachePath, cache.cachePathBack, cache.cachePathNewBackUp, cache.skyMonthPathBack })
                if (!string.IsNullOrEmpty(d) && string.IsNullOrEmpty(Path.GetExtension(d))) Directory.CreateDirectory(d);
            _cacheMoved = true;
            MelonLogger.Msg($"CacheMgr ({by}): {before} -> {cache.cachePath}");
        }

        public override void OnUpdate()
        {
            if (DataPath == null || (_cacheMoved && _dlcMoved)) return;
            try
            {
                MoveCache(g.cache, "first frame");
                var dlc = g.dlc;
                if (!_dlcMoved && dlc != null && !string.IsNullOrEmpty(dlc.dlcCachePath))
                {
                    dlc.dlcCachePath = Fix(dlc.dlcCachePath);
                    if (string.IsNullOrEmpty(Path.GetExtension(dlc.dlcCachePath))) Directory.CreateDirectory(dlc.dlcCachePath);
                    _dlcMoved = true;
                    MelonLogger.Msg($"DLCMgr: {dlc.dlcCachePath}");
                }
            }
            catch (Exception ex) { MelonLogger.Warning($"moving the data paths: {ex.Message}"); }
        }

        /// <summary>A path under the game's data folder moved under the slot's.</summary>
        public static string Fix(string path)
        {
            if (DataPath == null || string.IsNullOrEmpty(path)) return path;
            var p = path.Replace('\\', '/');
            if (p.StartsWith(DataPath, StringComparison.OrdinalIgnoreCase)) return path;
            if (!p.StartsWith(Original, StringComparison.OrdinalIgnoreCase)) return path;
            return DataPath + p.Substring(Original.Length);
        }

        private static void PathPostfix(ref string __result) => __result = Fix(__result);
    }
}
