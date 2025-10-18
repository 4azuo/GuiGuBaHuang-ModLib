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
        public static TimeSkipEvent Instance { get; set; }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                var uiMain = new UICover<UIMapMain>(e.ui);
                {
                    var btnFateFeature = uiMain.UI.playerInfo.btnFateFeature ?? uiMain.UI.playerInfo.btnFateFeature_En;
                    uiMain.AddButton(uiMain.FirstCol + 12, uiMain.LastRow - 4, () =>
                    {
                        g.ui.MsgBox(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020029"), 1), MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTime(12);
                        });
                    }, string.Format(GameTool.LS("other500020030"), 1)).Size(140, 40);
                    uiMain.AddButton(uiMain.FirstCol + 15, uiMain.LastRow - 4, () =>
                    {
                        g.ui.MsgBox(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020029"), 10), MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTime(120);
                        });
                    }, string.Format(GameTool.LS("other500020030"), 10)).Size(140, 40);
                }
            }
        }

        public void SkipTime(int month)
        {
            if (month <= 0)
            {
                g.data.SaveData((Il2CppSystem.Action<bool>)((b) => { }));
                return;
            }
            ModDelayEvent.Instance.DelayEvent(this, () =>
            {
                g.world.run.AddDay(30);
                g.world.playerUnit.AddExp(g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Mp).value / 10 + g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Sp).value / 2);
                if (GameHelper.GetGameTotalMonth() % SMLocalConfigsEvent.Instance.Configs.GrowUpSpeed == 0)
                {
                    EventHelper.CallGameEvent<WorldRunStart>(EGameType.WorldRunStart);
                    EventHelper.CallGameEvent<WorldRunEnd>(EGameType.WorldRunEnd);
                }
                g.world.playerUnit.SetProperty<int>(UnitPropertyEnum.Age, 16 * 12 + GameHelper.GetGameTotalMonth());
                SkipTime(month - 1);
            }, 1);
        }
    }
}
