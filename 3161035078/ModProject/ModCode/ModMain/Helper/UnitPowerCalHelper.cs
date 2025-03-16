using System.Linq;
using System.Collections.Generic;
using ModLib.Enum;
using System;
using MOD_nE7UL2.Mod;

public static class UnitPowerCalHelper
{
    public static double CalWUnitBattlePower(WorldUnitBase wunit)
    {
        if (wunit == null)
            return 0;
        return
            (wunit.GetDynProperty(UnitDynPropertyEnum.Attack).value * 3) +
            (wunit.GetDynProperty(UnitDynPropertyEnum.Defense).value * 10) +
            (wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value) +
            (wunit.GetDynProperty(UnitDynPropertyEnum.MpMax).value * 3) +
            (wunit.GetDynProperty(UnitDynPropertyEnum.SpMax).value * 5) +
            (wunit.GetEquippedArtifacts().Select(a => Math.Pow(2, a.propsInfoBase.grade) * 10000 + Math.Pow(2, a.propsInfoBase.level) * 2000 + CustomRefineEvent.GetRefineLvl(a) * 100).Sum());
    }

    public static double CalWUnitBattlePower(List<WorldUnitBase> wunits)
    {
        if (wunits == null || wunits.Count == 0)
            return 0;
        return wunits.Average(x => CalWUnitBattlePower(x));
    }

    public static double CalMonstBattlePower(int gameLvl, int areaId)
    {
        return Math.Pow(3, areaId) * 10000 + Math.Pow(3, gameLvl) * 3000;
    }
}