using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>TG-33 (RealMarketEvent3): the town market shows the player market buttons; "Market" opens the listings.</summary>
    public class TG33_RealMarketEvent3 : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-33";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-33", Feature = "RealMarketEvent3", Title = "Town market: player market buttons and listings", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => t.GetBuildSub<MapBuildTownMarket>() != null)))
                .Step("open market", c =>
                {
                    var town = CurrentTown;
                    if (town == null) { c.Skip("player is not in a town"); return; }
                    if (town.GetBuildSub<MapBuildTownMarket>() == null) { c.Skip($"{town.name} has no market"); return; }
                    OpenTownUI<UITownMarket>(UIType.TownMarket, ui => ui.InitData(town));
                })
                .Step("buttons", c =>
                {
                    if (!g.ui.HasUI(UIType.TownMarket)) { c.Skip("market not opened"); return; }
                    foreach (var key in new[] { "other500020085", "other500020040", "other500020060", "other500020061" })
                        c.Assert(FindButtons(UIType.TownMarket, GameTool.LS(key)).Any(), $"'{GameTool.LS(key)}' button");
                })
                .Step("open listings", c =>
                {
                    var btn = FindButtons(UIType.TownMarket, GameTool.LS("other500020040")).FirstOrDefault(x => x.GetFormat() == GameTool.LS("other500020040"));
                    if (btn == null) { c.Skip("no 'Market' button"); return; }
                    btn.Item.onClick.Invoke();
                    c.Assert(RealMarketEvent3.IsRealMarket3Open() || g.ui.HasUI(UIType.PropSelect) || g.ui.HasUI(UIType.NPCSearch), "listings window opened");
                    c.Info($"listings: {RealMarketEvent3.Instance.MarketStack.Count(x => !x.IsHidden)} visible / {RealMarketEvent3.Instance.MarketStack.Count}");
                })
                .Step("close", c =>
                {
                    CloseAll(UIType.PropSelect, UIType.NPCSearch, UIType.TownMarket);
                    c.Assert(!g.ui.HasUI(UIType.TownMarket), "market closed");
                });
        }
    }
}
