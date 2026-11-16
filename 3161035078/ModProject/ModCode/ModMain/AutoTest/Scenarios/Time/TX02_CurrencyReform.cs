using ModLib.Helper;
using MOD_nE7UL2.Mod;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TX-02 (InflationaryEvent): when the yearly price index passes LIMIT a currency reform happens: index reset,
    /// money divided by 10000, town budgets cut by 80 %, drama shown. Changes the save (restored by run-autotest.ps1).
    /// </summary>
    public class TX02_CurrencyReform : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TX-02";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TX-02", Feature = "InflationaryEvent", Title = "Currency reform when the price index passes the limit", Overlay = false }
                .Step("reform", c =>
                {
                    var ev = InflationaryEvent.Instance;
                    var town = AnyTown;
                    Player.SetUnitMoney(12340000);
                    var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    var corruption = ev.Corruption;
                    EconomyHelper.SetWorldPrice(InflationaryEvent.LIMIT);
                    c.Info($"test data: index {InflationaryEvent.LIMIT}, money 12,340,000, {town.name} budget {budget}");
                    ev.OnYearly();
                    c.AssertEqual(corruption + 1, ev.Corruption, "reform counted");
                    c.Assert(EconomyHelper.GetWorldPrice() <= InflationaryEvent.LIMIT, $"index reset to {EconomyHelper.GetWorldPrice()}");
                    c.AssertEqual(1234, Player.GetUnitMoney(), "money divided by 10000");
                    var cut = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Assert(System.Math.Abs(budget - (long)System.Math.Round(budget * 0.8) - cut) <= 1, $"town budget cut by 80 % ({budget} -> {cut}, rounded)");
                    c.AssertEqual(0, RealMarketEvent3.Instance.MarketStack.Count, "market cleared");
                })
                .Step("drama", c => c.Info("reform drama shown"), 1500)
                .Step("close", c => CloseAll(UIType.DramaDialogue), 500);
        }
    }
}
