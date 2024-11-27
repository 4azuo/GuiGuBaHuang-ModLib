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

        public const float ADDITIONAL_VALUE = 0.020f;
        public static IDictionary<MonstType, float> AdditionalStts { get; set; } = new Dictionary<MonstType, float>
        {
            [MonstType.Common] = 2.00f,
            [MonstType.Elite] = 0.20f,
            [MonstType.BOSS] = 0.02f,
        };

        public IDictionary<MonstType, float> Additional { get; set; } = new Dictionary<MonstType, float>
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

        public override void OnBattleUnitInto(UnitCtrlBase e)
        {
            base.OnBattleUnitInto(e);

            var monstData = e?.data?.TryCast<UnitDataMonst>();

            //additional
            if (monstData != null && AdditionalStts.ContainsKey(monstData.monstType))
            {
                var r = Additional[monstData.monstType];
                monstData.attack.baseValue += (monstData.attack.baseValue * (r / 3)).Parse<int>();
                monstData.defense.baseValue += (monstData.defense.baseValue * (r / 10)).Parse<int>();
                monstData.maxHP.baseValue += (monstData.maxHP.baseValue * r).Parse<int>();
            }

            //monster
            if (monstData != null && Configs.GrowRate.ContainsKey(monstData.monstType))
            {
                //area bonus
                var areaId = g.world.playerUnit.data.unitData.pointGridData.areaBaseID;
                if (Configs.AreaBonus.ContainsKey(areaId))
                {
                    var r = Configs.AreaBonus[areaId];
                    monstData.attack.baseValue += (monstData.attack.baseValue * (r / 3)).Parse<int>();
                    monstData.defense.baseValue += (monstData.defense.baseValue * (r / 10)).Parse<int>();
                    monstData.maxHP.baseValue += (monstData.maxHP.baseValue * r).Parse<int>();
                }

                //grow-rate & kill-rate
                var gameLvl = g.data.dataWorld.data.gameLevel.Parse<int>();
                var ratio = (Counter * Configs.GrowRate[monstData.monstType]) + (KillCounter[monstData.monstType] * Configs.KillGrowRate[monstData.monstType]);
                monstData.attack.baseValue += (monstData.attack.baseValue * (ratio * Configs.AtkR * monstData.grade.value * gameLvl)).Parse<int>();
                monstData.defense.baseValue += (monstData.defense.baseValue * (ratio * Configs.DefR * monstData.grade.value * gameLvl)).Parse<int>();
                monstData.maxHP.baseValue += (monstData.maxHP.baseValue * (ratio * Configs.MHpR * monstData.grade.value * gameLvl)).Parse<int>() +
                    //rebirth
                    (monstData.maxHP.baseValue * EventHelper.GetEvent<RebirthEvent>(ModConst.REBIRTH_EVENT).TotalGradeLvl * Configs.RebirthAffect * gameLvl).Parse<int>() +
                    //level oppressive
                    (g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Attack).value * Configs.PlayerAtk2HpRate[monstData.monstType] * (monstData.grade.value.Parse<float>() / g.world.playerUnit.GetGradeLvl().Parse<float>())).Parse<int>();

                //heal fullhp
                monstData.hp = monstData.maxHP.value;

                //basis
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

                //others
                //monstData.attack.baseValue += (??? / 100.00f * monstData.attack.baseValue).Parse<int>();
                var adjustDef1 = ((((monstData.basisFist.value + monstData.basisPalm.value + monstData.basisFinger.value) / 3.0f) / 1000.00f) * monstData.defense.baseValue).Parse<int>();
                var adjustDef2 = ((monstData.basisEarth.value / 1000.00f) * monstData.defense.baseValue).Parse<int>();
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