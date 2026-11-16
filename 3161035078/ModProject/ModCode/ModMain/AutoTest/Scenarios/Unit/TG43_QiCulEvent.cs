using ModLib.Helper;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>TG-43 (QiCulEvent): NPC info shows Qi and its bonuses; "transfer Qi" moves Qi from the player to the NPC.</summary>
    public class TG43_QiCulEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-43";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase npc = null;
            return new Scenario { Id = "TG-43", Feature = "QiCulEvent", Title = "Qi in NPC info and Qi transfer", Overlay = false }
                .Step("npc info", c =>
                {
                    if (!IsLoaded(ModConst.QI_CUL_EVENT)) { c.Skip("Qi cultivation disabled"); return; }
                    npc = Npcs.FirstOrDefault(x => !HirePeopleEvent.IsHired(x));
                    var qi = QiCulEvent.Instance.Qi;
                    qi[Player.GetUnitId()] = 50000;
                    qi[npc.GetUnitId()] = 1000;
                    c.Info($"test data: player Qi 50000, {npc.GetName()} Qi 1000");
                    g.ui.OpenUISafe<UINPCInfo>(UIType.NPCInfo).InitData(npc, false);
                })
                .Step("qi texts", c =>
                {
                    if (npc == null) { c.Skip("no NPC"); return; }
                    var ui = g.ui.GetUI<UINPCInfo>(UIType.NPCInfo);
                    // the Qi texts are on the "Taoist Mind" tab (uiHeart group), hidden while the Stats tab is shown
                    var texts = ui?.uiHeart?.goGroupRoot?.GetComponentsInChildren<UnityEngine.UI.Text>(true).Select(t => t.text ?? "").ToList() ?? new System.Collections.Generic.List<string>();
                    c.Assert(texts.Any(t => t.Contains("Qi: 1,000")), $"Qi text (Taoist Mind tab): {texts.FirstOrDefault(t => t.Contains("Qi:"))}");
                    c.Assert(texts.Any(t => t.StartsWith("Atk:")) && texts.Any(t => t.StartsWith("Hp:")) && texts.Any(t => t.StartsWith("Mp:")), "Qi Atk/Hp/Mp bonus texts");
                }, 1500)
                .Step("transfer", c =>
                {
                    if (npc == null) { c.Skip("no NPC"); return; }
                    var btn = FindButtons(UIType.NPCInfo, GameTool.LS("other500020071")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'transfer Qi' button")) return;
                    btn.Item.onClick.Invoke();
                    var qi = QiCulEvent.Instance.Qi;
                    c.Assert(qi[Player.GetUnitId()] < 50000, $"player Qi 50000 -> {qi[Player.GetUnitId()]}");
                    c.Assert(qi[npc.GetUnitId()] > 1000, $"{npc.GetName()} Qi 1000 -> {qi[npc.GetUnitId()]}");
                }, 500)
                .Step("message", c => c.Info("transfer drama and message shown"), 1500);
        }
    }
}
