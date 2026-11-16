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
    /// TG-25 (QiCulEvent/UseSpiritStoneCulEvent/UpgradeNaturallyEvent): Cultivation screen shows Qi and cultivation buttons.
    /// </summary>
    public class TG25_QiCulEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-25";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-25", Feature = "QiCulEvent/UseSpiritStoneCulEvent/UpgradeNaturallyEvent", Title = "Cultivation screen shows Qi and cultivation buttons" }
                .Step("open", c =>
                {
                    var ui = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                    (ui.playerInfo.btnFateFeature ?? ui.playerInfo.btnFateFeature_En).onClick.Invoke();
                })
                .Step("check", c =>
                {
                    if (!c.Assert(g.ui.HasUI(UIType.FateFeature), "FateFeature opened")) return;
                    c.Assert(CoverCount(UIType.FateFeature) >= 1, $"custom covers on FateFeature: {CoverCount(UIType.FateFeature)}");
                })
                .Step("close", c => { if (g.ui.HasUI(UIType.FateFeature)) g.ui.CloseUI(UIType.FateFeature); });
        }
    }
}
