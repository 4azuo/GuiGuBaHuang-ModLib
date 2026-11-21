using ModLib.Attributes;
using ModLib.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Settings of the town estates, read from ModConf/estate_configs.json (pictures, sizes, rates, prices); the defaults
    /// below are the same as the file, used when it is missing or a value is left out.
    /// Pictures: "Atlas/Sprite" of the game (docs/GameAssets, Buildings tab).
    /// </summary>
    public class EstateConfig
    {
        public const string FILE = "estate_configs.json";

        public class _Population
        {
            public long TownBase { get; set; } = 10000;
            public long TownPerArea { get; set; } = 2000;
            public long TownMax { get; set; } = 30000;
            public long CityBase { get; set; } = 40000;
            public long CityPerArea { get; set; } = 6000;
            public long CityMax { get; set; } = 100000;
            /// <summary>Residents attracted by the budget: budget / base tax x this (at most the base population).</summary>
            public long BudgetBonus { get; set; } = 5;
            /// <summary>Share of the gap to the target the population moves a month.</summary>
            public float MoveRate { get; set; } = 0.05f;
            /// <summary>Mortal tax = population x base tax / this.</summary>
            public long MortalTaxDivisor { get; set; } = 1000;
            /// <summary>
            /// Tax factor of the population = TaxBase - TaxSlope x tax rate (TaxMin - TaxMax). The mortal tax grows with
            /// rate x factor: the defaults make it best at a rate about 2 (x1.4 of rate 1); rate 4 is x0.4, rate 10 x0.3.
            /// </summary>
            public float TaxBase { get; set; } = 1.3f;
            public float TaxSlope { get; set; } = 0.3f;
            public float TaxMin { get; set; } = 0.03f;
            public float TaxMax { get; set; } = 1.15f;
            /// <summary>Tax rate an NPC town master sets every year (random between the two).</summary>
            public float[] NpcTaxRate { get; set; } = { 0.5f, 3.5f };
            public int PerHouseLevel { get; set; } = 300;
            public float PerPalaceLevel { get; set; } = 0.20f;
            /// <summary>The palace also raises the yearly budget of the town (MapBuildPropertyEvent): + this a level above 1.</summary>
            public float PalaceYearlyBudget { get; set; } = 0.25f;
            /// <summary>Every sub-building of the town (portal, auction, factory, markets, bounty board): target + this share.</summary>
            public float PerTownBuilding { get; set; } = 0.08f;
            /// <summary>Every sub-building of the town: the monthly move toward the target + this.</summary>
            public float MoveRatePerTownBuilding { get; set; } = 0.01f;
        }

        /// <summary>
        /// Prices. Houses, land and rooms: in "price taxes" = HouseTax x area x AreaWeight (area 10: x5), x2 in a city,
        /// inflation applied, the same for every buyer. The palace and the wall: in plain base taxes of the town (5 x 2^area,
        /// x2 in a city), paid from its budget.
        /// </summary>
        public class _Prices
        {
            public float HouseTax { get; set; } = 100f;
            public float AreaWeight { get; set; } = 0.5f;
            public float Land { get; set; } = 400;
            /// <summary>House level 1; x BuildGrowth every level.</summary>
            public float Build { get; set; } = 1200;
            public float BuildGrowth { get; set; } = 2.0f;
            /// <summary>Palace level 2 in plain base taxes; x PalaceGrowth every level (paid from the town budget).</summary>
            public float Palace { get; set; } = 7000;
            public float PalaceGrowth { get; set; } = 1.35f;
            /// <summary>An NPC master expands the palace when the budget is this many times its cost.</summary>
            public float NpcPalaceBudget { get; set; } = 1.5f;
            /// <summary>City wall level 2 (from the town budget); x WallGrowth every level.</summary>
            public float Wall { get; set; } = 3000;
            public float WallGrowth { get; set; } = 1.5f;
            /// <summary>Room level 1; x RoomGrowth every level.</summary>
            public float Room { get; set; } = 1500;
            public float RoomGrowth { get; set; } = 2.0f;
            /// <summary>Mortals bought out of a plot a bigger house takes: this share of their building.</summary>
            public float BuyOutMortals { get; set; } = 0.5f;
            /// <summary>Sub-buildings of the towns (BuildingArrangeEvent, paid from the town budget): x this (not the sects').</summary>
            public float TownBuildingCost { get; set; } = 10f;
            /// <summary>
            /// The most the player pays for one thing (land, building, room, upgrade). Big prices are paid with spirit
            /// notes (SpiritNoteEvent): his spirit stones are an int (at most 2,147,483,647), the notes are not limited so.
            /// </summary>
            public long MaxPrice { get; set; } = 1000000000000;
        }

        public class _House
        {
            public string Sprite { get; set; }
            /// <summary>Plots taken (width x height cells of the town map).</summary>
            public int Width { get; set; } = 1;
            public int Height { get; set; } = 1;
            /// <summary>Monthly tax, in base taxes of the town (x the price of the district).</summary>
            public float Tax { get; set; } = 1;
            /// <summary>Cultivation grade needed to build it.</summary>
            public int Grade { get; set; } = 1;
        }

        public class _District
        {
            public int Prosperity { get; set; }
            /// <summary>Factor of the land price and of the house tax.</summary>
            public float Price { get; set; } = 1;
            /// <summary>Level of the mortals' houses (their buy-out price).</summary>
            public int MortalLevel { get; set; } = 1;
            /// <summary>Ground of the plots, "#RRGGBBAA".</summary>
            public string Color { get; set; }
            /// <summary>Pictures of the mortals' houses.</summary>
            public List<string> Sprites { get; set; } = new List<string>();
        }

        public class _Map
        {
            public int[] TownSize { get; set; } = { 24, 16 };
            public int[] CitySize { get; set; } = { 30, 19 };
            public int[] TownPalace { get; set; } = { 4, 3 };
            public int[] CityPalace { get; set; } = { 5, 3 };
            /// <summary>Every plot is at most this many cells from a road (alleys are cut until it is so).</summary>
            public int RoadReach { get; set; } = 3;
            /// <summary>Plots of a district block that gets its name on the map.</summary>
            public int LabelBlock { get; set; } = 6;
            /// <summary>Names of a district shown on the map at most (its biggest blocks).</summary>
            public int MaxLabels { get; set; } = 3;
            /// <summary>Width of the roads drawn on the map, share of a cell's height (the rest of the cell: ground).</summary>
            public float MainRoadWidth { get; set; } = 0.5f;
            public float AlleyWidth { get; set; } = 0.25f;
            /// <summary>Decoration: pictures of the trees of the woods cells, of the people on the main roads (a share of them).</summary>
            public List<string> Trees { get; set; } = new List<string>
            {
                "CommonMapGrid/linggendashu", "CommonMapGrid/linggendashu2", "CommonMapGrid/lingguoshu1", "CommonMapGrid/zhulin",
            };
            public List<string> People { get; set; } = new List<string>
            {
                "VillageCommon/cunzhuangNPC03", "VillageCommon/cunzhuangNPC04", "VillageCommon/cunzhuangNPC05", "VillageCommon/cunzhuangNPC06",
            };
            public float PeopleChance { get; set; } = 0.12f;
            /// <summary>City walls: highest level; defense of the defenders: + WallDefense at level 1, + WallDefensePerLevel a level above.</summary>
            public int WallMax { get; set; } = 5;
            public float WallDefense { get; set; } = 0.10f;
            public float WallDefensePerLevel { get; set; } = 0.05f;
            /// <summary>A wall around the cities (not the towns), gates where the main roads leave it.</summary>
            public bool Wall { get; set; } = true;
            /// <summary>Chances of the landscape of a town, any mix: a river, a lake, woods (outside the town).</summary>
            public float RiverChance { get; set; } = 0.4f;
            public float LakeChance { get; set; } = 0.35f;
            public float WoodsChance { get; set; } = 0.5f;
            /// <summary>Chance weights of the main roads: crossroads, T crossing, ring road, two streets.</summary>
            public float[] RoadWeights { get; set; } = { 1, 1, 1, 1 };
            /// <summary>Colors "#RRGGBBAA": Ground, MainRoad, Alley, Water, Woods, Mine (the player's plots), Owned (an NPC's house), Palace.</summary>
            public Dictionary<string, string> Colors { get; set; } = new Dictionary<string, string>
            {
                ["Ground"] = "#E0D9C28C",
                ["MainRoad"] = "#6B6154D9",
                ["Alley"] = "#A89C85B3",
                ["Water"] = "#6194B3CC",
                ["Woods"] = "#668C5CA6",
                ["Mine"] = "#FFCC33E6",
                ["Owned"] = "#8FA8C7B3",
                ["Palace"] = "#BF4D3373",
                ["Wall"] = "#5C4F42F2",
                ["Wave"] = "#C9E2EEB3",
                ["Rail"] = "#4A3B2EF2",
            };
        }

        public class _Rooms
        {
            public int MaxLevel { get; set; } = 3;
            /// <summary>Quiet room: cultivation exp a month, share of the phase a level.</summary>
            public float QuietExp { get; set; } = 0.02f;
            /// <summary>Training ground: body refining exp a month a level.</summary>
            public int TrainingExp { get; set; } = 40;
            /// <summary>Herb garden: herbs a month a level, at most GardenCap a level.</summary>
            public int GardenHerbs { get; set; } = 1;
            public int GardenCap { get; set; } = 12;
            /// <summary>Spirit gathering array: Qi gathered in the town, + a level.</summary>
            public float ArrayQi { get; set; } = 0.15f;
            /// <summary>Ancestral hall: household upkeep, - a level.</summary>
            public float HallUpkeep { get; set; } = 0.10f;
        }

        /// <summary>The town map lives: the mortals' houses grow, NPC cultivators buy and upgrade houses.</summary>
        public class _Growth
        {
            /// <summary>A month, the chance that one mortal house of a town grows a level (or shrinks back when over its cap).</summary>
            public float MortalChance { get; set; } = 0.05f;
            /// <summary>Levels a mortal house can grow above its district's: +1 when the population is at least
            /// MortalPopulation x the base, +1 on a plot of prosperity MortalProsperity or more.</summary>
            public int MortalMaxBonus { get; set; } = 2;
            public float MortalPopulation { get; set; } = 1.0f;
            public int MortalProsperity { get; set; } = 70;
            /// <summary>Residents a grown level of a mortal house adds to the target population.</summary>
            public int PerMortalLevel { get; set; } = 30;
            /// <summary>NPC cultivators buy houses (paid in spirit stones, at most NpcSpend of theirs; one house each).</summary>
            public bool Npc { get; set; } = true;
            /// <summary>A month, the chance an NPC staying in a town without a house buys one, or upgrades the one it has.</summary>
            public float NpcBuyChance { get; set; } = 0.02f;
            public float NpcUpgradeChance { get; set; } = 0.03f;
            public float NpcSpend { get; set; } = 0.5f;
            /// <summary>At most this share of the plots of a town owned by NPCs (the rest for the mortals and the player).</summary>
            public float NpcShare { get; set; } = 0.25f;
            /// <summary>An NPC buys in a town of the area where it is; from this grade it prefers a city, below a small town.</summary>
            public int NpcCityGrade { get; set; } = 5;
            /// <summary>A month, the chance that an NPC with a home (its house, or its family's) goes back to its town.</summary>
            public float NpcHomeChance { get; set; } = 0.03f;
        }

        /// <summary>
        /// Cultivator clans of a town (ClanEvent): a cultivator with a house there and the living family (spouse, children,
        /// grandchildren). Two clans of a town have a relation -100..100: a base (random at first, StartRelation) moved by
        /// their fights, + MemberWeight x the mean intimacy between their members, + MarriageBonus a marriage between them.
        /// Yearly, hostile clans (HostileAt or less) brawl with the chance BrawlChance (a descendant against another; the loser
        /// hurt, dead with BrawlDeath); bitter enemies (WarAt or less) go to war with WarChance (every member fights; the
        /// losers die with WarDeath, lose their house; a clan without members is wiped out). The player's family is a clan
        /// when the player has a house; the player is asked to step in (a real battle) when it or an ally (AllyAt) is in it.
        /// </summary>
        public class _Clans
        {
            public bool Enabled { get; set; } = true;
            public int MinMembers { get; set; } = 2;
            public int[] StartRelation { get; set; } = { -40, 40 };
            public float MemberWeight { get; set; } = 0.5f;
            public int MarriageBonus { get; set; } = 25;
            public int HostileAt { get; set; } = -30;
            public int WarAt { get; set; } = -70;
            public int AllyAt { get; set; } = 50;
            public float BrawlChance { get; set; } = 0.01f;
            public float WarChance { get; set; } = 0.002f;
            public float BrawlDeath { get; set; } = 0.05f;
            public float WarDeath { get; set; } = 0.25f;
            /// <summary>Base relation after a brawl, a death in it, a war.</summary>
            public int BrawlRelation { get; set; } = -15;
            public int DeathRelation { get; set; } = -40;
            public int WarRelation { get; set; } = -50;
        }

        /// <summary>Spirit notes (SpiritNoteEvent): an item worth Value spirit stones, changed at the bank for a fee.</summary>
        public class _Notes
        {
            public long Value { get; set; } = 10000;
            /// <summary>Share of every exchange kept by the bank (to the town budget), both ways; + the S&amp;M bank fee.</summary>
            public float ExchangeFee { get; set; } = 0.01f;
        }

        public _Notes Notes { get; set; } = new _Notes();
        public _Population Population { get; set; } = new _Population();
        public _Prices Prices { get; set; } = new _Prices();
        public int MaxUnpaidMonths { get; set; } = 6;

        /// <summary>The player's house, levels 1-6.</summary>
        public List<_House> Houses { get; set; } = new List<_House>
        {
            new _House { Sprite = "CommonMapGrid/cunzhuangmaowu1", Width = 1, Height = 1, Tax = 1.0f, Grade = 1 },
            new _House { Sprite = "TownCommon/Build2005", Width = 1, Height = 1, Tax = 1.5f, Grade = 1 },
            new _House { Sprite = "TownCommon/Build2004", Width = 2, Height = 1, Tax = 2.0f, Grade = 2 },
            new _House { Sprite = "TownCommon/Build2002", Width = 2, Height = 1, Tax = 3.0f, Grade = 3 },
            new _House { Sprite = "TownCommon/Build2015", Width = 2, Height = 2, Tax = 4.5f, Grade = 4 },
            new _House { Sprite = "CommonMapGrid/wurenfangshi", Width = 3, Height = 2, Tax = 6.0f, Grade = 5 },
        };

        /// <summary>Pictures of the town master's palace, levels 1-5.</summary>
        public List<string> Palace { get; set; } = new List<string>
        {
            "TownCommon/Build2016",
            "SchoolCommon/BuildUp1001_1",
            "SchoolCommon/BuildUp1001_2",
            "SchoolCommon/BuildUp1001_3",
            "SchoolCommon/BuildUp1001_4",
        };

        /// <summary>M market, N noble quarter, R residential, C craftsmen, O outskirts.</summary>
        public Dictionary<string, _District> Districts { get; set; } = new Dictionary<string, _District>
        {
            ["M"] = new _District { Prosperity = 90, Price = 2.0f, MortalLevel = 3, Color = "#EDA86199",
                Sprites = new List<string> { "TownCommon/Build2001", "TownCommon/Build2009", "TownCommon/Build2013", "TownCommon/Build2001_1", "TownCommon/Build2009_1" } },
            ["N"] = new _District { Prosperity = 78, Price = 1.6f, MortalLevel = 3, Color = "#E6CC808C",
                Sprites = new List<string> { "TownCommon/Build2004", "TownCommon/Build2004_1" } },
            ["R"] = new _District { Prosperity = 52, Price = 1.0f, MortalLevel = 2, Color = "#D1C29E80",
                Sprites = new List<string> { "TownCommon/Build2005", "CommonMapGrid/chuzhuangmaowu2", "TownCommon/Build2005" } },
            ["C"] = new _District { Prosperity = 38, Price = 0.8f, MortalLevel = 2, Color = "#AD9E8C80",
                Sprites = new List<string> { "TownCommon/Build2008", "TownCommon/Build2008_1" } },
            ["O"] = new _District { Prosperity = 15, Price = 0.5f, MortalLevel = 1, Color = "#949E8573",
                Sprites = new List<string> { "CommonMapGrid/cunzhuangmaowu1", "CommonMapGrid/chuzhuangmaowu2" } },
        };

        public _Map Map { get; set; } = new _Map();
        public _Rooms Rooms { get; set; } = new _Rooms();
        public _Growth Growth { get; set; } = new _Growth();
        public _Clans Clans { get; set; } = new _Clans();

        /// <summary>Reads a config (missing values: the defaults) and makes it safe to use.</summary>
        public static EstateConfig Parse(string json)
        {
            var c = string.IsNullOrWhiteSpace(json)
                ? new EstateConfig()
                : JsonConvert.DeserializeObject<EstateConfig>(json, new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace }) ?? new EstateConfig();
            c.Fix();
            return c;
        }

        /// <summary>Fills what is missing or wrong with the defaults (counts of levels, districts, sizes).</summary>
        public void Fix()
        {
            var d = new EstateConfig();
            Population = Population ?? d.Population;
            Notes = Notes ?? d.Notes;
            Notes.Value = Math.Max(1, Notes.Value);
            Notes.ExchangeFee = Math.Max(0f, Math.Min(0.5f, Notes.ExchangeFee));
            Prices = Prices ?? d.Prices;
            Map = Map ?? d.Map;
            Rooms = Rooms ?? d.Rooms;
            Growth = Growth ?? d.Growth;
            Clans = Clans ?? d.Clans;
            if (Clans.StartRelation == null || Clans.StartRelation.Length != 2 || Clans.StartRelation[0] > Clans.StartRelation[1])
                Clans.StartRelation = d.Clans.StartRelation;
            Clans.MinMembers = Math.Max(1, Clans.MinMembers);
            Clans.BrawlChance = Math.Max(0f, Math.Min(1f, Clans.BrawlChance));
            Clans.WarChance = Math.Max(0f, Math.Min(1f, Clans.WarChance));
            Clans.BrawlDeath = Math.Max(0f, Math.Min(1f, Clans.BrawlDeath));
            Clans.WarDeath = Math.Max(0f, Math.Min(1f, Clans.WarDeath));
            Growth.MortalChance = Math.Max(0f, Math.Min(1f, Growth.MortalChance));
            Growth.MortalMaxBonus = Math.Max(0, Growth.MortalMaxBonus);
            Growth.NpcBuyChance = Math.Max(0f, Math.Min(1f, Growth.NpcBuyChance));
            Growth.NpcUpgradeChance = Math.Max(0f, Math.Min(1f, Growth.NpcUpgradeChance));
            Growth.NpcSpend = Math.Max(0f, Math.Min(1f, Growth.NpcSpend));
            Growth.NpcShare = Math.Max(0f, Math.Min(1f, Growth.NpcShare));
            Growth.NpcHomeChance = Math.Max(0f, Math.Min(1f, Growth.NpcHomeChance));
            Houses = Pad(Houses, d.Houses, EstateEvent.MAX_LEVEL);
            for (var i = 0; i < Houses.Count; i++)
            {
                Houses[i] = Houses[i] ?? d.Houses[i];
                Houses[i].Width = Math.Max(1, Math.Min(4, Houses[i].Width));
                Houses[i].Height = Math.Max(1, Math.Min(4, Houses[i].Height));
            }
            Palace = Pad(Palace, d.Palace, EstateEvent.PALACE_MAX);
            Districts = Districts ?? new Dictionary<string, _District>();
            foreach (var k in d.Districts.Keys)
                if (!Districts.TryGetValue(k, out var v) || v == null)
                    Districts[k] = d.Districts[k];
            foreach (var v in Districts.Values)
            {
                v.MortalLevel = Math.Max(1, Math.Min(EstateEvent.MAX_LEVEL, v.MortalLevel));
                if (v.Sprites == null || v.Sprites.Count == 0) v.Sprites = d.Districts["O"].Sprites;
            }
            Map.TownSize = Size(Map.TownSize, d.Map.TownSize, 12, 8);
            Map.CitySize = Size(Map.CitySize, d.Map.CitySize, 12, 8);
            Map.TownPalace = Size(Map.TownPalace, d.Map.TownPalace, 1, 1);
            Map.CityPalace = Size(Map.CityPalace, d.Map.CityPalace, 1, 1);
            Map.RoadReach = Math.Max(1, Map.RoadReach);
            Map.WallMax = Math.Max(1, Map.WallMax);
            Map.MainRoadWidth = Math.Max(0.1f, Math.Min(1f, Map.MainRoadWidth));
            Map.AlleyWidth = Math.Max(0.1f, Math.Min(1f, Map.AlleyWidth));
            Map.Trees = Map.Trees ?? new List<string>();
            Map.People = Map.People ?? new List<string>();
            Map.PeopleChance = Math.Max(0f, Math.Min(1f, Map.PeopleChance));

            if (Map.RoadWeights == null || Map.RoadWeights.Length != EstateEvent.ROAD_COUNT || Map.RoadWeights.Sum() <= 0)
                Map.RoadWeights = d.Map.RoadWeights;
            Map.Colors = Map.Colors ?? new Dictionary<string, string>();
            foreach (var kv in d.Map.Colors)
                if (!Map.Colors.ContainsKey(kv.Key)) Map.Colors[kv.Key] = kv.Value;
            Rooms.MaxLevel = Math.Max(1, Rooms.MaxLevel);
            MaxUnpaidMonths = Math.Max(1, MaxUnpaidMonths);
            Population.MortalTaxDivisor = Math.Max(1, Population.MortalTaxDivisor);
            if (Population.NpcTaxRate == null || Population.NpcTaxRate.Length != 2 || Population.NpcTaxRate[0] > Population.NpcTaxRate[1])
                Population.NpcTaxRate = d.Population.NpcTaxRate;
            Population.TaxMin = Math.Max(0f, Population.TaxMin);
            Population.TaxMax = Math.Max(Population.TaxMin, Population.TaxMax);
        }

        private static List<T> Pad<T>(List<T> list, List<T> defaults, int count)
        {
            var rs = (list ?? new List<T>()).Take(count).ToList();
            while (rs.Count < count) rs.Add(defaults[rs.Count]);
            return rs;
        }

        private static int[] Size(int[] v, int[] d, int minW, int minH) =>
            v != null && v.Length == 2 ? new[] { Math.Max(minW, v[0]), Math.Max(minH, v[1]) } : d;

        /// <summary>"Atlas/Sprite" -> (atlas, sprite); null when empty.</summary>
        public static string[] SplitSprite(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            var i = s.IndexOf('/');
            return i <= 0 ? new[] { "TownCommon", s.Trim() } : new[] { s.Substring(0, i).Trim(), s.Substring(i + 1).Trim() };
        }

        /// <summary>"#RRGGBB" or "#RRGGBBAA" -> r, g, b, a (0-1); null when not a color.</summary>
        public static float[] ParseColor(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            s = s.TrimStart('#');
            if (s.Length != 6 && s.Length != 8) return null;
            try
            {
                var rs = new float[4];
                for (var i = 0; i < 4; i++)
                    rs[i] = i * 2 < s.Length ? Convert.ToInt32(s.Substring(i * 2, 2), 16) / 255f : 1f;
                return rs;
            }
            catch (FormatException) { return null; }
        }
    }

    public partial class EstateEvent
    {
        /// <summary>The settings (ModConf/estate_configs.json); the defaults until the mod loads it.</summary>
        public static EstateConfig Conf { get; private set; } = EstateConfig.Parse(null);

        /// <summary>Loads ModConf/estate_configs.json (a broken file: the defaults, and a line in the log).</summary>
        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            try
            {
                SetConf(EstateConfig.Parse(ConfHelper.ReadConfData(modId, EstateConfig.FILE)));
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"EstateEvent: {EstateConfig.FILE} not read, defaults used: {ex.Message}");
                SetConf(EstateConfig.Parse(null));
            }
        }

        /// <summary>Uses a config (unit tests, the loader).</summary>
        public static void SetConf(EstateConfig conf)
        {
            Conf = conf ?? EstateConfig.Parse(null);
            _districts = null;
        }
    }
}
