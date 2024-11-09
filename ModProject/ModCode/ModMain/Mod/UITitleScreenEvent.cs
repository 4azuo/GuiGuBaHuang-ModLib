using EGameTypeData;
using ModLib.Mod;
using UnityEngine;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache("UITitleScreenEvent", IsGlobal = true)]
    public class UITitleScreenEvent : ModEvent
    {
        private Button BtnModTitle;

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            var uiLogin = g.ui.GetUI<UILogin>(UIType.Login);
            if (uiLogin != null)
            {
                if (BtnModTitle == null)
                {
                    BtnModTitle = uiLogin.btnPaperChange.Create();

                    var modTitleText = BtnModTitle.GetComponentInChildren<Text>();
                    modTitleText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    modTitleText.verticalOverflow = VerticalWrapMode.Overflow;
                    modTitleText.alignment = TextAnchor.MiddleCenter;
                    modTitleText.fontSize = 22;
                    modTitleText.text = "Taoist [Hardcore]";
                }
                else
                {
                    BtnModTitle.transform.position = new Vector3(0f, 4f);
                }
            }
        }
    }
}
