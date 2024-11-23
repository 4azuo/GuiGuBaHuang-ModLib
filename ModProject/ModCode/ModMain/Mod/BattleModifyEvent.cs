using EBattleTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
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

        private static readonly IDictionary<string, int> _nullify = new Dictionary<string, int>();
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
                _nullify[e.data.createUnitSoleID] = Convert.ToInt32(UnitModifyHelper.GetRefineCustommAdjValue(humanData.worldUnitData.unit, AdjTypeEnum.Nullify));
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

            DebugHelper.WriteLine($"1: {e.dynV.baseValue}");
            //nullify
            if (ModBattleEvent.IsWorldUnitHit)
            {
                if (_nullify[ModBattleEvent.HitUnit.data.createUnitSoleID] > 0)
                {
                    _nullify[ModBattleEvent.HitUnit.data.createUnitSoleID]--;
                    e.dynV.baseValue = 0;
                    return;
                }
            }

            DebugHelper.WriteLine($"2: {e.dynV.baseValue}");
            //evasion
            if (ModBattleEvent.IsWorldUnitHit)
            {
                var evaRate = GetEvadeBase(ModBattleEvent.HitWorldUnit);
                if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(GetEvadeMaxBase(ModBattleEvent.HitWorldUnit) + GetEvadeMaxPlus(ModBattleEvent.HitWorldUnit), evaRate) + GetEvadePlus(ModBattleEvent.HitWorldUnit)))
                {
                    e.hitData.isEvade = true;
                    e.dynV.baseValue = 0;
                    return;
                }
            }
            else
            {
                var evaRate = GetEvadeBase(ModBattleEvent.HitUnit);
                if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(GetEvadeMaxBase(ModBattleEvent.HitUnit), evaRate)))
                {
                    e.hitData.isEvade = true;
                    e.dynV.baseValue = 0;
                    return;
                }
            }

            DebugHelper.WriteLine($"3: {e.dynV.baseValue}");
            //add dmg (skill)
            e.dynV.baseValue += Convert.ToInt32(GetAdjSkillDmg(ModBattleEvent.AttackingWorldUnit) * GetAdjSkillDmgPlus(ModBattleEvent.AttackingWorldUnit));

            //add dmg (basis)
            if (pEnum != null)
            {
                var r = ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.AttackingUnit, pEnum).Parse<float>();
                e.dynV.baseValue += (atk * r / 1000f).Parse<int>();
            }

            DebugHelper.WriteLine($"4: {e.dynV.baseValue}");
            //add dmg (mp)
            if (ModBattleEvent.IsWorldUnitAttacking && attackUnitData.mp > 0)
            {
                var r = (attackUnitData.mp.Parse<float>() / attackUnitData.maxMP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
                hitUnitData.AddMP(-(atkGradeLvl / 5));
            }

            DebugHelper.WriteLine($"5: {e.dynV.baseValue}");
            //add dmg (sp)
            if (ModBattleEvent.IsWorldUnitAttacking && attackUnitData.sp > 0 && atkGradeLvl >= 4)
            {
                var r = (attackUnitData.sp.Parse<float>() / attackUnitData.maxSP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            DebugHelper.WriteLine($"6: {e.dynV.baseValue}");
            //add dmg (dp)
            if (ModBattleEvent.IsWorldUnitAttacking && attackUnitData.dp > 0 && atkGradeLvl >= 8)
            {
                var r = (attackUnitData.dp.Parse<float>() / attackUnitData.maxDP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            DebugHelper.WriteLine($"7: {e.dynV.baseValue}");
            //critical
            if (!e.hitData.isCrit)
            {
                if (ModBattleEvent.IsWorldUnitAttacking)
                {
                    var scritRate = GetSCritChanceBase(ModBattleEvent.AttackingWorldUnit);
                    if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(GetSCritChanceMaxBase(ModBattleEvent.AttackingWorldUnit) + GetSCritChanceMaxPlus(ModBattleEvent.AttackingWorldUnit), scritRate) + GetSCritChancePlus(ModBattleEvent.AttackingWorldUnit)))
                    {
                        e.hitData.isCrit = true;
                        e.dynV.baseValue += Convert.ToInt32(e.dynV.baseValue * (GetSCritDamageBase(ModBattleEvent.AttackingWorldUnit) + GetSCritDamagePlus(ModBattleEvent.AttackingWorldUnit)));
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

            DebugHelper.WriteLine($"8: {e.dynV.baseValue}");
            var defGradeLvl = hitUnitData?.worldUnitData?.unit?.GetGradeLvl() ?? 1;
            var def = ModBattleEvent.HitUnit.data.defense.baseValue;
            var minDmg = ModBattleEvent.IsWorldUnitAttacking ? (GetMinDmgBase(ModBattleEvent.AttackingWorldUnit) + GetMinDmgPlus(ModBattleEvent.AttackingWorldUnit)) : GetMinDmgBase(ModBattleEvent.AttackingUnit);
            var blockratio = GetBlockRatio(hitUnitData);

            //block
            var blockRate = Math.Sqrt(ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.HitUnit, pEnum) / 12);
            if (ModBattleEvent.IsWorldUnitHit)
            {
                if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(GetBlockMaxBase(ModBattleEvent.HitWorldUnit) + GetBlockMaxPlus(ModBattleEvent.HitWorldUnit), blockRate)))
                {
                    //var r = ModBattleEvent.HitUnit.data.hp.Parse<double>() / ModBattleEvent.HitUnit.data.maxHP.value.Parse<double>();
                    e.dynV.baseValue -= Convert.ToInt32((GetBlockDmgBase(ModBattleEvent.HitWorldUnit) + GetBlockDmgPlus(ModBattleEvent.HitWorldUnit))/* * r*/);
                }
            }
            else
            {
                if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(GetBlockMaxBase(ModBattleEvent.HitUnit), blockRate)))
                {
                    //var r = ModBattleEvent.HitUnit.data.hp.Parse<double>() / ModBattleEvent.HitUnit.data.maxHP.value.Parse<double>();
                    e.dynV.baseValue -= Convert.ToInt32(GetBlockDmgBase(ModBattleEvent.HitUnit)/* * r*/);
                }
            }

            DebugHelper.WriteLine($"9: {e.dynV.baseValue}");
            //block dmg (basis)
            if (pEnum != null && e.dynV.baseValue > minDmg)
            {
                var r = ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.HitUnit, pEnum).Parse<float>();
                e.dynV.baseValue -= (def * r / 200f).Parse<int>();
            }

            DebugHelper.WriteLine($"10: {e.dynV.baseValue}");
            //block dmg (mp)
            if (ModBattleEvent.IsWorldUnitHit && hitUnitData.mp > 0 && e.dynV.baseValue > minDmg)
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

            DebugHelper.WriteLine($"11: {e.dynV.baseValue}");
            //block dmg (sp)
            if (ModBattleEvent.IsWorldUnitHit && hitUnitData.sp > 0 && e.dynV.baseValue > minDmg && defGradeLvl >= 4)
            {
                var r = (hitUnitData.sp.Parse<float>() / hitUnitData.maxSP.value.Parse<float>()) * blockratio;
                e.dynV.baseValue -= (def * r).Parse<int>();
                hitUnitData.AddSP(-1);
            }

            DebugHelper.WriteLine($"12: {e.dynV.baseValue}");
            //block dmg (dp)
            if (ModBattleEvent.IsWorldUnitHit && hitUnitData.dp > 0 && e.dynV.baseValue > minDmg && defGradeLvl >= 8)
            {
                var r = (hitUnitData.dp.Parse<float>() / hitUnitData.maxDP.value.Parse<float>()) * blockratio;
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            DebugHelper.WriteLine($"13: {e.dynV.baseValue}");
            //min-dmg
            if (e.dynV.baseValue <= minDmg)
                e.dynV.baseValue = minDmg;

            //stronger every hit
            //if (e.dynV.baseValue < ModBattleEvent.HitUnit.data.maxHP.value / 50)
            //    ModBattleEvent.AttackingUnit.data.attack.baseValue += (ModBattleEvent.AttackingUnit.data.attack.baseValue * (ModBattleEvent.IsWorldUnitAttacking ? 0.0004f : 0.01f)).Parse<int>();
        }

        private static readonly IDictionary<string, SkillAttack> _castingSkill = new Dictionary<string, SkillAttack>();
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
                    var wunit = humanData.worldUnitData.unit;
                    if (humanData.hp < humanData.maxHP.value)
                        humanData.hp += Convert.ToInt32(GetHpRecoveryBase(wunit) + GetHpRecoveryPlus(wunit));
                    if (humanData.mp < humanData.maxMP.value)
                        humanData.mp += Convert.ToInt32(GetMpRecoveryBase(wunit) + GetMpRecoveryPlus(wunit));
                    if (humanData.sp < humanData.maxSP.value)
                        humanData.sp += Convert.ToInt32(GetSpRecoveryBase(wunit) + GetSpRecoveryPlus(wunit));
                }
            }
        }

        #region Modify
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

        public static double GetAdjSkillDmgPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.SkillDamage);
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

        public static int GetMinDmgBase(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var rs = wunit.GetGradeLvl();
            if (wunit.IsPlayer())
            {
                var x = EventHelper.GetEvent<RebirthEvent>(ModConst.REBIRTH_EVENT);
                rs += x.TotalGradeLvl;
            }
            return rs;
        }

        public static int GetMinDmgBase(UnitCtrlBase cunit)
        {
            if (cunit == null)
                return 0;
            return cunit.data.grade.value;
        }

        public static int GetMinDmgPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return Convert.ToInt32(UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.MinDamage));
        }

        public static double GetBlockMaxBase(WorldUnitBase wunit)
        {
            var defValue = 18.0d;
            if (wunit == null)
                return defValue;
            return defValue;
        }

        public static double GetBlockMaxBase(UnitCtrlBase cunit)
        {
            var defValue = 18.0d;
            if (cunit == null)
                return defValue;
            return defValue;
        }

        public static double GetBlockMaxPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BlockChanceMax);
        }

        public static double GetBlockDmgBase(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return wunit.GetDynProperty(UnitDynPropertyEnum.Defense).value;
        }

        public static double GetBlockDmgBase(UnitCtrlBase cunit)
        {
            if (cunit == null)
                return 0;
            return cunit.data.defense.value;
        }

        public static double GetBlockDmgPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BlockDmg);
        }

        public static double GetEvadeMaxBase(WorldUnitBase wunit)
        {
            var defValue = 12.0d;
            if (wunit == null)
                return defValue;
            return defValue;
        }

        public static double GetEvadeMaxBase(UnitCtrlBase cunit)
        {
            var defValue = 12.0d;
            if (cunit == null)
                return defValue;
            return defValue;
        }

        public static double GetEvadeMaxPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.EvadeChanceMax);
        }

        public static double GetEvadeBase(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisWind).value / 18);
        }

        public static double GetEvadeBase(UnitCtrlBase cunit)
        {
            if (cunit == null)
                return 0;
            return Math.Sqrt(cunit.data.basisWind.value / 18);
        }

        public static double GetEvadePlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            var stepData = wunit.GetMartialStep();
            if (stepData == null)
                return 0;
            var plusValue = 0d;
            //step
            var stepId = stepData.data.soleID;
            var stepGrade = stepData.data.propsInfoBase.grade;
            var stepLevel = stepData.data.propsInfoBase.level;
            plusValue += UnitModifyHelper.GetStepExpertEvade(ExpertEvent.GetExpertLvl(stepId, stepGrade, stepLevel), stepGrade, stepLevel);
            //adj
            plusValue += UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.EvadeChance);
            return plusValue;
        }

        public static double GetSCritChanceMaxBase(WorldUnitBase wunit)
        {
            var defValue = 8.0d;
            if (wunit == null)
                return defValue;
            return defValue;
        }

        public static double GetSCritChanceMaxPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.SCritChanceMax);
        }

        public static double GetSCritChanceBase(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisThunder).value / 50);
        }

        public static double GetSCritChancePlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.SCritChance);
        }

        public static double GetSCritDamageBase(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return 2.000f + wunit.GetDynProperty(UnitDynPropertyEnum.BasisFire).value / 1000.00f;
        }

        public static double GetSCritDamagePlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.SCritDamage);
        }

        public static double GetHpRecoveryBase(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisWood).value / 100f);
        }

        public static double GetHpRecoveryPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.RHp);
        }

        public static double GetMpRecoveryBase(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisFroze).value / 200f);
        }

        public static double GetMpRecoveryPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.RMp);
        }

        public static double GetSpRecoveryBase(WorldUnitBase wunit)
        {
            var defValue = 0.0d;
            if (wunit == null)
                return defValue;
            return defValue;
        }

        public static double GetSpRecoveryPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return UnitModifyHelper.GetRefineCustommAdjValue(wunit, AdjTypeEnum.RSp);
        }
        #endregion
    }
}
