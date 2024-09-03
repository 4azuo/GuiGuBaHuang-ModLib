using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_MODIFY_EVENT)]
    public class BattleModifyEvent : ModBattleEvent
    {

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            base.OnIntoBattleFirst(e);

            var humanData = e?.data?.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                var artifact = humanData.worldUnitData.unit.data.unitData.propData.GetEquipProps().ToArray().FirstOrDefault(x => x.propsItem.IsArtifact() != null);
                if (artifact != null)
                {
                    var artifactInfo = artifact.propsItem.IsArtifact();
                    if (artifactInfo.durable > 0)
                    {
                        humanData.attack.baseValue += Math.Sqrt(artifactInfo.atk).Parse<int>();
                    }
                }
                humanData.defense.baseValue += humanData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisEarth) / 100;
            }
        }

        public override void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        {
            var attackUnitData = e?.hitData?.attackUnit?.data?.TryCast<UnitDataHuman>();
            var hitUnitData = e?.hitUnit?.data?.TryCast<UnitDataHuman>();
            var dType = GetDmgBasisType(e.hitData);
            var pEnum = GetDmgPropertyEnum(dType);

            //evasion
            if (hitUnitData?.worldUnitData?.unit != null)
            {
                var basisWind = hitUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisWind);
                if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Sqrt(basisWind)))
                {
                    e.hitData.isEvade = true;
                    return;
                }
            }

            //critical
            if (attackUnitData?.worldUnitData?.unit != null && !e.hitData.isCrit)
            {
                var basisFire = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisFire);
                if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Sqrt(basisFire)))
                {
                    e.hitData.isCrit = true;
                }
            }

            //add dmg (mp)
            if (attackUnitData?.worldUnitData?.unit != null && IsBasisMagic(dType))
            {
                var atk = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Attack);
                var r = (attackUnitData.mp.Parse<float>() / attackUnitData.maxMP.value.Parse<float>()) / 10;
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //add dmg (basis)
            if (attackUnitData?.worldUnitData?.unit != null && pEnum != null)
            {
                var atk = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Attack);
                var addDmg = attackUnitData.worldUnitData.unit.GetProperty<int>(pEnum);
                e.dynV.baseValue += atk * addDmg / 100;
            }

            //block dmg (mp)
            if (hitUnitData?.worldUnitData?.unit != null)
            {
                var def = hitUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Defense);
                var r = hitUnitData.mp.Parse<float>() / hitUnitData.maxMP.value.Parse<float>();
                e.dynV.baseValue -= (def * r).Parse<int>();
                hitUnitData.AddMP(-1);
            }

            //block dmg (basis)
            if (hitUnitData?.worldUnitData?.unit != null && pEnum != null)
            {
                var def = hitUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Defense);
                var subDmg = hitUnitData.worldUnitData.unit.GetProperty<int>(pEnum);
                e.dynV.baseValue -= def * subDmg / 100;
            }
        }

        [EventCondition(IsInBattle = true)]
        public override void OnTimeUpdate()
        {
            foreach (var unit in DungeonUnits)
            {
                if (unit.isDie)
                    continue;

                var monstData = unit?.data?.TryCast<UnitDataMonst>();
                if (monstData != null && monstData.grade.value >= 3)
                {
                    if (monstData.hp < monstData.maxHP.value)
                        monstData.hp += Math.Sqrt(Math.Sqrt(monstData.maxHP.value / 3)).Parse<int>();
                }

                var humanData = unit?.data?.TryCast<UnitDataHuman>();
                if (humanData?.worldUnitData?.unit != null)
                {
                    if (humanData.hp < humanData.maxHP.value)
                        humanData.hp += Math.Sqrt(humanData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisWood) / 100).Parse<int>();
                    if (humanData.mp < humanData.maxMP.value)
                        humanData.mp += Math.Sqrt(humanData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisFroze) / 100).Parse<int>();
                }
            }
        }
    }
}
