using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$UI$", OrderIndex = 10001, CacheType = CacheAttribute.CType.Global)]
    public class ModUIEvent : ModEvent
    {
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            foreach (var ui in UIHelper.UIs.ToArray())
            {
                try
                {
                    ui?.UpdateUI();
                }
                catch
                {
                    ui?.Dispose();
                }
            }
        }
    }
}
