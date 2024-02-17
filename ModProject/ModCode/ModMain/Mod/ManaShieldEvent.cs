using EBattleTypeData;
using Harmony;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;

namespace MOD_nE7UL2.Mod
{
    //TESTING
    //[Cache(ModConst.MANA_SHIELD_EVENT)]
    //public sealed class ManaShieldEvent : ModEvent
    //{
    //    public override void OnBattleSetUnitType(SetUnitType e)
    //    {
    //        var humanData = e?.unit?.data?.TryCast<UnitDataHuman>();
    //        if (humanData != null)
    //        {
    //            humanData.AddHP(new MartialTool.HitData(humanData.unit, null, 0, 0, /*humanData.worldUnitData.unit.GetDynProperty(UnitDynPropertyEnum.Mp).value / 10*/9999)
    //            {
    //                isShieldDef = true,
    //            });
    //            DebugHelper.WriteLine($"TEST");
    //        }
    //        DebugHelper.Save();
    //    }
    //}
}
