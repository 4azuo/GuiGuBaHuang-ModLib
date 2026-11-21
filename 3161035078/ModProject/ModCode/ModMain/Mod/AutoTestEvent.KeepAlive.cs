using MOD_nE7UL2.AutoTest;
using ModLib.Enum;
using ModLib.Helper;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Games that stay open (run-autotest-parallel.ps1, AutoTestRequest.KeepAlive): once its scenarios are done the game
    /// waits in the save (phase Idle) for the next request - a request.json with another RunId written by
    /// run-autotest.ps1 -Attach - and runs it at once, without a restart. Every game scenario starts from the player
    /// state recorded when the save was loaded (the baseline: grade, exp, money, lifespan, age, place, HP / MP / SP,
    /// S&amp;M settings), then its own Scenario.Precondition.
    /// </summary>
    public partial class AutoTestEvent
    {
        /// <summary>The player when the save was loaded (null before).</summary>
        private class Baseline
        {
            public int Grade, Exp, Money, Life, Age, X, Y;
            public string Configs;
            /// <summary>The player's data of the cultivation paths (merit, body, artisan, Dao heart, spirit sense): JSON, null when none.</summary>
            public System.Collections.Generic.Dictionary<string, string> Paths;
            /// <summary>The unequipped items: soleID -> (item id, count).</summary>
            public System.Collections.Generic.Dictionary<string, (int id, int count)> Bag;
        }

        private Baseline _baseline;

        /// <summary>
        /// The player's record of every cultivation path (a scenario's merit, karma, body levels... must not leak into the
        /// next one: a massacre's karma made the NPCs of the next battle test hostile): read as JSON, written back.
        /// </summary>
        private static readonly (string name, Func<string, string> get, Action<string, string> set)[] PATH_DATA =
        {
            ("merit", id => Json(MeritEvent.Instance?.Merits, id), (id, j) => Put(MeritEvent.Instance?.Merits, id, j)),
            ("body", id => Json(BodyCulEvent.Instance?.Bodies, id), (id, j) => Put(BodyCulEvent.Instance?.Bodies, id, j)),
            ("artisan", id => Json(ArtisanEvent.Instance?.Artisans, id), (id, j) => Put(ArtisanEvent.Instance?.Artisans, id, j)),
            ("Dao heart", id => Json(DaoHeartEvent.Instance?.Hearts, id), (id, j) => Put(DaoHeartEvent.Instance?.Hearts, id, j)),
            ("spirit sense", id => Json(SpiritSenseEvent.Instance?.Senses, id), (id, j) => Put(SpiritSenseEvent.Instance?.Senses, id, j)),
        };

        private static string Json<T>(System.Collections.Generic.Dictionary<string, T> d, string id) =>
            d != null && d.TryGetValue(id, out var v) && v != null ? JsonConvert.SerializeObject(v) : null;

        private static void Put<T>(System.Collections.Generic.Dictionary<string, T> d, string id, string json)
        {
            if (d == null) return;
            if (json == null) d.Remove(id);
            else d[id] = JsonConvert.DeserializeObject<T>(json);
        }
        private DateTime _lastRequestCheck = DateTime.MinValue;

        /// <summary>A game that stays open waits for its next request after a finished batch (not after an error: the runner restarts it).</summary>
        private bool KeepsWaiting(string status) => _request != null && _request.KeepAlive && status == "DONE" && GameHelper.IsInGame();

        /// <summary>Records the player state right after the save was loaded.</summary>
        private void CaptureBaseline()
        {
            try
            {
                var p = g.world.playerUnit;
                var pos = p.GetUnitPos();
                _baseline = new Baseline
                {
                    Grade = p.GetProperty<int>(UnitPropertyEnum.GradeID),
                    Exp = p.GetExp(),
                    Money = p.GetUnitMoney(),
                    Life = p.GetProperty<int>(UnitPropertyEnum.Life),
                    Age = p.GetProperty<int>(UnitPropertyEnum.Age),
                    X = pos.x,
                    Y = pos.y,
                    Configs = SMLocalConfigsEvent.Instance?.Configs == null ? null : JsonConvert.SerializeObject(SMLocalConfigsEvent.Instance.Configs),
                    Paths = PATH_DATA.ToDictionary(x => x.name, x => x.get(p.GetUnitId())),
                    Bag = p.GetUnequippedProps().Where(x => x != null && !string.IsNullOrEmpty(x.soleID))
                        .GroupBy(x => x.soleID).ToDictionary(x => x.Key, x => (x.First().propsID, x.First().propsCount)),
                };
                Log($"baseline: grade {_baseline.Grade}, exp {_baseline.Exp}, money {_baseline.Money}, life {_baseline.Life}, age {_baseline.Age}, at {_baseline.X},{_baseline.Y}");
            }
            catch (Exception ex) { Log($"baseline failed: {ex.Message}"); }
        }

        /// <summary>
        /// Before a game scenario: the player back to the baseline (only what differs; a game that stays open ran other
        /// scenarios before in the same save), no drama left open, then the scenario's precondition. A failed set-up is
        /// a failed check of the scenario (its steps still run).
        /// </summary>
        private void Prepare(Scenario s, AutoTestContext c)
        {
            try
            {
                DramaHelper.DismissDramas();
                var b = _baseline;
                var p = g.world.playerUnit;
                if (b != null && p != null && !GameHelper.IsInBattlle())
                {
                    var changed = new System.Collections.Generic.List<string>();
                    if (p.GetProperty<int>(UnitPropertyEnum.GradeID) != b.Grade) { p.SetProperty<int>(UnitPropertyEnum.GradeID, b.Grade); changed.Add("grade"); }
                    if (p.GetExp() != b.Exp) { p.ClearExp(); p.AddExp(b.Exp); changed.Add("exp"); }
                    if (p.GetUnitMoney() != b.Money) { p.AddUnitMoney(b.Money - p.GetUnitMoney()); changed.Add("money"); }
                    if (p.GetProperty<int>(UnitPropertyEnum.Life) != b.Life) { p.SetProperty<int>(UnitPropertyEnum.Life, b.Life); changed.Add("lifespan"); }
                    if (p.GetProperty<int>(UnitPropertyEnum.Age) != b.Age) { p.SetProperty<int>(UnitPropertyEnum.Age, b.Age); changed.Add("age"); }
                    var pos = p.GetUnitPos();
                    if (pos.x != b.X || pos.y != b.Y) { p.SetUnitPos(new UnityEngine.Vector2Int(b.X, b.Y)); changed.Add("place"); }
                    p.SetProperty<int>(UnitPropertyEnum.Hp, p.GetDynProperty(UnitDynPropertyEnum.HpMax).value);
                    p.SetProperty<int>(UnitPropertyEnum.Mp, p.GetDynProperty(UnitDynPropertyEnum.MpMax).value);
                    p.SetProperty<int>(UnitPropertyEnum.Sp, p.GetDynProperty(UnitDynPropertyEnum.SpMax).value);
                    // the bag: items given by the earlier scenarios removed (a full bag refuses new items), the counts
                    // of the items of the loaded save back
                    var bagChanged = false;
                    foreach (var it in p.GetUnequippedProps().Where(x => x != null && !string.IsNullOrEmpty(x.soleID)).ToList())
                    {
                        if (!b.Bag.TryGetValue(it.soleID, out var was)) { p.RemoveUnitProp(it.soleID); bagChanged = true; }
                        else if (it.propsCount > was.count) { p.RemoveUnitProp(it.soleID, it.propsCount - was.count); bagChanged = true; }
                    }
                    var now = p.GetUnequippedProps().Where(x => x != null && !string.IsNullOrEmpty(x.soleID)).GroupBy(x => x.soleID).ToDictionary(x => x.Key, x => x.First().propsCount);
                    foreach (var kv in b.Bag)
                    {
                        var have = now.TryGetValue(kv.Key, out var n) ? n : 0;
                        if (have < kv.Value.count) { p.AddUnitProp(kv.Value.id, kv.Value.count - have); bagChanged = true; }
                    }
                    if (bagChanged) changed.Add("bag");
                    // the cultivation paths
                    foreach (var pd in PATH_DATA)
                    {
                        var was = b.Paths != null && b.Paths.TryGetValue(pd.name, out var j) ? j : null;
                        if (pd.get(p.GetUnitId()) == was) continue;
                        pd.set(p.GetUnitId(), was);
                        changed.Add(pd.name);
                    }
                    if (changed.Contains("merit") && MeritEvent.Enabled) MeritEvent.Sync(p);
                    if (changed.Contains("artisan") && ArtisanEvent.Enabled)
                        foreach (ArtisanEvent.Craft cr in System.Enum.GetValues(typeof(ArtisanEvent.Craft))) ArtisanEvent.Sync(p, cr);
                    if (b.Configs != null && SMLocalConfigsEvent.Instance?.Configs != null && JsonConvert.SerializeObject(SMLocalConfigsEvent.Instance.Configs) != b.Configs)
                    {
                        JsonConvert.PopulateObject(b.Configs, SMLocalConfigsEvent.Instance.Configs);
                        changed.Add("S&M settings");
                    }
                    if (changed.Count > 0) Log($"[{c.Result.Id}] reset to the loaded save: {string.Join(", ", changed)}");
                }
                if (s.Precondition != null)
                {
                    s.Precondition(c);
                    Log($"[{c.Result.Id}] precondition set");
                }
            }
            catch (Exception ex)
            {
                c.Assert(false, $"precondition: {ex.Message}");
                Log($"[{c.Result.Id}] precondition failed: {ex}");
            }
        }

        /// <summary>Idle: a request.json with another RunId is the next batch (read once a second).</summary>
        private void CheckNextRequest()
        {
            if ((DateTime.Now - _lastRequestCheck).TotalMilliseconds < 1000) return;
            _lastRequestCheck = DateTime.Now;
            var file = Path.Combine(GetAutoTestFolder(ModId), REQUEST_FILE);
            if (!File.Exists(file)) return;
            AutoTestRequest next;
            try { next = JsonConvert.DeserializeObject<AutoTestRequest>(File.ReadAllText(file)); }
            catch { return; }   // being written
            if (next == null || string.IsNullOrEmpty(next.RunId) || next.RunId == _request.RunId) return;
            if (!GameHelper.IsInGame())
            {
                Log("next request, but the save is not open any more");
                return;
            }
            _request = next;
            _request.KeepAlive = true;
            if (string.IsNullOrEmpty(_request.OutDir))
                _request.OutDir = Path.Combine(GetAutoTestFolder(ModId), _request.RunId);
            Directory.CreateDirectory(Path.Combine(_request.OutDir, "screens"));
            EventTraceHelper.Start(Path.Combine(_request.OutDir, "event-trace.log"));   // the hang finder writes to the new run
            _results.Clear();
            _log.Clear();
            _queue.Clear();
            _current = null;
            _snapIndex = 0;
            _startedAt = DateTime.Now;
            Log($"request loaded (same game, no restart): run={_request.RunId}, scenarios={string.Join(",", _request.Scenarios)}");
            if (_request.SlotCharacter)
                Log($"slot character: slot={Slot} folder={g.cache.curCache?.data?.folder} id={g.world.playerUnit.GetUnitId()} created=False");
            Log($"save loaded: {g.world.playerUnit.GetName()} ({g.world.playerUnit.GetUnitId()}), {GameHelper.GetGameYear()}/{GameHelper.GetGameMonth()}");
            Enqueue(ScenarioScope.Game, null);
            SetPhase(PhaseEnum.Game, 1000);
        }
    }
}
