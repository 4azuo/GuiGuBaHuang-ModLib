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

namespace MOD_nE7UL2.AutoTest.Scenarios.Unit
{
    /// <summary>
    /// TG-17 (UnitModifyEvent/QiCulEvent): Unit modifiers are hooked and bonuses are not negative.
    /// </summary>
    public class TG17_UnitModifyEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-17";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-17", Feature = "UnitModifyEvent/QiCulEvent", Title = "Unit modifiers are hooked and bonuses are not negative" }
                .Step("modifiers", c =>
                {
                    c.Assert(UnitModifyEvent.Instance.CachedUnits.Contains(Player.GetUnitId()), "player DynInts hooked");
                    c.Assert(UnitModifyEvent.GetAdjustAtk(Player) >= 0, $"atk bonus {UnitModifyEvent.GetAdjustAtk(Player)}");
                    c.Assert(UnitModifyEvent.GetAdjustDef(Player) >= 0, $"def bonus {UnitModifyEvent.GetAdjustDef(Player)}");
                    c.Assert(UnitModifyEvent.GetAdjustMaxHp(Player) >= 0, $"max HP bonus {UnitModifyEvent.GetAdjustMaxHp(Player)}");
                    c.Assert(UnitModifyEvent.GetAdjustMaxMp(Player) >= 0, $"max MP bonus {UnitModifyEvent.GetAdjustMaxMp(Player)}");
                    if (!IsLoaded(ModConst.QI_CUL_EVENT)) { c.Skip("Qi cultivation off"); return; }
                    c.Assert(UnitModifyHelper.GetQiAdjAtk(Player) >= 0, $"Qi atk {UnitModifyHelper.GetQiAdjAtk(Player)}");
                });
        }
    }
}
