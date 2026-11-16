using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System.Diagnostics;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-09 (BattleRewardEventV2, started by code): a real duel won by the player. A big hit makes the "training"
    /// growth loop (properties grow with the damage dealt) run many times and end quickly; winning gives exp from the
    /// damage dealt/received. Player exp and Attack are restored afterwards.
    /// </summary>
    public class TB09_BattleWinRewards : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-09";

        private const int BIG_HIT = 50000000;

        // damage dealt by the player in this battle (ModLib counter, filled by the UnitHit events)
        private static long DealtDamage() => ModBattleEvent.sGetDmg(ModBattleEvent.DmgSaveEnum.Local, ModBattleEvent.GetDmgKey(ModBattleEvent.DmgEnum.DmgDealt, ModBattleEvent.DmgTypeEnum.Damage));

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase npc = null;
            return new Scenario { Id = "TB-09", Feature = "BattleRewardEventV2", Title = "Battle won: training growth and exp from damage", Overlay = false }
                .Step("start duel", c =>
                {
                    npc = Npcs.Where(x => !HirePeopleEvent.IsHired(x)).OrderBy(x => x.GetGradeLvl()).FirstOrDefault();
                    if (!c.Assert(npc != null, "an NPC to fight")) return;
                    c.Bag["exp"] = Player.GetExp();
                    c.Bag["atk"] = Player.GetProperty<int>(UnitPropertyEnum.Attack);
                    Player.ClearExp(); // room for the battle exp
                    c.Bag["exp0"] = Player.GetExp();
                    c.Info($"opponent {npc.GetName()} grade {npc.GetGradeLvl()}; NoGrowupFromBattles {SMLocalConfigsEvent.Instance.Configs.NoGrowupFromBattles}, NoExpFromBattles {SMLocalConfigsEvent.Instance.Configs.NoExpFromBattles}");
                    StartRealDuel(npc);
                })
                .Wait("battle loaded", c => { StartBattle(); ProtectPlayer(); return npc == null || InBattle; }, 90000, 1000)
                .Wait("battle started", c => { ProtectPlayer(); return npc == null || StartBattle(); }, 60000, 1000)
                .Step("prepare hit", c =>
                {
                    if (npc == null) return;
                    if (!c.Assert(InBattle, "battle still running (player protected)")) return;
                    ProtectPlayer();
                    var o = BattleUnitOf(npc);
                    if (!c.Assert(o != null, "opponent in the battle")) return;
                    // a hit the opponent survives: NotDie blocks a lethal hit completely (0 damage applied, nothing counted),
                    // so the opponent gets 2 x BIG_HIT hp instead
                    o.data.maxHP.baseValue = BIG_HIT * 2;
                    o.data.hp = BIG_HIT * 2;
                    c.Bag["atk0"] = Player.GetProperty<int>(UnitPropertyEnum.Attack);
                    c.Bag["hitMs"] = 0L;
                }, 500)
                // hit until the game counts the damage: a hit right after the battle intro can be ignored by the game
                // (isStartBattle is already true), so it is repeated every second
                .Wait("big hit applied", c =>
                {
                    if (npc == null || !InBattle) return true;
                    ProtectPlayer();
                    if (DealtDamage() > 0) return true;
                    var o = BattleUnitOf(npc);
                    if (o == null) return true;
                    var sw = Stopwatch.StartNew();
                    BattleModifyEvent.TraceDamage = true; // damage steps in the mod log
                    try { PlayerHit(o, BIG_HIT); }
                    finally { BattleModifyEvent.TraceDamage = false; }
                    sw.Stop();
                    c.Bag["hitMs"] = System.Math.Max((long)c.Bag["hitMs"], sw.ElapsedMilliseconds);
                    return false;
                }, 20000, 1000)
                .Step("big hit", c =>
                {
                    if (npc == null || !c.Bag.ContainsKey("hitMs")) return;
                    c.Assert(DealtDamage() > 0, $"damage dealt counted: {DealtDamage()}");
                    c.Assert((long)c.Bag["hitMs"] < 3000, $"hit of {BIG_HIT} handled in {c.Bag["hitMs"]} ms (training growth loop ends)");
                }, 500)
                .Step("win", c =>
                {
                    if (!InBattle) return;
                    PlayerKill(BattleUnitOf(npc));
                    EndBattle(true);
                }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("rewards", c =>
                {
                    if (npc == null) return;
                    c.Assert(OnMap, "player back on the world map");
                    var exp0 = (int)c.Bag["exp0"];
                    if (SMLocalConfigsEvent.Instance.Configs.NoExpFromBattles) c.Skip("NoExpFromBattles on: no battle exp");
                    else c.Assert(Player.GetExp() > exp0, $"exp from the battle: {exp0} -> {Player.GetExp()}");
                    // the training growth runs with the UnitHit events of the hits (applied by the game on later frames):
                    // checked once the battle is over
                    var atk0 = (int)c.Bag["atk0"];
                    var atk = Player.GetProperty<int>(UnitPropertyEnum.Attack);
                    if (SMLocalConfigsEvent.Instance.Configs.NoGrowupFromBattles) c.Info("NoGrowupFromBattles on: no training growth");
                    else c.Assert(atk > atk0, $"Attack grew with the damage dealt: {atk0} -> {atk}");
                }, 2000)
                .Step("restore", c =>
                {
                    if (npc == null) return;
                    Player.SetProperty<int>(UnitPropertyEnum.Attack, (int)c.Bag["atk"]);
                    Player.ClearExp();
                    Player.AddExp((int)c.Bag["exp"]);
                    c.AssertEqual((int)c.Bag["atk"], Player.GetProperty<int>(UnitPropertyEnum.Attack), "Attack restored");
                });
        }
    }
}
