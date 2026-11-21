using MOD_nE7UL2.Helper;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Rooms of the player's house (B2): one room a house level, 3 levels a room.
    /// - Quiet room: a month spent in the town gives cultivation exp (2 % of the phase a level).
    /// - Training ground: a month spent in the town gives body refining exp (40 a level, BodyCulEvent).
    /// - Herb garden: grows herbs of the owner's grade every month (1 a level, up to 12 a level), collected in the house.
    /// - Spirit gathering array: the Qi gathered in the town grows faster (+15 % a level, QiCulEvent).
    /// - Ancestral hall: the household upkeep is lower (-10 % a level, FamilyFundEvent), the best hall of all houses.
    /// </summary>
    public partial class EstateEvent
    {
        public enum RoomKind { Quiet = 1, Training = 2, Garden = 3, Array = 4, Hall = 5 }

        // ModConf/estate_configs.json, "Rooms"
        public static int RoomMax => Conf.Rooms.MaxLevel;
        private static EstateConfig._Rooms R => Conf.Rooms;

        public static readonly RoomKind[] ROOM_KINDS = { RoomKind.Quiet, RoomKind.Training, RoomKind.Garden, RoomKind.Array, RoomKind.Hall };

        // ------------------------------------------------------------------ formulas (pure, unit tests)

        /// <summary>Rooms a house can have: one a house level.</summary>
        public static int RoomSlots(int houseLevel) => Math.Max(0, Math.Min(MAX_LEVEL, houseLevel));
        /// <summary>A room level (1-3): base tax x 300 x 2^(level - 1).</summary>
        public static long BaseRoomCost(int plainBaseTax, int level) => (long)Math.Round(plainBaseTax * Conf.Prices.Room * Math.Pow(Conf.Prices.RoomGrowth, Math.Max(0, level - 1)));
        public static float QiRateOfArray(int level) => 1f + R.ArrayQi * Math.Max(0, level);
        public static float UpkeepRateOfHall(int level) => Math.Max(0f, 1f - R.HallUpkeep * Math.Max(0, level));
        /// <summary>Exp of a month in the quiet room: a share of the exp of the phase.</summary>
        public static int QuietExp(int phaseExp, int level, int months) => (int)Math.Min(int.MaxValue, Math.Round((double)phaseExp * R.QuietExp * level * Math.Max(1, months)));

        // ------------------------------------------------------------------ data

        public static int RoomLevel(Plot p, RoomKind kind) => p?.Rooms != null && p.Rooms.TryGetValue((int)kind, out var l) ? l : 0;
        public static long RoomCost(MapBuildTown town, int level) => Payable(BaseRoomCost(PriceTax(town), level));
        public static string RoomName(RoomKind kind) => GameTool.LS($"estate_room_{(int)kind}");

        /// <summary>The effect of a room at a level, as a text.</summary>
        public static string RoomEffect(RoomKind kind, int level)
        {
            switch (kind)
            {
                case RoomKind.Quiet: return string.Format(GameTool.LS("estate_room_eff_1"), Math.Round(R.QuietExp * level * 100));
                case RoomKind.Training: return string.Format(GameTool.LS("estate_room_eff_2"), R.TrainingExp * level);
                case RoomKind.Garden: return string.Format(GameTool.LS("estate_room_eff_3"), R.GardenHerbs * level, R.GardenCap * level);
                case RoomKind.Array: return string.Format(GameTool.LS("estate_room_eff_4"), Math.Round(R.ArrayQi * level * 100));
                case RoomKind.Hall: return string.Format(GameTool.LS("estate_room_eff_5"), Math.Round(R.HallUpkeep * level * 100));
            }
            return string.Empty;
        }

        /// <summary>Builds a room (level 1) or raises it one level; paid to the town budget.</summary>
        public static BuildResult BuildRoom(MapBuildTown town, Plot p, RoomKind kind, WorldUnitBase wunit)
        {
            if (p.OwnerId != wunit.GetUnitId())
                return BuildResult.NotOwner;
            if (p.Rooms == null) p.Rooms = new Dictionary<int, int>();
            var lvl = RoomLevel(p, kind);
            if (lvl >= RoomMax)
                return BuildResult.MaxLevel;
            if (lvl == 0 && p.Rooms.Count(x => x.Value > 0) >= RoomSlots(p.Level))
                return BuildResult.NoSlot;
            var notes = SpiritNoteEvent.NotesFor(RoomCost(town, lvl + 1));
            if (!SpiritNoteEvent.PayNotes(wunit, notes))
                return BuildResult.NoMoney;
            var cost = notes * SpiritNoteEvent.NoteValue;
            MapBuildPropertyEvent.AddBuildProperty(town, cost);
            p.Rooms[(int)kind] = lvl + 1;
            if (wunit.IsPlayer())
                TaoistLogEvent.Log("log_estate_room", town.name, RoomName(kind), lvl + 1, cost);
            return BuildResult.Ok;
        }

        // ------------------------------------------------------------------ effects

        /// <summary>Herb garden: herbs of the owner's grade, waiting in the house.</summary>
        public static void Garden(Plot p, WorldUnitBase owner, int months)
        {
            var lvl = RoomLevel(p, RoomKind.Garden);
            if (lvl <= 0)
                return;
            if (p.Herbs == null) p.Herbs = new List<int>();
            var cap = R.GardenCap * lvl;
            for (var i = 0; i < R.GardenHerbs * lvl * months && p.Herbs.Count < cap; i++)
            {
                var conf = EncounterEvent.PickItem(x => x.IsHerbItem() != null, owner.GetGradeLvl());
                if (conf == null) break;
                p.Herbs.Add(conf.id);
            }
        }

        /// <summary>Takes the herbs of the garden to the bag. Returns the count.</summary>
        public static int CollectHerbs(Plot p, WorldUnitBase wunit)
        {
            if (p.Herbs == null || p.Herbs.Count == 0)
                return 0;
            var n = p.Herbs.Count;
            foreach (var g1 in p.Herbs.GroupBy(x => x))
                wunit.AddUnitProp(g1.Key, g1.Count());
            p.Herbs.Clear();
            return n;
        }

        /// <summary>The rooms that work when the owner spends the month in the town: quiet room, training ground.</summary>
        public static void HomeRooms(Plot p, WorldUnitBase wunit, int months)
        {
            var quiet = RoomLevel(p, RoomKind.Quiet);
            if (quiet > 0)
                wunit.AddExp(QuietExp(wunit.GetMaxExpCurrentPhase() - wunit.GetMinExpCurrentPhase(), quiet, months));
            var training = RoomLevel(p, RoomKind.Training);
            if (training > 0 && BodyCulEvent.Get(wunit) != null)
                BodyCulEvent.AddExp(wunit, R.TrainingExp * training * months);
        }

        /// <summary>Qi gain of a unit (QiCulEvent): faster in a town where its house has a spirit gathering array.</summary>
        public static float QiRate(WorldUnitBase wunit)
        {
            if (Instance == null || wunit == null || !wunit.IsPlayer())
                return 1f;
            var town = wunit.GetMapBuild<MapBuildTown>();
            return QiRateOfArray(RoomLevel(town == null ? null : HouseOf(town, wunit), RoomKind.Array));
        }

        /// <summary>Household upkeep rate (FamilyFundEvent): the best ancestral hall of the player's houses.</summary>
        public static float UpkeepRate()
        {
            var best = HousesOf(g.world.playerUnit).Select(x => RoomLevel(x, RoomKind.Hall)).DefaultIfEmpty(0).Max();
            return UpkeepRateOfHall(best);
        }

        // ------------------------------------------------------------------ window

        /// <summary>Inside the player's house: the house, its rooms, upgrade, herbs.</summary>
        public static void OpenHouse(MapBuildTown town, Plot p)
        {
            var ui = new UICustom1(string.Format(GameTool.LS("estate_house_title"), HouseName(p.Level), town.name));
            // the house on the left, its facts on the right
            var px = ui.Columns[ui.FirstCol + 6];
            var py = ui.Rows[ui.FirstRow + 3];
            Rect(ui, px, py, 250f, 150f, DistrictColor(p.District));
            Picture(ui, EstateConfig.SplitSprite(HouseConf(p.Level).Sprite), px, py, 240f, 140f);
            var row = ui.FirstRow;
            var col = ui.MidCol + 4;
            var used = p.Rooms?.Count(x => x.Value > 0) ?? 0;
            ui.AddText(col, row++, string.Format(GameTool.LS("estate_house_facts"), DistrictName(p.District), p.Prosperity, p.Level, HouseTax(town, p))).Format(null, 14);
            ui.AddText(col, row++, string.Format(GameTool.LS("estate_house_rooms"), used, RoomSlots(p.Level))).Format(null, 14);
            row++;
            // upgrade of the house
            if (p.Level < MAX_LEVEL)
            {
                var (add, land, cost) = UpgradeCost(town, p);
                if (add == null)
                {
                    // the plots around are roads, water or other owners': no room for the next level here
                    ui.AddText(col, row, NoRoomText(p.Level + 1)).Format(Color.gray, 13);
                    row += 2;
                }
                else
                {
                    var up = ui.AddButton(col, row, () =>
                    {
                        var rs = Upgrade(town, p, Player);
                        if (rs == BuildResult.Ok) { OpenHouse(town, p); InfoDialog.Msg(HouseName(p.Level), string.Format(GameTool.LS("estate_done"), HouseName(p.Level))); }
                        else Result(town, p, rs, cost, p.Level + 1);
                    }, string.Format(GameTool.LS("estate_upgrade_btn"), HouseName(p.Level + 1), SpiritNoteEvent.NotesFor(cost), RequiredGrade(p.Level + 1))).Size(480, 36);
                    SpiritNoteEvent.WithIcon(ui, up, ui.Columns[col], ui.Rows[row]);
                    row += 2;
                    if (add.Count > 0)
                    {
                        ui.AddText(col, row, string.Format(GameTool.LS("estate_upgrade_land"), add.Count, SpiritNoteEvent.NotesFor(land))).Format(new Color(0.55f, 0.35f, 0f), 12);
                        row += 2;
                    }
                }
            }
            // herbs of the garden
            if (RoomLevel(p, RoomKind.Garden) > 0)
            {
                ui.AddButton(col, row, () =>
                {
                    var n = CollectHerbs(p, Player);
                    OpenHouse(town, p);
                    InfoDialog.Msg(RoomName(RoomKind.Garden), string.Format(GameTool.LS("estate_herbs_done"), n));
                }, string.Format(GameTool.LS("estate_herbs_btn"), p.Herbs?.Count ?? 0)).Size(480, 36);
                row += 2;
            }
            ui.AddButton(col, row, () => OpenPlots(town), GameTool.LS("estate_back_map")).Size(480, 36);

            // rooms: built ones, then the empty slots (a line every 2 rows: the buttons are taller than a row)
            row = Math.Max(row + 2, ui.FirstRow + 8);
            ui.AddText(ui.MidCol, row++, GameTool.LS("estate_rooms_title")).Format(null, 16);
            row++;
            var textX = ui.Columns[ui.FirstCol + 3];
            var btnCol = ui.LastCol - 6;
            foreach (var kind in ROOM_KINDS.Where(k => RoomLevel(p, k) > 0))
            {
                var k = kind;
                var lvl = RoomLevel(p, k);
                LeftText(ui, textX, row, $"{RoomName(k)} {lvl}/{RoomMax}:  {RoomEffect(k, lvl)}", Color.black);
                if (lvl < RoomMax)
                {
                    var cost = RoomCost(town, lvl + 1);
                    SpiritNoteEvent.WithIcon(ui, ui.AddButton(btnCol, row, () => RoomResult(town, p, k, BuildRoom(town, p, k, Player), cost),
                        string.Format(GameTool.LS("estate_room_up"), SpiritNoteEvent.NotesFor(cost))).Size(220, 30), ui.Columns[btnCol], ui.Rows[row], 22f);
                }
                row += 2;
            }
            for (var i = used; i < RoomSlots(p.Level); i++)
            {
                LeftText(ui, textX, row, GameTool.LS("estate_room_empty"), Color.gray);
                ui.AddButton(btnCol, row, () => OpenRoomChoice(town, p), GameTool.LS("estate_room_build")).Size(220, 30);
                row += 2;
            }
            if (RoomSlots(p.Level) < MAX_LEVEL)
                ui.AddText(ui.MidCol, row, GameTool.LS("estate_room_more")).Format(Color.gray, 13);
        }

        /// <summary>The rooms that can be built in an empty slot.</summary>
        public static void OpenRoomChoice(MapBuildTown town, Plot p)
        {
            var ui = new UICustom1(GameTool.LS("estate_room_build"));
            var row = ui.FirstRow + 1;
            var cost = RoomCost(town, 1);
            var textX = ui.Columns[ui.FirstCol + 3];
            foreach (var kind in ROOM_KINDS.Where(k => RoomLevel(p, k) == 0))
            {
                var k = kind;
                LeftText(ui, textX, row, $"{RoomName(k)}:  {RoomEffect(k, 1)}", Color.black);
                SpiritNoteEvent.WithIcon(ui, ui.AddButton(ui.LastCol - 6, row, () => RoomResult(town, p, k, BuildRoom(town, p, k, Player), cost),
                    string.Format(GameTool.LS("estate_room_new"), SpiritNoteEvent.NotesFor(cost))).Size(220, 30), ui.Columns[ui.LastCol - 6], ui.Rows[row], 22f);
                row += 2;
            }
            ui.AddText(ui.MidCol, row + 1, GameTool.LS("estate_room_hint")).Format(null, 13);
            ui.AddButton(ui.MidCol, ui.LastRow - 2, () => OpenHouse(town, p), GameTool.LS("estate_back_house")).Size(240, 36);
        }

        /// <summary>A text left aligned at x (canvas units) on a row of the window.</summary>
        private static void LeftText(UICustom1 ui, float x, int row, string text, Color color)
        {
            var t = ui.AddText(x, ui.Rows[row], text).Format(color, 14).Align(TextAnchor.MiddleLeft);
            t.Pos(x + t.Item.rectTransform.rect.width / 2f, ui.Rows[row]);
        }

        private static void RoomResult(MapBuildTown town, Plot p, RoomKind kind, BuildResult rs, long cost)
        {
            switch (rs)
            {
                case BuildResult.Ok:
                    OpenHouse(town, p);
                    InfoDialog.Msg(RoomName(kind), string.Format(GameTool.LS("estate_done"), $"{RoomName(kind)} {RoomLevel(p, kind)}"));
                    break;
                case BuildResult.NoMoney:
                    InfoDialog.Msg(RoomName(kind), NoNotesText(cost));
                    break;
                case BuildResult.NoSlot:
                    InfoDialog.Msg(RoomName(kind), GameTool.LS("estate_room_noslot"));
                    break;
                default:
                    InfoDialog.Msg(RoomName(kind), GameTool.LS("estate_room_max"));
                    break;
            }
        }
    }
}
