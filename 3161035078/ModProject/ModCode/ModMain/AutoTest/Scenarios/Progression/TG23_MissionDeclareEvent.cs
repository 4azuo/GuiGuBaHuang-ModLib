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

namespace MOD_nE7UL2.AutoTest.Scenarios.Progression
{
    /// <summary>
    /// TG-23 (MissionDeclareEvent): Running commissions are consistent.
    /// </summary>
    public class TG23_MissionDeclareEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-23";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-23", Feature = "MissionDeclareEvent", Title = "Running commissions are consistent" }
                .Step("commissions", c =>
                {
                    var tasks = MissionDeclareEvent.Instance.CommissionTasks;
                    c.Info($"commissions: {tasks.Count}");
                    c.Assert(tasks.All(x => x.PassTime <= x.CostTime + 1), "no commission past its time");
                    c.Assert(tasks.All(x => x.SuccessRate >= 0 && x.SuccessRate <= 100), "success rates in 0..100");
                });
        }
    }
}
