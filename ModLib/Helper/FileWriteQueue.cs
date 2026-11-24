using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ModLib.Helper
{
    /// <summary>
    /// Writes text files on a worker thread, skipping a file whose content did not change since the last write.
    /// <para>
    /// The caller serializes on its own thread (game objects are not safe to read from another thread) and passes
    /// the text: only the disk write runs on the worker. A file is skipped when the text equals the one last written
    /// to it and the file on disk is still the one written (same size and time: a file changed or deleted by someone
    /// else is written again).
    /// </para>
    /// <para>
    /// Safety: each file is written to "&lt;file&gt;.tmp" and then replaces the file, so a failed or interrupted write
    /// never leaves a half-written file. A failed write is reported (OnError) and forgotten, so the next save writes it
    /// again. Only the newest text of a file waits in the queue. The worker is a foreground thread: the process does
    /// not exit before the queue is written. Flush waits for the queue (before reading the files again).
    /// </para>
    /// </summary>
    public sealed class FileWriteQueue
    {
        private sealed class Written
        {
            public string Content;
            public long Length = -1;
            public DateTime TimeUtc;
            public bool OnDisk;
        }

        private static readonly Encoding UTF8_NO_BOM = new UTF8Encoding(false);

        private readonly object sync = new object();
        private readonly Dictionary<string, Written> written = new Dictionary<string, Written>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> pending = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly ManualResetEvent idle = new ManualResetEvent(true);
        private Thread worker;

        /// <summary>Called on the worker thread when a file cannot be written (path, error).</summary>
        public Action<string, Exception> OnError { get; set; }

        /// <summary>Files queued since the creation (content changed).</summary>
        public int QueuedCount { get; private set; }

        /// <summary>Files skipped since the creation (content unchanged).</summary>
        public int SkippedCount { get; private set; }

        /// <summary>Files that could not be written since the creation.</summary>
        public int FailedCount { get; private set; }

        /// <summary>True while files wait or are being written.</summary>
        public bool IsBusy => !idle.WaitOne(0);

        /// <summary>
        /// Queues content for path, or skips it when unchanged (false; force: always queued). Returns at once; the file
        /// is written by the worker.
        /// </summary>
        public bool Write(string path, string content, bool force = false)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            content = content ?? string.Empty;
            lock (sync)
            {
                if (!force && written.TryGetValue(path, out var w) && w.Content == content && IsStillOnDisk(path, w))
                {
                    SkippedCount++;
                    return false;
                }
                written[path] = new Written { Content = content };
                pending[path] = content;
                QueuedCount++;
                idle.Reset();
                if (worker == null)
                {
                    worker = new Thread(Work) { IsBackground = false, Name = "ModLib file writer" };
                    worker.Start();
                }
                return true;
            }
        }

        /// <summary>Waits until every queued file is written (false on timeout).</summary>
        public bool Flush(int timeoutMs = 30000)
        {
            return idle.WaitOne(timeoutMs);
        }

        /// <summary>Forgets what was written (the next Write of every file writes it).</summary>
        public void Forget()
        {
            lock (sync)
            {
                written.Clear();
            }
        }

        /// <summary>
        /// Writes content to path through "&lt;path&gt;.tmp" (UTF-8 without BOM, as File.WriteAllText): the file is
        /// replaced only once the new content is fully on disk.
        /// </summary>
        public static void WriteFile(string path, string content)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            var tmp = path + ".tmp";
            File.WriteAllText(tmp, content, UTF8_NO_BOM);
            if (!File.Exists(path))
            {
                File.Move(tmp, path);
                return;
            }
            try
            {
                File.Replace(tmp, path, null);
            }
            catch (IOException)
            {
                // File.Replace can fail where Copy works (another program holding the file briefly, some drives)
                File.Copy(tmp, path, true);
                File.Delete(tmp);
            }
        }

        private static bool IsStillOnDisk(string path, Written w)
        {
            if (!w.OnDisk)
                return true; // queued, not written yet: the queued text is the same
            try
            {
                var fi = new FileInfo(path);
                return fi.Exists && fi.Length == w.Length && fi.LastWriteTimeUtc == w.TimeUtc;
            }
            catch
            {
                return false;
            }
        }

        private void Work()
        {
            while (true)
            {
                string path, content;
                lock (sync)
                {
                    if (pending.Count == 0)
                    {
                        worker = null;
                        idle.Set();
                        return;
                    }
                    var item = pending.First();
                    path = item.Key;
                    content = item.Value;
                    pending.Remove(path);
                }
                try
                {
                    WriteFile(path, content);
                    var fi = new FileInfo(path);
                    lock (sync)
                    {
                        if (written.TryGetValue(path, out var w) && w.Content == content)
                        {
                            w.Length = fi.Length;
                            w.TimeUtc = fi.LastWriteTimeUtc;
                            w.OnDisk = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lock (sync)
                    {
                        FailedCount++;
                        // forgotten: the next save writes it again
                        if (written.TryGetValue(path, out var w) && w.Content == content)
                            written.Remove(path);
                    }
                    try { File.Delete(path + ".tmp"); } catch { }
                    try { OnError?.Invoke(path, ex); } catch { }
                }
            }
        }
    }
}
