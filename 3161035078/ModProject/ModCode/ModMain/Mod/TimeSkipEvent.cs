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
        private static int skip2Month = -1;

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                var ui = new UICover<UIMapMain>(e.ui);
                {
                    var btnFateFeature = ui.UI.playerInfo.btnFateFeature ?? ui.UI.playerInfo.btnFateFeature_En;
                    ui.AddButton(btnFateFeature.transform.position.x + 3f, btnFateFeature.transform.position.y, () =>
                    {
                        g.ui.MsgBox(GameTool.LS("other500020022"), "Do you really wanna skip 1 year?", MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTime(12);
                        });
                    }, "Skip 1 year").Size(140, 40);
                    ui.AddButton(btnFateFeature.transform.position.x + 5f, btnFateFeature.transform.position.y, () =>
                    {
                        g.ui.MsgBox(GameTool.LS("other500020022"), "Do you really wanna skip 10 year?", MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTime(120);
                        });
                    }, "Skip 10 year").Size(140, 40);
                }
            }
        }

        public static void SkipTime(int month)
        {
            skip2Month = (GameHelper.GetGameTotalMonth() + month) * 30;
        }

        public override void OnTimeUpdate1s()
        {
            base.OnTimeUpdate1s();
            if (GameHelper.GetGameTotalMonth() < skip2Month && !g.world.run.isRunning && g.world.run.IsCanRun())
            {
                //g.world.run.RunWorld(g.world.run.on);
            }
        }
    }
}
