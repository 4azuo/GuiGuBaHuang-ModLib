﻿using EBattleTypeData;
using ModLib.Attributes;
using ModLib.Helper;
using ModLib.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Application = UnityEngine.Application;

namespace ModLib.AutoTest
{
    /// <summary>
    /// In-game test runner of a mod, driven by scripts/game/run-autotest.ps1 of ModLib (shared by the mods).
    /// Loaded only when &lt;mod cache&gt;\autotest\request.json exists (never for normal players).
    /// Phases: title screen scenarios -> load the save (automatically or by the tester) -> game scenarios
    /// -> optional battle watching. Results and screenshots are written to the request OutDir.
    /// The runner never saves the game; use a throwaway test save anyway.
    /// A mod declares its runner as a subclass with a Cache attribute, e.g.
    /// [Cache("AutoTestEvent", CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.All)]
    /// public class AutoTestEvent : AutoTestRunner { ... }, and its scenarios as IScenarioDefinition classes of its
    /// assembly. The hooks (ModVersion, OnRequestLoaded, OnNewCharacterRequested, CaptureModState / RestoreModState)
    /// add what is specific to the mod.
    /// </summary>
    public abstract partial class AutoTestRunner : ModEvent
    {
        /// <summary>Runner phases.</summary>
        /// <summary>Idle: a game that stays open (request.KeepAlive) waits in the save for its next request.</summary>
        public enum PhaseEnum { WaitTitle, Title, Loading, WaitGame, Game, Watch, Idle, Done }

        /// <summary>The running runner (set by OnLoadClass), null outside a run.</summary>
        public static AutoTestRunner Instance { get; set; }

        /// <summary>Popups seen on the title screen before the scenarios (closed by the runner).</summary>
        public static List<string> TitlePopups { get; } = new List<string>();

        /// <summary>Result of the last watched battle.</summary>
        public static bool LastBattleWin { get; set; }

        // ------------------------------------------------------------------ hooks of the mod

        /// <summary>Version of the mod, written into results.json ("modVersion").</summary>
        protected virtual string ModVersion => null;

        /// <summary>
        /// The scenarios: every IScenarioDefinition of the assembly of the runner (the mod) and of ModLib (its tools, e.g.
        /// the effect galleries TE-xx, run only when asked by id) by default.
        /// </summary>
        protected virtual List<Scenario> CreateScenarios() => ScenarioRegistry.Create(GetType().Assembly, typeof(AutoTestRunner).Assembly);

        /// <summary>True when a test may fight this NPC (a mod leaves out its companions...).</summary>
        public virtual bool CanFightInTests(WorldUnitBase npc) => true;

        /// <summary>A request was loaded (the mod switches off what would disturb the steps, e.g. dialogs opening by themselves).</summary>
        protected virtual void OnRequestLoaded(AutoTestRequest request) { }

        /// <summary>The run asks for a new character (the pre-test of the mod creates it) or not.</summary>
        protected virtual void OnNewCharacterRequested(bool on) { }

        /// <summary>
        /// Data of the mod for the player when the save is loaded (name -> JSON, null when none), put back before every
        /// game scenario by RestoreModState so that a scenario does not leak into the next one.
        /// </summary>
        protected virtual Dictionary<string, string> CaptureModState(WorldUnitBase player) => new Dictionary<string, string>();

        /// <summary>Puts the data of CaptureModState back; returns the names of what changed.</summary>
        protected virtual List<string> RestoreModState(WorldUnitBase player, Dictionary<string, string> state) => new List<string>();

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
        private DateTime _startedAt = DateTime.Now;
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

        /// <summary>
        /// Game slot of this game (GuiGuBaHuang-ModLib/scripts/game/new-game-slot.ps1): 1 for the normal game folder, N for a copy started from
        /// %LOCALAPPDATA%\ggbh-slotN (several games side by side, each with its own autotest folder).
        /// </summary>
        public static int Slot
        {
            get
            {
                if (_slot == 0)
                {
                    _slot = 1;
                    try
                    {
                        var m = Regex.Match(Application.dataPath ?? "", @"ggbh-slot(\d+)", RegexOptions.IgnoreCase);
                        if (m.Success) _slot = int.Parse(m.Groups[1].Value);
                    }
                    catch { }
                }
                return _slot;
            }
        }
        private static int _slot;

        /// <summary>Folder of the request/stop files: &lt;mod cache&gt;\autotest\ (slot N: autotest-slotN\).</summary>
        public static string GetAutoTestFolder(string modId)
        {
            return Path.Combine(CacheHelper.GetCacheFolderName(modId), Slot > 1 ? $"autotest-slot{Slot}" : "autotest");
        }

        /// <summary>A request whose script no longer runs is moved to this file (kept for a look, never read).</summary>
        public const string STALE_REQUEST_FILE = "request.stale.json";

        /// <summary>Loads the runner only when a request file of a running run-autotest.ps1 exists.</summary>
        public override bool OnCacheHandler()
        {
            return ReadActiveRequest(GetAutoTestFolder(ModId)) != null;
        }

        /// <summary>
        /// The request of <paramref name="folder"/>, null when there is none, it cannot be read (being written) or the
        /// run-autotest.ps1 that wrote it no longer runs (AutoTestRequest.OwnerPid): such a stale request is moved to
        /// request.stale.json, so a game started normally later plays without the runner.
        /// </summary>
        public static AutoTestRequest ReadActiveRequest(string folder)
        {
            var file = Path.Combine(folder, REQUEST_FILE);
            if (!File.Exists(file)) return null;
            AutoTestRequest request;
            try { request = JsonConvert.DeserializeObject<AutoTestRequest>(File.ReadAllText(file)); }
            catch { return null; }
            if (request == null) return null;
            if (request.IsOwnerAlive()) return request;
            try
            {
                var stale = Path.Combine(folder, STALE_REQUEST_FILE);
                if (File.Exists(stale)) File.Delete(stale);
                File.Move(file, stale);
                DebugHelper.WriteLine($"AutoTestEvent: request {request.RunId} ignored, its run-autotest.ps1 (pid {request.OwnerPid}) does not run any more: moved to {STALE_REQUEST_FILE}");
            }
            catch (Exception ex) { DebugHelper.WriteIgnoredError("AutoTestEvent stale request", ex); }
            return null;
        }

        /// <summary>Reads the request and prepares the output folder.</summary>
        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            Instance = this;
            _request = ReadActiveRequest(GetAutoTestFolder(modId));
            if (_request == null)
                return;
            OnNewCharacterRequested(_request.NewCharacter);
            if (string.IsNullOrEmpty(_request.OutDir))
                _request.OutDir = Path.Combine(GetAutoTestFolder(modId), _request.RunId ?? DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(Path.Combine(_request.OutDir, "screens"));
            // hang finder: names the event handler that blocks the main thread (written by a background thread)
            EventTraceHelper.Start(Path.Combine(_request.OutDir, "event-trace.log"));
            // no exception dialog during the run (it blocks the steps): errors are still logged and listed in the report
            DebugHelper.IsDebugMode = false;
            OnRequestLoaded(_request);
            Mute();
            Log($"request loaded: run={_request.RunId}, save={_request.Save}, autoLoad={_request.AutoLoad}, scenarios={(_request.Scenarios.Count == 0 ? "all" : string.Join(",", _request.Scenarios))}");
            WriteResults();
        }

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
            if ((DateTime.Now - _dramaSince.Value).TotalSeconds >= DramaTimeoutSec && DramaHelper.DismissDramas() > 0)
            {
                DebugHelper.WriteLine($"[AUTOTEST] drama dialog dismissed (open for {DramaTimeoutSec} s)");
                _dramaSince = null;
            }
        }

        /// <summary>Drives the state machine (title -> load -> game -> watch -> done).</summary>
        public override void OnTimeUpdate100ms()
        {
            base.OnTimeUpdate100ms();
            // the battle's pause menu (Esc, focus lost) stops the battle: Resume at once, also between the steps
            if (_request != null && _phase != PhaseEnum.Done && GameHelper.IsInBattlle())
            {
                try { if (BattleHelper.ResumeIfPaused()) Log("battle pause menu closed (Resume)"); } catch { }
            }
            // heartbeat: a long wait (a 10-year time skip with 4 games side by side: ~40 s a month) writes no step result;
            // results.json rewritten every 30 s while the mod's ticks run, so the runner's stall watchdog only stops a
            // game whose main thread really stopped
            if (_request != null && (_phase == PhaseEnum.Game || _phase == PhaseEnum.Watch) && (DateTime.Now - _lastWrite).TotalSeconds >= HEARTBEAT_SEC)
            {
                try { WriteResults(); } catch { }
            }
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
                            ApplyWindow();
                        if (_titleSeen == DateTime.MinValue)
                            _titleSeen = DateTime.Now;
                        // popups of the title screen (Workshop descriptions of the mods, game announcements...) appear late
                        foreach (var name in OpenUIs().Select(UIName).Where(n => !IsBaseUI(n)))
                            if (!TitlePopups.Contains(name)) { TitlePopups.Add(name); Log($"title popup: {name}"); }
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
                        else if (!_request.NewCharacter && !NeedsSave())
                        {
                            // only title-screen scenarios selected (e.g. -Scenario TT-06): no save to load (a slot copy
                            // would otherwise create its test character and run the title scenarios again)
                            Log("only title-screen scenarios selected: no save loaded");
                            Finish("DONE");
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
                        var created = _creating;
                        if (!FinishNewCharacter())
                        {
                            _nextAt = DateTime.Now.AddMilliseconds(1000);
                            break;
                        }
                        // read by run-autotest-parallel.ps1 (snapshot of the slot's character)
                        if (_request.SlotCharacter)
                            Log($"slot character: slot={Slot} folder={g.cache.curCache?.data?.folder} id={g.world.playerUnit.GetUnitId()} created={created}");
                        Log($"save loaded: {g.world.playerUnit.GetName()} ({g.world.playerUnit.GetUnitId()}), {GameHelper.GetGameYear()}/{GameHelper.GetGameMonth()}");
                        CaptureBaseline();
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
                case PhaseEnum.Idle:
                    CheckNextRequest();
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
                LastBattleWin = e.isWin;
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
                    UILoginCacheItem item;
                    if (_request.SlotCharacter)
                    {
                        item = FindSlotSave(items);
                        if (item == null)
                        {
                            // the list may still be filling: an empty list is waited for longer (~30 s: many saves load
                            // slowly; a slot game that saw none created a new character for every group, 2026-11-20)
                            if (_loadStep < 3 || (items.Count == 0 && _loadStep < 16))
                            {
                                _nextAt = DateTime.Now.AddMilliseconds(2000);
                                return;
                            }
                            StartNewCharacter();
                            return;
                        }
                    }
                    else
                    {
                        item = string.IsNullOrEmpty(wanted) ? items.FirstOrDefault(x => !x.isDie) :
                            items.FirstOrDefault(x => string.Equals(x.cacheData.data.name, wanted, StringComparison.OrdinalIgnoreCase) ||
                                                      string.Equals(x.cacheData.data.folder, wanted, StringComparison.OrdinalIgnoreCase));
                    }
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
        /// <summary>
        /// The window of the request ("1600x900"): the game's own settings (windowed, size) set and applied - the game
        /// applies them at start, over Unity's command line and registry. Kept after the run.
        /// </summary>
        private void ApplyWindow()
        {
            var m = Regex.Match(_request?.Window ?? "", @"^(\d+)x(\d+)$");
            if (!m.Success) return;
            try
            {
                int w = int.Parse(m.Groups[1].Value), h = int.Parse(m.Groups[2].Value);
                var gs = g.data.globle.gameSetting;
                if (gs.windowModel && gs.screenWidth == w && gs.screenHeight == h && !UnityEngine.Screen.fullScreen) return;
                gs.windowModel = true;
                gs.screenWidth = w;
                gs.screenHeight = h;
                gs.UpdateSetting();
                UnityEngine.Screen.SetResolution(w, h, false);
                Log($"window: {w} x {h}, windowed");
            }
            catch (Exception ex) { Log($"window: {ex.Message}"); }
        }

        #region Test character of a game slot (AutoTestRequest.SlotCharacter)
        /// <summary>Family name of the test characters: "AutoTest Slot1", "AutoTest Slot2"... (kept, used again by the next runs).</summary>
        public const string TEST_FAMILY_NAME = "AutoTest";
        public const string SHARED_FOLDER = "autotest-shared";
        public const string CREATING_LOCK = "creating.lock";
        /// <summary>A creation lock older than this is left by a killed game.</summary>
        public const int CREATING_LOCK_MAX_SEC = 600;
        private bool _creating;
        private bool _waitingLock;
        private DateTime _savedAt = DateTime.MinValue;

        /// <summary>Given name of the test character of this game slot.</summary>
        public static string TestGivenName => $"Slot{Slot}";

        private string SharedFolder => Path.Combine(CacheHelper.GetCacheFolderName(ModId), SHARED_FOLDER);

        private static string NameKey(string name) => new string((name ?? string.Empty).Where(ch => !char.IsWhiteSpace(ch)).ToArray()).ToLowerInvariant();

        /// <summary>The living test character of this slot ("AutoTest Slot&lt;n&gt;"), null when there is none yet.</summary>
        private UILoginCacheItem FindSlotSave(List<UILoginCacheItem> items)
        {
            var key = NameKey(TEST_FAMILY_NAME + TestGivenName);
            var item = items.FirstOrDefault(x => !x.isDie && NameKey(x.cacheData.data.name) == key);
            if (item != null)
                Log($"test character of slot {Slot}: {item.cacheData.data.name} [{item.cacheData.data.folder}]");
            return item;
        }

        /// <summary>
        /// No test character for this slot yet: a new one (TP-01), one game at a time - the game picks the first free save
        /// slot of the list read from the disk, so the next game waits until this one's new save is written.
        /// </summary>
        private void StartNewCharacter()
        {
            Directory.CreateDirectory(SharedFolder);
            var lockFile = Path.Combine(SharedFolder, CREATING_LOCK);
            try
            {
                if (File.Exists(lockFile) && (DateTime.Now - File.GetLastWriteTime(lockFile)).TotalSeconds > CREATING_LOCK_MAX_SEC)
                    File.Delete(lockFile);
                using (var fs = new FileStream(lockFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var sw = new StreamWriter(fs))
                    sw.Write(Slot);
            }
            catch (IOException)
            {
                if (!_waitingLock) Log("an other game creates its test character - waiting");
                _waitingLock = true;
                _loadStep = 3;   // stay on the list, look again
                _nextAt = DateTime.Now.AddMilliseconds(3000);
                return;
            }
            Log($"no test character for slot {Slot}: creating {TEST_FAMILY_NAME} {TestGivenName}");
            _creating = true;
            _request.NewCharacter = true;
            OnNewCharacterRequested(true);
            Enqueue(ScenarioScope.Title, null);
            SetPhase(PhaseEnum.Title, 500);
        }

        /// <summary>
        /// After the creation: the character named "AutoTest Slot&lt;n&gt;", saved, then the creation lock freed once the
        /// save is on the disk. False while it is not there yet (60 s at most).
        /// </summary>
        private bool FinishNewCharacter()
        {
            if (!_creating) return true;
            var folder = g.cache.curCache?.data?.folder;
            if (_savedAt == DateTime.MinValue)
            {
                try
                {
                    g.world.playerUnit.data.unitData.propertyData.name = new UnhollowerBaseLib.Il2CppStringArray(new[] { TEST_FAMILY_NAME, TestGivenName });
                    GameHelper.SaveGame();
                }
                catch (Exception ex) { Log($"name / save of the new character: {ex.Message}"); }
                _savedAt = DateTime.Now;
            }
            var onDisk = !string.IsNullOrEmpty(folder) && Directory.Exists(Path.Combine(SaveCleanupHelper.GameDataPath, "Steam", "CacheData", folder));
            if (!onDisk && (DateTime.Now - _savedAt).TotalSeconds < 60) return false;
            // the game's global data (its list of saves) is written only when the game quits normally; the test games are
            // closed by force: without this the next start of the slot sees no save and creates a new character again
            try { g.data.dataGloble.Save(); }
            catch (Exception ex) { Log($"global data of the game not saved: {ex.Message}"); }
            Log($"test character of slot {Slot}: {g.world.playerUnit.GetName()}, save {folder} {(onDisk ? "written" : "NOT seen on the disk")}; creation lock freed");
            try { File.Delete(Path.Combine(SharedFolder, CREATING_LOCK)); } catch { }
            _creating = false;
            return true;
        }
        #endregion

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
        /// <summary>A save is needed after the title screen: a full run, battle watching or a selected game / battle scenario.</summary>
        private bool NeedsSave()
        {
            // TP-01 on a slot (run-autotest-parallel.ps1): the slot's test character is loaded or created
            return _request.WatchBattles || _request.Scenarios.Count == 0 ||
                (_request.SlotCharacter && _request.Scenarios.Contains("TP-01")) ||
                CreateScenarios().Any(x => x.Scope != ScenarioScope.Title && _request.Scenarios.Contains(x.Id));
        }

        private void Enqueue(ScenarioScope scope, string trigger)
        {
            foreach (var s in CreateScenarios().Where(x => x.Scope == scope && x.Trigger == trigger))
            {
                // the pre-test (TP: new character) always runs: the selected scenarios need its character
                if (_request.Scenarios.Count > 0 && !_request.Scenarios.Contains(s.Id) && !s.Id.StartsWith("TP"))
                    continue;
                // tools (effect galleries...) only when asked by id: never in a full run
                if (s.Kind == ScenarioKind.Tool && !_request.Scenarios.Contains(s.Id))
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
                // a scenario run again in the same run (TP-01: skipped for an existing save, then run to create the
                // test character of the slot) replaces its earlier skipped result: one row per id
                _results.RemoveAll(x => x.Id == result.Id && x.Status == "SKIP");
                _results.Add(result);
                _ctx = new AutoTestContext(result) { ScreenshotFunc = label => Snap($"{result.Id}_{label}") };
                _stepIndex = 0;
                _pendingShot = _request.SnapSteps && _current.SnapSteps ? "before" : null;
                CloseNonBaseUIs($"[{result.Id}] before");
                // on the title screen: the game's announcements (StartGameTip) closed too; it stays a base
                // UI while a save loads (its "mods loaded" warning is confirmed by TryConfirmDialogs)
                if (GameHelper.IsLoginScreen() && g.ui.HasUI(UIType.StartGameTip))
                {
                    Log($"[{result.Id}] before: closing {UIType.StartGameTip.uiName}");
                    g.ui.CloseUI(UIType.StartGameTip);
                }
                // the save list ("Enter World") is a panel of the title screen itself, not a UI of its own: left open by
                // an earlier scenario or the loading of a save it covers the title screen (TT-04's language selector)
                try
                {
                    var login = GameHelper.IsLoginScreen() ? g.ui.GetUI<UILogin>(UIType.Login) : null;
                    if (login != null && login.goCacheRoot != null && login.goCacheRoot.activeSelf)
                    {
                        Log($"[{result.Id}] before: closing the save list");
                        if (login.btnCloseCache != null) login.btnCloseCache.onClick.Invoke();
                        else login.goCacheRoot.SetActive(false);
                    }
                }
                catch (Exception ex) { Log($"[{result.Id}] cannot close the save list: {ex.Message}"); }
                _uisBefore = new HashSet<int>(OpenUIs().Select(x => x.GetInstanceID()));
                Log($"[{result.Id}] start: {result.Title}");
                if (_current.Scope == ScenarioScope.Game && _current.Kind != ScenarioKind.Watch)
                    Prepare(_current, _ctx);
            }
            // evidence: the screen before the first step and after every step (taken just before the next action,
            // once the previous step had StepDelayMs to render); the capture itself needs ~0.3 s (run-autotest.ps1)
            if (_pendingShot != null)
            {
                if (_shotStage == 0 && _stepIndex > 0 && _current.Overlay && !_request.Promo && ShowOverlay())
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
                    if (!_current.KeepPopups) DismissStalePopups();
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
            if (_request.SnapSteps && _current != null && _current.SnapSteps)
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

        private static readonly string[] TIPS_OFF_LABELS = { "disable", "关闭", "關閉", "否", "no" };

        /// <summary>
        /// Before a step: message boxes left by the previous steps (already in their screenshots) are answered, so they
        /// do not cover the next windows - the only button of an information box (OK), and "Disable" of the game's
        /// tutorial-tips prompt ("Tips will pop up... Enable this function?", buttons Enable / Disable). Boxes with a
        /// real choice (Yes / No) are left to the scenario. Off with Scenario.KeepPopups.
        /// </summary>
        private void DismissStalePopups()
        {
            try
            {
                foreach (var ui in OpenUIs().Where(x => UIName(x) == UIType.CheckPopup.uiName).ToList())
                {
                    var buttons = ui.GetComponentsInChildren<UnityEngine.UI.Button>()
                        .Where(b => b != null && b.gameObject.activeInHierarchy && b.interactable)
                        .Select(b => new { Button = b, Label = b.GetComponentInChildren<UnityEngine.UI.Text>()?.text?.Trim() ?? string.Empty })
                        .Where(b => b.Label.Length > 0).ToList();
                    var labels = buttons.Select(b => b.Label.ToLowerInvariant()).ToList();
                    var pick = buttons.Count == 1 ? buttons[0]
                        : labels.Contains("enable") || labels.Contains("开启") || labels.Contains("開啟")
                            ? buttons.FirstOrDefault(b => TIPS_OFF_LABELS.Contains(b.Label.ToLowerInvariant())) : null;
                    if (pick == null) continue;
                    Log($"[{_ctx.Result.Id}] message box left open: click '{pick.Label}'");
                    pick.Button.onClick.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log($"[{_ctx?.Result?.Id}] cannot answer message boxes: {ex.Message}");
            }
        }

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
        /// <summary>
        /// The game muted while the autotest runs (GameHelper.Mute: Unity's master volume, not saved - the game's sound
        /// settings are unchanged). Set when the request is loaded and checked again at every phase.
        /// </summary>
        private static void Mute()
        {
            try { GameHelper.Mute(); } catch { }
        }

        private void SetPhase(PhaseEnum phase, int delayMs)
        {
            Mute();
            _phase = phase;
            _phaseStart = DateTime.Now;
            _nextAt = DateTime.Now.AddMilliseconds(delayMs);
            Log($"phase: {phase}");
            WriteResults();
        }

        private void Finish(string status)
        {
            _phase = KeepsWaiting(status) ? PhaseEnum.Idle : PhaseEnum.Done;
            // the mod logs are shared by the games of every slot: run-game.ps1 of slot N waits for its own line
            Log(Slot > 1 ? $"finished [slot {Slot}]: {status}" : $"finished: {status}");
            WriteResults(status);
            DebugHelper.Save();
        }

        /// <summary>
        /// Requests a screenshot of the game window into OutDir\screens and returns the relative file name.
        /// UnityEngine.ScreenCapture is stripped from the game ("Method unstripping failed"), so the runner writes
        /// a request file (&lt;autotest&gt;\snaps\&lt;n&gt;.req containing the target path) that run-autotest.ps1 captures
        /// within ~0.3 s (steps wait StepDelayMs before changing the screen).
        /// </summary>
        /// <summary>Output folder of the running autotest (null outside a run).</summary>
        public static string OutDir => Instance?._request?.OutDir;

        /// <summary>
        /// Requests a recording of the game window by run-autotest.ps1: frames (jpg, ~10 a second) into frameDir for ms
        /// milliseconds; the request file (&lt;autotest&gt;\snaps\&lt;n&gt;.rec) is deleted when the recording is done.
        /// Returns the request file (null when it could not be written).
        /// </summary>
        public static string Record(string frameDir, int ms)
        {
            try
            {
                var snapDir = Path.Combine(GetAutoTestFolder(Instance.ModId), "snaps");
                Directory.CreateDirectory(snapDir);
                var file = Path.Combine(snapDir, $"rec-{DateTime.Now:HHmmssfff}.rec");
                File.WriteAllText(file, $"{frameDir}|{ms}");
                return file;
            }
            catch { return null; }
        }

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

        private DateTime _lastWrite = DateTime.MinValue;
        /// <summary>Seconds between two writes of results.json while a scenario waits (heartbeat of the stall watchdog).</summary>
        public const int HEARTBEAT_SEC = 30;

        private void WriteResults(string status = null)
        {
            if (_request == null)
                return;
            _lastWrite = DateTime.Now;
            var data = new
            {
                runId = _request.RunId,
                status = status ?? "RUNNING",
                phase = _phase.ToString(),
                startedAt = _startedAt.ToString("s"),
                updatedAt = DateTime.Now.ToString("s"),
                modVersion = ModVersion,
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
