using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Title
{
    /// <summary>
    /// TT-06 (CrashReportHelper + ModTitleEvent, ModLib): error reports to the BugSplat database of the mod of the error
    /// (Taoist: ModConf/crash_reports.json), only with the player's consent for the current consent key.
    /// A local stand-in of BugSplat (MockBugSplatServer, CrashReportHelper.ServerUrlOverride) records what the game
    /// sends: nothing before the answer or after "No"; after "Yes" the 3 requests of the BugSplat protocol for Taoist
    /// with Callstack.txt + log.txt (sanitized), and the same error only once. Evidence: screenshots (consent dialog,
    /// captured requests, ModCreator window) and &lt;run folder&gt;\bugsplat\ (requests.json, uploaded zip,
    /// Callstack.txt, log.txt, commit form).
    /// Optional real upload: environment variable BUGSPLAT_TEST_DB (BugSplat database) when the game is started.
    /// Everything changed (consent, consent key, anonymous id, mods sending reports, debug dialogs) is restored.
    /// </summary>
    public class TT06_CrashReports : IScenarioDefinition
    {
        private const string MOCK_DB = "autotest-mock";
        private const string MOD_NAME = "Taoist (autotest)";
        private const string TaoistId = "nE7UL2";
        private const string ERROR_MSG = "AUTOTEST TT-06: error report test";

        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TT-06";

        private static Exception MakeError(string message)
        {
            try { throw new InvalidOperationException(message); }
            catch (Exception e) { return e; }
        }

        private static MockBugSplatServer Mock(AutoTestContext c) => c.Bag.TryGetValue("mock", out var m) ? (MockBugSplatServer)m : null;

        private static UICheckPopup Popup() => g.ui.HasUI(UIType.CheckPopup) ? g.ui.GetUI<UICheckPopup>(UIType.CheckPopup) : null;

        private static string PopupText(UICheckPopup p) =>
            string.Join(" / ", p.GetComponentsInChildren<UnityEngine.UI.Text>().Select(t => t.text?.Trim()).Where(t => !string.IsNullOrEmpty(t)));

        private static string EvidenceDir => Path.Combine(AutoTestEvent.OutDir ?? Path.GetTempPath(), "bugsplat");

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            return new Scenario { Id = "TT-06", Feature = "CrashReportHelper (ModLib)", Title = "Error reports to BugSplat only with the player's consent", Scope = ScenarioScope.Title }
                .Step("setup", c =>
                {
                    var title = ModTitleEvent.Instance;
                    c.Bag["consent"] = title.CrashReportConsent;
                    c.Bag["consentKey"] = title.CrashReportConsentKey;
                    c.Bag["userId"] = title.CrashReportUserId;
                    c.Bag["mods"] = CrashReportHelper.Mods.ToList();
                    c.Bag["debugMode"] = DebugHelper.IsDebugMode;
                    CrashReportHelper.Mods.TryGetValue(TaoistId, out var real);
                    c.Info($"Taoist {CrashReportHelper.CONF_FILE}: {(real == null ? "no database" : $"database set ({real.ModName}, {real.LogLines} log lines)")}; mods sending reports: {string.Join(", ", CrashReportHelper.Mods.Keys)}");
                    // only Taoist sends, to the stand-in server
                    var mock = new MockBugSplatServer();
                    c.Bag["mock"] = mock;
                    CrashReportHelper.Mods.Clear();
                    CrashReportHelper.Mods[TaoistId] = new CrashReportHelper.ModSettings { Database = MOCK_DB, ModName = MOD_NAME, LogLines = 100 };
                    CrashReportHelper.ServerUrlOverride = mock.BaseUrl;
                    CrashReportHelper.ResetSession();
                    DebugHelper.IsDebugMode = false;   // no exception dialog over the evidence
                    c.Info($"stand-in BugSplat server: {mock.BaseUrl} (database '{MOCK_DB}')");
                    c.Assert(CrashReportHelper.IsEnabled, "reports enabled when a mod has a database");

                    // an answer given for an older consent text / other mods is not used: asked again
                    title.CrashReportConsent = CrashReportConsentEnum.Allowed;
                    title.CrashReportConsentKey = "v1:flqrxq";
                    c.Assert(!CrashReportHelper.IsAllowed && CrashReportHelper.NeedsAsking, $"'Yes' given for another consent ({title.CrashReportConsentKey}) not used, asked again ({CrashReportHelper.ConsentKey})");
                    title.CrashReportConsent = CrashReportConsentEnum.NotAsked;
                    title.CrashReportConsentKey = null;
                    c.Assert(!CrashReportHelper.IsAllowed, "not allowed while the player was not asked");
                })
                .Step("error before the answer", c =>
                {
                    DebugHelper.WriteLine(MakeError(ERROR_MSG + " (not asked)"));
                    c.Info("an error logged before the player answered");
                })
                .Step("consent dialog", c =>
                {
                    ModTitleEvent.Instance.AskCrashReportConsent(null);
                    var p = Popup();
                    if (!c.Assert(p != null, "consent dialog opened")) return;
                    var text = PopupText(p);
                    c.Info($"dialog: {text}");
                    c.Assert(text.Contains("BugSplat"), "the dialog names the service (BugSplat)");
                    c.Assert(text.Contains(GameTool.LS("libtxt999990009")), "the dialog says where to change the answer (ModCreator)");
                    c.Assert(text.Contains(MOD_NAME), $"the dialog lists the mods sending reports ({MOD_NAME})");
                }, delayMs: 1000)
                .Step("answer No", c =>
                {
                    var p = Popup();
                    var clicked = p == null ? null : ClickIn(p, "No", "Cancel", "否", "取消");
                    c.Assert(clicked != null, $"'No' clicked ({clicked})");
                    c.AssertEqual(CrashReportConsentEnum.Denied, ModTitleEvent.Instance.CrashReportConsent, "answer saved");
                    DebugHelper.WriteLine(MakeError(ERROR_MSG + " (denied)"));
                    c.Info("an error logged after 'No'");
                })
                .Step("nothing sent", c =>
                {
                    var n = Mock(c).Requests.Count;
                    c.AssertEqual(0, n, "requests received before the answer and after 'No'");
                    c.AssertEqual(string.Empty, CrashReportHelper.LastResult, "no upload tried");
                }, delayMs: 3000)
                .Step("consent dialog again", c =>
                {
                    ModTitleEvent.Instance.AskCrashReportConsent(null);
                    c.Assert(Popup() != null, "consent dialog opened");
                }, delayMs: 1000)
                .Step("answer Yes", c =>
                {
                    var p = Popup();
                    var clicked = p == null ? null : ClickIn(p, "Yes", "OK", "是", "确定", "確定");
                    c.Assert(clicked != null, $"'Yes' clicked ({clicked})");
                    c.AssertEqual(CrashReportConsentEnum.Allowed, ModTitleEvent.Instance.CrashReportConsent, "answer saved");
                    c.Assert(CrashReportHelper.IsAllowed, "reports allowed");
                })
                .Step("error after Yes", c =>
                {
                    DebugHelper.WriteLine(MakeError(ERROR_MSG));
                    c.Info("an error logged after 'Yes'");
                })
                .Wait("uploaded", c => CrashReportHelper.LastResult != string.Empty, timeoutMs: 20000)
                .Step("requests", c =>
                {
                    var mock = Mock(c);
                    var reqs = mock.Requests;
                    mock.SaveEvidence(EvidenceDir);
                    c.Info($"evidence: {EvidenceDir}");
                    c.AssertEqual("sent", CrashReportHelper.LastResult, "result of the upload");
                    c.Bag["count"] = reqs.Count;
                    if (!c.Assert(reqs.Count >= 3, $"requests (upload url, zip, commit): {reqs.Count}")) return;
                    if (reqs.Count > 3) c.Info($"{reqs.Count - 3} request(s) of a first try (retried): the last 3 are checked. First try: {CrashReportHelper.LastFirstTryError}");
                    for (var i = 0; i < reqs.Count; i++)
                        c.Info($"request {i + 1}: {reqs[i].Method} {reqs[i].Path} {reqs[i].Time} connection={(reqs[i].Headers.TryGetValue("connection", out var cn) ? cn : "-")} expect={(reqs[i].Headers.TryGetValue("expect", out var ex) ? ex : "-")} length={reqs[i].BodyLength}");
                    reqs = reqs.Skip(reqs.Count - 3).ToList();

                    var get = reqs[0]; var put = reqs[1]; var post = reqs[2];
                    c.Assert(get.Method == "GET" && get.Path == "/api/getCrashUploadUrl", $"1. {get.Method} {get.Path}");
                    var qp = get.QueryParams();
                    c.AssertEqual(MOCK_DB, qp.TryGetValue("database", out var db) ? db : null, "1. database");
                    c.AssertEqual(MOD_NAME, qp.TryGetValue("appName", out var app) ? app : null, "1. appName = name of the mod of the error (crash_reports.json ModName)");
                    c.AssertEqual(ModConst.TAOIST_VERSION, qp.TryGetValue("appVersion", out var ver) ? ver : null, "1. appVersion = Taoist version (SetModVersion)");
                    c.AssertEqual(put.BodyLength.ToString(), qp.TryGetValue("crashPostSize", out var size) ? size : null, "1. crashPostSize = size of the uploaded zip");

                    c.Assert(put.Method == "PUT" && put.Path == MockBugSplatServer.UPLOAD_PATH, $"2. {put.Method} {put.Path} (the presigned url)");
                    c.Assert(put.ZipEntries().SequenceEqual(new[] { CrashReportHelper.CALLSTACK_FILE, CrashReportHelper.LOG_FILE }), $"2. zip holds Callstack.txt and log.txt ({string.Join(", ", put.ZipEntries())})");
                    var callstack = put.ZipCallstack() ?? string.Empty;
                    c.Assert(callstack.Contains("InvalidOperationException") && callstack.Contains(ERROR_MSG), "2. Callstack.txt has the exception and its message");
                    c.Assert(callstack.Contains(" at "), "2. Callstack.txt has the stack trace");
                    var log = put.ZipEntryText(CrashReportHelper.LOG_FILE) ?? string.Empty;
                    var logLines = log.Split('\n').Length;
                    c.Assert(log.Contains(ERROR_MSG), $"2. log.txt has the lines of the error ({logLines} lines, {log.Length} chars)");
                    c.Assert(logLines <= 100 && log.Length <= CrashReportHelper.MAX_LOG_CHARS, "2. log.txt cut to LogLines (100) / MAX_LOG_CHARS");
                    var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    c.Assert(!log.ToLower().Contains(profile.ToLower()) && !log.ToLower().Contains(profile.Replace('\\', '/').ToLower()), "2. log.txt: user folder removed");
                    c.Assert(string.IsNullOrEmpty(Environment.UserName) || !log.ToLower().Contains(Environment.UserName.ToLower()), "2. log.txt: Windows account removed");

                    c.Assert(post.Method == "POST" && post.Path == "/api/commitS3CrashUpload", $"3. {post.Method} {post.Path}");
                    var f = post.FormFields();
                    string F(string k) => f.TryGetValue(k, out var v) ? v : null;
                    c.AssertEqual("18", F("crashTypeId"), "3. crashTypeId (.NET exception)");
                    c.AssertEqual(mock.UploadUrl, F("s3Key"), "3. s3Key = upload url");
                    c.AssertEqual(MockBugSplatServer.ETAG, F("md5"), "3. md5 = ETag of the upload");
                    c.AssertEqual(MOCK_DB, F("database"), "3. database");
                    c.Assert((F("description") ?? string.Empty).Contains(ERROR_MSG), "3. description = exception type and message");
                    var user = F("user") ?? string.Empty;
                    c.Assert(user.Length == 32 && user == ModTitleEvent.Instance.CrashReportUserId, $"3. user = anonymous random id ({user})");
                    JObject attrs = null;
                    try { attrs = JObject.Parse(F("attributes") ?? "{}"); } catch { }
                    c.Assert(attrs != null && new[] { "modId", "modLibVersion", "caller", "gameVersion", "mods", "language" }.All(k => attrs[k] != null), $"3. attributes: {attrs?.ToString(Newtonsoft.Json.Formatting.None)}");
                    c.AssertEqual(TaoistId, attrs?.Value<string>("modId"), "3. modId = the mod of the error (MOD_nE7UL2 in the stack)");
                    c.Assert((attrs?.Value<string>("caller") ?? string.Empty).Contains("TT06_CrashReports"), "3. caller = where the error was logged");

                    // privacy: neither the Windows account nor the game data folder (saves, logs) is in what is sent
                    var all = string.Join("\n", reqs.Select(r => r.Query + "\n" + (r.ZipEntries().Count > 0
                        ? string.Join("\n", r.ZipEntries().Select(r.ZipEntryText)) : Encoding.UTF8.GetString(r.Body)))).ToLower();
                    c.Assert(string.IsNullOrEmpty(Environment.UserName) || !all.Contains(Environment.UserName.ToLower()), "nothing sent contains the Windows account name");
                    c.Assert(!all.Contains(SaveCleanupHelper.GameDataPath.ToLower()), "nothing sent contains the game data path (saves, logs)");
                    c.Assert(File.Exists(Path.Combine(EvidenceDir, "requests.json")), "evidence written (requests.json, zip, Callstack.txt, log.txt)");
                    c.Bag["callstack"] = callstack;
                    c.Bag["log"] = log;
                })
                .Step("captured requests", c =>
                {
                    var mock = Mock(c);
                    var sb = new StringBuilder();
                    sb.AppendLine("What the game sent (stand-in BugSplat server, 127.0.0.1):");
                    foreach (var r in mock.Requests)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"{r.Method} {r.Path}  ->  {r.Status}");
                        if (r.Method == "GET")
                            foreach (var kv in r.QueryParams()) sb.AppendLine($"    {kv.Key} = {kv.Value}");
                        else if (r.Method == "PUT")
                            sb.AppendLine($"    zip {r.BodyLength} bytes: {string.Join(", ", r.ZipEntries())}");
                        else
                            foreach (var kv in r.FormFields()) sb.AppendLine($"    {kv.Key} = {(kv.Key == "s3Key" ? "(upload url)" : kv.Value)}");
                    }
                    sb.AppendLine();
                    sb.AppendLine("Callstack.txt:");
                    var cs = c.Bag.TryGetValue("callstack", out var o) ? (string)o : "(none)";
                    foreach (var l in cs.Split('\n').Take(4)) sb.AppendLine("    " + l.TrimEnd());
                    var lg = c.Bag.TryGetValue("log", out var lo) ? (string)lo : string.Empty;
                    var lgLines = lg.Split('\n');
                    sb.AppendLine();
                    sb.AppendLine($"log.txt ({lgLines.Length} lines, last ones):");
                    foreach (var l in lgLines.Skip(Math.Max(0, lgLines.Length - 8))) sb.AppendLine("    " + (l.Length > 150 ? l.Substring(0, 150) + "..." : l.TrimEnd()));
                    var ui = g.ui.OpenUISafe<UITextInfoLong>(UIType.TextInfoLong);
                    ui.InitData("TT-06: error report sent", sb.ToString().Replace("<", "‹").Replace(">", "›"), "OK", ActionHelper.TracedIl2Action(() => { }), true);
                    ui.ptextInfo.fontSize = 13;
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "captured requests shown");
                })
                .Step("same error again", c =>
                {
                    if (g.ui.HasUI(UIType.TextInfoLong)) g.ui.CloseUI(UIType.TextInfoLong);
                    DebugHelper.WriteLine(MakeError(ERROR_MSG));
                    c.Info("the same error logged again");
                })
                .Step("sent once", c =>
                {
                    var before = c.Bag.TryGetValue("count", out var n) ? (int)n : 3;
                    c.AssertEqual(before, Mock(c).Requests.Count, "no new request for the same error (sent once per session)");
                }, delayMs: 3000)
                .Step("ModCreator window", c =>
                {
                    var btn = FindButtons(UIType.Login, GameTool.LS("libtxt999990009")).FirstOrDefault();
                    if (!c.Assert(btn != null, "ModCreator tools button")) return;
                    btn.Item.onClick.Invoke();
                })
                .Step("answer shown", c =>
                {
                    var ui = g.ui.HasUI(UIType.TextInfo) ? g.ui.GetUI<UIBase>(UIType.TextInfo) : null;
                    if (!c.Assert(ui != null, "tools window opened")) return;
                    c.Assert(FindTexts(ui, GameTool.LS("libtxt999990019")).Any(), "'Error reports' row");
                    c.Assert(FindTexts(ui, GameTool.LS("libtxt999990021")).Any(), "the row shows the answer (allowed)");
                })
                .Step("real BugSplat", c =>
                {
                    if (g.ui.HasUI(UIType.TextInfo)) g.ui.CloseUI(UIType.TextInfo);
                    var live = Environment.GetEnvironmentVariable("BUGSPLAT_TEST_DB");
                    c.Bag["live"] = CrashReportHelper.IsValidDatabase(live) ? live : null;
                    if (c.Bag["live"] == null)
                    {
                        c.Skip("real upload not tried: set the environment variable BUGSPLAT_TEST_DB (BugSplat database) before run-autotest.ps1");
                        return;
                    }
                    CrashReportHelper.Mods[TaoistId].Database = live;
                    CrashReportHelper.ServerUrlOverride = null;
                    CrashReportHelper.ResetSession();
                    DebugHelper.WriteLine(MakeError($"{ERROR_MSG} (real upload {DateTime.Now:yyyyMMdd-HHmmss})"));
                    c.Info($"sending to https://{live}.bugsplat.com");
                })
                .Wait("real upload", c => c.Bag["live"] == null || CrashReportHelper.LastResult != string.Empty, timeoutMs: 40000)
                .Step("real result", c =>
                {
                    if (c.Bag["live"] == null) { c.Skip("no real upload"); return; }
                    c.AssertEqual("sent", CrashReportHelper.LastResult, "accepted by BugSplat");
                    c.Info(string.IsNullOrEmpty(CrashReportHelper.LastFirstTryError) ? "sent at the first try" : $"first try failed: {CrashReportHelper.LastFirstTryError}");
                    c.Info($"crashes: https://app.bugsplat.com/v2/crashes?database={c.Bag["live"]}");
                })
                .Step("restore", c =>
                {
                    var title = ModTitleEvent.Instance;
                    title.CrashReportConsent = (CrashReportConsentEnum)c.Bag["consent"];
                    title.CrashReportConsentKey = (string)c.Bag["consentKey"];
                    title.CrashReportUserId = (string)c.Bag["userId"];
                    CacheHelper.SaveGlobalCache(title);
                    CrashReportHelper.Mods.Clear();
                    foreach (var kv in (List<KeyValuePair<string, CrashReportHelper.ModSettings>>)c.Bag["mods"])
                        CrashReportHelper.Mods[kv.Key] = kv.Value;
                    CrashReportHelper.ServerUrlOverride = null;
                    CrashReportHelper.ResetSession();
                    DebugHelper.IsDebugMode = (bool)c.Bag["debugMode"];
                    Mock(c)?.Dispose();
                    foreach (var t in new[] { UIType.CheckPopup, UIType.TextInfo, UIType.TextInfoLong })
                        if (g.ui.HasUI(t)) g.ui.CloseUI(t);
                    c.AssertEqual(c.Bag["consent"], (object)title.CrashReportConsent, "player's answer restored");
                    c.AssertEqual(((List<KeyValuePair<string, CrashReportHelper.ModSettings>>)c.Bag["mods"]).Count, CrashReportHelper.Mods.Count, "mods sending reports restored");
                });
        }
    }
}
