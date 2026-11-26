using ModLib.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace ModLib.Object
{
    /// <summary>
    /// The pages of a mod's options (ModOptions) in a settings window (UICustomBase with the grid of Columns / Rows): a
    /// title, its note, the options grouped (a group title over them), one row each (slider, toggle or select), wrapped to the
    /// next column, then to a new page; <c>onPage</c> adds the window's own items to every page (header buttons...). The
    /// getter returns the values edited (rounded to their step, within their bounds; keys of the values not in the schema
    /// kept), to be written by ModOptionsHelper.WriteValues.
    /// </summary>
    public static class ModOptionsPage
    {
        public static Color TitleColor { get; set; } = new Color(0.45f, 0.12f, 0.08f);

        public static Func<Dictionary<string, object>> AddPages(UICustomBase ui, string modId, ModOptions schema, IDictionary<string, object> values,
            int[] columns = null, Action<UICustomBase> onPage = null)
        {
            values = values ?? new Dictionary<string, object>();
            var rows = new List<KeyValuePair<ModOption, UIItemComposite>>();
            if (ui == null || schema == null) return () => new Dictionary<string, object>(values);
            columns = (columns ?? new[] { 2, 16 }).Where(c => c >= 0 && c < ui.Columns.Count).DefaultIfEmpty(0).ToArray();
            int ci = 0, row = 0;
            void NewPage()
            {
                ui.AddPage();
                onPage?.Invoke(ui);
                ci = 0;
                row = 0;
            }
            // the next row (wrapped to the next column, then to a new page)
            int Next()
            {
                if (row > ui.LastRow)
                {
                    ci++;
                    row = 0;
                    if (ci >= columns.Length) NewPage();
                }
                return row++;
            }
            NewPage();
            // the row first: Next() may move to the next column (columns[ci] read before it put the text in the column before)
            int row0;
            if (!string.IsNullOrEmpty(schema.Title))
            {
                row0 = Next();
                ui.AddText(columns[ci], row0, GameTool.LS(schema.Title)).Format(TitleColor, 20, FontStyle.Bold).Align(TextAnchor.MiddleRight);
            }
            if (!string.IsNullOrEmpty(schema.Note))
            {
                row0 = Next();
                ui.AddText(columns[ci], row0, GameTool.LS(schema.Note)).Format(Color.black, 14).Align(TextAnchor.MiddleRight);
            }
            string group = null;
            foreach (var o in schema.Options.Where(x => x?.Key != null))
            {
                try
                {
                    if (!string.IsNullOrEmpty(o.Group) && o.Group != group)
                    {
                        group = o.Group;
                        var gr = Next();
                        ui.AddText(columns[ci], gr, GameTool.LS(group)).Format(TitleColor, 17, FontStyle.Bold).Align(TextAnchor.MiddleRight);
                    }
                    var v = ModOptionsHelper.Clamp(o, values.TryGetValue(o.Key, out var raw) ? raw : null);
                    var label = string.IsNullOrEmpty(o.Label) ? o.Key : GameTool.LS(o.Label);
                    var hint = string.IsNullOrEmpty(o.Desc) ? null : GameTool.LS(o.Desc);
                    var r = Next();
                    UIItemComposite c;
                    switch ((o.Type ?? "float").ToLowerInvariant())
                    {
                        case "bool":
                            c = ui.AddCompositeToggle(columns[ci], r, label, (bool)v, hint);
                            break;
                        case "select":
                            c = ui.AddCompositeSelect(columns[ci], r, label, (o.Choices ?? new List<string>()).Select(k => GameTool.LS(k)).ToArray(), (int)v, hint);
                            break;
                        default:
                            c = ui.AddCompositeSlider(columns[ci], r, label, (float)o.Min, (float)o.Max, Convert.ToSingle(v, CultureInfo.InvariantCulture), hint);
                            break;
                    }
                    rows.Add(new KeyValuePair<ModOption, UIItemComposite>(o, c));
                }
                catch (Exception ex) { DebugHelper.WriteLine($"Mod option {modId} {o.Key}: {ex.Message}"); }
            }
            return () =>
            {
                var rs = new Dictionary<string, object>(values);
                foreach (var x in rows)
                {
                    try { rs[x.Key.Key] = ModOptionsHelper.Clamp(x.Key, x.Value.Get()); }
                    catch (Exception ex) { DebugHelper.WriteLine($"Mod option {modId} {x.Key.Key}: {ex.Message}"); }
                }
                return rs;
            };
        }
    }
}
