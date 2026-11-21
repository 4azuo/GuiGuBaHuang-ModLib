using MOD_nE7UL2.Helper;
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
    /// Artisan Dao (nghệ đạo): the crafts of the game as a path of cultivation - alchemy, artifact forging, talismans.
    /// - What the game's craft windows put in the bag (the bag before the window opened against the bag after it closed)
    ///   gives craft exp: 10 x item level² x √count.
    /// - 5 ranks a craft (Apprentice, Craftsman, Master, Grandmaster, Saint): the first comes by itself, the others by a
    ///   yearly exam in a city (a fee, a chance from the craft value of the game); every rank adds +15 to the craft value.
    /// - A masterpiece (an item above the usual level of the cultivator's grade) gives cultivation exp and keeps the name of
    ///   its maker; crafting two levels above the usual level may blow up the furnace (HP and lifespan lost).
    /// - NPCs with a craft value raise their craft every month and pass their exams.
    /// - A Saint's exp becomes the essence of the craft (craft value, attack / defense / max HP); a huge essence returns
    ///   the artisan to simplicity (phản phác quy chân): a large bonus to every stat, once.
    /// - The world: NPCs commission items (deliver one of the craft and level asked for a reward); a Master takes
    ///   apprentices who bring items; the title raises the sell price at the market. Feng shui is a craft too.
    /// Off with S&amp;M "No Artisan Dao".
    /// </summary>
    [Cache(ModConst.ARTISAN_EVENT)]
    public partial class ArtisanEvent : ModEvent
    {
        public static ArtisanEvent Instance { get; set; }

        public enum Craft { Alchemy = 1, Forge = 2, Talisman = 3, Geomancy = 4, Herbal = 5, Mining = 6 }
        public static readonly Craft[] CRAFTS = { Craft.Alchemy, Craft.Forge, Craft.Talisman, Craft.Geomancy, Craft.Herbal, Craft.Mining };
        /// <summary>Gathering crafts: herbs and ores taken on the map (no furnace, no accident).</summary>
        public static bool IsGathering(Craft c) => c == Craft.Herbal || c == Craft.Mining;
        /// <summary>Destiny (role feature, ModConf artisan_return_roleCreateFeature.json) of the return to simplicity of a craft:
        /// +50 to that craft, attack / defense / max HP +15 %, Luck +3.</summary>
        public const int RETURN_DESTINY = 420201000;
        public static int ReturnDestiny(Craft c) => RETURN_DESTINY + (int)c;

        // essence of a Saint, return to simplicity
        public const int ESSENCE_VALUE = 2;              // craft value an essence step
        public const float ESSENCE_STAT = 0.001f;        // attack / defense / max HP an essence step (all crafts)
        public const int RETURN_STEPS = 40;              // essence steps of one craft to return to simplicity
        // the return to simplicity of a craft is a destiny (ReturnDestiny): its bonuses are the destiny's effects
        public const float RETURN_STAT = 0.15f;          // attack, defense, max HP (shown; the destiny gives them)
        public const int RETURN_VALUE = 50;              // that craft's value
        public const int RETURN_LUCK = 3;
        // commissions, apprentices, market
        public const int MAX_COMMISSIONS = 3;
        public const float COMMISSION_RATE = 2f;         // % a month a rank of the best craft
        public const int COMMISSION_MONTHS = 12;
        public const int COMMISSION_REWARD = 4;          // x item level x EncounterEvent.Cost()
        public const int COMMISSION_INTIM = 50;
        public const int APPRENTICE_RANK = 3;            // Master: one apprentice, +1 a rank above
        public const float APPRENTICE_GIFT = 25f;        // % a month an apprentice brings an item
        public const int APPRENTICE_EXP = 50;            // craft exp a month of an apprentice
        public const float SELL_RATE_RANK = 2f;          // market sell rate (%) a rank of the best craft
        public const int MAX_CRAFTED_IDS = 200;

        public const int MAX_RANK = 5;
        public const int EXP_BASE = 100;           // exp of rank 0 -> 1; x4 a rank
        public const int EXP_PER_ITEM = 10;        // x item level²
        public const int RANK_VALUE = 15;          // craft value of the game a rank
        public const float MASTERPIECE_EXP = 0.03f;   // share of the phase exp a level above the usual one
        public const float MASTERPIECE_RANK = 0.25f;  // x(1 + 0.25 x rank)
        public const float ACCIDENT_CHANCE = 25f;     // % when crafting 2 levels or more above the usual one
        public const float ACCIDENT_HP = 0.2f;
        public const int ACCIDENT_LIFE = 3;           // months
        public const int EXAM_FEE = 3;                // x rank x EncounterEvent.Cost()
        public const int MAX_MASTERPIECES = 50;
        /// <summary>NPCs: a craft value from this level makes a crafter; exp a month = this share of the value.</summary>
        public const int NPC_MIN_VALUE = 100;
        public const float NPC_EXP_SHARE = 0.1f;

        public class CraftData
        {
            public int Rank { get; set; }
            public long Exp { get; set; }
            /// <summary>Craft value given by the rank (kept in step).</summary>
            public int Given { get; set; }
            public int LastExamYear { get; set; } = -1;
            public int Masterpieces { get; set; }
            /// <summary>Exp of a Saint: the essence of the craft.</summary>
            public long Essence { get; set; }
            /// <summary>Item ids made in this craft (commissions, apprentices).</summary>
            public List<int> CraftedIds { get; set; } = new List<int>();
            /// <summary>This craft returned to simplicity (its destiny given).</summary>
            public bool Returned { get; set; }
        }

        public class Commission
        {
            public string ClientId { get; set; }
            public int Craft { get; set; }
            public int Level { get; set; }
            public int Reward { get; set; }
            public int Deadline { get; set; }
        }

        public class Apprentice
        {
            public string UnitId { get; set; }
            public int Craft { get; set; }
        }

        public class Masterpiece
        {
            public string Maker { get; set; }
            public int Craft { get; set; }
            public int PropsId { get; set; }
            public int Month { get; set; }
        }

        /// <summary>Unit id -> craft -> data.</summary>
        public Dictionary<string, Dictionary<int, CraftData>> Artisans { get; set; } = new Dictionary<string, Dictionary<int, CraftData>>();
        public List<Masterpiece> Masterpieces { get; set; } = new List<Masterpiece>();
        /// <summary>The player's open commissions and apprentices.</summary>
        public List<Commission> Commissions { get; set; } = new List<Commission>();
        public List<Apprentice> Apprentices { get; set; } = new List<Apprentice>();

        public override bool OnCacheHandler() => !(SMLocalConfigsEvent.Instance?.Configs?.NoArtisan ?? false);
        public static bool Enabled => Instance != null && !(SMLocalConfigsEvent.Instance?.Configs?.NoArtisan ?? false);
        private static WorldUnitBase Player => g.world.playerUnit;

        // ------------------------------------------------------------------ formulas (pure)

        /// <summary>Exp from a rank to the next: 100, 400, 1,600, 6,400, 25,600.</summary>
        public static long NeedExp(int rank) => EXP_BASE * (long)Math.Pow(4, Math.Max(0, Math.Min(MAX_RANK - 1, rank)));
        /// <summary>Craft exp of some items of a level (1-6).</summary>
        public static long CraftExp(int itemLevel, int count) => (long)Math.Round(EXP_PER_ITEM * Math.Max(1, itemLevel) * Math.Max(1, itemLevel) * Math.Sqrt(Math.Max(1, count)));
        /// <summary>The usual item level of a grade: 1 for grades 1-2 ... 5 for grades 9-10.</summary>
        public static int UsualLevel(int grade) => Math.Max(1, Math.Min(6, 1 + (Math.Max(1, grade) - 1) / 2));
        public static bool IsMasterpiece(int itemLevel, int grade) => itemLevel > UsualLevel(grade);
        public static bool CanBlowUp(int itemLevel, int grade) => itemLevel >= UsualLevel(grade) + 2;
        /// <summary>Cultivation exp of a masterpiece: 3 % of the phase a level above the usual one, x(1 + 0.25 x rank).</summary>
        public static long MasterpieceExp(long phaseExp, int itemLevel, int grade, int rank) =>
            itemLevel <= UsualLevel(grade) ? 0 : (long)Math.Round(phaseExp * (double)MASTERPIECE_EXP * (itemLevel - UsualLevel(grade)) * (1 + MASTERPIECE_RANK * rank));
        /// <summary>Chance (%) to pass the exam of the next rank: 30 + craft value / 20 + 10 with a masterpiece, 10-90.</summary>
        public static float ExamChance(int craftValue, int rank, bool masterpiece) =>
            Math.Max(10f, Math.Min(90f, 30f + craftValue / 20f + (masterpiece ? 10f : 0f) - 5f * Math.Max(0, rank - 1)));
        /// <summary>Adds exp; the first rank comes by itself, the others stop at their exam. Returns the ranks gained.</summary>
        public static int AddExp(CraftData d, long exp)
        {
            if (d == null || exp <= 0) return 0;
            exp = (long)Math.Round(exp * SMGlobalConfigsEvent.Rate(c => c.ArtisanExpRate));
            if (d.Rank >= MAX_RANK) { d.Essence += exp; return 0; }
            d.Exp = Math.Min(NeedExp(d.Rank), d.Exp + exp);
            if (d.Rank == 0 && d.Exp >= NeedExp(0)) { d.Rank = 1; d.Exp = 0; return 1; }
            return 0;
        }
        /// <summary>Essence step of a Saint: the exp of the last rank (25,600).</summary>
        public static long EssenceStep => NeedExp(MAX_RANK - 1);
        public static int EssenceSteps(long essence) => (int)Math.Min(int.MaxValue, Math.Max(0, essence) / EssenceStep);
        /// <summary>A Saint whose essence reached RETURN_STEPS returns to simplicity in that craft (once a craft).</summary>
        public static bool CanReturn(CraftData d) => d != null && !d.Returned && d.Rank >= MAX_RANK && EssenceSteps(d.Essence) >= RETURN_STEPS;
        /// <summary>Share of attack / defense / max HP from the essence of every craft (the returns are destinies).</summary>
        public static float StatRate(int essenceSteps) => ESSENCE_STAT * essenceSteps;
        /// <summary>Sell rate bonus (%) at the market of the title of the best craft.</summary>
        public static float SellRateOf(int bestRank) => SELL_RATE_RANK * Math.Max(0, bestRank);
        /// <summary>Apprentices a rank allows: Master 1, Grandmaster 2, Saint 3.</summary>
        public static int MaxApprentices(int rank) => Math.Max(0, rank - APPRENTICE_RANK + 1);
        /// <summary>Reward of a commission of an item level for a cost unit.</summary>
        public static int CommissionReward(int level, int cost) => COMMISSION_REWARD * Math.Max(1, level) * Math.Max(1, cost);
        public static bool ExamReady(CraftData d) => d != null && d.Rank >= 1 && d.Rank < MAX_RANK && d.Exp >= NeedExp(d.Rank);

        // ------------------------------------------------------------------ data

        public static UnitPropertyEnum ValueOf(Craft c) => c == Craft.Alchemy ? UnitPropertyEnum.RefineElixir : c == Craft.Forge ? UnitPropertyEnum.RefineWeapon : c == Craft.Talisman ? UnitPropertyEnum.Symbol : c == Craft.Geomancy ? UnitPropertyEnum.Geomancy
            : c == Craft.Herbal ? UnitPropertyEnum.Herbal : UnitPropertyEnum.Mine;
        public static int CraftValue(WorldUnitBase w, Craft c) => w.GetProperty<int>(ValueOf(c));
        public static string CraftName(Craft c) => GameTool.LS($"artisan_craft_{(int)c}");
        /// <summary>The game's icon of a craft (a sprite tag and a space, for the window lines).</summary>
        public static string CraftIcon(Craft c) => TextIconHelper.ToSprites($"[icon:{CRAFT_ICONS[(int)c - 1]}] ");
        private static readonly string[] CRAFT_ICONS = { "liandan", "lianqi", "huafu", "fengshui", "zm_yaocai", "zm_kuangshi" };
        public static string RankName(int rank) => GameTool.LS($"artisan_rank_{Math.Max(0, Math.Min(MAX_RANK, rank))}");

        public static CraftData Get(WorldUnitBase w, Craft c, bool create = false)
        {
            if (w == null || !Enabled) return null;
            var id = w.GetUnitId();
            if (!Instance.Artisans.TryGetValue(id, out var crafts))
            {
                if (!create) return null;
                Instance.Artisans[id] = crafts = new Dictionary<int, CraftData>();
            }
            if (!crafts.TryGetValue((int)c, out var d) && create)
                crafts[(int)c] = d = new CraftData();
            return d;
        }
        public static int Rank(WorldUnitBase w, Craft c) => Get(w, c)?.Rank ?? 0;

        /// <summary>Keeps the craft value given by the rank in step.</summary>
        public static void Sync(WorldUnitBase w, Craft c)
        {
            var d = Get(w, c);
            if (d == null) return;
            var want = RANK_VALUE * d.Rank + ESSENCE_VALUE * EssenceSteps(d.Essence);
            if (want != d.Given)
            {
                w.AddProperty<int>(ValueOf(c), want - d.Given);
                d.Given = want;
            }
        }

        public static bool IsReturned(WorldUnitBase w, Craft c) => Get(w, c)?.Returned ?? false;
        public static List<Craft> ReturnedCrafts(WorldUnitBase w) => CRAFTS.Where(c => IsReturned(w, c)).ToList();
        public static int TotalEssenceSteps(WorldUnitBase w) =>
            w == null || !Enabled || !Instance.Artisans.TryGetValue(w.GetUnitId(), out var cs) ? 0 : cs.Values.Sum(x => EssenceSteps(x.Essence));
        public static int BestRank(WorldUnitBase w) => CRAFTS.Max(c => Rank(w, c));

        // bonuses (UnitModifyEvent, RealMarketEvent)
        public static int AdjAtk(WorldUnitBase w, int baseAtk) => Enabled ? (int)Math.Round(baseAtk * (double)StatRate(TotalEssenceSteps(w))) : 0;
        public static int AdjDef(WorldUnitBase w, int baseDef) => Enabled ? (int)Math.Round(baseDef * (double)StatRate(TotalEssenceSteps(w))) : 0;
        public static int AdjHp(WorldUnitBase w, int baseHp) => Enabled ? (int)Math.Min(int.MaxValue, Math.Round(baseHp * (double)StatRate(TotalEssenceSteps(w)))) : 0;
        public static float SellRate(WorldUnitBase w) => Enabled && w != null ? SellRateOf(BestRank(w)) : 0f;

        /// <summary>
        /// Return to simplicity of every craft ready (a Saint with RETURN_STEPS essence steps), each once: its destiny
        /// (ReturnDestiny: +50 to that craft, attack / defense / max HP +15 %, Luck +3); several crafts stack. Returns the count.
        /// </summary>
        public static int TryReturn(WorldUnitBase w)
        {
            if (w == null || !Enabled)
                return 0;
            var n = 0;
            foreach (var c in CRAFTS)
            {
                var d = Get(w, c);
                if (!CanReturn(d))
                    continue;
                d.Returned = true;
                w.AddLuck(ReturnDestiny(c));
                n++;
                if (w.IsPlayer())
                {
                    TaoistLogEvent.Log("log_artisan_return", CraftName(c));
                    DramaHelper.OpenDrama2(string.Format(GameTool.LS("artisan_return"), CraftName(c)), new List<string> { GameTool.LS("enc_leave") }, null, "xianrencijiu");
                }
                else TaoistLogEvent.LogIf(Known(w), "log_npc_artisan_return", w.GetName(), CraftName(c));
            }
            return n;
        }

        private static bool Known(WorldUnitBase w) =>
            FamilyEvent.IsFamilyOfPlayer(w) || Player.data.unitData.relationData.GetAllGoodRelationUnitID(false, false).ToArray().Contains(w.GetUnitId());

        /// <summary>Items crafted by a unit (count of one item id): craft exp, a masterpiece, an accident. Returns the craft exp.</summary>
        public static long Crafted(WorldUnitBase w, Craft c, int propsId, int count, float roll = -1f)
        {
            var conf = g.conf.itemProps.GetItem(propsId);
            if (w == null || conf == null || count <= 0 || !Enabled)
                return 0;
            if (roll < 0) roll = CommonTool.Random(0f, 100f);
            var d = Get(w, c, true);
            var lvl = Math.Max(1, conf.level);
            var grade = Math.Max(1, w.GetGradeLvl());
            var exp = CraftExp(lvl, count);
            if (!d.CraftedIds.Contains(propsId))
            {
                d.CraftedIds.Add(propsId);
                if (d.CraftedIds.Count > MAX_CRAFTED_IDS) d.CraftedIds.RemoveAt(0);
            }
            var steps0 = EssenceSteps(d.Essence);
            var up = AddExp(d, exp);
            if (EssenceSteps(d.Essence) != steps0) { Sync(w, c); TryReturn(w); }
            if (up > 0)
            {
                Sync(w, c);
                if (w.IsPlayer()) TaoistLogEvent.Log("log_artisan_rank", CraftName(c), RankName(d.Rank));
            }
            if (IsMasterpiece(lvl, grade))
            {
                d.Masterpieces++;
                var cul = MasterpieceExp(w.GetMaxExpCurrentPhase() - w.GetMinExpCurrentPhase(), lvl, grade, d.Rank);
                if (cul > 0) w.AddExp((int)Math.Min(int.MaxValue, cul));
                Instance.Masterpieces.Add(new Masterpiece { Maker = w.GetName(), Craft = (int)c, PropsId = propsId, Month = GameHelper.GetGameTotalMonth() });
                if (Instance.Masterpieces.Count > MAX_MASTERPIECES) Instance.Masterpieces.RemoveAt(0);
                if (w.IsPlayer()) TaoistLogEvent.Log("log_artisan_masterpiece", GameTool.LS(conf.name), cul);
                else TaoistLogEvent.LogIf(Known(w), "log_npc_artisan_masterpiece", w.GetName(), GameTool.LS(conf.name));
            }
            if (!IsGathering(c) && CanBlowUp(lvl, grade) && roll < ACCIDENT_CHANCE * DaoHeartEvent.RiskRate(w))
            {
                var hp = w.GetDynProperty(UnitDynPropertyEnum.Hp).value;
                var lost = Math.Max(0, Math.Min(hp - 1, (int)(hp * ACCIDENT_HP)));
                w.AddProperty<int>(UnitPropertyEnum.Hp, -lost);
                w.AddProperty<int>(UnitPropertyEnum.Life, -ACCIDENT_LIFE);
                LastAccident = lost;
                if (w.IsPlayer()) TaoistLogEvent.Log("log_artisan_accident", CraftName(c), lost, ACCIDENT_LIFE);
            }
            return exp;
        }
        /// <summary>HP lost by the last furnace accident (tests).</summary>
        public static int LastAccident { get; private set; }

        public enum ExamResult { NotReady, NotInCity, AlreadyThisYear, NoMoney, Passed, Failed }

        public static int ExamFee(int rank) => EncounterEvent.Cost(EXAM_FEE * Math.Max(1, rank));

        /// <summary>The yearly exam of the next rank (in a city, a fee). roll 0-100 against ExamChance.</summary>
        public static ExamResult Exam(WorldUnitBase w, Craft c, float roll)
        {
            var d = Get(w, c);
            if (!ExamReady(d)) return ExamResult.NotReady;
            if (w.IsPlayer())
            {
                var town = w.GetMapBuild<MapBuildTown>();
                if (town == null || !town.IsCity()) return ExamResult.NotInCity;
                if (d.LastExamYear == GameHelper.GetGameYear()) return ExamResult.AlreadyThisYear;
                var fee = ExamFee(d.Rank);
                if (w.GetUnitMoney() < fee) return ExamResult.NoMoney;
                w.AddUnitMoney(-fee);
                MapBuildPropertyEvent.AddBuildProperty(town, fee);
            }
            d.LastExamYear = GameHelper.GetGameYear();
            if (roll < ExamChance(CraftValue(w, c), d.Rank, d.Masterpieces > 0))
            {
                d.Rank++;
                d.Exp = 0;
                Sync(w, c);
                if (w.IsPlayer()) TaoistLogEvent.Log("log_artisan_rank", CraftName(c), RankName(d.Rank));
                else TaoistLogEvent.LogIf(Known(w), "log_npc_artisan_rank", w.GetName(), CraftName(c), RankName(d.Rank));
                return ExamResult.Passed;
            }
            return ExamResult.Failed;
        }

        // ------------------------------------------------------------------ the player's crafting (bag before / after a craft window)

        private static readonly Dictionary<string, Craft> CRAFT_UIS = new Dictionary<string, Craft>
        {
            { "RefineElixir", Craft.Alchemy }, { "TownFactoryRefineElixir", Craft.Alchemy },
            { "ArtifactShapeMade", Craft.Forge },
            { "RefineRuneFormula", Craft.Talisman }, { "RuneSelectDraw", Craft.Talisman },
            { "Geomancy", Craft.Geomancy }, { "GeomancyMap", Craft.Geomancy },
        };
        /// <summary>The bag (props id -> count) when the first craft window opened, and its craft.</summary>
        private static Dictionary<int, int> _bag;
        private static Craft _craft;
        private static readonly HashSet<string> _open = new HashSet<string>();
        /// <summary>The craft of the craft window open now (tests), null when none.</summary>
        public static Craft? OpenCraft => _open.Count > 0 ? _craft : (Craft?)null;

        /// <summary>Every window open now (the gathering on the map is counted only when none of BLOCKING is open).</summary>
        private static readonly HashSet<string> _uis = new HashSet<string>();
        /// <summary>Windows that bring herbs and ores by other ways (markets, shops, dramas, storage, the mod's windows). Not
        /// "GetReward": the game shows what was gathered in it.</summary>
        public static readonly string[] BLOCKING = { "Market", "Shop", "Drama", "Store", "Storage", "Auction", "Exchange", "Trade", "Mail", "Task",
            "TextInfo", "School", "Town", "Bag", "Prop", "Sell", "Buy", "Gift", "Battle", "Login", "Loading" };
        public static bool Blocking(IEnumerable<string> uis) => uis.Any(u => BLOCKING.Any(b => u.IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0));
        /// <summary>The bag seen last on the map (props id -> count), and the herbs / ores of the cells around then.</summary>
        private static Dictionary<int, int> _mapBag;
        private static int _cellHerbs, _cellOres;
        /// <summary>The mod gave items by code (gifts): the next bag is the new base, nothing is counted.</summary>
        public static void RefreshBag()
        {
            if (!Enabled || g.world?.playerUnit == null) return;
            _mapBag = Bag(Player);
            (_cellHerbs, _cellOres) = CellValues(Player.GetUnitPos());
        }

        /// <summary>Herbs (medicinal) and ores (mineral) of the cell and the cells next to it: the game's gathering lowers them.</summary>
        public static (int herbs, int ores) CellValues(Vector2Int p)
        {
            int h = 0, o = 0;
            for (var dx = -1; dx <= 1; dx++)
                for (var dy = -1; dy <= 1; dy++)
                {
                    DataGrid.GridData gd = null;
                    try { gd = g.data.grid.GetGridData(p + new Vector2Int(dx, dy)); } catch { }
                    if (gd == null) continue;
                    h += Math.Max(0, gd.medicinal);
                    o += Math.Max(0, gd.mineral);
                }
            return (h, o);
        }
        /// <summary>A gathered item counts only when the cells lost the matching resource (herbs for Herbs, ores for Mining).</summary>
        public static bool GatheredFromCell(Craft c, int herbsBefore, int herbsNow, int oresBefore, int oresNow) =>
            c == Craft.Herbal ? herbsNow < herbsBefore : c == Craft.Mining && oresNow < oresBefore;

        /// <summary>The craft of a gathered item: herbs (Herbal), ores (Mining), else none.</summary>
        public static Craft? GatherCraft(ConfItemPropsItem conf) =>
            conf == null ? (Craft?)null : conf.IsHerbItem() != null ? Craft.Herbal : (conf.IsMineItem()?.Length ?? 0) > 0 ? Craft.Mining : (Craft?)null;

        /// <summary>
        /// Herbs and ores that came into the bag on the map while the cells around lost herbs / ores (the game's gathering
        /// lowers them), with no window open that brings items another way: gathered.
        /// </summary>
        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnTimeUpdate1000ms()
        {
            base.OnTimeUpdate1000ms();
            if (!Enabled || Player == null)
                return;
            var now = Bag(Player);
            var cells = CellValues(Player.GetUnitPos());
            var counted = _mapBag != null && _open.Count == 0 && !Blocking(_uis) && g.ui.HasUI(UIType.MapMain)
                && !(TimeSkipEvent.Instance?.IsProcessing ?? false) && !SeclusionEvent.IsSecluded;
            if (counted)
                foreach (var x in Gained(_mapBag, now))
                {
                    var c = GatherCraft(g.conf.itemProps.GetItem(x.Key));
                    if (c != null && GatheredFromCell(c.Value, _cellHerbs, cells.herbs, _cellOres, cells.ores))
                    {
                        var exp = Crafted(Player, c.Value, x.Key, x.Value);
                        LastCrafted = $"{c} {x.Key} x{x.Value}: {exp}";
                    }
                }
            _mapBag = now;
            (_cellHerbs, _cellOres) = cells;
            // a mine vein / herb valley under the player's feet (ArtisanEvent.Spots.cs)
            CheckSpot();
        }

        public static Dictionary<int, int> Bag(WorldUnitBase w) =>
            w.GetUnitProps().GroupBy(x => x.propsID).ToDictionary(x => x.Key, x => x.Sum(y => y.propsCount));
        /// <summary>Items that came into the bag: id -> count.</summary>
        public static Dictionary<int, int> Gained(Dictionary<int, int> before, Dictionary<int, int> after) =>
            after.Where(x => x.Value > (before.TryGetValue(x.Key, out var b) ? b : 0))
                 .ToDictionary(x => x.Key, x => x.Value - (before.TryGetValue(x.Key, out var b) ? b : 0));

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            var name = e.uiType.uiName;
            _uis.Add(name);
            if (CRAFT_UIS.TryGetValue(name, out var craft))
            {
                if (_open.Count == 0)
                {
                    _bag = Bag(Player);
                    _craft = craft;
                }
                _open.Add(name);
                return;
            }
            if (name == UIType.FateFeature.uiName)
            {
                var ui = new UICover<UIFateFeature>(e.ui);
                ui.AddButton(ui.MidCol - 8, ui.MidRow - 7, OpenWindow, GameTool.LS("artisan_button")).Size(160f, 30f);
                ui.AddText(ui.MidCol - 8, ui.MidRow - 6, "{0}").Format(Color.black, 13).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { BestTitle(Player) }),
                });
                ui.IsAutoUpdate = true;
                ui.UpdateUI();
            }
            else if (name == UIType.NPCInfo.uiName)
            {
                var uiBase = g.ui.GetUI<UINPCInfo>(e.ui.uiType);
                var unit = uiBase?.unit;
                if (unit != null && !unit.IsPlayer() && CRAFTS.Any(x => Rank(unit, x) > 0) && !HirePeopleEvent.isShowHirePeopleUI)
                {
                    var ui = new UICover<UINPCInfo>(uiBase);
                    ui.AddText(ui.MidCol - 6, ui.MidRow - 7, string.Format(GameTool.LS("artisan_npc"), BestTitle(unit))).Align().Format(new Color(0.75f, 0.9f, 1f), 16)
                        .SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                    ui.UpdateUI();
                }
            }
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);
            _uis.Remove(e.uiType.uiName);
            if (Blocking(new[] { e.uiType.uiName })) RefreshBag();
            if (!_open.Remove(e.uiType.uiName) || _open.Count > 0 || _bag == null)
                return;
            var gained = Gained(_bag, Bag(Player));
            _bag = null;
            foreach (var x in gained)
            {
                var exp = Crafted(Player, _craft, x.Key, x.Value);
                if (exp > 0) LastCrafted = $"{_craft} {x.Key} x{x.Value}: {exp}";
            }
        }
        /// <summary>The last crafting counted (tests).</summary>
        public static string LastCrafted { get; private set; }

        /// <summary>Title of the best craft, e.g. "Alchemy Master" (none: "-").</summary>
        public static string BestTitle(WorldUnitBase w)
        {
            var best = CRAFTS.Select(c => (c, r: Rank(w, c))).OrderByDescending(x => x.r).First();
            return best.r <= 0 ? GameTool.LS("artisan_none") : string.Format(GameTool.LS("artisan_title"), CraftName(best.c), RankName(best.r));
        }

        // ------------------------------------------------------------------ NPCs

        /// <summary>NPCs with a craft value raise their best craft every month and take its exam once a year.</summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (wunit == null || wunit.isDie)
                return;
            var months = Math.Max(1, ModMaster.MonthsPassed);
            if (wunit.IsPlayer())
            {
                PlayerMonthly(wunit, months);
                return;
            }
            var craft = CRAFTS.OrderByDescending(x => CraftValue(wunit, x)).First();
            var value = CraftValue(wunit, craft);
            if (value < NPC_MIN_VALUE)
                return;
            var d = Get(wunit, craft, true);
            var steps0 = EssenceSteps(d.Essence);
            if (AddExp(d, (long)(value * NPC_EXP_SHARE * months)) > 0 || EssenceSteps(d.Essence) != steps0)
            {
                Sync(wunit, craft);
                TryReturn(wunit);
            }
            if (ExamReady(d) && d.LastExamYear != GameHelper.GetGameYear() && GameHelper.GetGameMonth() == 1)
                Exam(wunit, craft, CommonTool.Random(0f, 100f));
        }

        /// <summary>The mine veins and herb valleys of the save are made at load (once), and their cells made rich again every year.</summary>
        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            MakeSpots();
            RefreshBag();
        }

        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnYearly()
        {
            base.OnYearly();
            Enrich();
        }

        /// <summary>The spots refilled every month (the game regrows its cells slowly; a spot stays full).</summary>
        public override void OnMonthly()
        {
            base.OnMonthly();
            if (Enabled) Enrich();
        }

        // ------------------------------------------------------------------ commissions and apprentices (player)

        public static WorldUnitBase Unit(string id) => string.IsNullOrEmpty(id) ? null : g.world.unit.GetUnit(id);

        private static void PlayerMonthly(WorldUnitBase p, int months)
        {
            // commissions: the old ones expire, a new one now and then
            foreach (var cm in Instance.Commissions.Where(x => x.Deadline < GameHelper.GetGameTotalMonth()).ToList())
            {
                Instance.Commissions.Remove(cm);
                Unit(cm.ClientId)?.data.unitData.relationData.AddHate(p.GetUnitId(), COMMISSION_INTIM / 2);
                TaoistLogEvent.Log("log_artisan_commission_late", Unit(cm.ClientId)?.GetName() ?? "?");
                DaoHeartEvent.Add(p, DaoHeartEvent.PROMISE_BROKEN, "heart_src_promise_broken");
            }
            var best = BestRank(p);
            if (best > 0 && Instance.Commissions.Count < MAX_COMMISSIONS && CommonTool.Random(0f, 100f) < COMMISSION_RATE * best * months)
                NewCommission(p);
            // apprentices: craft exp, now and then an item
            foreach (var a in Instance.Apprentices.ToList())
            {
                var npc = Unit(a.UnitId);
                if (npc == null || npc.isDie) { Instance.Apprentices.Remove(a); continue; }
                var c = (Craft)a.Craft;
                var nd = Get(npc, c, true);
                if (AddExp(nd, APPRENTICE_EXP * months) > 0) Sync(npc, c);
                if (CommonTool.Random(0f, 100f) < APPRENTICE_GIFT * months)
                    ApprenticeGift(p, npc, c);
            }
        }

        /// <summary>A new commission of an NPC: an item of a craft the player knows, of the usual level or one above.</summary>
        public static Commission NewCommission(WorldUnitBase p)
        {
            var crafts = CRAFTS.Where(k => Rank(p, k) > 0 && (Get(p, k)?.CraftedIds.Count ?? 0) > 0).ToList();
            var client = p.GetUnitsAround(12, false, false).ToArray().Where(x => x != null && !x.isDie && !x.IsPlayer()).OrderBy(x => CommonTool.Random(0, 1000)).FirstOrDefault()
                ?? (ModMain.Store?.WUnits ?? new WorldUnitBase[0]).FirstOrDefault(x => x != null && !x.isDie && !x.IsPlayer());
            if (crafts.Count == 0 || client == null)
                return null;
            var c = crafts[CommonTool.Random(0, crafts.Count)];
            var level = Math.Min(6, UsualLevel(p.GetGradeLvl()) + CommonTool.Random(0, 2));
            var cm = new Commission
            {
                ClientId = client.GetUnitId(), Craft = (int)c, Level = level,
                Reward = CommissionReward(level, EncounterEvent.Cost()),
                Deadline = GameHelper.GetGameTotalMonth() + COMMISSION_MONTHS,
            };
            Instance.Commissions.Add(cm);
            TaoistLogEvent.Log("log_artisan_commission", client.GetName(), CraftName(c), level, cm.Reward);
            return cm;
        }

        /// <summary>An item of the bag that fits a commission: made in its craft (by the player), of its level or above.</summary>
        public static DataProps.PropsData CommissionItem(WorldUnitBase p, Commission cm)
        {
            var ids = Get(p, (Craft)cm.Craft)?.CraftedIds;
            if (ids == null) return null;
            return p.GetUnitProps().Where(x => ids.Contains(x.propsID) && (g.conf.itemProps.GetItem(x.propsID)?.level ?? 0) >= cm.Level)
                .OrderBy(x => g.conf.itemProps.GetItem(x.propsID).level).FirstOrDefault();
        }

        /// <summary>Delivers a commission: the item goes to the client, the reward and the craft exp to the player.</summary>
        public static bool Deliver(WorldUnitBase p, Commission cm)
        {
            var item = CommissionItem(p, cm);
            if (item == null)
                return false;
            p.RemoveUnitProp(item.soleID, 1);
            p.AddUnitMoney(cm.Reward);
            var client = Unit(cm.ClientId);
            client?.data.unitData.relationData.AddIntim(p.GetUnitId(), COMMISSION_INTIM);
            AddExp(Get(p, (Craft)cm.Craft, true), CraftExp(cm.Level, 1) * 2);
            Sync(p, (Craft)cm.Craft);
            Instance.Commissions.Remove(cm);
            TaoistLogEvent.Log("log_artisan_delivered", client?.GetName() ?? "?", cm.Reward);
            DaoHeartEvent.Add(p, DaoHeartEvent.PROMISE_KEPT, "heart_src_promise_kept");
            return true;
        }

        /// <summary>The NPC who would be the player's apprentice: the friendliest NPC around, not hired, not already an apprentice.</summary>
        public static WorldUnitBase ApprenticeCandidate(WorldUnitBase p) =>
            p.GetUnitsAround(12, false, false).ToArray()
                .Where(x => x != null && !x.isDie && !x.IsPlayer() && !HirePeopleEvent.IsHired(x) && !Instance.Apprentices.Any(a => a.UnitId == x.GetUnitId()))
                .OrderByDescending(x => x.data.unitData.relationData.GetIntim(p)).FirstOrDefault();

        public static bool CanTakeApprentice(WorldUnitBase p, Craft c) =>
            Instance.Apprentices.Count(a => a.Craft == (int)c) < MaxApprentices(Rank(p, c));

        public static Apprentice TakeApprentice(WorldUnitBase p, Craft c, WorldUnitBase npc)
        {
            if (npc == null || !CanTakeApprentice(p, c))
                return null;
            var a = new Apprentice { UnitId = npc.GetUnitId(), Craft = (int)c };
            Instance.Apprentices.Add(a);
            npc.data.unitData.relationData.AddIntim(p.GetUnitId(), COMMISSION_INTIM);
            TaoistLogEvent.Log("log_artisan_apprentice", npc.GetName(), CraftName(c));
            return a;
        }

        /// <summary>An apprentice brings an item the player has made in the craft, of the apprentice's usual level at most.</summary>
        public static string ApprenticeGift(WorldUnitBase p, WorldUnitBase npc, Craft c)
        {
            var max = UsualLevel(npc.GetGradeLvl()) + Rank(npc, c) / 2;
            var ids = (Get(p, c)?.CraftedIds ?? new List<int>()).Where(x => (g.conf.itemProps.GetItem(x)?.level ?? 99) <= max).ToList();
            if (ids.Count == 0)
                return null;
            var id = ids[CommonTool.Random(0, ids.Count)];
            p.AddUnitProp(id, 1);
            RefreshBag();
            var name = GameTool.LS(g.conf.itemProps.GetItem(id).name);
            TaoistLogEvent.Log("log_artisan_apprentice_gift", npc.GetName(), name);
            return name;
        }

        // the window: ArtisanEvent.Window.cs
    }
}
