using MelonLoader;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ModLib.Helper
{
    /// <summary>
    /// Sends the exceptions logged by DebugHelper.WriteLine(Exception) to BugSplat, to the database of the mod where the
    /// error happened, only when the player allowed it (ModTitleEvent.CrashReportConsent).
    /// Each mod using ModLib sets its own database in its ModConf/crash_reports.json (no file: its errors are not sent):
    ///   { "Database": "my-bugsplat-db", "ModName": "My mod", "LogLines": 200 }
    /// The mod of an error: the first MOD_&lt;modId&gt; assembly in the stack of the exception (innermost first), then in
    /// the stack of the code that logged it.
    /// Sent: the exception text with its stack (Callstack.txt), the last lines of the mod log (log.txt; the user folder,
    /// Windows account and player id replaced), the mod/ModLib/game versions, the loaded mod ids, the language and an
    /// anonymous random id. Not sent: saves, the Steam account.
    /// Protocol of BugSplat's .NET Standard client: getCrashUploadUrl -> PUT zip -> commitS3CrashUpload.
    /// </summary>
    [ActionCat("CrashReport")]
    public static class CrashReportHelper
    {
        /// <summary>Settings file of a mod, in its ModConf folder.</summary>
        public const string CONF_FILE = "crash_reports.json";
        /// <summary>
        /// Version of what is sent / of the consent text: increase it when it changes, the players are asked again
        /// (1: exception only, 2: + log, database of each mod).
        /// </summary>
        public const int CONSENT_VERSION = 2;
        /// <summary>Default number of log lines sent.</summary>
        public const int DEFAULT_LOG_LINES = 200;
        /// <summary>Most log text sent (the end is kept).</summary>
        public const int MAX_LOG_CHARS = 64 * 1024;
        /// <summary>Name of the log inside the uploaded zip.</summary>
        public const string LOG_FILE = "log.txt";

        /// <summary>Settings of a mod (ModConf/crash_reports.json).</summary>
        public class ModSettings
        {
            /// <summary>BugSplat database (https://&lt;database&gt;.bugsplat.com).</summary>
            public string Database { get; set; }
            /// <summary>Name shown to the player in the consent dialog and sent as BugSplat application name.</summary>
            public string ModName { get; set; }
            /// <summary>Number of the last lines of the mod log sent (0: no log).</summary>
            public int LogLines { get; set; } = DEFAULT_LOG_LINES;
        }

        private static Dictionary<string, ModSettings> _mods;
        private static readonly Dictionary<string, string> _versions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Mods sending reports (mod id -> settings): the loaded mods with a valid database in their
        /// ModConf/crash_reports.json, read once. Tests may replace the entries.
        /// </summary>
        public static Dictionary<string, ModSettings> Mods
        {
            get
            {
                if (_mods == null)
                    _mods = LoadModSettings();
                return _mods;
            }
        }

        /// <summary>Reads ModConf/crash_reports.json of the loaded mods again.</summary>
        public static void ReloadModSettings() { _mods = null; }

        private static Dictionary<string, ModSettings> LoadModSettings()
        {
            var rs = new Dictionary<string, ModSettings>(StringComparer.OrdinalIgnoreCase);
            foreach (var mod in g.mod.allModPaths)
            {
                if (!g.mod.IsLoadMod(mod.t1))
                    continue;
                try
                {
                    var file = ConfHelper.GetConfFilePath(mod.t1, CONF_FILE);
                    if (!File.Exists(file))
                        continue;
                    var s = JsonConvert.DeserializeObject<ModSettings>(File.ReadAllText(file));
                    s.Database = s.Database?.Trim();
                    if (!IsValidDatabase(s.Database))
                        continue;
                    if (string.IsNullOrWhiteSpace(s.ModName))
                        s.ModName = mod.t1;
                    rs[mod.t1] = s;
                }
                catch (Exception ex)
                {
                    DebugHelper.WriteLine($"{CONF_FILE} of {mod.t1} invalid: {ex.Message}");
                }
            }
            return rs;
        }

        /// <summary>Version of a mod sent with its reports (call it at init; default: the ModLib version).</summary>
        public static void SetModVersion(string modId, string version)
        {
            if (!string.IsNullOrEmpty(modId))
                _versions[modId] = version;
        }

        /// <summary>
        /// What the player answered for: the consent version and the mods sending reports. Another key (new mod with a
        /// database, new consent version) = asked again.
        /// </summary>
        public static string ConsentKey => $"v{CONSENT_VERSION}:{string.Join(",", Mods.Keys.Select(x => x.ToLower()).OrderBy(x => x))}";

        /// <summary>Names of the mods sending reports, for the consent dialog.</summary>
        public static List<string> ModNames => Mods.Values.Select(x => x.ModName).OrderBy(x => x).ToList();
        /// <summary>BugSplat crash type of a .NET exception (ExceptionTypeId.DotNetStandard).</summary>
        public const int DOTNET_CRASH_TYPE_ID = 18;
        /// <summary>At most this many distinct errors are sent per game session.</summary>
        public const int MAX_REPORTS_PER_SESSION = 5;
        /// <summary>BugSplat application name of the reports (the version is ModLib's).</summary>
        public const string APP_NAME = "GuiGuBaHuang-ModLib";
        /// <summary>Name of the exception text inside the uploaded zip (read by BugSplat).</summary>
        public const string CALLSTACK_FILE = "Callstack.txt";

        private static readonly Regex DATABASE_NAME = new Regex(@"^[A-Za-z0-9_\-]{1,64}$");
        private static readonly HashSet<string> _reported = new HashSet<string>();

        /// <summary>Result of the last send ("sent", "error: ..."), for the log / tests.</summary>
        public static string LastResult { get; private set; } = string.Empty;

        /// <summary>Error of the first try of the last send when it was retried (empty: first try sent), for the tests.</summary>
        public static string LastFirstTryError { get; private set; } = string.Empty;

        /// <summary>
        /// Tests only (Taoist autotest TT-06): base URL used instead of https://&lt;database&gt;.bugsplat.com, e.g. a local
        /// mock server. Null: BugSplat.
        /// </summary>
        public static string ServerUrlOverride { get; set; }

        /// <summary>Tests only: forgets the errors already sent this session and the last result.</summary>
        public static void ResetSession()
        {
            lock (_reported)
                _reported.Clear();
            LastResult = string.Empty;
        }

        /// <summary>Base URL of the BugSplat database (or ServerUrlOverride).</summary>
        public static string GetServerUrl(string database)
        {
            return string.IsNullOrEmpty(ServerUrlOverride) ? $"https://{database}.bugsplat.com" : ServerUrlOverride.TrimEnd('/');
        }

        /// <summary>A loaded mod sends reports: the player can be asked.</summary>
        public static bool IsEnabled => Mods.Count > 0;

        /// <summary>Reports are sent: the player said yes to this consent key (version + mods).</summary>
        public static bool IsAllowed => IsEnabled && ModTitleEvent.Instance?.CrashReportConsent == CrashReportConsentEnum.Allowed &&
            ModTitleEvent.Instance.CrashReportConsentKey == ConsentKey;

        /// <summary>The player has to be asked: never answered, or answered for another version / other mods.</summary>
        public static bool NeedsAsking => IsEnabled && ModTitleEvent.Instance != null &&
            (ModTitleEvent.Instance.CrashReportConsent == CrashReportConsentEnum.NotAsked || ModTitleEvent.Instance.CrashReportConsentKey != ConsentKey);

        /// <summary>A BugSplat database name (letters, digits, '-', '_'): it becomes a host name.</summary>
        public static bool IsValidDatabase(string database)
        {
            return !string.IsNullOrEmpty(database) && DATABASE_NAME.IsMatch(database);
        }

        /// <summary>
        /// Sends the exception in the background when allowed; each distinct error once per session, at most
        /// MAX_REPORTS_PER_SESSION. Never throws.
        /// Returns true when the error goes to BugSplat: sent now (in the background) or already sent this session.
        /// </summary>
        public static bool Report(Exception e, string callerInfo)
        {
            try
            {
                if (e == null || !IsAllowed)
                    return false;
                //the mod of the error: its database receives the report (no mod with a database: not sent)
                var modId = FindModOfAssemblies(GetStackAssemblies(e), Mods.Keys)
                    ?? (Mods.ContainsKey(ModMaster.ModObj.ModId) ? ModMaster.ModObj.ModId : null);
                if (modId == null)
                    return false;
                var key = DebugHelper.GetErrorKey(e);
                lock (_reported)
                {
                    if (_reported.Contains(key))
                        return true;
                    if (_reported.Count >= MAX_REPORTS_PER_SESSION)
                        return false;
                    _reported.Add(key);
                }
                var settings = Mods[modId];
                //game data is read here, on the main thread: the upload thread must not touch Il2Cpp objects
                var report = new CrashReport
                {
                    Database = settings.Database,
                    AppName = settings.ModName,
                    AppVersion = _versions.TryGetValue(modId, out var v) ? v : ModMaster.ModObj.Version,
                    User = GetAnonymousId(),
                    Description = $"{e.GetType().Name}: {e.Message}",
                    Callstack = e.ToString(),
                    Log = settings.LogLines > 0 ? ReadLogTail(DebugHelper.GetDebugFilePath(), settings.LogLines, GetPrivateValues()) : null,
                    Attributes = new Dictionary<string, string>
                    {
                        ["modId"] = modId,
                        ["modLibVersion"] = ModMaster.ModObj.Version,
                        ["caller"] = callerInfo ?? string.Empty,
                        ["gameVersion"] = UnityEngine.Application.version,
                        ["mods"] = string.Join(",", GetLoadedModIds()),
                        ["language"] = ModTitleEvent.GetTranslateLanguage() ?? "default",
                    },
                };
                Task.Run(async () =>
                {
                    LastResult = await SendAsync(report).ConfigureAwait(false);
                    if (LastResult == "sent")
                        MelonLogger.Msg($"[CrashReport] sent: {report.Description}");
                    else
                        MelonLogger.Warning($"[CrashReport] not sent: {LastResult}");
                });
                return true;
            }
            catch (Exception ex)
            {
                LastResult = $"error: {ex.Message}";
                MelonLogger.Warning($"[CrashReport] not sent: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Line shown above the exception in the error dialog: reported to BugSplat, or not sent (the player reports it
        /// to the mod author: ModLib is used by many authors, no fixed page).
        /// </summary>
        public static string GetReportNotice(bool reported)
        {
            return GameTool.LS(reported ? "libtxt999990023" : "libtxt999990024");
        }

        private static IEnumerable<string> GetLoadedModIds()
        {
            var rs = new List<string>();
            foreach (var mod in g.mod.allModPaths)
            {
                if (g.mod.IsLoadMod(mod.t1))
                    rs.Add(mod.t1);
            }
            return rs;
        }

        private static string GetAnonymousId()
        {
            var title = ModTitleEvent.Instance;
            if (title == null)
                return string.Empty;
            if (string.IsNullOrEmpty(title.CrashReportUserId))
            {
                title.CrashReportUserId = Guid.NewGuid().ToString("N");
                CacheHelper.SaveGlobalCache(title);
            }
            return title.CrashReportUserId;
        }

        /// <summary>
        /// Assembly names of the frames of the exceptions (innermost first), then of the code logging the error.
        /// </summary>
        public static IEnumerable<string> GetStackAssemblies(Exception e)
        {
            var chain = new List<Exception>();
            for (var x = e; x != null; x = x.InnerException) chain.Insert(0, x);
            var traces = chain.Select(x => new System.Diagnostics.StackTrace(x, false)).ToList();
            traces.Add(new System.Diagnostics.StackTrace(1, false));
            foreach (var t in traces)
            {
                foreach (var f in t.GetFrames() ?? new System.Diagnostics.StackFrame[0])
                {
                    string name = null;
                    try { name = f.GetMethod()?.DeclaringType?.Assembly.GetName().Name; } catch { }
                    if (name != null) yield return name;
                }
            }
        }

        /// <summary>The mod of the first assembly "MOD_&lt;modId&gt;" whose mod id is in modIds; null when none.</summary>
        public static string FindModOfAssemblies(IEnumerable<string> assemblies, IEnumerable<string> modIds)
        {
            var ids = modIds.ToList();
            foreach (var a in assemblies)
            {
                if (a == null || !a.StartsWith("MOD_", StringComparison.OrdinalIgnoreCase))
                    continue;
                var id = ids.FirstOrDefault(x => string.Equals(x, a.Substring(4), StringComparison.OrdinalIgnoreCase));
                if (id != null)
                    return id;
            }
            return null;
        }

        /// <summary>Private values replaced in the log (value -> placeholder): game data folder, user folder, Windows account, player id.</summary>
        private static List<KeyValuePair<string, string>> GetPrivateValues()
        {
            var rs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(SaveCleanupHelper.GameDataPath, "<game data>"),
                new KeyValuePair<string, string>(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "<user folder>"),
                new KeyValuePair<string, string>(Environment.UserName, "<user>"),
            };
            try
            {
                if (GameHelper.IsInGame())
                    rs.Add(new KeyValuePair<string, string>(g.world.playerUnit.GetUnitId(), "<player>"));
            }
            catch { }
            return rs;
        }

        /// <summary>
        /// Replaces the private values (with '\' or '/' as path separator, any case) by their placeholder, the longest
        /// first (the user folder is inside the game data path). Values shorter than 3 characters are kept.
        /// </summary>
        public static string SanitizeLog(string text, IEnumerable<KeyValuePair<string, string>> privateValues)
        {
            if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
            foreach (var kv in privateValues.Where(x => !string.IsNullOrEmpty(x.Key) && x.Key.Trim().Length >= 3).OrderByDescending(x => x.Key.Length))
            {
                foreach (var variant in new[] { kv.Key, kv.Key.Replace('\\', '/'), kv.Key.Replace("\\", "\\\\") }.Distinct())
                    text = Regex.Replace(text, Regex.Escape(variant), kv.Value.Replace("$", "$$"), RegexOptions.IgnoreCase);
            }
            return text;
        }

        /// <summary>The last lines of a text, at most maxChars characters (the end kept).</summary>
        public static string TailLines(string text, int lines, int maxChars = MAX_LOG_CHARS)
        {
            if (string.IsNullOrEmpty(text) || lines <= 0) return string.Empty;
            var all = text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
            var tail = string.Join("\n", all.Skip(Math.Max(0, all.Length - lines)));
            return tail.Length > maxChars ? tail.Substring(tail.Length - maxChars) : tail;
        }

        /// <summary>The last lines of the log file (shared read: the game writes it), sanitized; empty when unreadable.</summary>
        public static string ReadLogTail(string file, int lines, IEnumerable<KeyValuePair<string, string>> privateValues)
        {
            try
            {
                if (!File.Exists(file)) return string.Empty;
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                {
                    //the end of the file only (a log of a day can be big)
                    const int MAX_READ = 1024 * 1024;
                    if (fs.Length > MAX_READ) fs.Seek(-MAX_READ, SeekOrigin.End);
                    using (var r = new StreamReader(fs, Encoding.UTF8))
                        return SanitizeLog(TailLines(r.ReadToEnd(), lines), privateValues);
                }
            }
            catch { return string.Empty; }
        }

        /// <summary>Report data, read on the main thread.</summary>
        public class CrashReport
        {
            /// <summary>BugSplat database.</summary>
            public string Database { get; set; }
            /// <summary>Application name: the mod's name (crash_reports.json ModName).</summary>
            public string AppName { get; set; } = APP_NAME;
            /// <summary>Version of the mod (SetModVersion), else the ModLib version.</summary>
            public string AppVersion { get; set; }
            /// <summary>Anonymous random id of the player.</summary>
            public string User { get; set; }
            /// <summary>One line: exception type and message.</summary>
            public string Description { get; set; }
            /// <summary>Exception.ToString() (Callstack.txt).</summary>
            public string Callstack { get; set; }
            /// <summary>Last lines of the mod log, sanitized (log.txt; none when empty).</summary>
            public string Log { get; set; }
            /// <summary>Context: mod id, caller, game version, loaded mods, language.</summary>
            public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        }

        /// <summary>The uploaded zip (UTF-8 texts): Callstack.txt with the exception, log.txt with the log when not empty.</summary>
        public static byte[] BuildZip(string callstack, string log = null)
        {
            using (var ms = new MemoryStream())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    void Add(string name, string text)
                    {
                        var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
                        using (var w = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
                            w.Write(text ?? string.Empty);
                    }
                    Add(CALLSTACK_FILE, callstack);
                    if (!string.IsNullOrEmpty(log))
                        Add(LOG_FILE, log);
                }
                return ms.ToArray();
            }
        }

        /// <summary>URL of step 1 (asks BugSplat for the presigned upload URL).</summary>
        public static string GetUploadUrlRequest(CrashReport r, int zipSize)
        {
            return $"{GetServerUrl(r.Database)}/api/getCrashUploadUrl" +
                $"?database={Uri.EscapeDataString(r.Database)}" +
                $"&appName={Uri.EscapeDataString(r.AppName ?? string.Empty)}" +
                $"&appVersion={Uri.EscapeDataString(r.AppVersion ?? string.Empty)}" +
                $"&crashPostSize={zipSize}";
        }

        /// <summary>Fields of step 3 (commitS3CrashUpload), in the order of BugSplat's client.</summary>
        public static List<KeyValuePair<string, string>> GetCommitFields(CrashReport r, string s3Key, string md5)
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("database", r.Database),
                new KeyValuePair<string, string>("appName", r.AppName ?? string.Empty),
                new KeyValuePair<string, string>("appVersion", r.AppVersion ?? string.Empty),
                new KeyValuePair<string, string>("description", r.Description ?? string.Empty),
                new KeyValuePair<string, string>("user", r.User ?? string.Empty),
                new KeyValuePair<string, string>("crashTypeId", DOTNET_CRASH_TYPE_ID.ToString()),
                new KeyValuePair<string, string>("s3Key", s3Key),
                new KeyValuePair<string, string>("md5", md5 ?? string.Empty),
                new KeyValuePair<string, string>("attributes", JsonConvert.SerializeObject(r.Attributes ?? new Dictionary<string, string>())),
            };
        }

        /// <summary>The "url" of the getCrashUploadUrl answer; throws with BugSplat's "message" when missing.</summary>
        public static string ParseUploadUrl(string json)
        {
            var o = JObject.Parse(json);
            var url = o.Value<string>("url");
            if (string.IsNullOrEmpty(url))
                throw new InvalidOperationException($"no upload url: {o.Value<string>("message") ?? json}");
            return url;
        }

        /// <summary>
        /// Uploads a report (3 requests). Returns "sent" or "error: ...", never throws. No game object is used: it runs
        /// on a worker thread (and in the unit tests).
        /// </summary>
        public static async Task<string> SendAsync(CrashReport r)
        {
            //one retry: a network glitch must not lose the report (the game's Mono HttpClient also fails in
            //WebConnectionStream.EndWrite when the answer comes before it completed the write of the body: seen with a
            //local server only, TT-06)
            var result = await SendOnceAsync(r).ConfigureAwait(false);
            LastFirstTryError = result == "sent" ? string.Empty : result;
            if (result != "sent")
            {
                MelonLogger.Warning($"[CrashReport] first try failed, retrying: {result}");
                await Task.Delay(2000).ConfigureAwait(false);
                var retry = await SendOnceAsync(r).ConfigureAwait(false);
                result = retry == "sent" ? "sent" : $"{retry} (first try: {result})";
            }
            return result;
        }

        private static async Task<string> SendOnceAsync(CrashReport r)
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
                var zip = BuildZip(r.Callstack, r.Log);
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    //a new connection per request (3 requests to 2 hosts: nothing to gain from a pooled one)
                    client.DefaultRequestHeaders.ConnectionClose = true;
                    //1. presigned upload url
                    var res1 = await client.GetAsync(GetUploadUrlRequest(r, zip.Length)).ConfigureAwait(false);
                    var body1 = await res1.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!res1.IsSuccessStatusCode)
                        throw new HttpRequestException($"getCrashUploadUrl {(int)res1.StatusCode}: {body1}");
                    var uploadUrl = ParseUploadUrl(body1);

                    //2. zip -> S3
                    var put = new HttpRequestMessage(HttpMethod.Put, uploadUrl) { Content = new ByteArrayContent(zip) };
                    put.Headers.Accept.ParseAdd("application/octet-stream");
                    var res2 = await client.SendAsync(put).ConfigureAwait(false);
                    if (!res2.IsSuccessStatusCode)
                        throw new HttpRequestException($"upload {(int)res2.StatusCode}");
                    var md5 = res2.Headers.TryGetValues("ETag", out var etags) ? etags.FirstOrDefault()?.Trim('"') : string.Empty;

                    //3. commit
                    using (var form = new MultipartFormDataContent())
                    {
                        foreach (var f in GetCommitFields(r, uploadUrl, md5))
                            form.Add(new StringContent(f.Value, Encoding.UTF8), f.Key);
                        var res3 = await client.PostAsync($"{GetServerUrl(r.Database)}/api/commitS3CrashUpload", form).ConfigureAwait(false);
                        if (!res3.IsSuccessStatusCode)
                            throw new HttpRequestException($"commitS3CrashUpload {(int)res3.StatusCode}: {await res3.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                    }
                }
                return "sent";
            }
            catch (Exception ex)
            {
                //where it failed: the innermost exception and its first frames (the message alone is often generic)
                var inner = ex;
                while (inner.InnerException != null) inner = inner.InnerException;
                var at = string.Join(" < ", (inner.StackTrace ?? string.Empty).Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).Take(3));
                return $"error: {ex.Message}{(inner != ex ? $" ({inner.GetType().Name}: {inner.Message})" : string.Empty)} [{at}]";
            }
        }
    }
}
