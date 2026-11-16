using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Story
{
    /// <summary>
    /// TG-56 (drama texts): the reworked dramas. Every key of the battle dramas has its variants (key_1, key_2...) and
    /// a random pick returns one of them; an NPC-join drama opens with one of its texts; the Qi transfer drama goes on to
    /// its second line and its own option; the stalker drama offers "fight" and "pay them off (fee)".
    /// </summary>
    public class TG56_DramaTexts : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-56";

        private static List<string> Labels() => DramaOptions().Select(x => x.Item2).ToList();

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase npc = null;
            return new Scenario { Id = "TG-56", Feature = "BattleEvent/QiCulEvent (dramas)", Title = "Drama texts: variants, chains and options", Overlay = false }
                .Step("variants", c =>
                {
                    var expected = new Dictionary<string, int>
                    {
                        ["battleevent480110100"] = 4, ["battleevent480110200"] = 3, ["battleevent480110300"] = 3, ["battleevent480110400"] = 3,
                        ["battleevent480110500"] = 2, ["battleevent480110501"] = 2, ["battleevent480110502"] = 2,
                    };
                    foreach (var kv in expected)
                        c.AssertEqual(kv.Value, ModTextHelper.Variants(kv.Key).Count, $"{kv.Key} variants");
                    var texts = ModTextHelper.Variants("battleevent480110100").Select(x => GameTool.LS(x)).ToList();
                    var seen = new HashSet<string>();
                    for (var i = 0; i < 40; i++) seen.Add(ModTextHelper.RandomLS("battleevent480110100"));
                    c.Assert(seen.All(texts.Contains) && seen.Count >= 2, $"random picks among the variants ({seen.Count} different in 40)");
                })
                .Step("NPC join drama", c =>
                {
                    npc = Npcs.FirstOrDefault();
                    if (!c.Assert(npc != null, "an NPC")) return;
                    ModTextHelper.OpenDrama(BattleEvent.FRIENDLY_JOIN_DRAMA, "battleevent" + BattleEvent.FRIENDLY_JOIN_DRAMA, npc, Player);
                }, 1500)
                .Step("NPC join text", c =>
                {
                    var variants = ModTextHelper.Variants("battleevent480110200").Select(x => GameTool.LS(x)).ToList();
                    var text = DramaText();
                    c.Assert(variants.Contains(text), $"friend drama shows one of its texts: {text}");
                    DismissDramas();
                })
                .Step("Qi drama", c => DramaTool.OpenDrama(QiCulEvent.QI_TRANSFER_DRAMA), 1500)
                .Step("Qi first line", c =>
                {
                    c.AssertEqual(GameTool.LS("qicul480010100"), DramaText(), "Qi drama: first line");
                    c.Assert(Labels().Count == 1 && Labels()[0] == GameTool.LS("qicul480010104"), $"first line option: {string.Join(" | ", Labels())}");
                    c.Assert(ChooseOption(GameTool.LS("qicul480010104")), "option: next line");
                }, 1500)
                .Step("Qi second line", c =>
                {
                    c.AssertEqual(GameTool.LS("qicul480010102"), DramaText(), "Qi drama: second line");
                    c.Assert(Labels().Contains(GameTool.LS("qicul480010103")), $"Qi drama: its own option ({string.Join(" | ", Labels())})");
                    ChooseOption(GameTool.LS("qicul480010103"));
                    DismissDramas();
                }, 1000)
                .Step("stalker drama", c =>
                {
                    if (npc == null) return;
                    BattleEvent.StalkerDrama("battleevent480110601", npc, new List<UnitCtrlBase>());
                }, 1500)
                .Step("stalker options", c =>
                {
                    if (npc == null) return;
                    var pay = string.Format(GameTool.LS("battleevent480110611"), BattleEvent.StalkerFee(npc));
                    c.AssertEqual(GameTool.LS("battleevent480110601"), DramaText(), "stalker text");
                    var labels = Labels();
                    c.Assert(labels.Count == 2 && labels.Contains(GameTool.LS("battleevent480110610")) && labels.Contains(pay), $"options: {string.Join(" | ", labels)}");
                    c.Assert(ChooseOption(GameTool.LS("battleevent480110610")), "fight chosen");
                    DismissDramas();
                    c.Assert(DramaUI() == null, "drama closed");
                }, 1000);
        }
    }
}
