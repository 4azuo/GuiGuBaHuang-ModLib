using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-64 (EstateEvent rooms, B2, and the palace): the player's house opens its interior from the town map; rooms are
    /// built in the slots of the house level and raised; the quiet room, the training ground, the herb garden, the spirit
    /// gathering array and the ancestral hall work; the town master expands the palace from the town budget.
    /// Estate of the town, budget, money, exp and body data restored.
    /// </summary>
    public class TG64_HouseRooms : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-64";

        private static void SetNotes(int n)
        {
            var have = SpiritNoteEvent.Notes(Player);
            if (have > n) Player.RemoveUnitProp(SpiritNoteEvent.NOTE_ID, have - n);
            else if (have < n) Player.AddUnitProp(SpiritNoteEvent.NOTE_ID, n - have);
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildTown town = null;
            EstateEvent.Plot house = null;
            return new Scenario { Id = "TG-64", Feature = "EstateEvent", Title = "House rooms and the town master's palace", Overlay = false }
                .Step("a house", c =>
                {
                    if (!TravelTo(c, FindTown(t => t.IsSmallTown()) ?? FindTown(t => true))) return;
                    town = BagTown(c);
                    var e = EstateEvent.Get(town);
                    c.Bag["estate"] = JsonConvert.SerializeObject(e);
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(town);
                    c.Bag["money"] = Player.GetUnitMoney();
                    c.Bag["notes"] = SpiritNoteEvent.Notes(Player);
                    SetNotes(100000);
                    c.Bag["exp"] = Player.GetExp();
                    var body = BodyCulEvent.Get(Player);
                    c.Bag["body"] = body == null ? null : JsonConvert.SerializeObject(body);
                    // a house on a plot with room for an immortal palace (3 x 2 plots)
                    foreach (var p in e.Plots.Where(x => x.OwnerId == Player.GetUnitId() && x.Level > 0).ToList())
                        EstateEvent.Release(e, p);
                    house = e.Plots.Where(x => x.OwnerId == null).OrderByDescending(x => EstateEvent.MaxLevelAt(e, x)).ThenByDescending(x => x.Prosperity).First();
                    c.AssertEqual(EstateEvent.MAX_LEVEL, EstateEvent.MaxLevelAt(e, house), $"a plot with room for an immortal palace ({EstateEvent.StyleName(e)})");
                    house.Level = 0;
                    Player.SetUnitMoney(int.MaxValue / 2);
                    c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.Buy(town, house, Player), "a house bought");
                    house.Level = 2;
                    // a bigger house takes a plot around it: its land (and the mortals' house) in the cost
                    var (add, land, total) = EstateEvent.UpgradeCost(town, house);
                    c.Assert(add != null && add.Count == 1, $"a courtyard house takes 1 more plot ({add?.Count})");
                    c.AssertEqual(EstateEvent.BuildCost(town, 3) + land, total, $"cost: building + land {land:#,##0}");
                    if (Player.GetGradeLvl() >= EstateEvent.RequiredGrade(3))
                    {
                        c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.Upgrade(town, house, Player), "courtyard house");
                        c.AssertEqual(2, EstateEvent.PartsOf(e, house).Count, "on 2 plots");
                        EstateEvent.Release(e, house);
                        house.OwnerId = Player.GetUnitId();
                    }
                    else
                        c.Info($"grade {Player.GetGradeLvl()}: the upgrade itself is not run");
                    house.Level = 2;
                    house.Rooms.Clear();
                    house.Herbs.Clear();
                    Player.SetUnitMoney(int.MaxValue / 2);
                    EstateEvent.OpenHouse(town, house);
                }, 1500)
                .Step("interior", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "the house opened");
                    var texts = ScreenTexts();
                    c.Assert(texts.Any(x => x == string.Format(GameTool.LS("estate_house_rooms"), 0, 2)), "2 empty rooms for a tiled house");
                    c.AssertEqual(2, FindButtons(UIType.TextInfoLong, GameTool.LS("estate_room_build")).Count, "a 'Build a room' button per empty room");
                    var build = FindButtons(UIType.TextInfoLong, GameTool.LS("estate_room_build")).First();
                    build.Item.onClick.Invoke();
                }, 1500)
                .Step("build rooms", c =>
                {
                    c.Assert(ScreenTexts().Any(x => x.StartsWith(EstateEvent.RoomName(EstateEvent.RoomKind.Quiet))), "the room choice lists the quiet room");
                    c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.BuildRoom(town, house, EstateEvent.RoomKind.Quiet, Player), "quiet room");
                    c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.BuildRoom(town, house, EstateEvent.RoomKind.Garden, Player), "herb garden");
                    c.AssertEqual(EstateEvent.BuildResult.NoSlot, EstateEvent.BuildRoom(town, house, EstateEvent.RoomKind.Array, Player), "no third room in a tiled house");
                    c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.BuildRoom(town, house, EstateEvent.RoomKind.Quiet, Player), "quiet room level 2");
                    c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.BuildRoom(town, house, EstateEvent.RoomKind.Quiet, Player), "quiet room level 3");
                    c.AssertEqual(EstateEvent.BuildResult.MaxLevel, EstateEvent.BuildRoom(town, house, EstateEvent.RoomKind.Quiet, Player), "level 3 is the highest");
                    // an immortal palace on its 6 plots (drawn over them on the town map, see the last steps)
                    var e = EstateEvent.Get(town);
                    foreach (var a in EstateEvent.Expansion(e, house, EstateEvent.MAX_LEVEL, _ => 0)) EstateEvent.Take(a, house);
                    house.Level = EstateEvent.MAX_LEVEL;
                    c.AssertEqual(EstateEvent.FootprintSize(EstateEvent.MAX_LEVEL), EstateEvent.PartsOf(e, house).Count, "an immortal palace on 6 plots");
                    foreach (var k in new[] { EstateEvent.RoomKind.Training, EstateEvent.RoomKind.Array, EstateEvent.RoomKind.Hall })
                        c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.BuildRoom(town, house, k, Player), $"{EstateEvent.RoomName(k)} in a manor");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_estate_room"), "Taoist log: room built");
                })
                .Step("room effects", c =>
                {
                    // quiet room level 3: 6 % of the phase a month in the town
                    var phase = Player.GetMaxExpCurrentPhase() - Player.GetMinExpCurrentPhase();
                    Player.SetProperty<int>(UnitPropertyEnum.Exp, Player.GetMinExpCurrentPhase());
                    var exp = Player.GetExp();
                    var body = BodyCulEvent.Get(Player) ?? BodyCulEvent.Start(Player);
                    var bodyExp = body.Exp;
                    var bodyLevel = body.Level;
                    EstateEvent.HomeRooms(house, Player, 1);
                    var gain = Player.GetExp() - exp;
                    c.Assert(gain > 0 && gain <= EstateEvent.QuietExp(phase, 3, 1), $"quiet room: +{gain} exp (at most {EstateEvent.QuietExp(phase, 3, 1)}, body refining absorbs a part)");
                    c.Assert(body.Level > bodyLevel || body.Exp > bodyExp, $"training ground: body exp {bodyExp} -> {body.Exp} (level {bodyLevel} -> {body.Level})");
                    // herb garden
                    EstateEvent.Garden(house, Player, 2);
                    c.AssertEqual(2, house.Herbs.Count, "herb garden level 1: 2 herbs in 2 months");
                    c.AssertEqual(2, EstateEvent.CollectHerbs(house, Player), "herbs collected to the bag");
                    c.AssertEqual(0, house.Herbs.Count, "the garden is empty");
                    // array and hall
                    c.AssertEqual(EstateEvent.QiRateOfArray(1), EstateEvent.QiRate(Player), "spirit gathering array: Qi +15 % in the town");
                    c.AssertEqual(EstateEvent.UpkeepRateOfHall(1), EstateEvent.UpkeepRate(), "ancestral hall: upkeep -10 %");
                })
                .Step("house window", c =>
                {
                    EstateEvent.OpenHouse(town, house);
                    var texts = ScreenTexts();
                    c.Assert(texts.Any(x => x.StartsWith(EstateEvent.RoomName(EstateEvent.RoomKind.Quiet) + " 3/3")), "the quiet room 3/3 is listed");
                    c.Assert(texts.Any(x => x.StartsWith(EstateEvent.RoomName(EstateEvent.RoomKind.Hall) + " 1/3")), "the ancestral hall 1/3 is listed");
                }, 1500)
                .Step("palace", c =>
                {
                    CloseAll(UIType.TextInfoLong);
                    if (!MapBuildPropertyEvent.IsTownMaster(town, Player))
                        new TrainerEvent { Parent = ModMain.ModObj }.BecomeTownMaster();
                    if (!c.Assert(MapBuildPropertyEvent.IsTownMaster(town, Player), $"player is master of {town.name}")) return;
                    var e = EstateEvent.Get(town);
                    var level = e.PalaceLevel;
                    if (level >= EstateEvent.PALACE_MAX) { c.Info("palace already at the top"); return; }
                    var cost = EstateEvent.PalaceCost(town, e);
                    // the target of the mortals at the budget of the town (the budget itself attracts mortals)
                    var budget = MapBuildPropertyEvent.GetBuildProperty(town);
                    var target = EstateEvent.Target(town, e);
                    MapBuildPropertyEvent.AddBuildProperty(town, cost - budget - 1);
                    c.AssertEqual(EstateEvent.BuildResult.NoMoney, EstateEvent.UpgradePalace(town, Player), "the budget cannot pay");
                    MapBuildPropertyEvent.AddBuildProperty(town, 1000);
                    c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.UpgradePalace(town, Player), $"palace expanded for {cost:#,##0}");
                    c.AssertEqual(level + 1, e.PalaceLevel, "palace level");
                    c.AssertEqual(999L, MapBuildPropertyEvent.GetBuildProperty(town), "paid from the town budget");
                    MapBuildPropertyEvent.AddBuildProperty(town, budget - MapBuildPropertyEvent.GetBuildProperty(town));
                    c.Assert(EstateEvent.Target(town, e) > target, $"more mortals at the same budget: target {target:#,##0} -> {EstateEvent.Target(town, e):#,##0}");
                    EstateEvent.OpenPlots(town);
                }, 1500)
                .Step("town map of the master", c =>
                {
                    c.Assert(ScreenTexts().Any(x => x == string.Format(GameTool.LS("estate_palace_label"), EstateEvent.Get(town).PalaceLevel)), "the expanded palace on the map");
                }, 1000)
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfoLong);
                    EstateEvent.Instance.Towns[town.buildData.id] = JsonConvert.DeserializeObject<EstateEvent.TownEstate>((string)c.Bag["estate"]);
                    MapBuildPropertyEvent.AddBuildProperty(town, (long)c.Bag["budget"] - MapBuildPropertyEvent.GetBuildProperty(town));
                    Player.SetUnitMoney((int)c.Bag["money"]);
                    SetNotes((int)c.Bag["notes"]);
                    Player.SetProperty<int>(UnitPropertyEnum.Exp, (int)c.Bag["exp"]);
                    BodyCulEvent.Instance.Bodies.Remove(Player.GetUnitId());
                    if (c.Bag["body"] != null)
                        BodyCulEvent.Instance.Bodies[Player.GetUnitId()] = JsonConvert.DeserializeObject<BodyCulEvent.BodyData>((string)c.Bag["body"]);
                    c.AssertEqual((long)c.Bag["budget"], MapBuildPropertyEvent.GetBuildProperty(town), "budget restored");
                });
        }
    }
}
