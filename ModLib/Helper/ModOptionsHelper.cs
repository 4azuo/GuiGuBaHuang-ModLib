using ModLib.Object;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModLib.Helper
{
    /// <summary>
    /// Options of the mods a player may change (ModOptions): the schema shipped in ModConf/&lt;prefix&gt;_options.json,
    /// the values chosen in a JSON file of the global cache folder of the mod (outside the Workshop folder: an update keeps
    /// them; created with the defaults), applied to the mod's settings object. A settings window (Taoist S&amp;M Configs)
    /// lists the loaded mods with options (LoadedOptions) and shows a page per mod (ModOptionsPage).
    /// </summary>
    public static class ModOptionsHelper
    {
        /// <summary>A schema file of ModConf: its name ends with this.</summary>
        public const string SCHEMA_SUFFIX = "_options.json";
        /// <summary>The values file, in the global cache folder of the mod.</summary>
        public const string VALUES_FILE = "options.json";

        /// <summary>The schema of a mod: its ModConf/*_options.json (one per mod: the first by name, the others logged and
        /// skipped - they would share one values file); null: none.</summary>
        public static ModOptions ReadSchema(string modId)
        {
            var dir = ConfHelper.GetConfFolderPath(modId);
            if (!Directory.Exists(dir)) return null;
            var files = Directory.GetFiles(dir, "*" + SCHEMA_SUFFIX).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();
            if (files.Count == 0) return null;
            if (files.Count > 1) DebugHelper.WriteLine($"Mod options of {modId}: {Path.GetFileName(files[0])} used, skipped: {string.Join(", ", files.Skip(1).Select(Path.GetFileName))}");
            return ParseSchema(File.ReadAllText(files[0]));
        }

        public static ModOptions ParseSchema(string json) =>
            string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<ModOptions>(json);

        /// <summary>Every loaded mod with options: mod id, schema.</summary>
        public static List<KeyValuePair<string, ModOptions>> LoadedOptions()
        {
            var rs = new List<KeyValuePair<string, ModOptions>>();
            foreach (var mod in g.mod.allModPaths)
            {
                try
                {
                    if (!g.mod.IsLoadMod(mod.t1)) continue;
                    var schema = ReadSchema(mod.t1);
                    if (schema != null && schema.Options.Count > 0) rs.Add(new KeyValuePair<string, ModOptions>(mod.t1, schema));
                }
                catch (Exception ex) { DebugHelper.WriteLine($"Mod options of {mod.t1}: {ex.Message}"); }
            }
            return rs;
        }

        /// <summary>Where the values of a mod are kept.</summary>
        public static string ValuesFilePath(string modId) => Path.Combine(CacheHelper.GetGlobalCacheFolderName(modId), VALUES_FILE);

        /// <summary>The values chosen for a mod (key -> value; empty: none yet; null: the file cannot be read - it is
        /// never overwritten then, the player may fix it).</summary>
        public static Dictionary<string, object> ReadValues(string modId)
        {
            var f = ValuesFilePath(modId);
            if (!File.Exists(f)) return new Dictionary<string, object>();
            try { return ParseValues(File.ReadAllText(f)); }
            catch (Exception ex) { DebugHelper.WriteLine($"Mod options of {modId}: {f} not read (kept as it is): {ex.Message}"); return null; }
        }

        public static Dictionary<string, object> ParseValues(string json)
        {
            var rs = new Dictionary<string, object>();
            if (string.IsNullOrWhiteSpace(json)) return rs;
            foreach (var p in JObject.Parse(json).Properties())
                rs[p.Name] = (p.Value as JValue)?.Value;
            return rs;
        }

        public static void WriteValues(string modId, IDictionary<string, object> values) =>
            File.WriteAllText(ValuesFilePath(modId), JsonConvert.SerializeObject(values, Formatting.Indented));

        /// <summary>The value of an option within its bounds (a number rounded to its step; null: its default).</summary>
        public static object Clamp(ModOption o, object value)
        {
            if (o == null) return value;
            var v = value ?? o.Default;
            switch ((o.Type ?? "float").ToLowerInvariant())
            {
                case "bool":
                    return v is bool b ? b : v != null && bool.TryParse(Convert.ToString(v, CultureInfo.InvariantCulture), out var bb) && bb;
                case "int":
                case "select":
                    {
                        var max = o.Type == "select" && o.Choices != null ? o.Choices.Count - 1 : o.Max;
                        var d = Math.Max(o.Min, Math.Min(max, ToDouble(v)));
                        return (int)Math.Round(o.Step > 0 ? Math.Round(d / o.Step) * o.Step : d);
                    }
                default:
                    {
                        var d = Math.Max(o.Min, Math.Min(o.Max, ToDouble(v)));
                        return o.Step > 0 ? Math.Round(Math.Round(d / o.Step) * o.Step, 6) : d;
                    }
            }
        }

        private static double ToDouble(object v)
        {
            try { return v == null ? 0 : Convert.ToDouble(v, CultureInfo.InvariantCulture); } catch { return 0; }
        }

        /// <summary>
        /// The values set on the settings object (each Key a dotted path of properties), within their bounds; options without
        /// a value keep the setting as it is. Returns how many were set.
        /// </summary>
        public static int Apply(object settings, ModOptions schema, IDictionary<string, object> values)
        {
            if (settings == null || schema == null || values == null) return 0;
            var n = 0;
            foreach (var o in schema.Options)
            {
                if (o?.Key == null || !values.TryGetValue(o.Key, out var raw)) continue;
                try { if (SetPath(settings, o.Key, Clamp(o, raw))) n++; }
                catch (Exception ex) { DebugHelper.WriteLine($"Mod option {o.Key}: {ex.Message}"); }
            }
            return n;
        }

        /// <summary>The values of the options as they are now in the settings object (the defaults of a new values file).</summary>
        public static Dictionary<string, object> Current(object settings, ModOptions schema)
        {
            var rs = new Dictionary<string, object>();
            if (settings == null || schema == null) return rs;
            foreach (var o in schema.Options)
            {
                if (o?.Key == null) continue;
                var v = GetPath(settings, o.Key);
                rs[o.Key] = Clamp(o, v ?? o.Default);
            }
            return rs;
        }

        /// <summary>
        /// A mod applies its options (at its load and every world load: a settings window may have changed them): its
        /// values file created with the current settings when missing (or completed with the new options; keys it does not
        /// know are kept), then applied. An unreadable file is left as it is (the settings stay). Returns the values.
        /// </summary>
        public static Dictionary<string, object> Load(string modId, object settings)
        {
            ModOptions schema;
            try { schema = ReadSchema(modId); }
            catch (Exception ex) { DebugHelper.WriteLine($"Mod options of {modId}: schema: {ex.Message}"); return new Dictionary<string, object>(); }
            if (schema == null) return new Dictionary<string, object>();
            var values = ReadValues(modId);
            if (values == null) return new Dictionary<string, object>();
            var current = Current(settings, schema);
            var missing = current.Keys.Where(k => !values.ContainsKey(k)).ToList();
            foreach (var k in missing) values[k] = current[k];
            if (missing.Count > 0)
            {
                try { WriteValues(modId, values); } catch (Exception ex) { DebugHelper.WriteLine($"Mod options of {modId}: {ex.Message}"); }
            }
            Apply(settings, schema, values);
            return values;
        }

        private static object GetPath(object obj, string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            foreach (var name in path.Split('.'))
            {
                if (obj == null) return null;
                obj = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj);
            }
            return obj;
        }

        private static bool SetPath(object obj, string path, object value)
        {
            if (obj == null || string.IsNullOrEmpty(path)) return false;
            var names = path.Split('.');
            for (var i = 0; i < names.Length - 1; i++)
            {
                obj = obj?.GetType().GetProperty(names[i], BindingFlags.Public | BindingFlags.Instance)?.GetValue(obj);
                if (obj == null) return false;
            }
            var p = obj.GetType().GetProperty(names[names.Length - 1], BindingFlags.Public | BindingFlags.Instance);
            if (p == null || !p.CanWrite) return false;
            p.SetValue(obj, Convert.ChangeType(value, p.PropertyType, CultureInfo.InvariantCulture));
            return true;
        }
    }
}
