using ModLib.Attributes;
using System;
using System.Linq;

namespace ModLib.Helper
{
    /// <summary>
    /// Helper for value manipulation and range operations.
    /// Provides utilities for range checking, clamping, rounding, and value comparisons.
    /// </summary>
    [ActionCat("Value")]
    public static class ValueHelper
    {
        /// <summary>
        /// Checks if value is within range (inclusive).
        /// </summary>
        /// <param name="x">Value</param>
        /// <param name="from">Min</param>
        /// <param name="to">Max</param>
        /// <returns>True if within range</returns>
        public static bool IsBetween(this double x, double from, double to)
        {
            return x >= from && x <= to;
        }

        /// <summary>
        /// Checks if value is within range (inclusive).
        /// </summary>
        /// <param name="x">Value</param>
        /// <param name="from">Min</param>
        /// <param name="to">Max</param>
        /// <returns>True if within range</returns>
        public static bool IsBetween(this long x, long from, long to)
        {
            return x >= from && x <= to;
        }

        /// <summary>
        /// Checks if value is within range (inclusive).
        /// </summary>
        /// <param name="x">Value</param>
        /// <param name="from">Min</param>
        /// <param name="to">Max</param>
        /// <returns>True if within range</returns>
        public static bool IsBetween(this int x, int from, int to)
        {
            return x >= from && x <= to;
        }

        /// <summary>
        /// Checks if value is within range (inclusive).
        /// </summary>
        /// <param name="x">Value</param>
        /// <param name="from">Min</param>
        /// <param name="to">Max</param>
        /// <returns>True if within range</returns>
        public static bool IsBetween(this float x, float from, float to)
        {
            return x >= from && x <= to;
        }

        /// <summary>
        /// Clamps value to range.
        /// </summary>
        /// <param name="x">Value</param>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <returns>Clamped value</returns>
        public static int FixValue(this int x, int min = int.MinValue, int max = int.MaxValue)
        {
            return Math.Max(Math.Min(x, max), min);
        }

        /// <summary>
        /// Clamps value to range.
        /// </summary>
        /// <param name="x">Value</param>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <returns>Clamped value</returns>
        public static long FixValue(this long x, long min = long.MinValue, long max = long.MaxValue)
        {
            return Math.Max(Math.Min(x, max), min);
        }

        /// <summary>
        /// Clamps value to range.
        /// </summary>
        /// <param name="x">Value</param>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <returns>Clamped value</returns>
        public static float FixValue(this float x, float min = float.MinValue, float max = float.MaxValue)
        {
            return Math.Max(Math.Min(x, max), min);
        }

        /// <summary>
        /// Clamps value to range.
        /// </summary>
        /// <param name="x">Value</param>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <returns>Clamped value</returns>
        public static double FixValue(this double x, double min = double.MinValue, double max = double.MaxValue)
        {
            return Math.Max(Math.Min(x, max), min);
        }

        /// <summary>
        /// Checks nullable value equality with tri-state result.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="x">Nullable value</param>
        /// <param name="compareValue">Compare value</param>
        /// <returns>-1 if null, 1 if equal, 0 if not equal</returns>
        public static int Is<T>(this T? x, T compareValue) where T : struct
        {
            if (!x.HasValue)
                return -1;
            if (x.Value.Equals(compareValue))
                return 1;
            return 0;
        }

        /// <summary>
        /// Checks if two doubles are nearly equal within epsilon.
        /// </summary>
        /// <param name="a">First value</param>
        /// <param name="b">Second value</param>
        /// <param name="epsilon">Tolerance</param>
        /// <returns>True if nearly equal</returns>
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

        /// <summary>
        /// Sums long values with int overflow protection.
        /// </summary>
        /// <param name="values">Values to sum</param>
        /// <returns>Sum clamped to int range</returns>
        public static int SafeSumInt(params long[] values)
        {
            return values.Sum().FixValue(int.MinValue, int.MaxValue).Parse<int>();
        }

        /// <summary>
        /// Sums long values without overflow protection.
        /// </summary>
        /// <param name="values">Values to sum</param>
        /// <returns>Sum</returns>
        public static long SumBigNum(params long[] values)
        {
            return values.Sum();
        }

        /// <summary>
        /// Sums double values with float overflow protection.
        /// </summary>
        /// <param name="values">Values to sum</param>
        /// <returns>Sum clamped to float range</returns>
        public static float SafeSumInt(params double[] values)
        {
            return values.Sum().FixValue(float.MinValue, float.MaxValue).Parse<float>();
        }

        /// <summary>
        /// Sums double values without overflow protection.
        /// </summary>
        /// <param name="values">Values to sum</param>
        /// <returns>Sum</returns>
        public static double SumBigNum(params double[] values)
        {
            return values.Sum();
        }
    }
}