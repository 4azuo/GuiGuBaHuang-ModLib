using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;

namespace UnitTests.ModLib
{
    /// <summary>ValueHelper and ParseHelper (pure helpers).</summary>
    [TestClass]
    public class ValueParseHelperTests
    {
        [TestMethod, Description("ValueHelper.IsBetween: both bounds are inclusive, for int/float/double/long")]
        public void IsBetween_IsInclusive()
        {
            Check.True(5.IsBetween(5, 10), "5 in [5, 10]");
            Check.True(10.IsBetween(5, 10), "10 in [5, 10]");
            Check.False(4.IsBetween(5, 10), "4 in [5, 10]");
            Check.True(0.5f.IsBetween(0f, 1f), "0.5f in [0, 1]");
            Check.False(10.0001d.IsBetween(5d, 10d), "10.0001 in [5, 10]");
            Check.True(3L.IsBetween(-3L, 3L), "3L in [-3, 3]");
        }

        [TestMethod, Description("ValueHelper.FixValue: clamps a value into [min, max]")]
        public void FixValue_ClampsToRange()
        {
            Check.Eq(0, (-5).FixValue(0, 10), "-5 clamped to [0, 10]");
            Check.Eq(10, 99.FixValue(0, 10), "99 clamped to [0, 10]");
            Check.Eq(7, 7.FixValue(0, 10), "7 clamped to [0, 10]");
            Check.Eq(1.5f, 1.5f.FixValue(0f, 2f), "1.5f clamped to [0, 2]");
            Check.Eq(long.MaxValue, long.MaxValue.FixValue(), "long.MaxValue with default range");
        }

        [TestMethod, Description("ValueHelper.Is(int?): -1 when null, 1 when equal, 0 otherwise")]
        public void Is_ReturnsTriState()
        {
            int? none = null;
            Check.Eq(-1, none.Is(1), "null.Is(1)");
            Check.Eq(1, ((int?)1).Is(1), "1.Is(1)");
            Check.Eq(0, ((int?)2).Is(1), "2.Is(1)");
        }

        [TestMethod, Description("ValueHelper.NearlyEqual: relative error comparison (screen ratios, big numbers, zero)")]
        public void NearlyEqual_UsesRelativeError()
        {
            Check.True(ValueHelper.NearlyEqual(1600d / 900d, 16d / 9d, 0.001), "1600/900 ~ 16/9 (eps 0.001)");
            Check.True(ValueHelper.NearlyEqual(1000000d, 1000000.5d, 0.001), "1000000 ~ 1000000.5");
            Check.False(ValueHelper.NearlyEqual(1d, 1.1d, 0.001), "1 ~ 1.1");
            Check.True(ValueHelper.NearlyEqual(0d, 0d, 0.001), "0 ~ 0");
            Check.False(ValueHelper.NearlyEqual(0d, 0.1d, 0.001), "0 ~ 0.1");
        }

        [TestMethod, Description("ValueHelper.SafeSumInt / SumBigNum: sums clamp to the int range instead of overflowing")]
        public void SafeSumInt_ClampsToIntRange()
        {
            Check.Eq(int.MaxValue, ValueHelper.SafeSumInt(int.MaxValue, 10L), "int.MaxValue + 10");
            Check.Eq(int.MinValue, ValueHelper.SafeSumInt(int.MinValue, -10L), "int.MinValue - 10");
            Check.Eq(30, ValueHelper.SafeSumInt(10L, 20L), "10 + 20");
            Check.Eq(float.MaxValue, ValueHelper.SafeSumFloat(double.MaxValue, 1d), "SafeSumFloat(double.MaxValue + 1) clamped to float.MaxValue");
            Check.Eq(1.5f, ValueHelper.SafeSumFloat(1d, 0.5d), "SafeSumFloat(1 + 0.5)");
            Check.Eq(5_000_000_000L, ValueHelper.SumBigNum(int.MaxValue, int.MaxValue, 705032706L), "SumBigNum(int.MaxValue, int.MaxValue, 705032706)");
        }

        [TestMethod, Description("ValueHelper.ToIntClamp / SafeAddInt / SafeMulInt: growing values stop at the int limits (Parse<int> returns 0 on overflow, int * wraps to negative)")]
        public void IntClamp_StopsAtIntLimits()
        {
            Check.Eq(0, (1e20d).Parse<int>(), "Parse<int>(1e20) falls back to 0 (why ToIntClamp exists)");
            Check.Eq(int.MaxValue, (1e20d).ToIntClamp(), "ToIntClamp(1e20)");
            Check.Eq(int.MinValue, (-1e20d).ToIntClamp(), "ToIntClamp(-1e20)");
            Check.Eq(0, double.NaN.ToIntClamp(), "ToIntClamp(NaN)");
            Check.Eq(3, (2.6f).ToIntClamp(), "ToIntClamp(2.6f) rounds");
            Check.Eq(int.MaxValue, ValueHelper.SafeAddInt(int.MaxValue - 5, 1000), "int.MaxValue - 5 + 1000");
            Check.Eq(int.MaxValue, ValueHelper.SafeMulInt(1_000_000, 5000), "1e6 x 5000");
            Check.Eq(-2_000_000_000, ValueHelper.SafeMulInt(-1_000_000_000, 2), "-1e9 x 2 (in range)");
            Check.Eq(100, ValueHelper.SafeAddInt(100, double.NaN), "100 + NaN (invalid increment) stays 100");
            Check.Eq(100, ValueHelper.SafeMulInt(100, double.NaN), "100 x NaN (invalid factor) stays 100");
        }

        [TestMethod, Description("ParseHelper.Parse<T>: string/number conversions used by configs")]
        public void Parse_ConvertsCommonTypes()
        {
            Check.Eq(12, "12".Parse<int>(), "\"12\" -> int");
            Check.Eq(1.5d, "1.5".Parse<float>(), 0.0001d, "\"1.5\" -> float");
            Check.Eq(3, 3.0f.Parse<int>(), "3.0f -> int");
            Check.Eq("42", 42.Parse<string>(), "42 -> string");
            Check.True("true".Parse<bool>(), "\"true\" -> bool");
        }

        [TestMethod, Description("ParseHelper.Parse<T>: invalid input returns the default (or the given fallback)")]
        public void Parse_ReturnsDefaultOnFailure()
        {
            Check.Eq(0, "abc".Parse<int>(), "\"abc\" -> int");
            Check.Eq(-1, "abc".Parse<int>(-1), "\"abc\" -> int with fallback -1");
            Check.Eq(0, ((object)null).Parse<int>(), "null -> int");
        }

        public class Src { public int X { get; set; } public string Y { get; set; } public int Extra { get; set; } }
        public class Dst { public int X { get; set; } public string Y { get; set; } }

        [TestMethod, Description("ParseHelper.ParseJson: copies matching properties between two types through JSON")]
        public void ParseJson_CopiesBetweenTypes()
        {
            Check.Info("source: Src { X = 3, Y = \"y\", Extra = 9 } -> Dst");
            var dst = new Src { X = 3, Y = "y", Extra = 9 }.ParseJson<Dst>();
            Check.Eq(3, dst.X, "Dst.X");
            Check.Eq("y", dst.Y, "Dst.Y");
            var obj = (Dst)new Src { X = 4 }.ParseJson(typeof(Dst));
            Check.Eq(4, obj.X, "ParseJson(typeof(Dst)).X");
        }
    }
}
