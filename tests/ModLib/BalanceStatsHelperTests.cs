using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace UnitTests.ModLib
{
    /// <summary>BalanceStatsHelper: series of the play of a mod, kept in a file of its global cache folder.</summary>
    [TestClass]
    public class BalanceStatsHelperTests
    {
        private string dir;

        [TestInitialize]
        public void Init()
        {
            dir = Path.Combine(Path.GetTempPath(), "balance-stats-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            BalanceStatsHelper.ClearMemory();
            BalanceStatsHelper.FolderOf = _ => dir;
        }

        [TestCleanup]
        public void Cleanup()
        {
            CacheHelper.FlushWrites();
            BalanceStatsHelper.ClearMemory();
            try { Directory.Delete(dir, true); } catch { }
        }

        [TestMethod, Description("Add: count, sum, min, max, mean, median, p90 of a series")]
        public void Add_Aggregates()
        {
            for (var i = 1; i <= 100; i++) BalanceStatsHelper.Add("m", "hit", "r1", i);
            var s = BalanceStatsHelper.Get("m", "hit", "r1");
            Check.Eq(100L, s.Count, "count");
            Check.Eq(5050d, s.Sum, "sum");
            Check.Eq(1d, s.Min, "min");
            Check.Eq(100d, s.Max, "max");
            Check.Eq(50.5d, s.Mean, "mean");
            Check.Eq(51d, s.P50, "median");
            Check.Eq(91d, s.P90, "p90");
            BalanceStatsHelper.Add("m", "hit", "r1", double.NaN);
            Check.Eq(100L, s.Count, "NaN skipped");
        }

        [TestMethod, Description("Sample: at most SAMPLE_SIZE values, the count goes on")]
        public void Add_SampleBounded()
        {
            for (var i = 0; i < 5000; i++) BalanceStatsHelper.Add("m", "g", "k", i % 10);
            var s = BalanceStatsHelper.Get("m", "g", "k");
            Check.Eq(5000L, s.Count, "count");
            Check.Eq(BalanceStatsHelper.SAMPLE_SIZE, s.Sample.Count, "sample size");
        }

        [TestMethod, Description("Off: nothing kept")]
        public void Disabled_NothingKept()
        {
            BalanceStatsHelper.SetEnabled("m", false);
            BalanceStatsHelper.Add("m", "g", "k", 1);
            Check.Eq(true, BalanceStatsHelper.Get("m", "g", "k") == null, "no series");
            BalanceStatsHelper.SetEnabled("m", true);
            BalanceStatsHelper.Count("m", "g", "k");
            Check.Eq(1L, BalanceStatsHelper.Get("m", "g", "k").Count, "on again");
        }

        [TestMethod, Description("FlushAll writes the file; a new load continues it; another version is kept aside")]
        public void Flush_Reload_NewVersion()
        {
            BalanceStatsHelper.SetModVersion("m", "1.0");
            BalanceStatsHelper.Add("m", "g", "k", 3);
            BalanceStatsHelper.FlushAll();
            CacheHelper.FlushWrites();
            var path = BalanceStatsHelper.FilePath("m");
            var json = JObject.Parse(File.ReadAllText(path));
            Check.Eq("1.0", (string)json["modVersion"], "version written");
            Check.Eq(1L, (long)json["groups"]["g"]["k"]["n"], "series written");

            BalanceStatsHelper.ClearMemory();
            BalanceStatsHelper.SetModVersion("m", "1.0");
            BalanceStatsHelper.Add("m", "g", "k", 5);
            Check.Eq(2L, BalanceStatsHelper.Get("m", "g", "k").Count, "same version: continued");

            BalanceStatsHelper.FlushAll();
            CacheHelper.FlushWrites();
            BalanceStatsHelper.ClearMemory();
            BalanceStatsHelper.SetModVersion("m", "1.1");
            BalanceStatsHelper.Add("m", "g", "k", 7);
            Check.Eq(1L, BalanceStatsHelper.Get("m", "g", "k").Count, "new version: new series");
            Check.Eq(true, File.Exists(Path.Combine(dir, "balance-stats-1.0.json")), "old version kept");
        }
    }
}
