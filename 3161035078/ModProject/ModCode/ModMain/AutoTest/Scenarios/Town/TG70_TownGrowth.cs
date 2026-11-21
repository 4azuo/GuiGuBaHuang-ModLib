using ModLib.Helper;
using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-70 (EstateEvent, the town map lives): the mortal houses grow above their district's level with a big population
    /// and a prosperous plot, and shrink back when the population falls; an NPC cultivator staying in the town buys a
    /// house with its spirit stones and upgrades it; the town map shows the NPC's house (blue) and the grown houses, a
    /// click tells whose house it is. Estate, budget, NPC money and place restored.
    /// </summary>
    public class TG70_TownGrowth : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-70";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildTown town = null;
            WorldUnitBase npc = null;
            return new Scenario { Id = "TG-70", Feature = "EstateEvent", Title = "Town map grows: mortal houses, NPC houses", Overlay = false }
                .Step("mortal houses grow", c =>
                {
                    if (!TravelTo(c, FindTown(t => t.IsSmallTown()) ?? FindTown(t => true))) return;
                    town = BagTown(c);
                    var e = EstateEvent.Get(town);
                    c.Bag["estate"] = JsonConvert.SerializeObject(e);
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(town);
                    var basePop = EstateEvent.BasePopulation(town);
                    var target0 = EstateEvent.Target(town, e);
                    // a big population: the houses can grow (+1, +1 more on a prosperous plot); every month a chance hit
                    e.Population = (long)(basePop * 1.3);
                    EstateEvent.GrowMortals(town, e, 400, () => 0);
                    var grown = EstateEvent.GrownLevels(e);
                    c.Assert(grown > 0, $"mortal houses grew {grown} level(s) above their districts'");
                    var over = e.Plots.Where(x => x.OwnerId == null && x.Level > 0)
                        .Count(x => x.Level > EstateEvent.MortalCap(EstateEvent.DistrictOf(x.District).MortalLevel, x.Prosperity, e.Population, basePop));
                    c.AssertEqual(0, over, "none above its cap");
                    var top = e.Plots.Where(x => x.OwnerId == null && x.Level > 0).Max(x => x.Level - EstateEvent.DistrictOf(x.District).MortalLevel);
                    c.Assert(top <= EstateEvent.Conf.Growth.MortalMaxBonus, $"at most +{EstateEvent.Conf.Growth.MortalMaxBonus} ({top})");
                    c.Assert(EstateEvent.Target(town, e) > target0, $"they draw residents: target {target0:#,##0} -> {EstateEvent.Target(town, e):#,##0}");
                    // the population falls: they shrink back to the districts' levels
                    var copy = JsonConvert.DeserializeObject<EstateEvent.TownEstate>(JsonConvert.SerializeObject(e));
                    copy.Population = basePop / 3;
                    EstateEvent.GrowMortals(town, copy, 400, () => 0);
                    var left = copy.Plots.Count(x => x.OwnerId == null && x.Level > 0 && x.Prosperity < EstateEvent.Conf.Growth.MortalProsperity && x.Level > EstateEvent.DistrictOf(x.District).MortalLevel);
                    c.AssertEqual(0, left, "a small population: the houses of plain plots shrink back");
                    // no chance hit: nothing changes
                    var before = EstateEvent.GrownLevels(e);
                    EstateEvent.GrowMortals(town, e, 12, () => 1);
                    c.AssertEqual(before, EstateEvent.GrownLevels(e), "no growth without the monthly chance");
                })
                .Step("an NPC buys a house", c =>
                {
                    if (town == null) return;
                    var e = EstateEvent.Get(town);
                    npc = Npcs.FirstOrDefault(x => !HirePeopleEvent.IsHired(x) && !EstateEvent.HousesOf(x).Any() && !MapBuildPropertyEvent.IsTownGuardian(town, x));
                    if (!c.Assert(npc != null, "an NPC without a house")) return;
                    c.Bag["npcPos"] = npc.GetUnitPos();
                    c.Bag["npcMoney"] = npc.GetUnitMoney();
                    npc.SetUnitPos(town.GetOrigiPoint());
                    npc.SetUnitMoney(100);
                    c.AssertEqual(null, EstateEvent.NpcHouse(npc, 1, () => 0, town), "too poor: no house");
                    npc.SetUnitMoney(int.MaxValue / 2);
                    c.AssertEqual(null, EstateEvent.NpcHouse(npc, 1, () => 1), "no house without the monthly chance");
                    var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    var money = npc.GetUnitMoney();
                    c.AssertEqual("buy", EstateEvent.NpcHouse(npc, 1, () => 0, town), $"{npc.GetName()} buys a house in {town.name}");
                    var house = EstateEvent.HouseOf(town, npc);
                    if (!c.Assert(house != null && house.Level == 1, "a thatched hut")) return;
                    var paid = money - npc.GetUnitMoney();
                    c.Assert(paid > 0 && paid <= money / 2, $"paid {paid:#,##0} spirit stones (at most half of them)");
                    c.AssertEqual(budget + paid, MapBuildPropertyEvent.GetBuildProperty(town), "to the town budget");
                    c.AssertEqual(null, EstateEvent.NpcHouse(npc, 1, () => 1), "no upgrade without the chance");
                    // upgrades as far as its grade and stones go (one level a hit)
                    for (var i = 0; i < EstateEvent.MAX_LEVEL; i++)
                        if (EstateEvent.NpcHouse(npc, 1, () => 0) == null) break;
                    c.Assert(house.Level >= 2, $"upgraded to a {EstateEvent.HouseName(house.Level)} (level {house.Level}, grade {npc.GetGradeLvl()})");
                    c.AssertEqual(EstateEvent.FootprintSize(house.Level), EstateEvent.PartsOf(e, house).Count, "on the plots of its level");
                    EstateEvent.OpenPlots(town);
                }, 1500)
                .Step("the house of the NPC", c =>
                {
                    if (npc == null) return;
                    var house = EstateEvent.HouseOf(town, npc);
                    c.Bag["houseText"] = string.Format(GameTool.LS("estate_info_owner"), npc.GetName(), EstateEvent.HouseName(house.Level), EstateEvent.DistrictName(house.District), house.Prosperity, EstateEvent.HouseTax(town, house));
                    CloseAll(UIType.TextInfoLong);
                    EstateEvent.ClickPlot(town, house);
                    // the text window of the plot (InfoDialog, UITextInfo since 6.0): which plot and whose house
                    var shown = MOD_nE7UL2.Helper.InfoDialog.LastText ?? string.Empty;
                    c.Assert(g.ui.HasUI(UIType.TextInfo), "the window of the plot (UITextInfo)");
                    c.Assert(shown.Contains((string)c.Bag["houseText"]), $"the plot tells whose house it is: {c.Bag["houseText"]}");
                    c.Assert(shown.Contains(EstateEvent.PlotWhere(house)), $"and which plot: {EstateEvent.PlotWhere(house)}");
                    c.Info($"popup: {shown}");
                }, 1200)
                .Step("info", c =>
                {
                    if (npc == null) return;
                    CloseAll(UIType.CheckPopup);
                    // the cultivator dies: the mortals move in
                    var e = EstateEvent.Get(town);
                    var house = EstateEvent.HouseOf(town, npc);
                    EstateEvent.Release(e, house, true);
                    c.Assert(house.OwnerId == null && house.Level == EstateEvent.DistrictOf(house.District).MortalLevel, "a released house goes to the mortals");
                })
                .Step("restore", c =>
                {
                    CloseAll(UIType.CheckPopup, UIType.TextInfoLong);
                    if (town == null) return;
                    EstateEvent.Instance.Towns[town.buildData.id] = JsonConvert.DeserializeObject<EstateEvent.TownEstate>((string)c.Bag["estate"]);
                    MapBuildPropertyEvent.AddBuildProperty(town, (long)c.Bag["budget"] - MapBuildPropertyEvent.GetBuildProperty(town));
                    if (npc != null && c.Bag.ContainsKey("npcPos"))
                    {
                        npc.SetUnitMoney((int)c.Bag["npcMoney"]);
                        npc.SetUnitPos((UnityEngine.Vector2Int)c.Bag["npcPos"]);
                    }
                    c.AssertEqual((long)c.Bag["budget"], MapBuildPropertyEvent.GetBuildProperty(town), "budget restored");
                });
        }
    }
}
