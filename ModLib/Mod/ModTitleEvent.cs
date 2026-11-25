using EGameTypeData;
using MelonLoader;
using ModLib.Attributes;
using ModLib.Const;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Object;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModLib.Mod
{
    /// <summary>
    /// Built-in title screen extension (Global, order 90): ModLib credit button, language selector (translated LocalText)
    /// and the ModCreator tools window (duplicated config id check).
    /// </summary>
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

        /// <summary>The player's answer about error reports (CrashReportHelper): asked once on the title screen.</summary>
        public CrashReportConsentEnum CrashReportConsent { get; set; } = CrashReportConsentEnum.NotAsked;

        /// <summary>
        /// What the answer was given for (CrashReportHelper.ConsentKey: consent version + mods sending reports): another
        /// key = asked again (a new mod with a database, more data sent).
        /// </summary>
        public string CrashReportConsentKey { get; set; }

        /// <summary>Anonymous random id sent with the error reports (groups the reports of one player).</summary>
        public string CrashReportUserId { get; set; }

        /// <summary>Mod wallpaper chosen in the Change Theme window ("modId/file", WallpaperHelper); null = the game's.</summary>
        public string Wallpaper { get; set; }

        /// <summary>
        /// The player's choice: the game's title (logo) hidden on the title screen while a mod wallpaper is shown (its own
        /// title); null = the setting of the wallpaper's pack (hideGameTitle of its "*wallpaper.json").
        /// </summary>
        public bool? HideGameTitle { get; set; }

        /// <summary>
        /// Loads the LocalText of the saved translation language.
        /// </summary>
        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            LoadLocalTexts(GetTranslateLanguage());
        }

        /// <summary>
        /// On the Login UI: warns about unsupported screen ratio and adds the ModLib/language/tools buttons.
        /// </summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.WallPaper.uiName)
            {
                //the items of the game are made by InitData, after the opening: the mod wallpapers one frame later
                g.timer.Frame(ActionHelper.TracedIl2Action(() =>
                {
                    var n = WallpaperHelper.AddToThemeWindow(g.ui.GetUI<UIWallPaper>(UIType.WallPaper));
                    DebugHelper.WriteLine($"Change Theme: {n} mod wallpaper(s) added");
                    if (n > 0)
                        AddHideTitleToggle();
                }), 1, false);
            }
            if (e.uiType.uiName == UIType.Login.uiName)
            {
                if (!ValueHelper.NearlyEqual(g.data.globle.gameSetting.screenWidth.Parse<float>() / g.data.globle.gameSetting.screenHeight.Parse<float>(), ModLibConst.SUPPORT_SCREEN_RATIO, 0.001))
                {
                    UICustom2.WhenFree(() => g.ui.OpenUISafe<UITextInfo>(UIType.TextInfo).InitData(GameTool.LS("libtxt999990000"), GameTool.LS("libtxt999990001")));
                }

                var ui = new UICover<UILogin>(UIType.Login);
                {
                    var parentTransform = ui.UI.btnSet.transform.parent;
                    //the frame of the game's DLC buttons (without the DLC icon and the "Claimed" mark); the old frame when missing
                    var frame = GetTitleFrameButton(ui.UI);
                    ui.AddButton(ui.LastCol - 3, ui.FirstRow + 2, () => Process.Start(ModLibConst.MODLIB_WEBSITE), string.Format(GameTool.LS("libtxt999990014"), ModMaster.ModObj.Version), frame)
                        .Align(TextAnchor.MiddleCenter)
                        .Format(Color.black, 18)
                        .Size(300, 80)
                        .SetParentTransform(parentTransform);

                    var panelLangInit = ui.AddButton(ui.LastCol - 10, ui.FirstRow + 2, null, string.Empty, frame)
                        .Size(330, 80)
                        .SetParentTransform(parentTransform);
                    var slLang = ui.AddCompositeSelect(ui.Columns[ui.LastCol - 12], ui.Rows[ui.FirstRow + 2] + UIHelper.GetUIDeltaY() / 2, GameTool.LS("libtxt999990015"), new string[] { GameTool.LS("libtxt999990016"), "Deutsch", "Español", "Français", "日本語", "Latina", "Русский", "Tiếng Việt", "ไทย" }, TranslateIndex)
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
                    var modCreatorBtn = ui.AddButton(ui.Columns[ui.LastCol - 10], ui.Rows[ui.FirstRow + 2] - UIHelper.GetUIDeltaY() / 2, () => UICustom2.WhenFree(() =>
                    {
                        var miniWindow = new UICustom2(GameTool.LS("libtxt999990009"));
                        {
                            var col = miniWindow.FirstCol + 1;
                            var txtCol = col + 3;
                            var row = miniWindow.FirstRow;
                            //check duplicated IDs
                            miniWindow.AddButton(col, row, ShowCheckedIDs, GameTool.LS("libtxt999990010"));
                            miniWindow.AddText(txtCol, row, GameTool.LS("libtxt999990011")).Align();
                            //error reports (only when a BugSplat database is set)
                            if (CrashReportHelper.IsEnabled)
                            {
                                row += 2;
                                UIItemText txtConsent = null;
                                miniWindow.AddButton(col, row, () => AskCrashReportConsent(() => txtConsent?.Set(GetCrashReportConsentText())), GameTool.LS("libtxt999990019"));
                                txtConsent = miniWindow.AddText(txtCol, row, GetCrashReportConsentText()).Align();
                            }
                            //export assets
                            //row += 2;
                            //miniWindow.AddButton(col, row, ExportAssets, GameTool.LS("libtxt999990012"));
                            //miniWindow.AddText(txtCol, row, GameTool.LS("libtxt999990013")).Align();
                        }
                    }), GameTool.LS("libtxt999990009"))
                        .Align(TextAnchor.MiddleCenter)
                        .Size(200, 24)
                        .SetParentTransform(panelLangInit.Component.transform);
                }
                ui.UpdateUI();

                //the mod wallpaper of the session (startup draw of the packs, first title screen) or chosen by the player
                //(WallpaperHelper), after the game's own one
                WallpaperHelper.ShowStartupWallpaper();
                var wallpaper = WallpaperHelper.GetCurrentKey();
                if (wallpaper != null)
                {
                    WallpaperHelper.ShowOnTitle(wallpaper);
                    g.timer.Frame(ActionHelper.TracedIl2Action(() => WallpaperHelper.ShowOnTitle(WallpaperHelper.GetCurrentKey())), 1, false);
                }

                //error reports: ask once per consent key (the answer can be changed in the ModCreator window)
                if (CrashReportHelper.NeedsAsking)
                    AskCrashReportConsent(null);
            }
        }

        /// <summary>The toggle "hide the game's title on mod wallpapers" of the Change Theme window (tests).</summary>
        public static UIItemToggle HideTitleToggle { get; private set; }

        /// <summary>
        /// Change Theme window: the toggle "hide the game's title on mod wallpapers" (HideGameTitle, saved, applied at once),
        /// checkbox then its text, small, right above the top-left corner of the list.
        /// </summary>
        private void AddHideTitleToggle()
        {
            var ui = new UICover<UIWallPaper>(UIType.WallPaper);
            UIItemComposite tg;
            {
                var value = WallpaperHelper.ShouldHideGameTitle(WallpaperHelper.Find(WallpaperHelper.GetCurrentKey()));
                tg = ui.AddCompositeToggle(ui.MidCol, ui.FirstRow, string.Empty, value, GameTool.LS("libtxt999990033"));
                //white on the dark blurred background of the window
                tg.Postfix?.Format(Color.white, 15);
                HideTitleToggle = tg.MainComponent as UIItemToggle;
                HideTitleToggle.SetWork(new UIItemWork
                {
                    ChangeAct = ActionHelper.WTracedAction<UIItemBase, object>((a, b) =>
                    {
                        HideGameTitle = b.Parse<bool>();
                        CacheHelper.SaveGlobalCache(this);
                        WallpaperHelper.UpdateGameTitle();
                    })
                });
            }
            ui.UpdateUI();
            PlaceOverList(ui.UI, tg);
        }

        //the toggle and its text right above the top-left corner of the list of the Change Theme window
        private static void PlaceOverList(UIWallPaper w, UIItemComposite tg)
        {
            var scroll = w?.goItemRoot?.GetComponentInParent<ScrollRect>();
            var area = scroll?.viewport ?? scroll?.GetComponent<RectTransform>() ?? w?.goItemRoot?.transform?.parent?.GetComponent<RectTransform>();
            var box = tg.MainComponent.Component.GetComponent<RectTransform>();
            var txt = tg.Postfix?.Component?.GetComponent<RectTransform>();
            if (area == null || box == null)
                return;
            var r = area.rect;
            //left edge of the first item of the list (the list is narrower than its viewport), else of the viewport
            var left = r.xMin;
            RectTransform first = null;
            for (var i = 0; first == null && i < w.goItemRoot.transform.childCount; i++)
            {
                var c = w.goItemRoot.transform.GetChild(i);
                if (c.gameObject.activeSelf)
                    first = c.GetComponent<RectTransform>();
            }
            if (first != null)
                left = area.InverseTransformPoint(first.TransformPoint(new Vector3(first.rect.xMin, 0, 0))).x;
            var bw = box.rect.width;
            var bh = box.rect.height;
            var y = r.yMax + bh / 2 + 6;
            box.position = area.TransformPoint(new Vector3(left + bw * box.pivot.x, y + bh * (box.pivot.y - 0.5f), 0));
            if (txt != null)
            {
                txt.pivot = new Vector2(0f, 0.5f);
                tg.Postfix.Item.Align(TextAnchor.MiddleLeft);
                tg.Postfix.Item.horizontalOverflow = HorizontalWrapMode.Overflow;
                txt.position = area.TransformPoint(new Vector3(left + bw + 6, y, 0));
            }
            tg.Prefix?.Component?.gameObject?.SetActive(false);
        }

        //the components kept on the copy of a game button (the others are the game's own scripts)
        private static readonly HashSet<string> UNITY_UI_COMPONENTS = new HashSet<string>
        {
            "Transform", "RectTransform", "CanvasRenderer", "Image", "RawImage", "Text", "Button", "Outline", "Shadow",
            "LayoutElement", "ContentSizeFitter", "HorizontalLayoutGroup", "VerticalLayoutGroup", "CanvasGroup", "Mask", "RectMask2D"
        };

        /// <summary>
        /// A hidden copy of the frame of the game's DLC buttons on the title screen (the first one shown: DLC, Five Blooms,
        /// Archive) for the ModLib buttons: only its frame and its label are kept (no DLC icon, no "Claimed" mark: the
        /// small pictures), every part centered so that UIItemButton.Size sizes it like the default button. Null when the
        /// game has none (the default button is used).
        /// </summary>
        public static Button GetTitleFrameButton(UILogin login)
        {
            try
            {
                var buttons = new Button[] { login.btnDLC, login.btnDLC_En, login.btnFiveFlower, login.btnFiveFlower_En, login.btnArtBook, login.btnArtBook_En };
                var texts = new Text[] { login.textDLC, login.textDLC_En, login.textFiveFlower, login.textFiveFlower_En, login.textArtBook, login.textArtBook_En };
                var i = System.Array.FindIndex(buttons, x => x != null && x.gameObject.activeInHierarchy);
                if (i < 0)
                    i = System.Array.FindIndex(buttons, x => x != null);
                if (i < 0)
                    return null;
                var srcBtn = buttons[i];
                var srcTxt = texts[i];
                DebugHelper.WriteLine($"Title frame from {srcBtn.name}:\n{BattleSceneHelper.DumpHierarchy(srcBtn.transform, 4)}");

                //the path of the label in the button (its text: the label of the copy)
                var labelPath = srcTxt != null && srcTxt.transform.IsChildOf(srcBtn.transform) ? GetPath(srcBtn.transform, srcTxt.transform) : null;
                var frame = srcBtn.Create(login.canvas.transform);
                var root = frame.GetComponent<RectTransform>();
                var size = root.rect.size;
                var label = labelPath != null ? frame.transform.Find(labelPath) : frame.GetComponentInChildren<Text>(true)?.transform;
                //the frame of the game's buttons is a sprite ("Normal" {SpriteRenderer}: LoginButton_normal_DLC), drawn by
                //the button's SortingGroup: made a UI image of the size of the button
                //(the copy is inactive: the sprites shown are the ones of active objects)
                foreach (var sr in frame.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    var go = sr.gameObject;
                    var sprite = sr.sprite;
                    if (sprite == null || go == frame.gameObject || !go.activeSelf || !sr.enabled)
                        continue;
                    UnityEngine.Object.DestroyImmediate(sr);
                    var img = go.AddComponent<Image>();
                    img.sprite = sprite;
                    img.raycastTarget = false;
                    img.type = sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                    var rt = go.GetComponent<RectTransform>();
                    rt.localScale = Vector3.one;
                    rt.sizeDelta = size;
                    go.transform.SetAsFirstSibling();
                    DebugHelper.WriteLine($"Title frame: {sprite.name} ({sprite.rect.width}x{sprite.rect.height}, border {sprite.border}) as an image");
                }
                foreach (var rect in frame.GetComponentsInChildren<RectTransform>(true))
                {
                    if (rect == root)
                        continue;
                    var keep = label != null && (rect == label || label.IsChildOf(rect));
                    //the frame: a picture nearly as big as the button
                    var r = rect.rect.size;
                    if (!keep && rect.GetComponent<Image>() != null && r.x >= size.x * 0.8f && r.y >= size.y * 0.6f)
                        keep = true;
                    if (!keep)
                    {
                        rect.gameObject.SetActive(false);
                        continue;
                    }
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                }
                //the game's own components (localized text, DLC state: they set the text / show the marks again) removed
                foreach (var comp in frame.GetComponentsInChildren<Component>(true))
                {
                    var name = comp.GetIl2CppType().Name;
                    if (!UNITY_UI_COMPONENTS.Contains(name) && !name.StartsWith("TextMeshPro"))
                    {
                        DebugHelper.WriteLine($"Title frame: {name} removed from {comp.gameObject.name}");
                        UnityEngine.Object.DestroyImmediate(comp);
                    }
                }
                if (label != null)
                {
                    label.gameObject.SetActive(true);
                    label.SetAsLastSibling();
                }
                return frame;
            }
            catch (System.Exception ex)
            {
                DebugHelper.WriteLine($"Title frame: {ex.Message}");
                return null;
            }
        }

        private static string GetPath(Transform root, Transform t)
        {
            var names = new List<string>();
            for (var c = t; c != null && c != root; c = c.parent)
                names.Insert(0, c.name);
            return string.Join("/", names);
        }

        /// <summary>
        /// Asks the player whether error reports may be sent (what is sent and what is not) and saves the answer.
        /// Closing the dialog without an answer leaves it NotAsked (asked again next time).
        /// </summary>
        public void AskCrashReportConsent(System.Action onAnswer)
        {
            // the window is a UITextInfoLong of the game: one at a time (another one open, a mod's notice on the title
            // screen, would get these texts over its own): opened once it is closed (UICustomQueue)
            UICustom1.WhenFree(() => OpenConsentWindow(onAnswer));
        }

        private void OpenConsentWindow(System.Action onAnswer)
        {
            System.Action<CrashReportConsentEnum> answer = (v) =>
            {
                ConsentWindow = null;
                CrashReportConsent = v;
                CrashReportConsentKey = CrashReportHelper.ConsentKey;
                CacheHelper.SaveGlobalCache(this);
                DebugHelper.WriteLine($"Error reports: {v} ({CrashReportConsentKey})");
                DebugHelper.Save();
                onAnswer?.Invoke();
            };
            ConsentWindow = new UICustom1(GameTool.LS("libtxt999990019"), GameTool.LS("libtxt999990041"),
                () => answer(CrashReportConsentEnum.Allowed), true, () => answer(CrashReportConsentEnum.Denied));
            try { BuildConsentWindow(ConsentWindow); }
            catch (System.Exception ex) { DebugHelper.WriteLine(ex); }
        }

        /// <summary>The window asking for the error reports (null: not open). Tests: its texts, AnswerConsent.</summary>
        public static UICustom1 ConsentWindow { get; private set; }

        /// <summary>The texts of the consent window, for tests (question, mods, what is sent / never sent, where to change it).</summary>
        public static string ConsentText { get; private set; } = string.Empty;

        /// <summary>
        /// The consent window: the question over the whole width, the mods sending reports on the left, what is sent and
        /// what is never sent on the right, where the answer can be changed at the bottom; buttons Allow / Do not send.
        /// </summary>
        private static void BuildConsentWindow(UICustom1 ui)
        {
            var cancelLabel = ui.UI.btnCancel?.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (cancelLabel != null) cancelLabel.text = GameTool.LS("libtxt999990042");
            float left = ui.MinWidth + 30f, right = ui.MaxWidth - 30f, top = ui.MinHeight - 20f;
            var width = right - left;
            var leftWidth = width * 0.34f;
            var rightLeft = left + leftWidth + 30f;
            var rightWidth = right - rightLeft;
            var title = new Color(0.45f, 0.12f, 0.08f);
            var texts = new List<string>();
            UIItemText Block(float x, float top0, float w, float h, string text, int size, Color color, FontStyle style = FontStyle.Normal)
            {
                var t = ui.AddText(x + w / 2f, top0 - h / 2f, text);
                t.Align(TextAnchor.UpperLeft, VerticalWrapMode.Overflow, HorizontalWrapMode.Wrap).Format(color, size, style);
                t.Item.rectTransform.sizeDelta = new Vector2(w, h);
                texts.Add(text);
                return t;
            }
            var question = GameTool.LS("libtxt999990034");
            Block(left, top, width, 60f, question, 18, Color.black, FontStyle.Bold);
            var y = top - 75f;
            Block(left, y, leftWidth, 30f, GameTool.LS("libtxt999990035"), 18, title, FontStyle.Bold);
            var mods = CrashReportHelper.ModNames;
            Block(left, y - 36f, leftWidth, 30f * System.Math.Max(1, mods.Count) + 10f, string.Join("\n", mods.Select(x => $"• {x}")), 17, Color.black);
            Block(rightLeft, y, rightWidth, 30f, GameTool.LS("libtxt999990036"), 18, title, FontStyle.Bold);
            Block(rightLeft, y - 36f, rightWidth, 130f, GameTool.LS("libtxt999990037"), 16, Color.black);
            Block(rightLeft, y - 175f, rightWidth, 30f, GameTool.LS("libtxt999990038"), 18, title, FontStyle.Bold);
            Block(rightLeft, y - 211f, rightWidth, 60f, GameTool.LS("libtxt999990039"), 16, Color.black);
            Block(left, ui.MaxHeight + 70f, width, 30f, string.Format(GameTool.LS("libtxt999990040"), GameTool.LS("libtxt999990009"), GameTool.LS("libtxt999990019")), 15, new Color(0.35f, 0.24f, 0.13f));
            ConsentText = string.Join("\n", texts);
        }

        /// <summary>The consent window answered (tests): Allow / Do not send, as its buttons.</summary>
        public static bool AnswerConsent(bool allow)
        {
            var ui = ConsentWindow;
            if (ui?.UI == null) return false;
            var btn = allow ? ui.UI.btnOK : ui.UI.btnCancel;
            if (btn == null) return false;
            btn.onClick.Invoke();
            return true;
        }

        private string GetCrashReportConsentText()
        {
            return GameTool.LS(CrashReportHelper.IsAllowed ? "libtxt999990021" : "libtxt999990022");
        }

        /// <summary>
        /// Lists config ids loaded by several mods (red) or replacing base game ids (blue).
        /// </summary>
        private void ShowCheckedIDs() => UICustom1.WhenFree(ShowCheckedIDsNow);

        private void ShowCheckedIDsNow()
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

        /// <summary>
        /// Language code of the selected translation (null = default).
        /// </summary>
        public static string GetTranslateLanguage()
        {
            return TranslateCode[Instance.TranslateIndex];
        }

        /// <summary>
        /// Reloads the LocalText configs of all loaded mods for a language.
        /// </summary>
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
            //icons of the localTexts, after every load of the mod texts (the mod's and its translations): [icon:NAME] -> <sprite name=NAME>
            try { TextIconHelper.ConvertLocalTexts(); }
            catch (System.Exception ex) { DebugHelper.WriteLine($"TextIconHelper: {ex}"); }
        }
    }
}
