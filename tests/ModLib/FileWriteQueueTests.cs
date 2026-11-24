using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;
using System;
using System.IO;

namespace UnitTests.ModLib
{
    /// <summary>FileWriteQueue (cache save): unchanged files skipped, writes in the background, failures written again.</summary>
    [TestClass]
    public class FileWriteQueueTests
    {
        private string dir;

        [TestInitialize]
        public void Init()
        {
            dir = Path.Combine(Path.GetTempPath(), "fwq-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { Directory.Delete(dir, true); } catch { }
        }

        [TestMethod, Description("A file is written once; the same JSON again is skipped, a new JSON is written")]
        public void SkipsUnchanged()
        {
            var q = new FileWriteQueue();
            var f = Path.Combine(dir, "a.json");
            Check.True(q.Write(f, "{\"v\":1}"), "first write queued");
            Check.True(q.Flush(5000), "flushed");
            Check.Eq("{\"v\":1}", File.ReadAllText(f), "content");
            Check.True(!q.Write(f, "{\"v\":1}"), "same content skipped");
            Check.True(q.Write(f, "{\"v\":2}"), "new content queued");
            Check.True(q.Flush(5000), "flushed");
            Check.Eq("{\"v\":2}", File.ReadAllText(f), "new content written");
            Check.True(!File.Exists(f + ".tmp"), "no temp file left");
            Check.True(q.Write(f, "{\"v\":2}", force: true), "force: written even unchanged");
            Check.True(q.Flush(5000), "flushed");
        }

        [TestMethod, Description("A file changed or deleted by someone else is written again even when the JSON is the same")]
        public void WritesAgainWhenChangedOnDisk()
        {
            var q = new FileWriteQueue();
            var f = Path.Combine(dir, "b.json");
            q.Write(f, "{\"v\":1}");
            q.Flush(5000);
            File.WriteAllText(f, "{\"other\":true}");
            File.SetLastWriteTimeUtc(f, DateTime.UtcNow.AddMinutes(1));
            Check.True(q.Write(f, "{\"v\":1}"), "changed on disk: written again");
            q.Flush(5000);
            Check.Eq("{\"v\":1}", File.ReadAllText(f), "restored");
            File.Delete(f);
            Check.True(q.Write(f, "{\"v\":1}"), "deleted: written again");
            q.Flush(5000);
            Check.True(File.Exists(f), "file back");
        }

        [TestMethod, Description("A failed write is reported and forgotten: the next save writes it again")]
        public void FailureIsWrittenAgain()
        {
            var q = new FileWriteQueue();
            var errors = 0;
            q.OnError = (p, ex) => errors++;
            var f = Path.Combine(dir, "c.json");
            File.WriteAllText(f, "old");
            using (new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                q.Write(f, "new");
                Check.True(q.Flush(5000), "flushed");
            }
            Check.Eq(1, errors, "error reported");
            Check.Eq(1, q.FailedCount, "failed count");
            Check.Eq("old", File.ReadAllText(f), "old file kept whole");
            Check.True(q.Write(f, "new"), "same content queued again after the failure");
            q.Flush(5000);
            Check.Eq("new", File.ReadAllText(f), "written at the next save");
        }

        [TestMethod, Description("Only the newest content of a file is kept; many files are all written")]
        public void ManyWrites()
        {
            var q = new FileWriteQueue();
            for (var i = 0; i < 50; i++)
                for (var v = 0; v < 5; v++)
                    q.Write(Path.Combine(dir, $"f{i}.json"), $"{i}:{v}");
            Check.True(q.Flush(10000), "flushed");
            for (var i = 0; i < 50; i++)
                Check.Eq($"{i}:4", File.ReadAllText(Path.Combine(dir, $"f{i}.json")), $"file {i} newest");
        }
    }
}
