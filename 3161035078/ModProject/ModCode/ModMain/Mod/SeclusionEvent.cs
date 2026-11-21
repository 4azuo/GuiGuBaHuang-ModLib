using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Seclusion / enlightenment: years pass in a few minutes (the time skip of TimeSkipEvent, the world keeps running
    /// month by month: NPCs grow, marry, have children, die; inflation; town wars...).
    /// - Offered (the player confirms, sees the length and the lifespan left): Spirit vein (3-8 years, exp),
    ///   Ancient inheritance (10-30 years, rare: a lot of exp, Talent, an item of the next grade),
    ///   Old tree (5 years: + a random basis).
    /// - Forced (a drama tells what happens, then it runs): Heart demon (2-5 years, grade 3+: 50 % Talent and Luck,
    ///   50 % lose a part of the exp of the phase), Time realm (10-20 years, rare: items of the next grade, exp).
    /// A forced seclusion never ends the player's life: it is shortened to keep at least FORCED_KEEP_LIFE months.
    /// While secluded: no encounters, no loan penalty, no bounty hunters after the player.
    /// At the end a drama sums up: years, exp, prices (inflation), friends who died, births in the family.
    /// Off with S&M "No seclusion" / "No forced seclusion".
    /// </summary>
    [Cache(ModConst.SECLUSION_EVENT)]
    public class SeclusionEvent : ModEvent
    {
        public static SeclusionEvent Instance { get; set; }

        public enum Kind { SpiritVein, Inheritance, OldTree, HeartDemon, TimeRealm, Garden, Waterfall, Lava, Thunder, MineVein, HerbValley }

        /// <summary>The seclusion of a tempering place of the body (BodyCulEvent), chosen in the body refining window.</summary>
        public static Kind KindOf(BodyCulEvent.Place p) => p == BodyCulEvent.Place.Waterfall ? Kind.Waterfall : p == BodyCulEvent.Place.Lava ? Kind.Lava : Kind.Thunder;
        public static BodyCulEvent.Place? PlaceOf(Kind k) => k == Kind.Waterfall ? BodyCulEvent.Place.Waterfall : k == Kind.Lava ? BodyCulEvent.Place.Lava : k == Kind.Thunder ? BodyCulEvent.Place.Thunder : (BodyCulEvent.Place?)null;

        public static float OfferRate => SMLocalConfigsEvent.Instance?.Configs?.SeclusionRate ?? 0.5f; // % a month (S&M)
        public const float INHERITANCE_SHARE = 15f;  // % of the offers
        public static float HeartDemonRate => OfferRate * 0.3f * SpiritSenseEvent.HeartDemonRate(g.world.playerUnit);  // % a month (grade 3+), less with the spirit sense
        public static float TimeRealmRate => OfferRate * 0.1f;   // % a month
        public const int OFFER_COOLDOWN = 120;       // months, per kind
        public const int FORCED_COOLDOWN = 180;      // months, any forced one
        public const int FORCED_KEEP_LIFE = 12;      // months of lifespan kept by a forced seclusion

        /// <summary>Kind -> last month (total months) it happened.</summary>
        public Dictionary<string, int> LastMonth { get; set; } = new Dictionary<string, int>();
        public int LastForcedMonth { get; set; } = -9999;
        public int Count { get; set; }

        /// <summary>Running seclusion (saved: a save made during the skip ends it at load).</summary>
        public Running Current { get; set; }

        public class Running
        {
            public Kind Kind { get; set; }
            public int Months { get; set; }
            public int StartMonth { get; set; }
            public long StartExp { get; set; }
            public int StartLogIndex { get; set; }
            public List<string> Friends { get; set; } = new List<string>();
            public int StartPrice { get; set; }
            public int Roll { get; set; }
            /// <summary>Area of a mine vein / herb valley (ArtisanEvent spots): the grade of the items.</summary>
            public int Area { get; set; }
        }

        private static WorldUnitBase Player => g.world.playerUnit;
        public static bool Disabled => SMLocalConfigsEvent.Instance?.Configs?.NoSeclusion ?? false;
        public static bool NoForced => Disabled || (SMLocalConfigsEvent.Instance?.Configs?.NoForcedSeclusion ?? false);
        /// <summary>The player is in seclusion (time is passing).</summary>
        public static bool IsSecluded => Instance?.Current != null;

        /// <summary>Months of lifespan left.</summary>
        public static int LifeLeft() => Player.GetProperty<int>(UnitPropertyEnum.Life) - Player.GetProperty<int>(UnitPropertyEnum.Age);

        /// <summary>Exp of one year of seclusion: a tenth of the current phase (min 50); the game is played for a long time.</summary>
        public static int YearExp() => Math.Max(50, (Player.GetMaxExpCurrentPhase() - Player.GetMinExpCurrentPhase()) / 10);

        /// <summary>Length range (years) of a kind.</summary>
        public static Tuple<int, int> Years(Kind k)
        {
            switch (k)
            {
                case Kind.SpiritVein: return Tuple.Create(3, 8);
                case Kind.Inheritance: return Tuple.Create(10, 30);
                case Kind.OldTree: return Tuple.Create(5, 5);
                case Kind.HeartDemon: return Tuple.Create(2, 5);
                case Kind.Garden: return Tuple.Create(1, 20);
                case Kind.Waterfall:
                case Kind.Lava:
                case Kind.Thunder: return Tuple.Create(1, 5);
                case Kind.MineVein:
                case Kind.HerbValley: return Tuple.Create(1, 10);
                default: return Tuple.Create(10, 20);
            }
        }

        public static bool IsForced(Kind k) => k == Kind.HeartDemon || k == Kind.TimeRealm;

        /// <summary>Months of a forced seclusion, shortened to keep FORCED_KEEP_LIFE months of lifespan (0: cannot happen).</summary>
        public static int ForcedMonths(int years, int lifeLeft)
        {
            var m = Math.Min(years * 12, lifeLeft - FORCED_KEEP_LIFE);
            return m < 12 ? 0 : m;
        }

        private static string Background(Kind k)
        {
            switch (k)
            {
                case Kind.SpiritVein: return "dixiamushi";
                case Kind.Inheritance: return "dixiamushi";
                case Kind.OldTree: return "xingxingqiyu";
                case Kind.HeartDemon: return "hundengshangren";
                case Kind.Garden: return "xingxingqiyu";
                case Kind.Waterfall:
                case Kind.Lava:
                case Kind.Thunder: return BodyCulEvent.PlaceBackground(PlaceOf(k).Value);
                case Kind.MineVein: return "dixiamushi";
                case Kind.HerbValley: return "xingxingqiyu";
                default: return "xianrencijiu";
            }
        }

        // ------------------------------------------------------------------ trigger

        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnMonthly()
        {
            base.OnMonthly();
            if (Disabled || Current != null || (TimeSkipEvent.Instance?.IsProcessing ?? false))
                return;
            var cur = GameHelper.GetGameTotalMonth();
            // forced
            if (!NoForced && cur - LastForcedMonth >= FORCED_COOLDOWN)
            {
                if (Player.GetGradeLvl() >= 3 && CommonTool.Random(0f, 100f).IsBetween(0f, HeartDemonRate)) { Forced(Kind.HeartDemon); return; }
                if (CommonTool.Random(0f, 100f).IsBetween(0f, TimeRealmRate)) { Forced(Kind.TimeRealm); return; }
            }
            // offered
            if (!CommonTool.Random(0f, 100f).IsBetween(0f, OfferRate))
                return;
            var kind = CommonTool.Random(0f, 100f).IsBetween(0f, INHERITANCE_SHARE) ? Kind.Inheritance : (CommonTool.Random(0, 2) == 0 ? Kind.SpiritVein : Kind.OldTree);
            if (LastMonth.TryGetValue(kind.ToString(), out var last) && cur - last < OFFER_COOLDOWN)
                return;
            Offer(kind);
        }

        /// <summary>An offered seclusion: length, lifespan left (red warning when longer), accept / decline.</summary>
        public void Offer(Kind k, int years = -1)
        {
            var range = Years(k);
            years = years > 0 ? years : CommonTool.Random(range.Item1, range.Item2 + 1);
            LastMonth[k.ToString()] = GameHelper.GetGameTotalMonth();
            var life = LifeLeft() / 12;
            var warn = years * 12 >= LifeLeft() ? "\n" + string.Format(GameTool.LS("secl_warn_life"), life) : string.Empty;
            var text = string.Format(GameTool.LS($"secl_{k}"), years, life) + warn;
            DramaHelper.OpenDrama2(text, new List<string> { string.Format(GameTool.LS("secl_o1"), years), GameTool.LS("secl_o2") },
                x => { if (x.id % 10 == 0) Start(k, years * 12); else TaoistLogEvent.Log("log_secl_declined", GameTool.LS($"secl_name_{k}")); },
                Background(k));
        }

        /// <summary>A forced seclusion: a drama, then it starts (nothing when the lifespan is too short).</summary>
        public void Forced(Kind k, int years = -1)
        {
            var range = Years(k);
            years = years > 0 ? years : CommonTool.Random(range.Item1, range.Item2 + 1);
            var months = ForcedMonths(years, LifeLeft());
            if (months <= 0)
                return;
            LastForcedMonth = GameHelper.GetGameTotalMonth();
            DramaHelper.OpenDrama2(string.Format(GameTool.LS($"secl_{k}"), (months + 11) / 12), new List<string> { GameTool.LS("secl_forced_o1") },
                x => Start(k, months), Background(k));
        }

        // ------------------------------------------------------------------ run

        /// <summary>Starts the time skip of a seclusion.</summary>
        public void Start(Kind k, int months, int area)
        {
            Start(k, months);
            if (Current != null) Current.Area = area;
        }

        public void Start(Kind k, int months)
        {
            Count++;
            Current = new Running
            {
                Kind = k,
                Months = months,
                StartMonth = GameHelper.GetGameTotalMonth(),
                StartExp = Player.GetExp(),
                StartLogIndex = TaoistLogEvent.Instance?.Entries.Count ?? 0,
                Friends = Player.data.unitData.relationData.GetAllGoodRelationUnitID(false, false).ToArray().ToList(),
                StartPrice = InflationaryEvent.CalculateInflationary(10000),
                Roll = CommonTool.Random(0, 100),
            };
            TaoistLogEvent.Log("log_secl_start", GameTool.LS($"secl_name_{k}"), (months + 11) / 12);
            DebugHelper.WriteLine($"SeclusionEvent: {k} {months} months");
            TimeSkipEvent.Instance.SkipTime(months, End);
        }

        /// <summary>Rewards of a seclusion (applied at the end). Returns the result text.</summary>
        public static string Reward(Running r)
        {
            var years = Math.Max(1, r.Months / 12);
            switch (r.Kind)
            {
                case Kind.SpiritVein:
                    Player.AddExp(YearExp() * r.Months / 12);
                    return GameTool.LS("secl_res_SpiritVein");
                case Kind.Garden: // manual seclusion in Granny Shi's garden (story line E): paid, a bit less than a spirit vein
                    Player.AddExp(YearExp() * r.Months * 3 / 48);
                    return GameTool.LS("secl_res_Garden");
                case Kind.Waterfall: // tempering places of the body (BodyCulEvent): body exp, no cultivation exp
                case Kind.Lava:
                case Kind.Thunder:
                    var lvl0 = BodyCulEvent.GetLevel(Player);
                    BodyCulEvent.Temper(Player, PlaceOf(r.Kind).Value, r.Months);
                    return string.Format(GameTool.LS($"secl_res_{r.Kind}"), BodyCulEvent.RealmName(BodyCulEvent.GetLevel(Player)), BodyCulEvent.GetLevel(Player) - lvl0);
                case Kind.MineVein: // a mine vein / herb valley (ArtisanEvent spots): ores / herbs of the area, Mining / Herbs exp
                    return ArtisanEvent.SpotReward(ArtisanEvent.SpotKind.Mine, r.Area, r.Months);
                case Kind.HerbValley:
                    return ArtisanEvent.SpotReward(ArtisanEvent.SpotKind.Herb, r.Area, r.Months);
                case Kind.Inheritance:
                    Player.AddExp(YearExp() * r.Months * 3 / 24);
                    Player.AddProperty<int>(UnitPropertyEnum.Talent, 2 + years / 10);
                    return string.Format(GameTool.LS("secl_res_Inheritance"), EncounterEvent.GiveItem(x => x.IsPill() != null || x.IsHerbItem() != null, Player.GetGradeLvl() + 1));
                case Kind.OldTree:
                    var basis = new[] { UnitPropertyEnum.BasisBlade, UnitPropertyEnum.BasisSpear, UnitPropertyEnum.BasisSword, UnitPropertyEnum.BasisFist, UnitPropertyEnum.BasisPalm, UnitPropertyEnum.BasisFinger,
                        UnitPropertyEnum.BasisFire, UnitPropertyEnum.BasisFroze, UnitPropertyEnum.BasisThunder, UnitPropertyEnum.BasisWind, UnitPropertyEnum.BasisEarth, UnitPropertyEnum.BasisWood };
                    var bi = r.Roll % basis.Length;
                    var b = basis[bi];
                    var add = 2 + r.Roll % 3;
                    Player.AddProperty<int>(b, add);
                    return string.Format(GameTool.LS("secl_res_OldTree"), add, GameTool.LS($"secl_basis_{bi}"));
                case Kind.HeartDemon:
                    // the Dao heart decides (50 % + 8 a level); the spirit sense and the Dao heart grow from it
                    if (r.Roll < DaoHeartEvent.HeartDemonWin(Player))
                    {
                        DaoHeartEvent.Add(Player, DaoHeartEvent.HEART_DEMON_WIN, "heart_src_demon_win");
                        SpiritSenseEvent.AddExp(Player, SpiritSenseEvent.HEART_DEMON_WIN);
                        Player.AddProperty<int>(UnitPropertyEnum.Talent, 3);
                        Player.AddProperty<int>(UnitPropertyEnum.Luck, 1);
                        return GameTool.LS("secl_res_HeartDemon_win");
                    }
                    DaoHeartEvent.Add(Player, DaoHeartEvent.HEART_DEMON_LOSE, "heart_src_demon_lose");
                    var lost = Math.Max(0, Player.GetExp() - Player.GetMinExpCurrentPhase()) / 5;
                    Player.AddExp(-lost);
                    return string.Format(GameTool.LS("secl_res_HeartDemon_lose"), lost);
                default:
                    Player.AddExp(YearExp() * r.Months / 12);
                    var items = new List<string> { EncounterEvent.GiveItem(x => x.IsPill() != null || x.IsHerbItem() != null || x.IsMineItem() != null, Player.GetGradeLvl() + 1) };
                    return string.Format(GameTool.LS("secl_res_TimeRealm"), string.Join(", ", items.Where(x => !string.IsNullOrEmpty(x))));
            }
        }

        /// <summary>End of the skip: rewards and the summary drama.</summary>
        public void End()
        {
            var r = Current;
            if (r == null)
                return;
            Current = null;
            // interrupted (an enemy found the player...): the rewards of the months actually passed
            var passed = Math.Max(0, GameHelper.GetGameTotalMonth() - r.StartMonth);
            var interrupted = passed < r.Months;
            r.Months = Math.Min(r.Months, passed);
            var result = (interrupted ? string.Format(GameTool.LS("secl_interrupted"), passed) + "\n\n" : string.Empty) + Reward(r);
            // a seclusion steadies the heart and widens the spirit sense
            DaoHeartEvent.Add(Player, DaoHeartEvent.SECLUSION_YEAR * Math.Max(1, r.Months / 12), "heart_src_seclusion");
            SpiritSenseEvent.AddExp(Player, SpiritSenseEvent.SECLUSION_MONTH * r.Months);
            var years = (GameHelper.GetGameTotalMonth() - r.StartMonth + 11) / 12;
            var exp = Player.GetExp() - r.StartExp;
            var prices = (InflationaryEvent.CalculateInflationary(10000) - r.StartPrice) * 100 / Math.Max(1, r.StartPrice);
            var died = r.Friends.Select(id => g.world.unit.GetUnit(id)).Where(x => x == null || x.isDie).Count();
            var births = (TaoistLogEvent.Instance?.Entries.Skip(r.StartLogIndex) ?? Enumerable.Empty<TaoistLogEvent.Entry>()).Count(x => x.Key == "log_family_birth");
            TaoistLogEvent.Log("log_secl_end", GameTool.LS($"secl_name_{r.Kind}"), years, exp);
            var text = result + "\n\n" + string.Format(GameTool.LS("secl_summary"), years, exp, prices, died, births, Player.GetProperty<int>(UnitPropertyEnum.Age) / 12, LifeLeft() / 12);
            DramaHelper.OpenDrama2(text, new List<string> { GameTool.LS("secl_end_o1") }, null, "huapaonanziganxie");
        }

        /// <summary>A save loaded during a seclusion: the skip stopped there, the rewards are given.</summary>
        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            if (Current != null && !(TimeSkipEvent.Instance?.IsProcessing ?? false))
                ModDelayEvent.Instance.DelayEvent(this, End, 3000);
        }
    }
}
