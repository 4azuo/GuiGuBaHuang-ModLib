using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TX-06 (yearly handlers): the OnYearly events of every Taoist feature run once: a new NPC is born
    /// (NpcAutoEvent), speed units move faster (UnitTypeEvent), every town with guardians has a master
    /// (MapBuildPropertyEvent), the yearly price index is saved (InflationaryEvent). Changes the save
    /// (restored by run-autotest.ps1).
    /// </summary>
    public class TX06_Yearly : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TX-06";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase speed = null;
            return new Scenario { Id = "TX-06", Feature = "NpcAutoEvent/UnitTypeEvent/MapBuildPropertyEvent/InflationaryEvent", Title = "Yearly handlers: NPC born, speed units, town masters" }
                .Step("run the yearly handlers", c =>
                {
                    c.Bag["units"] = g.world.unit.allUnit.Count;
                    var ut = UnitTypeEvent.Instance;
                    var id = ut.UnitTypeDic.FirstOrDefault(x => x.Value == UnitTypeEnum.SpeedUnit && g.world.unit.GetUnit(x.Key) != null && !g.world.unit.GetUnit(x.Key).IsPlayer()).Key;
                    speed = id == null ? null : g.world.unit.GetUnit(id);
                    if (speed != null) c.Bag["speed"] = speed.GetProperty<int>(UnitPropertyEnum.MoveSpeed);
                    EventHelper.RunMinorEvents("OnYearly");
                    c.Info($"OnYearly run for every Taoist feature (year {GameHelper.GetGameYear()})");
                })
                .Step("results", c =>
                {
                    var before = (int)c.Bag["units"];
                    c.Assert(g.world.unit.allUnit.Count > before, $"a new NPC was born: {before} -> {g.world.unit.allUnit.Count} units");
                    if (speed == null) c.Skip("no speed unit NPC");
                    else c.AssertEqual((int)c.Bag["speed"] + 1, speed.GetProperty<int>(UnitPropertyEnum.MoveSpeed), $"speed unit {speed.GetName()} moves faster");
                    var towns = (ModMain.Store?.Towns ?? new MapBuildTown[0]).Where(t => MapBuildPropertyEvent.GetTownGuardians(t).Count > 0).ToList();
                    var noMaster = towns.Where(t => MapBuildPropertyEvent.GetTownMaster(t) == null).Select(t => t.name).ToList();
                    c.Assert(noMaster.Count == 0, $"every town with guardians has a master ({towns.Count} towns{(noMaster.Count > 0 ? ", without: " + string.Join(", ", noMaster) : "")})");
                    c.AssertEqual(EconomyHelper.GetWorldPrice(), InflationaryEvent.Instance.WorldPrice, "yearly price index saved");
                }, 2000);
        }
    }
}
