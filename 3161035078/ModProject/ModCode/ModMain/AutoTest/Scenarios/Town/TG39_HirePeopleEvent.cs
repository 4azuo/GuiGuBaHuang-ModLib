using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-39 (HirePeopleEvent): NPC info of a free NPC, hire it, team window with the monthly payment, the monthly
    /// handler pays the member, then dismiss.
    /// </summary>
    public class TG39_HirePeopleEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-39";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase npc = null;
            return new Scenario { Id = "TG-39", Feature = "HirePeopleEvent", Title = "Team: NPC info, hire, monthly payment, dismiss", Overlay = false }
                .Step("npc info", c =>
                {
                    npc = Npcs.FirstOrDefault(x => !HirePeopleEvent.IsHired(x) && !HirePeopleEvent.IsTeamMaster(x) && !HirePeopleEvent.IsFriend(Player, x));
                    if (!c.Assert(npc != null, "a free NPC (not a friend: friends work for free)")) return;
                    c.Info($"{npc.GetName()}: grade {npc.GetGradeLvl()}, price {HirePeopleEvent.GetRequiredSpiritStones(Player, npc)}, reputation {HirePeopleEvent.GetRequiredReputations(Player, npc)}");
                    g.ui.OpenUISafe<UINPCInfo>(UIType.NPCInfo).InitData(npc, false);
                })
                .Step("hire", c =>
                {
                    CloseAll(UIType.NPCInfo);
                    HirePeopleEvent.Hire(Player, npc);
                    c.Assert(HirePeopleEvent.IsTeam(Player, npc), $"{npc.GetName()} in the team");
                    c.Assert(npc.GetLuck(HirePeopleEvent.TEAM_LUCK_ID) != null, "team luck on the member");
                    HirePeopleEvent.OpenUIManageTeam();
                })
                .Step("monthly payment", c =>
                {
                    CloseAll(UIType.NPCSearch);
                    var pay = HirePeopleEvent.GetTotalMonthlyPayment(Player);
                    if (Player.GetUnitMoney() < pay) Player.AddUnitMoney(pay * 2);
                    var before = Player.GetUnitMoney();
                    HirePeopleEvent.Instance.OnMonthlyForEachWUnit(Player);
                    c.Assert(pay > 0, $"monthly payment {pay}");
                    c.AssertEqual(before - pay, Player.GetUnitMoney(), "payment taken from the master");
                    c.AssertEqual(Player.GetUnitPos(), npc.GetUnitPos(), "member follows the master");
                })
                .Step("dismiss", c =>
                {
                    HirePeopleEvent.Dismiss(Player, npc);
                    c.Assert(!HirePeopleEvent.IsTeam(Player, npc), "member dismissed");
                    c.Assert(npc.GetLuck(HirePeopleEvent.TEAM_LUCK_ID) == null, "team luck removed");
                });
        }
    }
}
