using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-14 (BattleRewardEventV2, lost real duel): with LostLifespanWhenDie the player loses lifespan (at least 6 months x game level) and the killer gains half of it.
    /// The lose options are set for the test only; the player state is restored afterwards.
    /// </summary>
    public class TB14_LoseLifespan : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-14";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-14", Feature = "BattleRewardEventV2", Title = "Battle lost with lost lifespan when die: life drained to the killer", Overlay = false }
                .Step("setup", c =>
                {
                    SetupLoss(c, lostLifespan: true);
                    c.Bag["mine"] = Player.GetProperty<int>(UnitPropertyEnum.Life);
                    c.Bag["his"] = LossNpc(c)?.GetProperty<int>(UnitPropertyEnum.Life) ?? 0;
                })
                .LoseDuel()
                .Step("penalty", c =>
                {
                    if (LossNpc(c) == null) return;
                    c.Assert(OnMap, "player back on the world map");
                    var drain = (int)c.Bag["mine"] - Player.GetProperty<int>(UnitPropertyEnum.Life);
                    var gameLvl = GameHelper.GetGameLevel().Parse<int>();
                    c.Assert(drain >= 6 * gameLvl, $"lifespan drained: {drain} months (>= 6 x game level {gameLvl})");
                    c.AssertEqual((int)c.Bag["his"] + drain / 2, LossNpc(c).GetProperty<int>(UnitPropertyEnum.Life), "the killer gained half of it");
                }, 2000)
                .Step("restore", c => RestoreAfterLoss(c));
        }
    }
}
