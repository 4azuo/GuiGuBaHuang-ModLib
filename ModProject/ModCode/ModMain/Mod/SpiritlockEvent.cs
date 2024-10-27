using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SPIRITLOCK_EVENT)]
    public class SpiritlockEvent : ModEvent
    {
        public static int[] VenerableTortoises = new int[]
        {
            12005,
            12006,
            12007,
            12008,
            12009,
            12010,
            12011
        };

        public static int SpiritlockCircle = 12012;

        public override void OnIntoBattleFirst(UnitCtrlBase e)
        {
            base.OnIntoBattleFirst(e);
            var data = e.data.TryCast<UnitDataMonst>();
            if (data != null)
            {
                if (VenerableTortoises.Contains(data.unitAttrItem.id))
                {
                    e.data.moveSpeed.baseValue = 0;
                    e.data.defense.baseValue /= 2;
                    e.data.maxHP.baseValue /= 2;
                }
                else if (data.unitAttrItem.id == SpiritlockCircle)
                {
                    e.data.moveSpeed.baseValue = 0;
                }
            }
        }
    }
}
