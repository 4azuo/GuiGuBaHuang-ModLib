using System;
using ModLib.Helper;

/// <summary>
/// Item helpers of Taoist.
/// </summary>
public static class ItemHelper
{
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