using MOD_nE7UL2.Helper;
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
    /// Body refining (thể tu): a second path of cultivation beside the Qi.
    /// - 5 realms (Copper Skin, Iron Bone, Jade Marrow, Golden Body, Undying Body) of 3 layers each (levels 1-15).
    /// - Exp: damage taken in battles (by share of the max HP), training every month, a medicinal bath (natural treasures
    ///   of the bag, 天材地宝 item class 508, once a month).
    /// - Inside a realm the layers go up by themselves; the last layer of a realm is a bottleneck: the body breaks into the
    ///   next realm with a chance (button), only from the cultivation grade the realm needs; a failure halves the exp and
    ///   injures the body (bonuses halved, no training) for some months.
    /// - Bonuses (UnitModifyEvent): max HP, defense, fist / palm / finger basis; the Qi of a body refiner grows slower.
    /// - NPCs take up the path now and then and refine their body every month.
    /// Window: button "Body refining" of the cultivation screen; NPC info shows the realm of NPC body refiners.
    /// </summary>
    [Cache(ModConst.BODY_CUL_EVENT)]
    public class BodyCulEvent : ModEvent
    {
        public static BodyCulEvent Instance { get; set; }

        public const int LAYERS = 3;
        public const int REALMS = 5;
        public const int MAX_LEVEL = LAYERS * REALMS;
        /// <summary>Cultivation grade needed to enter each realm (index = realm - 1).</summary>
        public static readonly int[] REALM_GRADE = { 1, 3, 5, 7, 9 };

        public const int NEED_EXP_RATE = 200;          // exp of a level = 200 x level²
        public const float HIT_EXP = 100f;             // exp for a whole max HP of damage taken
        public const int TRAIN_EXP_PLAYER = 10;        // a month
        public const int TRAIN_EXP_NPC = 30;           // a month (NPCs do not fight the battles of the player)
        public const int BATH_EXP_SHARE = 5;           // a bath = 1/5 of the level
        /// <summary>Natural treasures (天材地宝, item class 508) of a medicinal bath, by realm: count and lowest item level.</summary>
        public const int TREASURE_CLASS = 508;
        public static readonly int[] BATH_COUNT = { 2, 4, 2, 4, 2 };
        public static readonly int[] BATH_ITEM_LEVEL = { 4, 4, 5, 5, 6 };
        /// <summary>Herbs (药材, item class 503, any grade) of a bath by realm, and of a breakthrough into realm 2-5.</summary>
        public const int HERB_CLASS = 503;
        public static readonly int[] BATH_HERBS = { 5, 10, 20, 30, 50 };
        /// <summary>A breakthrough of the player into realm 2-5 (index 0-3): natural treasures (count, lowest item level) and herbs.</summary>
        public static readonly int[] BREAK_TREASURES = { 6, 10, 16, 24 };
        public static readonly int[] BREAK_ITEM_LEVEL = { 4, 5, 5, 6 };
        public static readonly int[] BREAK_HERBS = { 25, 50, 100, 200 };
        public const float HP_PER_LEVEL = 0.04f;       // of the base max HP
        public const float DEF_PER_LEVEL = 0.03f;      // of the base defense
        public const int BASIS_PER_LEVEL = 4;          // fist, palm, finger
        /// <summary>Qi gain by realm (index = realm, 0 = not a body refiner).</summary>
        public static readonly float[] QI_RATES = { 1f, 0.90f, 0.85f, 0.80f, 0.75f, 0.70f };
        public const float EXP_RATE_PER_REALM = 0.10f;    // cultivation exp needed: x(1 + 0.1 x realm)
        public const float BREAK_HP_PER_REALM = 0.05f;    // share of the base max HP gained by a breakthrough of the cultivation
        public const float BREAK_DEF_PER_REALM = 0.03f;   // ... defense
        public const float BREAK_ATK_PER_REALM = 0.01f;   // ... attack
        public const int INJURY_MONTHS = 3;
        public const float NPC_START_RATE = 0.5f;      // % a month that an NPC takes up the path

        public class BodyData
        {
            public int Level { get; set; }
            public long Exp { get; set; }
            public int InjuryMonths { get; set; }
            /// <summary>NPCs: the tempering place they stay at (Place, 0 = none) and the months left there.</summary>
            public int PlaceKind { get; set; }
            public int PlaceMonths { get; set; }
            public int LastBathMonth { get; set; } = -1;

            // cultivation tracking (exp absorbed by the body, bonuses of the breakthroughs)
            public int TrackPhase { get; set; }
            public int TrackExp { get; set; }
            public int TrackHp { get; set; }
            public int TrackDef { get; set; }
            public int TrackAtk { get; set; }
            /// <summary>Permanent bonuses won at the breakthroughs of the cultivation.</summary>
            public int BonusHp { get; set; }
            public int BonusDef { get; set; }
            public int BonusAtk { get; set; }
            /// <summary>Cultivation exp absorbed by the body (shown in the window).</summary>
            public long Absorbed { get; set; }
            /// <summary>Body essence: the exp gained at the top level (Undying Body - Late, full), stats without a level.</summary>
            public long Essence { get; set; }
            /// <summary>Tempering of the strong breakthroughs: the share of a realm's bonuses they added (x2 +0.2, x3 +0.5).</summary>
            public double Temper { get; set; }
        }

        public Dictionary<string, BodyData> Bodies { get; set; } = new Dictionary<string, BodyData>();

        /// <summary>Body tribulation of the player under way (the trial battle): 0 = none, else the level it breaks from.</summary>
        public int TribulationLevel { get; set; }

        /// <summary>Off with the S&amp;M switch "No body cultivation".</summary>
        public override bool OnCacheHandler()
        {
            return !(SMLocalConfigsEvent.Instance?.Configs?.NoBodyCultivation ?? false);
        }

        /// <summary>False when the S&amp;M switch "No body cultivation" is on (Instance is set even then, see EventHelper.IsLoaded).</summary>
        public static bool Enabled => Instance != null && !(SMLocalConfigsEvent.Instance?.Configs?.NoBodyCultivation ?? false);

        // ------------------------------------------------------------------ sects that teach body refining

        /// <summary>
        /// The sects that teach body refining, chosen by their style every year (RefreshBodySects): the third of the
        /// sects whose members fight the most with bare hands (fist + palm + finger in their basis).
        /// </summary>
        public List<string> BodySects { get; set; } = new List<string>();
        public const int SECT_SHARE = 3;
        /// <summary>A sect needs this many members to have a style.</summary>
        public const int SECT_MIN_MEMBERS = 3;
        /// <summary>Members of a body refining sect: they take up the path x4 more often, train x2.</summary>
        public const float SECT_START_RATE = 4f;
        public const float SECT_TRAIN_RATE = 2f;

        // properties, not static fields: the game's types must not load with the class (unit tests run without the game)
        public static UnitPropertyEnum[] HAND_BASIS => new[] { UnitPropertyEnum.BasisFist, UnitPropertyEnum.BasisPalm, UnitPropertyEnum.BasisFinger };
        public static UnitPropertyEnum[] ALL_BASIS => new[] { UnitPropertyEnum.BasisBlade, UnitPropertyEnum.BasisSpear, UnitPropertyEnum.BasisSword, UnitPropertyEnum.BasisFist, UnitPropertyEnum.BasisPalm, UnitPropertyEnum.BasisFinger,
            UnitPropertyEnum.BasisFire, UnitPropertyEnum.BasisFroze, UnitPropertyEnum.BasisThunder, UnitPropertyEnum.BasisWind, UnitPropertyEnum.BasisEarth, UnitPropertyEnum.BasisWood };

        /// <summary>Share (0-1) of fist + palm + finger in the whole basis of a unit.</summary>
        public static double HandShare(long hands, long all) => all <= 0 ? 0 : Math.Max(0, Math.Min(1, hands / (double)all));
        public static double HandShare(WorldUnitBase wunit) =>
            HandShare(HAND_BASIS.Sum(x => (long)wunit.GetProperty<int>(x)), ALL_BASIS.Sum(x => (long)wunit.GetProperty<int>(x)));

        /// <summary>
        /// The body refining sects from (sect, hand share) of their members: sects of SECT_MIN_MEMBERS or more, ranked by
        /// the average hand share, the first third (at least one).
        /// </summary>
        public static List<string> PickBodySects(IEnumerable<KeyValuePair<string, double>> members)
        {
            var sects = members.Where(x => !string.IsNullOrEmpty(x.Key)).GroupBy(x => x.Key)
                .Where(x => x.Count() >= SECT_MIN_MEMBERS)
                .Select(x => new { Id = x.Key, Share = x.Average(y => y.Value) })
                .OrderByDescending(x => x.Share).ThenBy(x => x.Id, StringComparer.Ordinal).ToList();
            var n = sects.Count == 0 ? 0 : Math.Max(1, (sects.Count + SECT_SHARE - 1) / SECT_SHARE);
            return sects.Take(n).Select(x => x.Id).ToList();
        }

        /// <summary>Chooses the body refining sects again from the members alive (yearly, and at load when none).</summary>
        public static void RefreshBodySects()
        {
            var units = ModMain.Store?.WUnits;
            if (units == null || !Enabled)
                return;
            Instance.BodySects = PickBodySects(units.Where(x => x != null && !x.isDie)
                .Select(x => new KeyValuePair<string, double>(x.data?.school?.schoolNameID, HandShare(x))));
        }

        public static bool IsBodySect(string schoolNameID) => !string.IsNullOrEmpty(schoolNameID) && Enabled && Instance.BodySects.Contains(schoolNameID);
        public static bool InBodySect(WorldUnitBase wunit) => IsBodySect(wunit?.data?.school?.schoolNameID);
        /// <summary>Monthly training exp: the base of the unit x2 in a body refining sect.</summary>
        public static int TrainExp(bool player, bool inSect, int months) => (int)Math.Round((player ? TRAIN_EXP_PLAYER : TRAIN_EXP_NPC) * (inSect ? SECT_TRAIN_RATE : 1f) * Math.Max(1, months));
        /// <summary>Chance (%) a month that an NPC takes up the path.</summary>
        public static float StartRate(bool inSect) => NPC_START_RATE * (inSect ? SECT_START_RATE : 1f);

        // ------------------------------------------------------------------ tempering places (seclusions of the body)

        public enum Place { Waterfall = 1, Lava = 2, Thunder = 3 }
        public static readonly Place[] PLACES = { Place.Waterfall, Place.Lava, Place.Thunder };
        /// <summary>Lowest body realm of a place (index = place - 1).</summary>
        public static readonly int[] PLACE_REALM = { 1, 2, 4 };
        /// <summary>A month at a place gives the exp of the current level / this (a bath gives / 5, with materials).</summary>
        public static readonly int[] PLACE_DIV = { 24, 14, 8 };
        public static readonly int[] PLACE_YEARS = { 1, 3, 5 };

        public static int PlaceRealm(Place p) => PLACE_REALM[(int)p - 1];
        /// <summary>Exp of a month at a place for a level.</summary>
        public static long PlaceExp(Place p, int level) => NeedExp(level) / PLACE_DIV[(int)p - 1];
        /// <summary>Body exp of some months at a place, month by month (the level, so the exp of a month, grows; the bottleneck stops it).</summary>
        public static long PlaceTotalExp(Place p, int level, int months)
        {
            long total = 0;
            var lvl = Math.Max(1, level);
            long exp = 0;
            for (var m = 0; m < months; m++)
            {
                var add = PlaceExp(p, lvl);
                total += add;
                exp += add;
                while (exp >= NeedExp(lvl) && lvl < MAX_LEVEL && !IsRealmEnd(lvl))
                {
                    exp -= NeedExp(lvl);
                    lvl++;
                }
            }
            return total;
        }

        /// <summary>Months of a tempering seclusion are over: the body exp of the place, month by month. Returns the levels gained.</summary>
        public static int Temper(WorldUnitBase wunit, Place p, int months)
        {
            var up = 0;
            for (var m = 0; m < months && Get(wunit) != null; m++)
                up += AddExp(wunit, PlaceExp(p, GetLevel(wunit)));
            return up;
        }

        /// <summary>Tempering places a unit's body can stand (its realm is high enough); none for a unit without body refining.</summary>
        public static List<Place> PlacesFor(WorldUnitBase wunit)
        {
            var lvl = GetLevel(wunit);
            return lvl <= 0 ? new List<Place>() : PLACES.Where(x => Realm(lvl) >= PlaceRealm(x)).ToList();
        }

        /// <summary>
        /// A tempering place found on the world map (an encounter of EncounterEvent): the player chooses to stay 1, 3 or
        /// 5 years (SeclusionEvent runs the time) or to leave.
        /// </summary>
        public static void OfferPlace(Place p)
        {
            var d = Get(Player);
            if (d == null || Realm(d.Level) < PlaceRealm(p))
                return;
            var life = LifeLeft(Player);
            var opts = PLACE_YEARS.Select(y => string.Format(GameTool.LS("body_place_o"), y, PlaceTotalExp(p, d.Level, y * 12))).ToList();
            opts.Add(GameTool.LS("body_place_no"));
            var text = string.Format(GameTool.LS($"body_place_text_{(int)p}"), life / 12) + (life < 12 * PLACE_YEARS.Last() ? "\n" + string.Format(GameTool.LS("secl_warn_life"), life / 12) : string.Empty);
            DramaHelper.OpenDrama2(text, opts, x =>
            {
                var i = x.id % 10;
                if (i < 0 || i >= PLACE_YEARS.Length || SeclusionEvent.Instance == null || SeclusionEvent.IsSecluded)
                {
                    TaoistLogEvent.Log("log_body_place_left", GameTool.LS($"body_place_{(int)p}"));
                    return;
                }
                SeclusionEvent.Instance.Start(SeclusionEvent.KindOf(p), PLACE_YEARS[i] * 12);
            }, PlaceBackground(p));
        }

        private static void CloseWindow()
        {
            if (g.ui.HasUI(UIType.TextInfoLong))
                g.ui.CloseUI(UIType.TextInfoLong);
        }

        public static string PlaceBackground(Place p) => p == Place.Waterfall ? "ayuluoshui1" : p == Place.Lava ? "penchuyanjiang" : "dengxianleijie";

        // ------------------------------------------------------------------ body tribulation (a breakthrough by a trial battle)

        /// <summary>Trial of lightning dungeon (DungeonHelper.IsTrailOfLightning) used for the body tribulation.</summary>
        public const int TRIBULATION_DUNGEON = 110081;
        /// <summary>The MP of the player in a body tribulation stays under this share of the max MP: the flesh must hold.</summary>
        public const float TRIBULATION_MP = 0.2f;
        /// <summary>A tribulation survived tempers the body as a x2 dose (no chance roll); a failure costs as a x1 dose.</summary>
        public const int TRIBULATION_DOSE = 2;

        public static bool InTribulation => Enabled && Instance.TribulationLevel > 0 && GameHelper.IsInBattlle() && DungeonHelper.IsTrailOfLightning();

        /// <summary>Starts the body tribulation: the materials of a x1 breakthrough are used, then the trial battle opens.</summary>
        public static BreakResult StartTribulation(WorldUnitBase wunit)
        {
            var d = Get(wunit);
            if (d == null || d.Level >= MAX_LEVEL || !IsBottleneck(d))
                return BreakResult.NotReady;
            if (wunit.GetGradeLvl() < RealmGrade(d.Level + 1))
                return BreakResult.GradeTooLow;
            var needs = BreakNeeds(d.Level);
            if (!HasAll(wunit, needs))
                return BreakResult.NoMaterials;
            Consume(wunit, needs);
            Instance.TribulationLevel = d.Level;
            TaoistLogEvent.Log("log_body_trib_start", GameTool.LS($"body_realm_{Realm(d.Level + 1)}"));
            CloseWindow();
            g.world.battle.IntoBattle(new DataMap.MonstData { id = TRIBULATION_DUNGEON, level = Math.Max(1, wunit.GetGradeLvl()) * 5 });
            return BreakResult.Success;
        }

        /// <summary>End of the tribulation: survived = into the next realm (tempered as a x2 dose), fallen = a failed breakthrough.</summary>
        public static BreakResult EndTribulation(WorldUnitBase wunit, bool survived)
        {
            var d = Get(wunit);
            var from = Instance.TribulationLevel;
            Instance.TribulationLevel = 0;
            TribulationEnded = true;
            if (d == null || d.Level != from || !IsBottleneck(d))
                return BreakResult.NotReady;
            return Resolve(wunit, d, survived, survived ? TRIBULATION_DOSE : 1);
        }

        /// <summary>Caps the MP of the player in a body tribulation.</summary>
        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.True)]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            if (!InTribulation)
                return;
            var h = ModBattleEvent.PlayerUnit?.data?.TryCast<UnitDataHuman>();
            if (h == null)
                return;
            var cap = (int)(h.maxMP.value * TRIBULATION_MP);
            if (h.mp > cap)
                h.mp = cap;
        }

        /// <summary>The tribulation battle is over: survived or not.</summary>
        public override void OnBattleEndHandler(BattleEndHandler e)
        {
            base.OnBattleEndHandler(e);
            if (!InTribulation)
                return;
            var rs = EndTribulation(g.world.playerUnit, !ModBattleEvent.PlayerUnit.isDie);
            if (rs == BreakResult.Success)
                PendingMessage = string.Format(GameTool.LS("body_trib_ok"), RealmName(GetLevel(g.world.playerUnit)));
            else if (rs == BreakResult.Failure)
                PendingMessage = string.Format(GameTool.LS("body_trib_fail"), INJURY_MONTHS) + "\n" + string.Format(GameTool.LS("body_life_lost"), LastLifeLost)
                    + (LifeLeft(g.world.playerUnit) <= 0 ? "\n" + GameTool.LS("body_deadly") : "");
        }

        /// <summary>True from the end of a tribulation until the next battle (other handlers of the same battle end).</summary>
        public static bool TribulationEnded { get; private set; }

        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            TribulationEnded = false;
        }

        /// <summary>The result of a tribulation, shown once the player is back on the map.</summary>
        public static string PendingMessage { get; set; }
        /// <summary>The last tribulation result shown (tests).</summary>
        public static string LastMessage { get; private set; }

        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            PendingMessage = null;
            if (Enabled && Instance.BodySects.Count == 0)
                RefreshBodySects();
            // a save made during the trial battle: the tribulation is lost (its materials too)
            if (Instance.TribulationLevel > 0 && !GameHelper.IsInBattlle())
                Instance.TribulationLevel = 0;
        }

        // ------------------------------------------------------------------ formulas (pure, unit tests)

        /// <summary>Realm (1-5) of a level (1-15); 0 = not a body refiner.</summary>
        public static int Realm(int level) => level <= 0 ? 0 : (Math.Min(level, MAX_LEVEL) - 1) / LAYERS + 1;
        /// <summary>Layer (1-3) of a level inside its realm.</summary>
        public static int Layer(int level) => level <= 0 ? 0 : (Math.Min(level, MAX_LEVEL) - 1) % LAYERS + 1;
        /// <summary>Exp to go from a level to the next one.</summary>
        public static long NeedExp(int level) => (long)NEED_EXP_RATE * Math.Max(1, level) * Math.Max(1, level);
        /// <summary>True when the level is the last layer of a realm below the last realm (a breakthrough is needed).</summary>
        public static bool IsRealmEnd(int level) => level > 0 && level < MAX_LEVEL && Layer(level) == LAYERS;
        /// <summary>Chance (%) to break into the next realm from the last layer of a realm.</summary>
        public static float BreakChance(int level) => Math.Max(20f, 70f - 10f * (Realm(level) - 1));
        /// <summary>Cultivation grade needed to enter the realm of a level.</summary>
        public static int RealmGrade(int level) => REALM_GRADE[Math.Max(1, Realm(Math.Max(1, level))) - 1];
        /// <summary>Exp from a hit: share of the max HP lost x HIT_EXP (a hit counts at most one whole max HP).</summary>
        public static float HitExp(long dmg, long hpMax) => hpMax <= 0 || dmg <= 0 ? 0f : Math.Min(1f, (float)dmg / hpMax) * HIT_EXP;
        /// <summary>Natural treasures of a medicinal bath: how many, of which item level at least.</summary>
        public static (int count, int itemLevel) BathNeed(int level) { var r = Math.Max(1, Realm(Math.Max(1, level))) - 1; return (BATH_COUNT[r], BATH_ITEM_LEVEL[r]); }
        /// <summary>Exp of a medicinal bath.</summary>
        public static long BathExp(int level) => NeedExp(level) / BATH_EXP_SHARE;
        /// <summary>Bonus share of a level (injured: half).</summary>
        public static float Share(int level, bool injured) => level <= 0 ? 0f : level * (injured ? 0.5f : 1f);
        /// <summary>Cultivation exp needed (rate) for a body level: 1 + 0.1 x realm.</summary>
        public static float ExpRate(int level) => 1f + EXP_RATE_PER_REALM * Realm(level);
        /// <summary>Qi gain rate for a body level.</summary>
        public static float QiRateOf(int level) => QI_RATES[Realm(level)];
        /// <summary>Exp kept from a gain of the cultivation (the rest is absorbed by the body).</summary>
        public static int KeptExp(int gained, int level) => gained <= 0 ? gained : (int)Math.Round(gained / ExpRate(level));
        /// <summary>Bonus of a breakthrough of the cultivation: share of the base stat gained.</summary>
        public static int BreakBonus(int gained, float perRealm, int level) => gained <= 0 ? 0 : (int)Math.Round(gained * perRealm * Realm(level));

        // ------------------------------------------------------------------ data

        public static BodyData Get(WorldUnitBase wunit)
        {
            if (wunit == null || !Enabled)
                return null;
            return Instance.Bodies.TryGetValue(wunit.GetUnitId(), out var d) && d.Level > 0 ? d : null;
        }

        public static int GetLevel(WorldUnitBase wunit) => Get(wunit)?.Level ?? 0;
        public static bool IsInjured(WorldUnitBase wunit) => (Get(wunit)?.InjuryMonths ?? 0) > 0;

        /// <summary>Realm and layer name, e.g. "Iron Bone - Middle".</summary>
        public static string RealmName(int level) => level <= 0
            ? GameTool.LS("body_none")
            : string.Format(GameTool.LS("body_realm_layer"), GameTool.LS($"body_realm_{Realm(level)}"), GameTool.LS($"body_layer_{Layer(level)}"));

        /// <summary>Takes up body refining (level 1).</summary>
        public static BodyData Start(WorldUnitBase wunit)
        {
            var id = wunit.GetUnitId();
            if (!Instance.Bodies.TryGetValue(id, out var d))
                Instance.Bodies[id] = d = new BodyData();
            if (d.Level <= 0)
            {
                d.Level = 1;
                d.Exp = 0;
                if (wunit.IsPlayer())
                    TaoistLogEvent.Log("log_body_start");
            }
            return d;
        }

        /// <summary>True when the exp is full at the last layer of a realm (or at the top): only a breakthrough goes on.</summary>
        /// <summary>Essence a step of the top level: one level's exp (+1 % max HP, +0.75 % defense, +1 fist / palm / finger).</summary>
        public static long EssenceStep => NeedExp(MAX_LEVEL);
        public const float ESSENCE_HP = 0.01f;
        public const float ESSENCE_DEF = 0.0075f;
        public static double EssenceSteps(long essence) => Math.Max(0, essence) / (double)EssenceStep;

        /// <summary>True at the top level with full exp: the exp goes into the essence of the body.</summary>
        public static bool IsTop(BodyData d) => d != null && d.Level >= MAX_LEVEL && d.Exp >= NeedExp(MAX_LEVEL);

        public static bool IsBottleneck(BodyData d) => d != null && (d.Level >= MAX_LEVEL || IsRealmEnd(d.Level)) && d.Exp >= NeedExp(d.Level);

        /// <summary>Adds exp; layers inside a realm go up by themselves, the exp stops at the bottleneck. Returns the levels gained.</summary>
        public static int AddExp(WorldUnitBase wunit, double exp)
        {
            var d = Get(wunit);
            if (d == null || exp <= 0)
                return 0;
            exp *= SMGlobalConfigsEvent.Rate(c => c.BodyExpRate);
            var up = 0;
            d.Exp += (long)Math.Min(long.MaxValue / 4, exp);
            while (d.Exp >= NeedExp(d.Level))
            {
                if (d.Level >= MAX_LEVEL || IsRealmEnd(d.Level))
                {
                    // a bottleneck (a realm end or the top level): the rest becomes the essence of the body
                    d.Essence += d.Exp - NeedExp(d.Level);
                    d.Exp = NeedExp(d.Level);
                    break;
                }
                d.Exp -= NeedExp(d.Level);
                d.Level++;
                up++;
            }
            if (up > 0 && wunit.IsPlayer())
                TaoistLogEvent.Log("log_body_layer", RealmName(d.Level));
            return up;
        }

        public enum BreakResult { NotReady, GradeTooLow, NoMaterials, Success, Failure }

        /// <summary>
        /// Materials of the bag: the item classes it takes and what one item of each counts for, the lowest item level,
        /// the count needed. Tiered (herbs): what an item counts for depends on its level against the lowest level the
        /// cultivator may use (HerbFloor of his grade): low 2, middle 5, high 10 (x the class weight: an elixir x2), so
        /// a need takes 10 / 4 / 2 times its base in low / middle / high herbs; below the floor an herb is useless.
        /// </summary>
        public struct Need
        {
            public int[] Classes; public int[] Weights; public int Level; public int Count; public bool Tiered;
            public Need(int[] classes, int[] weights, int level, int count, bool tiered = false) { Classes = classes; Weights = weights; Level = level; Count = count; Tiered = tiered; }
            public bool IsHerbs => Tiered;
            public int ClassWeight(int cls) { var i = Array.IndexOf(Classes, cls); return i < 0 ? 0 : Weights[i]; }
        }

        /// <summary>Natural treasures (天材地宝 508, divine stones 506, immortal dews 531, orbs 532), one each.</summary>
        public static readonly int[] TREASURE_CLASSES = { TREASURE_CLASS, 506, 531, 532 };
        public static readonly int[] TREASURE_WEIGHTS = { 1, 1, 1, 1 };
        /// <summary>Herbs (药材 503, spirit materials 502), dews (104) one each, elixirs (101) twice.</summary>
        public static readonly int[] HERB_CLASSES = { HERB_CLASS, 502, 101, 104 };
        public static readonly int[] HERB_WEIGHTS = { 1, 1, 2, 1 };
        /// <summary>What a low / middle / high herb counts for (a need of base B is B x 20: 10 B low, 4 B middle, 2 B high).</summary>
        public const int HERB_LOW = 2, HERB_MID = 5, HERB_HIGH = 10, HERB_UNIT = 20;

        /// <summary>Lowest item level of the herbs a cultivator of a grade may use: 1 for grades 1-2 ... 5 for grades 9-10.</summary>
        public static int HerbFloor(int grade) => Math.Max(1, Math.Min(6, 1 + (Math.Max(1, grade) - 1) / 2));
        /// <summary>What an herb of an item level counts for, for a floor (0 below it).</summary>
        public static int HerbWeight(int itemLevel, int floor) => itemLevel < floor ? 0 : itemLevel == floor ? HERB_LOW : itemLevel == floor + 1 ? HERB_MID : HERB_HIGH;

        public static Need Treasures(int level, int count) => new Need(TREASURE_CLASSES, TREASURE_WEIGHTS, level, count);
        /// <summary>Herbs of a base: 10 x base low herbs, 4 x base middle, 2 x base high (or a mix).</summary>
        public static Need Herbs(int baseCount) => new Need(HERB_CLASSES, HERB_WEIGHTS, 1, baseCount * HERB_UNIT, true);

        /// <summary>Herbs of each kind a need takes alone: (low, middle, high).</summary>
        public static (int low, int mid, int high) HerbCounts(Need n) =>
            ((n.Count + HERB_LOW - 1) / HERB_LOW, (n.Count + HERB_MID - 1) / HERB_MID, (n.Count + HERB_HIGH - 1) / HERB_HIGH);
        /// <summary>A medicinal bath: natural treasures and herbs (or pills).</summary>
        public static Need[] BathNeeds(int level)
        {
            var r = Math.Max(1, Realm(Math.Max(1, level))) - 1;
            return new[] { Treasures(BATH_ITEM_LEVEL[r], BATH_COUNT[r]), Herbs(BATH_HERBS[r]) };
        }

        /// <summary>A breakthrough from a level (the end of a realm) into the next realm: natural treasures and herbs (or pills).</summary>
        public static Need[] BreakNeeds(int level)
        {
            var r = Math.Max(0, Math.Min(BREAK_HERBS.Length - 1, Realm(Math.Max(1, level)) - 1));
            return new[] { Treasures(BREAK_ITEM_LEVEL[r], BREAK_TREASURES[r]), Herbs(BREAK_HERBS[r]) };
        }

        /// <summary>What an item counts for in a need, for a cultivator (herbs: by their level against his floor).</summary>
        public static int WeightOf(WorldUnitBase wunit, Need n, ConfItemPropsItem conf)
        {
            if (conf == null || conf.level < n.Level) return 0;
            var w = n.ClassWeight(conf.className);
            if (w == 0 || !n.Tiered) return w;
            return w * HerbWeight(conf.level, HerbFloor(wunit.GetGradeLvl()));
        }

        /// <summary>The items of the bag a need can use, the cheapest for what they count first.</summary>
        public static List<DataProps.PropsData> Materials(WorldUnitBase wunit, Need n) =>
            wunit.GetUnitProps()
                .Select(x => (prop: x, conf: g.conf.itemProps.GetItem(x.propsID)))
                .Select(x => (x.prop, x.conf, w: WeightOf(wunit, n, x.conf)))
                .Where(x => x.w > 0)
                .OrderBy(x => (double)x.conf.worth / x.w).Select(x => x.prop).ToList();

        private static int WeightOf(WorldUnitBase wunit, Need n, DataProps.PropsData p) => WeightOf(wunit, n, g.conf.itemProps.GetItem(p.propsID));

        /// <summary>What the bag holds for a need (counted by weight).</summary>
        public static int Have(WorldUnitBase wunit, Need n) => Materials(wunit, n).Sum(x => x.propsCount * WeightOf(wunit, n, x));
        public static bool HasAll(WorldUnitBase wunit, Need[] needs) => needs.All(n => Have(wunit, n) >= n.Count);
        /// <summary>Takes the materials from the bag (the cheapest first).</summary>
        public static void Consume(WorldUnitBase wunit, Need[] needs)
        {
            foreach (var n in needs)
            {
                var left = n.Count;
                foreach (var p in Materials(wunit, n))
                {
                    if (left <= 0) break;
                    var w = Math.Max(1, WeightOf(wunit, n, p));
                    var k = Math.Min(p.propsCount, (left + w - 1) / w);
                    wunit.RemoveUnitProp(p.soleID, k);
                    left -= k * w;
                }
            }
        }

        /// <summary>The item a need shows: the cheapest of the bag, else the cheapest of the game (of its first class).</summary>
        public static int SampleId(WorldUnitBase wunit, Need n)
        {
            var mine = Materials(wunit, n).FirstOrDefault();
            if (mine != null)
                return mine.propsID;
            var first = n.Classes[0];
            var lvl = n.Tiered ? Math.Min(6, HerbFloor(wunit.GetGradeLvl()) + 2) : n.Level;
            return g.conf.itemProps._allConfList.ToArray().Where(x => x.className == first && x.level >= lvl).OrderBy(x => x.worth).FirstOrDefault()?.id ?? 0;
        }
        // ------------------------------------------------------------------ doses: more materials, more power, more risk

        public const int DOSE_MAX = 3;
        /// <summary>Gain of a dose over its materials: x1 +0 %, x2 +20 %, x3 +50 % (bath exp; tempering of a breakthrough).</summary>
        public static readonly float[] DOSE_GAIN = { 0f, 0.2f, 0.5f };
        /// <summary>Chance (%) of a backlash of a bath by dose, before the body's toughness.</summary>
        public static readonly float[] BATH_DOSE_RISK = { 5f, 15f, 30f };
        /// <summary>Months of lifespan a backlash costs by dose (x 0.5-2 at random, less for a tough body).</summary>
        public static readonly int[] BACKLASH_LIFE = { 6, 24, 72 };
        /// <summary>Points the chance of a breakthrough loses by dose.</summary>
        public static readonly float[] BREAK_DOSE_CHANCE = { 0f, 10f, 20f };
        /// <summary>A failed breakthrough: dose x 12 x realm months of lifespan (x 0.5-2, less for a tough body).</summary>
        public const int BREAK_LIFE_MONTHS = 12;
        /// <summary>Toughness: how much it lowers the risks (relative) and the lifespan lost, at most.</summary>
        public const float TOUGH_RISK = 0.3f;
        public const float TOUGH_LIFE = 0.5f;

        public static int Dose(int dose) => Math.Max(1, Math.Min(DOSE_MAX, dose));
        /// <summary>The materials of a dose: every count x dose.</summary>
        public static Need[] Scale(Need[] needs, int dose) =>
            needs.Select(n => new Need(n.Classes, n.Weights, n.Level, n.Count * Dose(dose), n.Tiered)).ToArray();
        /// <summary>Exp of a bath of a dose: dose x the exp of a bath x (1 + its gain).</summary>
        public static long BathExp(int level, int dose) => (long)Math.Round(BathExp(level) * Dose(dose) * (1 + DOSE_GAIN[Dose(dose) - 1]));
        public static float BreakChance(int level, int dose) => Math.Max(5f, BreakChance(level) - BREAK_DOSE_CHANCE[Dose(dose) - 1]);

        /// <summary>
        /// Toughness of a body (0-1), from its max HP, defense and fist + palm + finger: a small help against the risks of
        /// the strong doses (pure: the stats are given).
        /// </summary>
        public static float Toughness(long hpMax, long defense, long basis) =>
            (float)Math.Max(0, Math.Min(1, hpMax / 200000.0 + defense / 20000.0 + basis / 3000.0));
        public static float Toughness(WorldUnitBase wunit) => Toughness(
            wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value, wunit.GetDynProperty(UnitDynPropertyEnum.Defense).value,
            wunit.GetDynProperty(UnitDynPropertyEnum.BasisFist).value + wunit.GetDynProperty(UnitDynPropertyEnum.BasisPalm).value + wunit.GetDynProperty(UnitDynPropertyEnum.BasisFinger).value);

        /// <summary>Chance (%) of a backlash of a bath, for a body of a toughness.</summary>
        public static float BathRisk(int dose, float tough) => BATH_DOSE_RISK[Dose(dose) - 1] * (1 - TOUGH_RISK * tough);
        /// <summary>Months of lifespan a backlash of a bath costs (roll 0.5-2), for a body of a toughness.</summary>
        public static int BacklashLife(int dose, float tough, double roll) => (int)Math.Round(BACKLASH_LIFE[Dose(dose) - 1] * roll * (1 - TOUGH_LIFE * tough));
        /// <summary>Months of lifespan a failed breakthrough costs (roll 0.5-2), into the next realm of a level.</summary>
        public static int BreakLifeLoss(int level, int dose, float tough = 0f, double roll = 1.0) =>
            (int)Math.Round(Dose(dose) * BREAK_LIFE_MONTHS * (Realm(level) + 1) * roll * (1 - TOUGH_LIFE * tough));

        /// <summary>Months of lifespan left (lifespan - age); at 0 or below the cultivator dies of old age.</summary>
        public static int LifeLeft(WorldUnitBase wunit) => wunit.GetProperty<int>(UnitPropertyEnum.Life) - wunit.GetProperty<int>(UnitPropertyEnum.Age);

        /// <summary>Lifespan lost (none left: the unit dies of old age, as the game does). Returns the months lost.</summary>
        private static int LoseLife(WorldUnitBase wunit, int months)
        {
            if (months > 0)
                wunit.AddProperty<int>(UnitPropertyEnum.Life, -months);
            if (wunit.IsPlayer() && LifeLeft(wunit) <= 0)
                TaoistLogEvent.Log("log_body_deadly", months);
            return months;
        }

        /// <summary>Months of lifespan lost by the last backlash or failed breakthrough (window message).</summary>
        public static int LastLifeLost { get; private set; }
        /// <summary>
        /// Breaks into the next realm with a dose of materials (x1-x3). roll (0-100) is compared with the chance of the
        /// dose; a success of a strong dose tempers the body for good (+20 % / +50 % of the bonuses of a realm); a failure
        /// halves the exp, injures the body for INJURY_MONTHS months and costs lifespan (none left: the cultivator dies).
        /// </summary>
        public static BreakResult Breakthrough(WorldUnitBase wunit, float roll, int dose = 1)
        {
            var d = Get(wunit);
            dose = Dose(dose);
            if (d == null || d.Level >= MAX_LEVEL || !IsBottleneck(d))
                return BreakResult.NotReady;
            if (wunit.GetGradeLvl() < RealmGrade(d.Level + 1))
                return BreakResult.GradeTooLow;
            // the player pays natural treasures and herbs, kept or not (the NPCs refine their body without)
            if (wunit.IsPlayer())
            {
                var needs = Scale(BreakNeeds(d.Level), dose);
                if (!HasAll(wunit, needs))
                    return BreakResult.NoMaterials;
                Consume(wunit, needs);
            }
            return Resolve(wunit, d, roll < BreakChance(d.Level, dose) + DaoHeartEvent.BreakBonus(wunit), dose);
        }

        /// <summary>
        /// The outcome of a breakthrough: a success goes into the next realm (a strong dose tempers the body), a failure
        /// halves the exp, injures the body and costs lifespan by the dose.
        /// </summary>
        public static BreakResult Resolve(WorldUnitBase wunit, BodyData d, bool success, int dose)
        {
            dose = Dose(dose);
            var tough = Toughness(wunit);
            DaoHeartEvent.Add(wunit, success ? DaoHeartEvent.BREAK_OK : DaoHeartEvent.BREAK_FAIL, success ? "heart_src_break_ok" : "heart_src_break_fail");
            if (success)
            {
                d.Level++;
                d.Exp = 0;
                d.Temper += DOSE_GAIN[dose - 1];
                if (wunit.IsPlayer())
                    TaoistLogEvent.Log("log_body_realm", RealmName(d.Level));
                return BreakResult.Success;
            }
            d.Exp /= 2;
            d.InjuryMonths = INJURY_MONTHS;
            LastLifeLost = LoseLife(wunit, BreakLifeLoss(d.Level, dose, tough, CommonTool.Random(0.5f, 2f)));
            if (wunit.IsPlayer())
                TaoistLogEvent.Log("log_body_fail", INJURY_MONTHS);
            return BreakResult.Failure;
        }
        /// <summary>Natural treasures of the bag a bath can use (item class 508, item level high enough), cheapest first.</summary>
        public static List<DataProps.PropsData> BathTreasures(WorldUnitBase wunit) => Materials(wunit, BathNeeds(GetLevel(wunit))[0]);

        /// <summary>The treasure a bath of this level shows (the cheapest of the bag, else the cheapest of the game).</summary>
        public static int BathTreasureId(WorldUnitBase wunit) => SampleId(wunit, BathNeeds(GetLevel(wunit))[0]);
        public static bool CanBath(WorldUnitBase wunit)
        {
            var d = Get(wunit);
            // at a bottleneck the bath feeds the essence of the body
            return d != null && d.LastBathMonth != GameHelper.GetGameTotalMonth();
        }

        public static bool HasBathTreasures(WorldUnitBase wunit) => HasAll(wunit, BathNeeds(GetLevel(wunit)));

        public enum BathResult { NotNow, NoMaterials, Done, Backlash }

        /// <summary>
        /// A medicinal bath (once a month) with a dose of materials (x1-x3, the cheapest of the bag first): more exp, but
        /// a risk of backlash (roll 0-100 under BathRisk: no exp, injured a month, lifespan lost; none left: the cultivator dies).
        /// </summary>
        public static BathResult Bath(WorldUnitBase wunit, int dose, float roll)
        {
            var d = Get(wunit);
            dose = Dose(dose);
            if (!CanBath(wunit))
                return BathResult.NotNow;
            var needs = Scale(BathNeeds(d.Level), dose);
            if (!HasAll(wunit, needs))
                return BathResult.NoMaterials;
            Consume(wunit, needs);
            d.LastBathMonth = GameHelper.GetGameTotalMonth();
            var tough = Toughness(wunit);
            if (roll < BathRisk(dose, tough) * DaoHeartEvent.RiskRate(wunit))
            {
                d.InjuryMonths = Math.Max(d.InjuryMonths, 1);
                LastLifeLost = LoseLife(wunit, BacklashLife(dose, tough, CommonTool.Random(0.5f, 2f)));
                return BathResult.Backlash;
            }
            AddExp(wunit, BathExp(d.Level, dose));
            return BathResult.Done;
        }
        /// <summary>A medicinal bath of the usual dose, no backlash (tests, NPCs).</summary>
        public static bool Bath(WorldUnitBase wunit) => Bath(wunit, 1, 100f) == BathResult.Done;

        /// <summary>X of the column of a dose (1-3) in the body window.</summary>
        private static float DoseColumn(UICustom1 ui, int dose) => ui.Columns[ui.MidCol] + (dose - 2) * 330f;

        /// <summary>Materials of a dose, one a row under its column: the icon of the item used, "name+: have / need" (red when short). Returns the rows used.</summary>
        /// <summary>Height of a line of materials (an item icon of 32 px and a little space).</summary>
        public const float NEED_LINE = 36f;

        /// <summary>
        /// The materials of a dose under its button: one line each (icon, then its text, left-aligned in the column),
        /// NEED_LINE apart from the row `row` down. Returns the rows of the window they take.
        /// </summary>
        private static int NeedsRows(UICustom1 ui, float cx, int row, Need[] needs)
        {
            var x0 = cx - 140f;
            var y = ui.Rows[row];
            foreach (var n in needs)
            {
                var id = SampleId(Player, n);
                var have = Have(Player, n);
                if (id != 0)
                    ui.AddPropIcon(x0, y, id, 32f);
                var text = n.IsHerbs
                    ? string.Format(GameTool.LS("body_need_herbs"), Math.Min(100, have * 100 / Math.Max(1, n.Count)), HerbCounts(n).high, HerbCounts(n).mid, HerbCounts(n).low)
                    : string.Format(GameTool.LS("body_need_treasure"), id == 0 ? "-" : GameTool.LS(g.conf.itemProps.GetItem(id)?.name ?? ""), have, n.Count);
                var t = ui.AddText(x0 + 22f, y, text).Format(have >= n.Count ? new Color(0.1f, 0.4f, 0.1f) : Color.red, 12).Align(TextAnchor.MiddleLeft);
                t.Pos(x0 + 22f + t.Item.rectTransform.rect.width / 2f, y);
                y -= NEED_LINE;
            }
            var dy = Math.Max(1f, ui.Rows.Count > 1 ? Math.Abs(ui.Rows[0] - ui.Rows[1]) : NEED_LINE);
            return (int)Math.Ceiling(needs.Length * NEED_LINE / dy);
        }
        // ------------------------------------------------------------------ bonuses (UnitModifyEvent)

        /// <summary>Max HP: a share of the base max HP by level + the bonuses of the cultivation breakthroughs.</summary>
        public static int AdjHp(WorldUnitBase wunit, int baseHpMax) =>
            (int)Math.Min(int.MaxValue, baseHpMax * (HP_PER_LEVEL * Share(GetLevel(wunit), IsInjured(wunit)) + ESSENCE_HP * EssenceSteps(Get(wunit)?.Essence ?? 0) + HP_PER_LEVEL * LAYERS * (Get(wunit)?.Temper ?? 0)) + (Get(wunit)?.BonusHp ?? 0));
        /// <summary>Defense: a share of the base defense, at least 1 a level (the base defense of a low grade is small) + breakthrough bonuses.</summary>
        public static int AdjDef(WorldUnitBase wunit, int baseDef)
        {
            var share = Share(GetLevel(wunit), IsInjured(wunit));
            return (int)Math.Min(int.MaxValue, baseDef * (DEF_PER_LEVEL * share + ESSENCE_DEF * EssenceSteps(Get(wunit)?.Essence ?? 0) + DEF_PER_LEVEL * LAYERS * (Get(wunit)?.Temper ?? 0)) + share + (Get(wunit)?.BonusDef ?? 0));
        }
        /// <summary>Attack: only the bonuses of the cultivation breakthroughs.</summary>
        public static int AdjAtk(WorldUnitBase wunit) => Get(wunit)?.BonusAtk ?? 0;
        public static int AdjBasis(WorldUnitBase wunit) => (int)Math.Min(int.MaxValue, BASIS_PER_LEVEL * Share(GetLevel(wunit), IsInjured(wunit)) + EssenceSteps(Get(wunit)?.Essence ?? 0));
        /// <summary>Qi gain rate of a unit (QiCulEvent): lower for a body refiner, by realm.</summary>
        public static float QiRate(WorldUnitBase wunit) => QiRateOf(GetLevel(wunit));

        // ------------------------------------------------------------------ cultivation (exp absorbed, breakthrough bonuses)

        /// <summary>
        /// Follows the cultivation of a body refiner: exp gained in the same phase is partly absorbed by the body (the unit
        /// keeps gained / ExpRate); a new phase (breakthrough) adds a share of the base max HP / defense / attack it gave.
        /// Player: every second outside battles (pills included) and monthly; NPCs: monthly. Returns the bonuses added (HP).
        /// </summary>
        public static int Track(WorldUnitBase wunit)
        {
            var d = Get(wunit);
            if (d == null || wunit.isDie)
                return 0;
            var phase = wunit.GetPhaseId();
            var exp = wunit.GetExp();
            var hp = wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue;
            var def = wunit.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue;
            var atk = wunit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue;
            var added = 0;
            if (d.TrackPhase != 0)
            {
                if (phase == d.TrackPhase && exp > d.TrackExp)
                {
                    var gained = exp - d.TrackExp;
                    var kept = KeptExp(gained, d.Level);
                    if (kept < gained)
                    {
                        exp = Math.Max(wunit.GetMinExpCurrentPhase(), d.TrackExp + kept);
                        wunit.SetProperty<int>(UnitPropertyEnum.Exp, exp);
                        d.Absorbed += gained - kept;
                    }
                }
                else if (phase > d.TrackPhase)
                {
                    added = BreakBonus(hp - d.TrackHp, BREAK_HP_PER_REALM, d.Level);
                    d.BonusHp += added;
                    d.BonusDef += BreakBonus(def - d.TrackDef, BREAK_DEF_PER_REALM, d.Level);
                    d.BonusAtk += BreakBonus(atk - d.TrackAtk, BREAK_ATK_PER_REALM, d.Level);
                    if (wunit.IsPlayer())
                        TaoistLogEvent.Log("log_body_breakbonus", d.BonusHp, d.BonusDef, d.BonusAtk);
                }
            }
            d.TrackPhase = phase;
            d.TrackExp = exp;
            d.TrackHp = hp;
            d.TrackDef = def;
            d.TrackAtk = atk;
            return added;
        }

        /// <summary>Player: absorbs the exp of pills and breakthroughs as soon as they happen.</summary>
        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnTimeUpdate1000ms()
        {
            base.OnTimeUpdate1000ms();
            Track(g.world.playerUnit);
            // the result of a tribulation once the player is back on the map (not during the scene change)
            if (PendingMessage != null && g.world.playerUnit != null && BattleHelper.OnMap)
            {
                var msg = PendingMessage;
                PendingMessage = null;
                LastMessage = msg;
                InfoDialog.Msg(GameTool.LS("body_title"), msg);
            }
        }

        // ------------------------------------------------------------------ events

        /// <summary>The body refining sects are chosen again by their style every year.</summary>
        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnYearly()
        {
            base.OnYearly();
            RefreshBodySects();
        }

        // ------------------------------------------------------------------ NPCs: tempering places, tribulations

        /// <summary>Chance (%) a month that an NPC body refiner goes to the best tempering place it can stand.</summary>
        public const float NPC_PLACE_RATE = 1f;
        /// <summary>Share (%) of the NPC breakthroughs made by a tribulation (the others by chance).</summary>
        public const float NPC_TRIB_SHARE = 50f;

        /// <summary>Chance (%) an NPC survives a body tribulation: 45 + 40 x toughness + 3 x merit level, 15-90.</summary>
        public static float TribulationChance(float tough, int meritLevel) => Math.Max(15f, Math.Min(90f, 45f + 40f * tough + 3f * meritLevel));

        private static bool Known(WorldUnitBase wunit) =>
            FamilyEvent.IsFamilyOfPlayer(wunit) || Player.data.unitData.relationData.GetAllGoodRelationUnitID(false, false).ToArray().Contains(wunit.GetUnitId());

        /// <summary>An NPC at a tempering place: the exp of the place for the months spent there. Returns true while it stays.</summary>
        public static bool NpcAtPlace(WorldUnitBase wunit, BodyData d, int months)
        {
            if (d.PlaceMonths <= 0 || d.PlaceKind <= 0)
                return false;
            var m = Math.Min(months, d.PlaceMonths);
            for (var i = 0; i < m; i++)
                AddExp(wunit, PlaceExp((Place)d.PlaceKind, d.Level));
            d.PlaceMonths -= m;
            if (d.PlaceMonths <= 0)
            {
                TaoistLogEvent.LogIf(Known(wunit), "log_npc_body_place_end", wunit.GetName(), GameTool.LS($"body_place_{d.PlaceKind}"), RealmName(d.Level));
                d.PlaceKind = 0;
            }
            return true;
        }

        /// <summary>An NPC goes to the best tempering place it can stand for 1, 3 or 5 years. Returns the place (null: none).</summary>
        public static Place? NpcGoToPlace(WorldUnitBase wunit, BodyData d, int yearsIndex)
        {
            var places = PlacesFor(wunit);
            if (places.Count == 0 || d.PlaceMonths > 0)
                return null;
            var p = places.Last();
            d.PlaceKind = (int)p;
            d.PlaceMonths = 12 * PLACE_YEARS[Math.Max(0, Math.Min(PLACE_YEARS.Length - 1, yearsIndex))];
            TaoistLogEvent.LogIf(Known(wunit), "log_npc_body_place", wunit.GetName(), GameTool.LS($"body_place_{(int)p}"), d.PlaceMonths / 12);
            return p;
        }

        /// <summary>An NPC at its bottleneck breaks through: by a tribulation (roll against TribulationChance) or by chance.</summary>
        public static BreakResult NpcBreak(WorldUnitBase wunit, BodyData d, bool tribulation, float roll)
        {
            if (!tribulation)
                return Breakthrough(wunit, roll);
            if (d.Level >= MAX_LEVEL || !IsBottleneck(d))
                return BreakResult.NotReady;
            if (wunit.GetGradeLvl() < RealmGrade(d.Level + 1))
                return BreakResult.GradeTooLow;
            var ok = roll < TribulationChance(Toughness(wunit), MeritEvent.Level(wunit));
            var rs = Resolve(wunit, d, ok, ok ? TRIBULATION_DOSE : 1);
            TaoistLogEvent.LogIf(Known(wunit), ok ? "log_npc_body_trib_ok" : "log_npc_body_trib_fail", wunit.GetName(), RealmName(d.Level));
            return rs;
        }

        /// <summary>Training, injury, NPCs taking up the path, going to tempering places and breaking through.</summary>
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            var months = Math.Max(1, ModMaster.MonthsPassed);
            var d = Get(wunit);
            if (d == null)
            {
                if (!wunit.IsPlayer() && !wunit.isDie && CommonTool.Random(0f, 100f) < StartRate(InBodySect(wunit)) * months)
                    Start(wunit);
                return;
            }
            Track(wunit);
            var injured = d.InjuryMonths > 0;
            if (injured)
                d.InjuryMonths = Math.Max(0, d.InjuryMonths - months);
            if (wunit.IsPlayer())
            {
                if (!injured)
                    AddExp(wunit, TrainExp(true, InBodySect(wunit), months));
                return;
            }
            // NPCs: a tempering place instead of the training while they stay there
            if (NpcAtPlace(wunit, d, months))
                return;
            if (!injured)
                AddExp(wunit, TrainExp(false, InBodySect(wunit), months));
            if (IsBottleneck(d))
                NpcBreak(wunit, d, CommonTool.Random(0f, 100f) < NPC_TRIB_SHARE, CommonTool.Random(0f, 100f));
            else if (!injured && CommonTool.Random(0f, 100f) < NPC_PLACE_RATE * months)
                NpcGoToPlace(wunit, d, CommonTool.Random(0, PLACE_YEARS.Length));
        }

        /// <summary>Damage taken by a body refiner (world unit) gives exp.</summary>
        public override void OnBattleUnitHit(UnitHit e)
        {
            base.OnBattleUnitHit(e);
            var human = e?.hitUnit?.data?.TryCast<UnitDataHuman>();
            var wunit = human?.worldUnitData?.unit;
            if (wunit == null || e.hitData == null || GetLevel(wunit) <= 0 || IsInjured(wunit))
                return;
            var hpMax = human.maxHP.value;
            AddExp(wunit, HitExp(Math.Abs((long)e.hitData.hitValue), hpMax));
        }

        /// <summary>Button on the cultivation screen; the realm of NPC body refiners in their info.</summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.FateFeature.uiName)
            {
                // left of the Qi lines (QiCulEvent), where the screen is empty
                var ui = new UICover<UIFateFeature>(e.ui);
                // bottom left corner, under the grades
                ui.AddButton(ui.MidCol - 8, ui.MidRow - 13, OpenWindow, GameTool.LS("body_button")).Size(160f, 30f);
                ui.AddText(ui.MidCol - 8, ui.MidRow - 12, "{0}").Format(Color.black, 13).SetWork(new UIItemWork
                {
                    Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { RealmName(GetLevel(Player)) }),
                });
                ui.IsAutoUpdate = true;
                ui.UpdateUI();
            }
            else if (e.uiType.uiName == UIType.School.uiName)
            {
                // a sect that teaches body refining says so
                var uiSchool = g.ui.GetUI<UISchool>(UIType.School);
                if (IsBodySect(uiSchool?.school?.schoolNameID))
                {
                    var ui = new UICover<UISchool>(uiSchool);
                    ui.AddText(ui.MidCol, ui.FirstRow + 1, string.Format(GameTool.LS("body_sect_ui"), SECT_TRAIN_RATE)).Format(new Color(0.55f, 0.1f, 0.1f), 15);
                    ui.UpdateUI();
                }
            }
            else if (e.uiType.uiName == UIType.NPCInfo.uiName)
            {
                var uiBase = g.ui.GetUI<UINPCInfo>(e.ui.uiType);
                var lvl = GetLevel(uiBase?.unit);
                if (lvl > 0 && !uiBase.unit.IsPlayer() && !HirePeopleEvent.isShowHirePeopleUI)
                {
                    var ui = new UICover<UINPCInfo>(uiBase);
                    ui.AddText(ui.MidCol - 6, ui.MidRow - 5, string.Format(GameTool.LS("body_npc"), RealmName(lvl))).Align().Format(Color.white, 16)
                        .SetParentTransform(ui.UI.uiHeart.goGroupRoot);
                    ui.UpdateUI();
                }
            }
        }

        // ------------------------------------------------------------------ window

        private static WorldUnitBase Player => g.world.playerUnit;

        /// <summary>Body refining window of the player.</summary>
        public static void OpenWindow()
        {
            var ui = new UICustom1(GameTool.LS("body_title"));
            var row = ui.FirstRow + 1;
            var d = Get(Player);
            if (d == null)
            {
                ui.AddText(ui.MidCol, row++, GameTool.LS("body_intro")).Format(null, 15);
                row += 3;
                ui.AddButton(ui.MidCol, row, () =>
                {
                    InfoDialog.Msg(GameTool.LS("body_title"), GameTool.LS("body_start_confirm"), MsgBoxButtonEnum.YesNo, () =>
                    {
                        Start(Player);
                        OpenWindow();
                    });
                }, GameTool.LS("body_start")).Size(240, 40);
                return;
            }

            var lvl = d.Level;
            ui.AddText(ui.MidCol, row++, RealmName(lvl)).Format(null, 20);
            var left = LifeLeft(Player);
            ui.AddText(ui.Columns[ui.LastCol] - 150f, ui.Rows[ui.FirstRow], string.Format(GameTool.LS("body_life_left"), left / 12, left % 12)).Format(left < 120 ? Color.red : new Color(0.3f, 0.3f, 0.3f), 13);
            ui.AddText(ui.MidCol, row++, IsBottleneck(d)
                ? (lvl >= MAX_LEVEL ? GameTool.LS("body_top") : GameTool.LS("body_bottleneck"))
                : string.Format(GameTool.LS("body_exp"), d.Exp, NeedExp(lvl))).Format(null, 15);
            if (d.Essence > 0 || IsBottleneck(d))
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_essence"), d.Essence, Math.Round(ESSENCE_HP * EssenceSteps(d.Essence) * 100, 1), Math.Round(ESSENCE_DEF * EssenceSteps(d.Essence) * 100, 1), (int)EssenceSteps(d.Essence))).Format(new Color(0.45f, 0.2f, 0.55f), 15);
            if (d.InjuryMonths > 0)
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_injured"), d.InjuryMonths)).Format(Color.red, 15);
            row++;
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_bonus_hp"), AdjHp(Player, Player.GetDynProperty(UnitDynPropertyEnum.HpMax).baseValue))).Format(null, 15);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_bonus_def"), AdjDef(Player, Player.GetDynProperty(UnitDynPropertyEnum.Defense).baseValue))).Format(null, 15);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_bonus_basis"), AdjBasis(Player))).Format(null, 15);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_bonus_break"), d.BonusHp, d.BonusDef, d.BonusAtk)).Format(null, 15);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_break_rates"),
                Math.Round(BREAK_HP_PER_REALM * Realm(lvl) * 100), Math.Round(BREAK_DEF_PER_REALM * Realm(lvl) * 100), Math.Round(BREAK_ATK_PER_REALM * Realm(lvl) * 100))).Format(null, 14);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_exp_rate"), ExpRate(lvl), d.Absorbed)).Format(new Color(0.5f, 0.15f, 0.1f), 14);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_bonus_qi"), (int)Math.Round((1 - QiRateOf(lvl)) * 100))).Format(new Color(0.5f, 0.15f, 0.1f), 14);
            row++;

            if (d.Temper > 0)
                ui.AddText(ui.MidCol, row - 1, string.Format(GameTool.LS("body_temper"), Math.Round(HP_PER_LEVEL * LAYERS * d.Temper * 100, 1), Math.Round(DEF_PER_LEVEL * LAYERS * d.Temper * 100, 1))).Format(new Color(0.45f, 0.2f, 0.55f), 14);
            var tough = Toughness(Player);

            // medicinal bath: a column a dose (button, risk, its materials)
            var used = 0;
            for (var k = 1; k <= DOSE_MAX; k++)
            {
                var dose = k;
                var cx = DoseColumn(ui, k);
                ui.AddButton(cx, ui.Rows[row], () =>
                {
                    var rs = Bath(Player, dose, CommonTool.Random(0f, 100f));
                    if (rs == BathResult.NotNow)
                        InfoDialog.Msg(GameTool.LS("body_title"), GameTool.LS(IsBottleneck(Get(Player)) && GetLevel(Player) < MAX_LEVEL ? "body_bath_bottleneck" : "body_bath_done"));
                    else if (rs == BathResult.NoMaterials)
                        InfoDialog.Msg(GameTool.LS("body_title"), GameTool.LS("body_bath_need"));
                    else
                    {
                        OpenWindow();
                        if (rs == BathResult.Backlash)
                            InfoDialog.Msg(GameTool.LS("body_title"), string.Format(GameTool.LS("body_backlash"), LastLifeLost) + (LifeLeft(Player) <= 0 ? "\n" + GameTool.LS("body_deadly") : ""));
                    }
                }, string.Format(GameTool.LS("body_bath_dose"), dose, BathExp(lvl, dose))).Size(300, 36);
                // its risk right under the button, then its materials
                ui.AddText(cx, ui.Rows[row + 1], string.Format(GameTool.LS("body_risk"), BathRisk(dose, tough), (int)Math.Round(BACKLASH_LIFE[dose - 1] * (1 - TOUGH_LIFE * tough)))).Format(new Color(0.5f, 0.15f, 0.1f), 12);
                used = Math.Max(used, NeedsRows(ui, cx, row + 2, Scale(BathNeeds(lvl), dose)));
            }
            row += 2 + used;

            // breakthrough: a column a dose too (the materials are used, kept or not)
            if (IsBottleneck(d) && lvl < MAX_LEVEL)
            {
                var grade = RealmGrade(lvl + 1);
                used = 0;
                for (var k = 1; k <= DOSE_MAX; k++)
                {
                    var dose = k;
                    var cx = DoseColumn(ui, k);
                    ui.AddButton(cx, ui.Rows[row], () =>
                    {
                        var rs = Breakthrough(Player, CommonTool.Random(0f, 100f), dose);
                        switch (rs)
                        {
                            case BreakResult.GradeTooLow:
                                InfoDialog.Msg(GameTool.LS("body_title"), string.Format(GameTool.LS("body_break_grade"), GameTool.LS($"body_realm_{Realm(lvl + 1)}"), grade));
                                return;
                            case BreakResult.NoMaterials:
                                InfoDialog.Msg(GameTool.LS("body_title"), GameTool.LS("body_break_need"));
                                return;
                            case BreakResult.Success:
                                OpenWindow();
                                InfoDialog.Msg(GameTool.LS("body_title"), string.Format(GameTool.LS("body_break_ok"), RealmName(GetLevel(Player))));
                                return;
                            case BreakResult.Failure:
                                OpenWindow();
                                InfoDialog.Msg(GameTool.LS("body_title"), string.Format(GameTool.LS("body_break_fail"), INJURY_MONTHS) + "\n"
                                    + string.Format(GameTool.LS("body_life_lost"), LastLifeLost) + (LifeLeft(Player) <= 0 ? "\n" + GameTool.LS("body_deadly") : ""));
                                return;
                        }
                    }, string.Format(GameTool.LS("body_break_dose"), dose, BreakChance(lvl, dose), Math.Round(DOSE_GAIN[dose - 1] * 100))).Size(300, 36);
                    ui.AddText(cx, ui.Rows[row + 1], string.Format(GameTool.LS("body_break_risk"), BreakLifeLoss(lvl, dose, tough))).Format(new Color(0.5f, 0.15f, 0.1f), 12);
                    used = Math.Max(used, NeedsRows(ui, cx, row + 2, Scale(BreakNeeds(lvl), dose)));
                }
                row += 2 + used;

                // or the body tribulation: a trial battle of lightning with little MP (materials of a x1 dose, no chance roll)
                ui.AddButton(DoseColumn(ui, 1), ui.Rows[row], () =>
                {
                    switch (StartTribulation(Player))
                    {
                        case BreakResult.GradeTooLow:
                            InfoDialog.Msg(GameTool.LS("body_title"), string.Format(GameTool.LS("body_break_grade"), GameTool.LS($"body_realm_{Realm(lvl + 1)}"), grade));
                            return;
                        case BreakResult.NoMaterials:
                            InfoDialog.Msg(GameTool.LS("body_title"), GameTool.LS("body_break_need"));
                            return;
                    }
                }, GameTool.LS("body_trib_button")).Size(300, 36);
                ui.AddText(DoseColumn(ui, 2) + 165f, ui.Rows[row], string.Format(GameTool.LS("body_trib_hint"), Math.Round(TRIBULATION_MP * 100), Math.Round(DOSE_GAIN[TRIBULATION_DOSE - 1] * 100)))
                    .Format(new Color(0.5f, 0.15f, 0.1f), 12);
                row += 2;
            }

            if (InBodySect(Player))
                ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_sect"), Player.data.school?.name, SECT_TRAIN_RATE)).Format(new Color(0.1f, 0.4f, 0.1f), 13);
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("body_materials_hint"), HerbFloor(Player.GetGradeLvl()))).Format(Color.gray, 12);
            ui.AddText(ui.MidCol, row, GameTool.LS("body_dose_hint")).Format(new Color(0.55f, 0.1f, 0.1f), 12);
            ui.AddText(ui.MidCol, ui.LastRow - 1, GameTool.LS("body_help")).Format(null, 13);
        }
    }
}
