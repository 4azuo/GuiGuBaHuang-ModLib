using EGameTypeData;
using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$UI$", OrderIndex = 10, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class ModUIEvent : ModEvent
    {
        public override void OnOpenUIStart(OpenUIStart e)
        {
            base.OnOpenUIStart(e);
            ClearUnuseUIs();
        }

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

        [EventCondition(IsInBattle = Enum.HandleEnum.True, IsInGame = Enum.HandleEnum.True, IsWorldRunning = Enum.HandleEnum.False)]
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
