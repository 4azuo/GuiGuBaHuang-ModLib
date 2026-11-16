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
    /// TG-06 (NpcAutoEvent): NPC without full exp gains exp monthly.
    /// </summary>
    public class TG06_NpcAutoEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-06";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-06", Feature = "NpcAutoEvent", Title = "NPC without full exp gains exp monthly" }
                .Step("exp", c =>
                {
                    var npc = Npcs.FirstOrDefault(x => !x.IsFullExp() && x.GetDynProperty(UnitDynPropertyEnum.Talent).value > 0);
                    if (npc == null) { c.Skip("no NPC without full exp"); return; }
                    var before = npc.GetExp();
                    NpcAutoEvent.Instance.OnMonthlyForEachWUnit(npc);
                    c.Assert(npc.GetExp() > before, $"{npc.GetName()}: exp {before} -> {npc.GetExp()}");
                });
        }
    }
}
