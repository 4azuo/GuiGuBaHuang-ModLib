using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SM_CONFIG_EVENT, IsGlobal = true)]
    public class SMConfigEvent : ModEvent
    {
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var uiLogin = g.ui.GetUI<UILogin>(UIType.Login);
                var modConfigBtn = uiLogin.btnSet.Create().Pos(0f, 3.3f, uiLogin.btnPaperChange.transform.position.z);
                var modConfigText = modConfigBtn.GetComponentInChildren<Text>().Align(TextAnchor.MiddleCenter);
                modConfigText.text = "S&M Configs";

                modConfigBtn.onClick.AddListener((UnityAction)(() =>
                {
                    var ui = g.ui.OpenUI<UITextInfo>(UIType.TextInfo);
                    ui.InitData("S&M Configs", string.Empty);
                    var txtAtk = ui.canvas.gameObject.AddComponent<Text>();//.Pos(ui.textTitle.gameObject, -1f, -0.5f).Align(TextAnchor.MiddleCenter).Format();
                    var txtDef = ui.canvas.gameObject.AddComponent<Text>();//.Pos(ui.textTitle.gameObject, -1f, -1.0f).Align(TextAnchor.MiddleCenter).Format();
                    var txtHp = ui.canvas.gameObject.AddComponent<Text>();//.Pos(ui.textTitle.gameObject, -1f, -1.5f).Align(TextAnchor.MiddleCenter).Format();
                    var slAtk = ui.canvas.gameObject.AddComponent<Slider>();//.Pos(ui.textTitle.gameObject, -0.5f, -0.5f);
                    var slDef = ui.canvas.gameObject.AddComponent<Slider>();//.Pos(ui.textTitle.gameObject, -0.5f, -1.0f);
                    var slHp = ui.canvas.gameObject.AddComponent<Slider>();//.Pos(ui.textTitle.gameObject, -0.5f, -1.5f);
                }));
            }
        }
    }
}
