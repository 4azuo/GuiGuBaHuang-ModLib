using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-35 (RebirthEvent/CustomRefineEvent): the pub offers rebirth when the town level allows it (not clicked),
    /// the forge of a small town opens the custom refine item selector.
    /// </summary>
    public class TG35_RebirthRefine : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-35";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-35", Feature = "RebirthEvent/CustomRefineEvent", Title = "Pub rebirth button and forge custom refine", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownPub) && Has(t, MapBuildSubType.TownFactory) && !t.buildTownData.isMainTown)))
                .Step("open pub", c =>
                {
                    var town = CurrentTown;
                    if (town == null) { c.Skip("player is not in a town"); return; }
                    OpenTownUI<UITownPub>(UIType.TownPub, ui => ui.InitData(town));
                })
                .Step("rebirth button", c =>
                {
                    var town = CurrentTown;
                    if (town == null || !g.ui.HasUI(UIType.TownPub)) { c.Skip("pub not opened"); return; }
                    var r = RebirthEvent.Instance;
                    var grade = Player.GetGradeLvl();
                    var expected = !SMLocalConfigsEvent.Instance.Configs.NoRebirth && town.gridData.areaBaseID > r.RebirthLevel && grade > r.RebirthLevel && town.gridData.areaBaseID >= grade;
                    var shown = FindButtons(UIType.TownPub, GameTool.LS($"rebirth{420101001 + r.RebirthCount}")).Any();
                    c.AssertEqual(expected, shown, $"rebirth button shown (town level {town.gridData.areaBaseID}, grade {grade}, rebirth level {r.RebirthLevel})");
                })
                .Step("open forge", c =>
                {
                    CloseAll(UIType.TownPub);
                    var town = CurrentTown;
                    if (town == null) { c.Skip("player is not in a town"); return; }
                    OpenTownUI<UITownFactoryShapeOption>(UIType.TownFactoryShapeOption, ui => ui.InitData(town));
                })
                .Step("custom refine", c =>
                {
                    var town = CurrentTown;
                    if (town == null || !g.ui.HasUI(UIType.TownFactoryShapeOption)) { c.Skip("forge not opened"); return; }
                    if (town.buildTownData.isMainTown) { c.Skip($"{town.name} is a main town: vanilla upgrade kept"); return; }
                    var ui = g.ui.GetUI<UITownFactoryShapeOption>(UIType.TownFactoryShapeOption);
                    // CompHelper.Replace hides the vanilla button and adds a clone named "CompHelper:<n>" with the same label
                    var label = ui.btnUpgrade.GetComponentInChildren<UnityEngine.UI.Text>(true)?.text;
                    var custom = ui.GetComponentsInChildren<UnityEngine.UI.Button>()
                        .FirstOrDefault(b => b.gameObject.activeInHierarchy && b.name.StartsWith("CompHelper:") && b.GetComponentInChildren<UnityEngine.UI.Text>()?.text == label);
                    c.Assert(!ui.btnUpgrade.gameObject.activeSelf, "vanilla upgrade button hidden");
                    if (!c.Assert(custom != null, $"custom refine button '{label}' replaces it")) return;
                    custom.onClick.Invoke();
                    var sel = g.ui.GetUI<UIPropSelect>(UIType.PropSelect);
                    c.Assert(sel != null, "custom refine item selector opened");
                    c.AssertEqual(GameTool.LS("other500020001"), sel?.textTitle1?.text, "selector title");
                    c.Info($"refinable items listed: {sel?.allItems?.allProps?.Count}");
                })
                .Step("close", c =>
                {
                    CloseAll(UIType.PropSelect, UIType.TownFactoryShapeOption, UIType.TownPub);
                    c.Assert(!g.ui.HasUI(UIType.TownFactoryShapeOption), "forge closed");
                });
        }
    }
}
