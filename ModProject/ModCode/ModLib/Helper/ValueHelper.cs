using ExcelDataReader.Core;
using System;
using System.Data.Common.CommandTrees.ExpressionBuilder;

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

    public static int FixValue(this int x, int min, int max)
    {
        return Math.Max(Math.Min(x, max), min);
    }

    public static float FixValue(this float x, float min, float max)
    {
        return Math.Max(Math.Min(x, max), min);
    }
}
