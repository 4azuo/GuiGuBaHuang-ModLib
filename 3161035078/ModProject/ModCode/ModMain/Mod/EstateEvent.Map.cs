using MOD_nE7UL2.Helper;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    /// <summary>The town map window: districts, roads, the palace of the town master, the plots and their houses.</summary>
    public partial class EstateEvent
    {
        // pictures, colors and sizes: ModConf/estate_configs.json (EstateConfig); the pictures are listed in
        // docs/GameAssets (Buildings tab; scripts/extract-game-assets.py --buildings)

        /// <summary>A color of the config (Unity type: only used by the windows).</summary>
        private static Color ConfColor(string key)
        {
            var c = Conf.Map.Colors.TryGetValue(key, out var s) ? EstateConfig.ParseColor(s) : null;
            return c == null ? new Color(0.8f, 0.8f, 0.7f, 0.5f) : new Color(c[0], c[1], c[2], c[3]);
        }

        /// <summary>
        /// A color of the config laid on the ground and made opaque: rectangles of it that overlap (roads, water) keep one
        /// shade instead of getting darker where they cross.
        /// </summary>
        private static Color OnGround(string key)
        {
            var c = ConfColor(key);
            var g0 = ConfColor("Ground");
            var a = c.a;
            return new Color(c.r * a + g0.r * (1 - a), c.g * a + g0.g * (1 - a), c.b * a + g0.b * (1 - a), Math.Max(g0.a, 0.95f));
        }

        private static Color DistrictColor(string code)
        {
            var c = DistrictOf(code).Color;
            return new Color(c[0], c[1], c[2], c[3]);
        }

        public static string[] HouseSprite(int level) => EstateConfig.SplitSprite(HouseConf(level).Sprite);
        public static string[] PalaceSprite(int level) => EstateConfig.SplitSprite(Conf.Palace[Math.Max(1, Math.Min(PALACE_MAX, level)) - 1]);

        public static string[] SpriteOf(Plot p)
        {
            if (p.Level <= 0)
                return null;
            if (p.OwnerId != null)
                return HouseSprite(p.Level);
            // a mortal house grown above its district's level: the house picture of its level
            if (p.Level > DistrictOf(p.District).MortalLevel)
                return HouseSprite(p.Level);
            var list = DistrictOf(p.District).Sprites;
            return list.Length == 0 ? null : list[(p.Index * 7 + p.Y) % list.Length];
        }

        /// <summary>The style of a town map: its landscape (plain, or river / lake / woods) and its main roads.</summary>
        public static string StyleName(TownEstate e)
        {
            var t = TerrainOf(e.Style);
            var parts = new List<string>();
            if (t == Terrain.Plain) parts.Add(GameTool.LS("estate_terrain_0"));
            if (t.HasFlag(Terrain.River)) parts.Add(GameTool.LS("estate_terrain_1"));
            if (t.HasFlag(Terrain.Lake)) parts.Add(GameTool.LS("estate_terrain_2"));
            if (t.HasFlag(Terrain.Woods)) parts.Add(GameTool.LS("estate_terrain_3"));
            parts.Add(GameTool.LS($"estate_roads_{(int)RoadsOf(e.Style)}"));
            return string.Join(" · ", parts);
        }

        /// <summary>The house a plot belongs to (itself, or the house that took it).</summary>
        public static Plot MainOf(TownEstate e, Plot p) =>
            p.MainIndex is int i && p.OwnerId != null && i >= 0 && i < e.Plots.Count && e.Plots[i].OwnerId == p.OwnerId ? e.Plots[i] : p;

        private static WorldUnitBase Player => g.world.playerUnit;

        /// <summary>An image without sprite (a colored rectangle) that does not catch the clicks.</summary>
        private static UIItemImage Rect(UICustom1 ui, float x, float y, float w, float h, Color c)
        {
            var img = ui.AddImage(x, y, null);
            img.Item.color = c;
            img.Size(w, h);
            img.Item.raycastTarget = false;
            return img;
        }

        /// <summary>A game picture fitted into a box (keeps its ratio); also for other windows (town management).</summary>
        public static void Picture(UICustomBase ui, string[] sprite, float x, float y, float w, float h)
        {
            var s = sprite == null ? null : GameSprite(sprite[0], sprite[1]);
            if (s == null)
                return;
            var k = Math.Min(w / s.rect.width, h / s.rect.height);
            var img = ui.AddImage(x, y, s);
            img.Size(s.rect.width * k, s.rect.height * k);
            img.Item.raycastTarget = false;
        }

        private static void Clickable(UIItemImage img, Action act)
        {
            img.Item.raycastTarget = true;
            var btn = img.Item.gameObject.AddComponent<Button>();
            btn.onClick.AddListener(ActionHelper.TracedUnityAction(act));
        }

        /// <summary>The town map: districts between roads, the palace, the plots and their houses.</summary>
        public static void OpenPlots(MapBuildTown town)
        {
            var e = Get(town);
            if (e == null)
                return;
            var ui = new UICustom1(string.Format(GameTool.LS("estate_title"), town.name));
            var row = ui.FirstRow;
            var built = e.Plots.Count(x => x.Level > 0 || x.OwnerId != null);
            ui.AddText(ui.MidCol, row++, StyleName(e) + " · " + string.Format(GameTool.LS("estate_population"), e.Population, Target(town, e), e.LastMortalTax, built, e.Plots.Count)).Format(null, 14);
            var mine = HouseOf(town, Player);
            ui.AddText(ui.MidCol, row++, mine == null
                ? GameTool.LS("estate_hint")
                : string.Format(GameTool.LS("estate_mine"), HouseName(mine.Level), DistrictName(mine.District), HouseTax(town, mine))).Format(mine == null ? Color.black : new Color(0.55f, 0.35f, 0f), 14);
            // the cultivator clans of the town (ClanEvent): top left corner
            if (ClanEvent.Conf.Enabled)
                ui.AddButton(ui.Columns[ui.FirstCol] + 120f, ui.Rows[ui.FirstRow + 1], () => ClanEvent.OpenClans(town),
                    string.Format(GameTool.LS("clan_button"), ClanEvent.Heads(town).Count)).Size(200, 30);
            // the city wall: its level, the master strengthens it (top right corner)
            if (town.IsCity())
            {
                var wx = ui.Columns[ui.LastCol] - 170f;
                var wy = ui.Rows[ui.FirstRow + 1];
                var defense = WallDefensePercent(e.WallLevel);
                if (MapBuildPropertyEvent.IsTownMaster(town, Player) && e.WallLevel < Conf.Map.WallMax)
                {
                    var cost = WallCost(town, e);
                    ui.AddButton(wx, wy, () => InfoDialog.Msg(PalaceName(e.PalaceLevel), string.Format(GameTool.LS("estate_wall_confirm"), e.WallLevel + 1, cost,
                            MapBuildPropertyEvent.GetBuildProperty(town), WallDefensePercent(e.WallLevel + 1)), MsgBoxButtonEnum.YesNo, () =>
                        {
                            var rs = UpgradeWall(town, Player);
                            OpenPlots(town);
                            if (rs != BuildResult.Ok) InfoDialog.Msg(PalaceName(e.PalaceLevel), string.Format(GameTool.LS("estate_palace_budget"), cost));
                        }), string.Format(GameTool.LS("estate_wall_btn"), e.WallLevel, defense)).Size(330, 30);
                }
                else
                    ui.AddText(wx, wy, string.Format(GameTool.LS("estate_wall_line"), e.WallLevel, defense)).Format(new Color(0.35f, 0.25f, 0.15f), 13);
            }

            // the map fills the window between the header and the legend
            var map = e.Map;
            int rows = map.Length, cols = map[0].Length;
            var left = ui.Columns[ui.FirstCol] + 10f;
            var right = ui.Columns[ui.LastCol] - 10f;
            var top = ui.Rows[row] - 6f;
            var bottom = ui.Rows[ui.LastRow - 2];
            var ch = Math.Min((top - bottom) / rows, (right - left) / cols / 1.3f);
            var cw = ch * 1.3f;
            var x0 = ui.Columns[ui.MidCol] - cols * cw / 2f;
            float X(float cx) => x0 + (cx + 0.5f) * cw;
            float Y(float cy) => top - (cy + 0.5f) * ch;

            // the ground of the town, then water and woods (a cell a little bigger: no seams between them)
            Rect(ui, ui.Columns[ui.MidCol], top - rows * ch / 2f, cols * cw + 8f, rows * ch + 8f, ConfColor("Ground"));
            // the wall of the town (the gates: the main roads drawn over it, see below)
            // the wall of a city (a town has none): along the border of the streets, outside them the woods; none on water
            var walled = Conf.Map.Wall && town.IsCity() && e.WallLevel > 0;
            if (walled)
            {
                var wall = ConfColor("Wall");
                var th = 2f + e.WallLevel;
                bool Outside(int x, int y) => x < 0 || y < 0 || x >= cols || y >= rows || map[y][x] == WOODS;
                // every street cell but the main roads (their gates) is inside the wall, the alleys too
                bool Inner(int x, int y) => !Outside(x, y) && map[y][x] != WATER && map[y][x] != BRIDGE && !IsMainRoad(map[y][x]);
                for (var y = 0; y < rows; y++)
                    for (var x = 0; x < cols; x++)
                    {
                        if (!Inner(x, y)) continue;
                        if (Outside(x, y - 1)) Rect(ui, X(x), Y(y) + ch / 2f, cw + th, th, wall);
                        if (Outside(x, y + 1)) Rect(ui, X(x), Y(y) - ch / 2f, cw + th, th, wall);
                        if (Outside(x - 1, y)) Rect(ui, X(x) - cw / 2f, Y(y), th, ch + th, wall);
                        if (Outside(x + 1, y)) Rect(ui, X(x) + cw / 2f, Y(y), th, ch + th, wall);
                    }
            }            var wave = ConfColor("Wave");
            for (var y = 0; y < rows; y++)
                for (var x = 0; x < cols; x++)
                {
                    var c = map[y][x];
                    if (c == WATER || c == BRIDGE)
                    {
                        Rect(ui, X(x), Y(y), cw + 1f, ch + 1f, OnGround("Water"));
                        // two short ripples a cell, moved by the cell
                        var k = Hash(x, y);
                        Rect(ui, X(x) - cw * 0.15f + (k % 5) - 2f, Y(y) + ch * 0.18f, cw * 0.35f, 1.5f, wave);
                        Rect(ui, X(x) + cw * 0.18f - (k % 3), Y(y) - ch * 0.2f, cw * 0.3f, 1.5f, wave);
                    }
                    if (c == WOODS) Rect(ui, X(x), Y(y), cw + 1f, ch + 1f, OnGround("Woods"));
                }
            // roads: a band through the middle of the cell and an arm to every road next to it (main roads run on to the
            // edge of the map); an arm between a main road and an alley has the width of the alley. Two layers: the alley
            // pieces first, the main road pieces over them (an alley meets the main road at its side, never across it)
            float RoadWidth(char c) => ch * (IsMainRoad(c) ? Conf.Map.MainRoadWidth : Conf.Map.AlleyWidth);
            var alleys = new List<(float x, float y, float w, float h)>();
            var mains = new List<(float x, float y, float w, float h)>();
            for (var y = 0; y < rows; y++)
                for (var x = 0; x < cols; x++)
                {
                    var c = map[y][x];
                    if (!IsRoad(c)) continue;
                    var w = RoadWidth(c);
                    (IsMainRoad(c) ? mains : alleys).Add((X(x), Y(y), w, w));
                    foreach (var (dx, dy) in DIRS)
                    {
                        int nx = x + dx, ny = y + dy;
                        var outside = ny < 0 || ny >= rows || nx < 0 || nx >= cols;
                        var n = outside ? c : map[ny][nx];
                        if (!(outside ? IsMainRoad(c) : IsRoad(n))) continue;
                        var aw = Math.Min(w, RoadWidth(n));
                        var layer = IsMainRoad(c) && IsMainRoad(n) ? mains : alleys;
                        if (dx != 0) layer.Add((X(x) + dx * cw / 4f, Y(y), cw / 2f + 1f, aw));
                        else layer.Add((X(x), Y(y) - dy * ch / 4f, aw, ch / 2f + 1f));
                    }
                }
            var alleyColor = OnGround("Alley");
            var mainColor = OnGround("MainRoad");
            foreach (var r in alleys) Rect(ui, r.x, r.y, r.w, r.h, alleyColor);
            foreach (var r in mains) Rect(ui, r.x, r.y, r.w, r.h, mainColor);

            // decoration: rails of the bridges, gates of the wall, trees of the woods, people on the main roads
            var rail = ConfColor("Rail");
            var trees = Conf.Map.Trees.Select(EstateConfig.SplitSprite).Where(s => s != null).ToArray();
            var people = Conf.Map.People.Select(EstateConfig.SplitSprite).Where(s => s != null).ToArray();
            var walk = new System.Random(); // the people walk elsewhere every time the map opens
            for (var y = 0; y < rows; y++)
                for (var x = 0; x < cols; x++)
                {
                    var c = map[y][x];
                    var k = Hash(x, y);
                    if (c == BRIDGE)
                    {
                        // rails on a straight piece of bridge only (a bend has no side to put them on)
                        bool Main(int nx, int ny) => nx < 0 || ny < 0 || nx >= cols || ny >= rows || IsRoad(map[ny][nx]);
                        bool lr = Main(x - 1, y) && Main(x + 1, y), ud = Main(x, y - 1) && Main(x, y + 1);
                        var bw = RoadWidth(c) / 2f + 1.5f;
                        if (lr && !Main(x, y - 1) && !Main(x, y + 1)) { Rect(ui, X(x), Y(y) + bw, cw + 1f, 2f, rail); Rect(ui, X(x), Y(y) - bw, cw + 1f, 2f, rail); }
                        else if (ud && !Main(x - 1, y) && !Main(x + 1, y)) { Rect(ui, X(x) - bw, Y(y), 2f, ch + 1f, rail); Rect(ui, X(x) + bw, Y(y), 2f, ch + 1f, rail); }
                    }
                    if (walled && IsMainRoad(c) && (x == 0 || y == 0 || x == cols - 1 || y == rows - 1))
                        Gate(ui, X(x), Y(y), x == 0 ? -1 : x == cols - 1 ? 1 : 0, y == 0 ? -1 : y == rows - 1 ? 1 : 0, cw, ch, RoadWidth(c), mainColor, rail);
                    if (c == WOODS && trees.Length > 0)
                        Picture(ui, trees[k % trees.Length], X(x) + (k % 7 - 3), Y(y) + (k % 5 - 2), cw * 1.15f, ch * 1.25f);
                    if (IsMainRoad(c) && c != BRIDGE && people.Length > 0 && walk.NextDouble() < Conf.Map.PeopleChance)
                        Picture(ui, people[walk.Next(people.Length)], X(x) + (float)(walk.NextDouble() - 0.5) * cw * 0.5f, Y(y) + ch * 0.1f, cw * 0.5f, ch * 0.85f);
                }

            // plots: the ground of every plot (a click), the mortals' houses, then the owners' houses over all their plots
            var me = Player.GetUnitId();
            foreach (var p in e.Plots)
            {
                var plot = p;
                var isMine = p.OwnerId != null && p.OwnerId == me;
                var ground = Rect(ui, X(p.X), Y(p.Y), cw - 2f, ch - 2f, isMine ? ConfColor("Mine") : p.OwnerId != null ? ConfColor("Owned") : DistrictColor(p.District));
                Clickable(ground, () => ClickPlot(town, MainOf(e, plot)));
                if (p.OwnerId == null)
                    Picture(ui, SpriteOf(p), X(p.X), Y(p.Y), cw - 3f, ch - 3f);
            }
            foreach (var p in e.Plots.Where(x => x.OwnerId != null && x.Level > 0))
            {
                var parts = PartsOf(e, p);
                float minX = parts.Min(v => v.X), maxX = parts.Max(v => v.X), minY = parts.Min(v => v.Y), maxY = parts.Max(v => v.Y);
                Picture(ui, SpriteOf(p), X((minX + maxX) / 2f), Y((minY + maxY) / 2f), (maxX - minX + 1) * cw - 4f, (maxY - minY + 1) * ch - 4f);
            }

            // the palace of the town master (one picture over its cells)
            var pc = Cells(map, PALACE);
            if (pc.Count > 0)
            {
                float px = (pc.Min(v => v.x) + pc.Max(v => v.x)) / 2f, py = (pc.Min(v => v.y) + pc.Max(v => v.y)) / 2f;
                float pw = (pc.Max(v => v.x) - pc.Min(v => v.x) + 1) * cw, ph = (pc.Max(v => v.y) - pc.Min(v => v.y) + 1) * ch;
                var ground = Rect(ui, X(px), Y(py), pw - 3f, ph - 3f, ConfColor("Palace"));
                Clickable(ground, () => ClickPalace(town));
                Picture(ui, PalaceSprite(e.PalaceLevel), X(px), Y(py) + 6f, pw - 6f, ph - 18f);
                Label(ui, X(px), Y(py) - ph / 2f + 9f, string.Format(GameTool.LS("estate_palace_label"), e.PalaceLevel), new Color(0.45f, 0.08f, 0.05f, 0.80f));
            }

            // district names: the biggest blocks first (at most MaxLabels a district), on the top, middle or bottom row
            // of the block, never over another name or the palace
            var taken = new List<(float x, float y, float w, float h)>();
            if (pc.Count > 0)
                taken.Add((X((pc.Min(v => v.x) + pc.Max(v => v.x)) / 2f), Y((pc.Min(v => v.y) + pc.Max(v => v.y)) / 2f),
                    (pc.Max(v => v.x) - pc.Min(v => v.x) + 1) * cw, (pc.Max(v => v.y) - pc.Min(v => v.y) + 1) * ch));
            var perDistrict = new Dictionary<char, int>();
            bool TryLabel(List<(int x, int y)> block)
            {
                var code = map[block[0].y][block[0].x];
                var name = DistrictName(code.ToString());
                var lw = LabelWidth(name);
                var blockRows = block.Select(v => v.y).Distinct().OrderBy(v => v).ToList();
                foreach (var ry in new[] { blockRows.First(), blockRows[blockRows.Count / 2], blockRows.Last() }.Distinct())
                {
                    var cells = block.Where(v => v.y == ry).ToList();
                    float minX = cells.Min(v => v.x), maxX = cells.Max(v => v.x);
                    foreach (var cx in new[] { (minX + maxX) / 2f, minX, maxX }.Distinct())
                        foreach (var dy in new[] { ch / 2f - 8f, 0f, -ch / 2f + 8f })
                        {
                            var lx = X(cx);
                            var ly = Y(ry) + dy;
                            if (taken.Any(r => Math.Abs(r.x - lx) * 2 < r.w + lw + 2f && Math.Abs(r.y - ly) * 2 < r.h + 17f))
                                continue;
                            Label(ui, lx, ly, name, new Color(0f, 0f, 0f, 0.55f));
                            taken.Add((lx, ly, lw, 17f));
                            perDistrict[code] = (perDistrict.TryGetValue(code, out var n) ? n : 0) + 1;
                            return true;
                        }
                }
                return false;
            }
            var blocks = Blocks(map).OrderByDescending(b => b.Count).ToList();
            // every district at least once (its biggest block that has room), then more names on the big blocks
            var labelled = new HashSet<List<(int x, int y)>>();
            foreach (var group in blocks.GroupBy(b => map[b[0].y][b[0].x]))
                foreach (var block in group)
                    if (TryLabel(block)) { labelled.Add(block); break; }
            foreach (var block in blocks.Where(b => b.Count >= LabelBlock && !labelled.Contains(b)))
            {
                var code = map[block[0].y][block[0].x];
                if ((perDistrict.TryGetValue(code, out var count) ? count : 0) >= Conf.Map.MaxLabels) continue;
                TryLabel(block);
            }
            ui.AddText(ui.MidCol, ui.LastRow - 1, LegendText()).Format(null, 13);
        }

        /// <summary>The price legend: the districts of the config with their price factor.</summary>
        public static string LegendText() =>
            string.Format(GameTool.LS("estate_legend"), string.Join(" · ", "MNRCO".Where(DISTRICTS.ContainsKey).Select(k => $"{DistrictName(k.ToString())} x{DISTRICTS[k].Price:0.0#}")));

        /// <summary>Width of a label on the map (font 12: about 6.5 px a latin letter, 12 px a CJK / Thai sign, + the band).</summary>
        public static float LabelWidth(string text) => (text ?? "").Sum(c => c < 0x0E00 ? 6.5f : 12f) + 12f;

        /// <summary>Plots of a district block that gets its name on the map.</summary>
        public static int LabelBlock => Conf.Map.LabelBlock;

        /// <summary>A number of a cell (the same every time): the decoration picks with it.</summary>
        private static int Hash(int x, int y) => (int)(((uint)(x * 73856093) ^ (uint)(y * 19349663) ^ 0x5bd1e995u) % 100000u);

        /// <summary>A gate of the wall where a main road leaves the town (dx, dy: the side), its two posts.</summary>
        private static void Gate(UICustom1 ui, float x, float y, int dx, int dy, float cw, float ch, float roadWidth, Color road, Color post)
        {
            if (dx != 0)
            {
                var ex = x + dx * (cw / 2f + 4f);
                Rect(ui, ex, y, 10f, roadWidth, road);
                Rect(ui, ex, y + roadWidth / 2f + 3f, 8f, 6f, post);
                Rect(ui, ex, y - roadWidth / 2f - 3f, 8f, 6f, post);
            }
            if (dy != 0)
            {
                var ey = y - dy * (ch / 2f + 4f);
                Rect(ui, x, ey, roadWidth, 10f, road);
                Rect(ui, x + roadWidth / 2f + 3f, ey, 6f, 8f, post);
                Rect(ui, x - roadWidth / 2f - 3f, ey, 6f, 8f, post);
            }
        }

        /// <summary>A small white text on a dark band.</summary>
        private static void Label(UICustom1 ui, float x, float y, string text, Color band)
        {
            var bg = Rect(ui, x, y, 10f, 16f, band);
            var t = ui.AddText(x, y, text).Format(Color.white, 12);
            t.Item.raycastTarget = false;
            bg.Size(t.Item.preferredWidth + 12f, 17f);
        }

        private static List<(int x, int y)> Cells(string[] map, char c)
        {
            var rs = new List<(int x, int y)>();
            for (var y = 0; y < map.Length; y++)
                for (var x = 0; x < map[y].Length; x++)
                    if (map[y][x] == c) rs.Add((x, y));
            return rs;
        }

        /// <summary>Connected blocks of plots of the same district (one name each).</summary>
        public static List<List<(int x, int y)>> Blocks(string[] map)
        {
            var seen = new HashSet<(int, int)>();
            var rs = new List<List<(int x, int y)>>();
            for (var y = 0; y < map.Length; y++)
                for (var x = 0; x < map[y].Length; x++)
                {
                    var c = map[y][x];
                    if (!IsPlotCell(c) || seen.Contains((x, y))) continue;
                    var block = new List<(int x, int y)>();
                    var stack = new Stack<(int x, int y)>();
                    stack.Push((x, y)); seen.Add((x, y));
                    while (stack.Count > 0)
                    {
                        var v = stack.Pop();
                        block.Add(v);
                        foreach (var n in new[] { (v.x + 1, v.y), (v.x - 1, v.y), (v.x, v.y + 1), (v.x, v.y - 1) })
                        {
                            if (n.Item2 < 0 || n.Item2 >= map.Length || n.Item1 < 0 || n.Item1 >= map[n.Item2].Length) continue;
                            if (map[n.Item2][n.Item1] != c || seen.Contains(n)) continue;
                            seen.Add(n); stack.Push(n);
                        }
                    }
                    rs.Add(block);
                }
            return rs;
        }

        /// <summary>A click on a plot: buy a free one, enter the player's house, information on the others.</summary>
        public static void ClickPlot(MapBuildTown town, Plot p)
        {
            var title = string.Format(GameTool.LS("estate_title"), town.name);
            var district = DistrictName(p.District);
            if (p.Level <= 0 && p.OwnerId == null)
            {
                if (HasHouse(town, Player))
                {
                    InfoDialog.Msg(title, GameTool.LS("estate_one_house"));
                    return;
                }
                var land = LandPrice(town, p); var build = BuildCost(town, 1);
                InfoDialog.Msg(title, PlotWhere(p) + "\n" + string.Format(GameTool.LS("estate_buy_confirm"), district, p.Prosperity, SpiritNoteEvent.NotesFor(Payable(land + build)), Payable(land + build), SpiritNoteEvent.Notes(Player), HouseTax(town, p, 1))
                        + "\n" + string.Format(GameTool.LS("estate_buy_room"), HouseName(MaxLevelAt(Get(town), p))),
                    MsgBoxButtonEnum.YesNo, () => Result(town, p, Buy(town, p, Player), land + build, 1));
                return;
            }
            if (p.OwnerId != null && p.OwnerId == Player.GetUnitId())
            {
                OpenHouse(town, p);
                return;
            }
            if (p.OwnerId != null)
            {
                // the house of an NPC cultivator
                var owner = g.world.unit.GetUnit(p.OwnerId);
                InfoDialog.Show(title, PlotWhere(p) + "\n" + string.Format(GameTool.LS("estate_info_owner"), owner?.GetName() ?? "-", HouseName(p.Level), district, p.Prosperity, HouseTax(town, p)));
                return;
            }
            var grown = p.Level > DistrictOf(p.District).MortalLevel ? "\n" + string.Format(GameTool.LS("estate_info_grown"), HouseName(p.Level)) : string.Empty;
            // a mortal house: what it is, and buying it back (MortalWindow, not a shop of the market)
            MortalWindow(town, p, title, string.Format(GameTool.LS(DistrictOf(p.District).Code == 'M' ? "estate_info_shop" : "estate_info_mortal"), district, p.Prosperity, LandPrice(town, p)) + grown);
        }

        /// <summary>The palace: the master expands it (town budget), others see who holds the town.</summary>
        public static void ClickPalace(MapBuildTown town)
        {
            var e = Get(town);
            var title = PalaceName(e.PalaceLevel);
            var master = MapBuildPropertyEvent.GetTownMaster(town);
            var bonus = (int)Math.Round(PopPerPalaceLevel * (e.PalaceLevel - 1) * 100);
            int Budget(int level) => (int)Math.Round(Conf.Population.PalaceYearlyBudget * (level - 1) * 100);
            if (master != null && master.IsPlayer() && MapBuildPropertyEvent.IsTownMaster(town, master) && e.PalaceLevel < PALACE_MAX)
            {
                var cost = PalaceCost(town, e);
                InfoDialog.Msg(title, string.Format(GameTool.LS("estate_palace_confirm"), e.PalaceLevel, bonus, PalaceName(e.PalaceLevel + 1), cost,
                        MapBuildPropertyEvent.GetBuildProperty(town), (int)Math.Round(PopPerPalaceLevel * e.PalaceLevel * 100), Budget(e.PalaceLevel), Budget(e.PalaceLevel + 1)),
                    MsgBoxButtonEnum.YesNo, () =>
                    {
                        var rs = UpgradePalace(town, master);
                        if (rs == BuildResult.Ok) { OpenPlots(town); InfoDialog.Msg(title, string.Format(GameTool.LS("estate_done"), PalaceName(e.PalaceLevel))); }
                        else InfoDialog.Msg(title, string.Format(GameTool.LS("estate_palace_budget"), cost));
                    });
                return;
            }
            InfoDialog.Msg(title, string.Format(GameTool.LS("estate_palace_info"), master?.GetName() ?? "-", e.PalaceLevel, bonus, Budget(e.PalaceLevel)));
        }

        /// <summary>Not enough banknotes for a price: how many it takes, how many the bag holds, where to get them.</summary>
        public static string NoNotesText(long cost) =>
            string.Format(GameTool.LS("estate_no_notes"), SpiritNoteEvent.NotesFor(cost), SpiritNoteEvent.Notes(Player));

        public static string NoRoomText(int level)
        {
            var f = Footprint(level);
            return string.Format(GameTool.LS("estate_noroom"), HouseName(level), f[0], f[1]);
        }

        private static void Result(MapBuildTown town, Plot p, BuildResult rs, long cost, int level)
        {
            var title = string.Format(GameTool.LS("estate_title"), town.name);
            switch (rs)
            {
                case BuildResult.Ok:
                    OpenHouse(town, p);
                    InfoDialog.Msg(title, string.Format(GameTool.LS("estate_done"), HouseName(level)));
                    break;
                case BuildResult.NoMoney:
                    InfoDialog.Msg(title, NoNotesText(cost));
                    break;
                case BuildResult.GradeTooLow:
                    InfoDialog.Msg(title, string.Format(GameTool.LS("estate_grade"), HouseName(level), RequiredGrade(level)));
                    break;
                case BuildResult.NoRoom:
                    InfoDialog.Msg(title, NoRoomText(level));
                    break;
                default:
                    InfoDialog.Msg(title, GameTool.LS("estate_not_free"));
                    break;
            }
        }
    }
}
