using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTests.ModLib
{
    /// <summary>SaveCleanupHelper: mod data of deleted saves (temporary folders, no game).</summary>
    [TestClass]
    public class SaveCleanupHelperTests
    {
        private static string NewRoot()
        {
            var root = Path.Combine(Path.GetTempPath(), "ggbh-cleanup-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return root;
        }

        private static void Touch(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, "x");
        }

        private static void ModData(string root, string id)
        {
            foreach (var mod in new[] { "flQrxq", "nE7UL2" })
                Touch(Path.Combine(root, "mod", mod, "saves", id, "a.json"));
            Touch(Path.Combine(root, "mod", "flQrxq", id + "_configs.json"));
            Touch(Path.Combine(root, "mod", "flQrxq", id + "_debug-20261116.log"));
        }

        [TestMethod, Description("Recorded saves 1_aaa (still in the game) and 2_bbb (deleted): only the data of BBBBBB is removed; unrecorded CCCCCC, globals and title logs stay")]
        public void CleanDeletedSaves_RemovesOnlyRecordedDeletedSaves()
        {
            var root = NewRoot();
            try
            {
                ModData(root, "AAAAAA"); ModData(root, "BBBBBB"); ModData(root, "CCCCCC");
                Touch(Path.Combine(root, "mod", "nE7UL2", "globals", "g.json"));
                Touch(Path.Combine(root, "mod", "flQrxq", "debug-20261116.log"));
                var index = SaveCleanupHelper.IndexPath(root);
                SaveCleanupHelper.Register(index, "1_aaa", "AAAAAA");
                SaveCleanupHelper.Register(index, "2_bbb", "BBBBBB");

                var removed = SaveCleanupHelper.CleanDeletedSaves(root, new HashSet<string> { "1_aaa", "3_other" });
                Check.Eq(4, removed.Count, "removed: 2 save folders + configs + log of BBBBBB");
                Check.True(removed.All(x => x.Contains("BBBBBB")), "only BBBBBB data", removed);
                Check.True(Directory.Exists(Path.Combine(root, "mod", "nE7UL2", "saves", "AAAAAA")), "kept: save still in the game");
                Check.True(Directory.Exists(Path.Combine(root, "mod", "nE7UL2", "saves", "CCCCCC")), "kept: save never recorded");
                Check.True(File.Exists(Path.Combine(root, "mod", "flQrxq", "debug-20261116.log")), "kept title log");
                Check.True(File.Exists(Path.Combine(root, "mod", "nE7UL2", "globals", "g.json")), "kept globals");
                Check.False(SaveCleanupHelper.ReadIndex(index).ContainsKey("2_bbb"), "deleted save forgotten in the index");
            }
            finally { Directory.Delete(root, true); }
        }

        [TestMethod, Description("A player id recorded for 2 folders is kept while one of them exists; no save list: nothing is removed")]
        public void CleanDeletedSaves_SafeCases()
        {
            var root = NewRoot();
            try
            {
                ModData(root, "AAAAAA");
                var index = SaveCleanupHelper.IndexPath(root);
                SaveCleanupHelper.Register(index, "1_old", "AAAAAA");
                SaveCleanupHelper.Register(index, "4_new", "AAAAAA");
                Check.Eq(0, SaveCleanupHelper.CleanDeletedSaves(root, new HashSet<string> { "4_new" }).Count, "id still used by another folder");
                Check.Eq(0, SaveCleanupHelper.CleanDeletedSaves(root, new HashSet<string>()).Count, "empty save list: nothing removed");
                Check.True(Directory.Exists(Path.Combine(root, "mod", "nE7UL2", "saves", "AAAAAA")), "data kept");
            }
            finally { Directory.Delete(root, true); }
        }
    }
}
