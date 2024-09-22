using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [TraceIgnore]
    [Cache(ModConst.BATTLE_MODIFY_EVENT)]
    public class BattleModifyEvent : ModBattleEvent
    {
        public Dictionary<int, float> BlockRatio { get; set; } = new Dictionary<int, float>()
        {
            [1] = 0.40f,
            [2] = 0.42f,
            [3] = 0.44f,
            [4] = 0.47f,
            [5] = 0.50f,
            [6] = 0.53f,
            [7] = 0.56f,
            [8] = 0.60f,
            [9] = 0.65f,
            [10] = 0.70f,
        };

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
            var grade = hitUnitData?.worldUnitData?.unit?.GetGradeLvl() ?? 1;
            var atk = attackUnitData?.worldUnitData?.unit?.GetProperty<int>(UnitPropertyEnum.Attack) ?? attackUnitData?.attack.baseValue ?? e?.hitData?.attackUnit?.data?.attack.baseValue ?? 0;
            var def = hitUnitData?.worldUnitData?.unit?.GetProperty<int>(UnitPropertyEnum.Defense) ?? hitUnitData?.defense.baseValue ?? e?.hitUnit?.data?.defense.baseValue ?? 0;

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
            if (!e.hitData.isCrit)
            {
                if (attackUnitData?.worldUnitData?.unit != null)
                {
                    var basisThunder = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisThunder);
                    if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(10.00f, Math.Sqrt(basisThunder) / 3)))
                    {
                        e.hitData.isCrit = true;
                        var basisFire = attackUnitData.worldUnitData.unit.GetProperty<int>(UnitPropertyEnum.BasisFire);
                        e.dynV.baseValue = e.dynV.baseValue + (e.dynV.baseValue.Parse<float>() * (1.000f + basisFire.Parse<float>() / 1000)).Parse<int>();
                    }
                }
                //monster
                else
                {
                    if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, 8.00f))
                    {
                        e.hitData.isCrit = true;
                        e.dynV.baseValue *= 2;
                    }
                    else if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, 2.00f))
                    {
                        e.hitData.isCrit = true;
                        e.dynV.baseValue *= 3;
                    }
                }
            }

            //add dmg (basis)
            if (attackUnitData?.worldUnitData?.unit != null && pEnum != null)
            {
                var addDmg = attackUnitData.worldUnitData.unit.GetProperty<int>(pEnum);
                e.dynV.baseValue += atk * addDmg / 200;
            }

            //add dmg (sp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.sp > 0)
            {
                var r = (attackUnitData.sp.Parse<float>() / attackUnitData.maxSP.value.Parse<float>()) / 10;
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //add dmg (dp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.dp > 0)
            {
                var r = (attackUnitData.dp.Parse<float>() / attackUnitData.maxDP.value.Parse<float>()) / 10;
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //add dmg (mp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.mp > 0 && IsBasisMagic(dType))
            {
                var r = (attackUnitData.mp.Parse<float>() / attackUnitData.maxMP.value.Parse<float>()) / 10;
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //block dmg (basis)
            if (hitUnitData?.worldUnitData?.unit != null && pEnum != null && e.dynV.baseValue > 0)
            {
                var subDmg = hitUnitData.worldUnitData.unit.GetProperty<int>(pEnum);
                e.dynV.baseValue -= def * subDmg / 100;
            }

            //block dmg (sp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.sp > 0 && e.dynV.baseValue > 0 && grade >= 4)
            {
                var r = (hitUnitData.sp.Parse<float>() / hitUnitData.maxSP.value.Parse<float>()) * BlockRatio[grade];
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //block dmg (dp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.dp > 0 && e.dynV.baseValue > 0 && grade >= 8)
            {
                var r = (hitUnitData.dp.Parse<float>() / hitUnitData.maxDP.value.Parse<float>()) * BlockRatio[grade];
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //block dmg (mp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.mp > 0 && e.dynV.baseValue > 0)
            {
                var blockTimes = CommonTool.Random(1, grade);
                for (int i = 0; i < blockTimes && hitUnitData.mp > 0 && e.dynV.baseValue > 0; i++)
                {
                    var r = (hitUnitData.mp.Parse<float>() / hitUnitData.maxMP.value.Parse<float>()) * BlockRatio[grade];
                    var blockedDmg = (def * r).Parse<int>();
                    var lostMp = Math.Max(grade, blockedDmg / (100 * grade));
                    e.dynV.baseValue -= blockedDmg;
                    hitUnitData.AddMP(-lostMp);
                }
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
