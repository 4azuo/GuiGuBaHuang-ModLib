using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;
using System;
using System.IO;

namespace UnitTests.ModLib
{
    /// <summary>PrivateModHelper: ModConf/private_mod.json of a project (no Steam Workshop upload).</summary>
    [TestClass]
    public class PrivateModHelperTests
    {
        [TestMethod, Description("ParseSettings: private unless empty or \"Private\": false; the reason trimmed")]
        public void ParseSettings_PrivateFlagAndReason()
        {
            Check.Eq(true, PrivateModHelper.ParseSettings("{}") != null, "no field: private");
            Check.Eq("team mod", PrivateModHelper.ParseSettings("{ \"Private\": true, \"Reason\": \"  team mod \" }")?.Reason, "reason trimmed");
            Check.Eq(null, PrivateModHelper.ParseSettings("{ \"Reason\": \" \" }").Reason, "blank reason: none");
            Check.Eq(null, PrivateModHelper.ParseSettings("{ \"Private\": false }"), "Private false: public");
            Check.Eq(null, PrivateModHelper.ParseSettings(""), "empty file: public");
            Check.Eq(null, PrivateModHelper.ParseSettings(null), "null: public");
        }

        [TestMethod, Description("CandidateFiles: the project's ModConf, its ModProject/ModConf, then the installed mod's ModConf")]
        public void CandidateFiles_Order()
        {
            var rs = PrivateModHelper.CandidateFiles(@"C:\p\ModProject", @"C:\m\ModConf");
            Check.Eq(3, rs.Count, "3 places");
            Check.Eq(@"C:\p\ModProject\ModConf\private_mod.json", rs[0], "project ModConf");
            Check.Eq(@"C:\p\ModProject\ModProject\ModConf\private_mod.json", rs[1], "parent folder of a project");
            Check.Eq(@"C:\m\ModConf\private_mod.json", rs[2], "installed mod");
            Check.Eq(1, PrivateModHelper.CandidateFiles(null, @"C:\m\ModConf").Count, "no project path");
            Check.Eq(0, PrivateModHelper.CandidateFiles(null, null).Count, "nothing");
        }

        [TestMethod, Description("ReadFirst: the first existing file decides (a public one stops the search); none: public")]
        public void ReadFirst_FirstFileDecides()
        {
            var dir = Path.Combine(Path.GetTempPath(), "privmod-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var a = Path.Combine(dir, "a.json");
                var b = Path.Combine(dir, "b.json");
                var missing = Path.Combine(dir, "missing.json");
                File.WriteAllText(b, "{ \"Reason\": \"b\" }");
                Check.Eq("b", PrivateModHelper.ReadFirst(new[] { missing, b })?.Reason, "missing skipped");
                File.WriteAllText(a, "{ \"Private\": false }");
                Check.Eq(null, PrivateModHelper.ReadFirst(new[] { a, b }), "first file public");
                Check.Eq(null, PrivateModHelper.ReadFirst(new[] { missing }), "no file");
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
