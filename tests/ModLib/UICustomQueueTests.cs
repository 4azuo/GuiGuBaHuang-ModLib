using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.Object;
using System;
using System.Collections.Generic;

namespace UnitTests.ModLib
{
    /// <summary>UICustomQueue: a window opens now when its UI of the game is free, else after the windows before it.</summary>
    [TestClass]
    public class UICustomQueueTests
    {
        [TestMethod, Description("Free: opened now; busy: queued, opened one at a time in order once the UI closes; each type on its own")]
        public void Queue_OpensWhenFree()
        {
            var open = new HashSet<string>();
            var opened = new List<string>();
            var keep = UICustomQueue.IsOpen;
            var keepLog = UICustomQueue.Log;
            var logged = new List<string>();
            UICustomQueue.Log = logged.Add;
            try
            {
                UICustomQueue.Clear();
                UICustomQueue.IsOpen = name => open.Contains(name);
                Action Win(string ui, string id) => () => { opened.Add(id); open.Add(ui); };

                Check.True(UICustomQueue.Open("Long", Win("Long", "a")), "nothing open: opened now");
                Check.True(!UICustomQueue.Open("Long", Win("Long", "b")), "its UI open: queued");
                Check.True(!UICustomQueue.Open("Long", Win("Long", "c")), "queued after b");
                Check.True(UICustomQueue.Open("Info", Win("Info", "x")), "another type is free: opened now");
                UICustomQueue.Update();
                Check.Seq(new[] { "a", "x" }, opened, "nothing more while 'a' is open");

                open.Remove("Long");
                UICustomQueue.Update();
                Check.Seq(new[] { "a", "x", "b" }, opened, "'a' closed: 'b' opens (one at a time)");
                UICustomQueue.Update();
                Check.Eq(1, UICustomQueue.CountOf("Long"), "'c' still waits for 'b'");

                open.Remove("Long");
                Check.True(!UICustomQueue.Open("Long", Win("Long", "d")), "a window waiting: a new one goes after it, even with the UI free");
                UICustomQueue.Update();
                Check.Seq(new[] { "a", "x", "b", "c" }, opened, "'c' before 'd'");

                UICustomQueue.Open("Long", () => throw new InvalidOperationException("broken window"));
                open.Remove("Long");
                UICustomQueue.Update();
                Check.Seq(new[] { "a", "x", "b", "c", "d" }, opened, "'d' next");
                open.Remove("Long");
                UICustomQueue.Update();
                Check.Eq(0, UICustomQueue.Count, "a window that throws is dropped, the next ones still open");
                Check.True(logged.Count == 1 && logged[0].Contains("broken window"), "its error logged");
            }
            finally
            {
                UICustomQueue.IsOpen = keep;
                UICustomQueue.Log = keepLog;
                UICustomQueue.Clear();
            }
        }
    }
}
