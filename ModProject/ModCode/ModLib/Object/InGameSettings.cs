using ModLib.Const;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace ModLib.Object
{
    public class InGameSettings : CachableObject
    {
        public const string MOD_SETTINGS_KEY = "$ModSettings$";

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

        public static void SetSettings(CachableObject replacementValue)
        {
            CacheHelper.GetGameCache().SetData(MOD_SETTINGS_KEY, replacementValue);
        }

        public static void ClearSettings()
        {
            CacheHelper.GetGameCache().ClearData(MOD_SETTINGS_KEY);
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
        public string CustomConfigFile { get; set; }
        public int? CustomConfigVersion { get; set; }
        public bool IsOldVersion { get; set; } = false;
        #endregion
    }
}
