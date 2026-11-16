using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-10 (BattleRewardEventV2, lost real duel): without a body reconstruction item for its grade the player loses its cultivation exp (phase minimum).
    /// The lose options are set for the test only; the player state is restored afterwards.
    /// </summary>
    public class TB10_LoseNoBodyItem : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-10";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-10", Feature = "BattleRewardEventV2", Title = "Battle lost without body reconstruction item: exp lost", Overlay = false }
                .Step("setup", c =>
                {
                    SetupLoss(c);
                    Player.ClearExp();
                    Player.AddExp(500);
                    c.Bag["exp0"] = Player.GetExp();
                    c.Bag["min"] = Player.GetMinExpCurrentPhase();
                    c.Info($"grade {Player.GetGradeLvl()} (no body reconstruction item below grade 4), exp {c.Bag["exp0"]}");
                })
                .LoseDuel()
                .Step("penalty", c =>
                {
                    if (LossNpc(c) == null) return;
                    c.Assert(OnMap, "player back on the world map");
                    c.AssertEqual((int)c.Bag["min"], Player.GetExp(), $"exp lost: {(int)c.Bag["exp0"]} -> {Player.GetExp()}");
                }, 2000)
                .Step("restore", c => RestoreAfterLoss(c));
        }
    }
}
