using EBattleTypeData;
using MOD_nE7UL2.AutoTest;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Helper;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// In-game test runner used by scripts/game/run-autotest.ps1.
    /// Loaded only when &lt;mod cache&gt;\autotest\request.json exists (never for normal players).
    /// Phases: title screen scenarios -> load the save (automatically or by the tester) -> game scenarios
    /// -> optional battle watching. Results and screenshots are written to the request OutDir.
    /// The runner never saves the game; use a throwaway test save anyway.
    /// </summary>
    [Cache(ModConst.AUTOTEST_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    public class AutoTestEvent : ModEvent
    {
        /// <summary>Runner phases.</summary>
        public enum PhaseEnum { WaitTitle, Title, Loading, WaitGame, Game, Watch, Done }

        public static AutoTestEvent Instance { get; set; }

        public const string REQUEST_FILE = "request.json";
        public const string STOP_FILE = "stop.flag";
        public const string RESULT_FILE = "results.json";
        public const int LOAD_TIMEOUT_SEC = 300;

        private AutoTestRequest _request;
        private PhaseEnum _phase = PhaseEnum.WaitTitle;
        private DateTime _nextAt = DateTime.MinValue;
        private DateTime _phaseStart = DateTime.Now;
        private int _loadStep;
        private int _snapIndex;
        private int _battleCount;
        private readonly DateTime _startedAt = DateTime.Now;
        private readonly Queue<Scenario> _queue = new Queue<Scenario>();
        private readonly List<ScenarioResult> _results = new List<ScenarioResult>();
        private readonly List<string> _log = new List<string>();
        private Scenario _current;
        private DateTime _titleSeen = DateTime.MinValue;
        private DateTime? _waitStart;
        private int _shotStage;   // 0 = none, 1 = overlay opened, waiting before the screenshot
        private UIBase _overlay;
        /// <summary>Time given to the title screen popups to appear before the title scenarios start.</summary>
        public const int TITLE_SETTLE_MS = 8000;
        private HashSet<int> _uisBefore = new HashSet<int>();
        private string _pendingShot;
        /// <summary>Wait after a screenshot request so run-autotest.ps1 captures the screen before the next action.</summary>
        public const int SHOT_WAIT_MS = 900;
        private AutoTestContext _ctx;
        private int _stepIndex;

        /// <summary>Folder of the request/stop files: &lt;mod cache&gt;\autotest\.</summary>
        public static string GetAutoTestFolder(string modId)
        {
            return Path.Combine(CacheHelper.GetCacheFolderName(modId), "autotest");
        }

        /// <summary>Loads the runner only when a request file exists.</summary>
        public override bool OnCacheHandler()
        {
            return File.Exists(Path.Combine(GetAutoTestFolder(ModId), REQUEST_FILE));
        }

        /// <summary>Reads the request and prepares the output folder.</summary>
        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            var requestFile = Path.Combine(GetAutoTestFolder(modId), REQUEST_FILE);
            if (!File.Exists(requestFile))
                return;
            _request = JsonConvert.DeserializeObject<AutoTestRequest>(File.ReadAllText(requestFile));
            Scenarios_NewCharacter(_request.NewCharacter);
            if (string.IsNullOrEmpty(_request.OutDir))
                _request.OutDir = Path.Combine(GetAutoTestFolder(modId), _request.RunId ?? DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(Path.Combine(_request.OutDir, "screens"));
            // hang finder: names the event handler that blocks the main thread (written by a background thread)
            EventTraceHelper.Start(Path.Combine(_request.OutDir, "event-trace.log"));
            // no exception dialog during the run (it blocks the steps): errors are still logged and listed in the report
            DebugHelper.IsDebugMode = false;
            MOD_nE7UL2.Story.StoryEvent.NoAutoOpen = true;
            Log($"request loaded: run={_request.RunId}, save={_request.Save}, autoLoad={_request.AutoLoad}, scenarios={(_request.Scenarios.Count == 0 ? "all" : string.Join(",", _request.Scenarios))}");
            WriteResults();
        }

        private static void Scenarios_NewCharacter(bool on) => MOD_nE7UL2.AutoTest.Scenarios.Title.TP01_NewCharacter.Requested = on;

        private DateTime _lastDramaCheck = DateTime.MinValue;
        private DateTime? _dramaSince;

        /// <summary>Seconds a drama may stay open before the runner closes it (story tests read every drama: longer).</summary>
        public static int DramaTimeoutSec { get; set; } = 5;

        /// <summary>
        /// Every second (real time): closes a drama dialog left open for 5 s (NPC/stalker joining a battle, an opponent
        /// escaping...). A drama pauses the game timers, so OnTimeUpdate100ms (the step runner) would never run again;
        /// the 5 s let the scenarios that open a drama check it first.
        /// </summary>
        public override void OnMonoUpdate()
        {
            base.OnMonoUpdate();
            if ((DateTime.Now - _lastDramaCheck).TotalSeconds < 1)
                return;
            _lastDramaCheck = DateTime.Now;
            // no exception dialog during the run (it blocks the steps): UISettingScreenEvent.OnLoadGame sets it back
            // from its "hide exception logs" option, so it is enforced here; errors are still logged and reported
            DebugHelper.IsDebugMode = false;
            var open = new[] { UIType.DramaDialogue, UIType.DramaFortuitous, UIType.DramaDialogueBigTexture, UIType.DramaBigTexture, UIType.ActionCourseInfo }.Any(t => g.ui.HasUI(t));
            if (!open) { _dramaSince = null; return; }
            if (_dramaSince == null) { _dramaSince = DateTime.Now; return; }
            if ((DateTime.Now - _dramaSince.Value).TotalSeconds >= DramaTimeoutSec && TownTestHelper.DismissDramas() > 0)
            {
                DebugHelper.WriteLine($"[AUTOTEST] drama dialog dismissed (open for {DramaTimeoutSec} s)");
                _dramaSince = null;
            }
        }

        /// <summary>Drives the state machine (title -> load -> game -> watch -> done).</summary>
        public override void OnTimeUpdate100ms()
        {
            base.OnTimeUpdate100ms();
            if (_request == null || _phase == PhaseEnum.Done || DateTime.Now < _nextAt)
                return;
            try
            {
                Tick();
            }
            catch (Exception ex)
            {
                Log($"runner error: {ex}");
                Finish("ERROR");
            }
        }

        private void Tick()
        {
            if (File.Exists(Path.Combine(GetAutoTestFolder(ModId), STOP_FILE)) && _phase != PhaseEnum.Done && _current == null)
            {
                Finish("STOPPED");
                return;
            }
            switch (_phase)
            {
                case PhaseEnum.WaitTitle:
                    if (GameHelper.IsLoginScreen() && g.ui.HasUI(UIType.Login))
                    {
                        if (_titleSeen == DateTime.MinValue)
                            _titleSeen = DateTime.Now;
                        // popups of the title screen (Taoist Workshop description, game announcements...) appear late
                        foreach (var name in OpenUIs().Select(UIName).Where(n => !IsBaseUI(n)))
                            if (!AutoTestHelper.TitlePopups.Contains(name)) { AutoTestHelper.TitlePopups.Add(name); Log($"title popup: {name}"); }
                        if ((DateTime.Now - _titleSeen).TotalMilliseconds > TITLE_SETTLE_MS)
                        {
                            // the title screen with its popups (Workshop description, announcements) before they are closed
                            Snap("title-screen");
                            Enqueue(ScenarioScope.Title, null);
                            SetPhase(PhaseEnum.Title, 500);
                        }
                    }
                    break;
                case PhaseEnum.Title:
                    if (!RunNextStep())
                    {
                        if (GameHelper.IsInGame())
                        {
                            Log("already in a save (the pre-test created the character)");
                            SetPhase(PhaseEnum.WaitGame, 1000);
                        }
                        else if (_request.NewCharacter)
                        {
                            Log("the pre-test did not create the character");
                            Finish("ERROR");
                        }
                        else if (_request.AutoLoad)
                        {
                            _loadStep = 0;
                            SetPhase(PhaseEnum.Loading, 500);
                        }
                        else
                        {
                            Log("waiting for the tester to load the save");
                            SetPhase(PhaseEnum.WaitGame, 1000);
                        }
                    }
                    break;
                case PhaseEnum.Loading:
                    LoadSaveStep();
                    break;
                case PhaseEnum.WaitGame:
                    if (GameHelper.IsInGame() && g.ui.HasUI(UIType.MapMain) && !GameHelper.IsLoadingScreen())
                    {
                        Log($"save loaded: {g.world.playerUnit.GetName()} ({g.world.playerUnit.GetUnitId()}), {GameHelper.GetGameYear()}/{GameHelper.GetGameMonth()}");
                        Enqueue(ScenarioScope.Game, null);
                        SetPhase(PhaseEnum.Game, 5000); //let OnLoadGame/OnLoadGameAfter of all events finish
                    }
                    else if (_request.AutoLoad && !GameHelper.IsInGame() && TryConfirmDialogs())
                    {
                        _nextAt = DateTime.Now.AddMilliseconds(3000);
                    }
                    else if ((DateTime.Now - _phaseStart).TotalSeconds > (_request.AutoLoad ? LOAD_TIMEOUT_SEC : LOAD_TIMEOUT_SEC * 4))
                    {
                        Log("timeout: the save was not loaded");
                        Snap("load-timeout");
                        Finish("ERROR");
                    }
                    break;
                case PhaseEnum.Game:
                    if (!RunNextStep())
                    {
                        if (_request.WatchBattles)
                        {
                            Log("automatic scenarios done - watching battles (create stop.flag to finish)");
                            SetPhase(PhaseEnum.Watch, 1000);
                        }
                        else
                        {
                            Finish("DONE");
                        }
                    }
                    break;
                case PhaseEnum.Watch:
                    RunNextStep();
                    break;
            }
        }

        /// <summary>Queues battle-start watch scenarios.</summary>
        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            if (_phase == PhaseEnum.Watch)
            {
                _battleCount++;
                Log($"battle #{_battleCount} started");
                Enqueue(ScenarioScope.Battle, "BattleStart");
            }
        }

        /// <summary>Queues battle-end watch scenarios.</summary>
        public override void OnBattleEndHandler(BattleEndHandler e)
        {
            base.OnBattleEndHandler(e);
            if (_phase == PhaseEnum.Watch)
            {
                Log($"battle #{_battleCount} ended (win={e.isWin})");
                AutoTestHelper.LastBattleWin = e.isWin;
                Enqueue(ScenarioScope.Battle, "BattleEnd");
            }
        }

        #region Save loading
        private void LoadSaveStep()
        {
            var ui = g.ui.GetUI<UILogin>(UIType.Login);
            if (ui == null)
            {
                Log("login UI not found - waiting for the game");
                SetPhase(PhaseEnum.WaitGame, 1000);
                return;
            }
            switch (_loadStep++)
            {
                case 0:
                    Log("opening the save list");
                    ui.btnInto.onClick.Invoke();
                    _nextAt = DateTime.Now.AddMilliseconds(2500);
                    break;
                default:
                    var items = ui.allCacheItem.ToArray().Where(x => x != null && x.cacheData != null).ToList();
                    Log($"saves: {string.Join(" | ", items.Select(x => $"{x.cacheData.data.name} [{x.cacheData.data.folder}]"))}");
                    var wanted = (_request.Save ?? string.Empty).Trim();
                    var item = string.IsNullOrEmpty(wanted) ? items.FirstOrDefault(x => !x.isDie) :
                        items.FirstOrDefault(x => string.Equals(x.cacheData.data.name, wanted, StringComparison.OrdinalIgnoreCase) ||
                                                  string.Equals(x.cacheData.data.folder, wanted, StringComparison.OrdinalIgnoreCase));
                    Snap("save-list");
                    if (item == null)
                    {
                        if (_loadStep < 5)
                        {
                            _nextAt = DateTime.Now.AddMilliseconds(2000);
                            return;
                        }
                        Log($"save not found: '{wanted}'");
                        Finish("ERROR");
                        return;
                    }
                    Log($"loading save {item.cacheData.data.name} [{item.cacheData.data.folder}]");
                    item.OnItemClick();
                    SetPhase(PhaseEnum.WaitGame, 3000);
                    break;
            }
        }
        private static readonly string[] CONFIRM_LABELS = { "ok", "confirm", "continue", "yes", "确定", "確定", "继续", "繼續", "确认", "確認", "是" };
        private DateTime _lastConfirm = DateTime.MinValue;

        /// <summary>
        /// While a save is loading, clicks the confirmation button of blocking dialogs (e.g. "the archive used
        /// mods that were removed"). Returns true when a button was clicked. Clicks at most every 3 s.
        /// </summary>
        private bool TryConfirmDialogs()
        {
            if ((DateTime.Now - _phaseStart).TotalSeconds < 5 || (DateTime.Now - _lastConfirm).TotalSeconds < 3 || g.ui.uiRoot == null)
                return false;
            foreach (var btn in g.ui.uiRoot.GetComponentsInChildren<UnityEngine.UI.Button>())
            {
                if (btn == null || !btn.gameObject.activeInHierarchy || !btn.interactable)
                    continue;
                var label = btn.GetComponentInChildren<UnityEngine.UI.Text>()?.text?.Trim();
                if (string.IsNullOrEmpty(label) || !CONFIRM_LABELS.Contains(label.ToLowerInvariant()))
                    continue;
                var owner = btn.GetComponentInParent<UIBase>();
                if (owner == null || owner.uiType.uiName == UIType.Login.uiName)
                    continue;
                _lastConfirm = DateTime.Now;
                Snap($"dialog-{owner.uiType.uiName}");
                Log($"confirm dialog {owner.uiType.uiName}: click '{label}' ({btn.name})");
                btn.onClick.Invoke();
                return true;
            }
            return false;
        }
        #endregion

        #region Scenario execution
        private void Enqueue(ScenarioScope scope, string trigger)
        {
            foreach (var s in AutoTestScenarios.Create().Where(x => x.Scope == scope && x.Trigger == trigger))
            {
                // the pre-test (TP: new character) always runs: the selected scenarios need its character
                if (_request.Scenarios.Count > 0 && !_request.Scenarios.Contains(s.Id) && !s.Id.StartsWith("TP"))
                    continue;
                _queue.Enqueue(s);
            }
        }

        private bool RunNextStep()
        {
            if (_current == null)
            {
                if (_queue.Count == 0)
                    return false;
                _current = _queue.Dequeue();
                var result = new ScenarioResult
                {
                    Id = _current.Kind == ScenarioKind.Watch ? $"{_current.Id}#{_battleCount}" : _current.Id,
                    Feature = _current.Feature,
                    Title = _current.Title,
                    Kind = _current.Kind.ToString(),
                    StartedAt = DateTime.Now.ToString("s"),
                };
                _results.Add(result);
                _ctx = new AutoTestContext(result) { ScreenshotFunc = label => Snap($"{result.Id}_{label}") };
                _stepIndex = 0;
                _pendingShot = _request.SnapSteps ? "before" : null;
                CloseNonBaseUIs($"[{result.Id}] before");
                // on the title screen: the game's announcements (StartGameTip) closed too; it stays a base
                // UI while a save loads (its "mods loaded" warning is confirmed by TryConfirmDialogs)
                if (GameHelper.IsLoginScreen() && g.ui.HasUI(UIType.StartGameTip))
                {
                    Log($"[{result.Id}] before: closing {UIType.StartGameTip.uiName}");
                    g.ui.CloseUI(UIType.StartGameTip);
                }
                _uisBefore = new HashSet<int>(OpenUIs().Select(x => x.GetInstanceID()));
                Log($"[{result.Id}] start: {result.Title}");
            }
            // evidence: the screen before the first step and after every step (taken just before the next action,
            // once the previous step had StepDelayMs to render); the capture itself needs ~0.3 s (run-autotest.ps1)
            if (_pendingShot != null)
            {
                if (_shotStage == 0 && _stepIndex > 0 && _current.Overlay && ShowOverlay())
                {
                    _shotStage = 1;
                    _nextAt = DateTime.Now.AddMilliseconds(700);
                    return true;
                }
                _ctx.AddScreenshot(_pendingShot, _stepIndex, _stepIndex == 0 ? "before" : "after");
                _pendingShot = null;
                _nextAt = DateTime.Now.AddMilliseconds(SHOT_WAIT_MS);
                if (_shotStage == 1)
                {
                    _shotStage = 2;   // close the overlay on the next tick, after the capture
                }
                return true;
            }
            if (_shotStage == 2)
            {
                _shotStage = 0;
                try { if (_overlay != null) g.ui.CloseUI(_overlay, true); } catch { }
                _overlay = null;
                _nextAt = DateTime.Now.AddMilliseconds(300);
                return true;
            }
            if (_stepIndex >= _current.Steps.Count)
            {
                _ctx.Result.Status = _ctx.ComputeStatus();
                _ctx.Result.EndedAt = DateTime.Now.ToString("s");
                CloseLeftoverUIs();
                Log($"[{_ctx.Result.Id}] {_ctx.Result.Status}");
                _current = null;
                WriteResults();
                return true;
            }
            var step = _current.Steps[_stepIndex];
            _ctx.CurrentStep = step.Label;
            _ctx.CurrentStepNo = _stepIndex + 1;
            try
            {
                if (step.Until != null)
                {
                    if (_waitStart == null)
                        _waitStart = DateTime.Now;
                    var waited = (DateTime.Now - _waitStart.Value).TotalMilliseconds;
                    if (!step.Until(_ctx))
                    {
                        if (waited < step.TimeoutMs)
                        {
                            _nextAt = DateTime.Now.AddMilliseconds(step.PollMs);
                            return true;
                        }
                        _ctx.Assert(false, $"{step.Label}: not reached within {step.TimeoutMs / 1000}s");
                    }
                    else
                    {
                        _ctx.Info($"{step.Label}: reached after {waited / 1000:0.0}s");
                    }
                    _waitStart = null;
                }
                else
                {
                    step.Action(_ctx);
                }
                _stepIndex++;
            }
            catch (Exception ex)
            {
                _ctx.Result.Error = $"{step.Label}: {ex}";
                Log($"[{_ctx.Result.Id}] error in '{step.Label}': {ex.Message}");
                _waitStart = null;
                _stepIndex = _current.Steps.Count;
            }
            if (_request.SnapSteps)
                _pendingShot = step.Label;
            var next = _stepIndex < _current.Steps.Count ? _current.Steps[_stepIndex].DelayMs : 0;
            _nextAt = DateTime.Now.AddMilliseconds(next >= 0 ? next : _request.StepDelayMs);
            WriteResults();
            return true;
        }
        #endregion

        /// <summary>
        /// Game UIs currently open. Compare them by GetInstanceID(): Il2Cpp wrappers are new objects at every read.
        /// </summary>
        private static List<UIBase> OpenUIs()
        {
            var rs = new List<UIBase>();
            foreach (var kv in g.ui.allUI)
                foreach (var ui in kv.Value)
                    if (ui != null) rs.Add(ui);
            return rs;
        }

        private static string UIName(UIBase ui) => ui?.uiType?.uiName ?? ui?.name ?? string.Empty;

        /// <summary>Screens that stay open (title, map, battle) and transient loading/mask UIs.</summary>
        private static bool IsBaseUI(string name) =>
            name == UIType.Mask.uiName || name.StartsWith("Loading") || name == UIType.Login.uiName || name == UIType.MapMain.uiName ||
            name == UIType.BattleInfo.uiName || name.StartsWith("Battle") || name == UIType.StartGameTip.uiName;

        /// <summary>Closes every popup (announcements, dialogs, windows) so a scenario starts from a clean screen.</summary>
        private void CloseNonBaseUIs(string tag)
        {
            try
            {
                foreach (var ui in OpenUIs().Where(x => !IsBaseUI(UIName(x))).ToList())
                {
                    Log($"{tag}: closing {UIName(ui)}");
                    g.ui.CloseUI(ui, true);
                }
            }
            catch (Exception ex)
            {
                Log($"{tag}: cannot close popups: {ex.Message}");
            }
        }

        /// <summary>
        /// When the step left only the base screen open, shows its checks in an in-game text panel so the screenshot
        /// shows the values computed in the game. Returns false when nothing is shown.
        /// </summary>
        private bool ShowOverlay()
        {
            try
            {
                var checks = _ctx.Result.Checks.Where(x => x.Step == _pendingShot).ToList();
                if (checks.Count == 0 || OpenUIs().Any(x => !IsBaseUI(UIName(x))) || GameHelper.IsInBattlle())
                    return false;
                var color = new Dictionary<string, string> { ["PASS"] = "#1f8a4c", ["FAIL"] = "#d1343b", ["INFO"] = "#3b6fd8", ["SKIP"] = "#7a828e" };
                var label = new Dictionary<string, string> { ["PASS"] = "OK", ["FAIL"] = "NG", ["INFO"] = "INFO", ["SKIP"] = "SKIP" };
                var text = string.Join("\n", checks.Select(x =>
                    $"<color={color[x.Status]}><b>[{label[x.Status]}]</b></color> {(x.Id != null ? x.Id + "  " : string.Empty)}{x.Message}"));
                var ui = g.ui.OpenUISafe<UITextInfoLong>(UIType.TextInfoLong);
                ui.InitData($"AutoTest {_ctx.Result.Id} - step {_stepIndex}: {_pendingShot}", text);
                _overlay = ui;
                return true;
            }
            catch (Exception ex)
            {
                Log($"overlay failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Closes the UIs a scenario opened and left open (failed or aborted steps), so they do not cover the
        /// screenshots or break the next scenarios. Mask/loading UIs are left alone.
        /// </summary>
        private void CloseLeftoverUIs()
        {
            try
            {
                foreach (var ui in OpenUIs().Where(x => !_uisBefore.Contains(x.GetInstanceID())).ToList())
                {
                    var name = ui.uiType?.uiName ?? ui.name;
                    if (name == UIType.Mask.uiName || name.StartsWith("Loading") || name == UIType.Login.uiName || name == UIType.MapMain.uiName)
                        continue;   // never close the base screens
                    Log($"[{_ctx.Result.Id}] closing leftover UI {name}");
                    g.ui.CloseUI(ui, true);
                }
            }
            catch (Exception ex)
            {
                Log($"[{_ctx.Result.Id}] cannot close leftover UIs: {ex.Message}");
            }
        }

        #region Output
        private void SetPhase(PhaseEnum phase, int delayMs)
        {
            _phase = phase;
            _phaseStart = DateTime.Now;
            _nextAt = DateTime.Now.AddMilliseconds(delayMs);
            Log($"phase: {phase}");
            WriteResults();
        }

        private void Finish(string status)
        {
            _phase = PhaseEnum.Done;
            Log($"finished: {status}");
            WriteResults(status);
            DebugHelper.Save();
        }

        /// <summary>
        /// Requests a screenshot of the game window into OutDir\screens and returns the relative file name.
        /// UnityEngine.ScreenCapture is stripped from the game ("Method unstripping failed"), so the runner writes
        /// a request file (&lt;autotest&gt;\snaps\&lt;n&gt;.req containing the target path) that run-autotest.ps1 captures
        /// within ~0.3 s (steps wait StepDelayMs before changing the screen).
        /// </summary>
        public string Snap(string label)
        {
            try
            {
                var safe = new string(label.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '-').ToArray());
                var file = $"screens/{++_snapIndex:000}_{safe}.png";
                var snapDir = Path.Combine(GetAutoTestFolder(ModId), "snaps");
                Directory.CreateDirectory(snapDir);
                File.WriteAllText(Path.Combine(snapDir, $"{_snapIndex:000}.req"), Path.Combine(_request.OutDir, file.Replace('/', '\\')));
                Log($"screenshot {file}");
                return file;
            }
            catch (Exception ex)
            {
                Log($"screenshot failed: {ex.Message}");
                return null;
            }
        }

        private void Log(string msg)
        {
            var line = $"{DateTime.Now:HH:mm:ss.fff} {msg}";
            _log.Add(line);
            DebugHelper.WriteLine($"[AUTOTEST] {msg}");
        }

        private void WriteResults(string status = null)
        {
            if (_request == null)
                return;
            var data = new
            {
                runId = _request.RunId,
                status = status ?? "RUNNING",
                phase = _phase.ToString(),
                startedAt = _startedAt.ToString("s"),
                updatedAt = DateTime.Now.ToString("s"),
                taoistVersion = ModConst.TAOIST_VERSION,
                modLibVersion = ModMaster.ModObj?.Version,
                player = GameHelper.IsInGame() ? $"{g.world.playerUnit.GetName()} ({g.world.playerUnit.GetUnitId()})" : null,
                summary = new
                {
                    total = _results.Count,
                    pass = _results.Count(x => x.Status == "PASS"),
                    fail = _results.Count(x => x.Status == "FAIL"),
                    error = _results.Count(x => x.Status == "ERROR"),
                    skip = _results.Count(x => x.Status == "SKIP"),
                },
                scenarios = _results,
                log = _log,
            };
            File.WriteAllText(Path.Combine(_request.OutDir, RESULT_FILE), JsonConvert.SerializeObject(data, Formatting.Indented));
        }
        #endregion
    }
}
