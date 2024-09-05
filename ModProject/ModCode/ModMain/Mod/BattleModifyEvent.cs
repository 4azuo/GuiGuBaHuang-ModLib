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
                if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(20.00f, Math.Sqrt(basisWind))))
                {
                    e.hitData.isEvade = true;
                    e.dynV.baseValue = 0;
                    return;
                }
            }

            //critical
            if (attackUnitData?.worldUnitData?.unit != null && !e.hitData.isCrit)
            {
                var basisFire = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisFire);
                if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(10.00f, Math.Sqrt(basisFire))))
                {
                    e.hitData.isCrit = true;
                    e.dynV.baseValue = e.dynV.baseValue + (e.dynV.baseValue.Parse<float>() * (1.000f + basisFire.Parse<float>() / 1000)).Parse<int>();
                }
            }

            //add dmg (basis)
            if (attackUnitData?.worldUnitData?.unit != null && pEnum != null)
            {
                var atk = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Attack);
                var addDmg = attackUnitData.worldUnitData.unit.GetProperty<int>(pEnum);
                e.dynV.baseValue += atk * addDmg / 100;
            }

            //add dmg (sp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.sp > 0)
            {
                var atk = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Attack);
                var r = (attackUnitData.sp.Parse<float>() / attackUnitData.maxSP.value.Parse<float>()) / 10;
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //add dmg (dp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.dp > 0)
            {
                var atk = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Attack);
                var r = (attackUnitData.dp.Parse<float>() / attackUnitData.maxDP.value.Parse<float>()) / 10;
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //add dmg (mp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.mp > 0 && IsBasisMagic(dType))
            {
                var atk = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Attack);
                var r = (attackUnitData.mp.Parse<float>() / attackUnitData.maxMP.value.Parse<float>()) / 10;
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //block dmg (sp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.sp > 0 && e.dynV.baseValue > 0)
            {
                var def = hitUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Defense);
                var r = (hitUnitData.sp.Parse<float>() / hitUnitData.maxSP.value.Parse<float>()) / 2;
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //block dmg (dp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.dp > 0 && e.dynV.baseValue > 0)
            {
                var def = hitUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Defense);
                var r = (hitUnitData.dp.Parse<float>() / hitUnitData.maxDP.value.Parse<float>()) / 2;
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //block dmg (basis)
            if (hitUnitData?.worldUnitData?.unit != null && pEnum != null && e.dynV.baseValue > 0)
            {
                var def = hitUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Defense);
                var subDmg = hitUnitData.worldUnitData.unit.GetProperty<int>(pEnum);
                e.dynV.baseValue -= def * subDmg / 100;
            }

            //block dmg (mp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.mp > 0 && e.dynV.baseValue > 0)
            {
                var def = hitUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Defense);
                var r = (hitUnitData.mp.Parse<float>() / hitUnitData.maxMP.value.Parse<float>()) / 2;
                e.dynV.baseValue -= (def * r).Parse<int>();
                hitUnitData.AddMP(-1);
            }

            if (e.dynV.baseValue <= 0)
                e.dynV.baseValue = 1;
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
