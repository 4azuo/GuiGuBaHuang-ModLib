using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.QI_CUL_EVENT)]
    public class QiCulEvent : ModEvent
    {
        public IDictionary<string, long> Qi { get; set; } = new Dictionary<string, long>();

        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);
            if (wunit.IsFullExp())
            {
                var unitId = wunit.GetUnitId();

                //inc qi
                if (!Qi.ContainsKey(unitId))
                    Qi.Add(unitId, 0);
                Qi[unitId] += (wunit.GetDynProperty(UnitDynPropertyEnum.Mp).value * CommonTool.Random(0.8f, 1.2f)).Parse<int>();
            }
        }
    }
}
