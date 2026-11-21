using MOD_nE7UL2.Object;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Unit roles (UnitTypeEvent), read from ModConf/unit_type_configs.json; the defaults below are the same as the file.
    /// No game types here (the unit tests read this class outside the game): the random role is UnitTypeEvent.RandomUnitType.
    /// </summary>
    public class UnitTypeConfig : JsonConfig<UnitTypeConfig>
    {
        public const string FILE = "unit_type_configs.json";
        public override string FileName => FILE;

        /// <summary>Chance in % of each role (UnitTypeEnum names); the rest: no role.</summary>
        public Dictionary<string, float> UnitTypes { get; set; } = new Dictionary<string, float>
        {
            ["PowerUnit"] = 8.0f,
            ["SpeedUnit"] = 8.0f,
            ["TaoistUnit"] = 8.0f,
            ["AtkUnit"] = 12.0f,
            ["DefUnit"] = 12.0f,
            ["Angel"] = 2.0f,
            ["Evil"] = 4.0f,
            ["Merchant"] = 16.0f,
        };

        protected override void Fix(UnitTypeConfig d)
        {
            if (UnitTypes == null)
                UnitTypes = d.UnitTypes;
            foreach (var k in UnitTypes.Keys.ToList())
                UnitTypes[k] = Rate(UnitTypes[k]);
        }
    }
}
