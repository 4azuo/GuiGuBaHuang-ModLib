using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-11 (BattleRewardEventV2, lost real duel): at grade 4 with a body reconstruction item (1011721) the item is used instead of the exp.
    /// The lose options are set for the test only; the player state is restored afterwards.
    /// </summary>
    public class TB11_LoseWithBodyItem : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-11";

        private const int BODY_ITEM = 1011721; // body reconstruction item of grade 4 (BattleRewardEventV2.BodyReconstructions)

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-11", Feature = "BattleRewardEventV2", Title = "Battle lost with a body reconstruction item: item used, exp kept", Overlay = false }
                .Step("setup", c =>
                {
                    SetupLoss(c);
                    var g4 = g.conf.roleGrade._allConfList.ToArray().FirstOrDefault(x => x.grade == 4);
                    if (!c.Assert(g4 != null, "grade 4 phase config")) return;
                    Player.SetProperty<int>(UnitPropertyEnum.GradeID, g4.id);
                    c.AssertEqual(4, Player.GetGradeLvl(), "player set to grade 4 for the test");
                    Player.AddUnitProp(BODY_ITEM, 1);
                    Player.ClearExp();
                    Player.AddExp(5000);
                    c.Bag["exp0"] = Player.GetExp();
                    c.Bag["items"] = Player.GetUnitPropCount(BODY_ITEM);
                })
                .LoseDuel()
                .Step("penalty", c =>
                {
                    if (LossNpc(c) == null) return;
                    c.Assert(OnMap, "player back on the world map");
                    c.AssertEqual((int)c.Bag["items"] - 1, Player.GetUnitPropCount(BODY_ITEM), "one body reconstruction item used");
                    c.AssertEqual((int)c.Bag["exp0"], Player.GetExp(), "exp kept");
                    Player.RemoveUnitProp(BODY_ITEM);
                }, 2000)
                .Step("restore", c => RestoreAfterLoss(c));
        }
    }
}
