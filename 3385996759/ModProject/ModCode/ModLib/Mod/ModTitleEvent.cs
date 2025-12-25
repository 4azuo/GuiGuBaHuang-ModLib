using EGameTypeData;
using MelonLoader;
using ModLib.Attributes;
using ModLib.Const;
using ModLib.Helper;
using ModLib.Object;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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
                        .Size(330, 80)
                        .SetParentTransform(parentTransform);
                    var slLang = ui.AddCompositeSelect(ui.Columns[ui.LastCol - 12], ui.Rows[ui.FirstRow + 2] + UIHelper.GetUIDeltaY() / 2, "Language:", new string[] { "Default", "German", "Spanish", "French", "Japanese", "Latin", "Russian", "Vietnamese", "Thai" }, TranslateIndex)
                        .SetParentTransform(panelLangInit.Component.transform) as UIItemComposite;
                    (slLang.MainComponent as UIItemSelect)
                        .Size(160, 24)
                        .SetWork(new UIItemWork
                        {
                            ChangeAct = ActionHelper.WTracedAction<UIItemBase, object>((a, b) =>
                            {
                                TranslateIndex = b.Parse<int>();
                                CacheHelper.SaveGlobalCache(this);
                                LoadLocalTexts(GetTranslateLanguage());
                            })
                        });
                    var modCreatorBtn = ui.AddButton(ui.Columns[ui.LastCol - 10], ui.Rows[ui.FirstRow + 2] - UIHelper.GetUIDeltaY() / 2, () =>
                    {
                        var miniWindow = new UICustom2(GameTool.LS("libtxt999990009"));
                        {
                            var col = miniWindow.FirstCol + 1;
                            var txtCol = col + 3;
                            var row = miniWindow.FirstRow;
                            //check duplicated IDs
                            miniWindow.AddButton(col, row, ShowCheckedIDs, GameTool.LS("libtxt999990010"));
                            miniWindow.AddText(txtCol, row, GameTool.LS("libtxt999990011")).Align();
                            //export assets
                            //row += 2;
                            //miniWindow.AddButton(col, row, ExportAssets, GameTool.LS("libtxt999990012"));
                            //miniWindow.AddText(txtCol, row, GameTool.LS("libtxt999990013")).Align();
                        }
                    }, GameTool.LS("libtxt999990009"))
                        .Align(TextAnchor.MiddleCenter)
                        .Size(200, 24)
                        .SetParentTransform(panelLangInit.Component.transform);
                }
                ui.UpdateUI();
            }
        }

        private void ShowCheckedIDs()
        {
            var listWindow = new UICustom1(GameTool.LS("libtxt999990010"));
            {
                listWindow.IsShowNavigationButtons = true;
                var i = 0;
                var maxRowsPerPage = listWindow.Rows.Count - 1;
                foreach (var dupID in ConfHelper.LoadedIDs)
                {
                    var dupEntry = dupID.Value.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
                    var color = ConfHelper.ReplacedBaseIDs.Contains(dupID.Key) && dupEntry.Length == 1 ? Color.blue : (dupEntry.Length > 1 ? Color.red : Color.black);
                    if (color == Color.black)
                        continue;
                    if (i % maxRowsPerPage == 0)
                        listWindow.AddPage();
                    listWindow.AddText(listWindow.FirstCol, listWindow.FirstRow + (i % maxRowsPerPage), i.ToString("0000")).Align();
                    listWindow.AddText(listWindow.FirstCol + 2, listWindow.FirstRow + (i % maxRowsPerPage), dupID.Key).Align();
                    listWindow.AddText(listWindow.FirstCol + 9, listWindow.FirstRow + (i % maxRowsPerPage), dupID.Value)
                        .Align().Format(color, 14);
                    i++;
                }
                listWindow.Pages.FirstOrDefault()?.Active();
            }
        }

        //private void ExportAssets()
        //{
        //    foreach (var r in g.res.allRes)
        //    {
        //        DebugHelper.WriteLine($"{r.key}");
        //    }
        //    DebugHelper.Save();
        //    return;

        //    var folder = @"C:\Users\hiros\OneDrive\Desktop\新しいフォルダー";
        //    //using (var dialog = new FolderBrowserDialog())
        //    {
        //        Texture2D[] textures = Resources.FindObjectsOfTypeAll<Texture2D>();

        //        MelonLogger.Msg($"Found {textures.Length} textures");

        //        foreach (var tex in textures)
        //        {
        //            if (tex.width <= 4 || tex.height <= 4)
        //                continue;

        //            try
        //            {
        //                var path = Path.Combine(folder, Sanitize(tex.name) + ".png");
        //                ExportTexture(tex, path);
        //                DebugHelper.WriteLine($"Exported texture: {tex.name} to {path}");
        //            }
        //            catch { }
        //        }
        //    }
        //    DebugHelper.Save();
        //}

        void ExportTexture(Texture2D tex, string path)
        {
            RenderTexture rt = RenderTexture.GetTemporary(
                tex.width, tex.height, 0, RenderTextureFormat.ARGB32);

            Graphics.Blit(tex, rt);
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D readable = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            readable.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            var raw = readable.GetRawTextureData<byte>();
            var rgba = EnumerableHelper.NativeToManaged(raw);
            var png = PngEncodeHelper.EncodeRGBA(rgba, (uint)tex.width, (uint)tex.height);
            File.WriteAllBytes(path, png);
        }

        string Sanitize(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return string.IsNullOrEmpty(name) ? "unnamed" : name;
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
