using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Taoist log of the save: what the mod did to the player month by month (taxes, fees, market deals, loans,
    /// encounters, stalkers...). Entries keep a localText key and its arguments, so they are shown in the current
    /// language. Opened from Taoist Settings, and shown in the month log window of the game.
    /// </summary>
    [Cache(ModConst.TAOIST_LOG_EVENT)]
    public class TaoistLogEvent : ModEvent
    {
        public static TaoistLogEvent Instance { get; set; }

        public const int MAX_ENTRIES = 300;
        public const int ROWS_PER_PAGE = 22;

        public class Entry
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public string Key { get; set; }
            public object[] Args { get; set; }

            /// <summary>The text in the current language.</summary>
            public string RenderText()
            {
                try { return Args == null || Args.Length == 0 ? GameTool.LS(Key) : string.Format(GameTool.LS(Key), Args); }
                catch (FormatException) { return GameTool.LS(Key); }
            }

            /// <summary>"Y1 M2: text" in the current language.</summary>
            public string Render() => string.Format(GameTool.LS("log_line"), Year, Month, RenderText());
        }

        public List<Entry> Entries { get; set; } = new List<Entry>();

        /// <summary>Adds an entry (no-op outside a save).</summary>
        public static void Log(string key, params object[] args)
        {
            if (Instance == null || !GameHelper.IsInGame())
                return;
            var entry = new Entry { Year = GameHelper.GetGameYear(), Month = GameHelper.GetGameMonth(), Key = key, Args = args };
            Instance.Entries.Add(entry);
            if (Instance.Entries.Count > MAX_ENTRIES)
                Instance.Entries.RemoveRange(0, Instance.Entries.Count - MAX_ENTRIES);
        }

        /// <summary>Adds an entry when the condition holds (events of the world that concern the player).</summary>
        public static void LogIf(bool cond, string key, params object[] args)
        {
            if (cond) Log(key, args);
        }

        /// <summary>
        /// Month log window of the game (map): the Taoist entries of the month shown are added under its own lines
        /// (the window is built by the game from its own events, it cannot hold other lines).
        /// </summary>
        public override void OnOpenUIEnd(EGameTypeData.OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName != UIType.MonthLog.uiName)
                return;
            var ui = new UICover<UIMonthLog>(e.ui);
            // one block of text next to the right edge of the window (its close button), growing downwards
            var text = ui.AddText(0, 0, "{0}").Format(Color.white, 15).Align(TextAnchor.UpperLeft);
            var close = ui.UI.btnClose.GetComponent<RectTransform>();
            var rt = text.Item.rectTransform;
            rt.SetParent(close.parent, false);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchorMin = rt.anchorMax = close.anchorMin;
            rt.sizeDelta = new Vector2(440f, 600f);
            rt.anchoredPosition = close.anchoredPosition + new Vector2(close.rect.width / 2f + 16f, -close.rect.height / 2f);
            text.Item.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.Item.verticalOverflow = VerticalWrapMode.Overflow;
            text.SetWork(new UIItemWork
            {
                Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x =>
                    new object[] { $"<b>{GameTool.LS("log_title")}</b>\n" + string.Join("\n", ShownMonthLines(ui.UI)) })
            });
            ui.IsAutoUpdate = true;
        }
        public const int MONTH_ROWS = 18;

        /// <summary>Taoist lines of the month shown by the month log window (current month when unknown).</summary>
        public static List<string> ShownMonthLines(UIMonthLog ui)
        {
            int year = GameHelper.GetGameYear(), month = GameHelper.GetGameMonth();
            try
            {
                var shown = ui?.monthLog != null && ui.curMonthIndex >= 0 && ui.curMonthIndex < ui.monthLog.Count ? ui.monthLog[ui.curMonthIndex].month : -1;
                if (shown >= 0) { year = shown / 12 + 1; month = shown % 12 + 1; }
            }
            catch { }
            var lines = OfMonth(year, month).Select(x => "• " + x.RenderText()).ToList();
            if (lines.Count == 0) lines.Add(GameTool.LS("log_empty"));
            return lines;
        }
        /// <summary>Entries of the month (current one by default).</summary>
        public static List<Entry> OfMonth(int year, int month) =>
            Instance?.Entries.Where(x => x.Year == year && x.Month == month).ToList() ?? new List<Entry>();

        /// <summary>Log window: newest first, page by page.</summary>
        public void OpenLog()
        {
            var ui = new UICustom1(GameTool.LS("log_title"));
            var lines = Entries.AsEnumerable().Reverse().Select(x => x.Render()).ToList();
            if (lines.Count == 0)
                lines.Add(GameTool.LS("log_empty"));
            var pages = Math.Max(1, (lines.Count + ROWS_PER_PAGE - 1) / ROWS_PER_PAGE);
            var page = 0;
            var rows = new List<UIItemText>();
            var col = ui.FirstCol + 1;
            for (var i = 0; i < ROWS_PER_PAGE; i++)
                rows.Add(ui.AddText(col, ui.FirstRow + i, string.Empty).Format(null, 14).Align(TextAnchor.MiddleLeft));
            var pageText = ui.AddText(ui.MidCol, ui.LastRow, string.Empty).Format(null, 14);
            Action show = () =>
            {
                for (var i = 0; i < ROWS_PER_PAGE; i++)
                {
                    var idx = page * ROWS_PER_PAGE + i;
                    rows[i].Item.Set(idx < lines.Count ? lines[idx] : string.Empty);
                }
                pageText.Item.Set($"{page + 1} / {pages}");
            };
            ui.AddButton(ui.MidCol - 4, ui.LastRow, () => { if (page > 0) { page--; show(); } }, "<").Size(60, 32);
            ui.AddButton(ui.MidCol + 4, ui.LastRow, () => { if (page < pages - 1) { page++; show(); } }, ">").Size(60, 32);
            // left aligned rows: their rect starts at the column
            foreach (var r in rows)
                r.Pos(r.Item.rectTransform.anchoredPosition.x + r.Item.rectTransform.rect.width / 2f, r.Item.rectTransform.anchoredPosition.y);
            show();
        }
    }
}
