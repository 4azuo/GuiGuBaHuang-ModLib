using EGameTypeData;
using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$UI$", OrderIndex = 10, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class ModUIEvent : ModEvent
    {
        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);
            ClearUnuseUIs();
        }

        private void ClearUnuseUIs()
        {
            foreach (var ui in UIHelper.UIs.ToArray())
            {
                if (!g.ui.HasUI(ui.UIBase.uiType))
                    ui.Dispose();
            }
        }

        [EventCondition(IsInGame = Enum.HandleEnum.Ignore, IsInBattle = Enum.HandleEnum.Ignore)]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            foreach (var ui in UIHelper.UIs.ToArray())
            {
                try
                {
                    if (ui.IsAutoUpdate)
                        ui?.UpdateUI();
                }
                catch
                {
                    //ui?.Dispose();
                }
            }
        }
    }
}
