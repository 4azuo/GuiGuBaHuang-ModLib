using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-13 (BattleRewardEventV2, lost real duel): with LostMoneyWhenDie the player loses all its spirit stones and the killer gets them.
    /// The lose options are set for the test only; the player state is restored afterwards.
    /// </summary>
    public class TB13_LoseMoney : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-13";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-13", Feature = "BattleRewardEventV2", Title = "Battle lost with lost money when die: spirit stones go to the killer", Overlay = false }
                .Step("setup", c =>
                {
                    SetupLoss(c, lostMoney: true);
                    Player.AddUnitMoney(12345);
                    c.Bag["mine"] = Player.GetUnitMoney();
                    c.Bag["his"] = LossNpc(c)?.GetUnitMoney() ?? 0;
                })
                .LoseDuel()
                .Step("penalty", c =>
                {
                    if (LossNpc(c) == null) return;
                    c.Assert(OnMap, "player back on the world map");
                    c.AssertEqual(0, Player.GetUnitMoney(), $"player money {(int)c.Bag["mine"]} -> 0");
                    c.AssertEqual((int)c.Bag["his"] + (int)c.Bag["mine"], LossNpc(c).GetUnitMoney(), "the killer got the spirit stones");
                }, 2000)
                .Step("restore", c => RestoreAfterLoss(c));
        }
    }
}
