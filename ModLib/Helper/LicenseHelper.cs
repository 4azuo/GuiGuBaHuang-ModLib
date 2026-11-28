using ModLib.Attributes;
using ModLib.Object;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ModLib.Helper
{
    /// <summary>
    /// Licensed mods: a mod that only runs with a key its author signed (supporters). A mod asks for a key with constants of
    /// its entry class:
    ///   namespace MOD_&lt;id&gt; { public class ModMain ... {
    ///       public const string LICENSE_PUBLIC_KEY = "&lt;base64url of the RSA-2048 modulus, exponent 65537&gt;";
    ///       public const string LICENSE_NAME = "My Mod";              //optional: the name shown on the title screen
    ///       public const string LICENSE_URL = "https://.../claim";    //optional: where a key is got ("Get a key")
    ///       public const string LICENSE_SUPPORT_URL = "https://ko-fi.com/me"; //optional: where to support ("Support"; else SUPPORT_URL, SupportHelper)
    ///       public const string LICENSE_INFO = "mymod_license_info";  //optional: localText key of the mod: what support gets a key
    ///       public const string LICENSE_QR = "kofi-qr.png";           //optional: image of ModImg of the mod (QR code of the support page; else SUPPORT_QR)
    ///       public const string LICENSE_RENEW_URL = "https://.../renew"; //optional: renewal of the key (RENEW_DAYS before its end)
    ///   } }
    /// Key: "&lt;payload&gt;.&lt;signature&gt;" (base64url, like a JWT without header): payload = JSON
    /// {"m": mod id, "n": supporter name, "e": e-mail, "i": id of the payment, "t": tier, "iat": issued (unix s), "exp": expiry (unix s, 0 = never)},
    /// signature = RSASSA-PKCS1-v1_5 SHA-256 of the ASCII payload part. The check is offline: only the public key is in the mod.
    /// Renewal (LICENSE_RENEW_URL): a key that ends within RENEW_DAYS (in the background) or has ended (at once, RENEW_TIMEOUT_MS
    /// at most) is sent to the server of the mod, which answers the newest key of the same e-mail when a newer payment came (a membership
    /// still paid); it is checked here like any key and kept. No network, no newer key: the key stays as it is.
    /// Without a valid key (file <see cref="KEY_FILE"/> of the global cache folder of the mod) ModLib loads no object of the mod
    /// (CacheHelper: no ModChild, no event, so no Harmony patch of the mod either) and the title screen gets a button to enter
    /// a key (ModTitleEvent); a new key works after a restart of the game.
    /// Like PRIVATE_MOD, it keeps the honest players honest: anyone can remove the check from a DLL.
    /// </summary>
    [ActionCat("License")]
    public static class LicenseHelper
    {
        /// <summary>Constants (string) of the entry class MOD_&lt;id&gt;.ModMain.</summary>
        public const string CONST_PUBLIC_KEY = "LICENSE_PUBLIC_KEY", CONST_NAME = "LICENSE_NAME", CONST_URL = "LICENSE_URL",
            CONST_SUPPORT_URL = "LICENSE_SUPPORT_URL", CONST_INFO = "LICENSE_INFO", CONST_QR = "LICENSE_QR", CONST_RENEW_URL = "LICENSE_RENEW_URL";
        /// <summary>The key of a mod, in its global cache folder (outside the Workshop folder: an update keeps it).</summary>
        public const string KEY_FILE = "license.key";
        /// <summary>
        /// The latest time seen for a mod (unix seconds, next to its key): the clock of the machine, the Date of the answers
        /// of the server, the issue time of the key. The end of a key is checked against the latest of them, never an earlier
        /// time: a clock set back does not bring an ended key back.
        /// </summary>
        public const string TIME_FILE = "license.time";
        /// <summary>A key is renewed when it ends within this many days (or has ended).</summary>
        public const int RENEW_DAYS = 7;
        /// <summary>The longest wait for the renewal of an ended key, at the start of the game.</summary>
        public const int RENEW_TIMEOUT_MS = 5000;

        public enum Renewal { None, Background, Now }

        public enum Status { NotRequired, Valid, Missing, Malformed, BadSignature, WrongMod, Expired, BadPublicKey, Revoked }

        /// <summary>What the check of a key found.</summary>
        public class Result
        {
            public string ModId { get; set; }
            public Status Status { get; set; }
            /// <summary>The supporter (payload "n").</summary>
            public string Name { get; set; }
            /// <summary>The e-mail of the supporter (payload "e"; null in the keys made before it).</summary>
            public string Email { get; set; }
            /// <summary>Id of the payment (payload "i").</summary>
            public string PaymentId { get; set; }
            public string Tier { get; set; }
            public DateTime? Issued { get; set; }
            /// <summary>Null: never expires.</summary>
            public DateTime? Expires { get; set; }
            /// <summary>The mod may run (no key asked, or a valid one).</summary>
            public bool IsUnlocked => Status == Status.NotRequired || Status == Status.Valid;
            public override string ToString() => $"{ModId}: {Status}" + (Status == Status.Valid || Status == Status.Expired
                ? $" ({Name}, {Tier}, until {(Expires.HasValue ? Expires.Value.ToString("yyyy-MM-dd") : "never")})" : string.Empty);
        }

        /// <summary>A licensed mod as read from its entry class.</summary>
        public class Info
        {
            public string ModId { get; set; }
            public string PublicKey { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public string SupportUrl { get; set; }
            /// <summary>localText key of the mod: what support gets a key.</summary>
            public string InfoKey { get; set; }
            /// <summary>Image of ModImg of the mod: QR code of the support page.</summary>
            public string QrImage { get; set; }
            /// <summary>Renewal of the key (server of the mod); null: none.</summary>
            public string RenewUrl { get; set; }
            public Result Result { get; set; }
        }

        //mod id -> licensed mod (read once, when ModLib lists the cache types of the mods)
        private static readonly Dictionary<string, Info> _mods = new Dictionary<string, Info>(StringComparer.OrdinalIgnoreCase);

        /// <summary>The licensed mods found (valid or not).</summary>
        public static IReadOnlyCollection<Info> Mods => _mods.Values;

        /// <summary>A licensed mod (its key: supporter, expiry); null: not licensed, or not read yet.</summary>
        public static Info Get(string modId) => modId != null && _mods.TryGetValue(modId, out var i) ? i : null;

        /// <summary>The licensed mods without a valid key (their objects are not loaded).</summary>
        public static List<Info> LockedMods() => _mods.Values.Where(x => !x.Result.IsUnlocked).ToList();

        #region pure (unit tests)

        /// <summary>The value of a string constant of a type; null without it.</summary>
        public static string ReadStringConst(Type entry, string name)
        {
            var f = entry?.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (f == null || !f.IsLiteral || f.FieldType != typeof(string))
                return null;
            return (string)f.GetRawConstantValue();
        }

        /// <summary>A pasted key without its spaces, line breaks, quotes.</summary>
        public static string CleanKey(string key) =>
            key == null ? null : new string(key.Where(c => !char.IsWhiteSpace(c) && c != '"' && c != '\'').ToArray());

        public static byte[] FromBase64Url(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
                case 1: throw new FormatException("bad base64url length");
            }
            return Convert.FromBase64String(s);
        }

        public static string ToBase64Url(byte[] data) => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        //DER of the DigestInfo of SHA-256 (RFC 8017 9.2 note 1)
        private static readonly byte[] SHA256_PREFIX = { 0x30, 0x31, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01, 0x05, 0x00, 0x04, 0x20 };

        /// <summary>Big-endian unsigned bytes -> BigInteger.</summary>
        private static BigInteger ToBigInteger(byte[] be)
        {
            var le = new byte[be.Length + 1];
            for (var i = 0; i < be.Length; i++) le[i] = be[be.Length - 1 - i];
            return new BigInteger(le);
        }

        /// <summary>BigInteger -> big-endian unsigned bytes of <paramref name="len"/> bytes; null when it does not fit.</summary>
        private static byte[] ToBytes(BigInteger v, int len)
        {
            var le = v.ToByteArray();
            var n = le.Length;
            while (n > 0 && le[n - 1] == 0) n--;
            if (n > len) return null;
            var be = new byte[len];
            for (var i = 0; i < n; i++) be[len - 1 - i] = le[i];
            return be;
        }

        /// <summary>
        /// RSASSA-PKCS1-v1_5 SHA-256 check (RFC 8017 8.2.2) with BigInteger only: no crypto provider of the runtime
        /// (MelonLoader's Mono) is needed.
        /// </summary>
        public static bool VerifyRsaSha256(byte[] data, byte[] signature, byte[] modulus, int exponent = 65537)
        {
            if (data == null || signature == null || modulus == null) return false;
            var k = modulus.SkipWhile(b => b == 0).Count();
            if (k < 128 || signature.Length != k) return false;
            var n = ToBigInteger(modulus);
            var s = ToBigInteger(signature);
            if (s >= n) return false;
            var em = ToBytes(BigInteger.ModPow(s, exponent, n), k);
            if (em == null) return false;
            byte[] hash;
            using (var sha = SHA256.Create()) hash = sha.ComputeHash(data);
            var t = SHA256_PREFIX.Concat(hash).ToArray();
            if (k < t.Length + 11) return false;
            var expected = new byte[k];
            expected[0] = 0x00;
            expected[1] = 0x01;
            for (var i = 2; i < k - t.Length - 1; i++) expected[i] = 0xff;
            expected[k - t.Length - 1] = 0x00;
            Array.Copy(t, 0, expected, k - t.Length, t.Length);
            var diff = 0;
            for (var i = 0; i < k; i++) diff |= em[i] ^ expected[i];
            return diff == 0;
        }

        private static DateTime? FromUnix(long s) => s <= 0 ? (DateTime?)null : new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(s);

        /// <summary>Checks a key of a mod against its public key (base64url modulus) at <paramref name="utcNow"/>.</summary>
        public static Result Verify(string key, string modId, string publicKey, DateTime utcNow)
        {
            var rs = new Result { ModId = modId };
            if (string.IsNullOrWhiteSpace(publicKey)) { rs.Status = Status.NotRequired; return rs; }
            byte[] modulus;
            try { modulus = FromBase64Url(CleanKey(publicKey)); }
            catch { rs.Status = Status.BadPublicKey; return rs; }
            if (modulus.SkipWhile(b => b == 0).Count() < 128) { rs.Status = Status.BadPublicKey; return rs; }

            key = CleanKey(key);
            if (string.IsNullOrEmpty(key)) { rs.Status = Status.Missing; return rs; }
            var parts = key.Split('.');
            if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0) { rs.Status = Status.Malformed; return rs; }
            JObject payload;
            byte[] sig;
            try
            {
                sig = FromBase64Url(parts[1]);
                payload = JObject.Parse(Encoding.UTF8.GetString(FromBase64Url(parts[0])));
            }
            catch { rs.Status = Status.Malformed; return rs; }
            if (!VerifyRsaSha256(Encoding.ASCII.GetBytes(parts[0]), sig, modulus)) { rs.Status = Status.BadSignature; return rs; }

            rs.Name = (string)payload["n"];
            rs.Email = (string)payload["e"];
            rs.PaymentId = (string)payload["i"];
            rs.Tier = (string)payload["t"];
            rs.Issued = FromUnix((long?)payload["iat"] ?? 0);
            rs.Expires = FromUnix((long?)payload["exp"] ?? 0);
            if (!string.Equals((string)payload["m"], modId, StringComparison.OrdinalIgnoreCase)) { rs.Status = Status.WrongMod; return rs; }
            rs.Status = rs.Expires.HasValue && rs.Expires.Value <= utcNow ? Status.Expired : Status.Valid;
            return rs;
        }

        /// <summary>The time the end of a key is checked against: the latest of the clock, the latest time seen, the issue time of the key.</summary>
        public static DateTime EffectiveNow(DateTime utcNow, DateTime latestSeen, DateTime? issued)
        {
            var t = utcNow > latestSeen ? utcNow : latestSeen;
            return issued.HasValue && issued.Value > t ? issued.Value : t;
        }

        /// <summary>
        /// When a key is renewed: an ended key at once (the mod may then load), a key ending within RENEW_DAYS in the
        /// background (the new one counts from the next start); never without an e-mail (the server of the mod finds the payments by
        /// it) or for a key without end.
        /// </summary>
        public static Renewal NeedsRenewal(Result r, DateTime utcNow)
        {
            if (r == null || string.IsNullOrEmpty(r.Email) || !r.Expires.HasValue) return Renewal.None;
            if (r.Status == Status.Expired) return Renewal.Now;
            if (r.Status == Status.Valid && r.Expires.Value - utcNow <= TimeSpan.FromDays(RENEW_DAYS)) return Renewal.Background;
            return Renewal.None;
        }

        /// <summary>
        /// The key answered by the renewal is kept only when it is valid for the mod and ends later than the current one
        /// (a key of another mod, an older one, a forged one: refused).
        /// </summary>
        public static bool IsBetterKey(Result current, Result renewed)
        {
            if (renewed == null || renewed.Status != Status.Valid) return false;
            if (current == null || (current.Status != Status.Valid && current.Status != Status.Expired)) return true;
            if (!current.Expires.HasValue) return false; //the current key never ends
            return !renewed.Expires.HasValue || renewed.Expires.Value > current.Expires.Value;
        }

        #endregion

        #region game

        /// <summary>The ModChild class of a loaded mod (the class that may hold the LICENSE_* constants); null: none.</summary>
        public static Type EntryType(string modId)
        {
            var ass = AssemblyHelper.GetModRootAssembly(modId);
            return ass?.GetLoadableTypes().FirstOrDefault(x => x.IsClass && x.IsSubclassOf(typeof(ModLib.Mod.ModChild)));
        }

        /// <summary>
        /// A mod may run (IsUnlocked of its ModChild class); a mod without ModChild (not made with ModLib) or that cannot be
        /// read runs. For the parts of ModLib that load something of every mod (configs, skills).
        /// </summary>
        public static bool IsModUnlocked(string modId)
        {
            try { return IsUnlocked(modId, EntryType(modId)); }
            catch (Exception ex) { DebugHelper.WriteLine($"License: {modId} not read: {ex.Message}"); return true; }
        }

        public static string TimeFilePath(string modId) => Path.Combine(CacheHelper.GetGlobalCacheFolderName(modId), TIME_FILE);

        /// <summary>The latest time seen for a mod (TIME_FILE); MinValue: none.</summary>
        public static DateTime LatestSeen(string modId)
        {
            try
            {
                var f = TimeFilePath(modId);
                if (File.Exists(f) && long.TryParse(File.ReadAllText(f).Trim(), out var sec) && sec > 0)
                    return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(sec);
            }
            catch (Exception ex) { DebugHelper.WriteLine($"License: time of {modId} not read: {ex.Message}"); }
            return DateTime.MinValue;
        }

        private static readonly object _timeLock = new object();

        /// <summary>Keeps a time seen for a mod when it is later than the kept one (never goes back).</summary>
        public static void NoteTime(string modId, DateTime utc)
        {
            try
            {
                lock (_timeLock)
                {
                    if (utc <= LatestSeen(modId)) return;
                    var sec = (long)(utc - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                    File.WriteAllText(TimeFilePath(modId), sec.ToString());
                }
            }
            catch (Exception ex) { try { DebugHelper.WriteLine($"License: time of {modId} not kept: {ex.Message}"); } catch { } }
        }

        /// <summary>The time the keys of a mod are checked against now (EffectiveNow with the latest time seen).</summary>
        public static DateTime Now(string modId) => EffectiveNow(DateTime.UtcNow, LatestSeen(modId), null);

        //the mods whose key was revoked in this game start (notice: ShowShareNotices)
        private static readonly HashSet<string> _revoked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>A key taken back by the author (the server answered revoked): removed; the mod is locked from the next start.</summary>
        public static void Revoke(Info info)
        {
            try { var f = KeyFilePath(info.ModId); if (File.Exists(f)) File.Delete(f); }
            catch (Exception ex) { try { DebugHelper.WriteLine($"License: key of {info.ModId} not removed: {ex.Message}"); } catch { } }
            info.Result = new Result { ModId = info.ModId, Status = Status.Revoked, Name = info.Result?.Name, Email = info.Result?.Email };
            lock (_share) _revoked.Add(info.ModId);
        }

        /// <summary>Where the key of a mod is kept.</summary>
        public static string KeyFilePath(string modId) => Path.Combine(CacheHelper.GetGlobalCacheFolderName(modId), KEY_FILE);

        public static string ReadKey(string modId)
        {
            var f = KeyFilePath(modId);
            return File.Exists(f) ? File.ReadAllText(f) : null;
        }

        public static void WriteKey(string modId, string key) => File.WriteAllText(KeyFilePath(modId), CleanKey(key));

        /// <summary>
        /// The mod may run: its entry class asks for no key, or its key file holds a valid one. Read once per mod (kept for
        /// the title screen button); a check that fails (file, constant) locks the mod and is logged.
        /// </summary>
        public static bool IsUnlocked(string modId, Type entry)
        {
            if (string.IsNullOrEmpty(modId)) return true;
            if (_mods.TryGetValue(modId, out var known)) return known.Result.IsUnlocked;
            var publicKey = ReadStringConst(entry, CONST_PUBLIC_KEY);
            if (string.IsNullOrWhiteSpace(publicKey)) return true;
            var info = new Info
            {
                ModId = modId,
                PublicKey = publicKey,
                Name = ReadStringConst(entry, CONST_NAME) ?? modId,
                Url = ReadStringConst(entry, CONST_URL),
                //the support page of the mod (SupportHelper) when the license gives none
                SupportUrl = ReadStringConst(entry, CONST_SUPPORT_URL) ?? ReadStringConst(entry, SupportHelper.CONST_URL),
                InfoKey = ReadStringConst(entry, CONST_INFO),
                QrImage = ReadStringConst(entry, CONST_QR) ?? ReadStringConst(entry, SupportHelper.CONST_QR),
                RenewUrl = ReadStringConst(entry, CONST_RENEW_URL),
            };
            string key = null;
            try
            {
                key = ReadKey(modId);
                var now = Now(modId);
                info.Result = Verify(key, modId, publicKey, now);
                //a clock earlier than the issue of the key: checked at its issue time (EffectiveNow)
                if (info.Result.Issued.HasValue && info.Result.Issued.Value > now)
                    info.Result = Verify(key, modId, publicKey, EffectiveNow(now, DateTime.MinValue, info.Result.Issued));
                NoteTime(modId, EffectiveNow(DateTime.UtcNow, DateTime.MinValue, info.Result.Issued));
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"License: {modId} not checked: {ex.Message}");
                info.Result = new Result { ModId = modId, Status = Status.Malformed };
            }
            _mods[modId] = info;
            DebugHelper.WriteLine($"License: {info.Result}");

            //check by the server of the mod (share detection) at each start, in the background; renewal: an ended key at once (the mod
            //may load), a key ending soon in the background (next start)
            if (!string.IsNullOrEmpty(info.RenewUrl) && (info.Result.Status == Status.Valid || info.Result.Status == Status.Expired))
            {
                var renewal = NeedsRenewal(info.Result, Now(modId));
                if (renewal == Renewal.Now)
                {
                    try { RenewAsync(info, key, true).Wait(RENEW_TIMEOUT_MS); }
                    catch (Exception ex) { DebugHelper.WriteLine($"License: {modId} renewal failed: {ex.GetBaseException().Message}"); }
                }
                else
                    System.Threading.Tasks.Task.Run(() => RenewAsync(info, key, renewal == Renewal.Background));
            }

            if (!info.Result.IsUnlocked)
                DebugHelper.WriteLine($"License: {modId} - the mod is not loaded");
            return info.Result.IsUnlocked;
        }

        /// <summary>Renewals done (tests, logs): mod id -> what the last one did.</summary>
        public static Dictionary<string, string> LastRenewal { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Share detection answered by the server of the mod for a mod (networks the key was used from over its window): above its
        /// warning threshold the player is told (ShowShareNotices), above its block threshold the key is not renewed.
        /// </summary>
        public class ShareInfo
        {
            public int Ips { get; set; }
            public bool Warn { get; set; }
            public bool Blocked { get; set; }
            /// <summary>Window (days) and block threshold (networks) of the server, for the texts (30 / 10 when not sent).</summary>
            public int Days { get; set; } = 30;
            public int BlockAt { get; set; } = 10;
        }

        //mod id -> the last answer of the server of the mod; the mods whose notice is to show (main thread: ShowShareNotices)
        private static readonly Dictionary<string, ShareInfo> _share = new Dictionary<string, ShareInfo>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> _noticeShown = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>The share detection of a mod (null: no answer yet).</summary>
        public static ShareInfo GetShare(string modId) { lock (_share) return modId != null && _share.TryGetValue(modId, out var s) ? s : null; }

        /// <summary>Reads the share detection of an answer of the server of the mod ({ips, warn, blocked}); null without it.</summary>
        public static ShareInfo ParseShare(JObject answer) =>
            answer?["ips"] == null ? null : new ShareInfo
            {
                Ips = (int?)answer["ips"] ?? 0, Warn = (bool?)answer["warn"] ?? false, Blocked = (bool?)answer["blocked"] ?? false,
                Days = (int?)answer["days"] ?? 30, BlockAt = (int?)answer["blockAt"] ?? 10,
            };

        /// <summary>
        /// The check of the key by the server of the mod (LICENSE_RENEW_URL, POST {"key": the current key, "renew": renew}): its share
        /// detection is kept (GetShare); with <paramref name="renew"/>, a newer key of the same supporter is kept (KEY_FILE,
        /// Info.Result) when it is valid for the mod and ends later. Never throws; true when a key was kept.
        /// </summary>
        public static async System.Threading.Tasks.Task<bool> RenewAsync(Info info, string currentKey, bool renew = true)
        {
            string what;
            var rs = false;
            try
            {
                System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
                using (var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromMilliseconds(RENEW_TIMEOUT_MS * 2) })
                {
                    var body = new JObject { ["key"] = CleanKey(currentKey), ["renew"] = renew }.ToString(Newtonsoft.Json.Formatting.None);
                    var res = await client.PostAsync(info.RenewUrl, new System.Net.Http.StringContent(body, Encoding.UTF8, "application/json")).ConfigureAwait(false);
                    var text = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    //the clock of the server (Cloudflare), not the one of the machine
                    if (res.Headers.Date.HasValue) NoteTime(info.ModId, res.Headers.Date.Value.UtcDateTime);
                    if (!res.IsSuccessStatusCode)
                        what = $"{(int)res.StatusCode} {text}";
                    else
                    {
                        var answer = JObject.Parse(text);
                        var share = ParseShare(answer);
                        if (share != null) lock (_share) _share[info.ModId] = share;
                        var newKey = (string)answer["key"];
                        var renewed = string.IsNullOrEmpty(newKey) ? null : Verify(newKey, info.ModId, info.PublicKey, Now(info.ModId));
                        if ((bool?)answer["revoked"] == true)
                        {
                            Revoke(info);
                            what = "revoked by the author: key removed";
                        }
                        else if (IsBetterKey(info.Result, renewed))
                        {
                            WriteKey(info.ModId, newKey);
                            info.Result = renewed;
                            rs = true;
                            what = $"renewed: {renewed}";
                        }
                        else
                            what = !renew ? "checked" : renewed == null ? $"no newer key ({(string)answer["reason"] ?? "no new payment"})" : $"answer not kept: {renewed}";
                        if (share != null) what += $", networks {share.Ips}{(share.Blocked ? " (blocked)" : share.Warn ? " (warning)" : string.Empty)}";
                    }
                }
            }
            catch (Exception ex) { what = $"failed: {ex.GetBaseException().Message}"; }
            lock (LastRenewal) LastRenewal[info.ModId] = what;
            try { DebugHelper.WriteLine($"License: {info.ModId} check: {what}"); } catch { /*a log from a worker thread*/ }
            return rs;
        }

        /// <summary>
        /// The share notices to show (main thread; ModTitleEvent, on the title screen): once per mod and game start, a small
        /// message when the server of the mod found the key used from too many networks: a warning (VPN, mobile network), or the key
        /// no longer renewed. Nothing is blocked: the mod runs on.
        /// </summary>
        public static void ShowShareNotices()
        {
            List<string> revoked;
            lock (_share) revoked = _revoked.Where(x => !_noticeShown.Contains(x)).ToList();
            foreach (var modId in revoked)
            {
                _noticeShown.Add(modId);
                var rname = Get(modId)?.Name ?? modId;
                UICustom2.WhenFree(() => g.ui.OpenUISafe<UITextInfo>(UIType.TextInfo).InitData(string.Format(GameTool.LS("libtxt999990045"), rname), string.Format(GameTool.LS("libtxt999990064"), rname)));
                DebugHelper.WriteLine($"License: {modId} revoked notice");
            }
            List<KeyValuePair<string, ShareInfo>> todo;
            lock (_share) todo = _share.Where(x => x.Value.Warn && !_noticeShown.Contains(x.Key)).ToList();
            foreach (var x in todo)
            {
                _noticeShown.Add(x.Key);
                var name = Get(x.Key)?.Name ?? x.Key;
                var text = string.Format(GameTool.LS(x.Value.Blocked ? "libtxt999990063" : "libtxt999990062"), name, x.Value.Ips, x.Value.Days, x.Value.BlockAt);
                UICustom2.WhenFree(() => g.ui.OpenUISafe<UITextInfo>(UIType.TextInfo).InitData(string.Format(GameTool.LS("libtxt999990045"), name), text));
                DebugHelper.WriteLine($"License: {x.Key} share notice: {x.Value.Ips} networks{(x.Value.Blocked ? ", not renewed" : string.Empty)}");
            }
        }

        /// <summary>The localText key of the message of a status.</summary>
        public static string StatusTextKey(Status s)
        {
            switch (s)
            {
                case Status.Valid: return "libtxt999990049";
                case Status.Missing: return "libtxt999990050";
                case Status.Expired: return "libtxt999990051";
                case Status.WrongMod: return "libtxt999990052";
                case Status.BadPublicKey: return "libtxt999990053";
                case Status.Revoked: return "libtxt999990064";
                default: return "libtxt999990054";
            }
        }

        /// <summary>The text of a check: its status, and the supporter / expiry of a signed key.</summary>
        public static string StatusText(Result r) =>
            string.Format(GameTool.LS(StatusTextKey(r.Status)), r.Name, r.Expires.HasValue ? r.Expires.Value.ToLocalTime().ToString("yyyy-MM-dd") : GameTool.LS("libtxt999990055"));

        /// <summary>The key window of a licensed mod (null: not open). Tests: its input, SubmitKey.</summary>
        public static UICustom1 KeyWindow { get; private set; }
        private static UIItemInput _keyInput;

        /// <summary>
        /// The key window of a mod: why it is locked, what support gets a key (LICENSE_INFO), an input for the key (paste
        /// with Ctrl+V or the Paste button), buttons to the page where a key is got (LICENSE_URL) and to the support page
        /// (LICENSE_SUPPORT_URL), the QR code of the support page on the right (LICENSE_QR); Save checks the key, keeps it
        /// when valid and asks for a restart.
        /// </summary>
        public static void OpenKeyWindow(Info info)
        {
            if (info == null) return;
            UICustom1.WhenFree(() =>
            {
                KeyWindow = new UICustom1(string.Format(GameTool.LS("libtxt999990045"), info.Name), GameTool.LS("libtxt999990046"),
                    () => { KeyWindow = null; SubmitKey(info, _keyInput?.Get() as string); }, true, () => KeyWindow = null);
                try { BuildKeyWindow(KeyWindow, info); }
                catch (Exception ex) { DebugHelper.WriteLine(ex); }
            });
        }

        /// <summary>Side of the QR code of the key window (UI units).</summary>
        public const float QR_SIZE = 230f;

        private static void BuildKeyWindow(UICustom1 ui, Info info)
        {
            float left = ui.MinWidth + 30f, right = ui.MaxWidth - 30f, top = ui.MinHeight - 20f;
            var brown = new UnityEngine.Color(0.35f, 0.24f, 0.13f);

            //right: the QR code of the support page, its address under it
            UnityEngine.Sprite qr = null;
            if (!string.IsNullOrEmpty(info.QrImage))
            {
                try { qr = SpriteHelper.GetImage(info.ModId, info.QrImage); }
                catch (Exception ex) { DebugHelper.WriteLine($"License: QR {info.QrImage} not read: {ex.Message}"); }
                if (qr == null) DebugHelper.WriteLine($"License: QR {SpriteHelper.GetImgFilePath(info.ModId, info.QrImage)} not found");
            }
            if (qr != null)
            {
                var qx = right - QR_SIZE / 2f;
                ui.AddImage(qx, top - 10f - QR_SIZE / 2f, qr).Size(QR_SIZE, QR_SIZE);
                if (!string.IsNullOrEmpty(info.SupportUrl))
                {
                    var t = ui.AddText(qx, top - QR_SIZE - 30f, info.SupportUrl);
                    t.Align(UnityEngine.TextAnchor.MiddleCenter).Format(brown, 15);
                    t.Item.rectTransform.sizeDelta = new UnityEngine.Vector2(QR_SIZE + 20f, 26f);
                }
                right -= QR_SIZE + 30f;
            }
            var width = right - left;
            var cx = (left + right) / 2f;
            UIItemText Block(float top0, float h, string text, int size, UnityEngine.Color color)
            {
                var t = ui.AddText(cx, top0 - h / 2f, text);
                t.Align(UnityEngine.TextAnchor.UpperLeft, UnityEngine.VerticalWrapMode.Overflow, UnityEngine.HorizontalWrapMode.Wrap).Format(color, size);
                t.Item.rectTransform.sizeDelta = new UnityEngine.Vector2(width, h);
                return t;
            }

            //left: why, the state of the key, what support gets a key, the input, the buttons
            var y = top;
            Block(y, 50f, string.Format(GameTool.LS("libtxt999990047"), info.Name), 18, UnityEngine.Color.black);
            y -= 56f;
            Block(y, 26f, StatusText(info.Result), 17, info.Result.IsUnlocked ? new UnityEngine.Color(0.1f, 0.45f, 0.1f) : new UnityEngine.Color(0.6f, 0.1f, 0.05f));
            y -= 34f;
            if (!string.IsNullOrEmpty(info.InfoKey) && LocalTextHelper.HasText(info.InfoKey))
            {
                Block(y, 96f, GameTool.LS(info.InfoKey), 16, brown);
                y -= 104f;
            }

            _keyInput = ui.AddInput(cx, y - 60f, string.Empty).Size(width, 120f);
            _keyInput.Item.characterLimit = 0;
            _keyInput.Item.lineType = UnityEngine.UI.InputField.LineType.MultiLineNewline;
            _keyInput.Item.textComponent.alignment = UnityEngine.TextAnchor.UpperLeft;
            _keyInput.Item.textComponent.horizontalOverflow = UnityEngine.HorizontalWrapMode.Wrap;
            _keyInput.Item.SetPlaceHolder(GameTool.LS("libtxt999990048"));
            y -= 150f;

            var bx = left + 95f;
            ui.AddButton(bx, y, () =>
            {
                try { _keyInput?.Set(UnityEngine.GUIUtility.systemCopyBuffer); }
                catch (Exception ex) { DebugHelper.WriteLine($"License: clipboard not read: {ex.Message}"); }
            }, GameTool.LS("libtxt999990056")).Size(180f, 36f).Align(UnityEngine.TextAnchor.MiddleCenter);
            if (!string.IsNullOrEmpty(info.Url))
            {
                bx += 200f;
                ui.AddButton(bx, y, () => System.Diagnostics.Process.Start(info.Url), GameTool.LS("libtxt999990057"))
                    .Size(180f, 36f).Align(UnityEngine.TextAnchor.MiddleCenter);
            }
            if (!string.IsNullOrEmpty(info.SupportUrl))
            {
                bx += 200f;
                ui.AddButton(bx, y, () => System.Diagnostics.Process.Start(info.SupportUrl), GameTool.LS("libtxt999990061"))
                    .Size(180f, 36f).Align(UnityEngine.TextAnchor.MiddleCenter);
            }
            Block(y - 30f, 26f, string.Format(GameTool.LS("libtxt999990058"), KeyFilePath(info.ModId)), 13, brown);
        }

        /// <summary>
        /// A key entered for a mod: kept (KEY_FILE) when valid, with a message asking for a restart; otherwise the reason,
        /// nothing kept. Returns the check.
        /// </summary>
        public static Result SubmitKey(Info info, string key)
        {
            var r = Verify(key, info.ModId, info.PublicKey, Now(info.ModId));
            DebugHelper.WriteLine($"License: key entered: {r}");
            if (r.Status == Status.Valid)
                WriteKey(info.ModId, key);
            var text = StatusText(r) + (r.Status == Status.Valid ? "\n\n" + GameTool.LS("libtxt999990059") : string.Empty);
            UICustom2.WhenFree(() => g.ui.OpenUISafe<UITextInfo>(UIType.TextInfo).InitData(string.Format(GameTool.LS("libtxt999990045"), info.Name), text));
            return r;
        }

        //the key windows opened by themselves (once per game start)
        private static bool _autoOpened;

        /// <summary>
        /// Title screen: a button per locked mod in the row of the mods (TitleModHelper), it opens the key window; the
        /// first time the title screen shows in a game start, the key window of each locked mod opens by itself.
        /// </summary>
        public static void AddTitleButtons()
        {
            var locked = LockedMods();
            foreach (var info in locked)
            {
                var i = info;
                TitleModHelper.AddModButton(string.Format(GameTool.LS("libtxt999990060"), i.Name), () => OpenKeyWindow(i));
            }
            if (_autoOpened || locked.Count == 0) return;
            _autoOpened = true;
            foreach (var info in locked)
                OpenKeyWindow(info);
        }

        #endregion
    }
}
