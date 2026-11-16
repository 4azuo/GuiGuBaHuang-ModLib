using System;
using ModLib.Helper;

/// <summary>
/// Item helpers of Taoist.
/// </summary>
public static class ItemHelper
{
    private static readonly System.Collections.Generic.Dictionary<int, int> gradeCache = new System.Collections.Generic.Dictionary<int, int>();

    /// <summary>Grade (realm) of an item config (from its props info, cached).</summary>
    public static int Grade(this ConfItemPropsItem conf)
    {
        int grade;
        if (gradeCache.TryGetValue(conf.id, out grade))
            return grade;
        try { grade = DataProps.PropsData.NewPropsNotData(conf.id)?.propsInfoBase?.grade ?? 1; }
        catch { grade = 1; }
        gradeCache[conf.id] = grade;
        return grade;
    }

    /// <summary>
    /// Worth of an item stack.
    /// </summary>
    public static double GetItemValue(this DataProps.PropsData prop)
    {
        return ((prop.propsInfoBase?.grade ?? 0) * 10) + Math.Pow(1.7, (prop.propsInfoBase?.level ?? 0));
    }

    /// <summary>
    /// True when the unit could rob the item (grade/level conditions).
    /// </summary>
    public static bool CheckItemCouldBeRobbed(this WorldUnitBase wunit, DataProps.PropsData prop)
    {
        return GetItemValue(prop) > wunit.GetGradeLvl() * 10;
    }
}