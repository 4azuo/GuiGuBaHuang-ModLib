using EBattleTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DataMap;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_MODIFY_EVENT)]
    public class BattleModifyEvent : ModEvent
    {
        public override int OrderIndex => 9000;

        public const int BASIS_ON_DMG = 1000;
        public const int BASIS_ON_DEF = 400;
        public const float BASIS_ON_BLOCK_RATIO = 10000f;
        public const int BASIS_ON_BLOCK_COST = 800;

        public static _MonstStrongerConfigs MonstStrongerConfigs => ModMain.ModObj.InGameCustomSettings.MonstStrongerConfigs;

        private static readonly IDictionary<string, int> _nullify = new Dictionary<string, int>();
        private static readonly IList<MonstType> _monstStrongerAdditionalFlg = new List<MonstType>();
        private static readonly IDictionary<string, DataProps.PropsData> _castingSkill = new Dictionary<string, DataProps.PropsData>();
        private static readonly IDictionary<string, MartialType> _castingSkillType = new Dictionary<string, MartialType>();

        public override void OnBattleEndOnce(BattleEnd e)
        {
            base.OnBattleEndOnce(e);
            _nullify.Clear();
            _monstStrongerAdditionalFlg.Clear();
            _castingSkill.Clear();
            _castingSkillType.Clear();
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
                _nullify[humanData.worldUnitData.unit.GetUnitId()] = Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(humanData.worldUnitData.unit, AdjTypeEnum.Nullify));
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

            //DebugHelper.WriteLine($"1: {e.dynV.baseValue}");
            //nullify
            if (ModBattleEvent.IsWorldUnitHit)
            {
                var unitId = ModBattleEvent.HitWorldUnit.GetUnitId();
                if (_nullify.ContainsKey(unitId) && _nullify[unitId] > 0)
                {
                    _nullify[unitId]--;
                    e.dynV.baseValue = 0;
                    return;
                }
            }

            //DebugHelper.WriteLine($"2: {e.dynV.baseValue}");
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

            //DebugHelper.WriteLine($"3: {e.dynV.baseValue}");
            //add dmg (skill)
            e.dynV.baseValue += Convert.ToInt32(GetSkillAdjDmgBase(ModBattleEvent.AttackingWorldUnit) * GetSkillAdjDmgPlus(ModBattleEvent.AttackingWorldUnit));

            //add dmg (basis)
            if (pEnum != null)
            {
                var r = ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.AttackingUnit, pEnum);
                e.dynV.baseValue += atk * r / BASIS_ON_DMG;
            }

            //DebugHelper.WriteLine($"4: {e.dynV.baseValue}");
            //add dmg (mp)
            if (ModBattleEvent.IsWorldUnitAttacking && attackUnitData.mp > 0)
            {
                var r = (attackUnitData.mp.Parse<float>() / attackUnitData.maxMP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
                attackUnitData.AddMP(-(atkGradeLvl / 5));
            }

            //DebugHelper.WriteLine($"5: {e.dynV.baseValue}");
            //add dmg (sp)
            if (ModBattleEvent.IsWorldUnitAttacking && attackUnitData.sp > 0 && atkGradeLvl >= 4)
            {
                var r = (attackUnitData.sp.Parse<float>() / attackUnitData.maxSP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //DebugHelper.WriteLine($"6: {e.dynV.baseValue}");
            //add dmg (dp)
            if (ModBattleEvent.IsWorldUnitAttacking && attackUnitData.dp > 0 && atkGradeLvl >= 8)
            {
                var r = (attackUnitData.dp.Parse<float>() / attackUnitData.maxDP.value.Parse<float>());
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //DebugHelper.WriteLine($"7: {e.dynV.baseValue}");
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

            //DebugHelper.WriteLine($"8: {e.dynV.baseValue}");
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

            //DebugHelper.WriteLine($"9: {e.dynV.baseValue}");
            //block dmg (basis)
            if (pEnum != null && e.dynV.baseValue > minDmg)
            {
                var r = ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.HitUnit, pEnum);
                e.dynV.baseValue -= def * r / BASIS_ON_DEF;
            }

            //DebugHelper.WriteLine($"10: {e.dynV.baseValue}");
            //block dmg (mp)
            if (ModBattleEvent.IsWorldUnitHit && hitUnitData.mp > 0 && e.dynV.baseValue > minDmg)
            {
                var blockcost = GetBlockMpCost(hitUnitData);
                var blockTimes = CommonTool.Random(defGradeLvl / 2, defGradeLvl);
                for (int i = 0; i < blockTimes && e.dynV.baseValue > minDmg; i++)
                {
                    var mpcost = Math.Max(defGradeLvl, e.dynV.baseValue / blockcost);
                    if (hitUnitData.mp < mpcost)
                        break;
                    var r = (hitUnitData.mp.Parse<float>() / hitUnitData.maxMP.value.Parse<float>()) * blockratio;
                    var blockedDmg = (def * r).Parse<int>();
                    e.dynV.baseValue -= blockedDmg;
                    hitUnitData.AddMP(-mpcost);
                }
            }

            //DebugHelper.WriteLine($"11: {e.dynV.baseValue}");
            //block dmg (sp)
            if (ModBattleEvent.IsWorldUnitHit && hitUnitData.sp > 0 && e.dynV.baseValue > minDmg && defGradeLvl >= 4)
            {
                var r = (hitUnitData.sp.Parse<float>() / hitUnitData.maxSP.value.Parse<float>()) * blockratio;
                e.dynV.baseValue -= (def * r).Parse<int>();
                hitUnitData.AddSP(-1);
            }

            //DebugHelper.WriteLine($"12: {e.dynV.baseValue}");
            //block dmg (dp)
            if (ModBattleEvent.IsWorldUnitHit && hitUnitData.dp > 0 && e.dynV.baseValue > minDmg && defGradeLvl >= 8)
            {
                var r = (hitUnitData.dp.Parse<float>() / hitUnitData.maxDP.value.Parse<float>()) * blockratio;
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //DebugHelper.WriteLine($"13: {e.dynV.baseValue}");
            //min-dmg
            if (e.dynV.baseValue <= minDmg)
                e.dynV.baseValue = minDmg;

            //DebugHelper.WriteLine($"14: {e.dynV.baseValue}");
            //monst-stronger
            var monstData = e?.hitUnit?.data?.TryCast<UnitDataMonst>();
            if (monstData != null && MonstStrongerEvent.AdditionalStts.ContainsKey(monstData.monstType) &&
                ModBattleEvent.IsPlayerAttacking && _castingSkillType[ModBattleEvent.AttackingWorldUnit.GetUnitId()] == MartialType.SkillLeft &&
                !_monstStrongerAdditionalFlg.Contains(monstData.monstType))
            {
                var x = EventHelper.GetEvent<MonstStrongerEvent>(ModConst.MONST_STRONGER_EVENT);
                if (e.dynV.baseValue >= GetAdditionalCond(monstData))
                {
                    x.Additional[monstData.monstType] += MonstStrongerConfigs.Additionnal;
                    _monstStrongerAdditionalFlg.Add(monstData.monstType);
                }
            }
        }

        private float GetAdditionalCond(UnitDataMonst monst)
        {
            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            var monstLvl = monst.grade.value;
            var addStt = MonstStrongerEvent.AdditionalStts[monst.monstType] - (MonstStrongerEvent.AdditionalStts[monst.monstType] * monstLvl * gameLvl / 100f);
            return monst.maxHP.value * addStt * ModBattleEvent.AttackingWorldUnit.GetGradeLvl().Parse<float>() / monstLvl.Parse<float>() / gameLvl;
        }

        public override void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            base.OnBattleUnitUseSkill(e);
            var humanData = e.unit.data.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                var skill = e?.skill?.TryCast<SkillAttack>();
                if (skill != null)
                {
                    var unitId = humanData.worldUnitData.unit.GetUnitId();
                    _castingSkill[unitId] = skill.skillData.data;
                    _castingSkillType[unitId] = skill.skillData.martialType;

                    humanData?.AddMP(-GetSkillAdjMpCost(humanData.worldUnitData.unit, _castingSkill[unitId]));
                }
            }
        }

        public override void OnBattleUnitUseStep(UnitUseStep e)
        {
            base.OnBattleUnitUseStep(e);
            var humanData = e.unit.data.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                var unitId = humanData.worldUnitData.unit.GetUnitId();
                _castingSkill[unitId] = e?.step?.data?.stepData?.data;
                _castingSkillType[unitId] = MartialType.Step;

                humanData?.AddMP(-GetStepAdjMpCost(humanData.worldUnitData.unit, _castingSkill[unitId]));
            }
        }

        [ErrorIgnore]
        [EventCondition(IsInBattle = 1)]
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
            int gradeLvl;
            if (u == null || !BlockRatio.ContainsKey(gradeLvl = u?.worldUnitData?.unit?.GetGradeLvl() ?? 0))
                return 0f;
            var wunit = u.worldUnitData.unit;
            return BlockRatio[gradeLvl] + wunit.GetBasisMagicSum() / BASIS_ON_BLOCK_RATIO;
        }

        public static int GetBlockMpCost(UnitDataHuman u)
        {
            int gradeLvl;
            if (u == null || !BlockRatio.ContainsKey(gradeLvl = u?.worldUnitData?.unit?.GetGradeLvl() ?? 0))
                return 0;
            var wunit = u.worldUnitData.unit;
            return 100 * gradeLvl + wunit.GetBasisPhysicSum() / BASIS_ON_BLOCK_COST;
        }

        public static int GetSkillAdjDmgBase(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            DataProps.PropsData skillData;
            _castingSkill.TryGetValue(wunit.GetUnitId(), out skillData);
            if (skillData == null)
                return 0;
            var soleId = skillData.soleID;
            var grade = skillData.propsInfoBase.grade;
            var level = skillData.propsInfoBase.level;
            var mType = skillData.To<DataProps.MartialData>().martialType;
            return UnitModifyHelper.GetSkillExpertAtk(wunit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, ExpertEvent.GetExpertLvl(soleId, grade, level), grade, level, mType);
        }

        public static double GetSkillAdjDmgPlus(WorldUnitBase wunit)
        {
            if (wunit == null)
                return 0;
            return CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.SkillDamage);
        }

        public static int GetSkillAdjMpCost(WorldUnitBase wunit, DataProps.PropsData props)
        {
            if (wunit == null || props == null)
                return 0;
            var soleId = props.soleID;
            var grade = props.propsInfoBase.grade;
            var level = props.propsInfoBase.level;
            var expertLvl = ExpertEvent.GetExpertLvl(soleId, grade, level);
            return UnitModifyHelper.GetSkillExpertMpCost(SkillHelper.GetSkillMpCost(props), expertLvl, grade, level, wunit.GetGradeLvl() * expertLvl / 10);
        }

        public static int GetStepAdjMpCost(WorldUnitBase wunit, DataProps.PropsData props)
        {
            if (wunit == null || props == null)
                return 0;
            var soleId = props.soleID;
            var grade = props.propsInfoBase.grade;
            var level = props.propsInfoBase.level;
            var expertLvl = ExpertEvent.GetExpertLvl(soleId, grade, level);
            return UnitModifyHelper.GetStepExpertMpCost(SkillHelper.GetStepMpCost(props), expertLvl, grade, level, wunit.GetGradeLvl() * expertLvl / 5);
        }

        public static int GetMinDmgBase(WorldUnitBase wunit)
        {
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
            var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
            return cunit.data.grade.value * gameLvl;
        }

        public static int GetMinDmgPlus(WorldUnitBase wunit)
        {
            return Convert.ToInt32(CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.MinDamage));
        }

        public static double GetBlockMaxBase(WorldUnitBase wunit)
        {
            return 18.0d;
        }

        public static double GetBlockMaxBase(UnitCtrlBase cunit)
        {
            return 18.0d;
        }

        public static double GetBlockMaxPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BlockChanceMax);
        }

        public static double GetBlockDmgBase(WorldUnitBase wunit)
        {
            return wunit.GetDynProperty(UnitDynPropertyEnum.Defense).value;
        }

        public static double GetBlockDmgBase(UnitCtrlBase cunit)
        {
            return cunit.data.defense.value;
        }

        public static double GetBlockDmgPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.BlockDmg);
        }

        public static double GetEvadeMaxBase(WorldUnitBase wunit)
        {
            return 12.0d;
        }

        public static double GetEvadeMaxBase(UnitCtrlBase cunit)
        {
            return 12.0d;
        }

        public static double GetEvadeMaxPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.EvadeChanceMax);
        }

        public static double GetEvadeBase(WorldUnitBase wunit)
        {
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisWind).value / 18);
        }

        public static double GetEvadeBase(UnitCtrlBase cunit)
        {
            return Math.Sqrt(cunit.data.basisWind.value / 18);
        }

        public static double GetEvadePlus(WorldUnitBase wunit)
        {
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
            plusValue += CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.EvadeChance);
            return plusValue;
        }

        public static double GetSCritChanceMaxBase(WorldUnitBase wunit)
        {
            return 8.0d;
        }

        public static double GetSCritChanceMaxPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.SCritChanceMax);
        }

        public static double GetSCritChanceBase(WorldUnitBase wunit)
        {
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisThunder).value / 50);
        }

        public static double GetSCritChancePlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.SCritChance);
        }

        public static double GetSCritDamageBase(WorldUnitBase wunit)
        {
            return 2.000f + wunit.GetDynProperty(UnitDynPropertyEnum.BasisFire).value / 1000.00f;
        }

        public static double GetSCritDamagePlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.SCritDamage);
        }

        public static double GetHpRecoveryBase(WorldUnitBase wunit)
        {
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisWood).value / 100f);
        }

        public static double GetHpRecoveryPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.RHp);
        }

        public static double GetMpRecoveryBase(WorldUnitBase wunit)
        {
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisFroze).value / 200f);
        }

        public static double GetMpRecoveryPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.RMp);
        }

        public static double GetSpRecoveryBase(WorldUnitBase wunit)
        {
            return 0.0d;
        }

        public static double GetSpRecoveryPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetRefineCustommAdjValue(wunit, AdjTypeEnum.RSp);
        }
        #endregion
    }
}
