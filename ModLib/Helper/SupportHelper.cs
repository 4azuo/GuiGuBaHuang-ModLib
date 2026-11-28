using ModLib.Attributes;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModLib.Helper
{
    /// <summary>
    /// Support pages of the mods (Ko-fi...): a mod made with ModLib gives its own with constants of its entry class:
    ///   namespace MOD_&lt;id&gt; { public class ModMain ... {
    ///       public const string SUPPORT_URL = "https://ko-fi.com/me";  //where to support
    ///       public const string SUPPORT_QR = "kofi-qr.png";            //optional: image of ModImg of the mod (QR code of the page)
    ///       public const string SUPPORT_NAME = "My Mod";               //optional: the name shown (LICENSE_NAME, else the mod id)
    ///       public const string SUPPORT_INFO = "mymod_support_info";   //optional: localText key: a few words about the mod / what the support gives
    ///   } }
    /// ModLib gives its own too (entry class of ModLib). The title screen gets one "Support" button in its bottom-right corner
    /// when a mod gives one; its window shows one QR code at a time (QR codes side by side are hard to scan: the phone reads
    /// the wrong one): the mods listed on the left in the order of the mod list of the game, the name, the words, the QR code
    /// and the page of the chosen one on the right. LicenseHelper uses them too when LICENSE_SUPPORT_URL / LICENSE_QR are not set.
    /// </summary>
    [ActionCat("Support")]
    public static class SupportHelper
    {
        /// <summary>Constants (string) of the entry class MOD_&lt;id&gt;.ModMain.</summary>
        public const string CONST_URL = "SUPPORT_URL", CONST_QR = "SUPPORT_QR", CONST_NAME = "SUPPORT_NAME", CONST_INFO = "SUPPORT_INFO";
        /// <summary>Side of the QR code of the window (UI units).</summary>
        public const float QR_SIZE = 300f;

        /// <summary>The support page of a mod.</summary>
        public class Info
        {
            /// <summary>Id of the mod.</summary>
            public string ModId { get; set; }
            /// <summary>Name shown in the window.</summary>
            public string Name { get; set; }
            /// <summary>The support page.</summary>
            public string Url { get; set; }
            /// <summary>Image of ModImg of the mod: QR code of the page; null: none.</summary>
            public string QrImage { get; set; }
            /// <summary>localText key: a few words about the mod / what the support gives; null: none.</summary>
            public string InfoKey { get; set; }
        }

        //the support pages read (when ModLib lists the cache types of the mods)
        private static readonly List<Info> _mods = new List<Info>();

        /// <summary>The mods that give a support page, in the order of the mod list of the game.</summary>
        public static IReadOnlyList<Info> Mods => Ordered();

        /// <summary>The support page of a mod; null: none, or not read yet.</summary>
        public static Info Get(string modId) => _mods.FirstOrDefault(x => string.Equals(x.ModId, modId, StringComparison.OrdinalIgnoreCase));

        /// <summary>Reads the SUPPORT_* constants of the entry class of a mod (once per mod); null: no SUPPORT_URL.</summary>
        public static Info Read(string modId, Type entry)
        {
            if (string.IsNullOrEmpty(modId) || entry == null) return null;
            var known = Get(modId);
            if (known != null) return known;
            var url = LicenseHelper.ReadStringConst(entry, CONST_URL);
            if (string.IsNullOrWhiteSpace(url))
            {
                DebugHelper.WriteLine($"Support: {modId} gives no {CONST_URL} ({entry.FullName})");
                return null;
            }
            var info = new Info
            {
                ModId = modId,
                Url = url,
                QrImage = LicenseHelper.ReadStringConst(entry, CONST_QR),
                InfoKey = LicenseHelper.ReadStringConst(entry, CONST_INFO),
                Name = LicenseHelper.ReadStringConst(entry, CONST_NAME) ?? LicenseHelper.ReadStringConst(entry, LicenseHelper.CONST_NAME) ?? modId,
            };
            _mods.Add(info);
            DebugHelper.WriteLine($"Support: {modId} ({info.Name}) {url}");
            return info;
        }

        //the pages in the order of the mod list of the game (the mods not in it last, in the order they were read)
        private static List<Info> Ordered()
        {
            var order = new List<string>();
            try
            {
                foreach (var m in g.mod.allModPaths)
                    order.Add(m.t1);
            }
            catch { }
            return _mods.Select((x, i) => new { x, i })
                .OrderBy(a =>
                {
                    var k = order.FindIndex(id => string.Equals(id, a.x.ModId, StringComparison.OrdinalIgnoreCase));
                    return k < 0 ? int.MaxValue : k;
                })
                .ThenBy(a => a.i)
                .Select(a => a.x)
                .ToList();
        }

        /// <summary>The support window (null: not open). Tests.</summary>
        public static UICustom1 Window { get; private set; }

        /// <summary>The gold frame around the chosen mod of the list of the window (null: one mod, no list). Tests.</summary>
        public static UIItemImage ListGlow { get; private set; }

        /// <summary>The "Support" button of the title screen (null: none). Tests.</summary>
        public static UIItemButton TitleButton { get; private set; }

        /// <summary>Title screen: the "Support" button in its bottom-right corner, when a mod gives a support page.</summary>
        public static void AddTitleButton()
        {
            TitleButton = null;
            if (_mods.Count == 0) return;
            TitleButton = TitleModHelper.AddCornerButton(GameTool.LS("libtxt999990061"), () => OpenWindow(0));
        }

        /// <summary>The support window, on the mod of index <paramref name="selected"/> (of <see cref="Mods"/>).</summary>
        public static void OpenWindow(int selected)
        {
            if (_mods.Count == 0) return;
            UICustom1.WhenFree(() =>
            {
                var title = GameTool.LS("libtxt999990061");
                Window = new UICustom1(title, string.Empty, () => Window = null);
                try
                {
                    ShowTitle(Window, title);
                    BuildWindow(Window, Ordered(), selected);
                }
                catch (Exception ex) { DebugHelper.WriteLine(ex); }
            });
        }

        //the title of the window shown whatever its length (the title text of the game wraps, then cuts what does not fit)
        private static void ShowTitle(UICustom1 ui, string title)
        {
            foreach (var t in ui.UI.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {
                if (t.text != title) continue;
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                t.gameObject.SetActive(true);
            }
        }

        private static void BuildWindow(UICustom1 ui, List<Info> mods, int selected)
        {
            selected = Math.Max(0, Math.Min(selected, mods.Count - 1));
            var info = mods[selected];
            var brown = new Color(0.35f, 0.24f, 0.13f);
            var red = new Color(0.45f, 0.12f, 0.08f);
            float left = ui.MinWidth + 30f, right = ui.MaxWidth - 30f, top = ui.MinHeight - 20f;

            UIItemText Block(float x, float y0, float w, float h, string text, int size, Color color, TextAnchor anchor, FontStyle style = FontStyle.Normal)
            {
                var t = ui.AddText(x, y0 - h / 2f, text);
                t.Align(anchor, VerticalWrapMode.Overflow, HorizontalWrapMode.Wrap).Format(color, size, style);
                t.Item.rectTransform.sizeDelta = new Vector2(w, h);
                return t;
            }

            //top, over the whole width: who the window is for
            Block((left + right) / 2f, top, right - left, 30f, GameTool.LS("libtxt999990065"), 18, red, TextAnchor.MiddleCenter, FontStyle.Bold);
            top -= 50f;

            //left: the mods (only when several), the chosen one framed by the pulsing gold frame (UIGlowHelper)
            if (mods.Count > 1)
            {
                var bx = left + 130f;
                UIItemBase chosen = null;
                for (var i = 0; i < mods.Count; i++)
                {
                    var k = i;
                    var b = ui.AddButton(bx, top - 20f - i * 46f, () => { ui.Clear(); BuildWindow(ui, mods, k); }, mods[k].Name)
                        .Size(240f, 38f).Align(TextAnchor.MiddleCenter);
                    if (k == selected) chosen = b;
                }
                ListGlow = UIGlowHelper.Add(ui, 240f, 38f);
                UIGlowHelper.Place(ListGlow, chosen);
                ui.IsAutoUpdate = true;
                left += 290f;
            }
            else ListGlow = null;

            //right of the list: the chosen mod, its words, its QR code, its page
            var cx = (left + right) / 2f;
            var width = right - left;
            var y = top;
            Block(cx, y, width, 30f, info.Name, 22, Color.black, TextAnchor.MiddleCenter, FontStyle.Bold);
            y -= 36f;
            if (!string.IsNullOrEmpty(info.InfoKey) && LocalTextHelper.HasText(info.InfoKey))
            {
                Block(cx, y, Math.Min(width, 640f), 70f, GameTool.LS(info.InfoKey), 16, brown, TextAnchor.UpperCenter);
                y -= 78f;
            }
            Sprite qr = null;
            if (!string.IsNullOrEmpty(info.QrImage))
            {
                try { qr = SpriteHelper.GetImage(info.ModId, info.QrImage); }
                catch (Exception ex) { DebugHelper.WriteLine($"Support: QR {info.QrImage} not read: {ex.Message}"); }
                if (qr == null) DebugHelper.WriteLine($"Support: QR {SpriteHelper.GetImgFilePath(info.ModId, info.QrImage)} not found");
            }
            if (qr != null)
            {
                ui.AddImage(cx, y - QR_SIZE / 2f, qr).Size(QR_SIZE, QR_SIZE);
                y -= QR_SIZE + 12f;
            }
            Block(cx, y, width, 24f, info.Url, 15, brown, TextAnchor.MiddleCenter);
            y -= 46f;
            ui.AddButton(cx, y, () => System.Diagnostics.Process.Start(info.Url), GameTool.LS("libtxt999990061"))
                .Size(200f, 36f).Align(TextAnchor.MiddleCenter);
            y -= 34f;
            Block(cx, y, width, 24f, GameTool.LS("libtxt999990066"), 14, brown, TextAnchor.MiddleCenter);
        }
    }
}
