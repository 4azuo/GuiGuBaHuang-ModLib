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

namespace MOD_nE7UL2.AutoTest.Scenarios.Progression
{
    /// <summary>
    /// TG-22 (RebirthEvent): Rebirth state and luck.
    /// </summary>
    public class TG22_RebirthEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-22";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-22", Feature = "RebirthEvent", Title = "Rebirth state and luck" }
                .Step("rebirth", c =>
                {
                    var r = RebirthEvent.Instance;
                    c.Assert(r.RebirthCount >= 0 && r.RebirthLevel >= 0 && r.TotalGradeLvl >= 0, $"count {r.RebirthCount}, level {r.RebirthLevel}, total {r.TotalGradeLvl}");
                    if (r.RebirthCount > 0)
                        c.Assert(Player.GetLuck(RebirthEvent.LUCK_BASE_ID + r.RebirthCount) != null, "player has the rebirth luck");
                });
        }
    }
}
