using System;

namespace ModLib.Helper
{
    /// <summary>
    /// What one month dispatch stands for when the calendar moved several months at once (time skip): months and
    /// year boundaries between two dispatched months, chances over several months. Pure (unit tests); ModMaster
    /// uses it for MonthsPassed / YearsPassed / Ticks / ChanceOver.
    /// </summary>
    public static class TimeScaleHelper
    {
        /// <summary>Most months one dispatch stands for.</summary>
        public const int MAX_MONTHS_PASSED = 1200;

        /// <summary>Months between two dispatched months (1 when unknown or going back).</summary>
        public static int MonthsBetween(int prev, int cur) => prev < 0 || cur <= prev ? 1 : Math.Min(MAX_MONTHS_PASSED, cur - prev);

        /// <summary>Year boundaries (months multiple of 12) crossed between two dispatched months.</summary>
        public static int YearsBetween(int prev, int cur) =>
            prev < 0 || cur <= prev ? (cur % 12 == 0 ? 1 : 0) : Math.Min(MAX_MONTHS_PASSED / 12, FloorDiv(cur, 12) - FloorDiv(prev, 12));

        /// <summary>Multiples of period in (now - months, now].</summary>
        public static int Ticks(int now, int months, int period) =>
            period <= 1 ? months : FloorDiv(now, period) - FloorDiv(now - months, period);

        /// <summary>Chance (0-100) of something with a monthly (or yearly) chance happening at least once over n months (years).</summary>
        public static float ChanceOver(float chance, int n)
        {
            if (n <= 1) return chance;
            var p = Math.Max(0f, Math.Min(100f, chance)) / 100f;
            return (float)(1.0 - Math.Pow(1.0 - p, n)) * 100f;
        }

        public static int FloorDiv(int a, int b) => a >= 0 ? a / b : -((-a + b - 1) / b);
    }
}
