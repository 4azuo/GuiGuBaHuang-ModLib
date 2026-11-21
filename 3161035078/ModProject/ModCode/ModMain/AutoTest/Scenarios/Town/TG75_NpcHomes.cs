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
    /// TG-75 (EstateEvent, where NPCs live): (a) an NPC buys in a town of the area where it is, a city first from
    /// NpcCityGrade, a small town first below; (b) the family lives together: the spouse and an unmarried child of a house
    /// owner buy nothing, a married child moves out in the same town; (c) an NPC away from home goes back to its town (its
    /// own house or its family's), a hired one does not. Changes the save (a marriage: restored by run-autotest.ps1);
    /// estates, money and places restored.
    /// </summary>
    public class TG75_NpcHomes : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-75";

        private static void Save(AutoTestContext c, params WorldUnitBase[] units)
        {
            var saved = c.Bag.ContainsKey("units") ? (Dictionary<string, object[]>)c.Bag["units"] : new Dictionary<string, object[]>();
            foreach (var u in units.Where(x => x != null && !saved.ContainsKey(x.GetUnitId())))
                saved[u.GetUnitId()] = new object[] { u.GetUnitPos(), u.GetUnitMoney() };
            c.Bag["units"] = saved;
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase head = null, spouse = null, child = null;
            return new Scenario { Id = "TG-75", Feature = "EstateEvent", Title = "Where NPCs live: town of the area, family house, going home", Overlay = false }
                .Step("a: the town of the area", c =>
                {
                    // an area with a city and a small town
                    var city = ModMain.Store.Towns.FirstOrDefault(x => x.IsCity() && ModMain.Store.Towns.Any(y => !y.IsCity() && y.gridData.areaBaseID == x.gridData.areaBaseID));
                    if (!c.Assert(city != null, "an area with a city and a small town")) return;
                    var small = ModMain.Store.Towns.First(x => !x.IsCity() && x.gridData.areaBaseID == city.gridData.areaBaseID);
                    c.Bag["estates"] = JsonConvert.SerializeObject(EstateEvent.Instance.Towns);
                    var cut = EstateEvent.Conf.Growth.NpcCityGrade;
                    var free = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && EstateEvent.HomeOf(x).town == null && EstateEvent.FamilyHome(x).town == null).ToList();
                    var strong = free.FirstOrDefault(x => x.GetGradeLvl() >= cut);
                    var weak = free.FirstOrDefault(x => x.GetGradeLvl() < cut);
                    if (!c.Assert(strong != null && weak != null, $"NPCs of grade {cut}+ and below")) return;
                    Save(c, strong, weak);
                    foreach (var u in new[] { strong, weak })
                    {
                        // standing in the small town of the area
                        u.SetUnitPos(small.GetOrigiPoint());
                        u.SetUnitMoney(int.MaxValue / 2);
                        var choice = EstateEvent.ChooseTowns(u);
                        c.Assert(choice.All(x => x.gridData.areaBaseID == small.gridData.areaBaseID), $"{u.GetName()}: only the towns of area {small.gridData.areaBaseID} ({choice.Count})");
                        var wantCity = u.GetGradeLvl() >= cut;
                        c.AssertEqual(wantCity, choice.First().IsCity(), $"grade {u.GetGradeLvl()}: {(wantCity ? "a city" : "a small town")} first ({choice.First().name})");
                        c.AssertEqual("buy", EstateEvent.NpcHouse(u, 1, () => 0), "buys");
                        var home = EstateEvent.HomeOf(u).town;
                        c.Assert(home != null && home.gridData.areaBaseID == small.gridData.areaBaseID && home.IsCity() == wantCity, $"its house in {home?.name}");
                    }
                })
                .Step("b: the family lives together", c =>
                {
                    // a married NPC with a grown child: the head buys, the others live with it
                    head = Npcs.FirstOrDefault(x => !HirePeopleEvent.IsHired(x) && EstateEvent.HomeOf(x).town == null && EstateEvent.FamilyHome(x).town == null
                        && FamilyEvent.Unit(FamilyEvent.Spouse(x)) is WorldUnitBase s && !s.isDie && EstateEvent.HomeOf(s).town == null
                        && FamilyEvent.Family(x).Children.Select(FamilyEvent.Unit).Any(k => k != null && !k.isDie && !k.IsPlayer() && EstateEvent.HomeOf(k).town == null));
                    if (!c.Assert(head != null, "a married NPC with a grown child")) return;
                    spouse = FamilyEvent.Unit(FamilyEvent.Spouse(head));
                    child = FamilyEvent.Family(head).Children.Select(FamilyEvent.Unit).First(k => k != null && !k.isDie && !k.IsPlayer() && EstateEvent.HomeOf(k).town == null);
                    Save(c, head, spouse, child);
                    foreach (var u in new[] { head, spouse, child }) u.SetUnitMoney(int.MaxValue / 2);
                    var town = BagTownOr(c);
                    c.AssertEqual("buy", EstateEvent.NpcHouse(head, 1, () => 0, town), $"{head.GetName()} buys in {town.name}");
                    c.Assert(EstateEvent.FamilyHome(spouse).town?.buildData.id == town.buildData.id, "the spouse's family house");
                    c.AssertEqual(null, EstateEvent.NpcHouse(spouse, 1, () => 0), $"{spouse.GetName()} (spouse) shares the house");
                    if (!string.IsNullOrEmpty(FamilyEvent.Spouse(child)))
                        c.Info($"{child.GetName()} is already married");
                    else
                    {
                        c.AssertEqual(null, EstateEvent.NpcHouse(child, 1, () => 0), $"{child.GetName()} (unmarried child) lives with the family");
                        var mate = Npcs.FirstOrDefault(x => string.IsNullOrEmpty(FamilyEvent.Spouse(x)) && !HirePeopleEvent.IsHired(x) && FamilyEvent.IsMan(x) != FamilyEvent.IsMan(child)
                            && EstateEvent.HomeOf(x).town == null && x != head && x != spouse);
                        if (mate == null) { c.Info("nobody to marry the child"); return; }
                        FamilyEvent.SetMarried(child, mate);
                    }
                    // a married child moves out, in the same town (wherever it stands)
                    child.SetUnitPos(ModMain.Store.Towns.First(x => x.buildData.id != town.buildData.id).GetOrigiPoint());
                    c.AssertEqual("buy", EstateEvent.NpcHouse(child, 1, () => 0), $"{child.GetName()} (married) moves out");
                    c.AssertEqual(town.buildData.id, EstateEvent.HomeOf(child).town?.buildData.id, $"in the family's town ({EstateEvent.HomeOf(child).town?.name})");
                    EstateEvent.OpenPlots(town);
                }, 1500)
                .Step("c: going home", c =>
                {
                    CloseAll(UIType.TextInfoLong);
                    if (head == null) return;
                    var home = EstateEvent.HomeOf(head).town;
                    var away = ModMain.Store.Towns.First(x => x.buildData.id != home.buildData.id);
                    head.SetUnitPos(away.GetOrigiPoint());
                    spouse.SetUnitPos(away.GetOrigiPoint());
                    c.AssertEqual(null, EstateEvent.NpcHome(head, 1, () => 0.99), "no chance hit: it stays");
                    c.AssertEqual(home.buildData.id, EstateEvent.NpcHome(head, 1, () => 0)?.buildData.id, $"{head.GetName()} goes back to {home.name}");
                    c.AssertEqual(home.buildData.id, head.GetMapBuild<MapBuildTown>()?.buildData.id, "now in its town");
                    c.AssertEqual(home.buildData.id, EstateEvent.NpcHome(spouse, 1, () => 0)?.buildData.id, "the spouse goes back to the family house");
                    c.AssertEqual(null, EstateEvent.NpcHome(head, 1, () => 0), "already home: stays");
                    var monthly = 1 - System.Math.Pow(1 - EstateEvent.Conf.Growth.NpcHomeChance, 12);
                    c.Info($"chance to go home in a year: {monthly:P0}");
                })
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfoLong);
                    if (c.Bag.ContainsKey("estates"))
                    {
                        EstateEvent.Instance.Towns = JsonConvert.DeserializeObject<Dictionary<string, EstateEvent.TownEstate>>((string)c.Bag["estates"]);
                        EstateEvent.Touch();
                    }
                    if (c.Bag.ContainsKey("units"))
                        foreach (var kv in (Dictionary<string, object[]>)c.Bag["units"])
                        {
                            var u = FamilyEvent.Unit(kv.Key);
                            if (u == null || u.isDie) continue;
                            u.SetUnitPos((UnityEngine.Vector2Int)kv.Value[0]);
                            u.SetUnitMoney((int)kv.Value[1]);
                        }
                    c.Assert(head == null || EstateEvent.HomeOf(head).town == null, "estates restored");
                });
        }

        /// <summary>The town of the test: the player's (traveled before) or the first small town.</summary>
        private static MapBuildTown BagTownOr(AutoTestContext c) =>
            g.world.playerUnit.GetMapBuild<MapBuildTown>() ?? ModMain.Store.Towns.First(x => x.IsSmallTown());
    }
}
