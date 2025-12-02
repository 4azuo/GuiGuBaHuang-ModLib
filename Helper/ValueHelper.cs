using ModLib.Attributes;
using System;
using System.Linq;

namespace ModLib.Helper
{
    [ActionCat("Value")]
    public static class ValueHelper
    {
        public static bool IsBetween(this double x, double from, double to)
        {
            return x >= from && x <= to;
        }

        public static bool IsBetween(this long x, long from, long to)
        {
            return x >= from && x <= to;
        }

        public static bool IsBetween(this int x, int from, int to)
        {
            return x >= from && x <= to;
        }

        public static bool IsBetween(this float x, float from, float to)
        {
            return x >= from && x <= to;
        }

        public static int FixValue(this int x, int min = int.MinValue, int max = int.MaxValue)
        {
            return Math.Max(Math.Min(x, max), min);
        }

        public static long FixValue(this long x, long min = long.MinValue, long max = long.MaxValue)
        {
            return Math.Max(Math.Min(x, max), min);
        }

        public static float FixValue(this float x, float min = float.MinValue, float max = float.MaxValue)
        {
            return Math.Max(Math.Min(x, max), min);
        }

        public static double FixValue(this double x, double min = double.MinValue, double max = double.MaxValue)
        {
            return Math.Max(Math.Min(x, max), min);
        }

        public static int Is<T>(this T? x, T compareValue) where T : struct
        {
            if (!x.HasValue)
                return -1;
            if (x.Value.Equals(compareValue))
                return 1;
            return 0;
        }

        public static bool NearlyEqual(double a, double b, double epsilon)
        {
            const double MIN_NORMAL = 2.2250738585072014E-308d;
            var absA = Math.Abs(a);
            var absB = Math.Abs(b);
            var diff = Math.Abs(a - b);

            if (a.Equals(b))
            {
                // shortcut, handles infinities
                return true;
            }
            else if (a == 0 || b == 0 || absA + absB < MIN_NORMAL)
            {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * MIN_NORMAL);
            }
            else
            {
                // use relative error
                return diff / (absA + absB) < epsilon;
            }
        }

        public static int SafeSumInt(params long[] values)
        {
            return values.Sum().FixValue(int.MinValue, int.MaxValue).Parse<int>();
        }

        public static long SumBigNum(params long[] values)
        {
            return values.Sum();
        }

        public static float SafeSumInt(params double[] values)
        {
            return values.Sum().FixValue(float.MinValue, float.MaxValue).Parse<float>();
        }

        public static double SumBigNum(params double[] values)
        {
            return values.Sum();
        }
    }
}