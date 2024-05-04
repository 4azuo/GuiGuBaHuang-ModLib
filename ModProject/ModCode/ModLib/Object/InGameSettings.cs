using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace ModLib.Object
{
    public class InGameSettings : CachableObject
    {
        private const string MOD_SETTINGS_KEY = "*ModSettings";

        public static T CreateIfNotExists<T>(T defaultSettings = null) where T : InGameSettings
        {
            return (T)CreateIfNotExists(typeof(T), defaultSettings);
        }

        [Obsolete]
        public static object CreateIfNotExists(Type sttType, InGameSettings defaultSettings = null)
        {
            return CacheHelper.GetGameCache().GetData(sttType, MOD_SETTINGS_KEY, defaultSettings);
        }

        public static T GetSettings<T>() where T : InGameSettings
        {
            return CreateIfNotExists<T>();
        }

        [Obsolete]
        public static object GetSettings(Type sttType)
        {
            return CreateIfNotExists(sttType);
        }

        public static void ReplaceData(CachableObject replacementValue)
        {
            CacheHelper.GetGameCache().SetData(MOD_SETTINGS_KEY, replacementValue);
        }

        [JsonIgnore]
        public bool LoadGameBefore { get; set; } = true;
        [JsonIgnore]
        public bool LoadGame { get; set; } = true;
        [JsonIgnore]
        public bool LoadGameAfter { get; set; } = true;

        [Inheritance]
        public bool LoadGameFirst { get; set; } = true;
        [Inheritance]
        public int CurMonth { get; set; } = -1;

        #region Custom
        public const string ConfCustomConfigFile = "game_configs.json";
        public const string ConfCustomConfigVersion = "config ver[1.0.2] update 2024/5/4";
        public string CustomConfigFile { get; set; }
        public string CustomConfigVersion { get; set; }

        public InGameSettings InitCustomSettings()
        {
            CustomConfigFile = ConfCustomConfigFile;
            CustomConfigVersion = ConfCustomConfigVersion;
            return this;
        }
        #endregion
    }
}
