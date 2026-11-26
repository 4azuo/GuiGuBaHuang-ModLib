using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModLib.Helper
{
    /// <summary>
    /// Numbers of the play kept by a mod for its balance (damage of a cast, share of HP taken, time of a battle...): a
    /// file per mod in its global cache folder (balance-stats.json, never sent anywhere), so that a player's report of
    /// "too strong" / "too weak" can come with numbers.
    /// <para>
    /// Add(modId, group, key, value) adds a value to a series (count, sum, min, max and a random sample of at most
    /// SAMPLE_SIZE values: median, p90). The series are written at every save and battle end (ModMaster: FlushAll) and when
    /// the game quits. A new version of the mod (SetModVersion) starts a new file: the one of the previous version is
    /// kept as balance-stats-&lt;version&gt;.json. Off (SetEnabled false): nothing is kept.
    /// </para>
    /// </summary>
    public static class BalanceStatsHelper
    {
        public const string FILE = "balance-stats.json";
        /// <summary>Values kept per series for the median / p90 (a random sample of every value added).</summary>
        public const int SAMPLE_SIZE = 200;
        public const int FORMAT = 1;

        public class Series
        {
            [JsonProperty("n")] public long Count;
            [JsonProperty("sum")] public double Sum;
            [JsonProperty("min")] public double Min = double.MaxValue;
            [JsonProperty("max")] public double Max = double.MinValue;
            [JsonProperty("mean")] public double Mean => Count == 0 ? 0 : Round(Sum / Count);
            [JsonProperty("p50")] public double P50 => Percentile(0.5);
            [JsonProperty("p90")] public double P90 => Percentile(0.9);
            [JsonProperty("sample")] public List<double> Sample = new List<double>();

            public void Add(double v, Random rnd)
            {
                Count++;
                Sum += v;
                if (v < Min) Min = v;
                if (v > Max) Max = v;
                // reservoir sampling: every value has the same chance to be in the sample
                if (Sample.Count < SAMPLE_SIZE) Sample.Add(Round(v));
                else
                {
                    var i = (long)(rnd.NextDouble() * Count);
                    if (i < SAMPLE_SIZE) Sample[(int)i] = Round(v);
                }
            }

            public double Percentile(double p)
            {
                if (Sample.Count == 0) return 0;
                var s = Sample.OrderBy(x => x).ToList();
                return s[Math.Min(s.Count - 1, (int)Math.Floor(p * s.Count))];
            }
        }

        public class StatsFile
        {
            [JsonProperty("$format")] public int Format = FORMAT;
            [JsonProperty("mod")] public string ModId;
            [JsonProperty("modVersion")] public string ModVersion;
            [JsonProperty("gameVersion")] public string GameVersion;
            [JsonProperty("since")] public DateTime Since = DateTime.Now;
            [JsonProperty("updated")] public DateTime Updated = DateTime.Now;
            /// <summary>group -> key -> series</summary>
            [JsonProperty("groups")] public SortedDictionary<string, SortedDictionary<string, Series>> Groups = new SortedDictionary<string, SortedDictionary<string, Series>>(StringComparer.Ordinal);
        }

        private static readonly object _sync = new object();
        private static readonly Dictionary<string, StatsFile> _files = new Dictionary<string, StatsFile>();
        private static readonly HashSet<string> _dirty = new HashSet<string>();
        private static readonly HashSet<string> _disabled = new HashSet<string>();
        private static readonly Dictionary<string, string> _versions = new Dictionary<string, string>();
        private static readonly Random _rnd = new Random();

        /// <summary>Test seam: the folder of the files of a mod (default: its global cache folder).</summary>
        public static Func<string, string> FolderOf { get; set; } = modId => CacheHelper.GetGlobalCacheFolderName(modId);

        public static string FilePath(string modId) => Path.Combine(FolderOf(modId), FILE);

        /// <summary>Version of the mod written in its file (call it at init): another version than the file's starts a new file.</summary>
        public static void SetModVersion(string modId, string version)
        {
            if (string.IsNullOrEmpty(modId)) return;
            lock (_sync) _versions[modId] = version;
        }

        /// <summary>On / off for a mod (an option of its players); off: Add does nothing.</summary>
        public static void SetEnabled(string modId, bool enabled)
        {
            if (string.IsNullOrEmpty(modId)) return;
            lock (_sync) { if (enabled) _disabled.Remove(modId); else _disabled.Add(modId); }
        }

        public static bool IsEnabled(string modId) { lock (_sync) return !_disabled.Contains(modId); }

        /// <summary>Adds a value to the series group/key of a mod (NaN and infinities are skipped).</summary>
        public static void Add(string modId, string group, string key, double value)
        {
            if (string.IsNullOrEmpty(modId) || string.IsNullOrEmpty(group) || string.IsNullOrEmpty(key)) return;
            if (double.IsNaN(value) || double.IsInfinity(value)) return;
            try
            {
                lock (_sync)
                {
                    if (_disabled.Contains(modId)) return;
                    var f = Get(modId);
                    if (!f.Groups.TryGetValue(group, out var keys))
                        f.Groups[group] = keys = new SortedDictionary<string, Series>(StringComparer.Ordinal);
                    if (!keys.TryGetValue(key, out var s))
                        keys[key] = s = new Series();
                    s.Add(value, _rnd);
                    f.Updated = DateTime.Now;
                    _dirty.Add(modId);
                }
            }
            catch (Exception ex) { Log($"BalanceStats {modId}: {ex.Message}"); }
        }

        /// <summary>Counts an event (a series of 1s: n is the count).</summary>
        public static void Count(string modId, string group, string key) => Add(modId, group, key, 1);

        /// <summary>The series of a mod (null: none).</summary>
        public static Series Get(string modId, string group, string key)
        {
            lock (_sync)
                return Get(modId).Groups.TryGetValue(group, out var keys) && keys.TryGetValue(key, out var s) ? s : null;
        }

        /// <summary>Writes the files changed since their last write (in the background: CacheHelper.Writer).</summary>
        public static void FlushAll()
        {
            List<KeyValuePair<string, string>> toWrite;
            lock (_sync)
            {
                toWrite = _dirty.Where(_files.ContainsKey)
                    .Select(m => new KeyValuePair<string, string>(FilePath(m), JsonConvert.SerializeObject(_files[m], Formatting.Indented)))
                    .ToList();
                _dirty.Clear();
            }
            foreach (var w in toWrite)
            {
                try { CacheHelper.Writer.Write(w.Key, w.Value); }
                catch (Exception ex) { Log($"BalanceStats: {w.Key}: {ex.Message}"); }
            }
        }

        /// <summary>Clears the series of a mod (its file too).</summary>
        public static void Reset(string modId)
        {
            lock (_sync)
            {
                _files[modId] = NewFile(modId);
                _dirty.Add(modId);
            }
            FlushAll();
        }

        /// <summary>Forgets what was loaded (tests).</summary>
        public static void ClearMemory()
        {
            lock (_sync) { _files.Clear(); _dirty.Clear(); _disabled.Clear(); _versions.Clear(); }
        }

        private static StatsFile Get(string modId)
        {
            if (_files.TryGetValue(modId, out var f)) return f;
            f = Load(modId);
            _files[modId] = f;
            return f;
        }

        private static StatsFile NewFile(string modId)
        {
            _versions.TryGetValue(modId, out var ver);
            string game = null;
            try { game = GameVersion(); } catch { /* outside the game (unit tests) */ }
            return new StatsFile { ModId = modId, ModVersion = ver, GameVersion = game };
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static string GameVersion() => UnityEngine.Application.version;

        /// <summary>The file of the mod, or a new one (none, unreadable, or of another version: that one is kept aside).</summary>
        private static StatsFile Load(string modId)
        {
            var path = FilePath(modId);
            _versions.TryGetValue(modId, out var ver);
            try
            {
                if (File.Exists(path))
                {
                    var f = JsonConvert.DeserializeObject<StatsFile>(File.ReadAllText(path));
                    if (f != null && f.Format == FORMAT && (ver == null || f.ModVersion == ver))
                        return f;
                    var keep = Path.Combine(Path.GetDirectoryName(path), $"balance-stats-{Safe(f?.ModVersion ?? "old")}.json");
                    if (File.Exists(keep)) File.Delete(keep);
                    File.Move(path, keep);
                    Log($"BalanceStats {modId}: version {f?.ModVersion} kept as {Path.GetFileName(keep)}");
                }
            }
            catch (Exception ex) { Log($"BalanceStats {modId}: {path} not read ({ex.Message}): new file"); }
            return NewFile(modId);
        }

        private static string Safe(string s) => string.Concat(s.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));

        private static void Log(string msg)
        {
            try { DebugHelper.WriteLine(msg); } catch { /* outside the game (unit tests) */ }
        }

        private static double Round(double v) => Math.Round(v, 4);
    }
}
