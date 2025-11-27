using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper for managing application settings from settings.json
    /// </summary>
    public static class SettingHelper
    {
        private static Dictionary<string, string> _settings;
        private static readonly object _lock = new object();

        /// <summary>
        /// Get setting value by key
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if key not found</param>
        /// <returns>Setting value or default value</returns>
        public static string Get(string key, string defaultValue = null)
        {
            if (_settings?.TryGetValue(key, out var value) ?? false)
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get setting value by key
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <param name="defaultValue">Default value if key not found</param>
        /// <returns>Setting value or default value</returns>
        public static string TryGet(string key, string defaultValue = null)
        {
            try
            {
                return _settings[key];
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get all settings as dictionary
        /// </summary>
        public static Dictionary<string, string> GetAll()
        {
            return new Dictionary<string, string>(_settings);
        }

        /// <summary>
        /// Check if setting key exists
        /// </summary>
        public static bool ContainsKey(string key)
        {
            return _settings.ContainsKey(key);
        }

        /// <summary>
        /// Reload settings from file
        /// </summary>
        public static void Reload()
        {
            lock (_lock)
            {
                _settings = null;
                EnsureLoaded();
            }
        }

        /// <summary>
        /// Ensure settings are loaded
        /// </summary>
        public static void EnsureLoaded()
        {
            if (_settings != null)
                return;

            lock (_lock)
            {
                if (_settings != null)
                    return;

                _settings = LoadDefaultSettings();
                foreach (var kvp in LoadSettings())
                {
                    _settings[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Load settings from embeded-settings.json file
        /// </summary>
        public static Dictionary<string, string> LoadDefaultSettings()
        {
            var asm = Assembly.GetExecutingAssembly();
            using var s = asm.GetManifestResourceStream("ModCreator.Resources.embeded-settings.json")
                ?? throw new Exception("Resource not found.");

            using var r = new StreamReader(s);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(r.ReadToEnd()) 
                   ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Load settings from settings.json file
        /// </summary>
        public static Dictionary<string, string> LoadSettings()
        {
            var settingsPath = Path.Combine(Constants.ResourcesDir, "settings.json");
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(settingsPath))
                   ?? new Dictionary<string, string>();
        }
    }
}
