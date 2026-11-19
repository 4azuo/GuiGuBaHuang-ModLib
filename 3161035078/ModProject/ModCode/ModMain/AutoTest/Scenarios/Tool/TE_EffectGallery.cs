using ModLib.Helper;
using ModLib.Mod;
using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Tool
{
    /// <summary>
    /// Effect galleries (tools, not tests: ScenarioKind.Tool, run only when asked with -Scenario). In a battle with the
    /// other units stopped and hidden and the HUD hidden, every effect of a kind (Effect/Battle/Skill, Unit, Scenes) is
    /// played twice on the player:
    /// 1. measure: its renderers / particles are sampled every 100 ms: casting time (until something shows), duration
    ///    (while something shows), size (union of the bounds) and direction (move of the centre);
    /// 2. record: the camera zoomed to its size and centred on it, the effect played again and recorded by
    ///    run-autotest.ps1 (a .rec request: frames of the game window for its duration).
    /// collect-effects.ps1 then makes docs/Effects/&lt;kind&gt;/NNNN_&lt;effect&gt;.webp (animated) and index.json (with the
    /// measures). The list of effects comes from the game's resources (the ModConf list files are only a fallback and
    /// are refreshed from it). Optional range: &lt;autotest folder&gt;\efx-range.txt = "start count".
    /// </summary>
    public abstract class EffectGalleryBase : IScenarioDefinition
    {
        public abstract string Id { get; }
        /// <summary>Resource folder (Effect/Battle/Skill...).</summary>
        protected abstract string Folder { get; }
        /// <summary>ModConf list file (fallback, refreshed by collect-effects.ps1).</summary>
        protected abstract string ListFile { get; }
        protected abstract string Title { get; }

        public const string RANGE_FILE = "efx-range.txt";
        public const string META_FILE = "effects-meta.jsonl";
        public const int SAMPLE_MS = 100;
        /// <summary>An effect is measured at most this long.</summary>
        public const int MAX_MS = 6000;
        /// <summary>Nothing visible this long after it showed: the effect is over.</summary>
        public const int END_GAP_MS = 400;
        /// <summary>Nothing visible after this long: an effect without visible parts.</summary>
        public const int NOTHING_MS = 2500;
        /// <summary>Renderers bigger than this (world units) are screen-wide tints: left out of the size.</summary>
        public const float MAX_PART = 15f;

        private static GameObject _last;
        private static WorldUnitBase _npc;
        private static Measure _m;
        private static float _camWanted;
        /// <summary>First error reading the particle settings (diagnostic, logged once).</summary>
        private static string _psError;
        private static string _rec;

        /// <summary>Samples of one effect.</summary>
        private class Measure
        {
            public DateTime Start = DateTime.Now;
            public double FirstMs = -1, LastMs = -1;
            public Vector2 Min = new Vector2(float.MaxValue, float.MaxValue), Max = new Vector2(float.MinValue, float.MinValue);
            public Vector2? FirstCentre, LastCentre;
            public Vector3 Origin;
            /// <summary>Timing of the particle systems (-1: none): first start delay, end of the longest, a loop.</summary>
            public int PsDelayMs = -1, PsDurMs;
            public bool PsLoop;
            /// <summary>Still showing at the end of the measure: a looping effect (the skill removes it).</summary>
            public bool Looping;
            /// <summary>Component types of the effect (diagnostic).</summary>
            public string Types;
            public bool Seen => FirstMs >= 0;
            public double Now => (DateTime.Now - Start).TotalMilliseconds;
        }

        private static string OutDir => AutoTestEvent.OutDir ?? Path.GetTempPath();

        private static int[] Range()
        {
            try
            {
                var range = Path.Combine(AutoTestEvent.GetAutoTestFolder(ModMain.ModObj.ModId), RANGE_FILE);
                if (File.Exists(range))
                    return File.ReadAllText(range).Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            }
            catch { }
            return new int[0];
        }

        /// <summary>The effects of the kind: the game's resources (in the game), else the ModConf list; cut by efx-range.txt.</summary>
        public List<string> ReadList(out string source)
        {
            source = "none";
            var all = new List<string>();
            try
            {
                all = BattleEffectHelper.ListEffects(Folder);
                if (all.Count > 0) source = "game resources";
            }
            catch (Exception ex) { source = "error: " + ex.Message; }
            try
            {
                if (all.Count == 0)
                {
                    all = File.ReadAllLines(ConfHelper.GetConfFilePath(ModMain.ModObj.ModId, ListFile)).Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
                    source = "ModConf/" + ListFile;
                }
                else
                {
                    // the fresh list for collect-effects.ps1 (it updates ModConf)
                    File.WriteAllLines(Path.Combine(OutDir, ListFile), all);
                }
                var r = Range();
                if (r.Length > 0) all = all.Skip(r[0]).Take(r.Length > 1 ? r[1] : int.MaxValue).ToList();
            }
            catch { }
            return all;
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            var list = ReadList(out var source);
            var r = Range();
            var startIndex = r.Length > 0 ? r[0] : 0;
            var sc = new Scenario { Id = Id, Feature = "Effects", Title = Title, Kind = ScenarioKind.Tool, Overlay = false, SnapSteps = false }
                .Step("start battle", c =>
                {
                    _npc = Npcs.Where(x => !HirePeopleEvent.IsHired(x) && !x.isDie).OrderBy(x => x.GetGradeLvl()).FirstOrDefault();
                    if (!c.Assert(_npc != null, "an NPC to fight")) return;
                    c.Info($"{list.Count} effects of {Folder} (list from {source})");
                    g.world.battle.IntoBattle(_npc, MapBuildPropertyEvent.CATCH_SNEAKY_DUNGEON_ID, battleData: new WorldBattleData { npcUnit = _npc });
                })
                .Wait("battle loaded", c => { StartBattle(); ProtectPlayer(); return _npc == null || InBattle; }, 90000, 1000)
                .Wait("battle started", c => { ProtectPlayer(); return _npc == null || StartBattle(); }, 60000, 1000)
                .Step("stage", c => Stage(c, true), 1500);
            for (var k = 0; k < list.Count; k++)
            {
                var i = k;
                var path = list[i];
                var n = startIndex + i;
                var tag = $"{n:0000}_{Path.GetFileName(path)}";
                sc = sc
                    .Step($"{tag} measure", c => StartMeasure(path), 0)
                    .Wait($"{tag} measured", c => SampleMeasure(), MAX_MS + 2000, SAMPLE_MS, 0)
                    // the camera first (it needs a few frames), then the recorder, then the effect once the recorder runs
                    .Step($"{tag} camera", c => PlaceCamera(c, path, n, tag), 0)
                    .Step($"{tag} record", c => StartRecorder(c, tag), CAMERA_SETTLE_MS)
                    .Wait($"{tag} recording", c =>
                    {
                        if (_plan == null) return true;
                        if (!File.Exists(Path.Combine(_plan.Frames, "started"))) return false;
                        Create(_plan.Path, _plan.Origin);
                        return true;
                    }, 10000, 50, 0)
                    .Wait($"{tag} recorded", c =>
                    {
                        var done = _rec == null || !File.Exists(_rec);
                        // the camera held in place while recording (shakes, impacts)
                        if (!done) BattleSceneHelper.StopCameraShake();
                        if (done && i < 3) c.Info($"{tag}: camera ortho size {BattleSceneHelper.CameraCtrl?.ca?.orthographicSize:0.##} (wanted {_camWanted:0.##})");
                        return done;
                    }, MAX_MS + 30000, 200, 0);
            }
            return sc
                .Step("clear", c => { Clear(); Stage(c, false); }, 500)
                .Step("win", c => { if (InBattle) EndBattle(true); }, 1500)
                .Wait("back to the map", c => LeaveBattle(), 90000, 2000);
        }

        // ------------------------------------------------------------------ stage

        private static readonly List<GameObject> _hidden = new List<GameObject>();
        private static bool _shakeBefore = true;

        /// <summary>Largest camera size (ortho) when zooming out on a big effect.</summary>
        public const float MAX_CAMERA = 8f;

        /// <summary>
        /// A clean stage (BattleSceneHelper): the other units stopped and hidden far away, every effect removed, the HUD,
        /// the weather, the player's view and the mouse off, the player in the middle of the map with its picture hidden
        /// (the effects play on it); restored after the gallery.
        /// </summary>
        private static void Stage(AutoTestContext c, bool on)
        {
            if (!InBattle) return;
            if (on)
            {
                _hidden.Clear();
                _hidden.AddRange(BattleSceneHelper.StopAndHideOtherUnits());
                BattleEffectHelper.RemoveAllEffects();
                _last = null;
                var mouse = BattleSceneHelper.HideMouse();
                _hidden.AddRange(mouse);
                _hidden.Add(BattleSceneHelper.HideHud());
                _hidden.Add(BattleSceneHelper.HideWeather());
                _hidden.Add(BattleSceneHelper.HidePlayerView());
                _shakeBefore = BattleSceneHelper.StopCameraShake();
                var centre = BattleSceneHelper.GetMapCentre();
                if (centre.HasValue)
                {
                    ModBattleEvent.PlayerUnit.MoveUnit(centre.Value);
                    c.Info($"player moved to the middle of the map {centre.Value.x:0.#}, {centre.Value.y:0.#}");
                }
                _hidden.Add(ModBattleEvent.PlayerUnit.HideUnitBody());
                _hidden.RemoveAll(x => x == null);
                c.Info($"hidden and stopped: {_hidden.Count} object(s) (mouse: {mouse.Count}{(mouse.Count > 0 ? " - " + string.Join(", ", mouse.Select(x => x.name)) : "")}); camera ortho size {BattleSceneHelper.CameraCtrl?.ca?.orthographicSize:0.##}");
            }
            else
            {
                BattleSceneHelper.ReleaseCamera();
                BattleSceneHelper.SetCameraShake(_shakeBefore);
                BattleSceneHelper.ShowAgain(_hidden);
                _hidden.Clear();
            }
        }

        // ------------------------------------------------------------------ measure

        /// <summary>Where the effects play: on the player (the camera is placed by hand when recording).</summary>
        private static Vector3 Spot => ModBattleEvent.PlayerUnit.transform.position;

        private static void StartMeasure(string path)
        {
            if (!InBattle) return;
            ProtectPlayer();
            BattleEffectHelper.RemoveAllEffects();
            _last = null;
            // the game may make the mouse objects again
            _hidden.AddRange(BattleSceneHelper.HideMouse());
            _m = new Measure { Origin = Spot };
            Create(path, _m.Origin);
        }

        /// <summary>The timing of the particle systems of the effect (once, when it exists).</summary>
        private static void ReadParticles()
        {
            if (_m == null || _m.PsDelayMs >= 0 || _last == null) return;
            if (_m.Types == null) _m.Types = BattleEffectHelper.GetComponentSummary(_last);
            _m.PsDelayMs = 0;   // read once
        }

        /// <summary>One sample; true when the effect is over (or invisible, or too long).</summary>
        private static bool SampleMeasure()
        {
            if (_m == null || !InBattle) return true;
            try { ReadParticles(); } catch (Exception ex) { if (_psError == null) _psError = ex.ToString(); }
            var t = _m.Now;
            var b = BattleEffectHelper.GetVisibleBounds(_last, MAX_PART);
            var visible = b.HasValue;
            if (visible)
            {
                var min = new Vector2(b.Value.min.x, b.Value.min.y);
                var max = new Vector2(b.Value.max.x, b.Value.max.y);
                if (!_m.Seen) { _m.FirstMs = t; _m.FirstCentre = (min + max) / 2f; }
                _m.LastMs = t;
                _m.LastCentre = (min + max) / 2f;
                _m.Min = Vector2.Min(_m.Min, min);
                _m.Max = Vector2.Max(_m.Max, max);
            }
            if (t >= MAX_MS) { _m.Looping = visible; return true; }
            if (!_m.Seen) return t >= NOTHING_MS;
            return !visible && t - _m.LastMs >= END_GAP_MS;
        }

        // ------------------------------------------------------------------ record

        /// <summary>Wait after the camera is placed, before the effect is created and recorded (the camera settles).</summary>
        public const int CAMERA_SETTLE_MS = 600;
        /// <summary>Length of the recording of a looping effect: as long as it was measured (it plays until the skill removes it).</summary>
        public const int LOOP_RECORD_MS = MAX_MS;

        /// <summary>What the record step plays: the effect, where, and the frames folder and length of its recording.</summary>
        private class RecordPlan
        {
            public string Path, Frames;
            public Vector3 Origin;
            public int Ms;
            /// <summary>Camera of the recording: centre and orthographic size.</summary>
            public Vector2 Centre;
            public float Size;
        }
        private static RecordPlan _plan;

        /// <summary>The measures written, the camera placed on the effect (null plan: nothing to record).</summary>
        private static void PlaceCamera(AutoTestContext c, string path, int index, string tag)
        {
            _rec = null;
            _plan = null;
            if (!InBattle || _m == null) return;
            BattleEffectHelper.RemoveAllEffects();
            _last = null;
            var seen = _m.Seen;
            var size = seen ? _m.Max - _m.Min : Vector2.zero;
            // camera: centred on the spot the effect is created on (the middle of the map), zoomed so the whole effect
            // shows (the farthest edge from the spot on each axis, both sides) - it does not follow each effect
            var origin = new Vector2(_m.Origin.x, _m.Origin.y);
            var centre = origin;
            var reach = seen ? 2f * Vector2.Max(origin - _m.Min, _m.Max - origin) : Vector2.zero;
            var baseSize = BattleSceneHelper.CameraCtrl?.orthoSize ?? 5.4f;
            var want = seen ? BattleSceneHelper.GetOrthoSizeFor(reach, 1.3f, 1.2f, MAX_CAMERA) : baseSize;
            // the view inside the map (outside it shows no ground, and the game pulls the camera back in)
            BattleSceneHelper.FitInMap(ref centre, ref want);
            var factor = baseSize > 0 ? want / baseSize : 1f;
            if (_m.Types != null && index < 3) c.Info($"{tag}: components {_m.Types}");
            if (_psError != null) { c.Info($"particle settings not read: {_psError}"); _psError = ""; }
            // held every frame: the effects of the game shake / move the camera
            BattleSceneHelper.HoldCamera(centre, want);
            _camWanted = want;
            // timing as seen (the samples every 100 ms: the particle settings cannot be read - their MainModule
            // accessors are stripped from the game)
            var castMs = seen ? (int)_m.FirstMs : -1;
            var durMs = seen ? (int)(_m.LastMs - _m.FirstMs) : 0;
            var loop = _m.Looping;   // still showing at the end of the measure
            if (loop) durMs = -1;   // no end of its own (the skill removes it)
            var meta = new Dictionary<string, object>
            {
                ["id"] = c.Result.Id, ["index"] = index, ["name"] = Path.GetFileName(path), ["path"] = path,
                ["visible"] = seen, ["castingTimeMs"] = castMs, ["durationMs"] = durMs, ["loop"] = loop,
                ["direction"] = seen ? Direction(_m) : "none",
                ["width"] = Math.Round(size.x, 2), ["height"] = Math.Round(size.y, 2), ["zoom"] = Math.Round(factor, 2),
                ["camera"] = Math.Round(want, 2),
            };
            try { File.AppendAllText(Path.Combine(OutDir, META_FILE), JsonConvert.SerializeObject(meta) + Environment.NewLine); } catch { }
            if (!seen) return;   // nothing to record
            _plan = new RecordPlan
            {
                Path = path, Origin = _m.Origin, Centre = centre, Size = want,
                Frames = Path.Combine(OutDir, "effects", c.Result.Id, tag),
                // from its start to its end (+ a margin; + the time between the recorder start and the effect)
                Ms = (loop ? LOOP_RECORD_MS : Math.Max(1000, Math.Min(MAX_MS, (int)_m.LastMs + END_GAP_MS))) + 300,
            };
        }

        /// <summary>The recording asked (run-autotest.ps1 writes "started" in the frames folder when it captures).</summary>
        private static void StartRecorder(AutoTestContext c, string tag)
        {
            if (_plan == null) return;
            _rec = AutoTestEvent.Record(_plan.Frames, _plan.Ms);
        }

        /// <summary>Direction of the move of the effect (its centre from the first to the last sample).</summary>
        private static string Direction(Measure m)
        {
            if (m.FirstCentre == null || m.LastCentre == null) return "static";
            var size = Mathf.Max((m.Max - m.Min).x, (m.Max - m.Min).y);
            return (m.LastCentre.Value - m.FirstCentre.Value).ToDirection8(Mathf.Max(0.5f, size * 0.2f));
        }

        /// <summary>The effect created alone: its unit commands removed (they would stop the game's timers).</summary>
        private static void Create(string path, Vector3 pos)
        {
            BattleEffectHelper.CreateEffect(path, pos, MAX_MS / 1000f + 2f, go =>
            {
                _last = go;
                // its camera controls off: the camera stays on the recorded area
                BattleEffectHelper.RemoveCameraControls(go);
            }, disarm: true);
        }

        private static void Clear()
        {
            BattleEffectHelper.DestroyEffect(_last);
            _last = null;
        }
    }

    /// <summary>TE-01: skill effects (Effect/Battle/Skill).</summary>
    public class TE01_SkillEffects : EffectGalleryBase
    {
        public override string Id => "TE-01";
        protected override string Folder => "Effect/Battle/Skill";
        protected override string ListFile => "_Def_Res_Efx_Battle_Skill.txt";
        protected override string Title => "Effect gallery: skill effects";
    }

    /// <summary>TE-02: unit effects (Effect/Battle/Unit).</summary>
    public class TE02_UnitEffects : EffectGalleryBase
    {
        public override string Id => "TE-02";
        protected override string Folder => "Effect/Battle/Unit";
        protected override string ListFile => "_Def_Res_Efx_Battle_Unit.txt";
        protected override string Title => "Effect gallery: unit effects";
    }

    /// <summary>TE-03: scene effects (Effect/Battle/Scenes).</summary>
    public class TE03_SceneEffects : EffectGalleryBase
    {
        public override string Id => "TE-03";
        protected override string Folder => "Effect/Battle/Scenes";
        protected override string ListFile => "_Def_Res_Efx_Battle_Scenes.txt";
        protected override string Title => "Effect gallery: scene effects";
    }
}
