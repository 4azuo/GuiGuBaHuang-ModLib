using Newtonsoft.Json;

namespace ModLib.Object
{
    public class InGameSettings : CachableObject
    {
        [JsonIgnore]
        public bool LoadMapFirst { get; set; } = true;
        [JsonIgnore]
        public bool LoadGameBefore { get; set; } = true;
        [JsonIgnore]
        public bool LoadGame { get; set; } = true;
        [JsonIgnore]
        public bool LoadGameAfter { get; set; } = true;



        public bool LoadMapNewGame { get; set; } = true;
        public bool LoadFirstMonth { get; set; } = true;
        public bool LoadNewGame { get; set; } = true;
        public int CurMonth { get; set; } = -1;



        public string CustomConfigFile { get; set; }
        public int? CustomConfigVersion { get; set; }
        public bool IsOldVersion { get; set; } = false;
    }
}
