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
    [Cache(ModConst.BATTLE_MODIFY_EVENT)]
    public class BattleModifyEvent : ModEvent
    {
        public override int OrderIndex => 9000;

        public Dictionary<int, float> BlockRatio { get; set; } = new Dictionary<int, float>()
        {
            [1] = 0.50f,
            [2] = 0.55f,
            [3] = 0.60f,
            [4] = 0.65f,
            [5] = 0.80f,
            [6] = 0.90f,
            [7] = 1.00f,
            [8] = 1.20f,
            [9] = 1.40f,
            [10] = 2.00f,
        };

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            base.OnIntoBattleFirst(e);

            var humanData = e.data.TryCast<UnitDataHuman>();
            if (humanData?.worldUnitData?.unit != null)
            {
                var artAdjustValues = ArtifactAffectEvent.GetAdjustValues(humanData.worldUnitData.unit);
                humanData.attack.baseValue += artAdjustValues[0];
                humanData.defense.baseValue += artAdjustValues[1];

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
            var dType = ModBattleEvent.GetDmgBasisType(e.hitData);
            var pEnum = ModBattleEvent.GetDmgPropertyEnum(dType);
            var defGradeLvl = hitUnitData?.worldUnitData?.unit?.GetGradeLvl() ?? 1;
            var atk = ModBattleEvent.AttackingUnit.data.attack.baseValue;
            var def = ModBattleEvent.HitUnit.data.defense.baseValue;
            var minDmg = ModBattleEvent.AttackingUnit.data.grade.value;

            //evasion
            var evaRate = Math.Sqrt(ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.HitUnit, UnitDynPropertyEnum.BasisWind) / 18);
            if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(12.00f, evaRate)))
            {
                e.hitData.isEvade = true;
                e.dynV.baseValue = 0;
                return;
            }

            //add dmg (basis)
            if (pEnum != null)
            {
                var r = ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.AttackingUnit, pEnum);
                e.dynV.baseValue += atk * r / 1000;
            }

            //add dmg (sp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.sp > 0)
            {
                var r = (attackUnitData.sp.Parse<float>() / attackUnitData.maxSP.value.Parse<float>()) / 10.0f;
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //add dmg (dp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.dp > 0)
            {
                var r = (attackUnitData.dp.Parse<float>() / attackUnitData.maxDP.value.Parse<float>()) / 10.0f;
                e.dynV.baseValue += (atk * r).Parse<int>();
            }

            //add dmg (mp)
            if (attackUnitData?.worldUnitData?.unit != null && attackUnitData.mp > 0 && ModBattleEvent.IsBasisMagic(dType))
            {
                var r = (attackUnitData.mp.Parse<float>() / attackUnitData.maxMP.value.Parse<float>()) / 10.0f;
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

            //block
            var blockRate = Math.Sqrt(ModBattleEvent.GetUnitPropertyValue(ModBattleEvent.HitUnit, pEnum) / 12);
            if (ValueHelper.IsBetween(CommonTool.Random(0.00f, 100.00f), 0.00f, Math.Min(18.00f, blockRate)))
            {
                var r = hitUnitData.hp.Parse<float>() / hitUnitData.maxHP.value.Parse<float>();
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
                var r = (hitUnitData.sp.Parse<float>() / hitUnitData.maxSP.value.Parse<float>()) * BlockRatio[defGradeLvl];
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //block dmg (dp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.dp > 0 && e.dynV.baseValue > minDmg && defGradeLvl >= 8)
            {
                var r = (hitUnitData.dp.Parse<float>() / hitUnitData.maxDP.value.Parse<float>()) * BlockRatio[defGradeLvl];
                e.dynV.baseValue -= (def * r).Parse<int>();
            }

            //block dmg (mp)
            if (hitUnitData?.worldUnitData?.unit != null && hitUnitData.mp > 0 && e.dynV.baseValue > minDmg)
            {
                var blockTimes = CommonTool.Random(defGradeLvl / 2, defGradeLvl);
                for (int i = 0; i < blockTimes && hitUnitData.mp > 0 && e.dynV.baseValue > minDmg; i++)
                {
                    var r = (hitUnitData.mp.Parse<float>() / hitUnitData.maxMP.value.Parse<float>()) * BlockRatio[defGradeLvl];
                    var blockedDmg = (def * r).Parse<int>();
                    var lostMp = Math.Max(defGradeLvl, blockedDmg / (100 * defGradeLvl));
                    e.dynV.baseValue -= blockedDmg;
                    hitUnitData.AddMP(-lostMp);
                }
            }

            //min-dmg
            if (e.dynV.baseValue <= minDmg)
                e.dynV.baseValue = minDmg;

            //stronger every hit
            e.hitData.attackUnit.data.attack.baseValue += (e.hitData.attackUnit.data.attack.baseValue * (attackUnitData?.worldUnitData?.unit != null ? 0.0004f : 0.01f)).Parse<int>();
        }

        [EventCondition(IsInBattle = true)]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();

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
