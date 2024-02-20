using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.NPC_AUTO_CUL_EVENT)]
    public class NpcAutoCulEvent : ModEvent
    {
        public override void OnMonthly()
        {
            foreach (var wunit in g.world.unit.GetUnits())
            {
                if (!wunit.IsPlayer())
                {
                    var maxExp = wunit.GetMaxExpCurrentGrade();
                    var curExp = wunit.GetProperty<int>(UnitPropertyEnum.Exp);
                    var exp = 200.00f * wunit.GetProperty<int>(UnitPropertyEnum.GradeID);
                    if (wunit.IsHero())
                    {
                        exp *= 1.80f;
                    }
                    wunit.SetProperty<int>(UnitPropertyEnum.Exp, Math.Min(curExp + exp.Parse<int>(), maxExp));
                }
            }
        }
    }
}
