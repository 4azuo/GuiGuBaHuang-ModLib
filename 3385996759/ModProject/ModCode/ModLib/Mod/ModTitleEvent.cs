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
            "ja",
            "vi",
            "ru",
            "la",
            "es",
        };

        public int TranslateIndex { get; set; } = 0;
        public bool FirstTime { get; set; } = true;

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
                    var uiWarning = g.ui.OpenUI<UITextInfo>(UIType.TextInfo);
                    uiWarning.InitData(GameTool.LS("libtxt999990000"), GameTool.LS("libtxt999990001"));
                }
                if (FirstTime)
                {
                    Benchmark(true);
                }

                var ui = new UICover<UILogin>(UIType.Login);
                {
                    var parentTransform = ui.UI.btnSet.transform.parent;
                    ui.AddButton(ui.LastCol - 5, ui.FirstRow, () => Process.Start(ModLibConst.MODLIB_WEBSITE), $"Powered by\nFouru's ModLib {ModMaster.ModObj.Version}")
                        .Align(TextAnchor.MiddleCenter)
                        .Format(Color.black, 18)
                        .Size(300, 80)
                        .SetParentTransform(parentTransform);

                    var panelLangInit = ui.AddButton(ui.LastCol - 14, ui.FirstRow, null, string.Empty)
                        .Size(320, 80)
                        .SetParentTransform(parentTransform);
                    var slLang = ui.AddCompositeSelect(ui.Columns[ui.LastCol - 17] + 0.15f, ui.Rows[ui.FirstRow] + 0.11f, "Language:", new string[] { "Default", "Japanese", "Vietnamese", "Russian", "Latin", "Spanish" }, TranslateIndex)
                        .SetParentTransform(panelLangInit.Component.transform) as UIItemComposite;
                    (slLang.MainComponent as UIItemSelect)
                        .Size(160, 24)
                        .SetWork(new UIItemWork
                        {
                            ChangeAct = (a, b) =>
                            {
                                TranslateIndex = b.Parse<int>();
                                CacheHelper.SaveGlobalCache(this);
                                LoadLocalTexts(GetTranslateLanguage());
                            }
                        })
                        .SetParentTransform(parentTransform);
                    ui.AddButton(ui.Columns[ui.LastCol - 14], ui.Rows[ui.FirstRow] - 0.11f, () => Benchmark(false), "Benchmark")
                        .Align(TextAnchor.MiddleCenter)
                        .Format(Color.black, 15)
                        .Size(160, 24)
                        .SetParentTransform(panelLangInit.Component.transform);
                }
                ui.UpdateUI();
            }
        }

        public static void Benchmark(bool firsttime)
        {
            g.ui.MsgBox(string.Empty, firsttime ? GameTool.LS("libtxt999990004") : GameTool.LS("libtxt999990002"), Enum.MsgBoxButtonEnum.YesNo, () =>
            {
                Instance.FirstTime = false;
                CacheHelper.SaveGlobalCache(Instance);

                var ui = new UICover<UILogin>(UIType.Login);
                var parentTransform = ui.UI.btnSet.transform.parent;
                for (int i = 0; i < 10000; i++)
                {
                    ui.AddButton(CommonTool.Random(ui.FirstCol, ui.LastCol), CommonTool.Random(ui.FirstRow, ui.LastRow), null, string.Empty).SetParentTransform(parentTransform);
                }
                g.ui.MsgBox(string.Empty, GameTool.LS("libtxt999990003"), onYesCall: () =>
                {
                    Application.Quit();
                });
            });
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
