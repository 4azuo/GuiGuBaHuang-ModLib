using EGameTypeData;
using ModLib.Mod;
using UnityEngine;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache("UITitleScreenEvent", IsGlobal = true)]
    public class UITitleScreenEvent : ModEvent
    {
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var uiLogin = g.ui.GetUI<UILogin>(UIType.Login);
                var modTitleBtn = uiLogin.btnPaperChange.Create().Pos(0f, 4f, uiLogin.btnPaperChange.transform.position.z);
                var modTitleText = modTitleBtn.GetComponentInChildren<Text>().Align(TextAnchor.MiddleCenter).Format(Color.white, 22);
                modTitleText.text = "Taoist [Hardcore]";
            }
        }
    }
}
