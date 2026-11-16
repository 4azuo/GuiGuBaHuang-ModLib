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
    /// TG-20 (MonstStrongerEvent): Monster growth counters exist.
    /// </summary>
    public class TG20_MonstStrongerEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-20";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-20", Feature = "MonstStrongerEvent", Title = "Monster growth counters exist" }
                .Step("counters", c =>
                {
                    var m = MonstStrongerEvent.Instance;
                    foreach (var t in new[] { MonstType.Common, MonstType.Elite, MonstType.BOSS, MonstType.NPC })
                        c.Assert(m.KillCounter.ContainsKey(t) && m.Additional.ContainsKey(t), $"{t}: kills {(m.KillCounter.ContainsKey(t) ? m.KillCounter[t] : -1)}");
                    c.Info($"growth counter: {m.Counter}");
                });
        }
    }
}
