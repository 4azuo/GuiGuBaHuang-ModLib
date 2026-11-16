using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-53 (limits of int values): values that keep growing must stop at the int maximum instead of wrapping to a
    /// negative number or falling back to 0 (ParseHelper.Parse returns 0 on overflow). Expertise exp, player
    /// properties (AddProperty), spirit stones and the inflation of a big price. Every value is restored.
    /// </summary>
    public class TG53_StatLimits : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-53";

        private const int NEAR_MAX = int.MaxValue - 10;

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-53", Feature = "ExpertEvent/WUnitHelper/InflationaryEvent", Title = "Int limits: growing values do not overflow" }
                .Step("expertise exp", c =>
                {
                    const string id = "__autotest_limit__";
                    ExpertEvent.GetExpertExps()[id] = NEAR_MAX;
                    var lvl = ExpertEvent.GetExpertLvl(id, 1, 0);
                    try
                    {
                        ExpertEvent.AddExpertExp(Player, id, 100f);
                        var exp = ExpertEvent.GetExpertExp(id);
                        c.Assert(exp >= NEAR_MAX, $"exp near the max + one use: {NEAR_MAX} -> {exp} (no wrap to negative)");
                        c.Assert(ExpertEvent.GetExpertLvl(id, 1, 0) >= lvl, $"expertise level kept: {lvl} -> {ExpertEvent.GetExpertLvl(id, 1, 0)}");
                    }
                    catch (Exception ex) { c.Assert(false, $"AddExpertExp near the max threw {ex.GetType().Name}"); }
                    finally { ExpertEvent.GetExpertExps().Remove(id); }
                })
                .Step("player property", c =>
                {
                    var old = Player.GetProperty<int>(UnitPropertyEnum.Attack);
                    try
                    {
                        Player.SetProperty<int>(UnitPropertyEnum.Attack, NEAR_MAX);
                        Player.AddProperty<int>(UnitPropertyEnum.Attack, 1000);
                        var v = Player.GetProperty<int>(UnitPropertyEnum.Attack);
                        c.Assert(v >= NEAR_MAX, $"Attack {NEAR_MAX} + 1000 -> {v} (not 0, not negative)");
                    }
                    catch (Exception ex) { c.Assert(false, $"AddProperty near the max threw {ex.GetType().Name}"); }
                    finally { Player.SetProperty<int>(UnitPropertyEnum.Attack, old); }
                    c.AssertEqual(old, Player.GetProperty<int>(UnitPropertyEnum.Attack), "Attack restored");
                })
                .Step("spirit stones", c =>
                {
                    var old = Player.GetUnitMoney();
                    try
                    {
                        Player.AddUnitMoney(int.MaxValue - old);
                        Player.AddUnitMoney(1000);
                        var v = Player.GetUnitMoney();
                        c.Assert(v > 0, $"money at the max + 1000 -> {v} (no wrap to negative)");
                    }
                    catch (Exception ex) { c.Assert(false, $"AddUnitMoney near the max threw {ex.GetType().Name}"); }
                    finally { Player.AddUnitMoney((int)Math.Max(int.MinValue, Math.Min(int.MaxValue, (long)old - Player.GetUnitMoney()))); }
                    c.AssertEqual(old, Player.GetUnitMoney(), "money restored");
                })
                .Step("inflation of a big price", c =>
                {
                    var index = EconomyHelper.GetWorldPrice();
                    try
                    {
                        var v = InflationaryEvent.CalculateInflationary(int.MaxValue / 2);
                        c.Assert(v > 0, $"CalculateInflationary(int.MaxValue / 2) with index {index} -> {v}");
                        if (index / 100d * (int.MaxValue / 2) >= int.MaxValue) c.AssertEqual(int.MaxValue, v, "capped at int.MaxValue");
                    }
                    catch (Exception ex) { c.Assert(false, $"CalculateInflationary(int.MaxValue / 2) with index {index} threw {ex.GetType().Name}"); }
                });
        }
    }
}
