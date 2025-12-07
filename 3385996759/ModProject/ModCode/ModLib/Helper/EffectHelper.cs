using ModLib.Attributes;
using System.Reflection;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for working with game effects.
    /// Provides utilities for effect reflection and discovery.
    /// </summary>
    //Effect3017: Shield
    [ActionCat("Effect")]
    public static class EffectHelper
    {
        /// <summary>
        /// Debug method to show all static methods in Effect classes (0000-9999).
        /// </summary>
        public static void ShowStaticMethods()
        {
            var ass = Assembly.LoadFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\鬼谷八荒\\MelonLoader\\Managed\\Assembly-CSharp.dll");
            for (int i = 0; i < 10000; i++)
            {
                var t = ass.GetType($"Effect{i:0000}");
                if (t != null)
                {
                    DebugHelper.WriteLine($"{t.Name}");
                    foreach (var m in t.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        DebugHelper.WriteLine($"　{m.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Gets a specific value from effect's underscore-delimited value string.
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="roleEfx">Role effect item</param>
        /// <param name="index">Index in the value string</param>
        /// <returns>Parsed value</returns>
        public static T GetEfxValue<T>(this ConfRoleEffectItem roleEfx, int index)
        {
            var info = roleEfx.value.Split('_');
            return info[index].Parse<T>();
        }

        /// <summary>
        /// Sets a specific value in effect's underscore-delimited value string.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="roleEfx">Role effect item</param>
        /// <param name="index">Index in the value string</param>
        /// <param name="value">New value</param>
        public static void SetEfxValue<T>(this ConfRoleEffectItem roleEfx, int index, T value)
        {
            var info = roleEfx.value.Split('_');
            info[index] = value.ToString();
            roleEfx.value = string.Join("_", info);
        }
    }
}