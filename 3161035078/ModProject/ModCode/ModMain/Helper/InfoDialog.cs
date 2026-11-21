using ModLib.Enum;
using ModLib.Helper;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Helper
{
    /// <summary>
    /// Message windows of the mod. A short text keeps the game's message box; a long text (or one of several lines) opens
    /// a text window (UITextInfo, UICustom2): the lines wrapped by words, the buttons under them. Same calls as
    /// g.ui.MsgBox, so a MsgBox call becomes InfoDialog.Msg without other change.
    /// </summary>
    public static class InfoDialog
    {
        /// <summary>
        /// Longer than this, or more lines: the text window. The game's message box holds a few short lines ("Cost
        /// 306.\nAre you sure?" stays a message box, with its Yes / No).
        /// </summary>
        public const int SHORT_TEXT = 120, SHORT_LINES = 3;
        public const int WRAP_CHARS = 64;

        public static bool IsLong(string text) => !string.IsNullOrEmpty(text) && (text.Length > SHORT_TEXT || text.Split('\n').Length > SHORT_LINES);

        /// <summary>A message (OK).</summary>
        public static void Msg(string title, string text)
        {
            if (!IsLong(text)) { g.ui.MsgBox(title, text); return; }
            Show(title, text);
        }

        /// <summary>A message with buttons (MsgBoxButtonEnum.YesNo: Yes runs the action).</summary>
        public static void Msg(string title, string text, MsgBoxButtonEnum buttons, Action yes)
        {
            if (!IsLong(text)) { g.ui.MsgBox(title, text, buttons, yes); return; }
            if (buttons == MsgBoxButtonEnum.YesNo)
                Show(title, text, (GameTool.LS("dlg_yes"), yes), (GameTool.LS("dlg_no"), null));
            else
                Show(title, text, (GameTool.LS("dlg_ok"), yes));
        }

        /// <summary>
        /// The text window: the text wrapped by words (a paragraph a line break), the buttons in a row under it (none: the
        /// window's own OK). A button closes the window, then runs its action.
        /// </summary>
        public static UICustom2 Show(string title, string text, params (string label, Action act)[] buttons)
        {
            g.ui.CloseUI(UIType.TextInfo);
            var ui = new UICustom2(title);
            var row = ui.FirstRow;
            foreach (var para in (text ?? string.Empty).Split('\n'))
                foreach (var line in Wrap(para, WRAP_CHARS))
                {
                    if (row >= ui.LastRow - (buttons.Length > 0 ? 2 : 0)) break;
                    ui.AddText(ui.MidCol, row++, line).Format(null, 14);
                }
            if (buttons.Length > 0)
            {
                // the window's own OK only closes it: with buttons of the dialog (Yes / No...) it is hidden, a player (or a
                // test clicking "OK") would close the choice without an answer
                try { if (ui.UI.btnOK != null) ui.UI.btnOK.gameObject.SetActive(false); } catch { }
                var y = Math.Min(ui.LastRow - 1, row + 1);
                var w = Math.Min(260f, (ui.Columns[ui.LastCol] - ui.Columns[ui.FirstCol]) / buttons.Length - 10f);
                for (var i = 0; i < buttons.Length; i++)
                {
                    var b = buttons[i];
                    var x = ui.Columns[ui.FirstCol] + (ui.Columns[ui.LastCol] - ui.Columns[ui.FirstCol]) * (i + 0.5f) / buttons.Length;
                    ui.AddButton(x, ui.Rows[y], () =>
                    {
                        g.ui.CloseUI(UIType.TextInfo);
                        b.act?.Invoke();
                    }, b.label).Format(Color.black, 14).Size(w, 34f);
                }
            }
            LastText = text;
            return ui;
        }

        /// <summary>The text of the last window (tests).</summary>
        public static string LastText { get; private set; }

        /// <summary>Lines of at most max characters, cut between words (a word longer than a line stays whole).</summary>
        public static List<string> Wrap(string text, int max)
        {
            var lines = new List<string>();
            var cur = string.Empty;
            // languages without spaces (Chinese, Japanese, Thai): cut by characters
            var words = text.Contains(' ') ? text.Split(' ') : text.Select(ch => ch.ToString()).ToArray();
            var sep = text.Contains(' ') ? " " : string.Empty;
            foreach (var w in words)
            {
                if (cur.Length > 0 && cur.Length + sep.Length + w.Length > max)
                {
                    lines.Add(cur);
                    cur = w;
                }
                else cur = cur.Length == 0 ? w : cur + sep + w;
            }
            if (cur.Length > 0 || lines.Count == 0) lines.Add(cur);
            return lines;
        }
    }
}
