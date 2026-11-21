using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Mine veins and herb valleys (mỏ khoáng, thung lũng dược liệu): a few rich cells in every area, near its towns (2 of each
    /// an area, fixed for the save). Their cells are rich in ores / herbs for the game's own gathering. When the player
    /// stands on one, a drama offers to stay 1, 3, 5 or 10 years (a seclusion, SeclusionEvent.Kind.MineVein / HerbValley):
    /// at the end the ores / herbs of the area's grade (2 a month in total, more for a higher craft rank) and their Mining /
    /// Herbs exp. The spirit sense's scan tells where the nearest one is.
    /// </summary>
    public partial class ArtisanEvent
    {
        public enum SpotKind { Mine = 1, Herb = 2 }

        public class Spot
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Area { get; set; }
            public int Kind { get; set; }
            public int LastOfferMonth { get; set; } = -999;
            public Vector2Int Point() => new Vector2Int(X, Y);
        }

        /// <summary>The spots of the save (made once, at load).</summary>
        public List<Spot> Spots { get; set; } = new List<Spot>();

        public const int SPOTS_AN_AREA = 2;           // of each kind
        public const int SPOT_MIN_RANGE = 4, SPOT_MAX_RANGE = 12;   // cells from a town of the area
        public const int SPOT_RICHNESS = 9;           // least max of herbs / ores of a spot cell (the game's cells hold 0..9); kept full every month
        public const int SPOT_COOLDOWN = 12;          // months before a spot offers again
        public static readonly int[] SPOT_YEARS = { 1, 3, 5, 10 };
        public const int SPOT_ITEMS_A_MONTH = 2;      // items a month of a stay, +25 % a craft rank

        public static Craft SpotCraft(SpotKind k) => k == SpotKind.Mine ? Craft.Mining : Craft.Herbal;
        /// <summary>Items of a stay: 2 a month, +25 % a rank of the craft.</summary>
        public static int SpotItems(int months, int rank) => (int)Math.Round(SPOT_ITEMS_A_MONTH * Math.Max(0, months) * (1 + 0.25 * Math.Max(0, rank)));
        /// <summary>Direction (0-7: N, NE, E, SE, S, SW, W, NW) from a point to another (y grows to the north).</summary>
        public static int Direction(Vector2Int from, Vector2Int to) => Direction(from.x, from.y, to.x, to.y);
        public static int Direction(int fx, int fy, int tx, int ty)
        {
            var a = Math.Atan2(ty - fy, tx - fx) * 180 / Math.PI;   // 0 = east, 90 = north
            var octant = (int)Math.Round(((90 - a) % 360 + 360) % 360 / 45.0) % 8;
            return octant;
        }

        /// <summary>Makes the spots of every area near its towns (deterministic for a save), the cells made rich.</summary>
        public static void MakeSpots()
        {
            if (!Enabled || Instance.Spots.Count > 0 || ModMain.Store?.Towns == null)
                return;
            var used = new HashSet<string>();
            foreach (var area in ModMain.Store.Towns.Where(x => x != null).GroupBy(x => x.gridData.areaBaseID).OrderBy(x => x.Key))
            {
                var towns = area.OrderBy(x => x.buildData.id, StringComparer.Ordinal).ToList();
                var rnd = new System.Random(StableSeed(towns[0].buildData.id) + area.Key);
                foreach (var kind in new[] { SpotKind.Mine, SpotKind.Herb })
                {
                    var made = 0;
                    for (var tries = 0; tries < 200 && made < SPOTS_AN_AREA; tries++)
                    {
                        var t = towns[rnd.Next(towns.Count)].GetOrigiPoint();
                        var r = rnd.Next(SPOT_MIN_RANGE, SPOT_MAX_RANGE + 1);
                        var p = new Vector2Int(t.x + rnd.Next(-r, r + 1), t.y + rnd.Next(-r, r + 1));
                        if (Math.Abs(p.x - t.x) < SPOT_MIN_RANGE && Math.Abs(p.y - t.y) < SPOT_MIN_RANGE) continue;
                        if (!WUnitHelper.IsValidPoint(p) || used.Contains($"{p.x},{p.y}")) continue;
                        var gd = g.data.grid.GetGridData(p);
                        if (gd == null || gd.IsBuild() || gd.isBlock || gd.areaBaseID != area.Key || g.world.build.GetBuild(p) != null) continue;
                        used.Add($"{p.x},{p.y}");
                        Instance.Spots.Add(new Spot { X = p.x, Y = p.y, Area = area.Key, Kind = (int)kind });
                        made++;
                    }
                }
            }
            Enrich();
        }

        private static int StableSeed(string s) { unchecked { var h = 23; foreach (var ch in s ?? "") h = h * 31 + ch; return h & 0x7fffffff; } }

        /// <summary>The spot cells are rich in their resource (the game's gathering finds more there). Yearly too.</summary>
        public static void Enrich()
        {
            foreach (var s in Instance.Spots)
            {
                DataGrid.GridData gd = null;
                try { gd = g.data.grid.GetGridData(s.Point()); } catch { }
                if (gd == null) continue;
                // the game's scale is small (a cell holds 0..9): a spot is full (its max, at least SPOT_RICHNESS)
                if (s.Kind == (int)SpotKind.Mine) { if (gd.mineralMax < SPOT_RICHNESS) gd.mineralMax = SPOT_RICHNESS; gd.SetMineralValue(gd.mineralMax); }
                else { if (gd.medicinalMax < SPOT_RICHNESS) gd.medicinalMax = SPOT_RICHNESS; gd.SetMedicinalValue(gd.medicinalMax); }
            }
        }

        public static Spot SpotAt(Vector2Int p) => !Enabled ? null : Instance.Spots.FirstOrDefault(s => Math.Abs(s.X - p.x) <= 1 && Math.Abs(s.Y - p.y) <= 1);
        public static Spot NearestSpot(Vector2Int p, SpotKind? kind = null) => !Enabled ? null :
            Instance.Spots.Where(s => kind == null || s.Kind == (int)kind).OrderBy(s => (s.X - p.x) * (s.X - p.x) + (s.Y - p.y) * (s.Y - p.y)).FirstOrDefault();
        public static string SpotName(Spot s) => GameTool.LS(s.Kind == (int)SpotKind.Mine ? "artisan_spot_mine" : "artisan_spot_herb");

        /// <summary>The player on a spot (not offered in the last SPOT_COOLDOWN months): the drama of the stay.</summary>
        private static void CheckSpot()
        {
            if (SeclusionEvent.IsSecluded || SeclusionEvent.Instance == null || (TimeSkipEvent.Instance?.IsProcessing ?? false) || _open.Count > 0 || Blocking(_uis) || DramaHelper.GetOpenDrama() != null)
                return;
            var s = SpotAt(Player.GetUnitPos());
            if (s == null || GameHelper.GetGameTotalMonth() - s.LastOfferMonth < SPOT_COOLDOWN)
                return;
            OfferSpot(s);
        }

        /// <summary>The drama of a spot: stay 1, 3, 5 or 10 years (a seclusion) or leave.</summary>
        public static void OfferSpot(Spot s)
        {
            s.LastOfferMonth = GameHelper.GetGameTotalMonth();
            var craft = SpotCraft((SpotKind)s.Kind);
            var rank = Rank(Player, craft);
            var life = Player.GetProperty<int>(UnitPropertyEnum.Life) - Player.GetProperty<int>(UnitPropertyEnum.Age);
            var opts = SPOT_YEARS.Select(y => string.Format(GameTool.LS("artisan_spot_o"), y, SpotItems(y * 12, rank))).ToList();
            opts.Add(GameTool.LS("artisan_spot_no"));
            var text = string.Format(GameTool.LS(s.Kind == (int)SpotKind.Mine ? "artisan_spot_mine_text" : "artisan_spot_herb_text"), s.Area, life / 12);
            DramaHelper.OpenDrama2(text, opts, x =>
            {
                var i = x.id % 10;
                if (i < 0 || i >= SPOT_YEARS.Length || SeclusionEvent.Instance == null || SeclusionEvent.IsSecluded) return;
                SeclusionEvent.Instance.Start(s.Kind == (int)SpotKind.Mine ? SeclusionEvent.Kind.MineVein : SeclusionEvent.Kind.HerbValley, SPOT_YEARS[i] * 12, s.Area);
            }, s.Kind == (int)SpotKind.Mine ? "dixiamushi" : "xingxingqiyu");
        }

        /// <summary>The end of a stay at a spot (SeclusionEvent.Reward): the items of the area's grade and the craft exp. Returns the text.</summary>
        public static string SpotReward(SpotKind kind, int area, int months)
        {
            var craft = SpotCraft(kind);
            var n = SpotItems(months, Rank(Player, craft));
            var grade = Math.Max(1, Math.Min(10, area));
            var got = new Dictionary<int, int>();
            for (var i = 0; i < n; i++)
            {
                var conf = EncounterEvent.PickItem(kind == SpotKind.Mine ? (Func<ConfItemPropsItem, bool>)(x => (x.IsMineItem()?.Length ?? 0) > 0) : x => x.IsHerbItem() != null, grade);
                if (conf == null) break;
                got[conf.id] = (got.TryGetValue(conf.id, out var c) ? c : 0) + 1;
            }
            long exp = 0;
            foreach (var x in got)
            {
                Player.AddUnitProp(x.Key, x.Value);
                exp += Crafted(Player, craft, x.Key, x.Value, 100f);
            }
            RefreshBag();
            TaoistLogEvent.Log("log_artisan_spot", GameTool.LS(kind == SpotKind.Mine ? "artisan_spot_mine" : "artisan_spot_herb"), got.Values.Sum(), exp);
            return string.Format(GameTool.LS(kind == SpotKind.Mine ? "secl_res_MineVein" : "secl_res_HerbValley"), got.Values.Sum(),
                string.Join(", ", got.OrderByDescending(x => x.Value).Take(4).Select(x => $"{GameTool.LS(g.conf.itemProps.GetItem(x.Key).name)} x{x.Value}")), exp);
        }
    }
}
