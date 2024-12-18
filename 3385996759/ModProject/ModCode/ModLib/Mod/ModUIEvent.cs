using System.Threading;
using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$UI$", OrderIndex = 10001, CacheType = CacheAttribute.CType.Global)]
    public class ModUIEvent : ModEvent
    {
        [ErrorIgnore]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            foreach (var ui in UIHelper.UIs)
            {
                ui.UpdateUI();
            }
        }
    }
}
