using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UI_TITLE_SCREEN_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
    public class UITitleScreenEvent : ModEvent
    {
        [EventCondition(IsInGame = 0)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var uiLogin = g.ui.GetUI<UILogin>(UIType.Login);
                var modTitleBtn = uiLogin.btnPaperChange.Copy().Pos(0f, 4.5f).Align(TextAnchor.MiddleCenter).Format(Color.white, 22).Set("Taoist");
                modTitleBtn.onClick.AddListener((UnityAction)(() =>
                {
                    Process.Start("explorer.exe", CacheHelper.GetCacheFolderName());
                }));
            }
        }
    }
}
