using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.BattleRule
{
    /// <summary>
    /// TG-18 (BattleModifyEvent): Block table and damage multiplier rules.
    /// </summary>
    public class TG18_BattleModifyEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-18";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-18", Feature = "BattleModifyEvent", Title = "Block table and damage multiplier rules" }
                .Step("rules", c =>
                {
                    var t = BattleModifyEvent.BlockRatio;
                    c.AssertEqual(10, t.Count, "block ratio for 10 grades");
                    c.Assert(t.OrderBy(x => x.Key).Select(x => x.Value).Zip(t.OrderBy(x => x.Key).Select(x => x.Value).Skip(1), (a, b) => a <= b).All(x => x), "block ratio grows with grade");
                    c.AssertEqual(0f, BattleModifyEvent.GetBlockRatio(null), "no unit, no block");
                    c.Assert(BattleModifyEvent.GLOBAL_DMG_MULTIPLIER > 1 && BattleModifyEvent.LOCAL_DMG_MULTIPLIER > 1, "damage multipliers grow over time");
                });
        }
    }
}
