using MOD_nE7UL2.Mod;
using ModLib.Helper;
using ModLib.Object;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.UI
{
    /// <summary>
    /// TG-27 (UISettingScreenEvent): game settings show the Taoist settings button, which opens the Taoist settings
    /// window (hide damage multiplier, hide exception logs).
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
                    var w = UIHelper.GetUICustomBase(UIType.TextInfo).LastOrDefault();
                    c.Assert(g.ui.HasUI(UIType.TextInfo) && w != null, "Taoist settings window opened");
                    c.Assert((w?.Items.OfType<UIItemToggle>().Count() ?? 0) >= 2, $"2 toggles (hide damage multiplier, hide exception logs): {w?.Items.OfType<UIItemToggle>().Count()}");
                    c.Info($"HideDamageMultiplier={UISettingScreenEvent.Instance.HideDamageMultiplier}, HideExceptionLogs={UISettingScreenEvent.Instance.HideExceptionLogs}");
                })
                .Step("close", c =>
                {
                    if (g.ui.HasUI(UIType.TextInfo)) g.ui.CloseUI(UIType.TextInfo);
                    if (g.ui.HasUI(UIType.GameSetting)) g.ui.CloseUI(UIType.GameSetting);
                    c.Assert(!g.ui.HasUI(UIType.GameSetting), "settings closed");
                });
        }
    }
}
