using ModLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ModLib.Helper
{
    /// <summary>
    /// Drives the game's UIs like a player: finds and clicks buttons by their label, reads the texts on screen, closes
    /// UIs. Labels are compared without case; a button is used only when active and interactable. The texts are read
    /// from the Text components and from the TextMeshPro ones (a text with an icon shows in the TextMeshPro twin of its
    /// Text, which is emptied: TextIconHelper.SetText).
    /// </summary>
    [ActionCat("UI")]
    public static class UIClickHelper
    {
        /// <summary>Labels of the buttons that confirm a dialog or leave a result screen (English and Chinese).</summary>
        public static readonly string[] CONFIRM_LABELS = { "OK", "Yes", "Confirm", "Continue", "Leave", "Exit", "Back", "Close", "确定", "確定", "是", "离开", "離開", "退出", "返回" };

        /// <summary>
        /// Clicks the first button of every open UI whose label is one of <paramref name="labels"/>, except on the title
        /// screen and the world map (their buttons start a game or open menus). Returns the clicked label, or null.
        /// </summary>
        public static string ClickButton(params string[] labels)
        {
            if (g.ui.uiRoot == null) return null;
            foreach (var btn in g.ui.uiRoot.GetComponentsInChildren<Button>())
            {
                if (!IsUsable(btn)) continue;
                var label = GetLabel(btn);
                if (!Matches(label, labels)) continue;
                var name = btn.GetComponentInParent<UIBase>()?.uiType?.uiName;
                if (name == UIType.Login.uiName || name == UIType.MapMain.uiName) continue;
                btn.onClick.Invoke();
                return label;
            }
            return null;
        }

        /// <summary>Clicks the first button of <paramref name="ui"/> whose label is one of <paramref name="labels"/>. Returns the label, or null.</summary>
        public static string ClickIn(UIBase ui, params string[] labels)
        {
            if (ui == null) return null;
            foreach (var btn in ui.GetComponentsInChildren<Button>())
            {
                if (!IsUsable(btn)) continue;
                var label = GetLabel(btn);
                if (!Matches(label, labels)) continue;
                btn.onClick.Invoke();
                return label;
            }
            return null;
        }

        /// <summary>
        /// Clicks the first button one of whose texts is <paramref name="label"/> (a drama option may start with a
        /// bullet: "• Accept"). Returns the clicked text, or null.
        /// </summary>
        public static string ClickOption(string label)
        {
            if (g.ui.uiRoot == null) return null;
            foreach (var btn in g.ui.uiRoot.GetComponentsInChildren<Button>())
            {
                if (!IsUsable(btn)) continue;
                var text = GetTexts(btn).Select(CleanLabel).FirstOrDefault(t => t == CleanLabel(label));
                if (text == null) continue;
                btn.onClick.Invoke();
                return text;
            }
            return null;
        }

        /// <summary>The first active button of <paramref name="ui"/> whose label starts with <paramref name="labelStart"/> (null when none).</summary>
        public static Button FindButton(UIBase ui, string labelStart)
        {
            return ui?.GetComponentsInChildren<Button>()
                .FirstOrDefault(b => b.gameObject.activeInHierarchy && (GetLabel(b) ?? string.Empty).StartsWith(labelStart));
        }

        /// <summary>The active texts of every open UI.</summary>
        public static List<string> ScreenTexts()
        {
            if (g.ui.uiRoot == null) return new List<string>();
            return GetTexts(g.ui.uiRoot.transform).Select(t => t.Trim()).ToList();
        }

        /// <summary>The active texts of <paramref name="ui"/> containing <paramref name="contains"/>.</summary>
        public static List<string> FindTexts(UIBase ui, string contains)
        {
            return ui == null ? new List<string>() : GetTexts(ui).Where(t => t.Contains(contains)).ToList();
        }

        /// <summary>Closes the given UIs when they are open.</summary>
        public static void CloseAll(params UIType.UITypeBase[] types)
        {
            foreach (var t in types)
                if (g.ui.HasUI(t))
                    g.ui.CloseUI(t);
        }

        /// <summary>Opens a UI fresh (closed first when open), then <paramref name="init"/> on it (e.g. InitData of a town building UI).</summary>
        public static T OpenFresh<T>(UIType.UITypeBase type, Action<T> init = null) where T : UIBase
        {
            if (g.ui.HasUI(type))
                g.ui.CloseUI(type);
            var ui = g.ui.OpenUISafe<T>(type);
            init?.Invoke(ui);
            return ui;
        }

        /// <summary>The active, non-empty texts under <paramref name="root"/>: Text and TextMeshPro components.</summary>
        public static List<string> GetTexts(UnityEngine.Component root)
        {
            var rs = new List<string>();
            if (root == null) return rs;
            rs.AddRange(root.GetComponentsInChildren<Text>().Where(t => t != null && t.gameObject.activeInHierarchy && !string.IsNullOrEmpty(t.text)).Select(t => t.text));
            rs.AddRange(root.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Where(t => t != null && t.gameObject.activeInHierarchy && !string.IsNullOrEmpty(t.text)).Select(t => t.text));
            return rs;
        }

        /// <summary>The label of a button: its first active, non-empty text (Text or TextMeshPro), trimmed.</summary>
        public static string GetLabel(UnityEngine.Component button) => GetTexts(button).Select(t => t.Trim()).FirstOrDefault(t => t.Length > 0);

        private static bool IsUsable(Button btn) => btn != null && btn.gameObject.activeInHierarchy && btn.interactable;

        private static bool Matches(string label, string[] labels) =>
            !string.IsNullOrEmpty(label) && labels.Any(l => string.Equals(l, label, StringComparison.OrdinalIgnoreCase));

        private static string CleanLabel(string s) => (s ?? string.Empty).Trim().TrimStart('•', '·', ' ').Trim();
    }
}
