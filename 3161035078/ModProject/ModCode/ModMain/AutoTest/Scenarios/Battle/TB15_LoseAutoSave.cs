using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-15 (BattleRewardEventV2, lost real duel): with AutoSaveAfterLostInBattle a month passes after the defeat (TimeSkipEvent.SkipTime(1)) and the game is saved, without overlapping saves.
    /// The lose options are set for the test only; the player state is restored afterwards.
    /// </summary>
    public class TB15_LoseAutoSave : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-15";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-15", Feature = "BattleRewardEventV2", Title = "Battle lost with auto save after lost: one month passes and the game saves", Overlay = false }
                .Step("setup", c =>
                {
                    SetupLoss(c, autoSave: true);
                    c.Bag["month"] = GameHelper.GetGameTotalMonth();
                })
                .LoseDuel()
                .Step("penalty", c =>
                {
                    if (LossNpc(c) == null) return;
                    c.Assert(OnMap, "player back on the world map");
                    c.Info($"month {(int)c.Bag["month"]} -> {GameHelper.GetGameTotalMonth()} (the skip runs one month per second)");
                }, 2000)
                .Wait("month passed and saved", c => LossNpc(c) == null || (GameHelper.GetGameTotalMonth() > (int)c.Bag["month"] && !TimeSkipEvent.Instance.IsProcessing && !DataMgr.isSaveData), 60000, 1000)
                .Step("saved", c =>
                {
                    if (LossNpc(c) == null) return;
                    c.AssertEqual((int)c.Bag["month"] + 1, GameHelper.GetGameTotalMonth(), "one month passed after the defeat");
                    c.Assert(!DataMgr.isSaveData, "save finished");
                }, 2000)
                .Step("restore", c => RestoreAfterLoss(c));
        }
    }
}
