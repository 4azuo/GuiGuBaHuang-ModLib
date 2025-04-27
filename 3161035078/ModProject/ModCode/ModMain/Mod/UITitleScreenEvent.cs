using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using ModLib.Object;
using System.Diagnostics;
using System.Text.RegularExpressions;
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
                if (!CompareVersion(ModConst.MODLIB_REQUIRED_VERSION, ModMaster.ModObj.Version))
                {
                    var uiWarning = g.ui.OpenUISafe<UITextInfo>(UIType.TextInfo);
                    uiWarning.InitData(GameTool.LS("other500020022"), $"Taoist {ModConst.TAOIST_VERSION} is not supported in ModLib {ModMaster.ModObj.Version}!\nPlease install ModLib {ModConst.MODLIB_REQUIRED_VERSION} or above.");
                }

                var ui = new UICover<UILogin>(e.ui);
                {
                    var parentTransform = ui.UI.btnSet.transform.parent;
                    ui.AddButton(ui.MidCol, ui.FirstRow + 2, () => Process.Start("explorer.exe", CacheHelper.GetCacheFolderName(ModId)), $"Taoist {ModConst.TAOIST_VERSION}", ui.UI.btnPaperChange)
                        .Align(TextAnchor.MiddleCenter)
                        .Format(Color.white, 22)
                        .SetParentTransform(parentTransform);
                }
                ui.UpdateUI();

                var uiInfo = g.ui.OpenUISafe<UITextInfoLong>(UIType.TextInfoLong);
                uiInfo.InitData(GameTool.LS("other500020046"), GetWorkshopDescription());
            }
        }

        private bool CompareVersion(string reqVersion, string curVersion)
        {
            var reqIndexes = Regex.Split(reqVersion, @"\D");
            var curIndexes = Regex.Split(curVersion, @"\D");
            for (int i = 0; i < reqIndexes.Length && i < curIndexes.Length; i++)
            {
                var reqIndex = reqIndexes[i].Parse<int>();
                var curIndex = reqIndexes[i].Parse<int>();
                if (curIndex < reqIndex)
                    return true;
                if (curIndex > reqIndex)
                    return false;
            }
            return true;
        }

        private string GetWorkshopDescription()
        {
            return HttpHelper.GetWorkshopDescription("https://steamcommunity.com/sharedfiles/filedetails/?id=3161035078");
        }
    }
}
