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
    /// TG-09 (HirePeopleEvent): Hire and dismiss a team member.
    /// </summary>
    public class TG09_HirePeopleEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-09";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-09", Feature = "HirePeopleEvent", Title = "Hire and dismiss a team member" }
                .Step("hire", c =>
                {
                    var npc = Npcs.FirstOrDefault(x => !HirePeopleEvent.IsHired(x) && !HirePeopleEvent.IsTeamMaster(x));
                    if (npc == null) { c.Skip("no free NPC"); return; }
                    c.Assert(HirePeopleEvent.GetRequiredSpiritStones(Player, npc) > 0, $"price of {npc.GetName()}: {HirePeopleEvent.GetRequiredSpiritStones(Player, npc)}");
                    c.Assert(HirePeopleEvent.GetRequiredReputations(Player, npc) > 0, $"reputation: {HirePeopleEvent.GetRequiredReputations(Player, npc)}");
                    HirePeopleEvent.Hire(Player, npc);
                    c.Assert(HirePeopleEvent.IsTeam(Player, npc), "member is in the player team");
                    c.Assert(HirePeopleEvent.GetTotalMonthlyPayment(Player) > 0, $"monthly payment {HirePeopleEvent.GetTotalMonthlyPayment(Player)}");
                    HirePeopleEvent.Dismiss(Player, npc);
                    c.Assert(!HirePeopleEvent.IsTeam(Player, npc), "member dismissed");
                });
        }
    }
}
