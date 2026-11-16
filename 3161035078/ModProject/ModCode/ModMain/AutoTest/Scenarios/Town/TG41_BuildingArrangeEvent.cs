using ModLib.Helper;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-41 (BuildingArrangeEvent): a destroyed original sub-building is rebuilt by the town with its budget
    /// (town UI before/after).
    /// </summary>
    public class TG41_BuildingArrangeEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-41";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-41", Feature = "BuildingArrangeEvent", Title = "Town rebuilds a destroyed building with its budget", Overlay = false }
                .Step("travel", c =>
                {
                    var town = FindTown(t => BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Any(e => BuildingArrangeEvent.IsDestroyable(t, e)));
                    if (TravelTo(c, town))
                        c.Bag["building"] = BuildingCostEnum.GetAllEnums<BuildingCostEnum>().First(e => BuildingArrangeEvent.IsDestroyable(town, e));   // in the bag: a typed closure field would load game enums in Create()
                })
                .Step("destroy", c =>
                {
                    var town = BagTown(c);
                    if (!c.Bag.ContainsKey("building")) { c.Skip("no town with a destroyable building"); return; }
                    var building = (BuildingCostEnum)c.Bag["building"];
                    BuildingArrangeEvent.Destroy(town, building);
                    c.Assert(!BuildingArrangeEvent.IsBuilt(town, building), $"{building.Name} destroyed in {town.name}");
                    town.OpenBuild();
                }, 500)
                .Step("rebuild", c =>
                {
                    CloseAll(UIType.Town);
                    var town = BagTown(c);
                    if (!c.Bag.ContainsKey("building")) { c.Skip("no town with a destroyable building"); return; }
                    var building = (BuildingCostEnum)c.Bag["building"];
                    var cost = BuildingArrangeEvent.GetBuildingCost(town, building);
                    MapBuildPropertyEvent.AddBuildProperty(town, cost * 2 + 1000);
                    var before = MapBuildPropertyEvent.GetBuildProperty(town);
                    var tries = 0;
                    while (!BuildingArrangeEvent.IsBuilt(town, building) && tries++ < 500)
                        BuildingArrangeEvent.Instance.AddBuildSub(town, building);
                    c.Assert(BuildingArrangeEvent.IsBuilt(town, building), $"{building.Name} rebuilt after {tries} monthly tries (build rate {building.BuildRate}%)");
                    c.AssertEqual(before - cost, MapBuildPropertyEvent.GetBuildProperty(town), $"cost {cost} paid from the town budget");
                    town.OpenBuild();
                }, 500)
                .Step("close", c => CloseAll(UIType.Town), 2500);
        }
    }
}
