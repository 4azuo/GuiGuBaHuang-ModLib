using ModLib.Helper;
using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-67 (EstateEvent maps): a town of every landscape (plain, river, lake, woods and their mixes; the road layouts and
    /// the palace place vary with them) is drawn: water with bridges, woods with trees, the
    /// wall and its gates, people on the main roads, names of the districts; a screenshot a terrain. Then a town grown
    /// into a city keeps its map in the middle of a bigger one with the player's house. Estate and town size restored.
    /// </summary>
    public class TG67_TownMapStyles : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-67";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var s = new Scenario { Id = "TG-67", Feature = "EstateEvent", Title = "Town maps of every terrain, a town grown into a city", Overlay = false };
            s.Step("maps of the world", c =>
            {
                var styles = ModMain.Store.Towns.Select(t => EstateEvent.Get(t)?.Style ?? -1).Where(x => x >= 0).ToList();
                c.Info($"{styles.Count} towns, {styles.Distinct().Count()} styles: " + string.Join(", ", styles.GroupBy(x => x).Select(gr => $"{EstateEvent.TerrainOf(gr.Key)}/{EstateEvent.RoadsOf(gr.Key)} x{gr.Count()}")));
                c.Assert(styles.Distinct().Count() >= System.Math.Min(10, styles.Count / 2), "the towns have different maps");
            });
            for (var i = 0; i < EstateEvent.TERRAIN_COUNT; i++)
            {
                var terrain = (EstateEvent.Terrain)i;
                s.Step($"{terrain} town", c =>
                {
                    CloseAll(UIType.TextInfoLong);
                    var town = ModMain.Store.Towns.FirstOrDefault(t => EstateEvent.TerrainOf(EstateEvent.Get(t)?.Style ?? 0) == terrain);
                    if (town == null) { c.Skip($"no {terrain} town in this world"); return; }
                    var e = EstateEvent.Get(town);
                    var cells = string.Concat(e.Map);
                    int water = cells.Count(x => x == EstateEvent.WATER), bridges = cells.Count(x => x == EstateEvent.BRIDGE), woods = cells.Count(x => x == EstateEvent.WOODS);
                    if (terrain.HasFlag(EstateEvent.Terrain.River) || terrain.HasFlag(EstateEvent.Terrain.Lake))
                        c.Assert(water > 5, $"{town.name}: water ({water} cells, {bridges} bridges)");
                    if (terrain.HasFlag(EstateEvent.Terrain.Woods))
                        c.Assert(woods > 5, $"{town.name}: woods outside the town ({woods} cells)");
                    EstateEvent.OpenPlots(town);
                    var texts = ScreenTexts();
                    c.Assert(texts.Any(x => x.StartsWith(EstateEvent.StyleName(e))), $"the style is named: {EstateEvent.StyleName(e)}");
                    var named = "MNRCO".Count(d => texts.Contains(EstateEvent.DistrictName(d.ToString())));
                    c.Assert(named >= 4, $"district names on the map: {named} of 5");
                }, 1500);
            }
            MapBuildTown grown = null;
            s.Step("a town grown into a city", c =>
            {
                CloseAll(UIType.TextInfoLong);
                grown = ModMain.Store.Towns.FirstOrDefault(t => t.IsSmallTown());
                if (grown == null) { c.Skip("no small town"); return; }
                var e = EstateEvent.Get(grown);
                c.Bag["estate"] = JsonConvert.SerializeObject(e);
                var house = e.Plots.Where(x => x.OwnerId == null).OrderByDescending(x => EstateEvent.MaxLevelAt(e, x)).First();
                house.OwnerId = Player.GetUnitId(); house.Level = 3; house.Rooms[1] = 2;
                var old = (rows: e.Map.Length, x: house.X, y: house.Y, plots: e.Plots.Count, map: (string[])e.Map.Clone());
                grown.buildTownData.isMainTown = true;
                var city = EstateEvent.Get(grown);
                if (!c.Assert(grown.IsCity() && city.Map.Length > old.rows, $"{grown.name}: a bigger map ({old.rows} -> {city.Map.Length} rows)")) return;
                int ox = (city.Map[0].Length - old.map[0].Length) / 2, oy = (city.Map.Length - old.rows) / 2;
                c.Assert(Enumerable.Range(0, old.rows).All(y => Enumerable.Range(0, old.map[0].Length).All(x => old.map[y][x] == EstateEvent.WOODS || city.Map[y + oy][x + ox] == old.map[y][x])), "the old map unchanged in the middle (its woods cleared)");
                var moved = EstateEvent.HouseOf(grown, Player);
                c.Assert(moved != null && moved.X == old.x + ox && moved.Y == old.y + oy && moved.Level == 3 && moved.Rooms[1] == 2, "the player's house kept, with its rooms");
                c.Assert(city.Plots.Count > old.plots, $"more plots: {old.plots} -> {city.Plots.Count}");
                EstateEvent.OpenPlots(grown);
            }, 1500);
            s.Step("restore", c =>
            {
                CloseAll(UIType.TextInfoLong);
                if (grown != null && c.Bag.ContainsKey("estate"))
                {
                    grown.buildTownData.isMainTown = false;
                    EstateEvent.Instance.Towns[grown.buildData.id] = JsonConvert.DeserializeObject<EstateEvent.TownEstate>((string)c.Bag["estate"]);
                    c.Assert(!grown.IsCity(), "the town is a town again");
                }
            });
            return s;
        }
    }
}
