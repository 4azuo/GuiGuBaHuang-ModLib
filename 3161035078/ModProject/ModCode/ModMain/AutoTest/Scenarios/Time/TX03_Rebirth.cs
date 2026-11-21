using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TX-03 (RebirthEvent): in the pub of a town of a high enough level, the rebirth button asks for confirmation,
    /// then the grade is reset, the rebirth luck is added and the drama is shown. Changes the save (restored by
    /// run-autotest.ps1).
    /// </summary>
    public class TX03_Rebirth : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TX-03";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var ready = false;
            return new Scenario { Id = "TX-03", Feature = "RebirthEvent", Title = "Rebirth in the pub", Overlay = false }
                .Step("travel", c =>
                {
                    var r = RebirthEvent.Instance;
                    var grade = Player.GetGradeLvl();
                    if (SMLocalConfigsEvent.Instance.Configs.NoRebirth) { c.Skip("rebirth disabled (NoRebirth)"); return; }
                    if (grade <= r.RebirthLevel) { c.Skip($"grade {grade} <= rebirth level {r.RebirthLevel}"); return; }
                    if (!string.IsNullOrEmpty(Player.data.unitData.schoolID)) { c.Skip("player is in a sect (rebirth refused)"); return; }
                    ready = TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownPub) && t.gridData.areaBaseID >= grade && t.gridData.areaBaseID > r.RebirthLevel));
                })
                // the rebirth button is on the player info, beside the damage calculation (until 6.0 at the pub)
                .Step("player info", c => { if (ready) g.ui.OpenUISafe<UIPlayerInfo>(UIType.PlayerInfo); }, 1500)
                .Step("rebirth button", c =>
                {
                    if (!ready) return;
                    var r = RebirthEvent.Instance;
                    c.Bag["count"] = r.RebirthCount;
                    c.Bag["grade"] = Player.GetGradeLvl();
                    c.AssertEqual(null, RebirthEvent.Missing(Player), "nothing missing for the rebirth");
                    var btn = FindButtons(UIType.PlayerInfo, RebirthEvent.ButtonText).FirstOrDefault();
                    if (!c.Assert(btn != null, "rebirth button on the player info")) { ready = false; return; }
                    btn.Item.onClick.Invoke();
                }, 1000)
                .Step("confirm", c => { if (ready) c.Info($"clicked '{ClickButton("Yes", "是", "OK")}' in the confirmation"); }, 1500)
                .Step("reborn", c =>
                {
                    if (!ready) return;
                    var r = RebirthEvent.Instance;
                    c.AssertEqual((int)c.Bag["count"] + 1, r.RebirthCount, "rebirth counted");
                    c.AssertEqual((int)c.Bag["grade"], r.RebirthLevel, "rebirth level = grade before");
                    c.AssertEqual(1, Player.GetGradeLvl(), "grade reset");
                    c.Assert(Player.GetLuck(RebirthEvent.LUCK_BASE_ID + r.RebirthCount) != null || r.RebirthCount > RebirthEvent.MAX_LUCK_LVL, "rebirth luck added");
                }, 1500)
                .Step("close", c => CloseAll(UIType.DramaDialogue, UIType.PlayerInfo, UIType.Town), 500);
        }
    }
}
