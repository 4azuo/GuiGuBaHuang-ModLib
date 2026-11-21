using ModLib.Helper;
using MOD_nE7UL2.Enum;
using MOD_nE7UL2.Mod;
using System.Collections.Generic;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-40 (MapBuildPropertyEvent/RealMarketEvent2/BuildingArrangeEvent, with the trainer): the player becomes town
    /// master (trainer action), the master gets the shop discount, the town management window opens with the buildings
    /// of the town (the game's picture of each, its cost, built / "build" / not in this town; one is taken down for the
    /// test so a "build" button shows). Masters and the building restored.
    /// </summary>
    public class TG40_TownMaster : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-40";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TG-40", Feature = "MapBuildPropertyEvent/RealMarketEvent2", Title = "Become town master (trainer) and manage the town", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => Has(t, MapBuildSubType.TownMarketPill))))
                .Step("become master", c =>
                {
                    var town = BagTown(c);
                    c.Bag["masters"] = MapBuildPropertyEvent.Instance.TownMasters.TryGetValue(town.buildData.id, out var m) ? m.ToList() : null;
                    c.Bag["luck"] = Player.GetLuck(MapBuildPropertyEvent.TOWN_MASTER_LUCK_ID) != null;
                    c.Bag["rate"] = RealMarketEvent2.GetBuyRate(town, Player);
                    new TrainerEvent { Parent = ModMain.ModObj }.BecomeTownMaster();
                    c.Assert(MapBuildPropertyEvent.IsTownMaster(town, Player), $"player is master of {town.name}");
                    c.Assert(MapBuildPropertyEvent.IsTownGuardian(town, Player), "masters are guardians");
                    var rate = RealMarketEvent2.GetBuyRate(town, Player);
                    c.AssertEqual((float)c.Bag["rate"] + RealMarketEvent2.TownMReduceBuyRate, rate, $"master shop rate {c.Bag["rate"]:0.00}% -> {rate:0.00}%");
                    // a building taken down (put back at the end): its "build" button shows in the window
                    var removed = BuildingCostEnum.GetAllEnums<BuildingCostEnum>().FirstOrDefault(x => !x.IsSchool && BuildingArrangeEvent.IsDestroyable(town, x));
                    c.Bag["removed"] = removed;
                    if (removed != null) BuildingArrangeEvent.Destroy(town, removed);
                    c.Info(removed == null ? "no building to take down" : $"taken down for the test: {GameTool.LS(removed.BuildingName)}");
                })
                .Step("manage town", c => MapBuildPropertyEvent.OpenUITownManage(BagTown(c)))
                .Step("management window", c =>
                {
                    var town = BagTown(c);
                    // the management window is the NPC search window (town guardians) with the Taoist cover (tax, budget, buildings)
                    c.Assert(g.ui.HasUI(UIType.NPCSearch) && MapBuildPropertyEvent.isShowManageTeamUI1, "town management window opened");
                    c.Info($"budget {MapBuildPropertyEvent.GetBuildProperty(town):#,##0}, guardians {MapBuildPropertyEvent.GetTownGuardians(town).Count}, salaries {MapBuildPropertyEvent.GetTotalMonthlyPayment(town)}/month");
                    var removed = c.Bag.ContainsKey("removed") ? c.Bag["removed"] as BuildingCostEnum : null;
                    var buildings = BuildingCostEnum.GetAllEnums<BuildingCostEnum>().Where(x => !x.IsSchool && x.IsMatchBuildConds(town) && !BuildingArrangeEvent.IsIgnored(x)).ToList();
                    foreach (var em in buildings)
                    {
                        var state = BuildingArrangeEvent.IsBuilt(town, em) ? "built" : BuildingArrangeEvent.IsBuildable(town, em) ? "can be built" : "not in this town";
                        c.Info($"{GameTool.LS(em.BuildingName)}: {BuildingArrangeEvent.GetBuildingCost(town, em):#,##0} from the budget, {state}, picture {string.Join("/", BuildingArrangeEvent.Sprite(em.BuildType) ?? new[] { "-" })}");
                    }
                    c.Assert(buildings.All(x => BuildingArrangeEvent.Sprite(x.BuildType) != null), $"a picture for each of the {buildings.Count} buildings");
                    if (removed != null)
                        c.Assert(FindButtons(UIType.NPCSearch, GameTool.LS("other500020017")).Any(), "a 'build' button for the building taken down");
                }, 1500)
                .Step("restore", c =>
                {
                    CloseAll(UIType.NPCSearch, UIType.TextInfoLong, UIType.TextInfo);
                    var town = BagTown(c);
                    var removed = c.Bag.ContainsKey("removed") ? c.Bag["removed"] as BuildingCostEnum : null;
                    if (removed != null && !BuildingArrangeEvent.IsBuilt(town, removed)) removed.Build(town);
                    if (c.Bag.ContainsKey("masters"))
                    {
                        if (c.Bag["masters"] is List<string> masters) MapBuildPropertyEvent.Instance.TownMasters[town.buildData.id] = masters;
                        else MapBuildPropertyEvent.Instance.TownMasters.Remove(town.buildData.id);
                        if (!(bool)c.Bag["luck"]) Player.DelLuck(MapBuildPropertyEvent.TOWN_MASTER_LUCK_ID);
                    }
                    if (removed != null) c.Assert(BuildingArrangeEvent.IsBuilt(town, removed), "the building is back");
                });
        }
    }
}
