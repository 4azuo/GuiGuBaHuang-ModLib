using ModLib.Helper;
using MOD_nE7UL2.Mod;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-40 (MapBuildPropertyEvent/RealMarketEvent2, with the trainer): the player becomes town master (trainer
    /// action), the town management window opens, the master gets the shop discount.
    /// </summary>
    public class TG40_TownMaster : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-40";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-40", Feature = "MapBuildPropertyEvent/RealMarketEvent2", Title = "Become town master (trainer) and manage the town", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownMarketPill))))
                .Step("become master", c =>
                {
                    var town = BagTown(c);
                    c.Bag["rate"] = RealMarketEvent2.GetBuyRate(town, Player);
                    new TrainerEvent { Parent = ModMain.ModObj }.BecomeTownMaster();
                    c.Assert(MapBuildPropertyEvent.IsTownMaster(town, Player), $"player is master of {town.name}");
                    c.Assert(MapBuildPropertyEvent.IsTownGuardian(town, Player), "masters are guardians");
                    var rate = RealMarketEvent2.GetBuyRate(town, Player);
                    c.AssertEqual((float)c.Bag["rate"] + RealMarketEvent2.TownMReduceBuyRate, rate, $"master shop rate {c.Bag["rate"]:0.00}% -> {rate:0.00}%");
                })
                .Step("manage town", c => MapBuildPropertyEvent.OpenUITownManage(BagTown(c)))
                .Step("management window", c =>
                {
                    // the management window is the NPC search window (town guardians) with the Taoist cover (tax, budget, buildings)
                    c.Assert(g.ui.HasUI(UIType.NPCSearch) && MapBuildPropertyEvent.isShowManageTeamUI1, "town management window opened");
                    c.Info($"budget {MapBuildPropertyEvent.GetBuildProperty(BagTown(c))}, guardians {MapBuildPropertyEvent.GetTownGuardians(BagTown(c)).Count}, salaries {MapBuildPropertyEvent.GetTotalMonthlyPayment(BagTown(c))}/month");
                }, 1500)
                .Step("close", c => CloseAll(UIType.NPCSearch, UIType.TextInfoLong, UIType.TextInfo));
        }
    }
}
