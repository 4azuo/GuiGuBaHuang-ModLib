using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_MODIFY_EVENT)]
    public class BattleModifyEvent : ModBattleEvent
    {
        public override void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        {
            var pEnum = GetDmgPropertyEnum(GetDmgBasisType(e.hitData));
            var attackUnitData = e?.hitData?.attackUnit?.data?.TryCast<UnitDataHuman>();
            var hitUnitData = e?.hitUnit?.data?.TryCast<UnitDataHuman>();

            if (attackUnitData?.worldUnitData?.unit != null && pEnum != null)
            {
                var atk = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Attack);
                var addDmg = attackUnitData.worldUnitData.unit.GetProperty<int>(pEnum);
                e.dynV.baseValue += (atk * addDmg / 100f).Parse<int>();
            }

            if (hitUnitData?.worldUnitData?.unit != null && pEnum != null)
            {
                var def = hitUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Defense);
                var subDmg = hitUnitData.worldUnitData.unit.GetProperty<int>(pEnum);
                e.dynV.baseValue -= (def * subDmg / 100f).Parse<int>();
            }

            if (e.dynV.baseValue <= 0)
                e.dynV.baseValue = 1;
        }
    }
}
