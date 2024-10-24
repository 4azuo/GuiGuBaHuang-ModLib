﻿using EBattleTypeData;
using Il2CppSystem.Xml.Serialization;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_MODIFY_EVENT)]
    public class BattleModifyEvent : ModEvent
    {
        public override int OrderIndex => 9000;

        public static Dictionary<int, float> BlockRatio { get; } = new Dictionary<int, float>()
        {
            [1] = 0.50f,
            [2] = 0.60f,
            [3] = 0.70f,
            [4] = 0.80f,
            [5] = 1.00f,
            [6] = 1.20f,
            [7] = 1.40f,
            [8] = 1.60f,
            [9] = 2.00f,
            [10] = 2.50f,
        };

        public static float GetBlockRatio(UnitDataHuman u)
        {
            int gradeLvl = 0;
            if (u == null || !BlockRatio.ContainsKey(gradeLvl = u?.worldUnitData?.unit?.GetGradeLvl() ?? 0))
                return 0f;
            var wunit = u.worldUnitData.unit;
            return BlockRatio[gradeLvl] + wunit.GetBasisMagicSum() / 10000.0f;
        }

        public static int GetBlockMpCost(UnitDataHuman u)
        {
            int gradeLvl = 0;
            if (u == null || !BlockRatio.ContainsKey(gradeLvl = u?.worldUnitData?.unit?.GetGradeLvl() ?? 0))
                return 0;
            var wunit = u.worldUnitData.unit;
            return 50 * gradeLvl + wunit.GetBasisPhysicSum() / 1000;
        }

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            base.OnIntoBattleFirst(e);

            var humanData = e.data.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                //humanData.attack.baseValue += (??? / 100.00f * humanData.attack.baseValue).Parse<int>();
                var adjustDef1 = ((((humanData.basisFist.value + humanData.basisPalm.value + humanData.basisFinger.value) / 2.0f) / 1000.00f) * humanData.defense.baseValue).Parse<int>();
                var adjustDef2 = ((humanData.basisEarth.value / 1000.00f) * humanData.defense.baseValue).Parse<int>();
                humanData.defense.baseValue += adjustDef1 + adjustDef2;
                var adjustMs = (humanData.basisWind.value / 100.00f).Parse<int>();
                humanData.moveSpeed.baseValue += adjustMs;
            }
        }

        public override void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        {
            base.OnBattleUnitHitDynIntHandler(e);

            var attackUnitData = ModBattleEvent.AttackingUnit.data.TryCast<UnitDataHuman>();
            var hitUnitData = ModBattleEvent.HitUnit.data.TryCast<UnitDataHuman>();

            var atkGradeLvl = attackUnitData?.worldUnitData?.unit?.GetGradeLvl() ?? 1;
            var dType = ModBattleEvent.GetDmgBasisType(e.hitData);
            var pEnum = ModBattleEvent.GetDmgPropertyEnum(dType);
            var atk = ModBattleEvent.AttackingUnit.data.attack.baseValue;

            attackUnitData?.AddMP(-GetAdjMpCost(e));

            //evasion
            var evaRate = Math.Sqrt(ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.HitUnit, UnitDynPropertyEnum.BasisWind) / 18);
            if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(12.00f, evaRate) + GetStepEvade(hitUnitData)))
            {
                e.hitData.isEvade = true;
                e.dynV.baseValue = 0;
                return;
            }

            //add dmg (skill)
            e.dynV.baseValue += GetAdjSkillDmg(e);

            //add dmg (basis)
            if (pEnum != null)
            {
                var r = ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.AttackingUnit, pEnum);
                e.dynV.baseValue += atk * r / 1000;
            }

            //add dmg (sp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.sp > 0)
            {
                var r = (attackUnitData.sp.Parse<float>() / attackUnitData.maxSP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //add dmg (dp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.dp > 0 && atkGradeLvl >= 4)
            {
                var r = (attackUnitData.dp.Parse<float>() / attackUnitData.maxDP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //add dmg (mp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.mp > 0 && atkGradeLvl >= 8)
            {
                var r = (attackUnitData.mp.Parse<float>() / attackUnitData.maxMP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //critical
            if (!e.hitData.isCrit)
            {
                if (attackUnitData?.worldUnitData?.unit != null)
                {
                    if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(8.00f, Math.Sqrt(attackUnitData.basisThunder.value / 50))))
                    {
                        e.hitData.isCrit = true;
                        e.dynV.baseValue += (e.dynV.baseValue.Parse<float>() * (1.000f + attackUnitData.basisFire.value / 1000.00f)).Parse<int>();
                    }
                }
                //monster
                else
                {
                    var criticalRate = CommonTool.Random(0.00f, 100.00f);
                    if (ValueHelper.IsBetween(criticalRate, 0.00f, 0.40f))
                    {
                        e.hitData.isCrit = true;
                        e.dynV.baseValue *= 4;
                    }
                    else if (ValueHelper.IsBetween(criticalRate, 0.00f, 2.00f))
                    {
                        e.hitData.isCrit = true;
                        e.dynV.baseValue *= 3;
                    }
                    else if (ValueHelper.IsBetween(criticalRate, 0.00f, 8.00f))
                    {
                        e.hitData.isCrit = true;
                        e.dynV.baseValue *= 2;
                    }
                }
            }

            var defGradeLvl = hitUnitData?.worldUnitData?.unit?.GetGradeLvl() ?? 1;
            var def = ModBattleEvent.HitUnit.data.defense.baseValue;
            var minDmg = ModBattleEvent.AttackingUnit.data.grade.value;
            var blockratio = GetBlockRatio(hitUnitData);

            //block
            var blockRate = Math.Sqrt(ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.HitUnit, pEnum) / 12);
            if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(18.00f, blockRate)))
            {
                var r = ModBattleEvent.HitUnit.data.hp.Parse<float>() / ModBattleEvent.HitUnit.data.maxHP.value.Parse<float>();
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //block dmg (basis)
            if (pEnum != null && e.dynV.baseValue > minDmg)
            {
                var r = ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.HitUnit, pEnum);
                e.dynV.baseValue -= def * r / 200;
            }

            //block dmg (sp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.sp > 0 && e.dynV.baseValue > minDmg && defGradeLvl >= 4)
            {
                var r = (hitUnitData.sp.Parse<float>() / hitUnitData.maxSP.value.Parse<float>()) * blockratio;
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //block dmg (dp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.dp > 0 && e.dynV.baseValue > minDmg && defGradeLvl >= 8)
            {
                var r = (hitUnitData.dp.Parse<float>() / hitUnitData.maxDP.value.Parse<float>()) * blockratio;
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //block dmg (mp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.mp > 0 && e.dynV.baseValue > minDmg)
            {
                var blockmpcost = GetBlockMpCost(hitUnitData);
                var blockTimes = CommonTool.Random(defGradeLvl / 2, defGradeLvl);
                for (int i = 0; i < blockTimes && hitUnitData.mp >= defGradeLvl && e.dynV.baseValue > minDmg; i++)
                {
                    var r = (hitUnitData.mp.Parse<float>() / hitUnitData.maxMP.value.Parse<float>()) * blockratio;
                    var blockedDmg = (def * r).Parse<int>();
                    var lostMp = Math.Max(defGradeLvl, blockedDmg / blockmpcost);
                    e.dynV.baseValue -= blockedDmg;
                    hitUnitData.AddMP(-lostMp);
                }
            }

            //min-dmg
            if (e.dynV.baseValue <= minDmg)
                e.dynV.baseValue = minDmg;

            //stronger every hit
            //if (e.dynV.baseValue < ModBattleEvent.HitUnit.data.maxHP.value / 50)
            //    ModBattleEvent.AttackingUnit.data.attack.baseValue += (ModBattleEvent.AttackingUnit.data.attack.baseValue * (attackUnitData?.worldUnitData?.unit != null ? 0.0004f : 0.01f)).Parse<int>();
        }

        private int GetAdjSkillDmg(UnitHitDynIntHandler e)
        {
            var skill = e?.hitData?.skillBase?.TryCast<SkillAttack>();
            if (skill != null)
            {
                DebugHelper
                var soleId = skill.skillData.data.soleID;
                var grade = skill.skillData.data.propsInfoBase.grade;
                var level = skill.skillData.data.propsInfoBase.level;
                var mType = skill.skillData.martialType;
                return ExpertEvent.GetSkillExpertAtk(e.dynV.baseValue, ExpertEvent.GetExpertLvl(soleId, grade, level), grade, level, mType);
            }
            return 0;
        }

        private int GetAdjMpCost(UnitHitDynIntHandler e)
        {
            var skill = e?.hitData?.skillBase?.TryCast<SkillAttack>();
            if (skill != null)
            {
                DebugHelper
                var soleId = skill.skillData.data.soleID;
                var grade = skill.skillData.data.propsInfoBase.grade;
                var level = skill.skillData.data.propsInfoBase.level;
                return ExpertEvent.GetSkillExpertMpCost(SkillHelper.GetSkillMpCost(skill.skillData.martialInfo.martialData), ExpertEvent.GetExpertLvl(soleId, grade, level), grade, level);
            }
            return 0;
        }

        private float GetStepEvade(UnitDataHuman humanData)
        {
            if (humanData == null)
                return 0;
            var stepData = humanData?.worldUnitData?.unit?.GetMartialStep();
            if (stepData != null)
            {
                var stepId = stepData.data.soleID;
                var stepGrade = stepData.data.propsInfoBase.grade;
                var stepLevel = stepData.data.propsInfoBase.level;
                return ExpertEvent.GetStepExpertEvade(ExpertEvent.GetExpertLvl(stepId, stepGrade, stepLevel), stepGrade, stepLevel);
            }
            return 0;
        }

        [EventCondition(IsInBattle = true)]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();

            foreach (var unit in ModBattleEvent.DungeonUnits)
            {
                if (unit.isDie)
                    continue;

                //monster recovery
                var monstData = unit?.data?.TryCast<UnitDataMonst>();
                if (monstData != null && monstData.grade.value >= 3)
                {
                    if (monstData.hp < monstData.maxHP.value)
                        monstData.hp += (Math.Sqrt(Math.Sqrt(monstData.maxHP.value / 3)) + Math.Sqrt(monstData.basisWood.value / 100)).Parse<int>();
                }

                //human recovery
                var humanData = unit?.data?.TryCast<UnitDataHuman>();
                if (humanData?.worldUnitData?.unit != null)
                {
                    if (humanData.hp < humanData.maxHP.value)
                        humanData.hp += Math.Sqrt(humanData.basisWood.value / 200).Parse<int>();
                    if (humanData.mp < humanData.maxMP.value)
                        humanData.mp += Math.Sqrt(humanData.basisFroze.value / 100).Parse<int>();
                }
            }
        }
    }
}
