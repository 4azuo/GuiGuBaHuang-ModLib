using ModLib.Helper;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// The window of a scan of the spirit sense: a summary; the cultivators around with their portraits (hostile first, then
    /// the strongest; a click on the name opens the game's NPC info); the places in reach (mine veins, herb valleys, towns)
    /// with a picture, their distance and direction (the nearest mine vein / herb valley even out of reach).
    /// Pictures of the places: ModImg/scan_mine.png, scan_herb.png, scan_town.png.
    /// </summary>
    public partial class SpiritSenseEvent
    {
        public const int SCAN_MAX_UNITS = 10, SCAN_MAX_PLACES = 4;
        public const float CARD_W = 110f, CARD_H = 150f, PLACE_W = 220f, PLACE_H = 138f;

        public class ScanPlace
        {
            public string Name, Image;
            public int Distance, Direction, Area;
            public bool InReach;
        }

        /// <summary>The cultivators and places a scan lists (filled by Scan).</summary>
        public static List<WorldUnitBase> ScanUnits(WorldUnitBase p, int range) =>
            p.GetUnitsAround(range, false, false).ToArray().Where(x => x != null && !x.isDie && !x.IsPlayer())
                .OrderByDescending(x => x.data.unitData.relationData.GetIntim(p) <= -100)
                .ThenByDescending(x => x.GetGradeLvl()).ToList();

        public static List<ScanPlace> ScanPlaces(WorldUnitBase p, int range)
        {
            var p0 = p.GetUnitPos();
            int Dist(Vector2Int q) => Math.Max(Math.Abs(q.x - p0.x), Math.Abs(q.y - p0.y));
            var list = new List<ScanPlace>();
            if (ArtisanEvent.Enabled)
            {
                foreach (var s in ArtisanEvent.Instance.Spots.Where(s => Dist(s.Point()) <= range))
                    list.Add(new ScanPlace { Name = ArtisanEvent.SpotName(s), Image = s.Kind == (int)ArtisanEvent.SpotKind.Mine ? "scan_mine.png" : "scan_herb.png",
                        Distance = Dist(s.Point()), Direction = ArtisanEvent.Direction(p0, s.Point()), Area = s.Area, InReach = true });
                if (list.Count == 0)
                {
                    var n = ArtisanEvent.NearestSpot(p0);
                    if (n != null)
                        list.Add(new ScanPlace { Name = ArtisanEvent.SpotName(n), Image = n.Kind == (int)ArtisanEvent.SpotKind.Mine ? "scan_mine.png" : "scan_herb.png",
                            Distance = Dist(n.Point()), Direction = ArtisanEvent.Direction(p0, n.Point()), Area = n.Area, InReach = false });
                }
            }
            foreach (var t in (ModMain.Store?.Towns ?? new MapBuildTown[0]).Where(t => t != null && Dist(t.GetOrigiPoint()) <= range))
                list.Add(new ScanPlace { Name = t.name, Image = "scan_town.png", Distance = Dist(t.GetOrigiPoint()), Direction = ArtisanEvent.Direction(p0, t.GetOrigiPoint()),
                    Area = t.gridData.areaBaseID, InReach = true });
            return list.OrderBy(x => !x.InReach).ThenBy(x => x.Distance).Take(SCAN_MAX_PLACES).ToList();
        }

        /// <summary>The window of a scan.</summary>
        public static void OpenScanWindow(ScanResult r)
        {
            var ui = new UICustom1(GameTool.LS("sense_scan_title"));
            var x0 = ui.Columns[ui.FirstCol];
            var width = ui.Columns[ui.LastCol] - x0;
            var dy = UIHelper.GetUIDeltaY();
            var top = ui.Rows[ui.FirstRow];
            ui.AddText(ui.Columns[ui.MidCol], top, string.Format(GameTool.LS("sense_scan_summary"), r.Units, r.Hostile, r.Herbs, r.Ores)).Format(null, 15);

            // the cultivators: a card each (portrait, name to click, grade and attitude)
            var y = top - dy * 1.6f;
            ui.AddText(x0 + 80f, y, GameTool.LS("sense_scan_people")).Format(new Color(0.1f, 0.35f, 0.3f), 16);
            var units = r.UnitList ?? new List<WorldUnitBase>();
            var cardW = width / SCAN_MAX_UNITS;
            var cardTop = y - dy * 0.8f;
            if (units.Count == 0)
                ui.AddText(ui.Columns[ui.MidCol], cardTop - dy, GameTool.LS("sense_scan_nobody")).Format(Color.gray, 14);
            for (var i = 0; i < Math.Min(SCAN_MAX_UNITS, units.Count); i++)
            {
                var npc = units[i];
                var cx = x0 + cardW * (i + 0.5f);
                try
                {
                    var holder = ui.AddImage(cx, cardTop - CARD_H / 2f, null);
                    holder.Item.color = new Color(0f, 0f, 0f, 0.06f);
                    holder.Size(CARD_W, CARD_H);
                    var img = PortraitHelper.Draw(holder.Item.transform, npc, new Vector2(CARD_W, CARD_H), Vector2.zero);
                    if (img != null) img.uvRect = DamageInfo.HUMAN_UV;
                }
                catch (Exception ex) { DebugHelper.WriteLine($"SpiritSense scan portrait: {ex.Message}"); }
                var hostile = npc.data.unitData.relationData.GetIntim(Player) <= -100;
                ui.AddButton(cx, cardTop - CARD_H - dy * 0.3f, () => g.ui.OpenUISafe<UINPCInfo>(UIType.NPCInfo).InitData(npc, false), npc.GetName())
                    .Format(hostile ? new Color(0.7f, 0.1f, 0.1f) : Color.black, 13).Size(cardW - 8f, 28f);
                ui.AddText(cx, cardTop - CARD_H - dy * 1.1f, string.Format(GameTool.LS("sense_scan_unit"), npc.GetGradeLvl(),
                    GameTool.LS(hostile ? "sense_scan_hostile" : "sense_scan_neutral"))).Format(hostile ? new Color(0.7f, 0.1f, 0.1f) : new Color(0.3f, 0.3f, 0.3f), 12);
            }
            if (units.Count > SCAN_MAX_UNITS)
                ui.AddText(ui.Columns[ui.MidCol], cardTop - CARD_H - dy * 1.8f, string.Format(GameTool.LS("sense_scan_more"), units.Count - SCAN_MAX_UNITS)).Format(Color.gray, 12);

            // the places: a picture each, name, area, distance and direction
            y = cardTop - CARD_H - dy * 2.6f;
            ui.AddText(x0 + 80f, y, GameTool.LS("sense_scan_places")).Format(new Color(0.1f, 0.35f, 0.3f), 16);
            var places = r.Places ?? new List<ScanPlace>();
            var placeW = width / SCAN_MAX_PLACES;
            var placeTop = y - dy * 0.8f;
            if (places.Count == 0)
                ui.AddText(ui.Columns[ui.MidCol], placeTop - dy, GameTool.LS("sense_scan_noplace")).Format(Color.gray, 14);
            for (var i = 0; i < places.Count; i++)
            {
                var pl = places[i];
                var cx = x0 + placeW * (i + 0.5f);
                var sprite = SpriteHelper.GetImage(Instance.ModId, pl.Image);
                var pic = ui.AddImage(cx, placeTop - PLACE_H / 2f, sprite);
                pic.Size(PLACE_W, PLACE_H);
                ui.AddText(cx, placeTop - PLACE_H - dy * 0.4f, pl.Name).Format(Color.black, 15);
                ui.AddText(cx, placeTop - PLACE_H - dy * 1.1f, string.Format(GameTool.LS(pl.InReach ? "sense_scan_place" : "sense_scan_place_far"), pl.Area, pl.Distance,
                    GameTool.LS($"sense_dir_{pl.Direction}"))).Format(new Color(0.3f, 0.3f, 0.3f), 12);
            }
            ui.AddText(ui.Columns[ui.MidCol], ui.Rows[ui.LastRow - 1], GameTool.LS("sense_scan_hint")).Format(Color.gray, 12);
        }
    }
}
