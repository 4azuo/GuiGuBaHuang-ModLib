using System;
using System.IO;
using System.Threading;

namespace ModLib.Helper
{
    /// <summary>
    /// Hang finder: records which ModLib event handler runs on the main thread, and a background thread writes it
    /// to a file every second. When the game freezes the main thread stops, but the background thread keeps
    /// writing "still running Xs: &lt;handler&gt;", which names the handler that blocks. Off by default
    /// (the Taoist autotest turns it on for its runs).
    /// </summary>
    public static class EventTraceHelper
    {
        private static volatile string _current;
        private static long _currentSince;
        private static Thread _thread;
        private static string _file;

        /// <summary>True when tracing.</summary>
        public static bool Enabled => _file != null;

        /// <summary>Starts the background writer (once).</summary>
        public static void Start(string file)
        {
            if (_thread != null || string.IsNullOrEmpty(file))
                return;
            _file = file;
            try { File.AppendAllText(_file, $"{DateTime.Now:HH:mm:ss.fff} trace started{Environment.NewLine}"); } catch { }
            _thread = new Thread(Loop) { IsBackground = true, Name = "ModLib.EventTrace" };
            _thread.Start();
        }

        /// <summary>Marks the start of a handler on the main thread; returns the previous value to restore (nested calls).</summary>
        public static string Enter(string name)
        {
            if (_file == null)
                return null;
            var prev = _current;
            _current = prev == null ? name : prev + " > " + name;
            Interlocked.Exchange(ref _currentSince, DateTime.Now.Ticks);
            return prev;
        }

        /// <summary>Marks the end of a handler (restores the value returned by Enter).</summary>
        public static void Exit(string prev)
        {
            if (_file == null)
                return;
            _current = prev;
            Interlocked.Exchange(ref _currentSince, DateTime.Now.Ticks);
        }

        private static void Loop()
        {
            string lastWritten = null;
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    var cur = _current;
                    if (cur == null)
                        continue;
                    var secs = (DateTime.Now.Ticks - Interlocked.Read(ref _currentSince)) / TimeSpan.TicksPerSecond;
                    // write every new handler that is still running after 1 s, then every second while it blocks
                    if (secs >= 1 && (cur != lastWritten || secs >= 2))
                    {
                        File.AppendAllText(_file, $"{DateTime.Now:HH:mm:ss.fff} still running {secs}s: {cur}{Environment.NewLine}");
                        lastWritten = cur;
                    }
                }
                catch { }
            }
        }
    }
}
