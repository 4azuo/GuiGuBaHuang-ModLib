using ModLib.Helper;
using MOD_nE7UL2.Mod;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>TG-30 (UIEvent/MapBuildPropertyEvent): travel to a town and enter it; Taoist town info is shown.</summary>
    public class TG30_Town : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-30";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-30", Feature = "UIEvent/MapBuildPropertyEvent", Title = "Enter the town: town UI with Taoist info", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => true)))
                .Step("enter town", c =>
                {
                    var town = BagTown(c);
                    c.Info($"{town.name}: budget {MapBuildPropertyEvent.GetBuildProperty(town)}, master {MapBuildPropertyEvent.GetTownMaster(town)?.GetName() ?? "-"}, guardians {MapBuildPropertyEvent.GetTownGuardians(town).Count}");
                    town.OpenBuild();
                })
                .Wait("town opened", c => g.ui.HasUI(UIType.Town), 10000)
                .Step("check", c => c.Info($"custom covers on Town: {CoverCount(UIType.Town)}"), 2000)
                .Step("leave town", c =>
                {
                    CloseAll(UIType.Town);
                    c.Assert(!g.ui.HasUI(UIType.Town), "town UI closed");
                });
        }
    }
}
