using ModLib.Helper;
using MOD_nE7UL2.Mod;
using ModLib.Object;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>TG-34 (RealMarketEvent2): the shop shows the town budget and the buy price rate.</summary>
    public class TG34_RealMarketEvent2 : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-34";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-34", Feature = "RealMarketEvent2", Title = "Shop: town budget and buy price rate", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownMarketPill))))
                .Step("open shop", c =>
                {
                    var town = CurrentTown;
                    if (town == null) { c.Skip("player is not in a town"); return; }
                    if (!town.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)) { c.Skip($"{town.name} has no shop"); return; }
                    OpenTownUI<UITownMarketBuy>(UIType.TownMarketBuy, ui => ui.InitData(town));
                })
                .Step("texts", c =>
                {
                    if (!g.ui.HasUI(UIType.TownMarketBuy)) { c.Skip("shop not opened"); return; }
                    var texts = UIHelper.GetUICustomBase(UIType.TownMarketBuy).SelectMany(x => x.Items).OfType<UIItemText>().Select(x => TextIconHelper.GetShownText(x.InnerText)).ToList();
                    c.Assert(texts.Any(x => x.StartsWith("Market:")), $"budget text: {texts.FirstOrDefault(x => x.StartsWith("Market:"))}");
                    c.Assert(texts.Any(x => x.StartsWith("Price rate:")), $"rate text: {texts.FirstOrDefault(x => x.StartsWith("Price rate:"))}");
                    c.AssertBetween(RealMarketEvent2.GetBuyRate(CurrentTown, Player), 1, 1000, "buy rate (%)");
                })
                .Step("close", c =>
                {
                    CloseAll(UIType.TownMarketBuy);
                    c.Assert(!g.ui.HasUI(UIType.TownMarketBuy), "shop closed");
                });
        }
    }
}
