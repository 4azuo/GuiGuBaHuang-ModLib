using MOD_nE7UL2.Mod;
using ModLib.Helper;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-05 (UnitTypeEvent): units receive a role and its luck. When no month passed yet (new save), the monthly
    /// handler is run once for the player and some NPCs.
    /// </summary>
    public class TG05_UnitTypeEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-05";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-05", Feature = "UnitTypeEvent", Title = "Units receive a role" }
                .Step("assign roles", c =>
                {
                    var dic = UnitTypeEvent.Instance.UnitTypeDic;
                    c.Info($"units with a role before: {dic.Count}");
                    if (UnitTypeEvent.GetUnitTypeEnum(Player) == null)
                    {
                        c.Info("no role yet (first month not passed): running the monthly handler for the player and 20 NPCs");
                        UnitTypeEvent.Instance.OnMonthlyForEachWUnit(Player);
                        foreach (var npc in Npcs.Take(20))
                            UnitTypeEvent.Instance.OnMonthlyForEachWUnit(npc);
                    }
                })
                .Step("check roles", c =>
                {
                    var role = UnitTypeEvent.GetUnitTypeEnum(Player);
                    c.Assert(role != null, $"player role: {role?.Name}");
                    var npcs = Npcs.Take(20).ToList();
                    var withRole = npcs.Count(x => UnitTypeEvent.GetUnitTypeEnum(x) != null);
                    c.Assert(npcs.Count == 0 || withRole == npcs.Count, $"NPCs with a role: {withRole}/{npcs.Count}");
                    if (role?.CustomLuck != null)
                        c.Assert(Player.GetLuck(role.Value.Parse<int>()) != null, "player has the luck of its role");
                    c.Info(string.Join(", ", UnitTypeEvent.Instance.UnitTypeDic.Values.GroupBy(x => x?.Name).Select(x => $"{x.Key}: {x.Count()}")));
                });
        }
    }
}
