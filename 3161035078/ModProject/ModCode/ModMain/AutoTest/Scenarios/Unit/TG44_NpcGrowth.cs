using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-44 (NpcUpgradeSkillEvent/NpcAutoCraftArtifact/NpcAutoEvent): 24 monthly NPC growth ticks raise the
    /// martial mastery exp and the cultivation exp of an NPC (NPC info before/after).
    /// </summary>
    public class TG44_NpcGrowth : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-44";

        private static long MartialExp(WorldUnitBase u) =>
            new[] { MartialType.SkillLeft, MartialType.SkillRight, MartialType.Step, MartialType.Ultimate, MartialType.Ability }
                .Sum(t => u.data.unitData.GetActionMartial(t).ToArray().Sum(x => (long)x.exp));

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase npc = null;
            return new Scenario { Id = "TG-44", Feature = "NpcUpgradeSkillEvent/NpcAutoCraftArtifact/NpcAutoEvent", Title = "NPC growth over 24 months", Overlay = false }
                .Step("npc before", c =>
                {
                    npc = Npcs.Where(x => !x.IsFullExp()).OrderByDescending(x => x.GetDynProperty(ModLib.Enum.UnitDynPropertyEnum.Talent).value).FirstOrDefault();
                    if (!c.Assert(npc != null, "an NPC without full exp")) return;
                    c.Bag["mexp"] = MartialExp(npc);
                    c.Bag["exp"] = npc.GetExp();
                    c.Bag["items"] = npc.GetUnitProps().Count;
                    c.Info($"{npc.GetName()}: grade {npc.GetGradeLvl()}, exp {npc.GetExp()}, martial exp {c.Bag["mexp"]}, items {c.Bag["items"]}");
                    g.ui.OpenUISafe<UINPCInfo>(UIType.NPCInfo).InitData(npc, false);
                })
                .Step("24 months", c =>
                {
                    if (npc == null) return;
                    if (g.ui.HasUI(UIType.NPCInfo)) g.ui.CloseUI(UIType.NPCInfo);
                    for (var i = 0; i < 24; i++)
                    {
                        NpcAutoEvent.Instance.OnMonthlyForEachWUnit(npc);
                        NpcUpgradeSkillEvent.Instance.OnMonthlyForEachWUnit(npc);
                        NpcAutoCraftArtifact.Instance.OnMonthlyForEachWUnit(npc);
                    }
                    c.Assert(MartialExp(npc) > (long)c.Bag["mexp"], $"martial exp {c.Bag["mexp"]} -> {MartialExp(npc)}");
                    c.Assert(npc.GetExp() > (int)c.Bag["exp"] || npc.GetGradeLvl() > 0, $"exp {c.Bag["exp"]} -> {npc.GetExp()} (grade {npc.GetGradeLvl()})");
                    c.Info($"items {c.Bag["items"]} -> {npc.GetUnitProps().Count} (artifact crafting is random)");
                    g.ui.OpenUISafe<UINPCInfo>(UIType.NPCInfo).InitData(npc, false);
                })
                .Step("close", c => { if (g.ui.HasUI(UIType.NPCInfo)) g.ui.CloseUI(UIType.NPCInfo); }, 1500);
        }
    }
}
