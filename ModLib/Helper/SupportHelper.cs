using ModLib.Attributes;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLib.Helper
{
    /// <summary>
    /// Support pages of the mods (Ko-fi...): a mod made with ModLib gives its own with constants of its entry class:
    ///   namespace MOD_&lt;id&gt; { public class ModMain ... {
    ///       public const string SUPPORT_URL = "https://ko-fi.com/me";  //where to support
    ///       public const string SUPPORT_QR = "kofi-qr.png";            //optional: image of ModImg of the mod (QR code of the page)
    ///       public const string SUPPORT_NAME = "My Mod";               //optional: the name shown (LICENSE_NAME, else the mod id)
    ///   } }
    /// The title screen gets one "Support" button in the row of the mods when a mod gives one; its window shows one QR code at a
    /// time (QR codes side by side are hard to scan): the mods listed on the left, the QR code and the page of the chosen one on
    /// the right. LicenseHelper uses them too when LICENSE_SUPPORT_URL / LICENSE_QR are not set.
    /// </summary>
    [ActionCat("Support")]
    public static class SupportHelper
    {
        /// <summary>Constants (string) of the entry class MOD_&lt;id&gt;.ModMain.</summary>
        public const string CONST_URL = "SUPPORT_URL", CONST_QR = "SUPPORT_QR", CONST_NAME = "SUPPORT_NAME";
        /// <summary>Side of the QR code of the window (UI units).</summary>
        public const float QR_SIZE = 300f;

        /// <summary>The support page of a mod.</summary>
        public class Info
        {
            public string ModId { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            /// <summary>Image of ModImg of the mod: QR code of the page; null: none.</summary>
            public string QrImage { get; set; }
        }

        //mod id -> support page (read when ModLib lists the cache types of the mods), in the order of the mods
        private static readonly List<Info> _mods = new List<Info>();

        /// <summary>The mods that give a support page.</summary>
        public static IReadOnlyList<Info> Mods => _mods;

        /// <summary>The support page of a mod; null: none, or not read yet.</summary>
        public static Info Get(string modId) => _mods.FirstOrDefault(x => string.Equals(x.ModId, modId, StringComparison.OrdinalIgnoreCase));

        /// <summary>Reads the SUPPORT_* constants of the entry class of a mod (once per mod); null: no SUPPORT_URL.</summary>
        public static Info Read(string modId, Type entry)
        {
            if (string.IsNullOrEmpty(modId) || entry == null) return null;
            var known = Get(modId);
            if (known != null) return known;
            var url = LicenseHelper.ReadStringConst(entry, CONST_URL);
            if (string.IsNullOrWhiteSpace(url)) return null;
            var info = new Info
            {
                ModId = modId,
                Url = url,
                QrImage = LicenseHelper.ReadStringConst(entry, CONST_QR),
                Name = LicenseHelper.ReadStringConst(entry, CONST_NAME) ?? LicenseHelper.ReadStringConst(entry, LicenseHelper.CONST_NAME) ?? modId,
            };
            _mods.Add(info);
            DebugHelper.WriteLine($"Support: {modId} ({info.Name}) {url}");
            return info;
        }

        /// <summary>The support window (null: not open). Tests.</summary>
        public static UICustom1 Window { get; private set; }

        /// <summary>Title screen: the "Support" button in the row of the mods, when a mod gives a support page.</summary>
        public static void AddTitleButton()
        {
            if (_mods.Count == 0) return;
            TitleModHelper.AddModButton(GameTool.LS("libtxt999990061"), () => OpenWindow(0), 16, 120f);
        }

        /// <summary>The support window, on the mod of index <paramref name="selected"/>.</summary>
        public static void OpenWindow(int selected)
        {
            if (_mods.Count == 0) return;
            UICustom1.WhenFree(() =>
            {
                Window = new UICustom1(GameTool.LS("libtxt999990065"), string.Empty, () => Window = null);
                try { BuildWindow(Window, selected); }
                catch (Exception ex) { DebugHelper.WriteLine(ex); }
            });
        }

        private static void BuildWindow(UICustom1 ui, int selected)
        {
            selected = Math.Max(0, Math.Min(selected, _mods.Count - 1));
            var info = _mods[selected];
            var brown = new UnityEngine.Color(0.35f, 0.24f, 0.13f);
            float left = ui.MinWidth + 30f, right = ui.MaxWidth - 30f, top = ui.MinHeight - 20f;

            //left: the mods (only when several), the chosen one marked
            if (_mods.Count > 1)
            {
                var bx = left + 130f;
                for (var i = 0; i < _mods.Count; i++)
                {
                    var k = i;
                    ui.AddButton(bx, top - 20f - i * 46f, () => { ui.Clear(); BuildWindow(ui, k); }, (k == selected ? "> " : string.Empty) + _mods[k].Name)
                        .Size(240f, 38f).Align(UnityEngine.TextAnchor.MiddleCenter);
                }
                left += 280f;
            }

            //right of the list: the chosen mod, its QR code, its page
            var cx = (left + right) / 2f;
            var y = top - 10f;
            ui.AddText(cx, y, info.Name).Align(UnityEngine.TextAnchor.MiddleCenter).Format(UnityEngine.Color.black, 20);
            y -= 36f;
            ui.AddText(cx, y, GameTool.LS("libtxt999990066")).Align(UnityEngine.TextAnchor.MiddleCenter).Format(brown, 15);
            y -= 24f;
            UnityEngine.Sprite qr = null;
            if (!string.IsNullOrEmpty(info.QrImage))
            {
                try { qr = SpriteHelper.GetImage(info.ModId, info.QrImage); }
                catch (Exception ex) { DebugHelper.WriteLine($"Support: QR {info.QrImage} not read: {ex.Message}"); }
                if (qr == null) DebugHelper.WriteLine($"Support: QR {SpriteHelper.GetImgFilePath(info.ModId, info.QrImage)} not found");
            }
            if (qr != null)
            {
                ui.AddImage(cx, y - QR_SIZE / 2f, qr).Size(QR_SIZE, QR_SIZE);
                y -= QR_SIZE + 20f;
            }
            var t = ui.AddText(cx, y, info.Url);
            t.Align(UnityEngine.TextAnchor.MiddleCenter).Format(brown, 15);
            t.Item.rectTransform.sizeDelta = new UnityEngine.Vector2(right - left, 26f);
            y -= 44f;
            ui.AddButton(cx, y, () => System.Diagnostics.Process.Start(info.Url), GameTool.LS("libtxt999990061"))
                .Size(200f, 36f).Align(UnityEngine.TextAnchor.MiddleCenter);
        }
    }
}
