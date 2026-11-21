using ModLib.Helper;
using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-73 (EstateEvent, NPC houses in depth): the monthly hook buys a house for an NPC staying in the town; the
    /// rules: the feature off, the share of the plots, one house in the world, the grade of the next level, at most half
    /// of the stones; the house tax paid by the NPC every month, unpaid months, the house taken back after MaxUnpaidMonths;
    /// the house of a dead cultivator goes to the mortals; the mortal houses over months (the monthly chance). Changes the
    /// save (an NPC dies; restored by run-autotest.ps1); estate, budget, settings, money and places restored.
    /// </summary>
    public class TG73_NpcHouses : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-73";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildTown town = null;
            WorldUnitBase npc = null, other = null;
            return new Scenario { Id = "TG-73", Feature = "EstateEvent", Title = "NPC houses in depth: rules, tax, seizure, death", Overlay = false }
                .Step("setup", c =>
                {
                    if (!TravelTo(c, FindTown(t => t.IsSmallTown()) ?? FindTown(t => true))) return;
                    town = BagTown(c);
                    var e = EstateEvent.Get(town);
                    c.Bag["estate"] = JsonConvert.SerializeObject(e);
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Bag["growth"] = JsonConvert.SerializeObject(EstateEvent.Conf.Growth);
                    var free = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && !EstateEvent.HousesOf(x).Any() && !MapBuildPropertyEvent.IsTownGuardian(town, x)).ToList();
                    // a grade-1 NPC (its house stops at level 2: the next one needs grade 2) and another one
                    npc = free.Where(x => x.GetGradeLvl() < EstateEvent.RequiredGrade(3)).FirstOrDefault() ?? free.FirstOrDefault();
                    other = free.FirstOrDefault(x => x != npc);
                    if (!c.Assert(npc != null && other != null, "two NPCs without a house")) return;
                    var saved = new Dictionary<string, object[]>();
                    foreach (var u in new[] { npc, other })
                    {
                        saved[u.GetUnitId()] = new object[] { u.GetUnitPos(), u.GetUnitMoney() };
                        u.SetUnitPos(town.GetOrigiPoint());
                        u.SetUnitMoney(int.MaxValue / 2);
                    }
                    c.Bag["units"] = saved;
                    c.Info($"{npc.GetName()} grade {npc.GetGradeLvl()}, {other.GetName()} grade {other.GetGradeLvl()}");
                })
                .Step("the rules", c =>
                {
                    if (npc == null) return;
                    var g0 = EstateEvent.Conf.Growth;
                    g0.Npc = false;
                    c.AssertEqual(null, EstateEvent.NpcHouse(npc, 1, () => 0, town), "the feature off: no house");
                    g0.Npc = true;
                    var share = g0.NpcShare;
                    g0.NpcShare = 0f;
                    c.AssertEqual(null, EstateEvent.NpcHouse(npc, 1, () => 0, town), "NpcShare 0: no plot for the NPCs");
                    g0.NpcShare = share;
                    c.AssertEqual(null, EstateEvent.NpcHouse(npc, 12, () => g0.NpcBuyChance * 12 + 0.001), "a roll above the chance of 12 months: nothing");
                    // the monthly hook of the mod (all its parts) with a sure chance
                    g0.NpcBuyChance = 1f;
                    var money = npc.GetUnitMoney();
                    EstateEvent.Instance.OnMonthlyForEachWUnit(npc);
                    var house = EstateEvent.HouseOf(town, npc);
                    c.Assert(house != null, $"the monthly hook: {npc.GetName()} bought a house");
                    c.Assert(money - npc.GetUnitMoney() <= money / 2, "at most half of its stones");
                    // one house in the world: not a second one, here or elsewhere
                    var e = EstateEvent.Get(town);
                    c.Assert(EstateEvent.NpcHouse(npc, 1, () => 0) != "buy", "no second house");
                    c.AssertEqual(1, e.Plots.Count(x => x.OwnerId == npc.GetUnitId() && x.MainIndex == null && x.Level > 0), "one house");
                    // upgrades until its grade, the room around or half of its stones stop it
                    for (var i = 0; i < EstateEvent.MAX_LEVEL; i++)
                        if (EstateEvent.NpcHouse(npc, 1, () => 0) == null) break;
                    var next = System.Math.Min(EstateEvent.MAX_LEVEL, house.Level + 1);
                    var (add, _, price) = EstateEvent.UpgradeCost(town, house);
                    var why = house.Level >= EstateEvent.MAX_LEVEL ? "the highest level" : npc.GetGradeLvl() < EstateEvent.RequiredGrade(next) ? $"grade {npc.GetGradeLvl()} < {EstateEvent.RequiredGrade(next)}"
                        : add == null ? "no room around" : price > npc.GetUnitMoney() / 2 ? "more than half of its stones" : null;
                    c.Assert(why != null, $"stops at a {EstateEvent.HouseName(house.Level)} (level {house.Level}): {why ?? "no reason"}");
                    c.AssertEqual(EstateEvent.FootprintSize(house.Level), EstateEvent.PartsOf(e, house).Count, "on the plots of its level");
                    other.SetUnitMoney(1000);
                    c.AssertEqual(null, EstateEvent.NpcHouse(other, 1, () => 0, town), $"{other.GetName()} with 1,000 stones: no house");
                    other.SetUnitMoney(int.MaxValue / 2);
                    c.AssertEqual("buy", EstateEvent.NpcHouse(other, 1, () => 0, town), $"{other.GetName()} buys one");
                    EstateEvent.OpenPlots(town);
                }, 1500)
                .Step("house tax", c =>
                {
                    if (npc == null) return;
                    CloseAll(UIType.TextInfoLong);
                    var e = EstateEvent.Get(town);
                    var house = EstateEvent.HouseOf(town, npc);
                    var tax = EstateEvent.HouseTax(town, house);
                    var money = npc.GetUnitMoney();
                    var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    // every house of the town pays (the other NPC's too)
                    var taxes = e.Plots.Where(x => x.OwnerId != null && x.Level > 0 && x.MainIndex == null).Sum(x => EstateEvent.HouseTax(town, x));
                    // the palace and the wall at their top: an NPC master does not spend the budget this month
                    int palace = e.PalaceLevel, wall = e.WallLevel;
                    e.PalaceLevel = EstateEvent.PALACE_MAX; e.WallLevel = EstateEvent.Conf.Map.WallMax;
                    EstateEvent.MonthTown(town, 1, () => 0.99);
                    e.PalaceLevel = palace; e.WallLevel = wall;
                    c.AssertEqual(money - tax, (long)npc.GetUnitMoney(), $"{npc.GetName()} paid the house tax {tax}");
                    c.AssertEqual(budget + taxes + e.LastMortalTax, MapBuildPropertyEvent.GetBuildProperty(town), $"to the town budget (house taxes {taxes} + the mortal tax {e.LastMortalTax})");
                    // no money: unpaid months, then the house taken back (the mortals move in)
                    npc.SetUnitMoney(0);
                    for (var m = 1; m < EstateEvent.MaxUnpaid; m++) EstateEvent.MonthTown(town, 1, () => 0.99);
                    c.AssertEqual(EstateEvent.MaxUnpaid - 1, house.Unpaid, "unpaid months counted");
                    c.Assert(house.OwnerId == npc.GetUnitId(), "still its house");
                    EstateEvent.MonthTown(town, 1, () => 0.99);
                    c.Assert(house.OwnerId == null && house.Level == EstateEvent.DistrictOf(house.District).MortalLevel, $"{EstateEvent.MaxUnpaid} months unpaid: the house taken back, the mortals move in");
                    c.Assert(!e.Plots.Any(x => x.OwnerId == npc.GetUnitId()), "all its plots");
                })
                .Step("death", c =>
                {
                    if (other == null) return;
                    var e = EstateEvent.Get(town);
                    var house = EstateEvent.HouseOf(town, other);
                    if (!c.Assert(house != null, "the other NPC's house")) return;
                    other.Die();
                    EstateEvent.MonthTown(town, 1, () => 0.99);
                    c.Assert(house.OwnerId == null && house.Level > 0, "the house of a dead cultivator goes to the mortals");
                })
                .Step("mortals over months", c =>
                {
                    if (town == null) return;
                    var e = EstateEvent.Get(town);
                    e.Population = EstateEvent.BasePopulation(town) * 2;
                    var g0 = EstateEvent.Conf.Growth;
                    var grown = EstateEvent.GrownLevels(e);
                    // 24 months, the chance hit every second month: 12 levels at most
                    var k = 0;
                    EstateEvent.GrowMortals(town, e, 24, () => k++ % 2 == 0 ? 0 : 0.99);
                    var now = EstateEvent.GrownLevels(e);
                    c.Assert(now > grown && now - grown <= 12, $"grown levels {grown} -> {now} (one level a hit)");
                    c.Assert(g0.MortalChance > 0 && g0.MortalChance < 0.2, $"the monthly chance stays small ({g0.MortalChance})");
                    EstateEvent.OpenPlots(town);
                }, 1500)
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfoLong, UIType.CheckPopup);
                    if (town == null) return;
                    var d = JsonConvert.DeserializeObject<EstateConfig._Growth>((string)c.Bag["growth"]);
                    var g0 = EstateEvent.Conf.Growth;
                    g0.Npc = d.Npc; g0.NpcShare = d.NpcShare; g0.NpcBuyChance = d.NpcBuyChance;
                    EstateEvent.Instance.Towns[town.buildData.id] = JsonConvert.DeserializeObject<EstateEvent.TownEstate>((string)c.Bag["estate"]);
                    MapBuildPropertyEvent.AddBuildProperty(town, (long)c.Bag["budget"] - MapBuildPropertyEvent.GetBuildProperty(town));
                    if (c.Bag.ContainsKey("units"))
                        foreach (var kv in (Dictionary<string, object[]>)c.Bag["units"])
                        {
                            var u = FamilyEvent.Unit(kv.Key);
                            if (u == null || u.isDie) continue;
                            u.SetUnitPos((UnityEngine.Vector2Int)kv.Value[0]);
                            u.SetUnitMoney((int)kv.Value[1]);
                        }
                    c.AssertEqual((long)c.Bag["budget"], MapBuildPropertyEvent.GetBuildProperty(town), "budget restored");
                    c.AssertEqual(d.NpcBuyChance, EstateEvent.Conf.Growth.NpcBuyChance, "settings restored");
                });
        }
    }
}
