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
    /// TG-04 (InflationaryEvent): World price index is applied to prices.
    /// </summary>
    public class TG04_InflationaryEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-04";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-04", Feature = "InflationaryEvent", Title = "World price index is applied to prices" }
                .Step("index", c =>
                {
                    var price = EconomyHelper.GetWorldPrice();
                    c.Info($"world price index: {price}, corruption: {InflationaryEvent.Instance.Corruption}");
                    c.AssertEqual(price, InflationaryEvent.Instance.WorldPrice, "WorldPrice saved = economy index");
                    c.Assert(price > 0 && price <= InflationaryEvent.LIMIT, $"0 < index <= {InflationaryEvent.LIMIT}");
                    c.AssertEqual(Convert.ToInt32(price / 100f * 1000), InflationaryEvent.CalculateInflationary(1000), "CalculateInflationary(1000) = index/100 x 1000");
                    c.AssertEqual(-5, InflationaryEvent.CalculateInflationary(-5), "negative values are not inflated");
                });
        }
    }
}
