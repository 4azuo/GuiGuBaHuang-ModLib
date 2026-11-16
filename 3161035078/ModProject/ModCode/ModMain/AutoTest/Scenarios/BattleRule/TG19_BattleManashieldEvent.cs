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

namespace MOD_nE7UL2.AutoTest.Scenarios.BattleRule
{
    /// <summary>
    /// TG-19 (BattleManashieldEvent): Mana shield of the player.
    /// </summary>
    public class TG19_BattleManashieldEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-19";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-19", Feature = "BattleManashieldEvent", Title = "Mana shield of the player" }
                .Step("shield", c =>
                {
                    c.Assert(BattleManashieldEvent.GetManashieldBase(Player) >= 0, $"base shield {BattleManashieldEvent.GetManashieldBase(Player)}");
                    c.Assert(BattleManashieldEvent.GetInitShield(Player) >= BattleManashieldEvent.GetManashieldBase(Player), $"initial shield {BattleManashieldEvent.GetInitShield(Player)}");
                });
        }
    }
}
