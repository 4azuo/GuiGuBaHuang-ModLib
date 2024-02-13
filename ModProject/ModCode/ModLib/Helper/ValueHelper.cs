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
}
