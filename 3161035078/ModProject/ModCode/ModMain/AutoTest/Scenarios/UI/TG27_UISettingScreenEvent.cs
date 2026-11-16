using MOD_nE7UL2.Mod;
using ModLib.Helper;
using ModLib.Object;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.UI
{
    /// <summary>
    /// TG-27 (UISettingScreenEvent): game settings show the Taoist settings button, which opens the Taoist settings
    /// window (large: hide damage multiplier, hide exception logs, market notifications, encounters, the Taoist log).
    /// </summary>
    public class TG27_UISettingScreenEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-27";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-27", Feature = "UISettingScreenEvent", Title = "Game settings: Taoist settings button and window" }
                .Step("open settings", c => g.ui.OpenUISafe<UIGameSetting>(UIType.GameSetting))
                .Step("settings button", c =>
                {
                    if (!c.Assert(g.ui.HasUI(UIType.GameSetting), "GameSetting opened")) return;
                    var btn = FindButtons(UIType.GameSetting, GameTool.LS("setting000")).FirstOrDefault();
                    if (!c.Assert(btn != null, "Taoist settings button on GameSetting")) return;
                    btn.Item.onClick.Invoke();
                })
                .Step("taoist settings window", c =>
                {
                    var w = UIHelper.GetUICustomBase(UIType.TextInfoLong).LastOrDefault();
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong) && w != null, "Taoist settings window opened (large window)");
                    c.Assert((w?.Items.OfType<UIItemToggle>().Count() ?? 0) >= 4, $"4 toggles (damage multiplier, exceptions, market notifications, encounters): {w?.Items.OfType<UIItemToggle>().Count()}");
                    c.Assert(FindButtons(UIType.TextInfoLong, GameTool.LS("setting013")).Any(), "Taoist log button");
                    c.Assert(w != null && w.Items.OfType<UIItemText>().Any(x => (x.GetFormat() ?? string.Empty) == GameTool.LS("setting006")), "hint: the artifact hotkeys are in the Bindings tab");
                    c.Info($"HideDamageMultiplier={UISettingScreenEvent.Instance.HideDamageMultiplier}, HideExceptionLogs={UISettingScreenEvent.Instance.HideExceptionLogs}");
                    TaoistLogEvent.Log("log_empty");
                    FindButtons(UIType.TextInfoLong, GameTool.LS("setting013")).Last().Item.onClick.Invoke();
                }, 1000)
                .Step("taoist log", c =>
                {
                    var ui = g.ui.GetUI<UIBase>(UIType.TextInfoLong);
                    c.Assert(ui != null && FindTexts(ui, " / ").Any(), "Taoist log window with pages");
                    c.Assert(FindTexts(ui, GameTool.LS("log_empty")).Any(), "the newest entry is shown");
                })
                .Step("close", c =>
                {
                    if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
                    if (g.ui.HasUI(UIType.GameSetting)) g.ui.CloseUI(UIType.GameSetting);
                    c.Assert(!g.ui.HasUI(UIType.GameSetting), "settings closed");
                });
        }
    }
}
