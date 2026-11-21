using MOD_nE7UL2.Object;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Custom refine (CustomRefine slots), read from ModConf/custom_refine_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class CustomRefineConfig : JsonConfig<CustomRefineConfig>
    {
        public const string FILE = "custom_refine_configs.json";
        public override string FileName => FILE;

        /// <summary>Chance in % of the quality of a slot (AdjLevelEnum names); total 100.</summary>
        public Dictionary<string, float> LevelRates { get; set; } = new Dictionary<string, float>
        {
            ["Common"] = 30.0f,
            ["Uncommon"] = 20.0f,
            ["Rare"] = 20.0f,
            ["Myst"] = 16.0f,
            ["Lgendary"] = 8.0f,
            ["Beast"] = 4.0f,
            ["Antique"] = 2.0f,
        };

        protected override void Fix(CustomRefineConfig d)
        {
            if (LevelRates == null || LevelRates.Count == 0 || LevelRates.Values.Sum() <= 0f)
                LevelRates = d.LevelRates;
        }
        // no game types here (the unit tests read this class outside the game): the random level is CustomRefine.RandomRefineLevel
    }
}
