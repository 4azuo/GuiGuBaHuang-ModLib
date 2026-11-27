using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLib.AutoTest
{
    /// <summary>
    /// Request written by GuiGuBaHuang-ModLib/scripts/game/run-autotest.ps1 to &lt;mod cache&gt;\autotest\request.json.
    /// The autotest runs only while this file exists, so players never trigger it.
    /// </summary>
    public class AutoTestRequest
    {
        /// <summary>Id of the run (folder name of the evidence).</summary>
        public string RunId { get; set; }
        /// <summary>Absolute folder where results.json and screenshots are written.</summary>
        public string OutDir { get; set; }
        /// <summary>Save to load from the title screen (name or folder, empty = first save). Ignored when AutoLoad is false.</summary>
        public string Save { get; set; }
        /// <summary>
        /// True: the pre-test TP-01 creates a new character (trainer on, CrueltyShura) in a free save slot and every
        /// scenario runs on it (the save is removed by the restore after the run). False: Save / AutoLoad are used.
        /// </summary>
        public bool NewCharacter { get; set; } = false;
        /// <summary>
        /// Games side by side (run-autotest.ps1 -SlotCharacter): every game slot plays its own test character
        /// "AutoTest Slot&lt;n&gt;" (kept between runs); created by TP-01 when missing - one game at a time
        /// (&lt;mod cache&gt;\autotest-shared\creating.lock), so two games never pick the same free save slot.
        /// </summary>
        public bool SlotCharacter { get; set; } = false;
        /// <summary>
        /// Window of the game ("1600x900", empty: as set): the game's own settings (g.data.globle.gameSetting: windowed,
        /// size) set and applied at the title screen, kept after the run.
        /// </summary>
        public string Window { get; set; }
        /// <summary>True: the runner opens the save itself. False: the tester loads it manually.</summary>
        public bool AutoLoad { get; set; } = true;
        /// <summary>Scenario ids to run (empty = all automatic scenarios).</summary>
        public List<string> Scenarios { get; set; } = new List<string>();
        /// <summary>Delay between steps.</summary>
        public int StepDelayMs { get; set; } = 1500;
        /// <summary>Keep watching battles after the automatic scenarios (battle "watch" scenarios).</summary>
        public bool WatchBattles { get; set; } = false;
        /// <summary>Capture the screen before the first step and after every step of each scenario.</summary>
        public bool SnapSteps { get; set; } = true;
        /// <summary>
        /// Promotion run (GuiGuBaHuang-ModLib/scripts/game/run-promo.ps1): the scenarios run only for their pictures ("promo" of
        /// scenarios.json, copied to ModProject/SteamImg): no check panels over the screenshots.
        /// </summary>
        public bool Promo { get; set; } = false;
        /// <summary>
        /// Games side by side that stay open (run-autotest-parallel.ps1): after its scenarios the game waits in the save
        /// for the next request (a request.json with another RunId, run-autotest.ps1 -Attach) instead of being closed;
        /// every game scenario starts from the player state recorded when the save was loaded (AutoTestRunner baseline)
        /// and its own Scenario.Precondition.
        /// </summary>
        public bool KeepAlive { get; set; } = false;
        /// <summary>
        /// Process id of the run-autotest.ps1 that wrote the request (0: none, a request of an older script). The request
        /// counts only while that process runs: a script killed or closed with its terminal never removes request.json
        /// (its finally does not run), and a game started normally afterwards must not run the tests.
        /// </summary>
        public int OwnerPid { get; set; }
        /// <summary>Start time of the OwnerPid process (a reused process id is another process); null: not checked.</summary>
        public DateTime? OwnerStart { get; set; }

        /// <summary>Largest difference between OwnerStart and the start time of the running process.</summary>
        public const double OWNER_START_TOLERANCE_SEC = 2;

        /// <summary>True while the run-autotest.ps1 that wrote the request still runs.</summary>
        public bool IsOwnerAlive() => IsOwnerAlive(OwnerPid, OwnerStart, GetProcessStart);

        /// <summary>
        /// Owner check without the process list: <paramref name="processStart"/> returns the start time of a running
        /// process, null when no process has that id, DateTime.MinValue when it runs but its start time cannot be read.
        /// </summary>
        public static bool IsOwnerAlive(int pid, DateTime? start, Func<int, DateTime?> processStart)
        {
            if (pid <= 0) return false;
            DateTime? actual;
            try { actual = processStart(pid); }
            catch { return false; }
            if (actual == null) return false;
            if (start == null || actual.Value == DateTime.MinValue) return true;
            return Math.Abs((actual.Value.ToUniversalTime() - start.Value.ToUniversalTime()).TotalSeconds) <= OWNER_START_TOLERANCE_SEC;
        }

        private static DateTime? GetProcessStart(int pid)
        {
            System.Diagnostics.Process p;
            try { p = System.Diagnostics.Process.GetProcessById(pid); }
            catch (ArgumentException) { return null; }   // no process with this id
            using (p)
            {
                try { if (p.HasExited) return null; } catch { }
                try { return p.StartTime; }
                catch { return DateTime.MinValue; }
            }
        }
    }

    /// <summary>Execution kind of a scenario.</summary>
    public enum ScenarioKind
    {
        /// <summary>Fully automatic (runs in the title screen or right after the save is loaded).</summary>
        Auto,
        /// <summary>Runs automatically when the tester starts a battle during the session.</summary>
        Watch,
        /// <summary>A tool, not a test (e.g. the effect galleries): runs only when its id is asked (-Scenario).</summary>
        Tool,
    }

    /// <summary>Where an automatic scenario can run.</summary>
    public enum ScenarioScope
    {
        /// <summary>Title screen (before loading the save).</summary>
        Title,
        /// <summary>Inside the loaded save (map screen).</summary>
        Game,
        /// <summary>Inside a battle (Watch scenarios).</summary>
        Battle,
    }

    /// <summary>One step of a scenario: an action run after a delay.</summary>
    public class ScenarioStep
    {
        /// <summary>Short label (used in logs and screenshot names).</summary>
        public string Label { get; set; }
        /// <summary>Delay before the step runs.</summary>
        public int DelayMs { get; set; }
        /// <summary>Step body.</summary>
        public Action<AutoTestContext> Action { get; set; }
        /// <summary>Wait step: polled every PollMs until it returns true (or TimeoutMs elapses -> FAIL).</summary>
        public Func<AutoTestContext, bool> Until { get; set; }
        /// <summary>Timeout of a wait step.</summary>
        public int TimeoutMs { get; set; } = 30000;
        /// <summary>Polling period of a wait step.</summary>
        public int PollMs { get; set; } = 500;
    }

    /// <summary>An automatic test scenario (definition only, see AutoTestScenarios).</summary>
    public class Scenario
    {
        /// <summary>Scenario id (same id as in 3161035078/tests/scenarios.json).</summary>
        public string Id { get; set; }
        /// <summary>Tested feature (event class name).</summary>
        public string Feature { get; set; }
        /// <summary>What the scenario checks.</summary>
        public string Title { get; set; }
        /// <summary>Execution kind.</summary>
        public ScenarioKind Kind { get; set; } = ScenarioKind.Auto;
        /// <summary>Execution scope.</summary>
        public ScenarioScope Scope { get; set; } = ScenarioScope.Game;
        /// <summary>Watch scenarios only: "BattleStart" or "BattleEnd".</summary>
        public string Trigger { get; set; }
        /// <summary>
        /// Show the checks of a step in an in-game panel before its screenshot when the step left no UI open
        /// (logic checks: the evidence then shows the values computed in the game instead of an empty map).
        /// </summary>
        public bool Overlay { get; set; } = true;
        /// <summary>
        /// Keep the message boxes between steps. Default false: before every step the runner clicks the only button of
        /// an information box (one button, already in the previous step's screenshot) and "Disable" in the game's
        /// tutorial-tips prompt, so they do not cover the next windows. True for scenarios whose next step answers a
        /// one-button box itself.
        /// </summary>
        public bool KeepPopups { get; set; } = false;
        /// <summary>Screenshots before / after the steps (off for tools with thousands of steps: they capture what they need).</summary>
        public bool SnapSteps { get; set; } = true;
        /// <summary>Steps executed in order.</summary>
        public List<ScenarioStep> Steps { get; } = new List<ScenarioStep>();
        /// <summary>
        /// Set-up run before the first step of a game scenario, after the common reset of the player to the state of
        /// the loaded save (grade, exp, money, lifespan, place, HP / MP / SP, S&amp;M settings): what the scenario needs
        /// besides (a grade, a town, items...). The games that stay open run many scenarios in a row in one save.
        /// </summary>
        public Action<AutoTestContext> Precondition { get; set; }

        /// <summary>Sets the precondition (fluent).</summary>
        public Scenario Pre(Action<AutoTestContext> precondition)
        {
            Precondition = precondition;
            return this;
        }

        /// <summary>Appends a step (fluent).</summary>
        public Scenario Step(string label, Action<AutoTestContext> action, int delayMs = -1)
        {
            Steps.Add(new ScenarioStep { Label = label, Action = action, DelayMs = delayMs });
            return this;
        }

        /// <summary>
        /// Appends a wait step: until() is called every pollMs (it may also act, e.g. click a dialog) until it
        /// returns true; after timeoutMs the step fails. Use it for asynchronous game states (battle loading...).
        /// </summary>
        public Scenario Wait(string label, Func<AutoTestContext, bool> until, int timeoutMs = 30000, int pollMs = 500, int delayMs = -1)
        {
            Steps.Add(new ScenarioStep { Label = label, Until = until, TimeoutMs = timeoutMs, PollMs = pollMs, DelayMs = delayMs });
            return this;
        }
    }

    /// <summary>Result of one check inside a scenario.</summary>
    public class CheckResult
    {
        /// <summary>Check id "&lt;scenario id&gt;-&lt;nn&gt;" (PASS/FAIL checks only), stable while the scenario code does not change.</summary>
        public string Id { get; set; }
        /// <summary>Step label.</summary>
        public string Step { get; set; }
        /// <summary>Check description.</summary>
        public string Message { get; set; }
        /// <summary>PASS, FAIL, INFO or SKIP.</summary>
        public string Status { get; set; }
        /// <summary>Time of the check.</summary>
        public string Time { get; set; }
    }

    /// <summary>One evidence screenshot.</summary>
    public class ScreenshotInfo
    {
        /// <summary>File name relative to OutDir (screens/...).</summary>
        public string File { get; set; }
        /// <summary>Step label ("before" for the state before the first step).</summary>
        public string Step { get; set; }
        /// <summary>1-based step number (0 = before the first step).</summary>
        public int StepNo { get; set; }
        /// <summary>"before", "after" (state after the step) or "extra" (taken inside a step).</summary>
        public string Kind { get; set; }
        /// <summary>Time of the request.</summary>
        public string Time { get; set; }
    }

    /// <summary>Result of a scenario, serialized into results.json.</summary>
    public class ScenarioResult
    {
        /// <summary>Scenario id.</summary>
        public string Id { get; set; }
        /// <summary>Tested feature.</summary>
        public string Feature { get; set; }
        /// <summary>Scenario title.</summary>
        public string Title { get; set; }
        /// <summary>Execution kind.</summary>
        public string Kind { get; set; }
        /// <summary>PASS, FAIL, ERROR, SKIP or RUNNING.</summary>
        public string Status { get; set; } = "RUNNING";
        /// <summary>Start time.</summary>
        public string StartedAt { get; set; }
        /// <summary>End time.</summary>
        public string EndedAt { get; set; }
        /// <summary>All checks and notes.</summary>
        public List<CheckResult> Checks { get; } = new List<CheckResult>();
        /// <summary>Screenshots in order: before the first step, after each step, and extra ones taken by a step.</summary>
        public List<ScreenshotInfo> Screenshots { get; } = new List<ScreenshotInfo>();
        /// <summary>Exception text when a step crashed.</summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// State passed to scenario steps: assertions, notes, screenshots and a bag to share values between steps.
    /// </summary>
    public class AutoTestContext
    {
        /// <summary>Result being filled.</summary>
        public ScenarioResult Result { get; }
        /// <summary>Label of the running step.</summary>
        public string CurrentStep { get; set; }
        /// <summary>Values shared between the steps of a scenario.</summary>
        public Dictionary<string, object> Bag { get; } = new Dictionary<string, object>();
        /// <summary>Screenshot callback provided by the runner (label -> file name).</summary>
        public Func<string, string> ScreenshotFunc { get; set; }
        /// <summary>1-based number of the running step.</summary>
        public int CurrentStepNo { get; set; }

        /// <summary>Creates the context of a scenario.</summary>
        public AutoTestContext(ScenarioResult result)
        {
            Result = result;
        }

        private int _checkNo;

        private void Add(string status, string message)
        {
            var id = status == "PASS" || status == "FAIL" ? $"{Result.Id.Split('#')[0]}-{++_checkNo:00}" : null;
            Result.Checks.Add(new CheckResult { Id = id, Step = CurrentStep, Message = message, Status = status, Time = DateTime.Now.ToString("HH:mm:ss.fff") });
        }

        /// <summary>Records PASS when the condition is true, FAIL otherwise.</summary>
        public bool Assert(bool condition, string message)
        {
            Add(condition ? "PASS" : "FAIL", message);
            return condition;
        }

        /// <summary>Records PASS when both values are equal.</summary>
        public bool AssertEqual<T>(T expected, T actual, string message)
        {
            return Assert(EqualityComparer<T>.Default.Equals(expected, actual), $"{message} (expected: {expected}, actual: {actual})");
        }

        /// <summary>Records PASS when min &lt;= value &lt;= max.</summary>
        public bool AssertBetween(double value, double min, double max, string message)
        {
            return Assert(value >= min && value <= max, $"{message} ({min} <= {value} <= {max})");
        }

        /// <summary>Records an informative note (does not affect the status).</summary>
        public void Info(string message) => Add("INFO", message);

        /// <summary>Records that a check was skipped (precondition not met).</summary>
        public void Skip(string message) => Add("SKIP", message);

        /// <summary>
        /// Takes an extra screenshot inside a step (the runner already captures the screen before the first step
        /// and after every step). The screen is captured by run-autotest.ps1 a few hundred ms later.
        /// </summary>
        public void Snap(string label) => AddScreenshot(label, CurrentStepNo, "extra");

        /// <summary>
        /// Records a video clip of the game window (~30 frames a second for <paramref name="ms"/> ms) into
        /// &lt;run&gt;\clips\&lt;scenario&gt;_&lt;name&gt;\ (f###.jpg + frames.txt); collect-promo.ps1 encodes the clips
        /// marked in scenarios.json ("promo" entries with "clip": true) to mp4. Returns the frame folder (null outside
        /// a run): "started" appears in it when the capture begins, frames.txt when it is done.
        /// </summary>
        public string Clip(string name, int ms, int intervalMs = 33)
        {
            if (AutoTestRunner.OutDir == null) return null;
            var safe = new string(name.Select(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_' ? ch : '-').ToArray());
            var dir = System.IO.Path.Combine(AutoTestRunner.OutDir, "clips", $"{Result.Id.Split('#')[0]}_{safe}");
            if (System.IO.Directory.Exists(dir)) System.IO.Directory.Delete(dir, true);
            if (AutoTestRunner.Record(dir, ms, intervalMs) == null) return null;
            Info($"clip {name}: {ms} ms");
            return dir;
        }

        /// <summary>Requests a screenshot and records it (used by the runner for before/after shots).</summary>
        public void AddScreenshot(string label, int stepNo, string kind)
        {
            var file = ScreenshotFunc?.Invoke($"{stepNo:00}-{label}");
            if (file != null)
                Result.Screenshots.Add(new ScreenshotInfo { File = file, Step = label, StepNo = stepNo, Kind = kind, Time = DateTime.Now.ToString("HH:mm:ss.fff") });
        }

        /// <summary>Final status from the recorded checks.</summary>
        public string ComputeStatus()
        {
            if (Result.Error != null)
                return "ERROR";
            if (Result.Checks.Any(x => x.Status == "FAIL"))
                return "FAIL";
            if (Result.Checks.Any(x => x.Status == "PASS"))
                return "PASS";
            return "SKIP";
        }
    }
}
