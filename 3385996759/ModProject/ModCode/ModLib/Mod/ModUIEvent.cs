﻿using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$UI$", OrderIndex = 10, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class ModUIEvent : ModEvent
    {
        public override void OnTimeUpdate100ms()
        {
            base.OnTimeUpdate100ms();
            UIHelper.UpdateAllUI(x => x.IsAutoUpdate);
        }
    }
}
