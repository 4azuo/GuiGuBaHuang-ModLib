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
    /// TG-11 (BankAccountEvent): Bank account fee is positive for every area.
    /// </summary>
    public class TG11_BankAccountEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-11";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-11", Feature = "BankAccountEvent", Title = "Bank account fee is positive for every area" }
                .Step("fees", c =>
                {
                    foreach (var area in BankAccountEvent.BankAccountConfigs.OpenFee.Keys.OrderBy(x => x))
                        c.Assert(BankAccountEvent.Cost(area) > 0, $"area {area}: fee {BankAccountEvent.Cost(area)}");
                    c.Info($"registered towns: {BankAccountEvent.Instance.RegisterdTown.Count}");
                });
        }
    }
}
