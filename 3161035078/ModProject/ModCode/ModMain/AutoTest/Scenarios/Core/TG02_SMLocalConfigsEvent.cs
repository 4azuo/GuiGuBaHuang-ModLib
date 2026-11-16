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

namespace MOD_nE7UL2.AutoTest.Scenarios.Core
{
    /// <summary>
    /// TG-02 (SMLocalConfigsEvent): Per-save S&amp;M configs exist and Calculate applies rates.
    /// </summary>
    public class TG02_SMLocalConfigsEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-02";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-02", Feature = "SMLocalConfigsEvent", Title = "Per-save S&M configs exist and Calculate applies rates" }
                .Step("calc", c =>
                {
                    var s = SMLocalConfigsEvent.Instance;
                    if (!c.Assert(s?.Configs != null, "local S&M configs exist")) return;
                    c.AssertEqual(150d, s.Calculate(100, 0.5), "Calculate(100, +50%)");
                    c.AssertEqual(100d, s.Calculate(100, 0), "Calculate(100, 0%)");
                    c.AssertEqual(50d, s.Calculate(100, -0.5), "Calculate(100, -50%)");
                });
        }
    }
}
