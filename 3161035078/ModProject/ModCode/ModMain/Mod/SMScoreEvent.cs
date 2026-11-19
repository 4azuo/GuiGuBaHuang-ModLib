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
    /// What the S&amp;M score (P) of a save does. P is the total score of the S&amp;M configs when the save was created
    /// (SMGlobalConfigsEvent.Score, copied into SMLocalConfigsEvent): harder settings give more P, easier ones less.
    /// 1. Rewards: battle exp and battle loot (items, spirit stones) x RewardMultiplier (P / 2,000 %, -20 % .. +50 %).
    /// 2. Titles: a title per tier of P in the player info, and a record of every character (P, best realm, years,
    ///    generations) kept across the saves (<see cref="SMScoreRecordsEvent"/>, Taoist Settings > Score records).
    /// 3. Content: the rare encounter "Heavenly trial" only from the tier Ascetic (30,000 P).
    /// 4. Start: a new save with P &gt; 0 starts with ability points and spirit stones (StartAbilityPoints / StartStones).
    /// 5. Easy features cost more: continuing as a descendant with P &lt; 0 keeps only a share of the spirit stones
    ///    (InheritShare, 100 % at 0 P down to 50 % at -20,000 P).
    /// </summary>
    [Cache(ModConst.SM_SCORE_EVENT)]
    public class SMScoreEvent : ModEvent
    {
        public static SMScoreEvent Instance { get; set; }

        // ------------------------------------------------------------------ formulas (no game state)

        /// <summary>Lowest P of each tier (title keys sm_title_0 .. sm_title_5).</summary>
        public static readonly int[] TIER_MIN = { int.MinValue, 0, 10000, 30000, 60000, 100000 };
        /// <summary>Icon of each tier (sprites of the game's TextMeshPro asset).</summary>
        public static readonly string[] TIER_ICONS = { "xinqing", "xingyun", "wuxing", "daodian", "zhandouxiaoguo", "daoli" };
        /// <summary>Tier of the rare encounter "Heavenly trial" (Ascetic).</summary>
        public const int HEAVEN_TIER = 3;
        public const int P_PER_REWARD_PERCENT = 2000;
        public const float MIN_REWARD_PERCENT = -20f;
        public const float MAX_REWARD_PERCENT = 50f;
        public const int P_PER_ABILITY_POINT = 10000;
        public const int MAX_START_ABILITY_POINTS = 10;
        public const int P_PER_START_STONE = 5;
        public const int MAX_START_STONES = 20000;
        /// <summary>P at which a descendant keeps only MIN_INHERIT_SHARE of the spirit stones.</summary>
        public const int INHERIT_FLOOR_P = -20000;
        public const float MIN_INHERIT_SHARE = 0.5f;

        /// <summary>Tier of a score (0 .. 5).</summary>
        public static int Tier(int p)
        {
            var t = 0;
            for (var i = 0; i < TIER_MIN.Length; i++)
                if (p >= TIER_MIN[i]) t = i;
            return t;
        }

        /// <summary>Reward bonus (%) of a score: P / 2,000, from -20 to +50.</summary>
        public static float RewardPercent(int p) => Math.Max(MIN_REWARD_PERCENT, Math.Min(MAX_REWARD_PERCENT, p / (float)P_PER_REWARD_PERCENT));

        /// <summary>Multiplier of the battle rewards (1 + RewardPercent / 100).</summary>
        public static double RewardMultiplier(int p) => 1d + RewardPercent(p) / 100d;

        /// <summary>Ability points given to a new save (P / 10,000, at most 10, none under 0 P).</summary>
        public static int StartAbilityPoints(int p) => p <= 0 ? 0 : Math.Min(MAX_START_ABILITY_POINTS, p / P_PER_ABILITY_POINT);

        /// <summary>Spirit stones given to a new save (P / 5, at most 20,000, none under 0 P).</summary>
        public static int StartStones(int p) => p <= 0 ? 0 : Math.Min(MAX_START_STONES, p / P_PER_START_STONE);

        /// <summary>Share of the spirit stones a descendant keeps (1 at 0 P and more, 0.5 at -20,000 P and less).</summary>
        public static float InheritShare(int p)
        {
            if (p >= 0) return 1f;
            if (p <= INHERIT_FLOOR_P) return MIN_INHERIT_SHARE;
            return 1f - (1f - MIN_INHERIT_SHARE) * p / (float)INHERIT_FLOOR_P;
        }

        /// <summary>The rare encounter is unlocked for a score.</summary>
        public static bool HeavenUnlocked(int p) => Tier(p) >= HEAVEN_TIER;

        /// <summary>A reward count scaled by the multiplier (roll 0..1 rounds the fraction: 2.4 = 2, or 3 when roll &lt; 0.4).</summary>
        public static int Scale(int count, double multiplier, double roll)
        {
            var v = count * multiplier;
            var whole = Math.Floor(v);
            return (int)Math.Max(0, whole + (roll < v - whole ? 1 : 0));
        }

        // ------------------------------------------------------------------ the save

        /// <summary>P of the current save (0 without a save).</summary>
        public static int P => SMLocalConfigsEvent.Instance?.Configs?.Score ?? 0;

        public static string TitleText(int p) => GameTool.LS($"sm_title_{Tier(p)}");
        public static string TitleLine(int p) => TextIconHelper.ToSprites($"[icon:{TIER_ICONS[Tier(p)]}]") + " " + string.Format(GameTool.LS("sm_title_line"), TitleText(p), p, RewardPercent(p).ToString("+0.#;-0.#;0"));

        /// <summary>True until the start bonus of a new save is given (set when the save is created).</summary>
        public bool StartBonusPending { get; set; }
        /// <summary>Best grade reached by the player (records).</summary>
        public int BestGrade { get; set; }

        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            if (isNew) StartBonusPending = true;
        }

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            GiveStartBonus();
            UpdateRecord();
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            GiveStartBonus();
            UpdateRecord();
        }

        /// <summary>Idea 4: the start bonus of a new save (once).</summary>
        public void GiveStartBonus()
        {
            if (!StartBonusPending || g.world?.playerUnit == null) return;
            StartBonusPending = false;
            int p = P, points = StartAbilityPoints(p), stones = StartStones(p);
            if (points <= 0 && stones <= 0) return;
            var player = g.world.playerUnit;
            if (points > 0) player.AddProperty<int>(UnitPropertyEnum.AbilityPoint, points);
            if (stones > 0) player.AddUnitMoney(stones);
            TaoistLogEvent.Log("log_score_start", p, points, stones);
            DebugHelper.WriteLine($"SMScoreEvent: start bonus for {p} P: +{points} ability points, +{stones} spirit stones");
        }

        /// <summary>Idea 2: the record of this character (global, across the saves).</summary>
        public void UpdateRecord()
        {
            var player = g.world?.playerUnit;
            if (player == null || SMScoreRecordsEvent.Instance == null) return;
            BestGrade = Math.Max(BestGrade, player.GetGradeLvl());
            SMScoreRecordsEvent.Instance.Upsert(new SMScoreRecordsEvent.Record
            {
                UnitId = player.GetUnitId(), Name = player.GetName(), P = P, BestGrade = BestGrade,
                Years = GameHelper.GetGameYear(), Generation = FamilyEvent.Instance?.Generation ?? 1,
                Updated = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            });
        }

        /// <summary>Idea 1: battle exp x RewardMultiplier (BattleRewardEventV2).</summary>
        public static int ScaleExp(int exp) => Scale(exp, RewardMultiplier(P), 0.5d);

        /// <summary>Idea 1: battle loot of a won battle x RewardMultiplier (counts of the items and spirit stones).</summary>
        public static void ScaleLoot(IEnumerable<DataProps.PropsData> loot)
        {
            var m = RewardMultiplier(P);
            if (Math.Abs(m - 1d) < 0.0001d) return;
            foreach (var item in loot)
            {
                if (item == null || item.propsCount <= 0) continue;
                var count = Scale(item.propsCount, m, CommonTool.Random(0f, 1f));
                item.propsCount = Math.Max(1, count);
            }
        }

        /// <summary>Records window (Taoist Settings): every character, best P first.</summary>
        public static void OpenRecords()
        {
            var ui = new UICustom1(GameTool.LS("sm_records_title"));
            var row = ui.FirstRow + 1;
            if (g.world?.playerUnit != null)
                ui.AddText(ui.MidCol, row++, TitleLine(P)).Format(new Color(0.45f, 0.12f, 0.08f), 17, FontStyle.Bold);
            ui.AddText(ui.MidCol, row++, GameTool.LS("sm_records_desc")).Format(null, 13);
            row++;
            var records = SMScoreRecordsEvent.Instance?.Records.OrderByDescending(x => x.P).ThenByDescending(x => x.BestGrade).Take(SMScoreRecordsEvent.SHOWN).ToList()
                ?? new List<SMScoreRecordsEvent.Record>();
            if (records.Count == 0)
                ui.AddText(ui.MidCol, row++, GameTool.LS("sm_records_none")).Format(null, 14);
            var rank = 1;
            foreach (var r in records)
                ui.AddText(ui.MidCol, row++, TextIconHelper.ToSprites($"[icon:{TIER_ICONS[Tier(r.P)]}]") + " " +
                    string.Format(GameTool.LS("sm_records_line"), rank++, r.Name, TitleText(r.P), r.P, r.BestGrade, r.Years, r.Generation, r.Updated)).Format(null, 14);
        }
    }

    /// <summary>
    /// Records of the characters (global, every save): name, P, title, best realm, years, generations. Kept in the
    /// global cache of the mod (like the S&amp;M configs), shown by SMScoreEvent.OpenRecords.
    /// </summary>
    [Cache(ModConst.SM_SCORE_RECORDS_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class SMScoreRecordsEvent : ModEvent
    {
        public static SMScoreRecordsEvent Instance { get; set; }

        public const int MAX_RECORDS = 50;
        public const int SHOWN = 18;

        public class Record
        {
            public string UnitId { get; set; }
            public string Name { get; set; }
            public int P { get; set; }
            public int BestGrade { get; set; }
            public int Years { get; set; }
            public int Generation { get; set; }
            public string Updated { get; set; }
        }

        public List<Record> Records { get; set; } = new List<Record>();

        /// <summary>Adds or updates the record of a character (saved at once; the best MAX_RECORDS kept).</summary>
        public void Upsert(Record r)
        {
            var old = Records.FirstOrDefault(x => x.UnitId == r.UnitId);
            if (old != null)
            {
                if (old.P == r.P && old.BestGrade == r.BestGrade && old.Years == r.Years && old.Name == r.Name && old.Generation == r.Generation)
                    return;
                Records.Remove(old);
            }
            Records.Add(r);
            if (Records.Count > MAX_RECORDS)
                Records = Records.OrderByDescending(x => x.P).ThenByDescending(x => x.BestGrade).Take(MAX_RECORDS).ToList();
            CacheHelper.SaveGlobalCache(this);
        }
    }
}
