using ModLib.Attributes;
using UnityEngine;
using ModLib.Helper;

namespace ModLib.Mod
{
    [Cache("$UI$", OrderIndex = 10, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class ModUIEvent : ModEvent
    {
        [ErrorIgnore]
        public override void OnTimeUpdate100ms()
        {
            base.OnTimeUpdate100ms();
            UIHelper.UpdateAllUI(x => x.IsAutoUpdate);
        }
    }
}
