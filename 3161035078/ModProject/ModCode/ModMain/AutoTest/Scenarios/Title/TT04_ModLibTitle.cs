using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Title
{
    /// <summary>
    /// TT-04 (ModTitleEvent, ModLib): title screen language selector and ModCreator tools window with the
    /// duplicated config id list.
    /// </summary>
    public class TT04_ModLibTitle : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TT-04";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TT-04", Feature = "ModTitleEvent (ModLib)", Title = "Language selector and ModCreator tools window", Scope = ScenarioScope.Title, Overlay = false }
                .Step("language selector", c =>
                {
                    var selects = UIHelper.GetUICustomBase(UIType.Login).SelectMany(x => x.Items).OfType<UIItemSelect>().ToList();
                    c.Assert(selects.Count >= 1, "language selector on the title screen");
                    c.Info($"translation index {ModTitleEvent.Instance.TranslateIndex} ({ModTitleEvent.GetTranslateLanguage() ?? "default"})");
                    c.Assert(ConfHelper.LoadedIDs.Count > 0, $"config ids loaded from mods: {ConfHelper.LoadedIDs.Count}");
                })
                .Step("open tools", c =>
                {
                    var btn = FindButtons(UIType.Login, GameTool.LS("libtxt999990009")).FirstOrDefault();
                    if (!c.Assert(btn != null, "ModCreator tools button")) return;
                    btn.Item.onClick.Invoke();
                })
                .Step("tools window", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.TextInfo), "tools window opened");
                    var btn = FindButtons(UIType.TextInfo, GameTool.LS("libtxt999990010")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'check duplicated IDs' button")) return;
                    btn.Item.onClick.Invoke();
                })
                .Step("duplicated ids", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "duplicated id list opened");
                    var dup = ConfHelper.LoadedIDs.Count(x => x.Value.Split(';').Count(y => y.Length > 0) > 1);
                    c.Info($"ids defined by several mods: {dup}, replaced base ids: {ConfHelper.ReplacedBaseIDs.Count}");
                })
                .Step("close", c =>
                {
                    if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
                    if (g.ui.HasUI(UIType.TextInfo)) g.ui.CloseUI(UIType.TextInfo);
                    c.Assert(!g.ui.HasUI(UIType.TextInfoLong), "windows closed");
                });
        }
    }
}
