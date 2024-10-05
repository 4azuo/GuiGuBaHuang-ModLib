using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using System.Collections.Generic;
using UnityEngine;
using static MOD_nE7UL2.Object.InGameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MONST_STRONGER_EVENT)]
    public class MonstStrongerEvent : ModEvent
    {
        public override int OrderIndex => 8010;

        public static _MonstStrongerConfigs Configs => ModMain.ModObj.InGameCustomSettings.MonstStrongerConfigs;

        public IDictionary<MonstType, int> KillCounter { get; set; } = new Dictionary<MonstType, int>
        {
            [MonstType.Common] = 0,
            [MonstType.Elite] = 0,
            [MonstType.BOSS] = 0,
        };

        public int Counter { get; set; } = 0;

        public override void OnMonthly()
        {
            Counter++;
        }

        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);

            var monstData = e?.unit?.data?.TryCast<UnitDataMonst>();
            if (monstData != null && Configs.KillGrowRate.ContainsKey(monstData.monstType))
            {
                KillCounter[monstData.monstType]++;
            }
        }

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            base.OnIntoBattleFirst(e);

            var monstData = e?.data?.TryCast<UnitDataMonst>();
            if (monstData != null && Configs.GrowRate.ContainsKey(monstData.monstType))
            {
                var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
                var ratio = (Counter * Configs.GrowRate[monstData.monstType]) + (KillCounter[monstData.monstType] * Configs.KillGrowRate[monstData.monstType]);
                monstData.attack.baseValue += (monstData.attack.baseValue * (ratio * Configs.AtkR * monstData.grade.baseValue * gameLvl)).Parse<int>();
                monstData.defense.baseValue += (monstData.defense.baseValue * (ratio * Configs.DefR * monstData.grade.baseValue * gameLvl)).Parse<int>();
                monstData.maxHP.baseValue += (monstData.maxHP.baseValue * (ratio * Configs.MHpR * monstData.grade.baseValue * gameLvl)).Parse<int>() +
                    (g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.Attack) * Configs.PlayerAtk2HpRate[monstData.monstType] * (monstData.grade.baseValue.Parse<float>() / g.world.playerUnit.GetProperty<int>(UnitPropertyEnum.GradeID).Parse<float>())).Parse<int>();
                monstData.hp = monstData.maxHP.baseValue;

                monstData.basisBlade.baseValue *= monstData.grade.baseValue;
                monstData.basisEarth.baseValue *= monstData.grade.baseValue;
                monstData.basisFinger.baseValue *= monstData.grade.baseValue;
                monstData.basisFire.baseValue *= monstData.grade.baseValue;
                monstData.basisFist.baseValue *= monstData.grade.baseValue;
                monstData.basisFroze.baseValue *= monstData.grade.baseValue;
                monstData.basisPalm.baseValue *= monstData.grade.baseValue;
                monstData.basisSpear.baseValue *= monstData.grade.baseValue;
                monstData.basisSword.baseValue *= monstData.grade.baseValue;
                monstData.basisThunder.baseValue *= monstData.grade.baseValue;
                monstData.basisWind.baseValue *= monstData.grade.baseValue;
                monstData.basisWood.baseValue *= monstData.grade.baseValue;

                //monstData.attack.baseValue += (??? / 100.00f * monstData.attack.baseValue).Parse<int>();
                var adjustDef1 = ((((monstData.basisFist.baseValue + monstData.basisPalm.baseValue + monstData.basisFinger.baseValue) / 3.0f) / 1000.00f) * monstData.maxHP.baseValue).Parse<int>();
                var adjustDef2 = ((monstData.basisEarth.baseValue / 1000.00f) * monstData.defense.baseValue).Parse<int>();
                monstData.defense.baseValue += adjustDef1 + adjustDef2;
                var adjustMs = (monstData.basisWind.baseValue / 100.00f).Parse<int>();
                monstData.moveSpeed.baseValue += adjustMs;
            }
        }

        //BattleModifyEventに参照してください。
        //public override void OnBattleUnitHitDynIntHandler(UnitHitDynIntHandler e)
        //{
        //    base.OnBattleUnitHitDynIntHandler(e);
        //}
    }
}