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

namespace MOD_nE7UL2.AutoTest.Scenarios.Economy
{
    /// <summary>
    /// TG-13 (RealMarketEvent/RealMarketEvent2): Shop sell/buy rates exist for every market town.
    /// </summary>
    public class TG13_RealMarketEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-13";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-13", Feature = "RealMarketEvent/RealMarketEvent2", Title = "Shop sell/buy rates exist for every market town" }
                .Step("rates", c =>
                {
                    var sell = RealMarketEvent.Instance.MarketPriceRate;
                    var buy = RealMarketEvent2.Instance.MarketPriceRate;
                    var towns = ModMain.Store.Buildings.Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)).ToList();
                    c.Assert(towns.All(x => sell.ContainsKey(x.buildData.id)), $"sell rate for {towns.Count} market towns");
                    c.Assert(towns.All(x => buy.ContainsKey(x.buildData.id)), $"buy rate for {towns.Count} market towns");
                    c.Assert(sell.Values.All(x => x > 0), $"sell rates > 0 (min {(sell.Count > 0 ? sell.Values.Min() : 0):0.00})");
                    c.Assert(buy.Values.All(x => x > 0), $"buy rates > 0 (min {(buy.Count > 0 ? buy.Values.Min() : 0):0.00})");
                    var town = Player.GetMapBuild<MapBuildTown>();
                    if (town != null && sell.ContainsKey(town.buildData.id))
                        c.Info($"current town {town.name}: sell {RealMarketEvent.GetSellRate(town, Player):0.00}%, buy {RealMarketEvent2.GetBuyRate(town, Player):0.00}%");
                });
        }
    }
}
