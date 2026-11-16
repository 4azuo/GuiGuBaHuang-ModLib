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
    /// TG-08 (CustomRefineEvent): Refine exp API.
    /// </summary>
    public class TG08_CustomRefineEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-08";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-08", Feature = "CustomRefineEvent", Title = "Refine exp API" }
                .Step("exp", c =>
                {
                    const string id = "__autotest_refine__";
                    c.Assert(!CustomRefineEvent.AddRefineExp((string)null, 10), "null item rejected");
                    c.Assert(!CustomRefineEvent.AddRefineExp(id, 0), "0 exp rejected");
                    c.Assert(CustomRefineEvent.AddRefineExp(id, 100), "exp added");
                    CustomRefineEvent.AddRefineExp(id, 50);
                    c.AssertEqual(150L, CustomRefineEvent.GetRefineExp(id), "exp accumulates");
                    CustomRefineEvent.Instance.RefineExp.Remove(id);
                    var items = CustomRefineEvent.GetEquippedRefinableItems(Player);
                    c.Info($"player refinable items: {string.Join(", ", items.Select(x => $"{x.propsInfoBase.name} lv{CustomRefineEvent.GetRefineLvl(x)}"))}");
                });
        }
    }
}
