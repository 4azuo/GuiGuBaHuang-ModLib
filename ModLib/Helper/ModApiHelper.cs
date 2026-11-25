using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModLib.Helper
{
    /// <summary>
    /// What a mod offers other mods without a reference to its assembly: a public static class (e.g.
    /// "MOD_rLvJjS.Helper.DocApi" of Dao of Creation) whose members take and return plain types, with a "VERSION" const
    /// raised when a member is added (none removed or changed). Another mod finds it (null: the mod is not loaded), checks
    /// its version and calls it; every call here is safe (the default value when the type, the member or the call fails).
    /// </summary>
    public static class ModApiHelper
    {
        private static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        /// <summary>Where the failed calls are written (the ModLib log; tests replace it).</summary>
        public static Action<string> Log { get; set; } = s => DebugHelper.WriteLine(s);

        /// <summary>The API type of a loaded mod (by full name, in every loaded assembly; cached), null when none is loaded.</summary>
        public static Type Find(string typeFullName)
        {
            if (string.IsNullOrEmpty(typeFullName)) return null;
            lock (_types)
            {
                if (_types.TryGetValue(typeFullName, out var cached) && cached != null) return cached;
                Type t = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try { t = asm.GetType(typeFullName, false); } catch { t = null; }
                    if (t != null) break;
                }
                // not cached when missing: the mod may load later
                if (t != null) _types[typeFullName] = t;
                return t;
            }
        }

        /// <summary>The mod of that API is loaded.</summary>
        public static bool IsLoaded(string typeFullName) => Find(typeFullName) != null;

        /// <summary>The VERSION of an API (its public const / static field or property), 0 when there is none or no API.</summary>
        public static int VersionOf(string typeFullName) => Get(typeFullName, "VERSION", 0);

        /// <summary>The API is loaded with a version of at least <paramref name="minVersion"/>.</summary>
        public static bool Has(string typeFullName, int minVersion = 0) => IsLoaded(typeFullName) && VersionOf(typeFullName) >= minVersion;

        /// <summary>A public static property / field of the API, <paramref name="fallback"/> when missing or of another type.</summary>
        public static T Get<T>(string typeFullName, string member, T fallback = default)
        {
            var t = Find(typeFullName);
            if (t == null || string.IsNullOrEmpty(member)) return fallback;
            try
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
                var p = t.GetProperty(member, flags);
                if (p != null) return Convert<T>(p.GetValue(null, null), fallback);
                var f = t.GetField(member, flags);
                if (f != null) return Convert<T>(f.GetValue(null), fallback);
            }
            catch (Exception ex) { Log?.Invoke($"ModApiHelper: {typeFullName}.{member}: {(ex.InnerException ?? ex).Message}"); }
            return fallback;
        }

        /// <summary>
        /// A public static method of the API called with <paramref name="args"/> (chosen by name and number of parameters),
        /// its result as <typeparamref name="T"/>; <paramref name="fallback"/> when missing or when it throws.
        /// </summary>
        public static T Call<T>(string typeFullName, string method, T fallback, params object[] args)
        {
            var m = FindMethod(typeFullName, method, args);
            if (m == null) return fallback;
            try { return Convert<T>(m.Invoke(null, args), fallback); }
            catch (Exception ex) { Log?.Invoke($"ModApiHelper: {typeFullName}.{method}: {(ex.InnerException ?? ex).Message}"); return fallback; }
        }

        /// <summary>A public static method of the API called, its result ignored; false when missing or when it throws.</summary>
        public static bool Invoke(string typeFullName, string method, params object[] args)
        {
            var m = FindMethod(typeFullName, method, args);
            if (m == null) return false;
            try { m.Invoke(null, args); return true; }
            catch (Exception ex) { Log?.Invoke($"ModApiHelper: {typeFullName}.{method}: {(ex.InnerException ?? ex).Message}"); return false; }
        }

        private static MethodInfo FindMethod(string typeFullName, string method, object[] args)
        {
            var t = Find(typeFullName);
            if (t == null || string.IsNullOrEmpty(method)) return null;
            var n = args?.Length ?? 0;
            return t.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(x => x.Name == method && x.GetParameters().Length == n);
        }

        private static T Convert<T>(object value, T fallback)
        {
            if (value == null) return fallback;
            if (value is T v) return v;
            try { return (T)System.Convert.ChangeType(value, typeof(T)); } catch { return fallback; }
        }
    }
}
