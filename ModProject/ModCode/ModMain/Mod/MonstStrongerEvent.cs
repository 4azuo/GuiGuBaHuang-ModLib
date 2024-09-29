using EBattleTypeData;
using MOD_nE7UL2.Const;
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
            var monstData = e?.unit?.data?.TryCast<UnitDataMonst>();
            if (monstData != null && Configs.KillGrowRate.ContainsKey(monstData.monstType))
            {
                KillCounter[monstData.monstType]++;
            }
        }

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            var monstData = e?.data?.TryCast<UnitDataMonst>();
            if (monstData != null && Configs.GrowRate.ContainsKey(monstData.monstType))
            {
                var ratio = (Counter * Configs.GrowRate[monstData.monstType]) + (KillCounter[monstData.monstType] * Configs.KillGrowRate[monstData.monstType]);
                monstData.attack.baseValue += (int)(monstData.attack.baseValue * (ratio * Configs.AtkR * monstData.grade.baseValue * g.data.dataWorld.data.gameLevel.Parse<int>()));
                monstData.defense.baseValue += (int)(monstData.defense.baseValue * (ratio * Configs.DefR * monstData.grade.baseValue * g.data.dataWorld.data.gameLevel.Parse<int>()));
                monstData.maxHP.baseValue += (int)(monstData.maxHP.baseValue * (ratio * Configs.MHpR * monstData.grade.baseValue * g.data.dataWorld.data.gameLevel.Parse<int>()));
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
            }
        }
    }
}