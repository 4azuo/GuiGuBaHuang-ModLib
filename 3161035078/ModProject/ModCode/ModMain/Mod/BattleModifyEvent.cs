﻿using EBattleTypeData;
using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MOD_nE7UL2.Object.ModStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BATTLE_MODIFY_EVENT)]
    public class BattleModifyEvent : ModEvent
    {
        public static BattleModifyEvent Instance { get; set; }

        public const int BASIS_ON_DMG = 1000;
        public const int BASIS_ON_DEF = 400;
        public const float BASIS_ON_BLOCK_RATIO = 10000f;
        public const int BASIS_ON_BLOCK_COST = 800;
        public const float DMG_MULTIPLIER = 1.002f;

        public static bool IsShowCustomMonstCount { get; set; } = false;
        public static Text TextCustomMonstCount1 { get; private set; }
        public static Text TextCustomMonstCount2 { get; private set; }
        public static UIItemText TextDamageMultiplier { get; private set; }

        public static _MonstStrongerConfigs MonstStrongerConfigs => ModMain.ModObj.ModSettings.MonstStrongerConfigs;

        private static readonly IDictionary<string, int> _nullify = new Dictionary<string, int>();

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                _nullify.Clear();
            }
            else
            if (e.uiType.uiName == UIType.BattleInfo.uiName)
            {
                var ui = new UICover<UIBattleInfo>(UIType.BattleInfo);
                {
                    TextDamageMultiplier = ui.AddText(ui.MidCol, ui.FirstRow + 5, GameTool.LS("other500020089")).Format(Color.white, 20);
                    TextDamageMultiplier.SetWork(new UIItemWork
                    {
                        Formatter = (x) => new object[] { Math.Pow(DMG_MULTIPLIER, ModBattleEvent.BattleTime.TotalSeconds).ToString("#,##0.00") }
                    });

                    ui.UI.uiMap.goGroupRoot.SetActive(!SMLocalConfigsEvent.Instance.Configs.HideBattleMap);
                    if (IsShowCustomMonstCount)
                    {
                        TextCustomMonstCount1 = ui.UI.uiInfo.textMonstCount1.Replace();
                        TextCustomMonstCount2 = ui.UI.uiInfo.textMonstCount2.Replace();
                    }
                    else
                    {
                        ui.UI.uiInfo.goMonstCount1.SetActive(false);
                        ui.UI.uiInfo.goMonstCount2.SetActive(false);
                    }
                }
                ui.IsAutoUpdate = true;
            }
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
                _nullify[humanData.worldUnitData.unit.GetUnitId()] = Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(humanData.worldUnitData.unit, AdjTypeEnum.Nullify));
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

            //instant kill
            if (ModBattleEvent.IsWorldUnitHit)
            {
                var rate = GetInstantKillBase(ModBattleEvent.AttackingWorldUnit) + GetInstantKillPlus(ModBattleEvent.AttackingWorldUnit);
                if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, rate))
                {
                    e.dynV.baseValue = int.MaxValue;
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

            //DebugHelper.WriteLine($"x");
            //add dmg by time
            e.dynV.baseValue += (Math.Pow(DMG_MULTIPLIER, ModBattleEvent.BattleTime.TotalSeconds) * e.dynV.baseValue).Parse<int>();

            //DebugHelper.WriteLine($"3: {e.dynV.baseValue}");
            //add dmg (skill)
            if (ModBattleEvent.IsWorldUnitAttacking)
            {
                var castingSkill = e.hitData.GetCastingSkill();
                if (castingSkill != null)
                {
                    e.dynV.baseValue += Convert.ToInt32(GetSkillAdjDmgBase(ModBattleEvent.AttackingWorldUnit, castingSkill) * GetSkillAdjDmgPlus(ModBattleEvent.AttackingWorldUnit));
                }
            }

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

            //steal
            if (ModBattleEvent.IsWorldUnitAttacking)
            {
                //steal hp
                ModBattleEvent.AttackingUnit.data.AddHP(new MartialTool.HitData(ModBattleEvent.AttackingUnit, null, 0, 0,
                    (e.dynV.baseValue * (GetStealHpBase(ModBattleEvent.AttackingWorldUnit) + GetStealHpPlus(ModBattleEvent.AttackingWorldUnit))).Parse<int>()));

                //steal mp
                ModBattleEvent.AttackingUnit.data.AddMP(
                    (e.dynV.baseValue * (GetStealMpBase(ModBattleEvent.AttackingWorldUnit) + GetStealMpPlus(ModBattleEvent.AttackingWorldUnit))).Parse<int>());

                //steal sp
                ModBattleEvent.AttackingUnit.data.AddSP(
                    (e.dynV.baseValue * (GetStealSpBase(ModBattleEvent.AttackingWorldUnit) + GetStealSpPlus(ModBattleEvent.AttackingWorldUnit))).Parse<int>());
            }
        }

        public override void OnBattleUnitUseSkill(UnitUseSkill e)
        {
            base.OnBattleUnitUseSkill(e);
            var humanData = e.unit.data.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                humanData?.AddMP(-GetSkillAdjMpCost(humanData.worldUnitData.unit, e?.skill?.TryCast<SkillAttack>()?.skillData.data));
            }
        }

        public override void OnBattleUnitUseStep(UnitUseStep e)
        {
            base.OnBattleUnitUseStep(e);
            var humanData = e.unit.data.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                humanData?.AddMP(-GetStepAdjMpCost(humanData.worldUnitData.unit, e?.step?.data?.stepData?.data));
            }
        }

        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.Ignore, IsInBattle = HandleEnum.True)]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            
            foreach (var unit in ModBattleEvent.BattleUnits)
            {
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
            [2] = 0.55f,
            [3] = 0.60f,
            [4] = 0.70f,
            [5] = 0.80f,
            [6] = 0.95f,
            [7] = 1.10f,
            [8] = 1.30f,
            [9] = 1.50f,
            [10] = 2.00f,
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

        public static int GetSkillAdjDmgBase(WorldUnitBase wunit, Tuple<MartialType, DataProps.PropsData> skillData)
        {
            var soleId = skillData.Item2.soleID;
            var grade = skillData.Item2.propsInfoBase.grade;
            var level = skillData.Item2.propsInfoBase.level;
            var mType = skillData.Item1;
            return UnitModifyHelper.GetSkillExpertAtk(wunit.GetDynProperty(UnitDynPropertyEnum.Attack).baseValue, ExpertEvent.GetExpertLvl(soleId, grade, level), grade, level, mType);
        }

        public static double GetSkillAdjDmgPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.SkillDamage);
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

        //Min Dmg
        public static int GetMinDmgBase(WorldUnitBase wunit)
        {
            var rs = wunit.GetGradeLvl();
            if (wunit.IsPlayer())
            {
                rs += RebirthEvent.Instance.TotalGradeLvl;
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
            return Convert.ToInt32(CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.MinDamage));
        }

        //Block Max
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
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BlockChanceMax);
        }

        //Block Dmg
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
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.BlockDmg);
        }

        //Evade Max
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
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.EvadeChanceMax);
        }

        //Evade
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
            plusValue += CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.EvadeChance);
            return plusValue;
        }

        //SCrit Chance Max
        public static double GetSCritChanceMaxBase(WorldUnitBase wunit)
        {
            return 8.0d;
        }

        public static double GetSCritChanceMaxPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.SCritChanceMax);
        }

        //SCrit Chance
        public static double GetSCritChanceBase(WorldUnitBase wunit)
        {
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisThunder).value / 50);
        }

        public static double GetSCritChancePlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.SCritChance);
        }

        //SCrit Damage
        public static double GetSCritDamageBase(WorldUnitBase wunit)
        {
            return 2.000f + wunit.GetDynProperty(UnitDynPropertyEnum.BasisFire).value / 1000.00f;
        }

        public static double GetSCritDamagePlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.SCritDamage);
        }

        //Hp Recovery
        public static double GetHpRecoveryBase(WorldUnitBase wunit)
        {
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisWood).value / 100f);
        }

        public static double GetHpRecoveryPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.RHp);
        }

        //Mp Recovery
        public static double GetMpRecoveryBase(WorldUnitBase wunit)
        {
            return Math.Sqrt(wunit.GetDynProperty(UnitDynPropertyEnum.BasisFroze).value / 200f);
        }

        public static double GetMpRecoveryPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.RMp);
        }

        //Sp Recovery
        public static double GetSpRecoveryBase(WorldUnitBase wunit)
        {
            return 0.0d;
        }

        public static double GetSpRecoveryPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.RSp);
        }

        //Steal Hp
        public static double GetStealHpBase(WorldUnitBase wunit)
        {
            return 0.0d;
        }

        public static double GetStealHpBase(UnitCtrlBase cunit)
        {
            return 0.0d;
        }

        public static double GetStealHpPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.StealHp);
        }

        //Steal Mp
        public static double GetStealMpBase(WorldUnitBase wunit)
        {
            return 0.0d;
        }

        public static double GetStealMpBase(UnitCtrlBase cunit)
        {
            return 0.0d;
        }

        public static double GetStealMpPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.StealMp);
        }

        //Steal Sp
        public static double GetStealSpBase(WorldUnitBase wunit)
        {
            return 0.0d;
        }

        public static double GetStealSpBase(UnitCtrlBase cunit)
        {
            return 0.0d;
        }

        public static double GetStealSpPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.StealSp);
        }

        //Instant Kill
        public static double GetInstantKillBase(WorldUnitBase wunit)
        {
            return 0.0d;
        }

        public static double GetInstantKillBase(UnitCtrlBase cunit)
        {
            return 0.0d;
        }

        public static double GetInstantKillPlus(WorldUnitBase wunit)
        {
            return CustomRefineEvent.GetCustomAdjValue(wunit, AdjTypeEnum.InstantKill);
        }
        #endregion
    }
}
