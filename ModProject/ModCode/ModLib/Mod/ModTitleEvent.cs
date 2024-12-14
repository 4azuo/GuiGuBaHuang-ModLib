using EGameTypeData;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

namespace ModLib.Mod
{
    [Cache("$TITLES$", OrderIndex = 90, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
    public class ModTitleEvent : ModEvent
    {
        [EventCondition(IsInGame = 0)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var uiLogin = g.ui.GetUI<UILogin>(UIType.Login);
                using (var a = new UIHelper.UISample())
                {
                    var modTitleBtn = a.ui.btnKeyOK.Copy(uiLogin)
                        .Size(300, 60)
                        .Pos(8f, UIHelper.SCREEN_Y_TOP, uiLogin.btnExit.transform.position.z)
                        .Align(TextAnchor.MiddleCenter)
                        .Format(Color.black, 19)
                        .Set("Powered by Fouru's ModLib");
                    modTitleBtn.onClick.AddListener((UnityAction)(() =>
                    {
                        Process.Start("https://github.com/4azuo/GuiGuBaHuang-ModLib");
                    }));
                }
            }
        }
    }
}
