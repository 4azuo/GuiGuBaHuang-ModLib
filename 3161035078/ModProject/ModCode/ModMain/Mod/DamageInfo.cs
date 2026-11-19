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
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Damage calculation window: the player's attack and defense numbers of the damage pipeline of
    /// <see cref="BattleModifyEvent"/> (base value of the game + bonus of the mod), the target compared (in a battle: the
    /// last unit the player hit or was hit by; outside: a cultivator with the player's own numbers), an example hit
    /// through every step without the random parts, and the last hit dealt / taken step by step (page 1); the last 3
    /// monsters and 3 cultivators fought, with their picture and their numbers of that time, and the damage both ways
    /// against each (page 2, kept in the save by <see cref="DamageInfoEvent"/>).
    /// Opened from a button of the battle screen, the hotkey "Damage calculation" (Taoist Settings, default F9) and a
    /// button of the player info (to work it out before a battle). The battle is paused while it is open.
    /// </summary>
    public static class DamageInfo
    {
        /// <summary>Default hotkey (KeyCode.F9).</summary>
        public const int DEFAULT_KEY = 290;
        public static readonly string[] BASIS = { "Blade", "Spear", "Sword", "Fist", "Palm", "Finger", "Fire", "Froze", "Thunder", "Wind", "Earth", "Wood" };
        /// <summary>Icon (sprite of the game's TextMeshPro asset) of each basis, same order as BASIS.</summary>
        public static readonly string[] BASIS_ICONS = { "daofa", "qiangfa", "jianfa", "quanfa", "zhangfa", "zhifa", "huolinggen", "shuilinggen", "leilinggen", "fenglinggen", "tulinggen", "mulinggen" };
        /// <summary>Foes kept of each kind (monsters, cultivators).</summary>
        public const int RECENT_FOES = 3;
        /// <summary>A foe is recorded again at most every this many seconds (a battle has many hits).</summary>
        public const float RECORD_EVERY_SECONDS = 2f;

        // ------------------------------------------------------------------ last hits

        /// <summary>One hit of the battle: the value after each step of the pipeline.</summary>
        public class Hit
        {
            public string Attacker { get; set; }
            public string Target { get; set; }
            public List<Tuple<string, int>> Steps { get; } = new List<Tuple<string, int>>();
        }

        public static Hit LastDealt { get; private set; }
        public static Hit LastTaken { get; private set; }
        /// <summary>The unit the player hit or was hit by last (the target of the window in a battle).</summary>
        public static UnitCtrlBase LastFoe { get; private set; }
        private static Hit current;
        private static readonly Dictionary<string, float> recordedAt = new Dictionary<string, float>();

        /// <summary>Start of a hit (BattleModifyEvent): recorded when the player deals or takes it.</summary>
        public static void BeginHit()
        {
            current = null;
            var atkPlayer = ModBattleEvent.IsWorldUnitAttacking && ModBattleEvent.AttackingWorldUnit.IsPlayer();
            var hitPlayer = ModBattleEvent.IsWorldUnitHit && ModBattleEvent.HitWorldUnit.IsPlayer();
            if (!atkPlayer && !hitPlayer) return;
            current = new Hit { Attacker = UnitName(ModBattleEvent.AttackingUnit), Target = UnitName(ModBattleEvent.HitUnit) };
            if (atkPlayer) { LastDealt = current; LastFoe = ModBattleEvent.HitUnit; }
            else { LastTaken = current; LastFoe = ModBattleEvent.AttackingUnit; }
            try { RecordFoe(LastFoe); }
            catch (Exception ex) { DebugHelper.WriteLine($"DamageInfo: record foe: {ex.Message}"); }
        }

        /// <summary>Value after a step of the pipeline (BattleModifyEvent.Trace).</summary>
        public static void Step(string step, int value)
        {
            current?.Steps.Add(Tuple.Create(step, value));
        }

        /// <summary>New battle: the hits of the previous one forgotten.</summary>
        public static void Reset()
        {
            LastDealt = LastTaken = current = null;
            LastFoe = null;
            recordedAt.Clear();
        }

        private static string UnitName(UnitCtrlBase u)
        {
            var w = u?.data?.TryCast<UnitDataHuman>()?.worldUnitData?.unit;
            if (w != null) return w.GetName();
            var attr = u?.data?.TryCast<UnitDataMonst>()?.unitAttrItem;
            if (attr != null && !string.IsNullOrEmpty(attr.name))
            {
                var name = GameTool.LS(attr.name);
                if (!string.IsNullOrEmpty(name) && name != attr.name) return name;
            }
            return string.Format(GameTool.LS("dmginfo_monster"), u?.data?.grade?.value ?? 0);
        }

        // ------------------------------------------------------------------ recent foes

        /// <summary>
        /// A foe of a battle with its numbers at the time it was fought (the attack and defense numbers of the pipeline
        /// and the chances / shields worked out then): the damage both ways is worked out from them later.
        /// </summary>
        public class FoeRecord
        {
            public bool Human { get; set; }
            public string Name { get; set; }
            public int Grade { get; set; }
            /// <summary>Monster: id of its ConfBattleUnitBase (model of the picture).</summary>
            public int BaseId { get; set; }
            /// <summary>Cultivator: unit id (portrait).</summary>
            public string UnitId { get; set; }
            public int Atk { get; set; }
            public int Def { get; set; }
            public int Hp { get; set; }
            public int HpMax { get; set; }
            public int Mp { get; set; }
            public int MpMax { get; set; }
            public int Sp { get; set; }
            public int SpMax { get; set; }
            public int Dp { get; set; }
            public int DpMax { get; set; }
            public Dictionary<string, int> Basis { get; set; } = new Dictionary<string, int>();
            public double Evade { get; set; }
            public double BlockMax { get; set; }
            public double BlockDmg { get; set; }
            public float BlockRatio { get; set; }
            public double CritChance { get; set; }
            public double CritDamage { get; set; }
            public int MinDmg { get; set; }
            public int SkillBonus { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }

            /// <summary>Same foe (a cultivator by its unit, a monster by its kind and grade).</summary>
            public string Identity => Human ? $"h:{UnitId}" : $"m:{BaseId}:{Grade}";
        }

        /// <summary>The numbers of a unit now (in a battle: its battle numbers).</summary>
        public static FoeRecord Snapshot(Side s, int baseId = 0)
        {
            var r = new FoeRecord
            {
                Human = s.Human, Name = s.Name, Grade = s.Grade, BaseId = baseId, UnitId = s.World?.GetUnitId(),
                Atk = s.Atk, Def = s.Def, Hp = s.Hp, HpMax = s.HpMax, Mp = s.Mp, MpMax = s.MpMax, Sp = s.Sp, SpMax = s.SpMax, Dp = s.Dp, DpMax = s.DpMax,
                Basis = new Dictionary<string, int>(s.Basis),
                Evade = EvadeChance(s), BlockMax = BlockMax(s), BlockDmg = BlockDmg(s), BlockRatio = BlockRatio(s),
                CritChance = CritChance(s), CritDamage = CritDamage(s), MinDmg = MinDmgPerGrade(s), SkillBonus = SkillBonus(s),
            };
            if (GameHelper.IsInGame()) { r.Year = GameHelper.GetGameYear(); r.Month = GameHelper.GetGameMonth(); }
            return r;
        }

        /// <summary>Records a foe of the player (newest first, RECENT_FOES of each kind).</summary>
        public static void RecordFoe(UnitCtrlBase u)
        {
            if (u == null || DamageInfoEvent.Instance == null) return;
            var key = u.GetHashCode().ToString();
            var now = Time.unscaledTime;
            if (recordedAt.TryGetValue(key, out var at) && now - at < RECORD_EVERY_SECONDS) return;
            recordedAt[key] = now;
            var side = FromCtrl(u);
            if (side.World != null && side.World.IsPlayer()) return;
            var baseId = u.data?.TryCast<UnitDataMonst>()?.unitBaseItem?.id ?? 0;
            AddRecord(Snapshot(side, baseId));
        }

        /// <summary>Adds a record: replaces the same foe, keeps RECENT_FOES of its kind.</summary>
        public static void AddRecord(FoeRecord r)
        {
            var list = DamageInfoEvent.Instance?.Foes;
            if (list == null || r == null) return;
            list.RemoveAll(x => x.Identity == r.Identity);
            list.Insert(0, r);
            foreach (var human in new[] { true, false })
            {
                var extra = list.Where(x => x.Human == human).Skip(RECENT_FOES).ToList();
                foreach (var x in extra) list.Remove(x);
            }
        }

        /// <summary>The recorded foes of a kind, newest first.</summary>
        public static List<FoeRecord> RecentFoes(bool human) =>
            DamageInfoEvent.Instance?.Foes.Where(x => x.Human == human).Take(RECENT_FOES).ToList() ?? new List<FoeRecord>();

        // ------------------------------------------------------------------ numbers of a side

        /// <summary>The numbers of one side (attacker or defender) used by the pipeline.</summary>
        public class Side
        {
            public string Name;
            public WorldUnitBase World;     // null for a monster (and for a recorded foe)
            public UnitCtrlBase Ctrl;       // null outside a battle
            public FoeRecord Snap;          // a recorded foe: its numbers of that time
            public int Grade, Atk, Def, Hp, HpMax, Mp, MpMax, Sp, SpMax, Dp, DpMax;
            public Dictionary<string, int> Basis = new Dictionary<string, int>();

            public string MainBasis => Basis.Count == 0 ? BASIS[0] : Basis.OrderByDescending(x => x.Value).First().Key;
            public bool Human => Snap != null ? Snap.Human : World != null;
            public int BasisOf(string b) => Basis.TryGetValue(b, out var v) ? v : 0;
        }

        public static Side FromWorld(WorldUnitBase w, UnitCtrlBase ctrl = null)
        {
            var s = new Side { World = w, Ctrl = ctrl, Name = w.GetName(), Grade = w.GetGradeLvl() };
            var h = ctrl?.data?.TryCast<UnitDataHuman>();
            if (h != null)
            {
                s.Atk = h.attack.baseValue; s.Def = h.defense.baseValue;
                s.Hp = h.hp; s.HpMax = h.maxHP.value; s.Mp = h.mp; s.MpMax = h.maxMP.value;
                s.Sp = h.sp; s.SpMax = h.maxSP.value; s.Dp = h.dp; s.DpMax = h.maxDP.value;
            }
            else
            {
                s.Atk = w.GetDynProperty(UnitDynPropertyEnum.Attack).value; s.Def = w.GetDynProperty(UnitDynPropertyEnum.Defense).value;
                s.Hp = w.GetDynProperty(UnitDynPropertyEnum.Hp).value; s.HpMax = w.GetDynProperty(UnitDynPropertyEnum.HpMax).value;
                s.Mp = w.GetDynProperty(UnitDynPropertyEnum.Mp).value; s.MpMax = w.GetDynProperty(UnitDynPropertyEnum.MpMax).value;
                s.Sp = w.GetDynProperty(UnitDynPropertyEnum.Sp).value; s.SpMax = w.GetDynProperty(UnitDynPropertyEnum.SpMax).value;
                s.Dp = w.GetDynProperty(UnitDynPropertyEnum.Dp).value; s.DpMax = w.GetDynProperty(UnitDynPropertyEnum.DpMax).value;
            }
            foreach (var b in BASIS)
            {
                var p = UnitDynPropertyEnum.GetEnumByName<UnitDynPropertyEnum>($"Basis{b}");
                s.Basis[b] = ctrl != null ? ModBattleEvent.GetUnitPropertyValue(ctrl, p) : (p == null ? 0 : w.GetDynProperty(p).value);
            }
            return s;
        }

        public static Side FromCtrl(UnitCtrlBase u)
        {
            var w = u?.data?.TryCast<UnitDataHuman>()?.worldUnitData?.unit;
            if (w != null) return FromWorld(w, u);
            var s = new Side { Ctrl = u, Name = UnitName(u), Grade = u.data.grade.value, Atk = u.data.attack.baseValue, Def = u.data.defense.baseValue,
                Hp = u.data.hp, HpMax = u.data.maxHP.value };
            foreach (var b in BASIS)
                s.Basis[b] = ModBattleEvent.GetUnitPropertyValue(u, UnitDynPropertyEnum.GetEnumByName<UnitDynPropertyEnum>($"Basis{b}"));
            return s;
        }

        /// <summary>The side of a recorded foe (its numbers of that time).</summary>
        public static Side FromRecord(FoeRecord r)
        {
            return new Side
            {
                Snap = r, Name = r.Name, Grade = r.Grade, Atk = r.Atk, Def = r.Def, Hp = r.Hp, HpMax = r.HpMax, Mp = r.Mp, MpMax = r.MpMax,
                Sp = r.Sp, SpMax = r.SpMax, Dp = r.Dp, DpMax = r.DpMax, Basis = new Dictionary<string, int>(r.Basis ?? new Dictionary<string, int>()),
            };
        }

        private static float Ratio(int v, int max) => max <= 0 ? 0f : Math.Max(0f, Math.Min(1f, v / (float)max));

        public static double EvadeChance(Side s) => s.Snap != null ? s.Snap.Evade : s.World != null
            ? Math.Min(BattleModifyEvent.GetEvadeMaxBase(s.World) + BattleModifyEvent.GetEvadeMaxPlus(s.World), Math.Sqrt(s.BasisOf("Wind") / 18d)) + BattleModifyEvent.GetEvadePlus(s.World)
            : Math.Min(12d, Math.Sqrt(s.BasisOf("Wind") / 18d));
        public static double BlockMax(Side s) => s.Snap != null ? s.Snap.BlockMax : s.World != null ? BattleModifyEvent.GetBlockMaxBase(s.World) + BattleModifyEvent.GetBlockMaxPlus(s.World) : 18d;
        public static double BlockChance(Side s, string basis) => Math.Min(BlockMax(s), Math.Sqrt(s.BasisOf(basis) / 12d));
        public static double BlockDmg(Side s) => s.Snap != null ? s.Snap.BlockDmg : s.World != null ? s.Def + BattleModifyEvent.GetBlockDmgPlus(s.World) : s.Def;
        public static float BlockRatio(Side s) => s.Snap != null ? s.Snap.BlockRatio : s.World == null || !BattleModifyEvent.BlockRatio.ContainsKey(s.Grade) ? 0f
            : BattleModifyEvent.BlockRatio[s.Grade] + s.World.GetBasisMagicSum() / BattleModifyEvent.BASIS_ON_BLOCK_RATIO;
        public static double CritChance(Side s) => s.Snap != null ? s.Snap.CritChance : s.World == null ? 0d
            : Math.Min(BattleModifyEvent.GetSCritChanceMaxBase(s.World) + BattleModifyEvent.GetSCritChanceMaxPlus(s.World), BattleModifyEvent.GetSCritChanceBase(s.World)) + BattleModifyEvent.GetSCritChancePlus(s.World);
        public static double CritDamage(Side s) => s.Snap != null ? s.Snap.CritDamage : s.World != null ? BattleModifyEvent.GetSCritDamageBase(s.World) + BattleModifyEvent.GetSCritDamagePlus(s.World) : 1d;
        public static int MinDmgPerGrade(Side s) => s.Snap != null ? s.Snap.MinDmg : s.World != null ? BattleModifyEvent.GetMinDmgBase(s.World) + BattleModifyEvent.GetMinDmgPlus(s.World)
            : s.Ctrl != null ? BattleModifyEvent.GetMinDmgBase(s.Ctrl) : s.Grade;

        public static int SkillBonus(Side s)
        {
            if (s.Snap != null) return s.Snap.SkillBonus;
            var m = s.World?.GetMartialAttack();
            if (m == null) return 0;
            return (int)Math.Min(int.MaxValue, BattleModifyEvent.GetSkillAdjDmgBase(s.World, Tuple.Create(MartialType.SkillLeft, m.data)) * BattleModifyEvent.GetSkillAdjDmgPlus(s.World));
        }

        public static double TimeMultiplier() => GameHelper.IsInBattlle() && BattleModifyEvent.Instance != null ? BattleModifyEvent.Instance.GetDamageMultiplier() : 1d;

        /// <summary>
        /// An example hit of base damage = ATK of the attacker through the steps of the pipeline, without its random
        /// parts (no evasion, no critical, no block chance; the shields of MP/SP/DP as they are now).
        /// </summary>
        public static List<Tuple<string, int>> Example(Side a, Side d, bool crit)
        {
            var steps = new List<Tuple<string, int>>();
            double v = a.Atk;
            steps.Add(Tuple.Create("base", (int)v));
            v += (TimeMultiplier() - 1d) * (a.Grade / 3f) * v; steps.Add(Tuple.Create("time", Clamp(v)));
            if (a.Human) { v += SkillBonus(a); steps.Add(Tuple.Create("skill", Clamp(v))); }
            var basis = a.MainBasis;
            v += (double)a.Atk * a.BasisOf(basis) / BattleModifyEvent.BASIS_ON_DMG; steps.Add(Tuple.Create("basis", Clamp(v)));
            if (a.Human)
            {
                v += a.Atk * Ratio(a.Mp, a.MpMax); steps.Add(Tuple.Create("mp", Clamp(v)));
                if (a.Grade >= 4) { v += a.Atk * Ratio(a.Sp, a.SpMax); steps.Add(Tuple.Create("sp", Clamp(v))); }
                if (a.Grade >= 8) { v += a.Atk * Ratio(a.Dp, a.DpMax); steps.Add(Tuple.Create("dp", Clamp(v))); }
                if (crit) { v += v * CritDamage(a); steps.Add(Tuple.Create("crit", Clamp(v))); }
            }
            var min = (double)MinDmgPerGrade(a) * d.Grade;
            if (v > min) { v -= (double)d.Def * d.BasisOf(basis) / BattleModifyEvent.BASIS_ON_DEF; steps.Add(Tuple.Create("basisdef", Clamp(v))); }
            if (d.Human && d.MpMax > 0 && d.Mp > d.MpMax / 5)
            {
                var mpRate = Ratio(d.Mp, d.MpMax);
                var times = (int)(mpRate * d.Grade);
                for (var i = 0; i < times && v > min; i++) v -= d.Def * mpRate * BlockRatio(d);
                steps.Add(Tuple.Create("mpblock", Clamp(v)));
            }
            if (d.Human && d.Sp > 0 && d.Grade >= 4 && v > min) { v -= d.Def * Ratio(d.Sp, d.SpMax) * BlockRatio(d); steps.Add(Tuple.Create("spblock", Clamp(v))); }
            if (d.Human && d.Dp > 0 && d.Grade >= 8 && v > min) { v -= d.Def * Ratio(d.Dp, d.DpMax) * BlockRatio(d); steps.Add(Tuple.Create("dpblock", Clamp(v))); }
            if (v < min) v = min;
            steps.Add(Tuple.Create("min", Clamp(v)));
            return steps;
        }

        /// <summary>Final value of an example hit.</summary>
        public static int Final(List<Tuple<string, int>> steps) => steps.Count == 0 ? 0 : steps[steps.Count - 1].Item2;

        /// <summary>Hits of an example hit to bring the HP of the defender to 0 (0 when the hit does nothing).</summary>
        public static int HitsToKill(int dmg, int hp) => dmg <= 0 ? 0 : Math.Max(1, (int)Math.Ceiling(Math.Max(1, hp) / (double)dmg));

        private static int Clamp(double v) => (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, v));

        // ------------------------------------------------------------------ window

        private static bool pausedByUs;

        /// <summary>The battle was paused by the window (it lets it run again when closed).</summary>
        public static bool PausedByWindow => pausedByUs;
        private static UICustom1 window;

        // colors of the window: titles, damage dealt / taken, notes (properties: no Unity type in the static constructor)
        public static Color TITLE => new Color(0.45f, 0.12f, 0.08f);
        public static Color DEALT => new Color(0.10f, 0.40f, 0.18f);
        public static Color TAKEN => new Color(0.62f, 0.12f, 0.10f);
        public static Color CRIT => new Color(0.72f, 0.40f, 0.02f);
        public static Color NOTE => new Color(0.35f, 0.35f, 0.35f);

        public static bool IsOpen => window != null && g.ui.HasUI(UIType.TextInfoLong);

        /// <summary>The window (null when closed).</summary>
        public static UICustom1 Window => IsOpen ? window : null;

        /// <summary>Opens the window (closes it when open: the hotkey toggles it).</summary>
        public static void Toggle()
        {
            if (IsOpen) { Close(); return; }
            Open();
        }

        /// <summary>"[icon:NAME]" as the sprite tag of a text.</summary>
        public static string I(string name) => TextIconHelper.ToSprites($"[icon:{name}]") + " ";

        public static string BasisIcon(string basis)
        {
            var i = Array.IndexOf(BASIS, basis);
            return I(i < 0 ? "jineng" : BASIS_ICONS[i]);
        }

        public static void Open(int page = 0)
        {
            var player = g.world?.playerUnit;
            if (player == null) return;
            var inBattle = GameHelper.IsInBattlle() && ModBattleEvent.PlayerUnit != null;
            if (inBattle && Time.timeScale > 0f)
            {
                GameHelper.PauseGame();
                pausedByUs = true;
            }
            var me = FromWorld(player, inBattle ? ModBattleEvent.PlayerUnit : null);
            Side foe;
            string foeTitle;
            if (inBattle && LastFoe != null && !LastFoe.isDie)
            {
                foe = FromCtrl(LastFoe);
                foeTitle = string.Format(GameTool.LS("dmginfo_target"), foe.Name, foe.Grade);
            }
            else
            {
                foe = FromWorld(player, inBattle ? ModBattleEvent.PlayerUnit : null);
                foeTitle = GameTool.LS("dmginfo_target_self");
            }

            g.ui.CloseUI(UIType.TextInfoLong);
            window = new UICustom1(GameTool.LS("dmginfo_title"), okAct: Close);
            window.IsShowNavigationButtons = true;
            window.AddPage();
            StatsPage(window, me, foe, foeTitle, inBattle);
            window.AddPage();
            FoesPage(window, me);
            window.Pages[Math.Max(0, Math.Min(page, window.Pages.Count - 1))].Active();
        }

        /// <summary>A text of the window, left aligned at (x, y) (canvas units of the window).</summary>
        private static UIItemText LText(UICustom1 ui, float x, float y, string text, Color? color = null, int size = 13, FontStyle style = FontStyle.Normal)
        {
            var t = ui.AddText(x, y, text).Format(color ?? Color.black, size, style).Align(TextAnchor.MiddleLeft);
            var rt = t.Item.rectTransform;
            t.Pos(x + rt.rect.width / 2f, y);
            return t;
        }

        private static void StatsPage(UICustom1 ui, Side me, Side foe, string foeTitle, bool inBattle)
        {
            var left = ui.Columns[ui.FirstCol + 1];
            var right = ui.Columns[ui.MidCol + 1];
            var row = ui.FirstRow;
            float Y(int r) => ui.Rows[Math.Min(r, ui.LastRow)];

            // attack (left) and defense (right) of the player
            var basis = me.MainBasis;
            LText(ui, left, Y(row), I("gongji") + GameTool.LS("dmginfo_attack"), TITLE, 17, FontStyle.Bold);
            LText(ui, right, Y(row), I("fangyu") + GameTool.LS("dmginfo_defense"), TITLE, 17, FontStyle.Bold);
            row++;
            var a = new List<string>
            {
                I("gongji") + string.Format(GameTool.LS("dmginfo_atk"), me.Atk),
                I("chixushijian") + string.Format(GameTool.LS("dmginfo_time"), TimeMultiplier().ToString("0.00")),
                I("jineng") + string.Format(GameTool.LS("dmginfo_skill"), SkillName(me), SkillBonus(me)),
                BasisIcon(basis) + string.Format(GameTool.LS("dmginfo_basis"), BasisName(basis), me.BasisOf(basis), (long)me.Atk * me.BasisOf(basis) / BattleModifyEvent.BASIS_ON_DMG),
                I("lingli") + string.Format(GameTool.LS("dmginfo_mp"), (Ratio(me.Mp, me.MpMax) * 100).ToString("0"), (int)(me.Atk * Ratio(me.Mp, me.MpMax))),
                I("nianli") + string.Format(GameTool.LS("dmginfo_sp"), (Ratio(me.Sp, me.SpMax) * 100).ToString("0"), me.Grade >= 4 ? (int)(me.Atk * Ratio(me.Sp, me.SpMax)) : 0),
                I("daoli") + string.Format(GameTool.LS("dmginfo_dp"), (Ratio(me.Dp, me.DpMax) * 100).ToString("0"), me.Grade >= 8 ? (int)(me.Atk * Ratio(me.Dp, me.DpMax)) : 0),
                I("huixin") + string.Format(GameTool.LS("dmginfo_crit"), CritChance(me).ToString("0.0"), (1d + CritDamage(me)).ToString("0.00")),
                I("zhijieshanghai") + string.Format(GameTool.LS("dmginfo_kill"), (BattleModifyEvent.GetInstantKillBase(me.World) + BattleModifyEvent.GetInstantKillPlus(me.World)).ToString("0.00")),
                I("shanghaiweili") + string.Format(GameTool.LS("dmginfo_mindmg"), MinDmgPerGrade(me), foe.Grade, MinDmgPerGrade(me) * foe.Grade),
                I("huifutili") + string.Format(GameTool.LS("dmginfo_steal"), (BattleModifyEvent.GetStealHpBase(me.World) + BattleModifyEvent.GetStealHpPlus(me.World)).ToString("0.0%"),
                    (BattleModifyEvent.GetStealMpBase(me.World) + BattleModifyEvent.GetStealMpPlus(me.World)).ToString("0.0%"),
                    (BattleModifyEvent.GetStealSpBase(me.World) + BattleModifyEvent.GetStealSpPlus(me.World)).ToString("0.0%")),
            };
            var d = new List<string>
            {
                I("fangyu") + string.Format(GameTool.LS("dmginfo_def"), me.Def),
                I("shanbiD") + string.Format(GameTool.LS("dmginfo_evade"), EvadeChance(me).ToString("0.0"), (BattleModifyEvent.GetEvadeMaxBase(me.World) + BattleModifyEvent.GetEvadeMaxPlus(me.World)).ToString("0.0"), BattleModifyEvent.GetEvadePlus(me.World).ToString("0.0")),
                I("hudun") + string.Format(GameTool.LS("dmginfo_block"), BlockChance(me, foe.MainBasis).ToString("0.0"), BlockMax(me).ToString("0.0"), (int)BlockDmg(me)),
                BasisIcon(foe.MainBasis) + string.Format(GameTool.LS("dmginfo_basisdef"), BasisName(foe.MainBasis), me.BasisOf(foe.MainBasis), (long)me.Def * me.BasisOf(foe.MainBasis) / BattleModifyEvent.BASIS_ON_DEF),
                I("lingli") + string.Format(GameTool.LS("dmginfo_mpblock"), (int)(Ratio(me.Mp, me.MpMax) * me.Grade), (int)(me.Def * Ratio(me.Mp, me.MpMax) * BlockRatio(me)), BlockRatio(me).ToString("0.00")),
                I("nianli") + string.Format(GameTool.LS("dmginfo_spblock"), me.Grade >= 4 ? (int)(me.Def * Ratio(me.Sp, me.SpMax) * BlockRatio(me)) : 0),
                I("daoli") + string.Format(GameTool.LS("dmginfo_dpblock"), me.Grade >= 8 ? (int)(me.Def * Ratio(me.Dp, me.DpMax) * BlockRatio(me)) : 0),
                I("jiankang") + string.Format(GameTool.LS("dmginfo_hp"), me.Hp, me.HpMax),
            };
            for (var i = 0; i < Math.Max(a.Count, d.Count); i++, row++)
            {
                if (i < a.Count) LText(ui, left, Y(row), a[i]);
                if (i < d.Count) LText(ui, right, Y(row), d[i]);
            }
            row++;

            // the target and the example hits both ways
            LText(ui, left, Y(row++), I("mubiaogeshu") + foeTitle, TITLE, 16, FontStyle.Bold);
            LText(ui, left, Y(row++), string.Format(GameTool.LS("dmginfo_foe"), foe.Atk, foe.Def, foe.Hp, foe.HpMax, EvadeChance(foe).ToString("0.0"), BlockChance(foe, basis).ToString("0.0")));
            LText(ui, left, Y(row++), I("gongji") + GameTool.LS("dmginfo_example_dealt") + "  " + Steps(Example(me, foe, false)), DEALT);
            LText(ui, left, Y(row++), I("huixinbeilv") + GameTool.LS("dmginfo_example_crit") + "  " + Steps(Example(me, foe, true)), CRIT);
            LText(ui, left, Y(row++), I("shanghaijianmian") + GameTool.LS("dmginfo_example_taken") + "  " + Steps(Example(foe, me, false)), TAKEN);
            row++;

            // last hits of the battle
            LText(ui, left, Y(row++), I("zhandouxiaoguo") + GameTool.LS("dmginfo_last"), TITLE, 16, FontStyle.Bold);
            LText(ui, left, Y(row++), LastLine("dmginfo_last_dealt", LastDealt), DEALT);
            LText(ui, left, Y(row++), LastLine("dmginfo_last_taken", LastTaken), TAKEN);
            LText(ui, left, Y(row++), GameTool.LS(inBattle ? "dmginfo_paused" : "dmginfo_foes_hint"), NOTE, 12, FontStyle.Italic);
        }

        /// <summary>Page 2: the recent monsters (top) and cultivators (bottom), a card each with its picture.</summary>
        private static void FoesPage(UICustom1 ui, Side me)
        {
            var x0 = ui.Columns[ui.FirstCol];
            var width = ui.Columns[ui.LastCol] - x0;
            var cardW = width / RECENT_FOES;
            var top = ui.Rows[ui.FirstRow];
            var dy = UIHelper.GetUIDeltaY();
            LText(ui, x0 + 10f, top, I("zm_kuangshi") + GameTool.LS("dmginfo_foes_monsters"), TITLE, 17, FontStyle.Bold);
            FoeCards(ui, me, RecentFoes(false), x0, top - dy * 1.2f, cardW);
            var mid = top - dy * 12f;
            LText(ui, x0 + 10f, mid, I("zm_renshu") + GameTool.LS("dmginfo_foes_humans"), TITLE, 17, FontStyle.Bold);
            FoeCards(ui, me, RecentFoes(true), x0, mid - dy * 1.2f, cardW);
        }

        public const float PICTURE_W = 130f;
        public const float PICTURE_H = 190f;
        /// <summary>Part of the portrait texture shown (x, y from the bottom, width, height; same share both ways: no stretch).</summary>
        public static Rect HUMAN_UV => new Rect(0.22f, 0.40f, 0.56f, 0.56f);
        /// <summary>Part of a monster's battle model texture shown.</summary>
        public static Rect MONSTER_UV => new Rect(0.16f, 0.30f, 0.68f, 0.68f);
        /// <summary>Offset of a monster's battle model in its picture (the game draws it high: its head was cut).</summary>
        public static float MonsterOffsetY = -2.3f;   // world units: -2 shows the head and the feet (TZ-99 experiment, 2026-11-19); a float: no Unity type in the static constructor

        private static void FoeCards(UICustom1 ui, Side me, List<FoeRecord> foes, float x0, float top, float cardW)
        {
            var dy = UIHelper.GetUIDeltaY();
            if (foes.Count == 0)
            {
                LText(ui, x0 + 30f, top - dy, GameTool.LS("dmginfo_foes_none"), NOTE, 14, FontStyle.Italic);
                return;
            }
            for (var i = 0; i < foes.Count; i++)
            {
                var r = foes[i];
                var f = FromRecord(r);
                var x = x0 + i * cardW + 10f;
                Picture(ui, r, x + PICTURE_W / 2f, top - PICTURE_H / 2f);
                var tx = x + PICTURE_W + 12f;
                var y = top - dy * 0.4f;
                var dealt = Final(Example(me, f, false));
                var crit = Final(Example(me, f, true));
                var taken = Final(Example(f, me, false));
                LText(ui, tx, y, r.Name, TITLE, 15, FontStyle.Bold); y -= dy;
                LText(ui, tx, y, string.Format(GameTool.LS("dmginfo_foe_grade"), r.Grade, r.Year, r.Month), NOTE, 12); y -= dy;
                LText(ui, tx, y, $"{I("gongji")}{r.Atk:#,##0}   {I("fangyu")}{r.Def:#,##0}", null, 13); y -= dy;
                LText(ui, tx, y, $"{I("jiankang")}{r.HpMax:#,##0}   {I("shanbiD")}{r.Evade:0.0}%", null, 13); y -= dy;
                LText(ui, tx, y, string.Format(GameTool.LS("dmginfo_foe_dealt"), dealt, crit, HitsToKill(dealt, r.HpMax)), DEALT, 13); y -= dy;
                LText(ui, tx, y, string.Format(GameTool.LS("dmginfo_foe_taken"), taken, HitsToKill(taken, me.HpMax)), TAKEN, 13);
            }
        }

        /// <summary>The picture of a foe: the battle model of a monster, the portrait of a cultivator (still in the world).</summary>
        private static void Picture(UICustom1 ui, FoeRecord r, float cx, float cy)
        {
            try
            {
                // a transparent image item holds the picture: it follows the page (shown / hidden with it)
                var holder = ui.AddImage(cx, cy, null);
                holder.Item.color = new Color(0f, 0f, 0f, 0.06f);
                holder.Size(PICTURE_W, PICTURE_H);
                var size = new Vector2(PICTURE_W, PICTURE_H);
                if (r.Human)
                {
                    var unit = string.IsNullOrEmpty(r.UnitId) ? null : g.world.unit.GetUnit(r.UnitId);
                    var img = unit == null ? null : PortraitHelper.Draw(holder.Item.transform, unit, size, Vector2.zero);
                    // the game draws the art in the top half of the texture (12-54 % of its height): that part, zoomed
                    // with the same ratio, fills the frame
                    if (img != null) img.uvRect = HUMAN_UV;
                }
                else if (r.BaseId > 0)
                {
                    var go = new GameObject("MonsterPicture");
                    go.transform.SetParent(holder.Item.transform, false);
                    var rimg = go.AddComponent<RawImage>();
                    rimg.raycastTarget = false;
                    var rt = rimg.rectTransform;
                    rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
                    rt.sizeDelta = size;
                    rt.anchoredPosition = Vector2.zero;
                    g.conf.battleUnitBase.CreateModel(r.BaseId, rimg, (int)PICTURE_W, (int)PICTURE_H, new Vector2(0f, MonsterOffsetY));
                    // the battle model stands in the top two thirds of the texture: that part fills the frame
                    rimg.uvRect = MONSTER_UV;
                }
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"DamageInfo: picture of {r.Name}: {ex.Message}");
            }
        }

        private static string LastLine(string key, Hit h) => h == null
            ? $"{GameTool.LS(key)} {GameTool.LS("dmginfo_none")}"
            : $"{GameTool.LS(key)} {h.Attacker} -> {h.Target}:  {Steps(h.Steps)}";

        /// <summary>"base 100 > +skill 150 > ... > final 80": the value after every step that changed it.</summary>
        public static string Steps(List<Tuple<string, int>> steps)
        {
            var parts = new List<string>();
            int? last = null;
            foreach (var s in steps)
            {
                if (last == s.Item2 && s.Item1 != "min" && s.Item1 != "base") continue;
                parts.Add($"{GameTool.LS("dmginfo_step_" + s.Item1)} {s.Item2:#,##0}");
                last = s.Item2;
            }
            return string.Join("  >  ", parts);
        }

        /// <summary>Name of the equipped attack skill, "-" without one.</summary>
        public static string SkillName(Side s)
        {
            // the name of a technique is in its martial info (its props item has none: was always "-")
            var m = s.World?.GetMartialAttack();
            var key = m?.martialInfo?.name ?? m?.data?.propsItem?.name;
            return string.IsNullOrEmpty(key) ? "-" : GameTool.LS(key);
        }

        public static string BasisName(string basis) => GameTool.LS("dmginfo_b_" + basis.ToLowerInvariant());

        /// <summary>Closes the window and lets the battle run again when it was paused by it.</summary>
        public static void Close()
        {
            if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
            window = null;
            if (pausedByUs) { pausedByUs = false; GameHelper.UnPauseGame(); }
        }

        /// <summary>Window closed another way (Esc, X): the battle runs again (BattleModifyEvent.OnTimeUpdate200ms).</summary>
        public static void Watch()
        {
            if (pausedByUs && !g.ui.HasUI(UIType.TextInfoLong)) { pausedByUs = false; window = null; GameHelper.UnPauseGame(); }
        }
    }

    /// <summary>
    /// The foes of <see cref="DamageInfo"/> kept in the save: the last monsters and cultivators fought with their numbers
    /// of that time (page 2 of the damage calculation window).
    /// </summary>
    [Cache(ModConst.DAMAGE_INFO_EVENT)]
    public class DamageInfoEvent : ModEvent
    {
        public static DamageInfoEvent Instance { get; set; }

        public List<DamageInfo.FoeRecord> Foes { get; set; } = new List<DamageInfo.FoeRecord>();
    }
}
