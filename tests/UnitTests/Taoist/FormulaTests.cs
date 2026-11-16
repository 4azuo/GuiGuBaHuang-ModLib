using Microsoft.VisualStudio.TestTools.UnitTesting;
using MOD_nE7UL2.Mod;

namespace UnitTests.Taoist
{
    /// <summary>Pure Taoist formulas (no game state).</summary>
    [TestClass]
    public class FormulaTests
    {
        [TestMethod, Description("ExpertEvent.GetExpertNeedExp(level, grade, itemLevel): exp needed per expertise level, doubles each level")]
        public void ExpertNeedExp_DoublesEachLevel()
        {
            Check.Eq(0, ExpertEvent.GetExpertNeedExp(0, 5, 5), "level 0");
            Check.Eq(1000, ExpertEvent.GetExpertNeedExp(1, 1, 0), "level 1, grade 1, item level 0");
            Check.Eq(1200, ExpertEvent.GetExpertNeedExp(1, 1, 1), "level 1, grade 1, item level 1");
            Check.Eq(4000, ExpertEvent.GetExpertNeedExp(1, 2, 5), "level 1, grade 2, item level 5");
            for (var lvl = 1; lvl < 10; lvl++)
                Check.Eq(2d * ExpertEvent.GetExpertNeedExp(lvl, 3, 2), ExpertEvent.GetExpertNeedExp(lvl + 1, 3, 2), 1, $"level {lvl + 1} = 2 x level {lvl} (grade 3, item level 2)");
        }

        [TestMethod, Description("SMLocalConfigsEvent.Calculate(value, rate) = value x (1 + rate), not below 0")]
        public void SMCalculate_AppliesRate()
        {
            var sm = new SMLocalConfigsEvent();
            Check.Eq(150d, sm.Calculate(100, 0.5), "100 with rate +0.5");
            Check.Eq(100d, sm.Calculate(100, 0), "100 with rate 0");
            Check.Eq(50d, sm.Calculate(100, -0.5), "100 with rate -0.5");
            Check.Eq(0d, sm.Calculate(100, -1), "100 with rate -1");
        }

        [TestMethod, Description("PriceHelper.UpPrice(price, grade, level, ratio, min, max): price x grade ratio x level ratio x ratio, clamped")]
        public void UpPrice_UsesGradeAndLevelRatios()
        {
            Check.Info($"GRADE_RATIO = {Check.Fmt(PriceHelper.GRADE_RATIO)}");
            Check.Info($"LEVEL_RATIO = {Check.Fmt(PriceHelper.LEVEL_RATIO)}");
            Check.Eq(100, PriceHelper.UpPrice(100, 1, 1), "100, grade 1, level 1");
            Check.Eq(400, PriceHelper.UpPrice(100, 10, 1), "100, grade 10 (ratio 4.0)");
            Check.Eq(180, PriceHelper.UpPrice(100, 1, 6), "100, level 6 (ratio 1.8)");
            Check.Eq(PriceHelper.UpPrice(100, 10, 6), PriceHelper.UpPrice(100, 99, 99), "grade/level 99 clamped to 10/6");
            Check.Eq(PriceHelper.UpPrice(100, 1, 1), PriceHelper.UpPrice(100, 0, 0), "grade/level 0 clamped to 1");
            Check.Eq(200, PriceHelper.UpPrice(100, 1, 1, 2f), "custom ratio 2");
            Check.Eq(150, PriceHelper.UpPrice(100, 10, 6, 1f, -1, 150), "max 150");
            Check.Eq(500, PriceHelper.UpPrice(10, 1, 1, 1f, 500), "min 500");
        }

        [TestMethod, Description("PriceHelper ratio tables: one ratio per grade/level, never decreasing; item type ratios >= 1")]
        public void PriceRatios_AreSortedAndComplete()
        {
            Check.Eq(10, PriceHelper.GRADE_RATIO.Count, "GRADE_RATIO count (one per grade)");
            Check.Eq(6, PriceHelper.LEVEL_RATIO.Count, "LEVEL_RATIO count (one per item level)");
            for (var i = 1; i < PriceHelper.GRADE_RATIO.Count; i++)
                Check.True(PriceHelper.GRADE_RATIO[i] >= PriceHelper.GRADE_RATIO[i - 1], $"grade ratio {i + 1} >= grade ratio {i}", $"{PriceHelper.GRADE_RATIO[i - 1]} -> {PriceHelper.GRADE_RATIO[i]}");
            for (var i = 1; i < PriceHelper.LEVEL_RATIO.Count; i++)
                Check.True(PriceHelper.LEVEL_RATIO[i] >= PriceHelper.LEVEL_RATIO[i - 1], $"level ratio {i + 1} >= level ratio {i}", $"{PriceHelper.LEVEL_RATIO[i - 1]} -> {PriceHelper.LEVEL_RATIO[i]}");
            foreach (var r in PriceHelper.ITEM_TYPE_RATIO)
                Check.True(r.Value >= 1f, $"{r.Key} ratio >= 1", r.Value);
        }

        [TestMethod, Description("BattleModifyEvent.BlockRatio: block ratio per grade never decreases; null unit -> 0")]
        public void BlockRatio_GrowsWithGrade()
        {
            var t = BattleModifyEvent.BlockRatio;
            Check.Eq(10, t.Count, "BlockRatio entries (one per grade)");
            for (var g = 2; g <= 10; g++)
                Check.True(t[g] >= t[g - 1], $"grade {g} >= grade {g - 1}", $"{t[g - 1]} -> {t[g]}");
            Check.Eq(0f, BattleModifyEvent.GetBlockRatio(null), "GetBlockRatio(null)");
            Check.Eq(0f, BattleModifyEvent.GetBlockMpCost(null), "GetBlockMpCost(null)");
        }
    }
}
