using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-05 (SpiritlockEvent, started by code): in the spiritlock circle dungeon the venerable tortoises and the
    /// circle cannot move.
    /// </summary>
    public class TB05_SpiritlockBattle : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-05";

        private static bool IsSpiritlockUnit(UnitCtrlBase x)
        {
            var id = x.data.TryCast<UnitDataMonst>()?.unitAttrItem?.id ?? 0;
            return SpiritlockEvent.VENERABLE_TORTOISES.Contains(id) || id == SpiritlockEvent.SPIRITLOCK_CIRCLE;
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-05", Feature = "SpiritlockEvent", Title = "Spiritlock circle: tortoises cannot move", Overlay = false }
                .Step("enter dungeon", c =>
                {
                    var level = (CurrentTown?.gridData.areaBaseID ?? 1) * 5;
                    c.Info($"spiritlock dungeon 7007, level {level}");
                    g.world.battle.IntoBattle(new DataMap.MonstData { id = 7007, level = level });
                })
                .Wait("battle loaded", c => { StartBattle(); return InBattle; }, 90000, 1000)
                .Wait("battle started", c => StartBattle(), 60000, 1000)
                .Wait("spiritlock units", c => InBattle && ModBattleEvent.BattleMonsters.Any(IsSpiritlockUnit), 30000, 1000)
                .Step("units cannot move", c =>
                {
                    if (!InBattle) return;
                    var units = ModBattleEvent.BattleMonsters.Where(IsSpiritlockUnit).ToList();
                    c.Assert(units.Count > 0, $"{units.Count} tortoise/circle units");
                    c.Assert(units.All(x => x.data.moveSpeed.baseValue == 0), "move speed 0 for all of them");
                }, 2000)
                .Step("leave", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("after battle", c => c.Assert(OnMap, "player back on the world map"), 2000);
        }
    }
}
