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

namespace MOD_nE7UL2.AutoTest.Scenarios.UI
{
    /// <summary>
    /// TG-24 (TimeSkipEvent/UIEvent): Map screen shows Taoist buttons and hides vanilla widgets.
    /// </summary>
    public class TG24_TimeSkipEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-24";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-24", Feature = "TimeSkipEvent/UIEvent", Title = "Map screen shows Taoist buttons and hides vanilla widgets" }
                .Step("map", c =>
                {
                    c.Assert(CoverCount(UIType.MapMain) >= 1, $"custom covers on MapMain: {CoverCount(UIType.MapMain)}");
                    var ui = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                    c.Assert(!ui.playerInfo.goAddLuckRoot.activeSelf, "add-luck widget hidden");
                });
        }
    }
}
