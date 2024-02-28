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

        public int Counter { get; set; } = 0;

        public override void OnMonthly()
        {
            Counter++;
        }

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            var monstData = e?.data?.TryCast<UnitDataMonst>();
            if (monstData != null && GROW_RATIO.ContainsKey(monstData.monstType))
            {
                var ratio = Counter * GROW_RATIO[monstData.monstType];
                monstData.attack.baseValue += (int)(monstData.attack.baseValue * ratio);
                monstData.maxHP.baseValue += (int)(monstData.attack.baseValue * ratio);
            }
        }
    }
}