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
        [EventCondition(IsInGame = 0)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var ver = g.mod.GetModProjectData(ModMain.ModObj.ModId).ver;
                var uiLogin = new UICover<UILogin>(UIType.Login, (ui) =>
                {
                    ui.AddButton(ui.MidCol, ui.FirstRow, () => Process.Start("explorer.exe", CacheHelper.GetCacheFolderName(ModId)), $"Taoist {ver}", ui.UI.btnPaperChange);
                });

                //var isLast = g.mod.allModPaths.ToArray().Last().t1 == ModMain.ModObj.ModId;
                //if (!isLast)
                //{
                //    var uiWarning = g.ui.OpenUI<UITextInfo>(UIType.TextInfo);
                //    uiWarning.InitData("Warning", "Taoist is not the last mod in mod-list!");
                //}
            }
        }
    }
}
