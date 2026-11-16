using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-32 (MissionDeclareEvent/HirePeopleEvent): the bounty board shows "Commission" and "Hire People";
    /// both open their selector.
    /// </summary>
    public class TG32_TownBounty : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-32";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-32", Feature = "MissionDeclareEvent/HirePeopleEvent", Title = "Bounty board: commission and hire people", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownBounty))))
                .Step("open bounty board", c =>
                {
                    var town = CurrentTown;
                    if (town == null) { c.Skip("player is not in a town"); return; }
                    OpenTownUI<UITownBounty>(UIType.TownBounty, ui => ui.InitData(town));
                })
                .Step("buttons", c =>
                {
                    if (!g.ui.HasUI(UIType.TownBounty)) { c.Skip("bounty board not opened"); return; }
                    c.Assert(FindButtons(UIType.TownBounty, GameTool.LS("other500020003")).Any(), "'Commission' button");
                    c.Assert(FindButtons(UIType.TownBounty, GameTool.LS("team420041131")).Any(), "'Hire People' button");
                })
                .Step("hire people", c =>
                {
                    var btn = FindButtons(UIType.TownBounty, GameTool.LS("team420041131")).FirstOrDefault();
                    if (btn == null) { c.Skip("no button"); return; }
                    btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.NPCSearch) && HirePeopleEvent.isShowHirePeopleUI, "hire list (NPC search) opened");
                    c.Info($"hirable people: {HirePeopleEvent.GetHirablePeople().Count}");
                })
                .Step("close hire list", c =>
                {
                    CloseAll(UIType.NPCSearch);
                    c.Assert(!HirePeopleEvent.isShowHirePeopleUI, "hire flag reset when the list closes");
                })
                .Step("commission", c =>
                {
                    var btn = FindButtons(UIType.TownBounty, GameTool.LS("other500020003")).FirstOrDefault();
                    if (btn == null) { c.Skip("no button"); return; }
                    if (!btn.IsActive()) { c.Skip("already commissioned this month"); return; }
                    btn.Item.onClick.Invoke();
                    c.Assert(g.ui.HasUI(UIType.PropSelect), "commission item selector opened");
                })
                .Step("close", c =>
                {
                    CloseAll(UIType.PropSelect, UIType.TownBounty);
                    c.Assert(!g.ui.HasUI(UIType.TownBounty), "bounty board closed");
                });
        }
    }
}
