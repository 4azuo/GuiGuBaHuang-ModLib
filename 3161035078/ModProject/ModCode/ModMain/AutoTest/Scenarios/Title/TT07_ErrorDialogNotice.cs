using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.AutoTest.Scenarios.Title
{
    /// <summary>
    /// TT-07 (ModMaster.ShowException + CrashReportHelper.GetReportNotice, ModLib): the exception dialog says above the
    /// exception whether the error was sent to BugSplat. Not allowed / mod without a database: "not sent, report it to the
    /// author of the mod" (no link: ModLib is used by many authors). Allowed: "reported to the mod author (BugSplat)",
    /// also for the same error again (already sent this session). Uploads go to the local stand-in server of TT-06
    /// (MockBugSplatServer). The exception dialogs are on during each error step only (AutoTestEvent turns them off).
    /// Everything changed (consent, consent key, mods sending reports, debug dialogs) is restored.
    /// </summary>
    public class TT07_ErrorDialogNotice : IScenarioDefinition
    {
        private const string MOCK_DB = "autotest-mock";
        private const string TaoistId = "nE7UL2";
        private const string ERROR_MSG = "AUTOTEST TT-07: exception dialog notice";

        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TT-07";

        private static Exception MakeError(string message)
        {
            try { throw new InvalidOperationException(message); }
            catch (Exception e) { return e; }
        }

        private static MockBugSplatServer Mock(AutoTestContext c) => c.Bag.TryGetValue("mock", out var m) ? (MockBugSplatServer)m : null;

        /// <summary>Logs the error with the exception dialog on (as for a player).</summary>
        private static UITextInfoLong LogWithDialog(AutoTestContext c, Exception e)
        {
            if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
            DebugHelper.IsDebugMode = true;
            try { DebugHelper.WriteLine(e); }
            finally { DebugHelper.IsDebugMode = false; }
            return c.Assert(g.ui.HasUI(UIType.TextInfoLong), "exception dialog opened") ? g.ui.GetUI<UITextInfoLong>(UIType.TextInfoLong) : null;
        }

        /// <summary>
        /// The note of the dialog: its own text in red, centered, at the bottom above the buttons (as the note of the
        /// S&amp;M configs), not in the exception text; the other note is not shown.
        /// </summary>
        private static void CheckNote(AutoTestContext c, UITextInfoLong ui, string noteKey, string otherKey)
        {
            if (ui == null) return;
            var note = GameTool.LS(noteKey);
            var texts = ui.GetComponentsInChildren<UnityEngine.UI.Text>().Where(t => t != null && t.gameObject.activeInHierarchy).ToList();
            var t1 = texts.FirstOrDefault(t => t.text == note);
            if (!c.Assert(t1 != null, $"note shown: {note}")) return;
            c.Assert(t1.color.r > 0.9f && t1.color.g < 0.1f && t1.color.b < 0.1f, $"note in red ({t1.color})");
            c.Assert(t1.alignment == UnityEngine.TextAnchor.MiddleCenter, $"note centered ({t1.alignment})");
            var noteY = t1.transform.position.y;
            var bodyY = ui.ptextInfo.transform.position.y;
            var btnY = ui.btnCancel.transform.position.y;
            c.Assert(noteY < bodyY && noteY > btnY, $"note at the bottom, above the buttons (y: text {bodyY:0}, note {noteY:0}, buttons {btnY:0})");
            var body = ui.ptextInfo.text ?? string.Empty;
            c.Assert(!body.Contains(note), "note not inside the exception text");
            c.Assert(body.Contains(ERROR_MSG), "the exception is shown");
            c.Assert(!texts.Any(t => t.text == GameTool.LS(otherKey)), "the other note is not shown");
            c.Assert(!texts.Any(t => (t.text ?? string.Empty).ToLower().Contains("http")), "no link in the dialog (the player contacts the mod author)");
        }

        private static void CheckNotSent(AutoTestContext c, UITextInfoLong ui) => CheckNote(c, ui, "libtxt999990024", "libtxt999990023");

        private static void CheckSent(AutoTestContext c, UITextInfoLong ui) => CheckNote(c, ui, "libtxt999990023", "libtxt999990024");

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TT-07", Feature = "CrashReportHelper (ModLib)", Title = "Exception dialog says whether the error was sent to BugSplat", Scope = ScenarioScope.Title }
                .Step("setup", c =>
                {
                    var title = ModTitleEvent.Instance;
                    c.Bag["consent"] = title.CrashReportConsent;
                    c.Bag["consentKey"] = title.CrashReportConsentKey;
                    c.Bag["mods"] = CrashReportHelper.Mods.ToList();
                    c.Bag["debugMode"] = DebugHelper.IsDebugMode;
                    var mock = new MockBugSplatServer();
                    c.Bag["mock"] = mock;
                    CrashReportHelper.Mods.Clear();
                    CrashReportHelper.Mods[TaoistId] = new CrashReportHelper.ModSettings { Database = MOCK_DB, ModName = "Taoist (autotest)", LogLines = 50 };
                    CrashReportHelper.ServerUrlOverride = mock.BaseUrl;
                    CrashReportHelper.ResetSession();
                    title.CrashReportConsent = CrashReportConsentEnum.Denied;
                    title.CrashReportConsentKey = CrashReportHelper.ConsentKey;
                    c.Assert(!CrashReportHelper.IsAllowed, "player answered No");
                    c.Info($"stand-in BugSplat server: {mock.BaseUrl}");
                })
                .Step("error, not allowed", c =>
                {
                    CheckNotSent(c, LogWithDialog(c, MakeError(ERROR_MSG + " (not allowed)")));
                    c.AssertEqual(0, Mock(c).Requests.Count, "nothing sent");
                }, delayMs: 500)
                .Step("error, allowed", c =>
                {
                    var title = ModTitleEvent.Instance;
                    title.CrashReportConsent = CrashReportConsentEnum.Allowed;
                    title.CrashReportConsentKey = CrashReportHelper.ConsentKey;
                    c.Assert(CrashReportHelper.IsAllowed, "player answered Yes");
                    CheckSent(c, LogWithDialog(c, MakeError(ERROR_MSG)));
                }, delayMs: 500)
                .Wait("uploaded", c => CrashReportHelper.LastResult != string.Empty, timeoutMs: 20000)
                .Step("upload result", c =>
                {
                    if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
                    c.AssertEqual("sent", CrashReportHelper.LastResult, "the error reached the stand-in BugSplat server");
                    c.Assert(Mock(c).Requests.Count >= 3, $"requests received: {Mock(c).Requests.Count}");
                    c.Bag["count"] = Mock(c).Requests.Count;
                })
                .Step("same error again", c =>
                {
                    CheckSent(c, LogWithDialog(c, MakeError(ERROR_MSG)));
                }, delayMs: 500)
                .Step("sent once", c =>
                {
                    if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
                    c.AssertEqual((int)c.Bag["count"], Mock(c).Requests.Count, "no new upload for the same error");
                }, delayMs: 2000)
                .Step("error, mod without database", c =>
                {
                    CrashReportHelper.Mods.Clear();
                    c.Assert(CrashReportHelper.IsAllowed == false, "no mod sends reports");
                    CheckNotSent(c, LogWithDialog(c, MakeError(ERROR_MSG + " (no database)")));
                }, delayMs: 500)
                .Step("restore", c =>
                {
                    var title = ModTitleEvent.Instance;
                    title.CrashReportConsent = (CrashReportConsentEnum)c.Bag["consent"];
                    title.CrashReportConsentKey = (string)c.Bag["consentKey"];
                    CrashReportHelper.Mods.Clear();
                    foreach (var kv in (List<KeyValuePair<string, CrashReportHelper.ModSettings>>)c.Bag["mods"])
                        CrashReportHelper.Mods[kv.Key] = kv.Value;
                    CrashReportHelper.ServerUrlOverride = null;
                    CrashReportHelper.ResetSession();
                    DebugHelper.IsDebugMode = (bool)c.Bag["debugMode"];
                    Mock(c)?.Dispose();
                    if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
                    c.AssertEqual(c.Bag["consent"], (object)title.CrashReportConsent, "player's answer restored");
                }, delayMs: 500);
        }
    }
}
