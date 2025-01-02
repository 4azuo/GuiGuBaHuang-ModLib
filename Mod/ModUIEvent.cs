using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$UI$", OrderIndex = 10, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class ModUIEvent : ModEvent
    {
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            DebugHelper.WriteLine(UIHelper.UIs.Count.ToString());
            DebugHelper.Save();
            foreach (var ui in UIHelper.UIs.ToArray())
            {
                try
                {
                    if (!g.ui.HasUI(ui.UIBase.uiType))
                        ui?.Dispose();
                    else if (ui.IsAutoUpdate)
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
