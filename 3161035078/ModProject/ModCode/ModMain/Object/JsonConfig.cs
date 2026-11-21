using ModLib.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Object
{
    /// <summary>
    /// A settings file of one feature in ModConf (like estate_configs.json): the defaults are the property initializers of
    /// the class (the same as the file), used when the file is missing or broken or a value is left out.
    /// Players can change the file without a new build of the mod.
    /// </summary>
    public abstract class JsonConfig<T> where T : JsonConfig<T>, new()
    {
        /// <summary>The settings in use; the defaults until <see cref="Load"/>.</summary>
        public static T Conf { get; private set; } = Parse(null);

        /// <summary>Name of the file in ModConf.</summary>
        [JsonIgnore]
        public abstract string FileName { get; }

        /// <summary>Reads a config (missing values: the defaults) and makes it safe to use.</summary>
        public static T Parse(string json)
        {
            var c = string.IsNullOrWhiteSpace(json)
                ? new T()
                : JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace }) ?? new T();
            c.Fix(new T());
            return c;
        }

        /// <summary>Loads ModConf/&lt;FileName&gt; (a missing or broken file: the defaults, and a line in the log).</summary>
        public static void Load(string modId)
        {
            var file = new T().FileName;
            try
            {
                Set(Parse(ConfHelper.ReadConfData(modId, file)));
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"{typeof(T).Name}: {file} not read, defaults used: {ex.Message}");
                Set(Parse(null));
            }
        }

        /// <summary>Uses a config (unit tests, the loader).</summary>
        public static void Set(T conf)
        {
            Conf = conf ?? Parse(null);
        }

        /// <summary>Fills what is missing or wrong with the defaults <paramref name="d"/>.</summary>
        protected virtual void Fix(T d) { }

        /// <summary>A chance in %: 0-100.</summary>
        protected static float Rate(float v) => Math.Max(0f, Math.Min(100f, v));

        /// <summary>A dictionary with every key of the defaults (a key left out: its default; null: the defaults).</summary>
        protected static Dictionary<TKey, TValue> Fill<TKey, TValue>(Dictionary<TKey, TValue> v, Dictionary<TKey, TValue> d)
        {
            v = v ?? new Dictionary<TKey, TValue>();
            foreach (var kv in d)
                if (!v.ContainsKey(kv.Key)) v[kv.Key] = kv.Value;
            return v;
        }

        /// <summary>An array of <paramref name="count"/> values (missing ones: the defaults).</summary>
        protected static TValue[] Pad<TValue>(TValue[] v, TValue[] d, int count)
        {
            var rs = new TValue[count];
            for (var i = 0; i < count; i++)
                rs[i] = v != null && i < v.Length ? v[i] : d[i];
            return rs;
        }
    }
}
