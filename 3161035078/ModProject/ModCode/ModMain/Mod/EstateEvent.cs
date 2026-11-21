using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Town estates: the mortals of every town, its map of plots and the player's house.
    /// - Population: a base by size (town / city) and area, more with the budget and the town master's palace, fewer with
    ///   high taxes; moves 5 % a month toward its target; the mortals pay a monthly tax to the town budget.
    /// - Town map: made once per town (EstateEvent.Layout.cs: a style, winding roads, alleys, districts), drawn by
    ///   EstateEvent.Map.cs; the district sets the prosperity, the land price and the house tax. The mortals build on a
    ///   share of the plots that follows the population.
    /// - A bigger house takes more plots (1 to 3 x 2): the plots around it are bought (land, mortals bought out).
    /// - Pictures, sizes, rates and prices: ModConf/estate_configs.json (EstateEvent.Config.cs).
    /// - The player's house: a free plot bought and built (6 levels, pictures of the game's buildings), upgraded; a fixed
    ///   monthly tax by level and district (unpaid 6 months: the house is taken back). Owning a house: no entry tax in that
    ///   town and a full rest every month spent in it.
    /// - Rooms of the house (EstateEvent.Rooms.cs): one room a house level (quiet room, training ground, herb garden,
    ///   spirit gathering array, ancestral hall), 3 levels each.
    /// - The palace of the town master (5 levels, paid from the town budget): more mortals.
    /// </summary>
    [Cache(ModConst.ESTATE_EVENT)]
    public partial class EstateEvent : ModEvent
    {
        public static EstateEvent Instance { get; set; }

        // levels are fixed (names of the texts); every other number is in ModConf/estate_configs.json (EstateConfig)
        public const int MAX_LEVEL = 6;
        public const int PALACE_MAX = 5;

        public static int MaxUnpaid => Conf.MaxUnpaidMonths;
        public static int PopPerHouseLevel => Conf.Population.PerHouseLevel;
        public static float PopPerPalaceLevel => Conf.Population.PerPalaceLevel;

        public static EstateConfig._House HouseConf(int level) => Conf.Houses[Math.Max(1, Math.Min(MAX_LEVEL, level)) - 1];
        /// <summary>Plots of a house (width, height in cells): a bigger house needs the plots around it.</summary>
        public static int[] Footprint(int level) { var h = HouseConf(level); return new[] { h.Width, h.Height }; }
        public static int FootprintSize(int level) { var f = Footprint(level); return f[0] * f[1]; }

        // ------------------------------------------------------------------ town maps (EstateEvent.Layout.cs)

        public class District
        {
            public char Code { get; set; }
            public int Prosperity { get; set; }
            /// <summary>Factor of the land price and of the house tax.</summary>
            public float Price { get; set; }
            /// <summary>Level of the mortals' houses.</summary>
            public int MortalLevel { get; set; }
            /// <summary>Pictures of the mortals' houses (atlas, sprite).</summary>
            public string[][] Sprites { get; set; }
            /// <summary>Ground color r, g, b, a.</summary>
            public float[] Color { get; set; }
        }

        private static Dictionary<char, District> _districts;

        /// <summary>The districts of the config (M market, N noble, R residential, C craftsmen, O outskirts).</summary>
        public static Dictionary<char, District> DISTRICTS
        {
            get
            {
                if (_districts == null)
                    _districts = Conf.Districts.Where(kv => kv.Key.Length > 0 && "MNRCO".Contains(kv.Key[0])).ToDictionary(kv => kv.Key[0], kv => new District
                    {
                        Code = kv.Key[0],
                        Prosperity = kv.Value.Prosperity,
                        Price = kv.Value.Price,
                        MortalLevel = kv.Value.MortalLevel,
                        Sprites = kv.Value.Sprites.Select(EstateConfig.SplitSprite).Where(x => x != null).ToArray(),
                        Color = EstateConfig.ParseColor(kv.Value.Color) ?? new[] { 0.8f, 0.8f, 0.7f, 0.5f },
                    });
                return _districts;
            }
        }

        public static bool IsPlotCell(char c) => DISTRICTS.ContainsKey(c);
        public static int PlotCount(string[] map) => map.Sum(r => r.Count(IsPlotCell));
        public static District DistrictOf(string code) => DISTRICTS.TryGetValue(string.IsNullOrEmpty(code) ? 'R' : code[0], out var d) ? d : DISTRICTS['R'];
        public static string DistrictName(string code) => GameTool.LS($"estate_district_{DistrictOf(code).Code}");

        /// <summary>True when a cell of the map touches a main road.</summary>
        public static bool NearMainRoad(string[] map, int x, int y)
        {
            for (var dy = -1; dy <= 1; dy++)
                for (var dx = -1; dx <= 1; dx++)
                {
                    var yy = y + dy; var xx = x + dx;
                    if (yy >= 0 && yy < map.Length && xx >= 0 && xx < map[yy].Length && IsMainRoad(map[yy][xx]))
                        return true;
                }
            return false;
        }

        // ------------------------------------------------------------------ formulas (pure, unit tests)

        /// <summary>Base population: town 10,000 + 2,000 per area above 1 (max 30,000), city 40,000 + 6,000 (max 100,000) (config).</summary>
        public static long BasePopulation(bool city, int area)
        {
            var a = Math.Max(0, area - 1);
            var p = Conf.Population;
            return city ? Math.Min(p.CityMax, p.CityBase + p.CityPerArea * a) : Math.Min(p.TownMax, p.TownBase + p.TownPerArea * a);
        }

        /// <summary>Residents attracted by the budget: budget / base tax x 5, at most the base population.</summary>
        public static long BudgetBonus(long budget, int plainBaseTax, long basePop) =>
            Math.Min(basePop, Math.Max(0, budget) / Math.Max(1, plainBaseTax) * Conf.Population.BudgetBonus);

        /// <summary>Tax factor of the population: 1.3 - 0.3 x tax rate (0.03 - 1.15): the mortal tax is best at a rate about 2.</summary>
        public static float TaxFactor(float taxRate)
        {
            var p = Conf.Population;
            return Math.Max(p.TaxMin, Math.Min(p.TaxMax, p.TaxBase - p.TaxSlope * taxRate));
        }

        public static long TargetPopulation(long basePop, long budgetBonus, float taxRate, int houseLevels, int palaceLevel = 1, int townBuildings = 0) =>
            (long)Math.Round((basePop + budgetBonus) * (double)TaxFactor(taxRate) * (1 + PopPerPalaceLevel * (Math.Max(1, palaceLevel) - 1) + Conf.Population.PerTownBuilding * Math.Max(0, townBuildings)))
            + houseLevels * PopPerHouseLevel;

        /// <summary>Population after some months (5 % of the gap a month).</summary>
        public static long NextPopulation(long pop, long target, int months, int townBuildings = 0) =>
            pop + (long)Math.Round((target - pop) * (1 - Math.Pow(1 - MoveRate(townBuildings), Math.Max(1, months))));

        /// <summary>Monthly move of the population toward its target: faster with the sub-buildings of the town.</summary>
        public static double MoveRate(int townBuildings) =>
            Math.Min(0.5, Conf.Population.MoveRate + Conf.Population.MoveRatePerTownBuilding * Math.Max(0, townBuildings));

        public static long MortalTax(long pop, int baseTax) => pop * Math.Max(0, baseTax) / Conf.Population.MortalTaxDivisor;

        /// <summary>Prosperity (5-100) of a plot: its district, +4 next to a main road, a little noise of the town.</summary>
        public static int Prosperity(char district, bool nearMainRoad, int seed, int index)
        {
            var noise = (int)((uint)(seed * 31 + index * 7919) % 13) - 6;
            return Math.Max(5, Math.Min(100, DistrictOf(district.ToString()).Prosperity + noise + (nearMainRoad ? 4 : 0)));
        }

        /// <summary>Plots the mortals build on: plots x population / (2 x base), 15 % - 95 %.</summary>
        public static int MortalPlotsTarget(int plots, long pop, long basePop) =>
            (int)Math.Round(plots * Math.Max(0.15, Math.Min(0.95, pop / (2.0 * Math.Max(1, basePop)))));

        /// <summary>
        /// Highest level of a mortal house: its district's, +1 with a big population (at least MortalPopulation x the
        /// base), +1 on a prosperous plot (MortalProsperity), at most MortalMaxBonus above (Growth of the config).
        /// </summary>
        public static int MortalCap(int districtLevel, int prosperity, long pop, long basePop)
        {
            var g = Conf.Growth;
            var bonus = (pop >= basePop * (double)g.MortalPopulation ? 1 : 0) + (prosperity >= g.MortalProsperity ? 1 : 0);
            return Math.Max(1, Math.Min(MAX_LEVEL, districtLevel + Math.Min(g.MortalMaxBonus, bonus)));
        }

        /// <summary>Levels the mortal houses of an estate grew above their districts'.</summary>
        public static int GrownLevels(TownEstate e) =>
            e.Plots.Where(x => x.OwnerId == null && x.Level > 0).Sum(x => Math.Max(0, x.Level - DistrictOf(x.District).MortalLevel));

        /// <summary>Land: price tax x 400 x district factor x (1 + prosperity / 100).</summary>
        public static long BaseLandPrice(int plainBaseTax, char district, int prosperity) =>
            (long)Math.Round(plainBaseTax * Conf.Prices.Land * DistrictOf(district.ToString()).Price * (1 + prosperity / 100.0));
        /// <summary>Building a house level: price tax x 1,200 x 2^(level - 1) (the same in every district).</summary>
        public static long BaseBuildCost(int plainBaseTax, int level) => (long)Math.Round(plainBaseTax * Conf.Prices.Build * Math.Pow(Conf.Prices.BuildGrowth, Math.Max(0, level - 1)));
        /// <summary>Cultivation grade needed for a house level (default: from level 3, level - 1).</summary>
        public static int RequiredGrade(int level) => HouseConf(level).Grade;
        public static long HouseTax(int baseTax, int level, char district) =>
            level <= 0 ? 0 : (long)Math.Round(baseTax * HouseConf(level).Tax * DistrictOf(district.ToString()).Price);
        /// <summary>Palace of the town master, next level (2-5): base tax x 15,000 x 1.35^(level - 2), paid from the town budget.</summary>
        public static long BasePalaceCost(int plainBaseTax, int nextLevel) => (long)Math.Round(plainBaseTax * Conf.Prices.Palace * Math.Pow(Conf.Prices.PalaceGrowth, Math.Max(0, nextLevel - 2)));

        // ------------------------------------------------------------------ data

        public class Plot
        {
            public int Index { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string District { get; set; }
            public int Prosperity { get; set; }
            public int Level { get; set; }
            /// <summary>Unit id of the owner (the player or an NPC cultivator), null for the mortals.</summary>
            public string OwnerId { get; set; }
            public int Unpaid { get; set; }
            /// <summary>Rooms of the house: kind -> level (EstateEvent.Rooms.cs).</summary>
            public Dictionary<int, int> Rooms { get; set; } = new Dictionary<int, int>();
            /// <summary>Herbs of the herb garden waiting to be collected (item ids).</summary>
            public List<int> Herbs { get; set; } = new List<int>();
            /// <summary>A plot taken by a bigger house: the index of the house's own plot (null: none).</summary>
            public int? MainIndex { get; set; }
        }

        public class TownEstate
        {
            public long Population { get; set; }
            public List<Plot> Plots { get; set; } = new List<Plot>();
            public long LastMortalTax { get; set; }
            public int PalaceLevel { get; set; } = 1;
            /// <summary>Level of the city wall (cities only: 1 at first, up to Map.WallMax).</summary>
            public int WallLevel { get; set; } = 1;
            /// <summary>The map of the town (EstateEvent.Layout.cs), made once.</summary>
            public string[] Map { get; set; }
            public int Style { get; set; }
            public int MapVersion { get; set; }
        }

        /// <summary>The plots of a house: its own plot and the plots it took.</summary>
        public static List<Plot> PartsOf(TownEstate e, Plot main) =>
            e.Plots.Where(x => x == main || (x.MainIndex == main.Index && x.OwnerId != null && x.OwnerId == main.OwnerId)).ToList();

        /// <summary>
        /// The plots a house must add to reach a level (empty: none; null: no room): the cheapest block of the level's
        /// size (both ways) that holds the house, made of plots that are free, of mortals or already the house's.
        /// </summary>
        public static List<Plot> Expansion(TownEstate e, Plot main, int level, Func<Plot, long> price)
        {
            var own = PartsOf(e, main);
            var f = Footprint(level);
            if (own.Count >= f[0] * f[1])
                return new List<Plot>();
            var at = new Dictionary<(int, int), Plot>();
            foreach (var p in e.Plots) at[(p.X, p.Y)] = p;
            int minX = own.Min(x => x.X), maxX = own.Max(x => x.X), minY = own.Min(x => x.Y), maxY = own.Max(x => x.Y);
            List<Plot> best = null;
            var bestCost = long.MaxValue;
            var shapes = f[0] == f[1] ? new[] { (f[0], f[1]) } : new[] { (f[0], f[1]), (f[1], f[0]) };
            foreach (var (w, h) in shapes)
                for (var x0 = maxX - w + 1; x0 <= minX; x0++)
                    for (var y0 = maxY - h + 1; y0 <= minY; y0++)
                    {
                        var add = new List<Plot>();
                        var cost = 0L;
                        var ok = true;
                        for (var y = y0; y < y0 + h && ok; y++)
                            for (var x = x0; x < x0 + w; x++)
                            {
                                if (!at.TryGetValue((x, y), out var p)) { ok = false; break; }
                                if (own.Contains(p)) continue;
                                if (p.OwnerId != null) { ok = false; break; }
                                add.Add(p);
                                cost += price(p);
                            }
                        if (ok && cost < bestCost) { bestCost = cost; best = add; }
                    }
            return best;
        }

        /// <summary>The highest house level a plot has room for (the plots around free or of mortals).</summary>
        public static int MaxLevelAt(TownEstate e, Plot p)
        {
            for (var l = MAX_LEVEL; l > 1; l--)
                if (Expansion(e, p, l, _ => 0) != null)
                    return l;
            return 1;
        }

        /// <summary>Gives the plots of a house back: free, or to the mortals moving in.</summary>
        public static void Release(TownEstate e, Plot main, bool mortals = false)
        {
            Touch();
            foreach (var p in PartsOf(e, main))
            {
                p.OwnerId = null; p.Unpaid = 0; p.MainIndex = null;
                p.Rooms = new Dictionary<int, int>(); p.Herbs = new List<int>();
                p.Level = mortals ? DistrictOf(p.District).MortalLevel : 0;
            }
        }

        public Dictionary<string, TownEstate> Towns { get; set; } = new Dictionary<string, TownEstate>();

        /// <summary>Base tax of a town without its tax rate (prices).</summary>
        public static int PlainBaseTax(MapBuildTown town) =>
            InflationaryEvent.CalculateInflationary((int)(Math.Pow(2, town.gridData.areaBaseID) * MapBuildPropertyEvent.FIXING_RATE)) * (town.IsCity() ? 2 : 1);

        /// <summary>
        /// The "price tax" houses, land and rooms are counted in: HouseTax x area x AreaWeight (area 10: x5), x2 in a city,
        /// inflation applied; the same for every buyer (the player or an NPC, whatever the grade).
        /// </summary>
        public static int BasePriceTax(int area, bool city) =>
            (int)Math.Round(Conf.Prices.HouseTax * Math.Max(1, area) * (double)Conf.Prices.AreaWeight) * (city ? 2 : 1);

        public static int PriceTax(MapBuildTown town) =>
            InflationaryEvent.CalculateInflationary(BasePriceTax(town.gridData.areaBaseID, town.IsCity()));

        /// <summary>Sub-buildings of the town (BuildingArrangeEvent: portal, auction, factory, markets, bounty board).</summary>
        public static int TownBuildings(MapBuildTown town)
        {
            try
            {
                return MOD_nE7UL2.Enum.BuildingCostEnum.GetAllEnums<MOD_nE7UL2.Enum.BuildingCostEnum>().Count(x => !x.IsSchool && BuildingArrangeEvent.IsBuilt(town, x));
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"EstateEvent.TownBuildings {town?.name}: {ex.Message}");
                return 0;
            }
        }

        public static long BasePopulation(MapBuildTown town) => BasePopulation(town.IsCity(), town.gridData.areaBaseID);

        public static float TaxRateOf(MapBuildTown town) =>
            MapBuildPropertyEvent.Instance.TaxRate.TryGetValue(town.buildData.id, out var r) ? r : 1f;

        public static long Target(MapBuildTown town, TownEstate e)
        {
            var basePop = BasePopulation(town);
            var houses = e.Plots.Where(x => x.OwnerId != null).Sum(x => x.Level);
            var target = TargetPopulation(basePop, BudgetBonus(MapBuildPropertyEvent.GetBuildProperty(town), PlainBaseTax(town), basePop), TaxRateOf(town), houses, e.PalaceLevel, TownBuildings(town))
                + (long)GrownLevels(e) * Conf.Growth.PerMortalLevel;
            // merit (MeritEvent): a ruin stays small; the town of a merit cultivator's house grows more
            return (long)Math.Round(target * (double)MeritEvent.PopulationRate(town)
                * MeritEvent.TownPopRate(MeritEvent.Level(g.world.playerUnit), HasHouse(town, g.world.playerUnit)));
        }

        /// <summary>True when an estate has a map of this generator and of the size of the town.</summary>
        public static bool IsCurrent(TownEstate e, bool city) =>
            e != null && e.MapVersion == MAP_VERSION && e.Map != null && e.Map.Length == MapSize(city).h && e.Plots.Count == PlotCount(e.Map);

        /// <summary>
        /// An estate on a map: its plots (prosperity by district and main roads), the houses of an older estate moved to a
        /// plot of the same district with room for their level (a smaller level when no plot has room).
        /// </summary>
        public static TownEstate MakeEstate(string[] map, int style, int seed, TownEstate old)
        {
            var e = new TownEstate
            {
                PalaceLevel = old?.PalaceLevel ?? 1,
                Population = old?.Population ?? 0,
                LastMortalTax = old?.LastMortalTax ?? 0,
                Map = map,
                Style = style,
                MapVersion = MAP_VERSION,
            };
            for (var y = 0; y < map.Length; y++)
                for (var x = 0; x < map[y].Length; x++)
                {
                    var c = map[y][x];
                    if (!IsPlotCell(c)) continue;
                    var i = e.Plots.Count;
                    e.Plots.Add(new Plot { Index = i, X = x, Y = y, District = c.ToString(), Prosperity = Prosperity(c, NearMainRoad(map, x, y), seed, i) });
                }
            if (old != null)
                foreach (var h in old.Plots.Where(x => x.OwnerId != null && x.Level > 0).ToList())
                    MoveHouse(e, h);
            return e;
        }

        private static void MoveHouse(TownEstate e, Plot h)
        {
            for (var level = Math.Min(MAX_LEVEL, h.Level); level >= 1; level--)
                foreach (var to in e.Plots.Where(x => x.OwnerId == null).OrderBy(x => x.District == h.District ? 0 : 1).ThenByDescending(x => x.Prosperity).ToList())
                {
                    var add = Expansion(e, to, level, _ => 0);
                    if (add == null) continue;
                    to.OwnerId = h.OwnerId; to.Level = level; to.Unpaid = h.Unpaid; to.MainIndex = null;
                    to.Rooms = h.Rooms ?? new Dictionary<int, int>(); to.Herbs = h.Herbs ?? new List<int>();
                    foreach (var a in add) Take(a, to);
                    return;
                }
        }

        /// <summary>A plot taken by a house (the mortals bought out).</summary>
        public static void Take(Plot part, Plot main)
        {
            Touch();
            part.OwnerId = main.OwnerId; part.Level = 0; part.MainIndex = main.Index; part.Unpaid = 0;
            part.Rooms = new Dictionary<int, int>(); part.Herbs = new List<int>();
        }

        /// <summary>Part of the seed of the maps that differs from a save to another (the towns have the same ids in every world).</summary>
        public static string SaveSalt()
        {
            try { return g.world?.playerUnit?.GetUnitId() ?? string.Empty; }
            catch { return string.Empty; }
        }

        /// <summary>The estate of a town (made the first time: its map, the plots, population at its target, mortal houses).</summary>
        public static TownEstate Get(MapBuildTown town)
        {
            if (town == null || Instance == null)
                return null;
            var id = town.buildData.id;
            var city = town.IsCity();
            if (Instance.Towns.TryGetValue(id, out var e) && IsCurrent(e, city))
                return e;
            var seed = StableSeed(id + "|" + SaveSalt());
            if (e != null && e.MapVersion == MAP_VERSION && e.Map != null && city && e.Map.Length < MapSize(true).h)
            {
                // a town grown into a city: its map in the middle of a bigger one, every plot kept
                var bigger = ExpandMap(e.Map, true, seed, out var ox, out var oy);
                e = ExpandEstate(e, bigger, e.Style, seed, ox, oy);
            }
            else
            {
                // first time, or a map of an older version: a new map, the houses moved
                var map = GenerateMap(city, seed, out var style);
                e = MakeEstate(map, style, seed, e);
            }
            Instance.Towns[id] = e;
            Touch();
            if (e.Population <= 0)
                e.Population = Target(town, e);
            if (e.LastMortalTax <= 0)
                e.LastMortalTax = MortalTax(e.Population, MapBuildPropertyEvent.GetBaseTax(town));
            var want = MortalPlotsTarget(e.Plots.Count, e.Population, BasePopulation(town));
            foreach (var p in e.Plots.Where(x => x.OwnerId == null).OrderByDescending(x => x.Prosperity).Take(want))
                p.Level = DistrictOf(p.District).MortalLevel;
            return e;
        }

        public static Plot HouseOf(MapBuildTown town, WorldUnitBase wunit)
        {
            var id = wunit?.GetUnitId();
            return id == null ? null : Get(town)?.Plots.FirstOrDefault(x => x.OwnerId == id && x.Level > 0);
        }

        public static bool HasHouse(MapBuildTown town, WorldUnitBase wunit) => HouseOf(town, wunit) != null;

        /// <summary>The houses of a unit in every town (estates already made).</summary>
        public static IEnumerable<Plot> HousesOf(WorldUnitBase wunit)
        {
            var id = wunit?.GetUnitId();
            if (id == null || Instance == null)
                return Enumerable.Empty<Plot>();
            return Instance.Towns.Values.SelectMany(x => x.Plots).Where(x => x.OwnerId == id && x.Level > 0);
        }

        /// <summary>A price the player can pay: at most Prices.MaxPrice (his money is an int).</summary>
        public static long Payable(long price) => Math.Max(0, Math.Min(Conf.Prices.MaxPrice, price));

        /// <summary>S&amp;M: the estates off (no plot window, NPCs buy no house), NPCs buy no house, prices and house tax x (1 + rate).</summary>
        public static bool Enabled => Instance != null && !(SMGlobalConfigsEvent.Current?.NoEstate ?? false);
        public static bool NpcHouses => Enabled && !(SMGlobalConfigsEvent.Current?.NoNpcHouses ?? false);
        public static double PriceRate => SMGlobalConfigsEvent.Rate(c => c.EstatePriceRate);
        public static double TaxRate => SMGlobalConfigsEvent.Rate(c => c.EstateTaxRate);

        public static long LandPrice(MapBuildTown town, Plot p) => Payable((long)Math.Round(BaseLandPrice(PriceTax(town), DistrictOf(p.District).Code, p.Prosperity) * PriceRate));
        public static long BuildCost(MapBuildTown town, int level) => Payable((long)Math.Round(BaseBuildCost(PriceTax(town), level) * PriceRate));
        public static long HouseTax(MapBuildTown town, Plot p, int level = 0) =>
            (long)Math.Round(HouseTax(MapBuildPropertyEvent.GetBaseTax(town), level > 0 ? level : p.Level, DistrictOf(p.District).Code) * TaxRate);
        /// <summary>Yearly budget of a town (MapBuildPropertyEvent): x this, +25 % a palace level above 1.</summary>
        public static float PalaceBudgetRate(MapBuildTown town)
        {
            var e = Instance == null || town == null ? null : Get(town);
            return 1f + Conf.Population.PalaceYearlyBudget * Math.Max(0, (e?.PalaceLevel ?? 1) - 1);
        }

        public static long PalaceCost(MapBuildTown town, TownEstate e) => BasePalaceCost(PlainBaseTax(town), e.PalaceLevel + 1);
        /// <summary>A plot bought for a bigger house: its land, and half the building of the mortals living on it.</summary>
        public static long AcquirePrice(MapBuildTown town, Plot p) =>
            Payable(LandPrice(town, p) + (p.Level > 0 && p.OwnerId == null ? (long)Math.Round(BuildCost(town, p.Level) * Conf.Prices.BuyOutMortals) : 0));

        /// <summary>Upgrade of a house: the plots to buy around it (null: no room) and the whole cost.</summary>
        public static (List<Plot> add, long land, long total) UpgradeCost(MapBuildTown town, Plot p)
        {
            var add = Expansion(Get(town), p, p.Level + 1, x => AcquirePrice(town, x));
            var land = add?.Sum(x => AcquirePrice(town, x)) ?? 0;
            return (add, land, Payable(BuildCost(town, p.Level + 1) + land));
        }

        /// <summary>
        /// Pays a house price: the player in whole spirit banknotes of the bag (his stones are an int, the prices are not);
        /// an NPC in spirit stones (it only buys what its stones cover). Returns what the town budget gets (0: not paid).
        /// </summary>
        private static long PayHouse(WorldUnitBase wunit, long price)
        {
            if (wunit.IsPlayer())
            {
                var notes = SpiritNoteEvent.NotesFor(price);
                return SpiritNoteEvent.PayNotes(wunit, notes) ? Math.Max(1, notes * SpiritNoteEvent.NoteValue) : 0;
            }
            if (price > wunit.GetUnitMoney())
                return 0;
            wunit.AddUnitMoney(-(int)price);
            return Math.Max(1, price);
        }

        public enum BuildResult { Ok, NotFree, HasHouse, NoMoney, GradeTooLow, MaxLevel, NotOwner, NoSlot, NoRoom }

        /// <summary>Buys a free plot and builds a level 1 house on it; the money goes to the town budget.</summary>
        public static BuildResult Buy(MapBuildTown town, Plot p, WorldUnitBase wunit)
        {
            if (p.Level > 0 || p.OwnerId != null)
                return BuildResult.NotFree;
            if (HasHouse(town, wunit))
                return BuildResult.HasHouse;
            // the player: whole banknotes of the bag; an NPC: spirit stones
            var cost = PayHouse(wunit, Payable(LandPrice(town, p) + BuildCost(town, 1)));
            if (cost <= 0)
                return BuildResult.NoMoney;
            MapBuildPropertyEvent.AddBuildProperty(town, cost);
            p.OwnerId = wunit.GetUnitId();
            p.Level = 1;
            Touch();
            p.Unpaid = 0;
            p.MainIndex = null;
            p.Rooms = new Dictionary<int, int>();
            p.Herbs = new List<int>();
            if (wunit.IsPlayer())
                TaoistLogEvent.Log("log_estate_buy", town.name, cost);
            return BuildResult.Ok;
        }

        /// <summary>Upgrades the house of the unit one level, buying the plots around it a bigger house needs.</summary>
        public static BuildResult Upgrade(MapBuildTown town, Plot p, WorldUnitBase wunit)
        {
            if (p.OwnerId != wunit.GetUnitId())
                return BuildResult.NotOwner;
            if (p.Level >= MAX_LEVEL)
                return BuildResult.MaxLevel;
            if (wunit.GetGradeLvl() < RequiredGrade(p.Level + 1))
                return BuildResult.GradeTooLow;
            var (add, _, price) = UpgradeCost(town, p);
            if (add == null)
                return BuildResult.NoRoom;
            var cost = PayHouse(wunit, price);
            if (cost <= 0)
                return BuildResult.NoMoney;
            MapBuildPropertyEvent.AddBuildProperty(town, cost);
            foreach (var a in add)
                Take(a, p);
            p.Level++;
            if (wunit.IsPlayer())
                TaoistLogEvent.Log("log_estate_upgrade", town.name, HouseName(p.Level), cost);
            return BuildResult.Ok;
        }

        /// <summary>City wall, next level: plain base tax x 3,000 x 1.5^(level - 2), from the town budget.</summary>
        public static long BaseWallCost(int plainBaseTax, int nextLevel) => (long)Math.Round(plainBaseTax * Conf.Prices.Wall * Math.Pow(Conf.Prices.WallGrowth, Math.Max(0, nextLevel - 2)));
        public static long WallCost(MapBuildTown town, TownEstate e) => BaseWallCost(PlainBaseTax(town), e.WallLevel + 1);

        /// <summary>Defense of a city attacked (MapBuildBattleEvent): x this, +10 % at wall level 1, +5 % a level above (a town has no wall).</summary>
        public static float WallDefenseRate(int wallLevel) =>
            wallLevel <= 0 ? 1f : 1f + Conf.Map.WallDefense + Conf.Map.WallDefensePerLevel * (wallLevel - 1);
        /// <summary>The defense bonus of a wall level, in %.</summary>
        public static int WallDefensePercent(int wallLevel) => (int)Math.Round((WallDefenseRate(wallLevel) - 1f) * 100);
        public static float WallDefenseRate(MapBuildBase build)
        {
            var town = build?.TryCast<MapBuildTown>();
            if (town == null || !town.IsCity() || Instance == null)
                return 1f;
            return WallDefenseRate(Get(town)?.WallLevel ?? 0);
        }

        /// <summary>The master of a city strengthens its wall one level (paid from the town budget).</summary>
        public static BuildResult UpgradeWall(MapBuildTown town, WorldUnitBase master)
        {
            var e = Get(town);
            if (!town.IsCity() || !MapBuildPropertyEvent.IsTownMaster(town, master))
                return BuildResult.NotOwner;
            if (e.WallLevel >= Conf.Map.WallMax)
                return BuildResult.MaxLevel;
            var cost = WallCost(town, e);
            if (MapBuildPropertyEvent.GetBuildProperty(town) < cost)
                return BuildResult.NoMoney;
            MapBuildPropertyEvent.AddBuildProperty(town, -cost);
            e.WallLevel++;
            if (master.IsPlayer())
                TaoistLogEvent.Log("log_estate_wall", town.name, e.WallLevel, cost);
            return BuildResult.Ok;
        }

        /// <summary>The town master expands the palace one level (paid from the town budget).</summary>
        public static BuildResult UpgradePalace(MapBuildTown town, WorldUnitBase master)
        {
            var e = Get(town);
            if (!MapBuildPropertyEvent.IsTownMaster(town, master))
                return BuildResult.NotOwner;
            if (e.PalaceLevel >= PALACE_MAX)
                return BuildResult.MaxLevel;
            var cost = PalaceCost(town, e);
            if (MapBuildPropertyEvent.GetBuildProperty(town) < cost)
                return BuildResult.NoMoney;
            MapBuildPropertyEvent.AddBuildProperty(town, -cost);
            e.PalaceLevel++;
            if (master.IsPlayer())
                TaoistLogEvent.Log("log_estate_palace", town.name, PalaceName(e.PalaceLevel), cost);
            return BuildResult.Ok;
        }

        public static string HouseName(int level) => GameTool.LS($"estate_house_{Math.Max(1, Math.Min(MAX_LEVEL, level))}");
        public static string PalaceName(int level) => GameTool.LS($"estate_palace_{Math.Max(1, Math.Min(PALACE_MAX, level))}");

        // ------------------------------------------------------------------ monthly

        /// <summary>Population, mortal tax, mortal houses, house taxes and gardens of every town.</summary>
        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnMonthly()
        {
            base.OnMonthly();
            var months = Math.Max(1, ModMaster.MonthsPassed);
            foreach (var town in ModMain.Store.Towns)
            {
                try { MonthTown(town, months); }
                catch (Exception ex) { DebugHelper.WriteLine($"EstateEvent.MonthTown {town?.name}: {ex}"); }
            }
        }

        public static void MonthTown(MapBuildTown town, int months, Func<double> roll = null)
        {
            roll = roll ?? Roll;
            var e = Get(town);
            if (e == null)
                return;
            // population and mortal tax
            e.Population = NextPopulation(e.Population, Target(town, e), months, TownBuildings(town));
            e.LastMortalTax = MortalTax(e.Population, MapBuildPropertyEvent.GetBaseTax(town));
            MapBuildPropertyEvent.AddBuildProperty(town, e.LastMortalTax * months);

            // mortal houses follow the population: one plot built or left a month
            var want = MortalPlotsTarget(e.Plots.Count, e.Population, BasePopulation(town));
            for (var m = 0; m < months; m++)
            {
                var mortal = e.Plots.Where(x => x.OwnerId == null && x.Level > 0).ToList();
                if (mortal.Count < want)
                {
                    var free = e.Plots.Where(x => x.OwnerId == null && x.Level == 0).OrderByDescending(x => x.Prosperity).FirstOrDefault();
                    if (free != null) free.Level = DistrictOf(free.District).MortalLevel;
                }
                else if (mortal.Count > want)
                {
                    mortal.OrderBy(x => x.Prosperity).First().Level = 0;
                }
                else break;
            }
            GrowMortals(town, e, months, roll);

            // an NPC master expands the palace when the budget is far above its cost
            var master = MapBuildPropertyEvent.GetTownMaster(town);
            if (master != null && !master.IsPlayer() && e.PalaceLevel < PALACE_MAX && MapBuildPropertyEvent.GetBuildProperty(town) > PalaceCost(town, e) * Conf.Prices.NpcPalaceBudget)
                UpgradePalace(town, master);
            // ... and the wall of a city
            if (master != null && !master.IsPlayer() && town.IsCity() && e.WallLevel < Conf.Map.WallMax && MapBuildPropertyEvent.GetBuildProperty(town) > WallCost(town, e) * Conf.Prices.NpcPalaceBudget)
                UpgradeWall(town, master);

            // houses: taxes and gardens
            foreach (var p in e.Plots.Where(x => x.OwnerId != null && x.Level > 0).ToList())
            {
                var owner = g.world.unit.GetUnit(p.OwnerId);
                if (owner == null || owner.isDie)
                {
                    Release(e, p, true);   // the house of a dead cultivator: the mortals move in
                    continue;
                }
                Garden(p, owner, months);
                var tax = HouseTax(town, p) * months;
                if (SpiritNoteEvent.Pay(owner, tax))
                {
                    MapBuildPropertyEvent.AddBuildProperty(town, tax);
                    MapBuildPropertyEvent.RecordTax(town, (int)Math.Min(int.MaxValue, tax), true);
                    p.Unpaid = 0;
                    continue;
                }
                p.Unpaid += months;
                if (p.Unpaid >= MaxUnpaid)
                {
                    if (owner.IsPlayer())
                        TaoistLogEvent.Log("log_estate_seized", town.name, HouseName(p.Level));
                    Release(e, p, true);   // the mortals move in
                }
                else if (owner.IsPlayer())
                    TaoistLogEvent.Log("log_estate_unpaid", town.name, tax, MaxUnpaid - p.Unpaid);
            }
        }

        /// <summary>A month spent in a town where the player has a house: a full rest (no inn) and the rooms.</summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (!wunit.IsPlayer())
            {
                try { NpcHouse(wunit, Math.Max(1, ModMaster.MonthsPassed), Roll); NpcHome(wunit, Math.Max(1, ModMaster.MonthsPassed), Roll); }
                catch (Exception ex) { DebugHelper.WriteLine($"EstateEvent.NpcHouse {wunit?.GetUnitId()}: {ex.Message}"); }
                return;
            }
            var town = wunit.GetMapBuild<MapBuildTown>();
            var house = town == null ? null : HouseOf(town, wunit);
            if (house == null)
                return;
            Rest(wunit);
            HomeRooms(house, wunit, Math.Max(1, ModMaster.MonthsPassed));
        }

        private static double Roll() => CommonTool.Random(0f, 1f);

        /// <summary>
        /// The mortal houses grow: a month, by chance (MortalChance), one house below its cap (MortalCap: population,
        /// prosperity) grows a level, the most prosperous first; one above its cap (the population fell) shrinks back.
        /// </summary>
        public static void GrowMortals(MapBuildTown town, TownEstate e, int months, Func<double> roll)
        {
            var basePop = BasePopulation(town);
            int Cap(Plot p) => MortalCap(DistrictOf(p.District).MortalLevel, p.Prosperity, e.Population, basePop);
            for (var m = 0; m < months; m++)
            {
                if (roll() >= Conf.Growth.MortalChance)
                    continue;
                var mortal = e.Plots.Where(x => x.OwnerId == null && x.Level > 0).ToList();
                var over = mortal.Where(x => x.Level > Cap(x)).OrderBy(x => x.Prosperity).FirstOrDefault();
                if (over != null) { over.Level--; continue; }
                var grow = mortal.Where(x => x.Level < Cap(x)).OrderByDescending(x => x.Prosperity).ThenBy(x => x.Index).FirstOrDefault();
                if (grow != null) grow.Level++;
            }
        }

        /// <summary>The house of a unit (its own plot) and its town; (null, null) when it has none.</summary>
        public static (MapBuildTown town, Plot plot) HomeOf(WorldUnitBase wunit)
        {
            var id = wunit?.GetUnitId();
            if (id == null || Instance == null) return (null, null);
            var owners = Owners();
            if (!owners.TryGetValue(id, out var h) || h.plot.OwnerId != id || h.plot.Level <= 0)
                return (null, null);
            return (ModMain.Store.Towns.FirstOrDefault(x => x.buildData.id == h.townId), h.plot);
        }

        // the houses by owner, made again when a house changes hands (Touch) or a month passes: the monthly NPC
        // logic asks it for every NPC
        private static Dictionary<string, (string townId, Plot plot)> _owners;
        private static int _ownersVersion = -1, _ownersMonth = -1, _version;

        /// <summary>A house changed hands (buy, upgrade, release, a new map): the index of the owners is made again.</summary>
        public static void Touch() => _version++;

        private static Dictionary<string, (string townId, Plot plot)> Owners()
        {
            var month = GameHelper.IsInGame() ? GameHelper.GetGameTotalMonth() : 0;
            if (_owners != null && _ownersVersion == _version && _ownersMonth == month)
                return _owners;
            _owners = new Dictionary<string, (string, Plot)>();
            foreach (var kv in Instance.Towns)
                foreach (var p in kv.Value.Plots)
                    if (p.OwnerId != null && p.Level > 0 && p.MainIndex == null)
                        _owners[p.OwnerId] = (kv.Key, p);
            _ownersVersion = _version; _ownersMonth = month;
            return _owners;
        }

        /// <summary>
        /// The family house a unit lives in when it has none: its spouse's, else its parents', else its grandparents'
        /// (FamilyEvent); who owns it. (null, null, null) when none.
        /// </summary>
        public static (MapBuildTown town, Plot plot, WorldUnitBase owner) FamilyHome(WorldUnitBase wunit)
        {
            if (wunit == null) return (null, null, null);
            var f = FamilyEvent.Family(wunit);
            var spouse = FamilyEvent.Unit(f.Spouse);
            var line = new List<WorldUnitBase>();
            if (spouse != null && !spouse.isDie) line.Add(spouse);
            var parents = f.Parents.Select(FamilyEvent.Unit).Where(x => x != null && !x.isDie).ToList();
            line.AddRange(parents);
            foreach (var p in f.Parents.Select(FamilyEvent.Unit).Where(x => x != null))
                line.AddRange(FamilyEvent.Family(p).Parents.Select(FamilyEvent.Unit).Where(x => x != null && !x.isDie));
            foreach (var u in line)
            {
                var (town, plot) = HomeOf(u);
                if (town != null) return (town, plot, u);
            }
            return (null, null, null);
        }

        /// <summary>
        /// The towns where an NPC without a family house looks for one: those of the area where it is, a city first from
        /// NpcCityGrade (else a small town first), the town where it stands first among its kind, then at random.
        /// </summary>
        public static List<MapBuildTown> ChooseTowns(WorldUnitBase wunit)
        {
            var area = wunit.GetUnitPosAreaId();
            var here = wunit.GetMapBuild<MapBuildTown>();
            var towns = ModMain.Store.Towns.Where(x => x.gridData.areaBaseID == area).ToList();
            if (towns.Count == 0 && here != null) towns.Add(here);
            var wantCity = wunit.GetGradeLvl() >= Conf.Growth.NpcCityGrade;
            return towns.OrderBy(x => x.IsCity() == wantCity ? 0 : 1)
                .ThenBy(x => here != null && x.buildData.id == here.buildData.id ? 0 : 1)
                .ThenBy(_ => CommonTool.Random(0, 10000)).ToList();
        }

        /// <summary>
        /// An NPC cultivator and its house, by chance (a month): without a house, it buys one (at = the town to buy in,
        /// for the tests; else ChooseTowns). A spouse and the unmarried children / grandchildren of a house owner live in
        /// the family house (no house of their own); a married one moves out, in the same town. With a house, it upgrades
        /// it one level. Paid with its spirit stones (at most NpcSpend of them); never more than NpcShare of the plots of a
        /// town owned by NPCs. Returns what it did (null: nothing).
        /// </summary>
        public static string NpcHouse(WorldUnitBase wunit, int months, Func<double> roll, MapBuildTown at = null)
        {
            var gr = Conf.Growth;
            if (!gr.Npc || !NpcHouses || wunit == null || wunit.isDie || wunit.IsPlayer() || Instance == null)
                return null;
            // one roll first (cheap: most NPCs stop here every month), then the house
            var r = roll();
            if (r >= Math.Max(gr.NpcBuyChance, gr.NpcUpgradeChance) * months)
                return null;
            var budget = (long)(wunit.GetUnitMoney() * (double)gr.NpcSpend);
            var (homeTown, house) = HomeOf(wunit);
            if (house == null)
            {
                if (r >= gr.NpcBuyChance * months)
                    return null;
                List<MapBuildTown> towns;
                var family = FamilyHome(wunit);
                if (family.town != null)
                {
                    // the spouse shares the house; an unmarried child lives with the family
                    if (family.owner.GetUnitId() == FamilyEvent.Spouse(wunit) || string.IsNullOrEmpty(FamilyEvent.Spouse(wunit)))
                        return null;
                    towns = new List<MapBuildTown> { family.town };
                }
                else if (at != null)
                    towns = new List<MapBuildTown> { at };
                else
                {
                    if (wunit.GetMapBuild<MapBuildTown>() == null && wunit.GetUnitPosAreaId() <= 0)
                        return null;
                    towns = ChooseTowns(wunit);
                }
                foreach (var town in towns.Take(3))
                {
                    var e = Get(town);
                    var me = Player?.GetUnitId();
                    if (e.Plots.Count(x => x.OwnerId != null && x.OwnerId != me) >= e.Plots.Count * gr.NpcShare)
                        continue;
                    var plot = e.Plots.Where(x => x.OwnerId == null && x.Level == 0)
                        .Where(x => LandPrice(town, x) + BuildCost(town, 1) <= budget)
                        .OrderByDescending(x => x.Prosperity).ThenBy(x => x.Index).FirstOrDefault();
                    if (plot != null && Buy(town, plot, wunit) == BuildResult.Ok)
                        return "buy";
                }
                return null;
            }
            if (house.Level >= MAX_LEVEL || wunit.GetGradeLvl() < RequiredGrade(house.Level + 1) || r >= gr.NpcUpgradeChance * months)
                return null;
            var (add, _, price) = UpgradeCost(homeTown, house);
            if (add == null || price > budget)
                return null;
            return Upgrade(homeTown, house, wunit) == BuildResult.Ok ? "upgrade" : null;
        }

        /// <summary>
        /// An NPC with a home (its house or its family's) goes back to that town with the chance NpcHomeChance a month;
        /// not a hired one, not the player's team, not a town guardian of another town. Returns the town (null: stays).
        /// </summary>
        public static MapBuildTown NpcHome(WorldUnitBase wunit, int months, Func<double> roll)
        {
            var gr = Conf.Growth;
            if (!gr.Npc || wunit == null || wunit.isDie || wunit.IsPlayer() || Instance == null || HirePeopleEvent.IsHired(wunit))
                return null;
            if (roll() >= 1 - Math.Pow(1 - gr.NpcHomeChance, Math.Max(1, months)))
                return null;
            var town = HomeOf(wunit).town ?? FamilyHome(wunit).town;
            if (town == null || wunit.GetMapBuild<MapBuildTown>()?.buildData.id == town.buildData.id)
                return null;
            var guard = MapBuildPropertyEvent.GetGuardTown(wunit);
            if (guard != null && guard.buildData.id != town.buildData.id)
                return null;
            wunit.SetUnitPos(town.GetOrigiPoint());
            return town;
        }

        public static void Rest(WorldUnitBase wunit)
        {
            wunit.SetProperty<int>(UnitPropertyEnum.Hp, wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value);
            wunit.SetProperty<int>(UnitPropertyEnum.Mp, wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value);
            wunit.SetProperty<int>(UnitPropertyEnum.Sp, wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value);
        }

        // ------------------------------------------------------------------ pictures

        /// <summary>Loaded atlases (a nested class: its Unity types load only when a picture is needed, not in unit tests).</summary>
        private static class Atlases
        {
            public static readonly Dictionary<string, UnityEngine.U2D.SpriteAtlas> Loaded = new Dictionary<string, UnityEngine.U2D.SpriteAtlas>();
        }

        /// <summary>A sprite of the game's atlases (null when not found).</summary>
        public static Sprite GameSprite(string atlas, string name)
        {
            // the game's own loader first (it knows the atlases loaded by its atlas manager, e.g. TownCommon)
            try
            {
                var s = SpriteTool.GetSprite(atlas, name);
                if (s != null)
                    return s;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"EstateEvent.GameSprite SpriteTool {atlas}/{name}: {ex.Message}");
            }
            try
            {
                if (!Atlases.Loaded.TryGetValue(atlas, out var a) || a == null)
                {
                    a = g.res.Load<UnityEngine.U2D.SpriteAtlas>("SpriteAtlas/" + atlas);
                    Atlases.Loaded[atlas] = a;
                }
                return a?.GetSprite(name);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"EstateEvent.GameSprite {atlas}/{name}: {ex.Message}");
                return null;
            }
        }
    }
}
