using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-04 (BattleEvent/BattleManashieldEvent/BattleRewardEventV2, started by code): duel with an NPC (guard
    /// battle dungeon); the NPC gets its manashield, the player defeats it and returns to the map.
    /// </summary>
    public class TB04_NpcDuel : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-04";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase npc = null;
            return new Scenario { Id = "TB-04", Feature = "BattleEvent/BattleManashieldEvent/BattleRewardEventV2", Title = "NPC duel: start, defeat the NPC, back to the map", Overlay = false }
                .Step("start duel", c =>
                {
                    npc = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && !x.isDie).OrderBy(x => x.GetGradeLvl()).FirstOrDefault();
                    if (!c.Assert(npc != null, "an NPC to fight")) return;
                    c.Info($"opponent {npc.GetName()} grade {npc.GetGradeLvl()}, expected manashield {BattleManashieldEvent.GetInitShield(npc)}");
                    g.world.battle.IntoBattle(npc, MapBuildPropertyEvent.CATCH_SNEAKY_DUNGEON_ID, battleData: new WorldBattleData { npcUnit = npc });
                })
                .Wait("battle loaded", c => { StartBattle(); ProtectPlayer(); return npc == null || (InBattle && ModBattleEvent.BattleUnits.ToArray().Any(x => x.IsWorldUnit() && !x.IsPlayer())); }, 90000, 1000)
                .Wait("battle started", c => { ProtectPlayer(); return npc == null || StartBattle(); }, 60000, 1000)
                .Step("battle start", c =>
                {
                    if (!InBattle) return;
                    var enemy = ModBattleEvent.BattleUnits.ToArray().FirstOrDefault(x => x.IsWorldUnit() && !x.IsPlayer());
                    c.Assert(enemy != null, "opponent unit in the battle");
                    c.Bag["kills"] = ModBattleEvent.KillList.Count;
                    if (enemy != null) c.Info($"opponent hp {enemy.data.hp}/{enemy.data.maxHP.value}");
                }, 3000)
                .Step("defeat", c =>
                {
                    if (npc == null) return;
                    if (!c.Assert(InBattle, "battle still running (player protected)")) return;
                    ProtectPlayer();
                    var enemy = ModBattleEvent.BattleUnits.ToArray().FirstOrDefault(x => x.IsWorldUnit() && !x.IsPlayer());
                    if (enemy == null) return;
                    // a hit can be evaded (BattleModifyEvent evasion is random): hit again, up to 10 times
                    var hits = 0;
                    var hp0 = enemy.data.hp;
                    BattleModifyEvent.TraceDamage = true; // damage steps in the mod log
                    try
                    {
                    while (hits < 10 && enemy.data.hp > 1 && !enemy.isDie)
                    {
                        PlayerHit(enemy, (enemy.data.hp + enemy.data.maxHP.value) * 3);
                        hits++;
                    }
                    }
                    finally { BattleModifyEvent.TraceDamage = false; }
                    c.Info($"opponent hp {hp0} -> {enemy.data.hp} (damage steps: \"BattleModifyEvent dmg\" lines in the mod log)");
                    c.Assert(enemy.data.hp <= 1 || enemy.isDie, $"opponent hp after {hits} hit(s): {enemy.data.hp}");
                }, 2000)
                .Step("win", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000)
                .Step("after battle", c => c.Assert(OnMap, "player back on the world map"), 2000);
        }
    }
}
