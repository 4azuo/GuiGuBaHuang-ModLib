using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.NPC_AUTO_CUL_EVENT)]
    public class NpcAutoCulEvent : ModEvent
    {
        public override void OnMonthly()
        {
            base.OnMonthly();
            foreach (var wunit in g.world.unit.GetUnits())
            {
                if (!wunit.IsPlayer())
                {
                    var exp = CommonTool.Random(0.00f, 100.00f) * wunit.GetDynProperty(UnitDynPropertyEnum.GradeID).value;
                    if (wunit.IsHero())
                        exp *= 1.80f;
                    wunit.AddExp(exp.Parse<int>());
                }
            }
        }
    }
}
