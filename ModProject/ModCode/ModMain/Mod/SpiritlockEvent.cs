using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SPIRITLOCK_EVENT)]
    public class SpiritlockEvent : ModEvent
    {
        public static int[] VENERABLE_TORTOISES = new int[]
        {
            12005,
            12006,
            12007,
            12008,
            12009,
            12010,
            12011
        };

        public const int SPIRITLOCK_CIRCLE = 12012;

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            base.OnIntoBattleFirst(e);
            var data = e.data.TryCast<UnitDataMonst>();
            if (data != null)
            {
                if (VENERABLE_TORTOISES.Contains(data.unitAttrItem.id))
                {
                    e.data.moveSpeed.baseValue = 0;
                    e.data.defense.baseValue /= 2;
                    e.data.maxHP.baseValue /= 2;
                }
                else if (data.unitAttrItem.id == SPIRITLOCK_CIRCLE)
                {
                    e.data.moveSpeed.baseValue = 0;
                }
            }
        }
    }
}
