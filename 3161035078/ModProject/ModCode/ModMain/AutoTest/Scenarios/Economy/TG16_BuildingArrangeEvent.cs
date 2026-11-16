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

namespace MOD_nE7UL2.AutoTest.Scenarios.Economy
{
    /// <summary>
    /// TG-16 (BuildingArrangeEvent): Original buildings are known and building costs are valid.
    /// </summary>
    public class TG16_BuildingArrangeEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-16";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-16", Feature = "BuildingArrangeEvent", Title = "Original buildings are known and building costs are valid" }
                .Step("costs", c =>
                {
                    c.Assert(BuildingArrangeEvent.Instance.DefaultBuilding.Count > 0, $"original sub-buildings remembered: {BuildingArrangeEvent.Instance.DefaultBuilding.Count}");
                    var town = AnyTown;
                    if (town == null) { c.Skip("no town"); return; }
                    foreach (var e in BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => !BuildingArrangeEvent.IsIgnored(x)))
                        c.Assert(BuildingArrangeEvent.GetBuildingCost(town, e) >= 0, $"{town.name} {e.Name}: cost {BuildingArrangeEvent.GetBuildingCost(town, e)}, built {BuildingArrangeEvent.IsBuilt(town, e)}");
                });
        }
    }
}
