using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.NPC_AUTO_CUL_EVENT)]
    public class NpcAutoCulEvent : ModEvent
    {
        public override void OnMonthlyForEachWUnit(WorldUnitBase wunit)
        {
            base.OnMonthlyForEachWUnit(wunit);

            if (!wunit.IsPlayer())
            {
                var exp = CommonTool.Random(10.00f, 200.00f) * wunit.GetGradeLvl() * wunit.GetDynProperty(UnitDynPropertyEnum.Talent).value / 100f;
                if (wunit.IsHero())
                    exp *= CommonTool.Random(0.50f, 2.00f);
                wunit.AddExp(exp.Parse<int>());
            }
        }
    }
}
