using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Battle
{
    /// <summary>
    /// TB-01 (ModBattleEvent/MonstStrongerEvent/BattleManashieldEvent): Battle start: statistics reset, monsters buffed, humans shielded.
    /// </summary>
    public class TB01_ModBattleEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TB-01";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TB-01", Feature = "ModBattleEvent/MonstStrongerEvent/BattleManashieldEvent", Title = "Battle start: statistics reset, monsters buffed, humans shielded", Kind = ScenarioKind.Watch, Scope = ScenarioScope.Battle, Trigger = "BattleStart" }
                .Step("start", c =>
                {
                    c.Assert(ModBattleEvent.Instance.BattleDmg.Values.All(x => x == 0), "battle damage statistics reset");
                    c.Info($"dungeon {g.world.battle.data.dungeonBaseItem.id}, joinable by NPCs: {BattleEvent.IsJoinableBattle()}, town war: {MapBuildBattleEvent.IsBattleTownWar()}, wave: {MapBuildBattleEvent.IsBattleMonstWave()}");
                }, 2000)
                .Step("units", c =>
                {
                    var monsters = ModBattleEvent.BattleMonsters;
                    c.Info($"monsters: {monsters.Count}, units: {ModBattleEvent.BattleUnits.Count}");
                    c.Assert(monsters.All(x => x.data.maxHP.value > 0 && x.data.attack.value >= 0), "monster stats valid after buffs");
                }, 3000)
                .Step("fight", c =>
                {
                    c.Info($"damage multiplier x{BattleModifyEvent.Instance.GetDamageMultiplier():0.00}");
                }, 10000);
        }
    }
}
