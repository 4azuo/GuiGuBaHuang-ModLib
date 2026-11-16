using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Economy
{
    /// <summary>
    /// TG-54 (MapBuildPropertyEvent monthly town economy): an NPC staying in a town pays the town tax into the town
    /// budget and rests at the inn (HP back to max); an NPC who cannot pay is sent out of the town.
    /// </summary>
    public class TG54_TownTax : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-54";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase npc = null;
            return new Scenario { Id = "TG-54", Feature = "MapBuildPropertyEvent", Title = "Monthly town tax: NPC pays the town, or leaves" }
                .Step("NPC in a town", c =>
                {
                    var town = AnyTown;
                    if (!c.Assert(town != null, "a town")) return;
                    c.Bag["town"] = town;
                    npc = Npcs.FirstOrDefault(x => !HirePeopleEvent.IsHired(x) && !MapBuildPropertyEvent.IsTownGuardian(town, x));
                    if (!c.Assert(npc != null, "an NPC who is not guardian of the town")) return;
                    npc.SetUnitPos(town.GetOrigiPoint());
                    c.Assert(npc.GetMapBuild<MapBuildTown>()?.buildData.id == town.buildData.id, $"{npc.GetName()} in {town.name}");
                })
                .Step("pays the tax", c =>
                {
                    if (npc == null) return;
                    var town = BagTown(c);
                    var tax = MapBuildPropertyEvent.GetTax(town, npc);
                    if (SMLocalConfigsEvent.Instance.Configs.NoTaxing || tax <= 0) { c.Skip($"no tax (NoTaxing {SMLocalConfigsEvent.Instance.Configs.NoTaxing}, tax {tax})"); return; }
                    if (npc.GetUnitMoney() <= tax * 2) npc.AddUnitMoney(tax * 10);
                    npc.SetProperty<int>(UnitPropertyEnum.Hp, 1);
                    var m0 = npc.GetUnitMoney();
                    var b0 = MapBuildPropertyEvent.GetBuildProperty(town);
                    MapBuildPropertyEvent.Instance.OnMonthlyForEachWUnit(npc);
                    c.AssertEqual(m0 - tax, npc.GetUnitMoney(), $"NPC paid the tax {tax}");
                    c.AssertEqual(b0 + tax, MapBuildPropertyEvent.GetBuildProperty(town), "tax added to the town budget");
                    c.AssertEqual(npc.GetDynProperty(UnitDynPropertyEnum.HpMax).value, npc.GetDynProperty(UnitDynPropertyEnum.Hp).value, "rested at the inn (HP full)");
                })
                .Step("cannot pay", c =>
                {
                    if (npc == null) return;
                    var town = BagTown(c);
                    if (SMLocalConfigsEvent.Instance.Configs.NoTaxing || MapBuildPropertyEvent.GetTax(town, npc) <= 0) { c.Skip("no tax"); return; }
                    npc.AddUnitMoney(-npc.GetUnitMoney());
                    MapBuildPropertyEvent.Instance.OnMonthlyForEachWUnit(npc);
                    c.Assert(npc.GetMapBuild<MapBuildTown>()?.buildData.id != town.buildData.id, $"NPC without money left {town.name}");
                });
        }
    }
}
