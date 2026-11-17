using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Time
{
    /// <summary>
    /// TX-07 (TaoistLogEvent): the month log window of the game shows the Taoist entries of the month it shows: an entry
    /// is added, one month passes, the month log window (previous month) shows it. Changes the save (restored by run-autotest.ps1).
    /// </summary>
    public class TX07_GameMonthLog : IScenarioDefinition
    {
        /// <summary>Texts of the month log window; when the entry is not there, shows the previous month (read at the next step: the Taoist block updates on the next frame).</summary>
        private static void Shown(AutoTestContext c, string text, bool last)
        {
            var ui = g.ui.GetUI<UIMonthLog>(UIType.MonthLog);
            if (ui == null) { if (last) c.Assert(c.Bag.ContainsKey("found"), $"the Taoist entry is in the month log: {text}"); return; }
            var all = ui.GetComponentsInChildren<UnityEngine.UI.Text>().Select(x => x.text)
                .Concat(ui.GetComponentsInChildren<TMPro.TextMeshProUGUI>().Select(x => x.text)).Where(x => !string.IsNullOrEmpty(x)).ToList();
            c.Info($"month log: {string.Join(" | ", all.Take(12))}");
            if (all.Any(x => x.Contains(text ?? "?")))
            {
                c.Bag["found"] = true;
                c.Assert(true, $"the Taoist entry is in the month log: {text}");
                CloseAll(UIType.MonthLog);
                return;
            }
            if (last)
            {
                c.Assert(false, $"the Taoist entry is in the month log: {text}");
                CloseAll(UIType.MonthLog);
                return;
            }
            ui.btnMonthLeft.onClick.Invoke();
        }

        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TX-07";

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            string text = null;
            return new Scenario { Id = "TX-07", Feature = "TaoistLogEvent", Title = "Taoist log in the month log of the game", Overlay = false }
                .Step("entry", c =>
                {
                    var name = Npcs.First().GetName();
                    TaoistLogEvent.Log("log_team_friend", name);
                    text = string.Format(GameTool.LS("log_team_friend"), name);
                    c.Bag["month"] = GameHelper.GetGameTotalMonth();
                    TimeSkipEvent.Instance.SkipTime(1);
                })
                .Wait("next month", c => !TimeSkipEvent.Instance.IsProcessing && GameHelper.GetGameTotalMonth() > (int)c.Bag["month"], 300000, 1000) // longer than the 2 min a stuck month run of the game may last (TimeSkipEvent.RUN_TIMEOUT_MS)
                .Step("diagnose", c =>
                {
                    var pid = Player.GetUnitId();
                    var pending = g.world.unitLog.allAddLogData;
                    c.Info($"now {GameHelper.GetGameYear()}/{GameHelper.GetGameMonth()} (total {GameHelper.GetGameTotalMonth()}, before {(int)c.Bag["month"]}), pending {(pending.ContainsKey(pid) ? pending[pid].Count : 0)}");
                    var data = g.world.unitLog.GetLogDataSync(pid);
                    var all = data?.allLogData?.ToArray() ?? new DataUnitLog.LogData.LogItemData[0];
                    c.Info($"written logs: {all.Length} months; last: {string.Join(" / ", all.Reverse().Take(3).Select(x => $"m{x.month}: {x.logs.Count} logs"))}");
                    var vital = data?.allVitalLogData?.ToArray() ?? new DataUnitLog.LogData.LogItemData[0];
                    c.Info($"vital logs: {vital.Length}");
                    c.Info($"Taoist entries: {TaoistLogEvent.Instance?.Entries.Count}: {string.Join(" / ", (TaoistLogEvent.Instance?.Entries ?? new System.Collections.Generic.List<TaoistLogEvent.Entry>()).Select(x => $"Y{x.Year} M{x.Month} {x.Key}"))}");
                }, 1000)
                .Step("open month log", c =>
                {
                    DismissDramas();
                    ClickButton(CONFIRM);
                    g.ui.OpenUISafe<UIMonthLog>(UIType.MonthLog);
                }, 2000)
                .Step("shown", c => Shown(c, text, false), 1000)
                .Step("previous month", c => Shown(c, text, false), 1000)
                .Step("2 months before", c => Shown(c, text, true));
        }
    }
}
