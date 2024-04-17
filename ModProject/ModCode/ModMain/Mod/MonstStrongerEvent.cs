using EBattleTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.MONST_STRONGER_EVENT_KEY)]
    public class MonstStrongerEvent : ModEvent
    {
        public static readonly IDictionary<MonstType, float> GROW_RATIO = new Dictionary<MonstType, float>
        {
            [MonstType.Common] = 0.0007f,
            [MonstType.Elite] = 0.0005f,
            [MonstType.BOSS] = 0.0005f,
        };

        public static readonly IDictionary<MonstType, float> KILL_GROW_RATIO = new Dictionary<MonstType, float>
        {
            [MonstType.Common] = 0,
            [MonstType.Elite] = 0.00004f,
            [MonstType.BOSS] = 0.0001f,
        };

        public IDictionary<MonstType, int> KillCounter = new Dictionary<MonstType, int>
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
                monstData.maxHP.baseValue += (int)(monstData.attack.baseValue * ratio);
            }
        }
    }
}