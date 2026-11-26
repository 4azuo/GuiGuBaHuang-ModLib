using ModLib.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModLib.Helper
{
    /// <summary>
    /// Drawing kit for the information windows of a UICustom (UICustom1 = UITextInfoLong, UICustom2 = UITextInfo), in the
    /// canvas units of the window (x to the right, y up; Columns / Rows of the window give its area):
    /// - Box: a colored rectangle by its top left corner (cards, highlighted rows, lines);
    /// - Text / RightText / Center: a text by its left edge, its right edge, its center (y: its middle);
    /// - Group: a group title (game text icon, bold) with a line under it;
    /// - Bar: a progress bar with its text;
    /// - Grid: the top left corners of n cards in columns;
    /// - Table: a header row and rows of cells at column positions, a row highlighted, rows striped.
    /// Colors: PanelColors (defaults of the parchment windows of the game), every method takes its own.
    /// </summary>
    public static class UIPanelHelper
    {
        /// <summary>The colors of a panel (defaults for the parchment of the game windows).</summary>
        public class PanelColors
        {
            public Color Title = new Color(0.45f, 0.12f, 0.08f);
            public Color Text = new Color(0.12f, 0.10f, 0.08f);
            public Color Note = new Color(0.38f, 0.36f, 0.33f);
            public Color Grey = new Color(0.52f, 0.50f, 0.48f);
            public Color Highlight = new Color(0.62f, 0.40f, 0.02f);
            public Color Line = new Color(0.45f, 0.12f, 0.08f, 0.55f);
            public Color Card = new Color(0.55f, 0.42f, 0.25f, 0.10f);
            public Color CardOn = new Color(0.90f, 0.66f, 0.18f, 0.24f);
            public Color CardOff = new Color(0.30f, 0.30f, 0.30f, 0.08f);
            public Color BarBack = new Color(0f, 0f, 0f, 0.14f);
            public Color BarFill = new Color(0.85f, 0.55f, 0.10f, 0.85f);
        }

        public static PanelColors Colors { get; } = new PanelColors();

        /// <summary>A game text icon ("[icon:NAME]" as a sprite) and a space; empty for no icon.</summary>
        public static string Icon(string name) => string.IsNullOrEmpty(name) ? string.Empty : TextIconHelper.Icon(name) + " ";

        /// <summary>Height of a row of the window (distance of its first two rows).</summary>
        public static float RowHeight(UICustomBase ui) => ui.Rows.Count < 2 ? 30f : Math.Abs(ui.Rows[ui.FirstRow + 1] - ui.Rows[ui.FirstRow]);

        public static float Left(UICustomBase ui) => ui.Columns[ui.FirstCol];
        public static float Right(UICustomBase ui) => ui.Columns[ui.LastCol];
        public static float Top(UICustomBase ui) => ui.Rows[ui.FirstRow] + RowHeight(ui) * 0.5f;
        public static float Bottom(UICustomBase ui) => ui.Rows[ui.LastRow] - RowHeight(ui) * 0.5f;

        /// <summary>A colored rectangle by its top left corner (no click).</summary>
        public static UIItemImage Box(UICustomBase ui, float left, float top, float w, float h, Color c)
        {
            var img = ui.AddImage(left + w / 2f, top - h / 2f, null);
            img.Item.color = c;
            img.Size(w, h);
            img.Item.raycastTarget = false;
            return img;
        }

        /// <summary>A text whose left edge is at x (y: its middle).</summary>
        public static UIItemText Text(UICustomBase ui, float x, float y, string text, Color? color = null, int size = 13, FontStyle style = FontStyle.Normal)
        {
            var t = ui.AddText(x, y, text ?? string.Empty).Format(color ?? Colors.Text, size, style).Align(TextAnchor.MiddleLeft);
            t.Pos(x + t.Item.rectTransform.rect.width / 2f, y);
            return t;
        }

        /// <summary>A text whose right edge is at x.</summary>
        public static UIItemText RightText(UICustomBase ui, float x, float y, string text, Color? color = null, int size = 13, FontStyle style = FontStyle.Normal)
        {
            var t = ui.AddText(x, y, text ?? string.Empty).Format(color ?? Colors.Text, size, style).Align(TextAnchor.MiddleRight);
            t.Pos(x - t.Item.rectTransform.rect.width / 2f, y);
            return t;
        }

        /// <summary>A text centered at x.</summary>
        public static UIItemText Center(UICustomBase ui, float x, float y, string text, Color? color = null, int size = 13, FontStyle style = FontStyle.Normal)
            => ui.AddText(x, y, text ?? string.Empty).Format(color ?? Colors.Text, size, style).Align(TextAnchor.MiddleCenter);

        /// <summary>A group title (icon, bold) with a line under it, by its top left corner; returns the y under the line.</summary>
        public static float Group(UICustomBase ui, float left, float top, float w, string icon, string title, Color? color = null, int size = 16)
        {
            var dy = RowHeight(ui);
            Text(ui, left + 4f, top - dy * 0.5f, Icon(icon) + title, color ?? Colors.Title, size, FontStyle.Bold);
            Box(ui, left, top - dy * 1.05f, w, 2f, Colors.Line);
            return top - dy * 1.25f;
        }

        /// <summary>A progress bar (share 0..1) by its top left corner, its text centered on it.</summary>
        public static void Bar(UICustomBase ui, float left, float top, float w, float h, double share, string text = null, Color? fill = null, int size = 12)
        {
            Box(ui, left, top, w, h, Colors.BarBack);
            var s = (float)Math.Max(0d, Math.Min(1d, double.IsNaN(share) ? 0d : share));
            if (s > 0f) Box(ui, left, top, w * s, h, fill ?? Colors.BarFill);
            if (!string.IsNullOrEmpty(text)) Center(ui, left + w / 2f, top - h / 2f, text, Colors.Text, size, FontStyle.Bold);
        }

        /// <summary>The top left corners of count cards in cols columns (gap between them), from (left, top) in an area
        /// of width w; the size of a card is (card width, h).</summary>
        public static List<Vector2> Grid(int count, int cols, float left, float top, float w, float h, float gap, out float cardW)
        {
            cols = Math.Max(1, cols);
            cardW = (w - gap * (cols - 1)) / cols;
            var rs = new List<Vector2>();
            for (var i = 0; i < count; i++)
                rs.Add(new Vector2(left + (i % cols) * (cardW + gap), top - (i / cols) * (h + gap)));
            return rs;
        }

        /// <summary>The bottom of a grid of count cards (the y under its last row).</summary>
        public static float GridBottom(int count, int cols, float top, float h, float gap)
            => top - ((count + Math.Max(1, cols) - 1) / Math.Max(1, cols)) * (h + gap);

        /// <summary>A cell of a table: its text and color (null: the color of its row).</summary>
        public class Cell
        {
            public string Text;
            public Color? Color;
            public FontStyle? Style;
            public Cell(string text, Color? color = null, FontStyle? style = null) { Text = text; Color = color; Style = style; }
            public static implicit operator Cell(string text) => new Cell(text);
        }

        /// <summary>
        /// A table by its top left corner: the header (small, bold, grey), then the rows (cells at the columns x, shares of
        /// w from left), striped; the row highlighted (index, -1: none) on a CardOn box in bold. rowColor gives the color of
        /// a row (e.g. black for the reached ones, grey for the next ones). Returns the y under the table.
        /// </summary>
        public static float Table(UICustomBase ui, float left, float top, float w, float[] columns, string[] header, IList<Cell[]> rows,
            int highlight = -1, Func<int, Color> rowColor = null, int size = 13, float rowStep = 1f)
        {
            var dy = RowHeight(ui) * rowStep;
            var y = top;
            if (header != null)
            {
                for (var c = 0; c < header.Length && c < columns.Length; c++)
                    Text(ui, left + w * columns[c], y - dy * 0.5f, header[c], Colors.Note, size - 1, FontStyle.Bold);
                y -= dy;
            }
            for (var r = 0; r < rows.Count; r++)
            {
                var on = r == highlight;
                Box(ui, left, y, w, dy * 0.96f, on ? Colors.CardOn : (r % 2 == 0 ? Colors.Card : new Color(0f, 0f, 0f, 0f)));
                var color = on ? Colors.Highlight : rowColor?.Invoke(r) ?? Colors.Text;
                for (var c = 0; c < rows[r].Length && c < columns.Length; c++)
                {
                    var cell = rows[r][c];
                    if (cell == null) continue;
                    Text(ui, left + w * columns[c] + 6f, y - dy * 0.48f, cell.Text, cell.Color ?? color, size, cell.Style ?? (on ? FontStyle.Bold : FontStyle.Normal));
                }
                y -= dy;
            }
            return y;
        }
    }
}
