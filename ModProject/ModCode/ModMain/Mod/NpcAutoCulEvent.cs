using MOD_nE7UL2.Const;
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
                var exp = CommonTool.Random(0.00f, 100.00f) * wunit.GetGradeLvl();
                if (wunit.IsHero())
                    exp *= 1.80f;
                wunit.AddExp(exp.Parse<int>());
            }
        }
    }
}
