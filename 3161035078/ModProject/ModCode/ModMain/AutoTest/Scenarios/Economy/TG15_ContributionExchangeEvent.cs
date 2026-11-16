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
    /// TG-15 (ContributionExchangeEvent): Sect exchange ratio is positive.
    /// </summary>
    public class TG15_ContributionExchangeEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-15";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-15", Feature = "ContributionExchangeEvent", Title = "Sect exchange ratio is positive" }
                .Step("ratio", c =>
                {
                    var ratio = ContributionExchangeEvent.GetExchangeRatio();
                    c.Assert(ratio > 0, $"base ratio {ratio}");
                    var cur = ContributionExchangeEvent.Instance.CurMonthRatio;
                    if (cur == 0) c.Skip("CurMonthRatio not set yet (first month not passed)");
                    else c.AssertBetween(cur, Math.Floor(ratio * 0.8) - 1, Math.Ceiling(ratio * 1.3) + 1, "monthly ratio in 80%..130% of base");
                });
        }
    }
}
