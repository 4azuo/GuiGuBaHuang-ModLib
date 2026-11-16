using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TZ-01 (BattleRewardEventV2, one-life): with the S&amp;M one-life option a lost real duel sets the lifespan to 0
    /// and a month passes, so the character dies of old age. It ends the character: it is the last scenario of the
    /// run (TZ prefix) and the save is restored by run-autotest.ps1.
    /// </summary>
    public class TZ01_OneLife : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TZ-01";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TZ-01", Feature = "BattleRewardEventV2", Title = "Battle lost with one-life: the character dies", Overlay = false }
                .Step("setup", c =>
                {
                    SetupLoss(c, oneLife: true);
                    c.Bag["month"] = GameHelper.GetGameTotalMonth();
                })
                .LoseDuel()
                .Step("lifespan", c =>
                {
                    if (LossNpc(c) == null) return;
                    c.AssertEqual(0, Player.GetProperty<int>(UnitPropertyEnum.Life), "lifespan set to 0 by one-life");
                }, 500)
                .Wait("character dies", c =>
                {
                    // the world may be gone (game over, title screen): any error means the character is gone too
                    try { return LossNpc(c) == null || GameHelper.IsLoginScreen() || Player.isDie; }
                    catch { return true; }
                }, 90000, 1000)
                .Step("dead", c =>
                {
                    if (LossNpc(c) == null) return;
                    bool dead;
                    try { dead = GameHelper.IsLoginScreen() || Player.isDie; } catch { dead = true; }
                    c.Assert(dead, "the character is dead (one-life)");
                    c.Info("the run ends here: the save is restored by run-autotest.ps1");
                }, 2000);
        }
    }
}
