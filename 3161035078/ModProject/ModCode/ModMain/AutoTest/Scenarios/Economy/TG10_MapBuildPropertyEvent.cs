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
    /// TG-10 (MapBuildPropertyEvent): Town budget add/remove and taxes.
    /// </summary>
    public class TG10_MapBuildPropertyEvent : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-10";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-10", Feature = "MapBuildPropertyEvent", Title = "Town budget add/remove and taxes" }
                .Step("budget", c =>
                {
                    var town = AnyTown;
                    if (town == null) { c.Skip("no town"); return; }
                    var before = MapBuildPropertyEvent.GetBuildProperty(town);
                    MapBuildPropertyEvent.AddBuildProperty(town, 12345);
                    c.AssertEqual(before + 12345, MapBuildPropertyEvent.GetBuildProperty(town), $"{town.name}: budget +12345");
                    MapBuildPropertyEvent.AddBuildProperty(town, -12345);
                    c.AssertEqual(before, MapBuildPropertyEvent.GetBuildProperty(town), "budget restored");
                    c.Assert(MapBuildPropertyEvent.GetBaseTax(town) >= 0, $"base tax {MapBuildPropertyEvent.GetBaseTax(town)}");
                    var npc = Npcs.FirstOrDefault();
                    if (npc != null) c.Assert(MapBuildPropertyEvent.GetTax(town, npc) >= 0, $"tax of {npc.GetName()}: {MapBuildPropertyEvent.GetTax(town, npc)}");
                    var towns = ModMaster.ModObj.ParameterStore.Towns;
                    c.Info($"towns with a master: {towns.Count(x => MapBuildPropertyEvent.GetTownMaster(x) != null)}/{towns.Length}");
                });
        }
    }
}
