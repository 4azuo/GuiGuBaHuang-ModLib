using ModLib.Helper;
using ModLib.Object;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.UI
{
    /// <summary>
    /// TG-51 (TrainerEvent): the Ctrl+1/2/3 hotkeys (TrainerEvent.OnHotkey, called by OnMonoUpdate for the key
    /// presses) open the trainer on page 1/2/3, switch pages while it is open, and open it again after the window
    /// was closed. Only the three Input.GetKey lines are not covered. Replaces the manual TM-32.
    /// </summary>
    public class TG51_TrainerHotkeys : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-51";

        private static UICustomBase Window => UIHelper.GetUICustomBase(UIType.TextInfoLong).LastOrDefault();

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            TrainerEvent trainer = null;
            return new Scenario { Id = "TG-51", Feature = "TrainerEvent", Title = "Trainer hotkeys Ctrl+1/2/3" }
                .Step("Ctrl+2", c =>
                {
                    CloseAll(UIType.TextInfoLong);
                    c.Info(IsLoaded(ModConst.TRAINER_EVENT) ? "trainer enabled" : "trainer disabled in S&M configs: testing with a temporary instance");
                    trainer = FeatureEvent<TrainerEvent>(ModConst.TRAINER_EVENT);
                    trainer.OnHotkey(1);
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong) && Window != null, "trainer opened");
                    c.AssertEqual(1, Window?.CurrentPageIndex ?? -1, "page 2 active");
                })
                .Step("Ctrl+3", c =>
                {
                    trainer.OnHotkey(2);
                    c.AssertEqual(2, Window?.CurrentPageIndex ?? -1, "page 3 active in the open window");
                })
                .Step("Ctrl+1", c =>
                {
                    trainer.OnHotkey(0);
                    c.AssertEqual(0, Window?.CurrentPageIndex ?? -1, "page 1 active");
                    var before = Player.GetUnitMoney();
                    trainer.AddMoney();
                    c.Assert(Player.GetUnitMoney() > before, "a page 1 action works");
                })
                .Step("close with X, Ctrl+1 again", c =>
                {
                    g.ui.CloseUI(UIType.TextInfoLong);
                    c.Assert(!g.ui.HasUI(UIType.TextInfoLong), "trainer closed (window close button)");
                    trainer.OnHotkey(0);
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "the hotkey opens the trainer again after it was closed");
                })
                .Step("close", c =>
                {
                    CloseAll(UIType.TextInfoLong);
                    c.Assert(!g.ui.HasUI(UIType.TextInfoLong), "trainer closed");
                });
        }
    }
}
