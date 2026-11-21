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
    /// TG-69 (EstateEvent, SpiritNoteEvent, MapBuildBattleEvent): the player becomes master of a city; the town map shows
    /// the city wall and the "strengthen" button; the wall is strengthened from the town budget (+15 % defense a level,
    /// the rate of the defenders); a house of the city is bought with spirit banknotes, not spirit stones. Estate, budget,
    /// masters, money and banknotes restored.
    /// </summary>
    public class TG69_CityMaster : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-69";

        private static void SetNotes(int n)
        {
            var have = SpiritNoteEvent.Notes(Player);
            if (have > n) Player.RemoveUnitProp(SpiritNoteEvent.NOTE_ID, have - n);
            else if (have < n) Player.AddUnitProp(SpiritNoteEvent.NOTE_ID, n - have);
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            MapBuildTown city = null;
            return new Scenario { Id = "TG-69", Feature = "EstateEvent", Title = "City master: the wall, a house paid with banknotes", Overlay = false }
                .Step("a city", c =>
                {
                    if (!TravelTo(c, FindTown(t => t.IsCity()))) return;
                    city = BagTown(c);
                    if (!c.Assert(city != null && city.IsCity(), $"in a city: {city?.name}")) return;
                    var e = EstateEvent.Get(city);
                    c.Bag["estate"] = JsonConvert.SerializeObject(e);
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(city);
                    c.Bag["money"] = Player.GetUnitMoney();
                    c.Bag["notes"] = SpiritNoteEvent.Notes(Player);
                    c.Bag["masters"] = MapBuildPropertyEvent.Instance.TownMasters.TryGetValue(city.buildData.id, out var m) ? m.ToList() : null;
                    c.Bag["luck"] = Player.GetLuck(MapBuildPropertyEvent.TOWN_MASTER_LUCK_ID) != null;
                    new TrainerEvent { Parent = ModMain.ModObj }.BecomeTownMaster();
                    CloseAll(UIType.CheckPopup);
                    c.Assert(MapBuildPropertyEvent.IsTownMaster(city, Player), "the player is master of the city");
                    e.WallLevel = 1;
                    MapBuildPropertyEvent.AddBuildProperty(city, EstateEvent.WallCost(city, e) * 3);
                    EstateEvent.OpenPlots(city);
                }, 1500)
                .Step("the wall on the map", c =>
                {
                    var e = EstateEvent.Get(city);
                    var btn = FindButtons(UIType.TextInfoLong, string.Format(GameTool.LS("estate_wall_btn"), e.WallLevel, EstateEvent.WallDefensePercent(e.WallLevel))).FirstOrDefault();
                    c.Assert(btn != null, "the master's 'strengthen the wall' button");
                    c.Assert(System.Math.Abs(EstateEvent.WallDefenseRate(city) - 1.10f) < 0.0001f, $"wall level 1 (+10 %): defense x{EstateEvent.WallDefenseRate(city):0.00}");
                    var budget = MapBuildPropertyEvent.GetBuildProperty(city);
                    var cost = EstateEvent.WallCost(city, e);
                    c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.UpgradeWall(city, Player), $"wall level 2 for {cost:#,##0}");
                    c.AssertEqual(budget - cost, MapBuildPropertyEvent.GetBuildProperty(city), "paid from the town budget");
                    c.Assert(System.Math.Abs(EstateEvent.WallDefenseRate(city) - 1.15f) < 0.0001f, $"wall level 2 (+5 %): defense x{EstateEvent.WallDefenseRate(city):0.00} (simulated battles and the defenders, the player too)");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "log_estate_wall"), "Taoist log: wall");
                    EstateEvent.OpenPlots(city);
                }, 1500)
                .Step("a house paid with banknotes", c =>
                {
                    CloseAll(UIType.TextInfoLong);
                    var e = EstateEvent.Get(city);
                    foreach (var p in e.Plots.Where(x => x.OwnerId == Player.GetUnitId() && x.Level > 0).ToList()) EstateEvent.Release(e, p);
                    var plot = e.Plots.Where(x => x.OwnerId == null).OrderByDescending(x => x.Prosperity).First();
                    plot.Level = 0;
                    var cost = EstateEvent.Payable(EstateEvent.LandPrice(city, plot) + EstateEvent.BuildCost(city, 1));
                    // stones: less than the price; banknotes: enough
                    var notes = (int)(cost / SpiritNoteEvent.NoteValue) + 1;
                    Player.SetUnitMoney(100);
                    SetNotes(notes);
                    c.AssertEqual(EstateEvent.BuildResult.Ok, EstateEvent.Buy(city, plot, Player), $"a house of {cost:#,##0} bought with 100 stones and {notes} banknotes");
                    c.AssertEqual(notes - (int)SpiritNoteEvent.NotesFor(cost), SpiritNoteEvent.Notes(Player), $"{SpiritNoteEvent.NotesFor(cost)} banknotes used");
                    c.AssertEqual(100, Player.GetUnitMoney(), "no spirit stone used");
                    EstateEvent.OpenPlots(city);
                }, 1500)
                .Step("restore", c =>
                {
                    CloseAll(UIType.TextInfoLong);
                    if (city == null || !c.Bag.ContainsKey("estate")) return;
                    EstateEvent.Instance.Towns[city.buildData.id] = JsonConvert.DeserializeObject<EstateEvent.TownEstate>((string)c.Bag["estate"]);
                    MapBuildPropertyEvent.AddBuildProperty(city, (long)c.Bag["budget"] - MapBuildPropertyEvent.GetBuildProperty(city));
                    if (c.Bag["masters"] is List<string> masters) MapBuildPropertyEvent.Instance.TownMasters[city.buildData.id] = masters;
                    else MapBuildPropertyEvent.Instance.TownMasters.Remove(city.buildData.id);
                    if (!(bool)c.Bag["luck"]) Player.DelLuck(MapBuildPropertyEvent.TOWN_MASTER_LUCK_ID);
                    Player.SetUnitMoney((int)c.Bag["money"]);
                    SetNotes((int)c.Bag["notes"]);
                    c.AssertEqual((long)c.Bag["budget"], MapBuildPropertyEvent.GetBuildProperty(city), "budget restored");
                });
        }
    }
}
