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

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-07 (ExpertEvent): Expertise exp formulas.
    /// </summary>
    public class TG07_ExpertEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-07";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-07", Feature = "ExpertEvent", Title = "Expertise exp formulas" }
                .Step("formula", c =>
                {
                    c.AssertEqual(0, ExpertEvent.GetExpertNeedExp(0, 1, 1), "level 0 needs 0");
                    c.AssertEqual(1000, ExpertEvent.GetExpertNeedExp(1, 1, 0), "level 1, grade 1, level 0");
                    c.AssertEqual(2000, ExpertEvent.GetExpertNeedExp(2, 1, 0), "level 2 doubles");
                    c.AssertEqual(4000, ExpertEvent.GetExpertNeedExp(1, 2, 5), "grade 2, item level 5: 1000x2x2");
                    const string id = "__autotest_expert__";
                    c.AssertEqual(0, ExpertEvent.GetExpertLvl(id, 1, 0), "unknown item has level 0");
                    ExpertEvent.AddExpertExp(Player, id, 1f);
                    c.Assert(ExpertEvent.GetExpertExp(id) > 0, $"exp added: {ExpertEvent.GetExpertExp(id)}");
                    ExpertEvent.GetExpertExps().Remove(id);
                });
        }
    }
}
