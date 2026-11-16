using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOD_nE7UL2.Mod;

namespace UnitTests.Taoist
{
    /// <summary>FamilyEvent / SeclusionEvent formulas (no game state).</summary>
    [TestClass]
    public class FamilyFormulaTests
    {
        [TestMethod, Description("FamilyEvent.MarryChance: 0 under 18 or with under 20 years left, grows from 18 to 30, falls with the share of life lived")]
        public void MarryChance_ByAge()
        {
            Check.Eq(0f, FamilyEvent.MarryChance(17, 100), "17 years");
            Check.Eq(0f, FamilyEvent.MarryChance(85, 100), "15 years left");
            Check.True(FamilyEvent.MarryChance(18, 100) < FamilyEvent.MarryChance(29, 100), "18 < 29 years");
            Check.True(FamilyEvent.MarryChance(29, 100) <= FamilyEvent.MARRY_RATE, "at most MARRY_RATE");
            Check.True(FamilyEvent.MarryChance(60, 100) < FamilyEvent.MarryChance(30, 100), "60 < 30 years (life 100)");
            Check.True(FamilyEvent.MarryChance(60, 1000) > FamilyEvent.MarryChance(60, 100), "a long-lived cultivator stays young longer");
        }

        [TestMethod, Description("FamilyEvent.BirthChance: lower with age, grade and children born; 0 at MAX_CHILDREN or 80 % of life lived")]
        public void BirthChance_Falls()
        {
            Check.True(FamilyEvent.BirthChance(0.2f, 1f, 0) > FamilyEvent.BirthChance(0.5f, 1f, 0), "older couple");
            Check.True(FamilyEvent.BirthChance(0.2f, 1f, 0) > FamilyEvent.BirthChance(0.2f, 6f, 0), "higher grade");
            Check.True(FamilyEvent.BirthChance(0.2f, 1f, 0) > FamilyEvent.BirthChance(0.2f, 1f, 2), "children already born");
            Check.Eq(0f, FamilyEvent.BirthChance(0.2f, 1f, FamilyEvent.MAX_CHILDREN), "MAX_CHILDREN");
            Check.Eq(0f, FamilyEvent.BirthChance(0.8f, 1f, 0), "80 % of life lived");
        }

        [TestMethod, Description("FamilyEvent.FatherSurname: S&M rule father / mother / random (by the roll)")]
        public void Surname_Rule()
        {
            Check.True(FamilyEvent.FatherSurname(FamilyEvent.Surname.Father, 1), "father");
            Check.False(FamilyEvent.FatherSurname(FamilyEvent.Surname.Mother, 0), "mother");
            Check.True(FamilyEvent.FatherSurname(FamilyEvent.Surname.Random, 0), "random, even roll");
            Check.False(FamilyEvent.FatherSurname(FamilyEvent.Surname.Random, 1), "random, odd roll");
        }

        [TestMethod, Description("TimeSkipEvent: a skip saves 4 times (1 year: every 3 months, 2 years: every 6, 10 years: every 30), always at the end")]
        public void TimeSkip_SavesFourTimes()
        {
            Check.Eq(3, TimeSkipEvent.SaveEvery(12), "1 year");
            Check.Eq(6, TimeSkipEvent.SaveEvery(24), "2 years");
            Check.Eq(30, TimeSkipEvent.SaveEvery(120), "10 years");
            Check.Eq(1, TimeSkipEvent.SaveEvery(1), "1 month");
            foreach (var months in new[] { 1, 5, 12, 24, 37, 120, 360 })
            {
                var saves = 0;
                for (var m = 1; m <= months; m++) if (TimeSkipEvent.SaveAfter(m, months)) saves++;
                Check.Eq(System.Math.Min(months, TimeSkipEvent.SAVES), saves, $"{months} months: saves");
                Check.True(TimeSkipEvent.SaveAfter(months, months), $"{months} months: saved at the end");
            }
        }

        [TestMethod, Description("FamilyEvent.InheritIntim: the old player's friendships count half, enmities in full; the heir's own bond wins unless either side is an enmity")]
        public void Inherit_Intimacy()
        {
            Check.Eq(50f, FamilyEvent.InheritIntim(100f, null).Value, "friend of the old player: half");
            Check.Eq(-100f, FamilyEvent.InheritIntim(-100f, null).Value, "enemy of the old player: in full");
            Check.Eq(30f, FamilyEvent.InheritIntim(null, 30f).Value, "the heir's own bond");
            Check.Eq(60f, FamilyEvent.InheritIntim(120f, 20f).Value, "both friends: the warmest");
            Check.Eq(-40f, FamilyEvent.InheritIntim(-40f, 90f).Value, "enemy of the old player, friend of the heir: the enmity stays");
            Check.Eq(-70f, FamilyEvent.InheritIntim(100f, -70f).Value, "enemy of the heir: the enmity stays");
            Check.True(!FamilyEvent.InheritIntim(null, null).HasValue, "no bond");
        }

        [TestMethod, Description("FamilyFundEvent: upkeep grade² x 30 (child 20), a gift is 2-4 months of upkeep, the upkeep is paid from the money then the storage stones")]
        public void FamilyFund_Formulas()
        {
            Check.Eq(30, FamilyFundEvent.BaseUpkeep(1, false), "grade 1");
            Check.Eq(270, FamilyFundEvent.BaseUpkeep(3, false), "grade 3");
            Check.Eq(FamilyFundEvent.MINOR_UPKEEP, FamilyFundEvent.BaseUpkeep(5, true), "child");
            Check.Eq(2 * 270, FamilyFundEvent.BaseGift(3, 0), "lowest gift: 2 months");
            Check.Eq(4 * 270, FamilyFundEvent.BaseGift(3, 99), "highest gift: 4 months");
            Check.True(FamilyFundEvent.GIFT_RATE < 5f && FamilyFundEvent.USE_RATE < 5f, "small chances");
            var (m, s, u) = FamilyFundEvent.Split(1000, 600, 300);
            Check.Eq(600L, m, "money first");
            Check.Eq(300L, s, "then the storage");
            Check.Eq(100L, u, "the rest unpaid");
            var (m2, s2, u2) = FamilyFundEvent.Split(500, 600, 300);
            Check.Eq(500L, m2, "money enough");
            Check.Eq(0L, s2 + u2, "nothing else");
        }

        [TestMethod, Description("TimeSkipEvent.Stride / NextStep: the mods run every N months for a skip of N years (12 runs), the 4 save points are single months of the game")]
        public void TimeSkip_Stride()
        {
            Check.Eq(1, TimeSkipEvent.Stride(12), "1 year: every month");
            Check.Eq(2, TimeSkipEvent.Stride(24), "2 years: every 2 months");
            Check.Eq(100, TimeSkipEvent.Stride(1200), "100 years: every 100 months");
            foreach (var months in new[] { 12, 24, 120, 1200 })
            {
                int done = 0, runs = 0, saves = 0;
                while (done < months)
                {
                    var (to, save) = TimeSkipEvent.NextStep(done, months);
                    Check.True(to > done && to - done <= TimeSkipEvent.Stride(months), $"{months}: step {done} -> {to}");
                    if (save) saves++; else runs++;
                    done = to;
                }
                Check.Eq(TimeSkipEvent.SAVES, saves, $"{months} months: save points");
                Check.True(runs + saves <= 12 + TimeSkipEvent.SAVES, $"{months} months: {runs} runs of the mods + {saves} of the game");
            }
            Check.Eq((1, false), TimeSkipEvent.NextStep(0, 120, true), "full simulation: every month (a month run of the game, saved at the save points only)");
        }

        [TestMethod, Description("TimeScaleHelper.MonthsBetween / YearsBetween / Ticks / ChanceOver: what one month dispatch stands for")]
        public void ModMaster_MonthsPassed()
        {
            Check.Eq(1, global::ModLib.Helper.TimeScaleHelper.MonthsBetween(10, 11), "normal month");
            Check.Eq(10, global::ModLib.Helper.TimeScaleHelper.MonthsBetween(10, 20), "10 months at once");
            Check.Eq(1, global::ModLib.Helper.TimeScaleHelper.MonthsBetween(-1, 5), "first dispatch");
            Check.Eq(1, global::ModLib.Helper.TimeScaleHelper.YearsBetween(11, 12), "year boundary");
            Check.Eq(0, global::ModLib.Helper.TimeScaleHelper.YearsBetween(12, 13), "no boundary");
            Check.Eq(8, global::ModLib.Helper.TimeScaleHelper.YearsBetween(100, 200), "100 months: 8 boundaries (108 ... 192)");
            Check.Eq(5f, global::ModLib.Helper.TimeScaleHelper.ChanceOver(5f, 1), "1 month");
            Check.Eq(1, global::ModLib.Helper.TimeScaleHelper.Ticks(30, 1, 3), "normal month, multiple of 3");
            Check.Eq(0, global::ModLib.Helper.TimeScaleHelper.Ticks(31, 1, 3), "normal month, not a multiple");
            Check.Eq(3, global::ModLib.Helper.TimeScaleHelper.Ticks(40, 10, 3), "10 months (31-40): 33, 36, 39");
            Check.True(System.Math.Abs(global::ModLib.Helper.TimeScaleHelper.ChanceOver(10f, 2) - 19f) < 0.001f, "10 % twice: 19 %");
            Check.True(global::ModLib.Helper.TimeScaleHelper.ChanceOver(1f, 1000) > 99.9f, "almost sure over a long time");
        }

        [TestMethod, Description("TimeSkipEvent.CatchUpAge: the catch-up ages a unit up to its last month of lifespan (the game's month run handles the death)")]
        public void TimeSkip_CatchUpAge()
        {
            Check.Eq(129, TimeSkipEvent.CatchUpAge(100, 600, 29), "young unit: + 29 months");
            Check.Eq(599, TimeSkipEvent.CatchUpAge(590, 600, 29), "old unit: up to life - 1");
            Check.Eq(605, TimeSkipEvent.CatchUpAge(605, 600, 29), "already over its lifespan: unchanged");
            Check.Eq(100, TimeSkipEvent.CatchUpAge(100, 600, 0), "no month");
        }

        [TestMethod, Description("SeclusionEvent.ForcedMonths: a forced seclusion keeps FORCED_KEEP_LIFE months of lifespan; under a year it does not happen")]
        public void ForcedSeclusion_NeverKills()
        {
            Check.Eq(60, SeclusionEvent.ForcedMonths(5, 600), "5 years with 50 years left");
            Check.Eq(100 - SeclusionEvent.FORCED_KEEP_LIFE, SeclusionEvent.ForcedMonths(20, 100), "shortened to the lifespan");
            Check.Eq(0, SeclusionEvent.ForcedMonths(5, 20), "8 months possible: no seclusion");
        }
    }
}
