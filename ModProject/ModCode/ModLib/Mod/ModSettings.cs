using ModLib.Object;
using Newtonsoft.Json;
using System;

namespace ModLib.Mod
{
    public class ModSettings : CachableObject
    {
        private const string MOD_SETTINGS_KEY = "*ModSettings";
        public static T CreateIfNotExists<T>(T defaultSettings = null) where T : ModSettings
        {
            return (T)CreateIfNotExists(typeof(T), defaultSettings);
        }

        public static object CreateIfNotExists(Type sttType, ModSettings defaultSettings = null)
        {
            return CacheHelper.GetGameCache().GetData(sttType, MOD_SETTINGS_KEY, defaultSettings);
        }

        public static T GetSettings<T>() where T : ModSettings
        {
            return CreateIfNotExists<T>();
        }

        public static object GetSettings(Type sttType)
        {
            return CreateIfNotExists(sttType);
        }

        [JsonIgnore]
        public bool LoadGameBefore { get; set; } = true;
        [JsonIgnore]
        public bool LoadGame { get; set; } = true;
        [JsonIgnore]
        public bool LoadGameAfter { get; set; } = true;
        public bool LoadGameFirst { get; set; } = true;
        public int CurMonth { get; set; } = -1;
    }
}
