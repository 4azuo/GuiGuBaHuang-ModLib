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
    /// TG-12 (RealStorageEvent): Storage fee rate and storage value.
    /// </summary>
    public class TG12_RealStorageEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-12";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-12", Feature = "RealStorageEvent", Title = "Storage fee rate and storage value" }
                .Step("fee", c =>
                {
                    c.Assert(RealStorageEvent.FeeRate() >= 0, $"fee rate {RealStorageEvent.FeeRate():0.0000}");
                    var value = RealStorageEvent.GetStorageValue();
                    c.Assert(value >= RealStorageEvent.GetStorageSpiritStones(), $"storage value {value} >= cash {RealStorageEvent.GetStorageSpiritStones()}");
                    c.Assert(RealStorageEvent.Instance.Debt >= 0, $"debt {RealStorageEvent.Instance.Debt} >= 0");
                });
        }
    }
}
