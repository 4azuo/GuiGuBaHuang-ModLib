using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace UnitTests.ModLib
{
    /// <summary>CrashReportHelper: the BugSplat payload (zip, URLs, commit fields) and the error identity.</summary>
    [TestClass]
    public class CrashReportHelperTests
    {
        private static CrashReportHelper.CrashReport Sample() => new CrashReportHelper.CrashReport
        {
            Database = "fouru_modlib",
            AppVersion = "2.2.32",
            User = "abc",
            Description = "NullReferenceException: x",
            Callstack = "System.NullReferenceException: x\n   at A.B()",
        };

        [TestMethod, Description("The zip holds one Callstack.txt with the exception text (UTF-8)")]
        public void BuildZip_HasCallstack()
        {
            var bytes = CrashReportHelper.BuildZip("Exception: é\n   at X()");
            using (var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read))
            {
                Check.Eq(1, zip.Entries.Count, "one entry");
                Check.Eq(CrashReportHelper.CALLSTACK_FILE, zip.Entries[0].FullName, "entry name");
                using (var r = new StreamReader(zip.Entries[0].Open(), Encoding.UTF8))
                    Check.Eq("Exception: é\n   at X()", r.ReadToEnd(), "content");
            }
        }

        [TestMethod, Description("The zip holds log.txt next to Callstack.txt when there is a log")]
        public void BuildZip_WithLog()
        {
            using (var zip = new ZipArchive(new MemoryStream(CrashReportHelper.BuildZip("E", "line 1\nline 2")), ZipArchiveMode.Read))
                Check.Seq(new[] { "Callstack.txt", "log.txt" }, zip.Entries.Select(x => x.FullName).ToArray(), "entries");
            using (var zip = new ZipArchive(new MemoryStream(CrashReportHelper.BuildZip("E", "")), ZipArchiveMode.Read))
                Check.Eq(1, zip.Entries.Count, "no log.txt for an empty log");
        }

        [TestMethod, Description("The report goes to the mod of the first MOD_<id> assembly of the stack that has a database")]
        public void FindModOfAssemblies()
        {
            var mods = new[] { "nE7UL2", "flQrxq" };
            Check.Eq("nE7UL2", CrashReportHelper.FindModOfAssemblies(new[] { "ModLib", "mscorlib", "MOD_nE7UL2", "MOD_flQrxq" }, mods), "first mod frame (ModLib frames skipped)");
            Check.Eq("nE7UL2", CrashReportHelper.FindModOfAssemblies(new[] { "MOD_ne7ul2" }, mods), "any case");
            Check.Null(CrashReportHelper.FindModOfAssemblies(new[] { "MOD_other", "ModLib" }, mods), "mod without database: none");
            Check.Eq("flQrxq", CrashReportHelper.FindModOfAssemblies(new[] { "MOD_other", "MOD_flQrxq" }, mods), "next mod with a database");
        }

        [TestMethod, Description("Private values of the log replaced (both separators, any case, longest first); short values kept")]
        public void SanitizeLog()
        {
            var priv = new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>(@"C:\Users\Jane Doe\AppData\LocalLow\guigugame\guigubahuang", "<game data>"),
                new System.Collections.Generic.KeyValuePair<string, string>(@"C:\Users\Jane Doe", "<user folder>"),
                new System.Collections.Generic.KeyValuePair<string, string>("Jane Doe", "<user>"),
                new System.Collections.Generic.KeyValuePair<string, string>("ab", "<x>"),
            };
            var log = @"save c:\users\jane doe\AppData\LocalLow\guigugame\guigubahuang\mod\x.json" + "\n" +
                      "desktop C:/Users/Jane Doe/Desktop, account JANE DOE, json C:\\\\Users\\\\Jane Doe\\\\x, ab stays";
            var s = CrashReportHelper.SanitizeLog(log, priv);
            Check.Info(s);
            Check.Contains(s, @"save <game data>\mod\x.json", "game data path (other case)");
            Check.Contains(s, "desktop <user folder>/Desktop", "user folder with '/'");
            Check.Contains(s, "account <user>", "account name");
            Check.Contains(s, "json <user folder>", "JSON-escaped path");
            Check.Contains(s, "ab stays", "values shorter than 3 kept");
            Check.False(s.IndexOf("jane", StringComparison.OrdinalIgnoreCase) >= 0, "no trace of the account");
        }

        [TestMethod, Description("TailLines keeps the last lines, at most maxChars (the end)")]
        public void TailLines()
        {
            Check.Eq("c\nd", CrashReportHelper.TailLines("a\r\nb\r\nc\r\nd\r\n", 2), "last 2 lines");
            Check.Eq("a\nb", CrashReportHelper.TailLines("a\nb", 10), "fewer lines than asked");
            Check.Eq("", CrashReportHelper.TailLines("a\nb", 0), "0 lines");
            Check.Eq("789", CrashReportHelper.TailLines("123456789", 5, 3), "cut to maxChars, end kept");
        }

        [TestMethod, Description("Only letters, digits, '-' and '_' are a database: it becomes a host name")]
        public void IsValidDatabase()
        {
            Check.True(CrashReportHelper.IsValidDatabase("fouru_modlib-1"), "valid");
            Check.True(!CrashReportHelper.IsValidDatabase(""), "empty");
            Check.True(!CrashReportHelper.IsValidDatabase(null), "null");
            Check.True(!CrashReportHelper.IsValidDatabase("evil.com/x"), "host injection");
        }

        [TestMethod, Description("Step 1 URL: database host, escaped query, zip size")]
        public void UploadUrlRequest()
        {
            var r = Sample();
            r.AppVersion = "2.2 beta";
            var url = CrashReportHelper.GetUploadUrlRequest(r, 123);
            Check.Eq("https://fouru_modlib.bugsplat.com/api/getCrashUploadUrl?database=fouru_modlib&appName=GuiGuBaHuang-ModLib&appVersion=2.2%20beta&crashPostSize=123", url, "url");
        }

        [TestMethod, Description("ServerUrlOverride (autotest stand-in server) replaces https://<db>.bugsplat.com")]
        public void ServerUrlOverride()
        {
            try
            {
                CrashReportHelper.ServerUrlOverride = "http://127.0.0.1:5000/";
                Check.True(CrashReportHelper.GetUploadUrlRequest(Sample(), 1).StartsWith("http://127.0.0.1:5000/api/getCrashUploadUrl?"), "override used");
            }
            finally { CrashReportHelper.ServerUrlOverride = null; }
            Check.Eq("https://fouru_modlib.bugsplat.com", CrashReportHelper.GetServerUrl("fouru_modlib"), "BugSplat without override");
        }

        [TestMethod, Description("Step 3 fields: .NET crash type 18, s3Key = upload url, attributes as JSON")]
        public void CommitFields()
        {
            var r = Sample();
            r.Attributes["modId"] = "nE7UL2";
            var f = CrashReportHelper.GetCommitFields(r, "https://s3/x?sig=1", "md5v").ToDictionary(x => x.Key, x => x.Value);
            Check.Eq("18", f["crashTypeId"], "crash type");
            Check.Eq("https://s3/x?sig=1", f["s3Key"], "s3Key");
            Check.Eq("md5v", f["md5"], "md5");
            Check.Eq("{\"modId\":\"nE7UL2\"}", f["attributes"], "attributes");
            Check.Eq("abc", f["user"], "user");
        }

        [TestMethod, Description("The upload url is read from the answer; BugSplat's message is in the error when missing")]
        public void ParseUploadUrl()
        {
            Check.Eq("https://s3/u", CrashReportHelper.ParseUploadUrl("{\"url\":\"https://s3/u\"}"), "url");
            try
            {
                CrashReportHelper.ParseUploadUrl("{\"message\":\"bad database\"}");
                Assert.Fail("no exception");
            }
            catch (InvalidOperationException ex)
            {
                Check.True(ex.Message.Contains("bad database"), "message kept");
            }
        }

        [TestMethod, Description("Live upload to the BugSplat database of the env var BUGSPLAT_TEST_DB (skipped when not set)")]
        public void Live_SendToBugSplat()
        {
            var db = Environment.GetEnvironmentVariable("BUGSPLAT_TEST_DB");
            if (string.IsNullOrEmpty(db))
                Assert.Inconclusive("BUGSPLAT_TEST_DB not set");
            var r = Sample();
            r.Database = db;
            r.Description = "Unit test report (CrashReportHelperTests.Live_SendToBugSplat)";
            r.Callstack = new InvalidOperationException("unit test").ToString();
            r.Attributes["modId"] = "unittest";
            Check.Eq("sent", CrashReportHelper.SendAsync(r).GetAwaiter().GetResult(), "result");
        }

        [TestMethod, Description("Same innermost error = same key (sent once); another message = another key")]
        public void ErrorKey()
        {
            Exception Make(string m)
            {
                try { throw new InvalidOperationException(m); }
                catch (Exception e) { return new Exception("outer", e); }
            }
            Check.Eq(DebugHelper.GetErrorKey(Make("a")), DebugHelper.GetErrorKey(Make("a")), "same");
            Check.True(DebugHelper.GetErrorKey(Make("a")) != DebugHelper.GetErrorKey(Make("b")), "different");
        }
    }
}
