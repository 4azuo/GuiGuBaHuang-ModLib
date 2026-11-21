using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// The map of a town, made once from the seed of the town (pure code, unit tests): a style (crossroads, riverside,
    /// lakeside, ring road, hillside), winding main roads, the town master's palace near the middle, alleys so that every
    /// plot is near a road, then the districts by the distance to the palace and to the main roads.
    /// A char a cell: M market, N noble, R residential, C craftsmen, O outskirts (plots), P palace, # main road,
    /// B bridge (main road over water), = alley, ~ water, ^ woods.
    /// </summary>
    public partial class EstateEvent
    {
        /// <summary>Version of the map generator: an estate of another version is made again.</summary>
        public const int MAP_VERSION = 4;
        /// <summary>Every plot is at most this many cells from a road.</summary>
        public static int RoadReach => EstateEvent.Conf.Map.RoadReach;

        /// <summary>Landscape of a town, any mix: a river, a lake, woods outside the town (plain: none).</summary>
        [Flags] public enum Terrain { Plain = 0, River = 1, Lake = 2, Woods = 4 }
        /// <summary>Main roads of a town.</summary>
        public enum RoadLayout { Crossroads, TCrossing, RingRoad, TwoStreets }
        public const int TERRAIN_COUNT = 8;
        public const int ROAD_COUNT = 4;
        /// <summary>Styles: landscape x road layout (32; with the palace in the middle or off it, 64 kinds of maps).</summary>
        public const int STYLE_COUNT = TERRAIN_COUNT * ROAD_COUNT;
        public static int StyleOf(Terrain t, RoadLayout r) => (int)t * ROAD_COUNT + (int)r;
        public static Terrain TerrainOf(int style) => (Terrain)(Math.Max(0, style) / ROAD_COUNT % TERRAIN_COUNT);
        public static RoadLayout RoadsOf(int style) => (RoadLayout)(Math.Max(0, style) % ROAD_COUNT);

        public const char FREE = '.';
        public const char MAIN_ROAD = '#';
        public const char BRIDGE = 'B';
        public const char ALLEY = '=';
        public const char WATER = '~';
        public const char WOODS = '^';
        public const char PALACE = 'P';

        public static (int w, int h) MapSize(bool city) { var s = city ? Conf.Map.CitySize : Conf.Map.TownSize; return (s[0], s[1]); }
        public static (int w, int h) PalaceSize(bool city) { var s = city ? Conf.Map.CityPalace : Conf.Map.TownPalace; return (s[0], s[1]); }

        /// <summary>An index by the weights of the config.</summary>
        private static int Pick(Random rng, float[] weights)
        {
            var r = rng.NextDouble() * weights.Sum(x => Math.Max(0, x));
            for (var i = 0; i < weights.Length; i++)
            {
                r -= Math.Max(0, weights[i]);
                if (r < 0) return i;
            }
            return 0;
        }

        public static bool IsRoad(char c) => c == MAIN_ROAD || c == BRIDGE || c == ALLEY;
        public static bool IsMainRoad(char c) => c == MAIN_ROAD || c == BRIDGE;

        /// <summary>Scrambles a seed: System.Random gives close first numbers for close seeds (0, 1, 2...).</summary>
        public static int Mix(int seed)
        {
            unchecked
            {
                var h = (uint)seed * 0x9E3779B1u;
                h ^= h >> 15; h *= 0x85EBCA77u; h ^= h >> 13;
                return (int)(h & 0x7fffffff);
            }
        }

        /// <summary>A seed that does not change between runs (string.GetHashCode may).</summary>
        public static int StableSeed(string s)
        {
            unchecked
            {
                var h = (int)2166136261;
                foreach (var ch in s ?? string.Empty)
                    h = (h ^ ch) * 16777619;
                return h & 0x7fffffff;
            }
        }

        /// <summary>
        /// Makes the map of a town (the same map for the same seed): a terrain and a road layout of the config weights,
        /// the palace in the middle or off it, then the random details (bends of the roads, alleys, district borders).
        /// </summary>
        public static string[] GenerateMap(bool city, int seed, out int style)
        {
            var rng = new Random(Mix(seed));
            var terrain = Terrain.Plain;
            if (rng.NextDouble() < Conf.Map.RiverChance) terrain |= Terrain.River;
            if (rng.NextDouble() < Conf.Map.LakeChance) terrain |= Terrain.Lake;
            if (rng.NextDouble() < Conf.Map.WoodsChance) terrain |= Terrain.Woods;
            var roads = (RoadLayout)Pick(rng, Conf.Map.RoadWeights);
            style = StyleOf(terrain, roads);
            var (w, h) = MapSize(city);
            var m = new char[h, w];
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                    m[y, x] = FREE;
            var cx = w / 2 + rng.Next(-2, 3);
            var cy = h / 2 + rng.Next(-1, 2);

            // woods and river first: the roads cross them (bridges); the lake after the roads, around them
            if (terrain.HasFlag(Terrain.Woods)) Woods(m, rng);
            if (terrain.HasFlag(Terrain.River)) River(m, rng, cx, city);

            // the palace in the middle, or off it (not inside a ring road)
            var px0 = cx;
            if (roads != RoadLayout.RingRoad && rng.Next(2) == 0)
                px0 = cx + (rng.Next(2) == 0 ? -1 : 1) * Math.Max(3, w / 5);

            // main roads
            if (roads == RoadLayout.RingRoad)
            {
                int rw = city ? 6 : 5, rh = city ? 4 : 3;
                for (var x = cx - rw; x <= cx + rw; x++) { Road(m, x, cy - rh); Road(m, x, cy + rh); }
                for (var y = cy - rh; y <= cy + rh; y++) { Road(m, cx - rw, y); Road(m, cx + rw, y); }
                WalkH(m, rng, 0, cx - rw, cy + rng.Next(-1, 2), cy - rh + 1, cy + rh - 1);
                WalkH(m, rng, cx + rw, w - 1, cy + rng.Next(-1, 2), cy - rh + 1, cy + rh - 1);
                WalkV(m, rng, 0, cy - rh, cx + rng.Next(-2, 3), cx - rw + 1, cx + rw - 1);
                WalkV(m, rng, cy + rh, h - 1, cx + rng.Next(-2, 3), cx - rw + 1, cx + rw - 1);
            }
            else if (roads == RoadLayout.TwoStreets)
            {
                // two streets across the town and one road joining them
                var gap = city ? 4 : 3;
                WalkH(m, rng, 0, w - 1, cy - gap, Math.Max(1, cy - gap - 1), cy - gap + 1);
                WalkH(m, rng, 0, w - 1, cy + gap, cy + gap - 1, Math.Min(h - 2, cy + gap + 1));
                WalkV(m, rng, 0, h - 1, cx, cx - 4, cx + 4);
            }
            else
            {
                WalkH(m, rng, 0, w - 1, cy, Math.Max(2, cy - 2), Math.Min(h - 3, cy + 2));
                // a T crossing: the second road stops at the first one
                if (roads == RoadLayout.TCrossing)
                {
                    if (rng.Next(2) == 0) WalkV(m, rng, 0, h - 1, cx, cx - 4, cx + 4, true);
                    else WalkV(m, rng, h - 1, 0, cx, cx - 4, cx + 4, true);
                }
                else WalkV(m, rng, 0, h - 1, cx, cx - 4, cx + 4);
                if (city)
                {
                    // a second street, away from the first (it meets the second road of a full crossroads)
                    var y2 = cy + (rng.Next(2) == 0 ? -6 : 6);
                    if (roads == RoadLayout.TCrossing) y2 = cy + (IsMainRoad(m[0, Math.Max(0, Math.Min(w - 1, cx))]) || IsMainRoad(m[1, Math.Max(0, Math.Min(w - 1, cx))]) ? -6 : 6);
                    WalkH(m, rng, 0, w - 1, y2, Math.Max(1, y2 - 2), Math.Min(h - 2, y2 + 2));
                }
            }

            ThinRoads(m);
            if (terrain.HasFlag(Terrain.Lake)) Lake(m, rng, city);
            var (px, py) = PlacePalace(m, px0, cy, PalaceSize(city));
            Alleys(m, rng);
            Districts(m, rng, roads == RoadLayout.RingRoad, px + PalaceSize(city).w / 2f - 0.5f, py + PalaceSize(city).h / 2f - 0.5f);

            var rows = new string[h];
            for (var y = 0; y < h; y++)
            {
                var cs = new char[w];
                for (var x = 0; x < w; x++) cs[x] = m[y, x];
                rows[y] = new string(cs);
            }
            return rows;
        }

        /// <summary>
        /// A town grown into a city: its map in the middle of a bigger one, unchanged; the main roads that left the old
        /// map run on to the new edge, so does a river; alleys and districts for the new ring only.
        /// </summary>
        public static string[] ExpandMap(string[] old, bool city, int seed, out int ox, out int oy)
        {
            var rng = new Random(Mix(seed + 1));
            var (w, h) = MapSize(city);
            int w0 = old[0].Length, h0 = old.Length;
            w = Math.Max(w, w0); h = Math.Max(h, h0);
            ox = (w - w0) / 2; oy = (h - h0) / 2;
            var m = new char[h, w];
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                    m[y, x] = FREE;
            for (var y = 0; y < h0; y++)
                for (var x = 0; x < w0; x++)
                    m[y + oy, x + ox] = old[y][x];
            // the woods of the old town are cleared for the city (an islet of a lake stays)
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                    if (m[y, x] == WOODS && !DIRS.Any(d => In(m, x + d.dx, y + d.dy) && m[y + d.dy, x + d.dx] == WATER))
                        m[y, x] = FREE;
            // water and main roads at the old edge go on to the new one
            for (var x = 0; x < w0; x++)
            {
                Extend(m, rng, old[0][x], x + ox, oy - 1, 0, -1, false);
                Extend(m, rng, old[h0 - 1][x], x + ox, oy + h0, h - 1, 1, false);
            }
            for (var y = 0; y < h0; y++)
            {
                Extend(m, rng, old[y][0], ox - 1, y + oy, 0, -1, true);
                Extend(m, rng, old[y][w0 - 1], ox + w0, y + oy, w - 1, 1, true);
            }
            Alleys(m, rng);
            var pc = new List<(int x, int y)>();
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                    if (m[y, x] == PALACE) pc.Add((x, y));
            float pcx = pc.Count > 0 ? (float)pc.Average(v => v.x) : w / 2f, pcy = pc.Count > 0 ? (float)pc.Average(v => v.y) : h / 2f;
            Districts(m, rng, false, pcx, pcy);
            var rows = new string[h];
            for (var y = 0; y < h; y++)
            {
                var cs = new char[w];
                for (var x = 0; x < w; x++) cs[x] = m[y, x];
                rows[y] = new string(cs);
            }
            return rows;
        }

        /// <summary>A main road or water of the old edge, straight on (from start to end, step dir) across the new ring.</summary>
        private static void Extend(char[,] m, Random rng, char c, int fixedOrX, int startOrY, int end, int dir, bool horizontal)
        {
            if (!IsMainRoad(c) && c != WATER)
                return;
            for (var i = horizontal ? fixedOrX : startOrY; dir > 0 ? i <= end : i >= end; i += dir)
            {
                int x = horizontal ? i : fixedOrX, y = horizontal ? startOrY : i;
                if (!In(m, x, y)) break;
                if (c == WATER) { if (m[y, x] == FREE) m[y, x] = WATER; else if (IsMainRoad(m[y, x])) m[y, x] = BRIDGE; }
                else Road(m, x, y);
            }
        }

        /// <summary>
        /// The estate of a town grown into a city: the old plots keep everything (owner, house, rooms, prosperity,
        /// district), moved by (ox, oy); the new ring gets new plots.
        /// </summary>
        public static TownEstate ExpandEstate(TownEstate old, string[] map, int style, int seed, int ox, int oy)
        {
            var e = MakeEstate(map, style, seed, null);
            e.Population = old.Population;
            e.LastMortalTax = old.LastMortalTax;
            e.PalaceLevel = old.PalaceLevel;
            var at = e.Plots.ToDictionary(p => (p.X, p.Y));
            var index = new Dictionary<int, int>();
            foreach (var o in old.Plots)
                if (at.TryGetValue((o.X + ox, o.Y + oy), out var n))
                {
                    index[o.Index] = n.Index;
                    n.District = o.District; n.Prosperity = o.Prosperity; n.Level = o.Level; n.OwnerId = o.OwnerId; n.Unpaid = o.Unpaid;
                    n.Rooms = o.Rooms ?? new Dictionary<int, int>(); n.Herbs = o.Herbs ?? new List<int>();
                    n.MainIndex = o.MainIndex;
                }
            foreach (var p in e.Plots.Where(x => x.MainIndex != null))
                p.MainIndex = index.TryGetValue(p.MainIndex.Value, out var i) ? i : (int?)null;
            return e;
        }

        /// <summary>
        /// No 2 x 2 square of main road: a corner of the square is given back to the plots when the main roads stay as
        /// joined as before (same number of pieces, the same cells on the edge of the map). A bridge stays.
        /// </summary>
        private static void ThinRoads(char[,] m)
        {
            int h = m.GetLength(0), w = m.GetLength(1);
            int Pieces()
            {
                var seen = new bool[h, w];
                var n = 0;
                for (var y = 0; y < h; y++)
                    for (var x = 0; x < w; x++)
                    {
                        if (seen[y, x] || !IsMainRoad(m[y, x])) continue;
                        n++;
                        var st = new Stack<(int x, int y)>();
                        st.Push((x, y)); seen[y, x] = true;
                        while (st.Count > 0)
                        {
                            var v = st.Pop();
                            foreach (var d in DIRS)
                            {
                                int nx = v.x + d.dx, ny = v.y + d.dy;
                                if (!In(m, nx, ny) || seen[ny, nx] || !IsMainRoad(m[ny, nx])) continue;
                                seen[ny, nx] = true; st.Push((nx, ny));
                            }
                        }
                    }
                return n;
            }
            var pieces = Pieces();
            for (var y = 0; y < h - 1; y++)
                for (var x = 0; x < w - 1; x++)
                {
                    var sq = new[] { (x, y), (x + 1, y), (x, y + 1), (x + 1, y + 1) };
                    if (!sq.All(c => IsMainRoad(m[c.Item2, c.Item1]))) continue;
                    foreach (var (cx0, cy0) in sq)
                    {
                        if (m[cy0, cx0] == BRIDGE) continue;
                        var keep = m[cy0, cx0];
                        m[cy0, cx0] = FREE;
                        if (Pieces() == pieces) break;
                        m[cy0, cx0] = keep;
                    }
                }
        }
        private static char At(char[,] m, int x, int y) => In(m, x, y) ? m[y, x] : FREE;

        private static bool IsWet(char[,] m, int x, int y) => In(m, x, y) && (m[y, x] == WATER || m[y, x] == BRIDGE);

        private static bool In(char[,] m, int x, int y) => y >= 0 && y < m.GetLength(0) && x >= 0 && x < m.GetLength(1);

        private static void Road(char[,] m, int x, int y)
        {
            if (!In(m, x, y) || m[y, x] == PALACE) return;
            m[y, x] = m[y, x] == WATER || m[y, x] == BRIDGE ? BRIDGE : MAIN_ROAD;
        }

        /// <summary>A winding main road from x0 to x1: it keeps a drift and turns now and then (4-connected).</summary>
        private static void WalkH(char[,] m, Random rng, int x0, int x1, int y, int yMin, int yMax)
        {
            y = Math.Max(yMin, Math.Min(yMax, y));
            var drift = rng.Next(2) == 0 ? -1 : 1;
            var straight = 0;
            for (var x = x0; x <= x1; x++)
            {
                Road(m, x, y);
                straight++;
                if (x == x1 || straight < 2 || rng.NextDouble() > 0.32) continue;
                // never bend at a crossing (no little squares of road)
                if (Enumerable.Range(-1, 3).Any(k => IsMainRoad(At(m, x + k, y - 1)) || IsMainRoad(At(m, x + k, y + 1)))) continue;
                if (rng.NextDouble() < 0.35) drift = -drift;
                var ny = y + drift;
                if (ny < yMin || ny > yMax) { drift = -drift; ny = y + drift; }
                if (ny < yMin || ny > yMax) continue;
                Road(m, x, ny);
                y = ny;
                straight = 0;
            }
        }

        /// <summary>A winding main road from y0 to y1 (either way); stopAtRoad: it ends on the first main road it meets.</summary>
        private static void WalkV(char[,] m, Random rng, int y0, int y1, int x, int xMin, int xMax, bool stopAtRoad = false)
        {
            x = Math.Max(xMin, Math.Min(xMax, x));
            var drift = rng.Next(2) == 0 ? -1 : 1;
            var straight = 0;
            var step = y1 >= y0 ? 1 : -1;
            for (var y = y0; y != y1 + step; y += step)
            {
                if (stopAtRoad && y != y0 && IsMainRoad(m[y, x]))
                    return;
                // never along a river: on water with water ahead, step aside to the nearest dry cell
                if (y != y0 && IsWet(m, x, y) && IsWet(m, x, y - step))
                    for (var k = 1; k <= 3; k++)
                    {
                        var side = new[] { x + drift * k, x - drift * k }.Where(sx => sx >= xMin && sx <= xMax && In(m, sx, y) && !IsWet(m, sx, y)).DefaultIfEmpty(-1).First();
                        if (side < 0) continue;
                        for (var px = Math.Min(x, side); px <= Math.Max(x, side); px++) Road(m, px, y - step);
                        x = side;
                        break;
                    }
                Road(m, x, y);
                straight++;
                if (y == y1 || straight < 2 || rng.NextDouble() > 0.38) continue;
                // never bend at a crossing (no little squares of road)
                if (Enumerable.Range(-1, 3).Any(k => IsMainRoad(At(m, x - 1, y + k)) || IsMainRoad(At(m, x + 1, y + k)))) continue;
                if (rng.NextDouble() < 0.35) drift = -drift;
                var nx = x + drift;
                if (nx < xMin || nx > xMax) { drift = -drift; nx = x + drift; }
                if (nx < xMin || nx > xMax) continue;
                Road(m, nx, y);
                x = nx;
                straight = 0;
            }
        }

        /// <summary>A winding river from the top to the bottom, on one side of the middle.</summary>
        private static void River(char[,] m, Random rng, int cx, bool city)
        {
            int h = m.GetLength(0), w = m.GetLength(1);
            var x = cx + (rng.Next(2) == 0 ? -1 : 1) * (city ? rng.Next(7, 10) : rng.Next(6, 8));
            var width = city ? 2 : 1;
            for (var y = 0; y < h; y++)
            {
                for (var k = 0; k < width; k++)
                    if (In(m, x + k, y)) m[y, x + k] = WATER;
                if (rng.NextDouble() < 0.4)
                {
                    var nx = Math.Max(1, Math.Min(w - 1 - width, x + (rng.Next(2) == 0 ? -1 : 1)));
                    for (var k = 0; k < width; k++)
                        if (In(m, nx + k, y)) m[y, nx + k] = WATER;
                    x = nx;
                }
            }
        }

        /// <summary>A lake in a corner (an ellipse with ragged shores).</summary>
        private static void Lake(char[,] m, Random rng, bool city)
        {
            int h = m.GetLength(0), w = m.GetLength(1);
            double rx = city ? 7 : 6, ry = city ? 5 : 4;
            bool Inside(int x, int y, int cx0, int cy0) => Math.Pow((x - cx0) / rx, 2) + Math.Pow((y - cy0) / ry, 2) <= 1.25;
            // the corner with the fewest main roads (made before the lake): the lake goes around them, a bank along them
            var corners = new[] { (3, 2), (w - 4, 2), (3, h - 3), (w - 4, h - 3) }.OrderBy(_ => rng.Next()).ToList();
            var (lx, ly) = corners.OrderBy(c => Enumerable.Range(0, h).Sum(y => Enumerable.Range(0, w).Count(x => IsMainRoad(m[y, x]) && Inside(x, y, c.Item1, c.Item2)))).First();
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    if (m[y, x] != FREE && m[y, x] != WOODS) continue;
                    if (DIRS.Any(dd => In(m, x + dd.dx, y + dd.dy) && IsRoad(m[y + dd.dy, x + dd.dx]))) continue;
                    var d = Math.Pow((x - lx) / rx, 2) + Math.Pow((y - ly) / ry, 2);
                    if (d <= 1 + (rng.NextDouble() - 0.5) * 0.5)
                        m[y, x] = WATER;
                }
            // an islet of trees in a big enough lake: the water cell most inside the lake
            var best = (x: -1, y: -1); var bestDepth = 0;
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    if (m[y, x] != WATER) continue;
                    var depth = 0;
                    while (depth < 4 && Enumerable.Range(-depth - 1, 2 * depth + 3).All(dy => Enumerable.Range(-depth - 1, 2 * depth + 3)
                        .All(dx => In(m, x + dx, y + dy) && m[y + dy, x + dx] == WATER))) depth++;
                    if (depth > bestDepth) { bestDepth = depth; best = (x, y); }
                }
            if (bestDepth >= 2)
            {
                m[best.y, best.x] = WOODS;
                if (city && bestDepth >= 3) m[best.y, best.x + 1] = WOODS;
            }
        }

        /// <summary>Woods along one edge of the town (1-3 cells deep).</summary>
        private static void Woods(char[,] m, Random rng)
        {
            int h = m.GetLength(0), w = m.GetLength(1);
            // one side, sometimes two (outside the town: the wall of a city runs along their inner edge)
            var sides = new List<int> { rng.Next(4) };
            if (rng.NextDouble() < 0.4) sides.Add((sides[0] + 1 + rng.Next(3)) % 4);
            foreach (var side in sides)
            {
                var len = side < 2 ? w : h;
                var depth = 2;
                for (var i = 0; i < len; i++)
                {
                    depth = Math.Max(1, Math.Min(3, depth + rng.Next(-1, 2)));
                    for (var d = 0; d < depth; d++)
                    {
                        int x = side == 0 || side == 1 ? i : side == 2 ? d : w - 1 - d;
                        int y = side == 0 ? d : side == 1 ? h - 1 - d : i;
                        m[y, x] = WOODS;
                    }
                }
            }
        }

        /// <summary>The palace on free cells near the middle, next to a main road when possible; returns its top left cell.</summary>
        private static (int x, int y) PlacePalace(char[,] m, int cx, int cy, (int w, int h) size)
        {
            int h = m.GetLength(0), w = m.GetLength(1);
            var best = (x: -1, y: -1);
            var bestScore = double.MaxValue;
            for (var y = 1; y + size.h <= h - 1; y++)
                for (var x = 1; x + size.w <= w - 1; x++)
                {
                    var ok = true;
                    var road = false;
                    for (var dy = -1; dy <= size.h && ok; dy++)
                        for (var dx = -1; dx <= size.w; dx++)
                        {
                            var inside = dy >= 0 && dy < size.h && dx >= 0 && dx < size.w;
                            var c = m[y + dy, x + dx];
                            if (inside && c != FREE) { ok = false; break; }
                            if (!inside && IsMainRoad(c) && (dx == -1 || dx == size.w) != (dy == -1 || dy == size.h)) road = true;
                        }
                    if (!ok) continue;
                    var score = Math.Abs(x + size.w / 2.0 - cx) * 1.3 + Math.Abs(y + size.h / 2.0 - cy) + (road ? 0 : 6);
                    if (score < bestScore) { bestScore = score; best = (x, y); }
                }
            if (best.x < 0)
                return (cx, cy);
            for (var dy = 0; dy < size.h; dy++)
                for (var dx = 0; dx < size.w; dx++)
                    m[best.y + dy, best.x + dx] = PALACE;
            return best;
        }

        private static readonly (int dx, int dy)[] DIRS = { (1, 0), (-1, 0), (0, 1), (0, -1) };

        /// <summary>
        /// Alleys: while a free cell is farther than RoadReach from a road, an alley is cut from the farthest one to the
        /// nearest road (a breadth search with shuffled directions, so the alleys bend). Free cells no road can reach
        /// (closed by water) become woods.
        /// </summary>
        private static void Alleys(char[,] m, Random rng)
        {
            int h = m.GetLength(0), w = m.GetLength(1);
            for (var guard = 0; guard < 400; guard++)
            {
                var dist = new int[h, w];
                var from = new (int x, int y)[h, w];
                var queue = new Queue<(int x, int y)>();
                for (var y = 0; y < h; y++)
                    for (var x = 0; x < w; x++)
                    {
                        dist[y, x] = IsRoad(m[y, x]) ? 0 : -1;
                        if (dist[y, x] == 0) queue.Enqueue((x, y));
                    }
                var dirs = DIRS.ToArray();
                while (queue.Count > 0)
                {
                    var v = queue.Dequeue();
                    Shuffle(dirs, rng);
                    foreach (var d in dirs)
                    {
                        int nx = v.x + d.dx, ny = v.y + d.dy;
                        if (!In(m, nx, ny) || dist[ny, nx] >= 0 || m[ny, nx] != FREE) continue;
                        dist[ny, nx] = dist[v.y, v.x] + 1;
                        from[ny, nx] = v;
                        queue.Enqueue((nx, ny));
                    }
                }
                var far = new List<(int x, int y)>();
                var max = RoadReach;
                var unreached = false;
                for (var y = 0; y < h; y++)
                    for (var x = 0; x < w; x++)
                    {
                        if (m[y, x] != FREE) continue;
                        if (dist[y, x] < 0) { unreached = true; continue; }
                        if (dist[y, x] > max) { max = dist[y, x]; far.Clear(); }
                        if (dist[y, x] == max && max > RoadReach) far.Add((x, y));
                    }
                if (far.Count == 0)
                {
                    if (unreached)
                        for (var y = 0; y < h; y++)
                            for (var x = 0; x < w; x++)
                                if (m[y, x] == FREE && dist[y, x] < 0) m[y, x] = WOODS;
                    return;
                }
                // cut from a cell next to the farthest one (the alley ends one cell before the edge of the block)
                var c = far[rng.Next(far.Count)];
                c = from[c.y, c.x];
                while (dist[c.y, c.x] > 0)
                {
                    m[c.y, c.x] = ALLEY;
                    c = from[c.y, c.x];
                }
            }
        }

        private static void Shuffle<T>(T[] a, Random rng)
        {
            for (var i = a.Length - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                var t = a[i]; a[i] = a[j]; a[j] = t;
            }
        }

        /// <summary>
        /// Districts of the free cells: noble around the palace, market along the main roads near the middle, outskirts
        /// far from it, craftsmen in a sector of the town and along the water, residential the rest.
        /// </summary>
        private static void Districts(char[,] m, Random rng, bool ring, float pcx, float pcy)
        {
            int h = m.GetLength(0), w = m.GetLength(1);
            var radius = Math.Sqrt(Math.Pow(w / 2.0 * 1.3, 2) + Math.Pow(h / 2.0, 2));
            var rN = (ring ? 0.22 : 0.16) + (rng.NextDouble() - 0.5) * 0.04;
            var rM = 0.50 + (rng.NextDouble() - 0.5) * 0.10;
            var rO = 0.74 + (rng.NextDouble() - 0.5) * 0.08;
            var a0 = rng.NextDouble() * Math.PI * 2;
            // a score a district gives to a cell: the cells of a missing district are the best ones (see the end)
            var score = new Dictionary<char, double[,]>();
            foreach (var k in "MNRCO") score[k] = new double[h, w];
            for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                {
                    if (m[y, x] != FREE) continue;
                    var d = Math.Sqrt(Math.Pow((x - pcx) * 1.3, 2) + Math.Pow(y - pcy, 2)) / radius;
                    bool nearMain = false, nearWater = false;
                    for (var dy = -1; dy <= 1; dy++)
                        for (var dx = -1; dx <= 1; dx++)
                        {
                            if (!In(m, x + dx, y + dy)) continue;
                            var c = m[y + dy, x + dx];
                            nearMain |= IsMainRoad(c);
                            nearWater |= c == WATER;
                        }
                    var angle = Math.Atan2(y - pcy, (x - pcx) * 1.3) - a0;
                    while (angle < 0) angle += Math.PI * 2;
                    char district;
                    if (d <= rN) district = 'N';
                    else if (nearMain && d <= rM) district = 'M';
                    else if (d >= rO) district = 'O';
                    else if (nearWater || angle < Math.PI * 2 / 3) district = 'C';
                    else district = 'R';
                    m[y, x] = district;
                    score['N'][y, x] = -d;
                    score['M'][y, x] = (nearMain ? 1 : 0) - d;
                    score['O'][y, x] = d;
                    score['C'][y, x] = (nearWater ? 1 : 0) - angle;
                    score['R'][y, x] = angle;
                }
            // a district missing on a small map: the best cells of the biggest district become it
            foreach (var k in "MNRCO")
            {
                var cells = new List<(int x, int y)>();
                var count = new Dictionary<char, int>();
                for (var y = 0; y < h; y++)
                    for (var x = 0; x < w; x++)
                        if (DISTRICTS.ContainsKey(m[y, x]))
                        {
                            cells.Add((x, y));
                            count[m[y, x]] = count.TryGetValue(m[y, x], out var n) ? n + 1 : 1;
                        }
                if (count.ContainsKey(k) || count.Count == 0)
                    continue;
                var biggest = count.OrderByDescending(v => v.Value).First().Key;
                foreach (var c in cells.Where(v => m[v.y, v.x] == biggest).OrderByDescending(v => score[k][v.y, v.x]).Take(6).ToList())
                    m[c.y, c.x] = k;
            }
        }
    }
}
