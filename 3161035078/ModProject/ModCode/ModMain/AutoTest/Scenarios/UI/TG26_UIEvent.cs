using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.UI
{
    /// <summary>
    /// TG-26 (UIEvent): Game menu save/reload buttons follow the S&amp;M configs.
    /// </summary>
    public class TG26_UIEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-26";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-26", Feature = "UIEvent", Title = "Game menu save/reload buttons follow the S&M configs" }
                .Step("open", c => g.ui.OpenUISafe<UIGameMemu>(UIType.GameMemu))
                .Step("check", c =>
                {
                    var ui = g.ui.GetUI<UIGameMemu>(UIType.GameMemu);
                    if (!c.Assert(ui != null, "GameMemu opened")) return;
                    var cfg = SMLocalConfigsEvent.Instance.Configs;
                    c.AssertEqual(!cfg.HideSaveButton, ui.btnSave.gameObject.activeSelf, "save button visibility");
                    c.AssertEqual(!cfg.HideReloadButton, ui.btnReloadCache.gameObject.activeSelf, "reload button visibility");
                })
                .Step("close", c => { if (g.ui.HasUI(UIType.GameMemu)) g.ui.CloseUI(UIType.GameMemu); });
        }
    }
}
