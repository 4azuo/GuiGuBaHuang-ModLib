using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TX-09 (TimeSkipEvent): timings of the time skip. 3 months with the month run of the game every month (full
    /// simulation), then 10 years with the default skip (4 month runs of the game, the other months of the mods only).
    /// The timings are in the step infos and the log ("TimeSkipEvent: done, ..."). Changes the save (restored by run-autotest.ps1).
    /// </summary>
    public class TX09_SkipTiming : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TX-09";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TX-09", Feature = "TimeSkipEvent", Title = "Time skip timings (full simulation, 10 years)", Overlay = false }
                .Step("full simulation: 3 months", c =>
                {
                    DismissDramas();
                    // no NPC drama (they interrupt the skip): deterministic timings
                    TimeSkipEvent.HidePlayer = true;
                    c.Bag["month"] = GameHelper.GetGameTotalMonth();
                    TimeSkipEvent.FullSimulation = true;
                    TimeSkipEvent.Instance.SkipTime(3);
                })
                .Wait("3 months pass", c => !TimeSkipEvent.Instance.IsProcessing, 600000, 1000)
                .Step("full simulation timing", c =>
                {
                    TimeSkipEvent.FullSimulation = false;
                    var t = TimeSkipEvent.Instance.LastTiming;
                    c.Info($"full simulation: {t}");
                    c.AssertEqual(3, GameHelper.GetGameTotalMonth() - (int)c.Bag["month"], "3 months passed");
                    c.AssertEqual(3, t.GameRuns, "a month run of the game every month");
                    c.Bag["month"] = GameHelper.GetGameTotalMonth();
                    TimeSkipEvent.Instance.SkipTime(120);
                })
                .Wait("10 years pass", c => !TimeSkipEvent.Instance.IsProcessing, 3600000, 2000)
                .Step("10 years timing", c =>
                {
                    var t = TimeSkipEvent.Instance.LastTiming;
                    c.Info($"10 years: {t}");
                    c.AssertEqual(120, GameHelper.GetGameTotalMonth() - (int)c.Bag["month"], "120 months passed");
                    c.AssertEqual(TimeSkipEvent.SAVES, t.GameRuns, "4 month runs of the game");
                    c.Assert(!g.ui.HasUI(UIType.Loading), "loading window closed");
                    DismissDramas();
                    TimeSkipEvent.HidePlayer = false;
                })
                .Step("interrupted by a drama", c =>
                {
                    c.Bag["month"] = GameHelper.GetGameTotalMonth();
                    TimeSkipEvent.Instance.SkipTime(24);
                    // something happens to the player during the skip (a drama of the game or of a mod)
                    ModDelayEvent.Instance.DelayEvent(TimeSkipEvent.Instance, () =>
                        DramaHelper.OpenDrama2("TX-09: someone is looking for you", new System.Collections.Generic.List<string> { "OK" }, null, "xinjianhualuo1"), 1500);
                })
                .Wait("the skip stops", c => !TimeSkipEvent.Instance.IsProcessing, 600000, 1000)
                .Step("interrupted", c =>
                {
                    DismissDramas();
                    var passed = GameHelper.GetGameTotalMonth() - (int)c.Bag["month"];
                    c.Assert(TimeSkipEvent.Instance.Interrupted, "interrupted");
                    c.Assert(passed > 0 && passed < 24, $"stopped early: {passed} / 24 months");
                    c.Assert(!g.ui.HasUI(UIType.Loading), "loading window closed");
                    c.Info($"interrupted: {TimeSkipEvent.Instance.LastTiming}");
                });
        }
    }
}
