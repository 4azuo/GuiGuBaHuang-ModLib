using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using ModLib.Object;
using System.Diagnostics;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UI_TITLE_SCREEN_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
    public class UITitleScreenEvent : ModEvent
    {
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var ui = new UICover<UILogin>(e.ui);
                {
                    var parentTransform = ui.UI.btnSet.transform.parent;
                    ui.AddButton(ui.MidCol, ui.FirstRow, () => Process.Start("explorer.exe", CacheHelper.GetCacheFolderName(ModId)), $"Taoist {ModConst.TAOIST_VERSION}", ui.UI.btnPaperChange)
                        .Align(TextAnchor.MiddleCenter)
                        .Format(Color.white, 22)
                        .SetParentTransform(parentTransform);
                }
                ui.UpdateUI();
            }
        }
    }
}
