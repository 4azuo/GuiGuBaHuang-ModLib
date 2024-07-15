using Boo.Lang;
using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Linq;
using UnityEngine;
using static DataMap;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_MODIFY_EVENT)]
    public class BattleModifyEvent : ModBattleEvent
    {
        private List<UnitCtrlBase> dungeonUnits = new List<UnitCtrlBase>();

        public override void OnBattleEnd(BattleEnd e)
        {
            base.OnBattleEnd(e);
            dungeonUnits.Clear();
        }

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            dungeonUnits.Add(e);

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
                var basisFire = hitUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisFire);
                if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Sqrt(basisFire)))
                {
                    e.hitData.isCrit = true;
                }
            }

            var pEnum = GetDmgPropertyEnum(GetDmgBasisType(e.hitData));

            //add dmg
            if (attackUnitData?.worldUnitData?.unit != null && pEnum != null)
            {
                var atk = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Attack);
                var addDmg = attackUnitData.worldUnitData.unit.GetProperty<int>(pEnum);
                e.dynV.baseValue += atk * addDmg / 100;
            }

            //block dmg
            if (hitUnitData?.worldUnitData?.unit != null && pEnum != null)
            {
                var def = hitUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.Defense);
                var subDmg = hitUnitData.worldUnitData.unit.GetProperty<int>(pEnum);
                e.dynV.baseValue -= def * subDmg / 100;
            }

            if (e.dynV.baseValue <= 0)
                e.dynV.baseValue = 1;
        }

        [EventCondition(IsInBattle = true)]
        public override void OnTimeUpdate()
        {
            foreach (var unit in dungeonUnits)
            {
                if (unit.isDie)
                    continue;

                var monstData = unit?.data?.TryCast<UnitDataMonst>();
                if (monstData != null)
                {
                    if (monstData.hp < monstData.maxHP.baseValue)
                        monstData.hp += Math.Sqrt(Math.Sqrt(monstData.maxHP.value)).Parse<int>();
                }

                var humanData = unit?.data?.TryCast<UnitDataHuman>();
                if (humanData?.worldUnitData?.unit != null)
                {
                    if (humanData.hp < humanData.maxHP.baseValue)
                        humanData.hp += Math.Sqrt(humanData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisWood) / 100).Parse<int>();
                    if (humanData.mp < humanData.maxMP.baseValue)
                        humanData.mp += Math.Sqrt(humanData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisFroze) / 100).Parse<int>();
                }
            }
        }
    }
}
