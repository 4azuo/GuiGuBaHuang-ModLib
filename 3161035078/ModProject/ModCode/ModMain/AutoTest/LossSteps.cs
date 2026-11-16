using ModLib.Mod;

namespace MOD_nE7UL2.AutoTest
{
    /// <summary>
    /// Steps of a lost real duel (TB-10..TB-15, TZ-01): start, "Fight", the player unit is killed by the opponent,
    /// back to the map. The steps are static methods (no lambda in this class) so that Scenario.Create() can append
    /// them outside the game (unit tests) without loading any game type.
    /// </summary>
    public static class LossSteps
    {
        /// <summary>Appends the steps. Needs TownTestHelper.SetupLoss in an earlier step.</summary>
        public static Scenario LoseDuel(this Scenario s)
        {
            return s
                .Step("start duel", StartDuel)
                .Wait("battle loaded", BattleLoaded, 90000, 1000)
                .Wait("battle started", BattleStarted, 60000, 1000)
                .Step("player dies", PlayerDies, 1500)
                .Wait("back to the map", BackToMap, 120000, 2000);
        }

        private static void StartDuel(AutoTestContext c)
        {
            if (TownTestHelper.LossNpc(c) != null)
                TownTestHelper.StartRealDuel(TownTestHelper.LossNpc(c));
        }

        private static bool BattleLoaded(AutoTestContext c)
        {
            if (TownTestHelper.LossNpc(c) == null) return true;
            TownTestHelper.StartBattle();
            return TownTestHelper.InBattle;
        }

        private static bool BattleStarted(AutoTestContext c) => TownTestHelper.LossNpc(c) == null || TownTestHelper.StartBattle();

        private static void PlayerDies(AutoTestContext c)
        {
            var npc = TownTestHelper.LossNpc(c);
            if (npc == null || !TownTestHelper.InBattle) return;
            var p = ModBattleEvent.PlayerUnit;
            p.DelState(UnitStateType.NotDie);
            p.Die(true, new MartialTool.HitData(TownTestHelper.BattleUnitOf(npc) ?? p, null, 0, 1, p.data.hp + p.data.maxHP.value));
            c.Assert(p.isDie, "player unit killed by the opponent");
        }

        private static bool BackToMap(AutoTestContext c) => TownTestHelper.LossNpc(c) == null || TownTestHelper.LeaveBattle();
    }
}
