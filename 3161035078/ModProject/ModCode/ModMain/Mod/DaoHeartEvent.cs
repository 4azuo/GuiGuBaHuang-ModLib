using EBattleTypeData;
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
    /// Dao heart (đạo tâm): the steadiness of the mind, for the player and the NPCs.
    /// - Up: heart demons beaten, promises kept (commissions delivered), seclusions, body breakthroughs, a good mood.
    ///   Down: heart demons lost, betrayal (killing a friend, a relative, the master or the spouse), promises broken
    ///   (commissions late), failed breakthroughs, a bad mood.
    /// - 5 levels (100 / 300 / 600 / 1,000 / 2,000); above the top the value becomes essence.
    /// - Effects, mainly: max SP +2 % and max MP +2 % a level (+1 % SP an essence step); a little: the heart demon won more
    ///   often, the body breakthroughs, the backlash of the medicinal baths and the furnace accidents less often.
    /// Off with S&amp;M "No Dao heart".
    /// </summary>
    [Cache(ModConst.DAO_HEART_EVENT)]
    public class DaoHeartEvent : ModEvent
    {
        public static DaoHeartEvent Instance { get; set; }

        public static readonly long[] LEVELS = { 100, 300, 600, 1000, 2000 };
        public static int MAX_LEVEL => LEVELS.Length;
        public static long MaxValue => LEVELS[LEVELS.Length - 1];
        public const long START = 100;            // a unit starts Steady
        public const long ESSENCE_STEP = 1000;
        public const double SP_LEVEL = 0.02, MP_LEVEL = 0.02, SP_STEP = 0.01;
        public const float HEART_DEMON_BASE = 50f, HEART_DEMON_LEVEL = 8f;
        public const float BREAK_LEVEL = 2f;      // body breakthrough chance (points) a level
        public const float RISK_LEVEL = 0.08f;    // backlash / accident chance -8 % a level

        // sources
        public const int HEART_DEMON_WIN = 100, HEART_DEMON_LOSE = -60;
        public const int SECLUSION_YEAR = 10;
        public const int BREAK_OK = 15, BREAK_FAIL = -10;
        public const int PROMISE_KEPT = 20, PROMISE_BROKEN = -30;
        public const int BETRAYAL = -150;
        public const int BETRAY_INTIM = 200;      // intimacy from which a kill is a betrayal
        public const int MOOD_GOOD = 1, MOOD_BAD = -1;

        public class HeartData
        {
            public long Value { get; set; } = START;
            public long Essence { get; set; }
        }

        public Dictionary<string, HeartData> Hearts { get; set; } = new Dictionary<string, HeartData>();

        public override bool OnCacheHandler() => !(SMLocalConfigsEvent.Instance?.Configs?.NoDaoHeart ?? false);
        public static bool Enabled => Instance != null && !(SMLocalConfigsEvent.Instance?.Configs?.NoDaoHeart ?? false);
        private static WorldUnitBase Player => g.world.playerUnit;

        // ------------------------------------------------------------------ formulas (pure)

        public static int LevelOf(long value) => LEVELS.Count(x => value >= x);
        public static long NextOf(long value) { var l = LevelOf(value); return l >= MAX_LEVEL ? MaxValue : LEVELS[l]; }
        public static int EssenceSteps(long essence) => (int)Math.Min(int.MaxValue, Math.Max(0, essence) / ESSENCE_STEP);
        public static double SpRateOf(int level, int steps) => SP_LEVEL * level + SP_STEP * steps;
        public static double MpRateOf(int level) => MP_LEVEL * level;
        /// <summary>Chance (%) to beat a heart demon: 50 + 8 a level, at most 95.</summary>
        public static float HeartDemonWinOf(int level) => Math.Min(95f, HEART_DEMON_BASE + HEART_DEMON_LEVEL * level);
        /// <summary>Points added to the chance of a body breakthrough.</summary>
        public static float BreakBonusOf(int level) => BREAK_LEVEL * level;
        /// <summary>Rate of the backlash / accident chances: -8 % a level.</summary>
        public static float RiskRateOf(int level) => Math.Max(0.5f, 1f - RISK_LEVEL * level);

        // ------------------------------------------------------------------ data

        public static HeartData Get(WorldUnitBase w, bool create = false)
        {
            if (w == null || !Enabled) return null;
            var id = w.GetUnitId();
            if (!Instance.Hearts.TryGetValue(id, out var d) && create)
                Instance.Hearts[id] = d = new HeartData();
            return d;
        }
        /// <summary>Level of a unit (one never seen is Steady, the start value).</summary>
        public static int Level(WorldUnitBase w) => !Enabled || w == null ? 0 : LevelOf(Get(w)?.Value ?? START);
        public static string LevelName(int level) => GameTool.LS($"heart_lvl_{Math.Max(0, Math.Min(MAX_LEVEL, level))}");
        public static float HeartDemonWin(WorldUnitBase w) => Enabled ? HeartDemonWinOf(Level(w)) : HEART_DEMON_BASE;
        public static float BreakBonus(WorldUnitBase w) => Enabled ? BreakBonusOf(Level(w)) : 0f;
        public static float RiskRate(WorldUnitBase w) => Enabled ? RiskRateOf(Level(w)) : 1f;
        /// <summary>Max SP / MP bonuses (UnitModifyEvent).</summary>
        public static int AdjSp(WorldUnitBase w, int baseSp) => !Enabled || w == null ? 0 : (int)Math.Round(baseSp * SpRateOf(Level(w), EssenceSteps(Get(w)?.Essence ?? 0)), MidpointRounding.AwayFromZero);
        public static int AdjMp(WorldUnitBase w, int baseMp) => !Enabled || w == null ? 0 : (int)Math.Round(baseMp * MpRateOf(Level(w)), MidpointRounding.AwayFromZero);

        /// <summary>Moves the Dao heart; above the top the value goes into the essence. Returns the levels moved.</summary>
        public static int Add(WorldUnitBase w, long points, string reasonKey = null)
        {
            var d = Get(w, true);
            if (d == null || points == 0) return 0;
            if (points > 0) points = (long)Math.Round(points * SMGlobalConfigsEvent.Rate(c => c.DaoHeartRate));
            var before = LevelOf(d.Value);
            var v = d.Value + points;
            if (v > MaxValue) { d.Essence += v - MaxValue; v = MaxValue; }
            d.Value = Math.Max(0, v);
            var after = LevelOf(d.Value);
            if (w.IsPlayer())
            {
                if (reasonKey != null)
                    TaoistLogEvent.Log(points > 0 ? "log_heart_up" : "log_heart_down", Math.Abs(points), GameTool.LS(reasonKey));
                if (after != before)
                    TaoistLogEvent.Log("log_heart_level", LevelName(after));
            }
            return after - before;
        }


        // ------------------------------------------------------------------ sources

        /// <summary>Betrayal: the player kills a friend, a relative, the master or the spouse (intimacy 200 or more).</summary>
        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);
            var dead = e?.unit;
            var killer = e?.hitData?.attackUnit?.GetWorldUnit();
            if (dead == null || killer == null || !killer.IsPlayer() || !dead.IsWorldUnit() || DungeonHelper.IsSpar() || DungeonHelper.IsArena())
                return;
            var w = dead.GetWorldUnit();
            if (w == null || w.IsPlayer()) return;
            if (IsBond(Player, w))
                Add(Player, BETRAYAL, "heart_src_betrayal");
        }

        /// <summary>A bond the player betrays by killing: family, master, spouse, or an intimacy of 200 or more.</summary>
        public static bool IsBond(WorldUnitBase p, WorldUnitBase w)
        {
            if (FamilyEvent.IsFamilyOfPlayer(w)) return true;
            if (p.data.allUnitRelation.ContainsKey(w.GetUnitId()))
            {
                var r = p.data.allUnitRelation[w.GetUnitId()];
                if (r == UnitRelationType.Master || r == UnitRelationType.Married || r == UnitRelationType.Student) return true;
            }
            return w.data.unitData.relationData.GetIntim(p) >= BETRAY_INTIM;
        }

        /// <summary>The mood moves the player's heart; NPCs steady theirs slowly.</summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (wunit == null || wunit.isDie) return;
            var months = Math.Max(1, ModMaster.MonthsPassed);
            if (wunit.IsPlayer())
            {
                var max = Math.Max(1, wunit.GetProperty<int>(UnitPropertyEnum.MoodMax));
                var mood = wunit.GetProperty<int>(UnitPropertyEnum.Mood);
                if (mood >= max * 0.8f) Add(wunit, MOOD_GOOD * months);
                else if (mood <= max * 0.2f) Add(wunit, MOOD_BAD * months);
                return;
            }
            Add(wunit, CommonTool.Random(0, 3) * months);
        }

        // ------------------------------------------------------------------ UI

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.FateFeature.uiName)
            {
                var ui = new UICover<UIFateFeature>(e.ui);
                ui.AddButton(ui.MidCol - 8, ui.MidRow - 4, OpenWindow, GameTool.LS("heart_button")).Size(160f, 30f);
                ui.AddText(ui.MidCol - 8, ui.MidRow - 3, "{0}").Format(Color.black, 13).SetWork(new UIItemWork
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
                if (unit != null && !unit.IsPlayer() && Get(unit) != null && !HirePeopleEvent.isShowHirePeopleUI)
                {
                    var ui = new UICover<UINPCInfo>(uiBase);
                    ui.AddText(ui.MidCol - 6, ui.MidRow - 8, string.Format(GameTool.LS("heart_npc"), LevelName(Level(unit)))).Align().Format(new Color(0.85f, 0.85f, 1f), 16)
                        .SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                    ui.UpdateUI();
                }
            }
        }

        public static void OpenWindow()
        {
            var ui = new UICustom1(GameTool.LS("heart_title"));
            var row = ui.FirstRow + 1;
            var d = Get(Player, true);
            var level = LevelOf(d.Value);
            var steps = EssenceSteps(d.Essence);
            ui.AddText(ui.MidCol, row++, LevelName(level)).Format(new Color(0.2f, 0.3f, 0.6f), 20);
            ui.AddText(ui.MidCol, row++, level >= MAX_LEVEL ? string.Format(GameTool.LS("heart_value_top"), d.Value) : string.Format(GameTool.LS("heart_value"), d.Value, NextOf(d.Value))).Format(null, 15);
            if (d.Essence > 0 || level >= MAX_LEVEL)
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("heart_essence"), d.Essence, steps, Math.Round(SP_STEP * steps * 100, 1))).Format(new Color(0.45f, 0.2f, 0.55f), 14);
            row++;
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("heart_effects"), Math.Round(SpRateOf(level, steps) * 100, 1), Math.Round(MpRateOf(level) * 100, 1), HeartDemonWinOf(level), BreakBonusOf(level), Math.Round((1 - RiskRateOf(level)) * 100))).Format(null, 15);
            row++;
            for (var l = 0; l <= MAX_LEVEL; l++)
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("heart_level_line"), LevelName(l), l == 0 ? 0 : LEVELS[l - 1], Math.Round(SpRateOf(l, 0) * 100), Math.Round(MpRateOf(l) * 100), HeartDemonWinOf(l)))
                    .Format(l == level ? new Color(0.1f, 0.4f, 0.1f) : new Color(0.35f, 0.35f, 0.35f), 13);
            ui.AddText(ui.MidCol, ui.LastRow - 1, GameTool.LS("heart_help")).Format(null, 13);
        }
    }
}
