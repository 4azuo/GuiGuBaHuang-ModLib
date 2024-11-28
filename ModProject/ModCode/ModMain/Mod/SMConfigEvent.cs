using EGameTypeData;
using MOD_nE7UL2.Const;
using System.Linq;
using ModLib.Mod;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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
                }));
            }
        }
    }
}
