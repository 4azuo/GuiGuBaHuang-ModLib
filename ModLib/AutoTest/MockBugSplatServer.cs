using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ModLib.AutoTest
{
    /// <summary>
    /// Local stand-in of the BugSplat upload API for TT-06 (127.0.0.1, free port, a background thread): answers the 3
    /// requests of CrashReportHelper like BugSplat and S3 do and records every request (the evidence of what a player's
    /// game would send). Used through CrashReportHelper.ServerUrlOverride.
    ///   GET  /api/getCrashUploadUrl       -> {"url": "&lt;base&gt;/upload/crash.zip?X-Amz-Signature=autotest"}
    ///   PUT  /upload/crash.zip            -> 200, ETag "&lt;ETAG&gt;"
    ///   POST /api/commitS3CrashUpload     -> {"status": "success", "crashId": 4242}
    /// </summary>
    public class MockBugSplatServer : IDisposable
    {
        /// <summary>ETag returned by the upload (the client must send it back as md5).</summary>
        public const string ETAG = "autotest0123456789abcdef";
        /// <summary>Path of the presigned upload URL.</summary>
        public const string UPLOAD_PATH = "/upload/crash.zip";

        /// <summary>One recorded request.</summary>
        public class Request
        {
            /// <summary>Time received.</summary>
            public string Time { get; set; }
            /// <summary>GET, PUT, POST.</summary>
            public string Method { get; set; }
            /// <summary>Path without query.</summary>
            public string Path { get; set; }
            /// <summary>Query string without '?'.</summary>
            public string Query { get; set; }
            /// <summary>Headers (lower-case names).</summary>
            public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
            /// <summary>Body bytes.</summary>
            [JsonIgnore]
            public byte[] Body { get; set; } = new byte[0];
            /// <summary>Body length.</summary>
            public int BodyLength => Body.Length;
            /// <summary>Status answered.</summary>
            public int Status { get; set; }

            /// <summary>Query parameters (unescaped).</summary>
            public Dictionary<string, string> QueryParams()
            {
                var rs = new Dictionary<string, string>();
                foreach (var kv in (Query ?? string.Empty).Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var i = kv.IndexOf('=');
                    var k = Uri.UnescapeDataString(i < 0 ? kv : kv.Substring(0, i));
                    rs[k] = i < 0 ? string.Empty : Uri.UnescapeDataString(kv.Substring(i + 1).Replace('+', ' '));
                }
                return rs;
            }

            /// <summary>Fields of a multipart/form-data body.</summary>
            public Dictionary<string, string> FormFields()
            {
                var rs = new Dictionary<string, string>();
                Headers.TryGetValue("content-type", out var ct);
                var bi = ct?.IndexOf("boundary=", StringComparison.OrdinalIgnoreCase) ?? -1;
                if (bi < 0) return rs;
                var boundary = "--" + ct.Substring(bi + 9).Trim().Trim('"');
                var text = Encoding.UTF8.GetString(Body);
                foreach (var part in text.Split(new[] { boundary }, StringSplitOptions.None))
                {
                    var sep = part.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                    if (sep < 0) continue;
                    var head = part.Substring(0, sep);
                    var m = System.Text.RegularExpressions.Regex.Match(head, "name=\"?([^\";\r\n]+)\"?");
                    if (!m.Success) continue;
                    var value = part.Substring(sep + 4);
                    if (value.EndsWith("\r\n")) value = value.Substring(0, value.Length - 2);
                    rs[m.Groups[1].Value] = value;
                }
                return rs;
            }

            /// <summary>Text of the Callstack.txt inside a zip body (null when the body is not such a zip).</summary>
            public string ZipCallstack() => ZipEntryText("Callstack.txt");

            /// <summary>Text of an entry of a zip body (null when missing / not a zip).</summary>
            public string ZipEntryText(string name)
            {
                try
                {
                    using (var zip = new ZipArchive(new MemoryStream(Body), ZipArchiveMode.Read))
                    {
                        var e = zip.Entries.FirstOrDefault(x => x.FullName == name);
                        if (e == null) return null;
                        using (var r = new StreamReader(e.Open(), Encoding.UTF8))
                            return r.ReadToEnd();
                    }
                }
                catch { return null; }
            }

            /// <summary>Names of the zip entries (empty when not a zip).</summary>
            public List<string> ZipEntries()
            {
                try
                {
                    using (var zip = new ZipArchive(new MemoryStream(Body), ZipArchiveMode.Read))
                        return zip.Entries.Select(x => x.FullName).ToList();
                }
                catch { return new List<string>(); }
            }
        }

        private readonly TcpListener _listener;
        private readonly Thread _thread;
        private volatile bool _stopped;
        private readonly List<Request> _requests = new List<Request>();

        /// <summary>http://127.0.0.1:&lt;port&gt;</summary>
        public string BaseUrl { get; }
        /// <summary>The presigned URL handed to the client.</summary>
        public string UploadUrl => $"{BaseUrl}{UPLOAD_PATH}?X-Amz-Signature=autotest";

        /// <summary>Copy of the requests received so far.</summary>
        public List<Request> Requests { get { lock (_requests) return _requests.ToList(); } }

        /// <summary>Starts listening on a free port of 127.0.0.1.</summary>
        public MockBugSplatServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            BaseUrl = $"http://127.0.0.1:{((IPEndPoint)_listener.LocalEndpoint).Port}";
            _thread = new Thread(Loop) { IsBackground = true, Name = "MockBugSplatServer" };
            _thread.Start();
        }

        private void Loop()
        {
            while (!_stopped)
            {
                TcpClient client;
                try { client = _listener.AcceptTcpClient(); }
                catch { break; }
                // one thread per connection, several requests per connection (keep-alive), like a real server
                new Thread(() =>
                {
                    try { while (!_stopped && Handle(client)) { } }
                    catch { /* broken connection / timeout: the client reports its own error */ }
                    finally { client.Close(); }
                }) { IsBackground = true, Name = "MockBugSplatServer-conn" }.Start();
            }
        }

        private static string ReadLine(Stream s)
        {
            var sb = new StringBuilder();
            int b;
            while ((b = s.ReadByte()) >= 0)
            {
                if (b == '\n') break;
                if (b != '\r') sb.Append((char)b);
            }
            return b < 0 && sb.Length == 0 ? null : sb.ToString();
        }

        private static byte[] ReadExactly(Stream s, int n)
        {
            var buf = new byte[n];
            var read = 0;
            while (read < n)
            {
                var r = s.Read(buf, read, n - read);
                if (r <= 0) break;
                read += r;
            }
            return buf;
        }

        /// <summary>Serves one request of the connection; false when the client closed it (or asked to).</summary>
        private bool Handle(TcpClient client)
        {
            client.ReceiveTimeout = 15000;
            var s = client.GetStream();
            var first = ReadLine(s);
            if (string.IsNullOrEmpty(first)) return false;
            var parts = first.Split(' ');
            var target = parts.Length > 1 ? parts[1] : "/";
            var q = target.IndexOf('?');
            var req = new Request
            {
                Time = DateTime.Now.ToString("HH:mm:ss.fff"),
                Method = parts[0],
                Path = q < 0 ? target : target.Substring(0, q),
                Query = q < 0 ? string.Empty : target.Substring(q + 1),
            };
            string line;
            while (!string.IsNullOrEmpty(line = ReadLine(s)))
            {
                var i = line.IndexOf(':');
                if (i > 0) req.Headers[line.Substring(0, i).Trim().ToLower()] = line.Substring(i + 1).Trim();
            }
            if (req.Headers.TryGetValue("expect", out var expect) && expect.Equals("100-continue", StringComparison.OrdinalIgnoreCase))
                Write(s, "HTTP/1.1 100 Continue\r\n\r\n");
            if (req.Headers.TryGetValue("content-length", out var cl) && int.TryParse(cl, out var len) && len > 0)
                req.Body = ReadExactly(s, len);
            else if (req.Headers.TryGetValue("transfer-encoding", out var te) && te.Contains("chunked"))
                req.Body = ReadChunked(s);

            string body = string.Empty;
            var extra = string.Empty;
            if (req.Method == "GET" && req.Path == "/api/getCrashUploadUrl")
            {
                req.Status = 200;
                body = JsonConvert.SerializeObject(new { url = UploadUrl });
            }
            else if (req.Method == "PUT" && req.Path == UPLOAD_PATH)
            {
                req.Status = 200;
                extra = $"ETag: \"{ETAG}\"\r\n";
            }
            else if (req.Method == "POST" && req.Path == "/api/commitS3CrashUpload")
            {
                req.Status = 200;
                body = JsonConvert.SerializeObject(new { status = "success", crashId = 4242 });
            }
            else
            {
                req.Status = 404;
                body = JsonConvert.SerializeObject(new { message = "not found" });
            }
            lock (_requests) _requests.Add(req);
            // network latency of a real server: an answer at once (same machine) arrives before the game's Mono
            // HttpClient has completed its own write of the request body, and it then fails in
            // WebConnectionStream.EndWrite ("Cannot access a disposed object: NetworkStream", TT-06 2026-11-19)
            Thread.Sleep(100);
            // the connection is kept until the client closes it: the next ReadLine returns null when it does
            var bytes = Encoding.UTF8.GetBytes(body);
            Write(s, $"HTTP/1.1 {req.Status} {(req.Status == 200 ? "OK" : "Not Found")}\r\nContent-Type: application/json\r\nContent-Length: {bytes.Length}\r\n{extra}\r\n");
            s.Write(bytes, 0, bytes.Length);
            s.Flush();
            return true;
        }

        private static byte[] ReadChunked(Stream s)
        {
            var ms = new MemoryStream();
            while (true)
            {
                var sizeLine = ReadLine(s);
                if (sizeLine == null) break;
                var size = Convert.ToInt32(sizeLine.Split(';')[0].Trim(), 16);
                if (size == 0) { ReadLine(s); break; }
                var chunk = ReadExactly(s, size);
                ms.Write(chunk, 0, chunk.Length);
                ReadLine(s);
            }
            return ms.ToArray();
        }

        private static void Write(Stream s, string text)
        {
            var b = Encoding.ASCII.GetBytes(text);
            s.Write(b, 0, b.Length);
            s.Flush();
        }

        /// <summary>
        /// Writes the evidence into a folder: requests.json (method, path, query, headers, sizes, form fields),
        /// &lt;n&gt;-upload.zip and &lt;n&gt;-Callstack.txt (what is uploaded), &lt;n&gt;-commit-form.txt (raw form).
        /// </summary>
        public void SaveEvidence(string dir)
        {
            Directory.CreateDirectory(dir);
            var list = Requests;
            var summary = new List<object>();
            for (var i = 0; i < list.Count; i++)
            {
                var r = list[i];
                var n = (i + 1).ToString("00");
                var zipEntries = r.ZipEntries();
                if (zipEntries.Count > 0)
                {
                    File.WriteAllBytes(Path.Combine(dir, $"{n}-upload.zip"), r.Body);
                    foreach (var entry in zipEntries)
                        File.WriteAllText(Path.Combine(dir, $"{n}-{entry}"), r.ZipEntryText(entry) ?? string.Empty);
                }
                else if (r.Body.Length > 0)
                    File.WriteAllText(Path.Combine(dir, $"{n}-{(r.Method == "POST" ? "commit-form" : "body")}.txt"), Encoding.UTF8.GetString(r.Body));
                summary.Add(new
                {
                    r.Time, r.Method, r.Path, query = r.QueryParams(), r.Headers, r.BodyLength, r.Status,
                    zipEntries, form = r.Method == "POST" ? r.FormFields() : null,
                });
            }
            File.WriteAllText(Path.Combine(dir, "requests.json"), JsonConvert.SerializeObject(summary, Formatting.Indented));
        }

        /// <summary>Stops listening.</summary>
        public void Dispose()
        {
            _stopped = true;
            try { _listener.Stop(); } catch { }
        }
    }
}
