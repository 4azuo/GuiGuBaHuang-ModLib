using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-03 (MapBuildBattleEvent/ModBattleEvent/BattleManashieldEvent/MonstStrongerEvent/BattleRewardEventV2,
    /// started by code): the player joins the monster wave of a town; monsters are buffed and counted, the player
    /// kills monsters (kill list), the battle is won and the player returns to the map (slice-life counter reset).
    /// </summary>
    public class TB03_MonstWaveBattle : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-03";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-03", Feature = "MapBuildBattleEvent/ModBattleEvent/BattleManashieldEvent/BattleRewardEventV2", Title = "Monster wave battle: join, kill, win, back to the map", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => t != null)))
                .Step("join wave", c =>
                {
                    c.Info($"expected manashield of the player: {BattleManashieldEvent.GetInitShield(Player)}");
                    MapBuildBattleEvent.JoinMonstWave(BagTown(c), MapBuildBattleEvent.TOWN_MONST_WAVE_DUNGEON_BASE_ID);
                })
                .Wait("battle loaded", c => { StartBattle(); return InBattle && ModBattleEvent.BattleMonsters.Count > 0; }, 90000, 1000)
                .Wait("battle started", c => StartBattle(), 60000, 1000)
                .Step("battle start", c =>
                {
                    if (!InBattle) return;
                    c.Assert(MapBuildBattleEvent.IsBattleMonstWave(), "battle is a town monster wave");
                    var monsters = ModBattleEvent.BattleMonsters;
                    c.Assert(monsters.All(x => x.data.maxHP.value > 0), $"{monsters.Count} monsters with valid stats");
                    c.Info($"player hp {ModBattleEvent.PlayerUnit.data.hp}/{ModBattleEvent.PlayerUnit.data.maxHP.value}, damage multiplier x{BattleModifyEvent.Instance.GetDamageMultiplier():0.00}");
                }, 3000)
                .Step("kill monsters", c =>
                {
                    if (!InBattle) return;
                    c.Bag["kills"] = ModBattleEvent.KillList.Count;
                    var targets = ModBattleEvent.BattleMonsters.Take(3).ToList();
                    foreach (var m in targets)
                    {
                        var hp = m.data.hp;
                        var how = PlayerKill(m);
                        c.Info($"monster hp {hp}/{m.data.maxHP.value}, def {m.data.defense.baseValue}: killed by {how} (hp after {m.data.hp})");
                    }
                    c.Assert(ModBattleEvent.KillList.Count >= (int)c.Bag["kills"] + targets.Count, $"kill list {(int)c.Bag["kills"]} -> {ModBattleEvent.KillList.Count} after {targets.Count} kills");
                }, 1500)
                .Step("win", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("after battle", c =>
                {
                    c.Assert(OnMap, "player back on the world map");
                    c.AssertEqual(0, SliceLifeQuestEvent.Instance.LastCount, "slice-life counter reset by the battle");
                }, 2000);
        }
    }
}
