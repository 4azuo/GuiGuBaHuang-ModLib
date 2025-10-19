using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using ModLib.Object;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UI_SETTING_SCREEN_EVENT)]
    public class UISettingScreenEvent : ModEvent
    {
        public static UISettingScreenEvent Instance { get; set; }

        public bool HideDamageMultiplier { get; set; } = false;
        public bool HideExceptionLogs { get; set; } = false;

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            DebugHelper.IsDebugMode = !HideExceptionLogs;
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.GameSetting.uiName)
            {
                var ui = new UICover<UIGameSetting>(e.ui);
                {
                    ui.AddButton(ui.MidCol, ui.MidRow - 11, () =>
                    {
                        var uiTaoistSetting = new UICustom2(GameTool.LS("setting000"));
                        {
                            uiTaoistSetting.AddText(uiTaoistSetting.MidCol, uiTaoistSetting.LastRow, GameTool.LS("setting001")).Format(Color.red, 17);
                            uiTaoistSetting.AddCompositeToggle(uiTaoistSetting.MidCol, uiTaoistSetting.FirstRow, GameTool.LS("setting002"), HideDamageMultiplier).SetWork(new UIItemWork
                            {
                                ChangeAct = new ModLib.Helper.ActionHelper.TracedAction<UIItemBase, object>((s, v) =>
                                {
                                    HideDamageMultiplier = v.Parse<bool>();
                                })
                            });
                            uiTaoistSetting.AddCompositeToggle(uiTaoistSetting.MidCol, uiTaoistSetting.FirstRow + 1, GameTool.LS("setting003"), !DebugHelper.IsDebugMode).SetWork(new UIItemWork
                            {
                                ChangeAct = new ModLib.Helper.ActionHelper.TracedAction<UIItemBase, object>((s, v) =>
                                {
                                    DebugHelper.IsDebugMode = !v.Parse<bool>();
                                    HideExceptionLogs = !DebugHelper.IsDebugMode;
                                })
                            });
                            uiTaoistSetting.AddText(uiTaoistSetting.MidCol, uiTaoistSetting.FirstRow + 2, GameTool.LS("setting004")).Format(null, 13);
                        }
                    }, GameTool.LS("setting000"), ui.UI.btnSystemOK).Size(200, 40);
                }
            }
        }
    }
}
