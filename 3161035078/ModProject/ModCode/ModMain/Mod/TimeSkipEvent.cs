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

        private int skip2Month = -1;

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                var uiMain = new UICover<UIMapMain>(e.ui);
                {
                    var btnFateFeature = uiMain.UI.playerInfo.btnFateFeature ?? uiMain.UI.playerInfo.btnFateFeature_En;
                    uiMain.AddButton(uiMain.FirstCol + 11, uiMain.LastRow - 4, () =>
                    {
                        g.ui.MsgBox(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020029"), 1), MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTime(12);
                        });
                    }, string.Format(GameTool.LS("other500020030"), 1)).Size(140, 40);
                    uiMain.AddButton(uiMain.FirstCol + 13, uiMain.LastRow - 4, () =>
                    {
                        g.ui.MsgBox(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020031"), 1), MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTimeWithoutEvents(12);
                        });
                    }, string.Format(GameTool.LS("other500020032"), 1)).Size(40, 40);
                    uiMain.AddButton(uiMain.FirstCol + 15, uiMain.LastRow - 4, () =>
                    {
                        g.ui.MsgBox(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020029"), 10), MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTime(120);
                        });
                    }, string.Format(GameTool.LS("other500020030"), 10)).Size(140, 40);
                    uiMain.AddButton(uiMain.FirstCol + 17, uiMain.LastRow - 4, () =>
                    {
                        g.ui.MsgBox(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020031"), 10), MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTimeWithoutEvents(12);
                        });
                    }, string.Format(GameTool.LS("other500020032"), 1)).Size(40, 40);
                }
            }
        }

        public void SkipTime(int month)
        {
            skip2Month = GameHelper.GetGameTotalMonth() + month;
        }

        public static void SkipTimeWithoutEvents(int month)
        {
            for (int i = 0; i < month; i++)
            {
                g.world.playerUnit.AddExp(g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Mp).value / 10 + g.world.playerUnit.GetDynProperty(UnitDynPropertyEnum.Sp).value / 2);
                g.world.playerUnit.AddProperty<int>(UnitPropertyEnum.Age, 1);
                foreach (var e in CacheHelper.GetAllCachableObjects<ModEvent>())
                {
                    e.OnMonthly();
                    foreach (var wunit in g.world.unit.GetUnits())
                    {
                        e.OnMonthlyForEachWUnit(wunit);
                    }
                    e.OnSave(null);
                }
                g.world.run.AddDay(30, i == month - 1);
            }
        }

        public override void OnTimeUpdate1000ms()
        {
            base.OnTimeUpdate1000ms();
            if (GameHelper.GetGameTotalMonth() < skip2Month && !g.world.run.isRunning && g.world.run.IsCanRun())
            {
                g.world.run.AddDay(30, true);
            }
        }
    }
}
