using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$UI$", OrderIndex = 10, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
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
