using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-08 (RealTrialEvent, started by code): in the trial of lightning (dungeon 110081) the lightning units hit
    /// with a damage scaled on the player (attack x (game level and grade factors) - defense x thunder basis), and
    /// surviving the trial raises the player properties of the Trial role. Replaces the manual TM-25.
    /// </summary>
    public class TB08_TrialOfLightning : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-08";

        /// <summary>Trial of lightning dungeon (DungeonHelper.IsTrailOfLightning).</summary>
        public const int TRIAL_DUNGEON_ID = 110081;

        private static int ExpectedAttack()
        {
            var p = g.world.playerUnit;
            var atk = p.GetDynProperty(UnitDynPropertyEnum.Attack).value;
            var def = p.GetDynProperty(UnitDynPropertyEnum.Defense).value;
            var thunder = p.GetDynProperty(UnitDynPropertyEnum.BasisThunder).value;
            var cfg = RealTrialEvent.RealTrialConfigs;
            return (atk * (GameHelper.GetGameLevel().Parse<int>() * cfg.PowerUpOnGameLevel + p.GetGradeLvl() * cfg.PowerUpOnGradeLevel)).Parse<int>() - (def * thunder / 100);
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-08", Feature = "RealTrialEvent", Title = "Trial of lightning: damage scaled on the player, properties after surviving", Overlay = false }
                .Step("enter trial", c =>
                {
                    c.Bag["expected"] = ExpectedAttack();
                    c.Bag["props"] = UnitTypeEnum.Trial.PropIncRatio.Select(p => p.Values[0] as UnitPropertyEnum).Where(p => p != null)
                        .ToDictionary(p => p, p => Player.GetProperty<int>(p));
                    c.Info($"expected lightning attack {c.Bag["expected"]}");
                    g.world.battle.IntoBattle(new DataMap.MonstData { id = TRIAL_DUNGEON_ID, level = Player.GetGradeLvl() * 5 });
                })
                .Wait("battle loaded", c => { StartBattle(); return InBattle; }, 90000, 1000)
                .Wait("battle started", c => StartBattle(), 60000, 1000)
                .Wait("lightning units", c => !InBattle || ModBattleEvent.BattleUnits.ToArray().Any(x => !x.IsPlayer() && !x.IsPotmon() && !x.IsSummoned()), 60000, 1000)
                .Step("lightning damage", c =>
                {
                    if (!InBattle) return;
                    c.Assert(RealTrialEvent.IsInTrial, $"battle is the trial of lightning (dungeon {g.world.battle.data.dungeonBaseItem.id})");
                    var units = ModBattleEvent.BattleUnits.ToArray().Where(x => !x.IsPlayer() && !x.IsPotmon() && !x.IsSummoned()).ToList();
                    if (!c.Assert(units.Count > 0, $"{units.Count} trial units")) return;
                    var expected = (int)c.Bag["expected"];
                    c.Assert(units.All(x => x.data.attack.baseValue == expected), $"trial unit attack = {expected}: {string.Join(", ", units.Select(x => x.data.attack.baseValue).Distinct())}");
                }, 2000)
                .Step("survive", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("properties", c =>
                {
                    c.Assert(OnMap, "player back on the world map");
                    var before = (System.Collections.Generic.Dictionary<UnitPropertyEnum, int>)c.Bag["props"];
                    if (!c.Assert(before.Count > 0, $"{before.Count} trial properties")) return;
                    foreach (var p in before)
                        c.Assert(Player.GetProperty<int>(p.Key) > p.Value, $"{p.Key}: {p.Value} -> {Player.GetProperty<int>(p.Key)}");
                }, 2000);
        }
    }
}
