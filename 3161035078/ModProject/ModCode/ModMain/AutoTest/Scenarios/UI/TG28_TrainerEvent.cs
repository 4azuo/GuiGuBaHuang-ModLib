using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using ModLib.Helper;
using ModLib.Object;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.UI
{
    /// <summary>
    /// TG-28 (TrainerEvent): every trainer page opens and a trainer action changes the player.
    /// When the trainer is disabled in the S&amp;M configs its UI is still tested with a temporary instance.
    /// </summary>
    public class TG28_TrainerEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-28";

        private static UICustomBase Window => UIHelper.GetUICustomBase(UIType.TextInfoLong).LastOrDefault();

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            TrainerEvent trainer = null;
            var s = new Scenario { Id = "TG-28", Feature = "TrainerEvent", Title = "Trainer: all pages open and an action changes the player" }
                .Step("open page 1", c =>
                {
                    var loaded = IsLoaded(ModConst.TRAINER_EVENT);
                    c.Info(loaded ? "trainer enabled" : "trainer disabled in S&M configs: testing the window with a temporary instance");
                    trainer = loaded ? EventHelper.GetEvent<TrainerEvent>(ModConst.TRAINER_EVENT) : new TrainerEvent { Parent = ModMain.ModObj };   // pages use the ModChild ParameterStore
                    trainer.OpenTrainer(0);
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong) && Window != null, "trainer window opened");
                    c.Assert((Window?.Pages.Count ?? 0) >= 3, $"pages: {Window?.Pages.Count}");
                    c.AssertEqual(0, Window?.CurrentPageIndex ?? -1, "page 1 active");
                })
                .Step("add money", c =>
                {
                    var before = Player.GetUnitMoney();
                    trainer.AddMoney();
                    c.Assert(Player.GetUnitMoney() > before, $"money {before} -> {Player.GetUnitMoney()}");
                    Window?.UpdateUI();
                });
            for (var p = 2; p <= 4; p++)
            {
                var page = p;
                s.Step($"page {page}", c =>
                {
                    if (Window == null || Window.Pages.Count < page) { c.Skip($"no page {page}"); return; }
                    Window.NextPage();
                    c.AssertEqual(page - 1, Window.CurrentPageIndex, $"page {page} active");
                });
            }
            return s.Step("close", c =>
            {
                if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
                c.Assert(!g.ui.HasUI(UIType.TextInfoLong), "trainer window closed");
            });
        }
    }
}
