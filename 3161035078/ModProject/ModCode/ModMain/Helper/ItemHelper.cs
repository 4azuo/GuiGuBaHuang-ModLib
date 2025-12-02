using System;
using ModLib.Helper;

public static class ItemHelper
{
    public static double GetItemValue(this DataProps.PropsData prop)
    {
        return ((prop.propsInfoBase?.grade ?? 0) * 10) + Math.Pow(1.7, (prop.propsInfoBase?.level ?? 0));
    }

    public static bool CheckItemCouldBeRobbed(this WorldUnitBase wunit, DataProps.PropsData prop)
    {
        return GetItemValue(prop) > wunit.GetGradeLvl() * 10;
    }
}