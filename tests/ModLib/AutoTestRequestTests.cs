using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModLib.AutoTest;

namespace UnitTests.ModLib
{
    /// <summary>AutoTestRequest owner check: a request.json left by a killed run-autotest.ps1 must not start the runner.</summary>
    [TestClass]
    public class AutoTestRequestTests
    {
        private static readonly DateTime Start = new DateTime(2026, 11, 21, 9, 0, 0, DateTimeKind.Local);

        [TestMethod, Description("IsOwnerAlive: no owner pid (old script) or no such process = stale")]
        public void Owner_MissingOrGone_Stale()
        {
            Check.Eq(false, AutoTestRequest.IsOwnerAlive(0, Start, _ => Start), "pid 0");
            Check.Eq(false, AutoTestRequest.IsOwnerAlive(1234, Start, _ => null), "process gone");
            Check.Eq(false, AutoTestRequest.IsOwnerAlive(1234, Start, _ => throw new InvalidOperationException()), "lookup error");
        }

        [TestMethod, Description("IsOwnerAlive: running process with the same start time (2 s tolerance) = alive, another start = reused pid")]
        public void Owner_StartTime()
        {
            Check.Eq(true, AutoTestRequest.IsOwnerAlive(1234, Start, _ => Start.AddSeconds(1)), "same process");
            Check.Eq(true, AutoTestRequest.IsOwnerAlive(1234, Start.ToUniversalTime(), _ => Start), "UTC vs local");
            Check.Eq(false, AutoTestRequest.IsOwnerAlive(1234, Start, _ => Start.AddMinutes(5)), "reused pid");
            Check.Eq(true, AutoTestRequest.IsOwnerAlive(1234, null, _ => Start), "no start time in the request");
            Check.Eq(true, AutoTestRequest.IsOwnerAlive(1234, Start, _ => DateTime.MinValue), "start time not readable");
        }

        [TestMethod, Description("IsOwnerAlive with the real process list: this test process is alive, a request JSON of the script parses")]
        public void Owner_RealProcess()
        {
            using (var p = Process.GetCurrentProcess())
            {
                var json = $"{{\"RunId\":\"x\",\"OwnerPid\":{p.Id},\"OwnerStart\":\"{p.StartTime:o}\"}}";
                var request = Newtonsoft.Json.JsonConvert.DeserializeObject<AutoTestRequest>(json);
                Check.Eq(true, request.IsOwnerAlive(), "this process");
                request.OwnerStart = p.StartTime.AddHours(-1);
                Check.Eq(false, request.IsOwnerAlive(), "other start time");
            }
            Check.Eq(false, new AutoTestRequest { RunId = "old" }.IsOwnerAlive(), "request without owner");
        }
    }
}
