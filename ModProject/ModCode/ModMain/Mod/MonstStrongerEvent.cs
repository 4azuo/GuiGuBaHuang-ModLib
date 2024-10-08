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
                monstData.attack.baseValue += (monstData.attack.value * (ratio * Configs.AtkR * monstData.grade.value * gameLvl)).Parse<int>();
                monstData.defense.baseValue += (monstData.defense.value * (ratio * Configs.DefR * monstData.grade.value * gameLvl)).Parse<int>();
                monstData.maxHP.baseValue += (monstData.maxHP.value * (ratio * Configs.MHpR * monstData.grade.value * gameLvl)).Parse<int>() +
                    (g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Attack).value * Configs.PlayerAtk2HpRate[monstData.monstType] * (monstData.grade.value.Parse<float>() / g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.GradeID).value.Parse<float>())).Parse<int>();
                monstData.hp = monstData.maxHP.value;

                monstData.basisBlade.baseValue *= monstData.grade.value;
                monstData.basisEarth.baseValue *= monstData.grade.value;
                monstData.basisFinger.baseValue *= monstData.grade.value;
                monstData.basisFire.baseValue *= monstData.grade.value;
                monstData.basisFist.baseValue *= monstData.grade.value;
                monstData.basisFroze.baseValue *= monstData.grade.value;
                monstData.basisPalm.baseValue *= monstData.grade.value;
                monstData.basisSpear.baseValue *= monstData.grade.value;
                monstData.basisSword.baseValue *= monstData.grade.value;
                monstData.basisThunder.baseValue *= monstData.grade.value;
                monstData.basisWind.baseValue *= monstData.grade.value;
                monstData.basisWood.baseValue *= monstData.grade.value;

                //monstData.attack.baseValue += (??? / 100.00f * monstData.attack.value).Parse<int>();
                var adjustDef1 = ((((monstData.basisFist.value + monstData.basisPalm.value + monstData.basisFinger.value) / 3.0f) / 1000.00f) * monstData.maxHP.value).Parse<int>();
                var adjustDef2 = ((monstData.basisEarth.value / 1000.00f) * monstData.defense.value).Parse<int>();
                monstData.defense.baseValue += adjustDef1 + adjustDef2;
                var adjustMs = (monstData.basisWind.value / 100.00f).Parse<int>();
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