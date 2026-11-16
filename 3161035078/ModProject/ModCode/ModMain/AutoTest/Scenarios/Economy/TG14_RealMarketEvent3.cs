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
    /// TG-14 (RealMarketEvent3): Player market listings and VIP price are consistent.
    /// </summary>
    public class TG14_RealMarketEvent3 : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-14";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-14", Feature = "RealMarketEvent3", Title = "Player market listings and VIP price are consistent" }
                .Step("market", c =>
                {
                    var m = RealMarketEvent3.Instance;
                    c.Info($"listings: {m.MarketStack.Count}, deals: {m.NegotiatingDeals.Count}");
                    c.Assert(m.MarketStack.All(x => x.SellerPrice > 0), "all listings have a positive price");
                    for (var area = 1; area <= 10; area++)
                        c.Assert(RealMarketEvent3.VIPCost(area) > 0, $"VIP cost area {area}: {RealMarketEvent3.VIPCost(area)}");
                });
        }
    }
}
