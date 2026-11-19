using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-37 (RealMarketEvent): the sell window shows rate and town budget; when the town cannot pay (budget below
    /// the total) the sell button is hidden and "Over price" is shown.
    /// </summary>
    public class TG37_RealMarketEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-37";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-37", Feature = "RealMarketEvent", Title = "Sell window: rate, town budget and over-budget block", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownMarketPill))))
                .Step("open sell window", c => g.ui.OpenUISafe<UIPropSell>(UIType.PropSell))
                .Step("budget ok", c =>
                {
                    var ui = g.ui.GetUI<UIPropSell>(UIType.PropSell);
                    if (!c.Assert(ui != null, "sell window opened")) return;
                    c.Assert(FindTexts(ui, "Price rate:").Any(), $"rate text: {FindTexts(ui, "Price rate:").FirstOrDefault()}");
                    c.Assert(FindTexts(ui, "Market:").Any(), $"budget text: {FindTexts(ui, "Market:").FirstOrDefault()}");
                    c.Assert(ui.btnOK.gameObject.activeSelf, "sell button visible while the town can pay");
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(BagTown(c));
                }, 1500)
                .Step("town cannot pay", c =>
                {
                    var town = BagTown(c);
                    MapBuildPropertyEvent.AddBuildProperty(town, -((long)c.Bag["budget"] + 1));
                    c.Info($"town budget set to {MapBuildPropertyEvent.GetBuildProperty(town)} for the test");
                })
                .Step("over budget", c =>
                {
                    var ui = g.ui.GetUI<UIPropSell>(UIType.PropSell);
                    c.Assert(!ui.btnOK.gameObject.activeSelf, "sell button hidden");
                    c.Assert(FindTexts(ui, GameTool.LS("ui_over_price")).Any(), $"'{GameTool.LS("ui_over_price")}' warning shown");
                }, 1000)
                .Step("restore", c =>
                {
                    var town = BagTown(c);
                    MapBuildPropertyEvent.AddBuildProperty(town, (long)c.Bag["budget"] - MapBuildPropertyEvent.GetBuildProperty(town));
                    c.AssertEqual((long)c.Bag["budget"], MapBuildPropertyEvent.GetBuildProperty(town), "budget restored");
                    CloseAll(UIType.PropSell);
                });
        }
    }
}
