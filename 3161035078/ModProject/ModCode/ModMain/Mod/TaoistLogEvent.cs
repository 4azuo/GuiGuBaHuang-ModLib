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

            /// <summary>"[icon] Year 1, Month 2: text" in the current language.</summary>
            public string Render() => $"{TextIconHelper.ToSprites($"[icon:{Icon(Key)}]")} {string.Format(GameTool.LS("log_line"), Year, Month, RenderText())}";
        }

        // icon of an entry by the feature of its key (sprites of the game's TextMeshPro sprite asset)
        private static readonly string[][] ICONS =
        {
            new[] { "log_tax", "zm_fanrongdu" }, new[] { "log_storage", "zm_lingshi" }, new[] { "log_loan", "zm_lingshi" },
            new[] { "log_stalker", "shanbiD" }, new[] { "log_buyorder", "yaocai" }, new[] { "log_market", "zm_lingshi" },
            new[] { "market_", "zm_lingshi" }, new[] { "log_family", "zm_renshu" }, new[] { "log_secl", "wuxing" },
            new[] { "log_story", "daodian" }, new[] { "log_wanted", "gongji" }, new[] { "log_clan", "zhandouxiaoguo" }, new[] { "log_town", "zm_andingdu" },
            new[] { "log_team", "shengwang" }, new[] { "log_refine", "lianqi" }, new[] { "log_score", "daoli" }, new[] { "enc_", "xingyun" },
        };

        /// <summary>Icon of an entry key ("xingyun" for the others).</summary>
        public static string Icon(string key) => ICONS.FirstOrDefault(x => key != null && key.StartsWith(x[0]))?[1] ?? "xingyun";

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
            var lines = OfMonth(year, month).Select(x => TextIconHelper.ToSprites($"[icon:{Icon(x.Key)}] ") + x.RenderText()).ToList();
            if (lines.Count == 0) lines.Add(GameTool.LS("log_empty"));
            return lines;
        }
        /// <summary>Entries of the month (current one by default).</summary>
        public static List<Entry> OfMonth(int year, int month) =>
            Instance?.Entries.Where(x => x.Year == year && x.Month == month).ToList() ?? new List<Entry>();

        /// <summary>Log window: newest first, page by page; 2 columns: the date, then the icon and the text.</summary>
        public void OpenLog()
        {
            var ui = new UICustom1(GameTool.LS("log_title"));
            var lines = Entries.AsEnumerable().Reverse()
                .Select(x => Tuple.Create(string.Format(GameTool.LS("log_date"), x.Year, x.Month), TextIconHelper.ToSprites($"[icon:{Icon(x.Key)}]") + " " + x.RenderText()))
                .ToList();
            if (lines.Count == 0)
                lines.Add(Tuple.Create(string.Empty, GameTool.LS("log_empty")));
            var pages = Math.Max(1, (lines.Count + ROWS_PER_PAGE - 1) / ROWS_PER_PAGE);
            var page = 0;
            var dates = new List<UIItemText>();
            var texts = new List<UIItemText>();
            for (var i = 0; i < ROWS_PER_PAGE; i++)
            {
                dates.Add(ui.AddText(ui.FirstCol + 1, ui.FirstRow + i, string.Empty).Format(new Color(0.45f, 0.12f, 0.08f), 14).Align(TextAnchor.MiddleLeft));
                texts.Add(ui.AddText(ui.FirstCol + DATE_COLS + 1, ui.FirstRow + i, string.Empty).Format(null, 14).Align(TextAnchor.MiddleLeft));
            }
            var pageText = ui.AddText(ui.MidCol, ui.LastRow, string.Empty).Format(null, 14);
            Action show = () =>
            {
                for (var i = 0; i < ROWS_PER_PAGE; i++)
                {
                    var idx = page * ROWS_PER_PAGE + i;
                    dates[i].Item.Set(idx < lines.Count ? lines[idx].Item1 : string.Empty);
                    texts[i].Item.Set(idx < lines.Count ? lines[idx].Item2 : string.Empty);
                }
                pageText.Item.Set($"{page + 1} / {pages}");
            };
            ui.AddButton(ui.MidCol - 4, ui.LastRow, () => { if (page > 0) { page--; show(); } }, "<").Size(60, 32);
            ui.AddButton(ui.MidCol + 4, ui.LastRow, () => { if (page < pages - 1) { page++; show(); } }, ">").Size(60, 32);
            // left aligned columns: the rect of each text starts at its column
            foreach (var r in dates.Concat(texts))
                r.Pos(r.Item.rectTransform.anchoredPosition.x + r.Item.rectTransform.rect.width / 2f, r.Item.rectTransform.anchoredPosition.y);
            show();
        }

        /// <summary>Grid columns of the date column of the log window.</summary>
        public const int DATE_COLS = 4;
    }
}
