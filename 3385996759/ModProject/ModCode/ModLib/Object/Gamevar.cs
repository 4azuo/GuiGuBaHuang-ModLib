using Newtonsoft.Json;

namespace ModLib.Object
{
    public class Gamevar : CachableObject
    {
        //[JsonIgnore]
        //public bool LoadMapFirst { get; set; } = true;
        [JsonIgnore]
        public bool LoadGameBefore { get; set; } = true;
        [JsonIgnore]
        public bool LoadGame { get; set; } = true;
        [JsonIgnore]
        public bool LoadGameAfter { get; set; } = true;



        //public bool LoadMapNewGame { get; set; } = true;
        public bool LoadFirstMonth { get; set; } = true;
        public bool LoadNewGame { get; set; } = true;
        public int CurMonth { get; set; } = -1;
    }
}
