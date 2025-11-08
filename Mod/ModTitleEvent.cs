using EGameTypeData;
using ModLib.Const;
using ModLib.Object;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace ModLib.Mod
{
    [Cache("$TITLES$", OrderIndex = 90, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
    public class ModTitleEvent : ModEvent
    {
        public static ModTitleEvent Instance { get; set; }

        public static List<string> TranslateCode { get; } = new List<string>
        {
            null,
            "de",
            "es",
            "fr",
            "ja",
            "la",
            "ru",
            "vi",
            "th"
        };

        public int TranslateIndex { get; set; } = 0;

        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            LoadLocalTexts(GetTranslateLanguage());
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                if (!ValueHelper.NearlyEqual(g.data.globle.gameSetting.screenWidth.Parse<float>() / g.data.globle.gameSetting.screenHeight.Parse<float>(), ModLibConst.SUPPORT_SCREEN_RATIO, 0.001))
                {
                    var uiWarning = g.ui.OpenUISafe<UITextInfo>(UIType.TextInfo);
                    uiWarning.InitData(GameTool.LS("libtxt999990000"), GameTool.LS("libtxt999990001"));
                }

                var ui = new UICover<UILogin>(UIType.Login);
                {
                    var parentTransform = ui.UI.btnSet.transform.parent;
                    ui.AddButton(ui.LastCol - 3, ui.FirstRow + 2, () => Process.Start(ModLibConst.MODLIB_WEBSITE), $"Powered by\nFouru's ModLib {ModMaster.ModObj.Version}")
                        .Align(TextAnchor.MiddleCenter)
                        .Format(Color.black, 18)
                        .Size(300, 80)
                        .SetParentTransform(parentTransform);

                    var panelLangInit = ui.AddButton(ui.LastCol - 10, ui.FirstRow + 2, null, string.Empty)
                        .Size(320, 80)
                        .SetParentTransform(parentTransform);
                    var slLang = ui.AddCompositeSelect(ui.LastCol - 12, ui.FirstRow + 2, "Language:", new string[] { "Default", "German", "Spanish", "French", "Japanese", "Latin", "Russian", "Vietnamese", "Thai" }, TranslateIndex)
                        .SetParentTransform(panelLangInit.Component.transform) as UIItemComposite;
                    (slLang.MainComponent as UIItemSelect)
                        .Size(160, 24)
                        .SetWork(new UIItemWork
                        {
                            ChangeAct = Helper.ActionHelper.WTracedAction<UIItemBase, object>((a, b) =>
                            {
                                TranslateIndex = b.Parse<int>();
                                CacheHelper.SaveGlobalCache(this);
                                LoadLocalTexts(GetTranslateLanguage());
                            })
                        });
                }
                ui.UpdateUI();
            }
        }

        public static string GetTranslateLanguage()
        {
            return TranslateCode[Instance.TranslateIndex];
        }

        public static void LoadLocalTexts(string transCode)
        {
            if (transCode == null)
                transCode = string.Empty;
            foreach (var mod in g.mod.allModPaths)
            {
                if (g.mod.IsLoadMod(mod.t1))
                {
                    //load configs
                    ConfHelper.LoadCustomConf(mod.t1, transCode);
                }
            }
        }
    }
}
