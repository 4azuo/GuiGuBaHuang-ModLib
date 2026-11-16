using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-58 (MapBuildPropertyEvent/MapBuildBattleEvent): town master tools. Tax statistics count payers, income and
    /// residents who left, and show the previous month; the management window shows them. When the player's town is
    /// attacked far away, a drama calls the player back (rush back / trust the guardians); trusting simulates the war.
    /// </summary>
    public class TG58_TownMasterTools : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-58";

        private static bool DramaOpen => g.ui.HasUI(UIType.DramaDialogue) || g.ui.HasUI(UIType.DramaDialogueBigTexture) || g.ui.HasUI(UIType.DramaBigTexture) || g.ui.HasUI(UIType.DramaFortuitous);

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildTown townA = null, townB = null;
            return new Scenario { Id = "TG-58", Feature = "MapBuildPropertyEvent/MapBuildBattleEvent", Title = "Town master: tax statistics, called back to defend the town", Overlay = false }
                .Step("tax statistics", c =>
                {
                    townA = FindTown(t => MapBuildPropertyEvent.GetTownMaster(t) != null);
                    if (!c.Assert(townA != null, "a town")) return;
                    var id = townA.buildData.id;
                    var cur = GameHelper.GetGameTotalMonth();
                    MapBuildPropertyEvent.Instance.TaxStats.Remove(id);
                    MapBuildPropertyEvent.RecordTax(townA, 100, true);
                    MapBuildPropertyEvent.RecordTax(townA, 50, true);
                    MapBuildPropertyEvent.RecordTax(townA, 0, false);
                    var s = MapBuildPropertyEvent.Instance.TaxStats[id];
                    c.Assert(s.Income == 150 && s.Payers == 2 && s.Leavers == 1, $"this month: {s.Income} from {s.Payers}, {s.Leavers} left");
                    // next month: the first record moves this month to "last month"
                    s.Month = cur - 1;
                    MapBuildPropertyEvent.RecordTax(townA, 10, true);
                    c.Assert(s.LastIncome == 150 && s.LastPayers == 2 && s.LastLeavers == 1 && s.Income == 10, "next month: the previous month is kept");
                    var text = MapBuildPropertyEvent.TaxStatText(townA);
                    c.Assert(text.Contains("150"), $"text: {text}");
                })
                .Step("management window", c =>
                {
                    if (townA == null) return;
                    MapBuildPropertyEvent.GetBaseTax(townA); // tax rate of the town set
                    MapBuildPropertyEvent.OpenUITownManage(townA);
                }, 1500)
                .Step("statistics shown", c =>
                {
                    var ui = g.ui.GetUI<UIBase>(UIType.NPCSearch);
                    c.Assert(ui != null && FindTexts(ui, "150").Any(), "tax statistics in the town management window");
                    CloseAll(UIType.NPCSearch);
                })
                .Step("call to defend", c =>
                {
                    townB = FindTown(t => townA != null && t.buildData.id != townA.buildData.id);
                    if (!c.Assert(townA != null && townB != null, "two towns")) return;
                    MapBuildBattleEvent.CallToDefend(townA, townB);
                }, 1500)
                .Step("trust the guardians", c =>
                {
                    if (townB == null) return;
                    c.Assert(DramaOpen, "call-to-defend drama");
                    c.Assert(DramaText().Contains(townA.name) && ChooseOption(GameTool.LS("town_call_o2")), $"'{GameTool.LS("town_call_o2")}' chosen ({DramaText()})");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_town_call"), "logged");
                }, 1500)
                .Step("no battle", c =>
                {
                    DismissDramas();
                    c.Assert(!GameHelper.IsInBattlle() && OnMap, "the war was simulated, the player stays on the map");
                }, 1000);
        }
    }
}
