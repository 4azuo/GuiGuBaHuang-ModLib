using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Mod;
using ModLib.Object;
using ModLib.Helper;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Title screen: version button (opens the cache folder), Workshop description, ModLib version check.
    /// </summary>
    [Cache(ModConst.UI_TITLE_SCREEN_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
    public class UITitleScreenEvent : ModEvent
    {
        /// <summary>
        /// Login UI: adds the button, shows news and warns when ModLib is too old.
        /// </summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                var ui = new UICover<UILogin>(e.ui);
                var parentTransform = ui.UI.btnSet.transform.parent;
                VersionButton = ui.AddButton(ui.MidCol, ui.FirstRow + 2, () => Process.Start("explorer.exe", CacheHelper.GetCacheFolderName(ModId)), $"Taoist {ModConst.TAOIST_VERSION}", ui.UI.btnPaperChange);
                VersionButton.Align(TextAnchor.MiddleCenter)
                    .Format(Color.white, 22)
                    .SetParentTransform(parentTransform);
                StripGameText(VersionButton);

                var uiInfo = g.ui.OpenUISafe<UITextInfoLong>(UIType.TextInfoLong);
                uiInfo.InitData(GameTool.LS("other500020046"), GetWorkshopDescription());

                if (!CompareVersion(ModConst.MODLIB_REQUIRED_VERSION, ModMaster.ModObj.Version))
                {
                    var uiWarning = g.ui.OpenUISafe<UITextInfo>(UIType.TextInfo);
                    uiWarning.InitData(GameTool.LS("other500020022"), ModTextHelper.LSF("ui_modlib_too_old", ModConst.TAOIST_VERSION, ModMaster.ModObj.Version, ModConst.MODLIB_REQUIRED_VERSION));
                }
            }
        }

        /// <summary>The version button of the title screen (tests).</summary>
        public static UIItemButton VersionButton { get; private set; }

        /// <summary>
        /// A button copied from a game button keeps the game's own components on its label (the game sets their text again
        /// when the title screen comes back from the character creation: the label went blank). Only the Unity ones stay.
        /// </summary>
        public static void StripGameText(UIItemButton btn)
        {
            var label = btn?.ButtonLabel;
            if (label == null) return;
            foreach (var comp in label.gameObject.GetComponents<Component>())
            {
                var name = comp.GetIl2CppType().Name;
                if (name == "Text" || name == "RectTransform" || name == "CanvasRenderer" || name == "Outline" || name == "Shadow" || name.StartsWith("TextMeshPro"))
                    continue;
                DebugHelper.WriteLine($"StripGameText: {name} removed from {label.text}");
                UnityEngine.Object.Destroy(comp);
            }
        }

        /// <summary>The labels of the mod's title buttons are set again when the title screen shows (after the character creation).</summary>
        [ModLib.Attributes.ErrorIgnore]
        public override void OnTimeUpdate1000ms()
        {
            base.OnTimeUpdate1000ms();
            if (!g.ui.HasUI(UIType.Login)) return;
            RefreshLabel(VersionButton, $"Taoist {ModConst.TAOIST_VERSION}");
            RefreshLabel(SMGlobalConfigsEvent.TitleButton, SMGlobalConfigsEvent.TITLE);
        }

        public static void RefreshLabel(UIItemButton btn, string text)
        {
            if (btn?.ButtonLabel == null) return;
            if (string.IsNullOrEmpty(btn.ButtonLabel.text)) btn.Set(text);
        }

        /// <summary>
        /// True when curVersion &gt;= reqVersion (numeric parts).
        /// </summary>
        private bool CompareVersion(string reqVersion, string curVersion)
        {
            var reqIndexes = Regex.Split(reqVersion, @"\D");
            var curIndexes = Regex.Split(curVersion, @"\D");
            for (int i = 0; i < reqIndexes.Length && i < curIndexes.Length; i++)
            {
                var reqIndex = reqIndexes[i].Parse<int>();
                var curIndex = curIndexes[i].Parse<int>();
                if (curIndex < reqIndex)
                    return false;
                if (curIndex > reqIndex)
                    return true;
            }
            return true;
        }

        /// <summary>
        /// Downloads the Workshop description.
        /// </summary>
        private string GetWorkshopDescription()
        {
            return HttpHelper.GetWorkshopDescription("https://steamcommunity.com/sharedfiles/filedetails/?id=3161035078");
        }
    }
}
