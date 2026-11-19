using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOD_nE7UL2.Mod;

namespace UnitTests.Taoist
{
    /// <summary>SMScoreEvent (what the S&amp;M score P does) and DamageInfo formulas (no game state).</summary>
    [TestClass]
    public class SMScoreTests
    {
        [TestMethod, Description("SMScoreEvent.Tier: <0 Leisurely, 0 Mortal, 10k Seeker, 30k Ascetic, 60k Asura, 100k Heaven Defier")]
        public void Tier_Thresholds()
        {
            Check.Eq(0, SMScoreEvent.Tier(-1), "-1 P");
            Check.Eq(1, SMScoreEvent.Tier(0), "0 P");
            Check.Eq(1, SMScoreEvent.Tier(9999), "9,999 P");
            Check.Eq(2, SMScoreEvent.Tier(10000), "10,000 P");
            Check.Eq(3, SMScoreEvent.Tier(30000), "30,000 P");
            Check.Eq(4, SMScoreEvent.Tier(60000), "60,000 P");
            Check.Eq(5, SMScoreEvent.Tier(100000), "100,000 P");
            Check.Eq(5, SMScoreEvent.Tier(int.MaxValue), "max");
            Check.Eq(SMScoreEvent.TIER_MIN.Length, SMScoreEvent.TIER_ICONS.Length, "an icon per tier");
        }

        [TestMethod, Description("Idea 1 - SMScoreEvent.RewardPercent / RewardMultiplier: P / 2,000 %, clamped to -20 .. +50 %")]
        public void Reward_Clamped()
        {
            Check.Eq(0f, SMScoreEvent.RewardPercent(0), "0 P");
            Check.Eq(5f, SMScoreEvent.RewardPercent(10000), "10,000 P: +5 %");
            Check.Eq(50f, SMScoreEvent.RewardPercent(100000), "100,000 P: +50 %");
            Check.Eq(50f, SMScoreEvent.RewardPercent(1000000), "1,000,000 P: still +50 %");
            Check.Eq(-20f, SMScoreEvent.RewardPercent(-1000000), "-1,000,000 P: -20 %");
            Check.Eq(1.2d, SMScoreEvent.RewardMultiplier(40000), "40,000 P: x1.2");
        }

        [TestMethod, Description("Idea 1 - SMScoreEvent.Scale: count x multiplier, the fraction rounded up when the roll is under it, never below 0")]
        public void Scale_Rounds_ByRoll()
        {
            Check.Eq(120, SMScoreEvent.Scale(100, 1.2d, 0.5d), "100 x 1.2");
            Check.Eq(2, SMScoreEvent.Scale(2, 1.2d, 0.5d), "2.4, roll 0.5: 2");
            Check.Eq(3, SMScoreEvent.Scale(2, 1.2d, 0.3d), "2.4, roll 0.3: 3");
            Check.Eq(0, SMScoreEvent.Scale(0, 1.5d, 0d), "nothing stays nothing");
            Check.Eq(80, SMScoreEvent.Scale(100, 0.8d, 0.5d), "100 x 0.8");
        }

        [TestMethod, Description("Idea 4 - start bonus: P / 10,000 ability points (max 10), P / 5 spirit stones (max 20,000), none at 0 P or less")]
        public void StartBonus()
        {
            Check.Eq(0, SMScoreEvent.StartAbilityPoints(-5000), "negative P");
            Check.Eq(0, SMScoreEvent.StartStones(0), "0 P");
            Check.Eq(5, SMScoreEvent.StartAbilityPoints(50000), "50,000 P: 5 points");
            Check.Eq(10000, SMScoreEvent.StartStones(50000), "50,000 P: 10,000 stones");
            Check.Eq(SMScoreEvent.MAX_START_ABILITY_POINTS, SMScoreEvent.StartAbilityPoints(500000), "points capped");
            Check.Eq(SMScoreEvent.MAX_START_STONES, SMScoreEvent.StartStones(500000), "stones capped");
        }

        [TestMethod, Description("Idea 5 - SMScoreEvent.InheritShare: 100 % from 0 P, linear down to 50 % at -20,000 P, 50 % below")]
        public void InheritShare_Falls()
        {
            Check.Eq(1f, SMScoreEvent.InheritShare(50000), "positive P");
            Check.Eq(1f, SMScoreEvent.InheritShare(0), "0 P");
            Check.Eq(0.75f, SMScoreEvent.InheritShare(-10000), "-10,000 P");
            Check.Eq(0.5f, SMScoreEvent.InheritShare(-20000), "-20,000 P");
            Check.Eq(0.5f, SMScoreEvent.InheritShare(-90000), "-90,000 P");
        }

        [TestMethod, Description("Idea 3 - SMScoreEvent.HeavenUnlocked: the rare encounter from the tier Ascetic (30,000 P)")]
        public void Heaven_Unlock()
        {
            Check.False(SMScoreEvent.HeavenUnlocked(29999), "29,999 P");
            Check.True(SMScoreEvent.HeavenUnlocked(30000), "30,000 P");
            Check.True(SMScoreEvent.HeavenUnlocked(200000), "200,000 P");
        }

        [TestMethod, Description("DamageInfo.HitsToKill / Final: hits to bring the HP to 0, final value of an example hit")]
        public void DamageInfo_Hits()
        {
            Check.Eq(0, DamageInfo.HitsToKill(0, 100), "no damage: never");
            Check.Eq(4, DamageInfo.HitsToKill(30, 100), "30 of 100 HP: 4 hits");
            Check.Eq(1, DamageInfo.HitsToKill(500, 100), "one hit");
            Check.Eq(0, DamageInfo.Final(new System.Collections.Generic.List<System.Tuple<string, int>>()), "no step");
            Check.Eq(80, DamageInfo.Final(new System.Collections.Generic.List<System.Tuple<string, int>> { System.Tuple.Create("base", 100), System.Tuple.Create("min", 80) }), "last step");
            Check.Eq(DamageInfo.BASIS.Length, DamageInfo.BASIS_ICONS.Length, "an icon per basis");
        }

        [TestMethod, Description("TaoistLogEvent.Icon: an icon per feature of the entry key, a default for the others")]
        public void TaoistLog_Icons()
        {
            Check.Eq("zm_lingshi", TaoistLogEvent.Icon("log_loan_borrow"), "loan");
            Check.Eq("zm_renshu", TaoistLogEvent.Icon("log_family_birth"), "family");
            Check.Eq("daoli", TaoistLogEvent.Icon("log_score_start"), "S&M score");
            Check.Eq("xingyun", TaoistLogEvent.Icon("something_else"), "default");
            Check.Eq("xingyun", TaoistLogEvent.Icon(null), "no key");
        }
    }
}
