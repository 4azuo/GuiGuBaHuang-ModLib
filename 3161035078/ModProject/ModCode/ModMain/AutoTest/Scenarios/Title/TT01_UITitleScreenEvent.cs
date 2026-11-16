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

namespace MOD_nE7UL2.AutoTest.Scenarios.Title
{
    /// <summary>
    /// TT-01 (UITitleScreenEvent): Title screen shows Taoist/ModLib buttons and versions are compatible.
    /// </summary>
    public class TT01_UITitleScreenEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TT-01";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TT-01", Feature = "UITitleScreenEvent", Title = "Title screen shows Taoist/ModLib buttons and versions are compatible", Scope = ScenarioScope.Title }
                .Step("login", c =>
                {
                    c.Info($"popups shown on the title screen (closed by the runner before the scenarios): {string.Join(", ", TitlePopups)}");
                    c.Assert(TitlePopups.Contains(UIType.TextInfoLong.uiName), "Taoist Workshop description popup was shown");
                    c.Assert(CoverCount(UIType.Login) >= 2, $"custom covers on Login: {CoverCount(UIType.Login)} (ModLib + Taoist + S&M)");
                    c.Assert(FindButtons(UIType.Login, $"Taoist {ModConst.TAOIST_VERSION}").Count == 1, $"button 'Taoist {ModConst.TAOIST_VERSION}' exists");
                    c.Assert(VersionGreaterOrEqual(ModMaster.ModObj.Version, ModConst.MODLIB_REQUIRED_VERSION), $"ModLib {ModMaster.ModObj.Version} >= required {ModConst.MODLIB_REQUIRED_VERSION} (no warning popup)");
                    c.Assert(!TitlePopups.Contains(UIType.TextInfo.uiName), "no 'ModLib too old' warning popup");
                });
        }
    }
}
