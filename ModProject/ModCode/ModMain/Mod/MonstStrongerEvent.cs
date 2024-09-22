using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MONST_STRONGER_EVENT)]
    public class MonstStrongerEvent : ModEvent
    {
        public static Dictionary<MonstType, float> GROW_RATIO
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.MonstStrongerConfigs.GrowRate;
            }
        }

        public static Dictionary<MonstType, float> KILL_GROW_RATIO
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.MonstStrongerConfigs.KillGrowRate;
            }
        }

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
            if (monstData != null && KILL_GROW_RATIO.ContainsKey(monstData.monstType))
            {
                KillCounter[monstData.monstType]++;
            }
        }

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            var monstData = e?.data?.TryCast<UnitDataMonst>();
            if (monstData != null && GROW_RATIO.ContainsKey(monstData.monstType))
            {
                var ratio = (Counter * GROW_RATIO[monstData.monstType]) + (KillCounter[monstData.monstType] * KILL_GROW_RATIO[monstData.monstType]);
                monstData.attack.baseValue += (int)(monstData.attack.baseValue * ratio);
                monstData.defense.baseValue += (int)(monstData.defense.baseValue * (ratio / 2));
                monstData.maxHP.baseValue += (int)(monstData.maxHP.baseValue * ratio);
                monstData.hp = monstData.maxHP.baseValue;
            }
        }
    }
}