using EBattleTypeData;
using MOD_JhUKQ7.Const;
using ModLib.Mod;
using System.Collections.Generic;
using UnityEngine;

namespace MOD_JhUKQ7.Mod
{
    [Cache(ModConst.MONST_STRONGER_EVENT_KEY)]
    public sealed class MonstStrongerEvent : ModEvent
    {
        public override int OrderIndex => 1;

        public static readonly IDictionary<MonstType, float> GROW_RATIO = new Dictionary<MonstType, float>
        {
            [MonstType.Common] = 0.001f,
            [MonstType.Elite] = 0.0012f,
            [MonstType.BOSS] = 0.0015f,
        };

        public int Counter { get; set; } = 0;

        public override void OnMonthly()
        {
            Counter++;
        }

        public override void OnUnitInit(UnitInit e)
        {
            if (e.unit.data.unitType == UnitType.Monst)
            {
                var monstData = e.unit.data.TryCast<UnitDataMonst>();
                if (monstData != null && GROW_RATIO.ContainsKey(monstData.monstType))
                {
                    monstData.attack.baseValue += (int)(monstData.attack.baseValue * (Counter * GROW_RATIO[monstData.monstType]));
                    monstData.maxHP.baseValue += (int)(monstData.attack.baseValue * (Counter * GROW_RATIO[monstData.monstType]));
                }
            }
        }
    }
}