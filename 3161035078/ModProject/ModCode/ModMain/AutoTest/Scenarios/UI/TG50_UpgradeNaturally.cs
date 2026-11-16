using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.UI
{
    /// <summary>
    /// TG-50 (UpgradeNaturallyEvent): with full exp the cultivation screen shows "Break through"; clicking it moves
    /// the player to the next phase and clears the exp. Grade and exp are restored afterwards; when
    /// AllowUpgradeNaturally is off the feature is tested with a temporary instance. Replaces the manual TM-18.
    /// </summary>
    public class TG50_UpgradeNaturally : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-50";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            UpgradeNaturallyEvent ev = null;
            return new Scenario { Id = "TG-50", Feature = "UpgradeNaturallyEvent", Title = "Break through without items", Overlay = false }
                .Step("full exp", c =>
                {
                    c.Info(IsLoaded(ModConst.UPGRADE_NATURALLY_EVENT) ? "natural upgrade enabled" : "AllowUpgradeNaturally off: testing with a temporary instance");
                    ev = FeatureEvent<UpgradeNaturallyEvent>(ModConst.UPGRADE_NATURALLY_EVENT);
                    c.Bag["grade"] = Player.GetProperty<int>(UnitPropertyEnum.GradeID);
                    c.Bag["exp"] = Player.GetExp();
                    for (var i = 0; i < 50 && !Player.IsFullExp(); i++) Player.AddExp(100000);
                    c.Assert(Player.IsFullExp(), "player exp is full");
                    var next = Player.GetNextPhaseConf();
                    if (!c.Assert(next != null, "a next phase exists")) return;
                    c.Bag["next"] = next.id;
                })
                .Step("open cultivation", c =>
                {
                    var ui = g.ui.GetUI<UIMapMain>(UIType.MapMain);
                    (ui.playerInfo.btnFateFeature ?? ui.playerInfo.btnFateFeature_En).onClick.Invoke();
                    RaiseOpenUIEnd(ev, ModConst.UPGRADE_NATURALLY_EVENT, g.ui.GetUI<UIFateFeature>(UIType.FateFeature), UIType.FateFeature);
                })
                .Step("break through", c =>
                {
                    if (!c.Assert(g.ui.HasUI(UIType.FateFeature), "cultivation screen opened")) return;
                    var btn = FindButtons(UIType.FateFeature, GameTool.LS("other500020006")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'Break through' button")) return;
                    btn.Item.onClick.Invoke();
                    c.AssertEqual((int)c.Bag["next"], Player.GetProperty<int>(UnitPropertyEnum.GradeID), "player moved to the next phase");
                    c.AssertEqual(Player.GetMinExpCurrentPhase(), Player.GetExp(), "exp cleared (minimum exp of the new phase)");
                }, 1000)
                .Step("restore", c =>
                {
                    CloseAll(UIType.FateFeature);
                    Player.SetProperty<int>(UnitPropertyEnum.GradeID, (int)c.Bag["grade"]);
                    Player.ClearExp();
                    Player.AddExp((int)c.Bag["exp"]);
                    c.AssertEqual((int)c.Bag["grade"], Player.GetProperty<int>(UnitPropertyEnum.GradeID), "grade restored");
                });
        }
    }
}
