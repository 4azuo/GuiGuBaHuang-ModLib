using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
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

        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);

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

            //evasion
            var evaRate = GetEvadeBase(ModBattleEvent.HitWorldUnit);
            if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(GetEvadeMaxBase() + GetEvadeMaxPlus(ModBattleEvent.HitWorldUnit), evaRate) + GetEvadePlus(ModBattleEvent.HitWorldUnit)))
            {
                //e.hitData.isEvade = true;
                e.dynV.baseValue = 0;
                return;
            }

            //add dmg (skill)
            e.dynV.baseValue += GetAdjSkillDmg(ModBattleEvent.AttackingWorldUnit);

            //add dmg (basis)
            if (pEnum != null)
            {
                var r = ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.AttackingUnit, pEnum).Parse<float>();
                e.dynV.baseValue += (atk * r / 1000f).Parse<int>();
            }

            //add dmg (mp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.mp > 0)
            {
                var r = (attackUnitData.mp.Parse<float>() / attackUnitData.maxMP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
                hitUnitData.AddMP(-(atkGradeLvl / 5));
            }

            //add dmg (sp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.sp > 0 && atkGradeLvl >= 4)
            {
                var r = (attackUnitData.sp.Parse<float>() / attackUnitData.maxSP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //add dmg (dp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.dp > 0 && atkGradeLvl >= 8)
            {
                var r = (attackUnitData.dp.Parse<float>() / attackUnitData.maxDP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //critical
            if (!e.hitData.isCrit)
            {
                if (ModBattleEvent.IsWorldUnitAttacking)
                {
                    var scritRate = GetSCritChanceBase(ModBattleEvent.AttackingWorldUnit);
                    if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(GetSCritChanceMaxBase() + GetSCritChanceMaxPlus(ModBattleEvent.AttackingWorldUnit), scritRate) + GetSCritChancePlus(ModBattleEvent.AttackingWorldUnit)))
                    {
                        e.hitData.isCrit = true;
                        e.dynV.baseValue += (e.dynV.baseValue.Parse<float>() * (GetSCritDamageBase(ModBattleEvent.AttackingWorldUnit) + GetSCritDamagePlus(ModBattleEvent.AttackingWorldUnit))).Parse<int>();
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
            if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(GetBlockMaxBase() + GetBlockMaxPlus(ModBattleEvent.HitWorldUnit), blockRate)))
            {
                e.dynV.baseValue -= GetBlockDmgBase(ModBattleEvent.HitWorldUnit) + GetBlockDmgPlus(ModBattleEvent.HitWorldUnit);
            }

            //block dmg (basis)
            if (pEnum != null && e.dynV.baseValue > minDmg)
            {
                var r = ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.HitUnit, pEnum).Parse<float>();
                e.dynV.baseValue -= (def * r / 200f).Parse<int>();
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

            //block dmg (sp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.sp > 0 && e.dynV.baseValue > minDmg && defGradeLvl >= 4)
            {
                var r = (hitUnitData.sp.Parse<float>() / hitUnitData.maxSP.value.Parse<float>()) * blockratio;
                e.dynV.baseValue -= (def * r).Parse<int>();
                hitUnitData.AddSP(-1);
            }

            //block dmg (dp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.dp > 0 && e.dynV.baseValue > minDmg && defGradeLvl >= 8)
            {
                var r = (hitUnitData.dp.Parse<float>() / hitUnitData.maxDP.value.Parse<float>()) * blockratio;
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //min-dmg
            if (e.dynV.baseValue <= minDmg)
                e.dynV.baseValue = minDmg;

            //stronger every hit
            //if (e.dynV.baseValue < ModBattleEvent.HitUnit.data.maxHP.value / 50)
            //    ModBattleEvent.AttackingUnit.data.attack.baseValue += (ModBattleEvent.AttackingUnit.data.attack.baseValue * (attackUnitData?.worldUnitData?.unit != null ? 0.0004f : 0.01f)).Parse<int>();
        }

        private static IDictionary<string, SkillAttack> _castingSkill = new Dictionary<string, SkillAttack>();
        public override void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            base.OnBattleUnitUseSkill(e);
            var humanData = e.unit.data.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                var unitId = humanData.worldUnitData.unit.GetUnitId();
                _castingSkill[unitId] = e?.skill?.TryCast<SkillAttack>();

                humanData?.AddMP(-GetAdjMpCost(humanData.worldUnitData.unit));
            }
        }

        public static int GetAdjSkillDmg(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var skill = _castingSkill[wunit.GetUnitId()];
            if (skill == null)
                return 0;
            var soleId = skill.skillData.data.soleID;
            var grade = skill.skillData.data.propsInfoBase.grade;
            var level = skill.skillData.data.propsInfoBase.level;
            var mType = skill.skillData.martialType;
            return UnitModifyHelper.GetSkillExpertAtk(wunit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, ExpertEvent.GetExpertLvl(soleId, grade, level), grade, level, mType);
        }

        public static int GetAdjMpCost(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var skill = _castingSkill[wunit.GetUnitId()];
            if (skill == null)
                return 0;
            var soleId = skill.skillData.data.soleID;
            var grade = skill.skillData.data.propsInfoBase.grade;
            var level = skill.skillData.data.propsInfoBase.level;
            var expertLvl = ExpertEvent.GetExpertLvl(soleId, grade, level);
            return UnitModifyHelper.GetSkillExpertMpCost(SkillHelper.GetSkillMpCost(skill.skillData.data), expertLvl, grade, level, wunit.GetGradeLvl() * expertLvl / 10);
        }

        public static double GetBlockMaxBase()
        {
            return 18.00f;
        }

        public static double GetBlockMaxPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var plusValue = 0f;
            //adj
            //...
            return plusValue;
        }

        public static int GetBlockDmgBase(WorldUnitBase wunit)
        {
            var r = wunit.GetDynProperty(UnitDynPropertyEnum.Hp).value.Parse<float>() / wunit.GetDynProperty(UnitDynPropertyEnum.HpMax).value.Parse<float>();
            return (wunit.GetDynProperty(UnitDynPropertyEnum.Defense).value * r).Parse<int>();
        }

        public static int GetBlockDmgPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var plusValue = 0;
            //adj
            //...
            return plusValue;
        }

        public static double GetEvadeMaxBase()
        {
            return 12.00f;
        }

        public static double GetEvadeMaxPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var plusValue = 0f;
            //adj
            //...
            return plusValue;
        }

        public static double GetEvadeBase(WorldUnitBase wunit)
        {
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisWind).value / 18);
        }

        public static float GetEvadePlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var stepData = wunit.GetMartialStep();
            if (stepData == null)
                return 0;
            var plusValue = 0f;
            //step
            var stepId = stepData.data.soleID;
            var stepGrade = stepData.data.propsInfoBase.grade;
            var stepLevel = stepData.data.propsInfoBase.level;
            plusValue += UnitModifyHelper.GetStepExpertEvade(ExpertEvent.GetExpertLvl(stepId, stepGrade, stepLevel), stepGrade, stepLevel);
            //adj
            //...
            return plusValue;
        }

        public static double GetSCritChanceMaxBase()
        {
            return 8.00f;
        }

        public static double GetSCritChanceMaxPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var plusValue = 0f;
            //adj
            //...
            return plusValue;
        }

        public static double GetSCritChanceBase(WorldUnitBase wunit)
        {
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisThunder).value / 50);
        }

        public static double GetSCritChancePlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var plusValue = 0f;
            //adj
            //...
            return plusValue;
        }

        public static float GetSCritDamageBase(WorldUnitBase wunit)
        {
            return 2.000f + wunit.GetDynProperty(UnitDynPropertyEnum.BasisFire).value / 1000.00f;
        }

        public static float GetSCritDamagePlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var plusValue = 0f;
            //adj
            //...
            return plusValue;
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
