using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.TIME_SKIP_EVENT)]
    public class TimeSkipEvent : ModEvent
    {
        private static UICover<UIMapMain> uiMain;
        private static int skip2Month = -1;

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                uiMain = new UICover<UIMapMain>(e.ui);
                {
                    var btnFateFeature = uiMain.UI.playerInfo.btnFateFeature ?? uiMain.UI.playerInfo.btnFateFeature_En;
                    uiMain.AddButton(btnFateFeature.transform.position.x + 3f, btnFateFeature.transform.position.y, () =>
                    {
                        g.ui.MsgBox(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020029"), 1), MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTime(12);
                        });
                    }, string.Format(GameTool.LS("other500020030"), 1)).Size(140, 40);
                    //uiMain.AddButton(btnFateFeature.transform.position.x + 5f, btnFateFeature.transform.position.y, () =>
                    //{
                    //    g.ui.MsgBox(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020029"), 10), MsgBoxButtonEnum.YesNo, () =>
                    //    {
                    //        SkipTime(120);
                    //    });
                    //}, string.Format(GameTool.LS("other500020030"), 10)).Size(140, 40);
                }
            }
        }

        public static void SkipTime(int month)
        {
            skip2Month = GameHelper.GetGameTotalMonth() + month;
        }

        public override void OnTimeUpdate1s()
        {
            base.OnTimeUpdate1s();
            if (g.ui.HasUI(UIType.MapMain) && uiMain != null &&
                GameHelper.GetGameTotalMonth() < skip2Month && !g.world.run.isRunning && g.world.run.IsCanRun())
            {
                uiMain.UI.playerInfo.btnNextMonth.onClick.Invoke();
            }
        }
    }
}
