using EGameTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Enum;
using ModLib.Enum;
using ModLib.Mod;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UPGRADE_NATURALLY_EVENT)]
    public class UpgradeNaturallyEvent : ModEvent
    {
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.UpGrade.uiName)
            {

            }
        }
    }
}
