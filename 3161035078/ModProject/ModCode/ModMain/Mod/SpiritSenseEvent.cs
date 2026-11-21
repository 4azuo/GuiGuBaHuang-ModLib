using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Spirit sense (thần thức): the reach of the mind, for the player and the NPCs.
    /// - Exp: every month (2 + Talent / 20), seclusions (5 a month), heart demons beaten, a scan of the surroundings once a
    ///   month (window button).
    /// - 5 levels (200 / 1,000 / 4,000 / 15,000 / 50,000 exp); above the top the exp becomes essence (+1 % max SP a step).
    /// - Effects, mainly: max SP +4 % and max MP +2 % a level (+1 % SP an essence step); a little: a bounty hunter's ambush sensed and avoided (20 % a level); the heart demons come
    ///   less often (-15 % a level); the scan shows who is around (hostile cultivators, the strongest one) and how rich the
    ///   cells are in herbs and ores. The map view of the game cannot be widened without a patch of the game.
    /// Off with S&amp;M "No spirit sense".
    /// </summary>
    [Cache(ModConst.SPIRIT_SENSE_EVENT)]
    public partial class SpiritSenseEvent : ModEvent
    {
        public static SpiritSenseEvent Instance { get; set; }

        public static readonly long[] LEVELS = { 200, 1000, 4000, 15000, 50000 };
        public static int MAX_LEVEL => LEVELS.Length;
        public static long MaxExp => LEVELS[LEVELS.Length - 1];
        public const long ESSENCE_STEP = 10000;
        public const double MP_LEVEL = 0.02, SP_LEVEL = 0.04, SP_STEP = 0.01;
        public const float AMBUSH_LEVEL = 20f;
        public const float HEART_DEMON_LEVEL = 0.15f;
        public const int MONTHLY_BASE = 2, MONTHLY_TALENT = 20;
        public const int SECLUSION_MONTH = 5, HEART_DEMON_WIN = 200, SCAN_EXP = 20;
        public const int SCAN_RANGE = 6, SCAN_RANGE_LEVEL = 3;

        public class SenseData
        {
            public long Exp { get; set; }
            public long Essence { get; set; }
            public int LastScanMonth { get; set; } = -1;
            public int Avoided { get; set; }
        }

        public Dictionary<string, SenseData> Senses { get; set; } = new Dictionary<string, SenseData>();

        public override bool OnCacheHandler() => !(SMLocalConfigsEvent.Instance?.Configs?.NoSpiritSense ?? false);
        public static bool Enabled => Instance != null && !(SMLocalConfigsEvent.Instance?.Configs?.NoSpiritSense ?? false);
        private static WorldUnitBase Player => g.world.playerUnit;

        // ------------------------------------------------------------------ formulas (pure)

        public static int LevelOf(long exp) => LEVELS.Count(x => exp >= x);
        public static long NextOf(long exp) { var l = LevelOf(exp); return l >= MAX_LEVEL ? MaxExp : LEVELS[l]; }
        public static int EssenceSteps(long essence) => (int)Math.Min(int.MaxValue, Math.Max(0, essence) / ESSENCE_STEP);
        public static double MpRateOf(int level) => MP_LEVEL * level;
        public static double SpRateOf(int level, int steps) => SP_LEVEL * level + SP_STEP * steps;
        /// <summary>Chance (%) to sense an ambush and avoid it: 20 a level, at most 90.</summary>
        public static float AmbushChanceOf(int level) => Math.Min(90f, AMBUSH_LEVEL * level);
        /// <summary>Rate of the heart demon chance: -15 % a level, at least x0.25.</summary>
        public static float HeartDemonRateOf(int level) => Math.Max(0.25f, 1f - HEART_DEMON_LEVEL * level);
        /// <summary>Exp a month: 2 + Talent / 20.</summary>
        public static int MonthlyExp(int talent) => MONTHLY_BASE + Math.Max(0, talent) / MONTHLY_TALENT;
        public static int ScanRange(int level) => SCAN_RANGE + SCAN_RANGE_LEVEL * level;

        // ------------------------------------------------------------------ data

        public static SenseData Get(WorldUnitBase w, bool create = false)
        {
            if (w == null || !Enabled) return null;
            var id = w.GetUnitId();
            if (!Instance.Senses.TryGetValue(id, out var d) && create)
                Instance.Senses[id] = d = new SenseData();
            return d;
        }
        public static int Level(WorldUnitBase w) => LevelOf(Get(w)?.Exp ?? 0);
        public static string LevelName(int level) => GameTool.LS($"sense_lvl_{Math.Max(0, Math.Min(MAX_LEVEL, level))}");
        public static float HeartDemonRate(WorldUnitBase w) => Enabled ? HeartDemonRateOf(Level(w)) : 1f;
        public static int AdjMp(WorldUnitBase w, int baseMp) => Get(w) == null ? 0 : (int)Math.Round(baseMp * MpRateOf(Level(w)), MidpointRounding.AwayFromZero);
        public static int AdjSp(WorldUnitBase w, int baseSp) => Get(w) == null ? 0 : (int)Math.Round(baseSp * SpRateOf(Level(w), EssenceSteps(Get(w).Essence)), MidpointRounding.AwayFromZero);

        /// <summary>Adds exp; above the top it goes into the essence. Returns the levels gained.</summary>
        public static int AddExp(WorldUnitBase w, long exp, string reasonKey = null)
        {
            var d = Get(w, true);
            if (d == null || exp <= 0) return 0;
            exp = (long)Math.Round(exp * SMGlobalConfigsEvent.Rate(c => c.SpiritSenseRate));
            var before = LevelOf(d.Exp);
            var v = d.Exp + exp;
            if (v > MaxExp) { d.Essence += v - MaxExp; v = MaxExp; }
            d.Exp = v;
            var after = LevelOf(d.Exp);
            if (w.IsPlayer() && after != before)
                TaoistLogEvent.Log("log_sense_level", LevelName(after));
            return after - before;
        }

        /// <summary>
        /// WantedEvent: a hunter comes for the player. With the spirit sense the ambush is sensed (roll 0-100 under the
        /// chance) and avoided: the player slips away. Returns true when avoided.
        /// </summary>
        public static bool SenseAmbush(WorldUnitBase hunter, float roll = -1f)
        {
            if (!Enabled || hunter == null) return false;
            if (roll < 0) roll = TestRoll ?? CommonTool.Random(0f, 100f);
            if (roll >= AmbushChanceOf(Level(Player))) return false;
            Get(Player, true).Avoided++;
            TaoistLogEvent.Log("log_sense_ambush", hunter.GetName());
            DramaHelper.OpenDrama2(string.Format(GameTool.LS("sense_ambush"), hunter.GetName()), new List<string> { GameTool.LS("enc_leave") }, null, "hundengshangren");
            return true;
        }

        // ------------------------------------------------------------------ the scan

        public class ScanResult
        {
            public int Units, Hostile, StrongestGrade, Herbs, Ores;
            public string Strongest;
            /// <summary>The nearest mine vein / herb valley (ArtisanEvent): name, distance (cells), direction (0-7), null when none.</summary>
            public string SpotName;
            public int SpotDistance, SpotDirection;
            /// <summary>The cultivators around (hostile first, then the strongest) and the places in reach (SpiritSenseEvent.Scan.cs).</summary>
            public List<WorldUnitBase> UnitList;
            public List<ScanPlace> Places;
        }

        /// <summary>A scan of the surroundings: the cultivators around (hostile: hating the player or a hunter of his bounty), the herbs and ores of the cells.</summary>
        public static ScanResult Scan(WorldUnitBase p)
        {
            var range = ScanRange(Level(p));
            var units = p.GetUnitsAround(range, false, false).ToArray().Where(x => x != null && !x.isDie && !x.IsPlayer()).ToList();
            var rs = new ScanResult { Units = units.Count };
            rs.Hostile = units.Count(x => x.data.unitData.relationData.GetIntim(p) <= -100);
            var strong = units.OrderByDescending(x => x.GetGradeLvl()).FirstOrDefault();
            rs.Strongest = strong?.GetName();
            rs.StrongestGrade = strong?.GetGradeLvl() ?? 0;
            var p0 = p.GetUnitPos();
            for (var dx = -range; dx <= range; dx++)
                for (var dy = -range; dy <= range; dy++)
                {
                    DataGrid.GridData gd = null;
                    try { gd = g.data.grid.GetGridData(p0 + new Vector2Int(dx, dy)); } catch { }
                    if (gd == null) continue;
                    rs.Herbs += Math.Max(0, gd.medicinal);
                    rs.Ores += Math.Max(0, gd.mineral);
                }
            rs.UnitList = ScanUnits(p, range);
            rs.Places = ScanPlaces(p, range);
            var spot = ArtisanEvent.NearestSpot(p0);
            if (spot != null)
            {
                rs.SpotName = ArtisanEvent.SpotName(spot);
                rs.SpotDistance = Math.Max(Math.Abs(spot.X - p0.x), Math.Abs(spot.Y - p0.y));
                rs.SpotDirection = ArtisanEvent.Direction(p0, spot.Point());
            }
            return rs;
        }

        // ------------------------------------------------------------------ events

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (wunit == null || wunit.isDie) return;
            var months = Math.Max(1, ModMaster.MonthsPassed);
            AddExp(wunit, MonthlyExp(wunit.GetProperty<int>(UnitPropertyEnum.Talent)) * months);
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.FateFeature.uiName)
            {
                var ui = new UICover<UIFateFeature>(e.ui);
                ui.AddButton(ui.MidCol - 8, ui.MidRow - 1, OpenWindow, GameTool.LS("sense_button")).Size(160f, 30f);
                ui.AddText(ui.MidCol - 8, ui.MidRow, "{0}").Format(Color.black, 13).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { LevelName(Level(Player)) }),
                });
                ui.IsAutoUpdate = true;
                ui.UpdateUI();
            }
            else if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var uiBase = g.ui.GetUI<UINPCInfo>(e.ui.uiType);
                var unit = uiBase?.unit;
                if (unit != null && !unit.IsPlayer() && Level(unit) > 0 && !HirePeopleEvent.isShowHirePeopleUI)
                {
                    var ui = new UICover<UINPCInfo>(uiBase);
                    ui.AddText(ui.MidCol - 6, ui.MidRow - 9, string.Format(GameTool.LS("sense_npc"), LevelName(Level(unit)))).Align().Format(new Color(0.8f, 1f, 0.9f), 16)
                        .SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                    ui.UpdateUI();
                }
            }
        }

        public static void OpenWindow()
        {
            var ui = new UICustom1(GameTool.LS("sense_title"));
            var row = ui.FirstRow + 1;
            var d = Get(Player, true);
            var level = LevelOf(d.Exp);
            var steps = EssenceSteps(d.Essence);
            ui.AddText(ui.MidCol, row++, LevelName(level)).Format(new Color(0.1f, 0.45f, 0.4f), 20);
            ui.AddText(ui.MidCol, row++, level >= MAX_LEVEL ? string.Format(GameTool.LS("sense_exp_top"), d.Exp) : string.Format(GameTool.LS("sense_exp"), d.Exp, NextOf(d.Exp))).Format(null, 15);
            if (d.Essence > 0 || level >= MAX_LEVEL)
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("sense_essence"), d.Essence, steps, steps)).Format(new Color(0.45f, 0.2f, 0.55f), 14);
            row++;
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("sense_effects"), Math.Round(SpRateOf(level, steps) * 100, 1), Math.Round(MpRateOf(level) * 100, 1), AmbushChanceOf(level),
                Math.Round((1 - HeartDemonRateOf(level)) * 100), ScanRange(level), d.Avoided)).Format(null, 15);
            row++;
            if (d.LastScanMonth == GameHelper.GetGameTotalMonth())
                ui.AddText(ui.MidCol, row++, GameTool.LS("sense_scan_done")).Format(Color.gray, 13);
            else
                ui.AddButton(ui.MidCol, ui.Rows[row++], () =>
                {
                    d.LastScanMonth = GameHelper.GetGameTotalMonth();
                    AddExp(Player, SCAN_EXP);
                    var r = Scan(Player);
                    LastScan = r;
                    OpenScanWindow(r);
                }, string.Format(GameTool.LS("sense_scan_button"), ScanRange(level), SCAN_EXP)).Format(Color.black, 14).Size(420, 34);
            row++;
            for (var l = 1; l <= MAX_LEVEL; l++)
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("sense_level_line"), LevelName(l), LEVELS[l - 1], Math.Round(SpRateOf(l, 0) * 100), Math.Round(MpRateOf(l) * 100), AmbushChanceOf(l)))
                    .Format(l == level ? new Color(0.1f, 0.4f, 0.1f) : new Color(0.35f, 0.35f, 0.35f), 13);
            ui.AddText(ui.MidCol, ui.LastRow - 1, GameTool.LS("sense_help")).Format(null, 13);
        }
        /// <summary>A fixed roll for the ambush (tests), null: random.</summary>
        public static float? TestRoll { get; set; }
        /// <summary>The last scan (tests).</summary>
        public static ScanResult LastScan { get; private set; }
    }
}
