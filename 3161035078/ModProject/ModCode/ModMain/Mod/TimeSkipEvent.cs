using MOD_nE7UL2.Helper;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Helper;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Map buttons to skip 1 or 10 years (also used by seclusions), with exp from MP/SP.
    /// The month of ModLib and its mods (OnMonthly, OnYearly, loans, families...) runs every Stride months (1 a year
    /// skipped: 1 year -> every month, 10 years -> every 10 months, 100 years -> every 100 months: 12 runs whatever the
    /// length); each run stands for the months passed (ModMaster.MonthsPassed / YearsPassed: the handlers scale by them).
    /// SAVES times (1 year -> every 3 months, 2 years -> every 6 months...) the month run of the game itself runs
    /// instead (NPCs cultivate, break through, marry, die) and the game saves, the last one at the end.
    /// The loading window of the game shows the progress meanwhile.
    /// </summary>
    [Cache(ModConst.TIME_SKIP_EVENT)]
    public class TimeSkipEvent : ModEvent
    {
        public static TimeSkipEvent Instance { get; set; }

        public const int SAVES = 4;
        public const int MONTH_DELAY = 200;

        [JsonIgnore]
        public bool IsProcessing { get; set; }
        [JsonIgnore]
        public int Total { get; private set; }
        [JsonIgnore]
        public int Done { get; private set; }

        /// <summary>The month run of the game every skipped month (saved only at the save points); slower, see the timings.</summary>
        [JsonIgnore]
        public static bool FullSimulation { get; set; }

        /// <summary>Timings of the last skip (ms): all, the month runs of the game, the months of the mods only.</summary>
        [JsonIgnore]
        public Timing LastTiming { get; private set; } = new Timing();

        public class Timing
        {
            public int Months;
            public long TotalMs;
            public int GameRuns;
            public long GameRunMs;
            public int ModMonths;
            public long ModMonthMs;
            public override string ToString() =>
                $"{Months} months in {TotalMs / 1000.0:0.0} s: {GameRuns} month runs of the game ({(GameRuns == 0 ? 0 : GameRunMs / GameRuns / 1000.0):0.0} s each), {ModMonths} months of the mods ({(ModMonths == 0 ? 0 : ModMonthMs / ModMonths / 1000.0):0.00} s each)";
        }

        private readonly System.Diagnostics.Stopwatch _watch = new System.Diagnostics.Stopwatch();

        /// <summary>Months between two saves of a skip of the given length (SAVES saves in all).</summary>
        public static int SaveEvery(int months) => Math.Max(1, (months + SAVES - 1) / SAVES);

        /// <summary>Months one dispatch of the mods stands for: 1 a year skipped (1 year -> every month, 2 years -> every 2 months, 100 years -> every 100 months).</summary>
        public static int Stride(int months) => Math.Max(1, months / 12);

        /// <summary>
        /// Next stop of a skip after `done` months: at most Stride months further, stopping on the first save point
        /// (a month run of the game) on the way. Full simulation: every month. Returns the month reached and whether it is a save point.
        /// </summary>
        public static (int to, bool save) NextStep(int done, int total, bool full = false)
        {
            var last = full ? done + 1 : Math.Min(total, done + Stride(total));
            for (var m = done + 1; m <= last; m++)
                if (SaveAfter(m, total)) return (m, true);
            return (last, false);
        }

        /// <summary>Is the game saved after this month of the skip (1-based)?</summary>
        /// (evenly spread: exactly SAVES saves when months >= SAVES, the last one at the end; every month for shorter skips)
        public static bool SaveAfter(int done, int months) => done >= months || (long)done * SAVES / months > (long)(done - 1) * SAVES / months;

        /// <summary>
        /// MapMain: adds the skip buttons.
        /// </summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (IsProcessing && DRAMAS.Any(t => t.uiName == e.uiType.uiName))
                Interrupt($"drama {e.uiType.uiName}");
            if (e.uiType.uiName == UIType.MapMain.uiName)
            {
                var uiMain = new UICover<UIMapMain>(e.ui);
                {
                    uiMain.AddButton(uiMain.FirstCol + 12, uiMain.LastRow - 4, () =>
                    {
                        InfoDialog.Msg(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020029"), 1), MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTime(12);
                        });
                    }, string.Format(GameTool.LS("other500020030"), 1)).Size(140, 40);
                    uiMain.AddButton(uiMain.FirstCol + 15, uiMain.LastRow - 4, () =>
                    {
                        InfoDialog.Msg(GameTool.LS("other500020022"), string.Format(GameTool.LS("other500020029"), 10), MsgBoxButtonEnum.YesNo, () =>
                        {
                            SkipTime(120);
                        });
                    }, string.Format(GameTool.LS("other500020030"), 10)).Size(140, 40);
                }
            }
        }

        /// <summary>
        /// Skips the months one by one (a month each MONTH_DELAY ms), then calls done (seclusion).
        /// </summary>
        public void SkipTime(int months, Action done = null)
        {
            if (months <= 0 || IsProcessing)
            {
                done?.Invoke();
                return;
            }
            IsProcessing = true;
            Interrupted = false;
            if (HidePlayer) Hide(true);
            Total = months;
            Done = 0;
            _monthsWithoutGameRun = 0;
            LastTiming = new Timing { Months = months };
            _watch.Restart();
            DebugHelper.WriteLine($"TimeSkipEvent: {months} months, save every {SaveEvery(months)}{(FullSimulation ? ", month run of the game every month" : string.Empty)}");
            ShowProgress();
            NextMonth(done);
        }

        /// <summary>Dramas that interrupt a skip (an enemy finds the player, an NPC visits...).</summary>
        private static readonly UIType.UITypeBase[] DRAMAS = { UIType.DramaDialogue, UIType.DramaFortuitous, UIType.DramaDialogueBigTexture, UIType.DramaBigTexture };

        /// <summary>
        /// Something happened to the player during the skip (a drama: an enemy, a visitor, an ambush...; a battle): the
        /// skip stops after the month in progress, so the player can answer it. A seclusion ends early.
        /// </summary>
        [JsonIgnore]
        public bool Interrupted { get; private set; }

        /// <summary>Tests only: dramas / battles do not stop the skip (closed by the test runner): a full length.</summary>
        [JsonIgnore]
        public static bool NoInterrupt { get; set; }

        private void Interrupt(string why)
        {
            if (Interrupted || NoInterrupt) return;
            Interrupted = true;
            DebugHelper.WriteLine($"TimeSkipEvent: interrupted at month {Done}/{Total} ({why})");
            HideProgress();
        }

        /// <summary>A battle starts during the skip: interrupted.</summary>
        public override void OnBattleStart(ETypeData e)
        {
            base.OnBattleStart(e);
            if (IsProcessing) Interrupt("battle");
        }

        /// <summary>A drama or a battle waits for the player: the month run of the game waits too (no timeout).</summary>
        private static bool PlayerBusy() => GameHelper.IsInBattlle() || DRAMAS.Any(t => g.ui.HasUI(t));

        /// <summary>Tests only: the player is hidden during the skip (no NPC drama: deterministic timings).</summary>
        [JsonIgnore]
        public static bool HidePlayer { get; set; }

        /// <summary>Feature "hidden unit" of the game: other units cannot target the player.</summary>
        public const int HIDDEN_LUCK = 115;

        private static void Hide(bool on)
        {
            try { g.world.playerUnit.CreateAction(on ? (UnitActionBase)new UnitActionLuckAdd(HIDDEN_LUCK, -1) : new UnitActionLuckDel(HIDDEN_LUCK), false); }
            catch (Exception ex) { DebugHelper.WriteLine($"TimeSkipEvent.Hide({on}): {ex.Message}"); }
        }

        /// <summary>A skip interrupted (game closed) left the player hidden: shown again.</summary>
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            if (!IsProcessing && HidePlayer) Hide(false);
        }

        /// <summary>Milliseconds after which a month run of the game that did not call back is considered over.</summary>
        public const int RUN_TIMEOUT_MS = 120000;

        /// <summary>Months since the last month run of the game (the units did not age during them).</summary>
        private int _monthsWithoutGameRun;

        /// <summary>
        /// One skipped month. A month of the game itself (NPCs cultivate, break through, marry, die of old age... and the
        /// game saves) takes ~15 s, so it runs only SAVES times (the save points, SaveAfter); the other months run the
        /// month of ModLib and its mods only (~3 s, no save). Before a month run of the game every unit ages by the months
        /// passed without one, so lifespans and deaths follow the time skipped.
        /// </summary>
        private void NextMonth(Action done)
        {
            ModDelayEvent.Instance.DelayEvent(this, () =>
            {
                var (to, save) = NextStep(Done, Total, FullSimulation);
                var advance = to - Done;
                // the months of the step before its last one only move the calendar; ModLib then dispatches the
                // step once with ModMaster.MonthsPassed = advance (YearsPassed for the years crossed)
                for (var i = 1; i < advance; i++)
                    g.world.run.AddDay(g.world.run.roundDayMax + 1, false);
                _monthsWithoutGameRun += advance - 1;
                Done = to;
                var month = Done;
                var finished = false;
                var gameRun = save || FullSimulation;
                var start = _watch.ElapsedMilliseconds;
                Action after = () =>
                {
                    if (finished || month != Done) return;
                    finished = true;
                    var ms = _watch.ElapsedMilliseconds - start;
                    if (gameRun) { LastTiming.GameRuns++; LastTiming.GameRunMs += ms; }
                    else { LastTiming.ModMonths++; LastTiming.ModMonthMs += ms; }
                    ModLib.Mod.ModMaster.NoSaveOnWorldRunEnd = false;
                    var player = g.world.playerUnit;
                    player.AddExp((player.GetDynProperty(UnitDynPropertyEnum.Mp).value / 10 + player.GetDynProperty(UnitDynPropertyEnum.Sp).value / 2) * advance);
                    if (!Interrupted) ShowProgress();
                    if (Done >= Total || Interrupted)
                        WhenSaved(() =>
                        {
                            LastTiming.Months = Done;
                            LastTiming.TotalMs = _watch.ElapsedMilliseconds;
                            DebugHelper.WriteLine($"TimeSkipEvent: {(Interrupted ? "interrupted" : "done")}, {LastTiming}");
                            IsProcessing = false; if (HidePlayer) Hide(false); HideProgress(); done?.Invoke();
                        });
                    else
                        WhenSaved(() => NextMonth(done));
                };
                ModLib.Mod.ModMaster.NoSaveOnWorldRunEnd = !save;
                if (gameRun && RunGameMonth(after))
                {
                    WatchRun(() => finished || month != Done, after, 0);
                    return;
                }
                // month of the mods only
                _monthsWithoutGameRun++;
                g.world.run.AddDay(g.world.run.roundDayMax + 1, false);
                EventHelper.CallGameEvent<WorldRunStart>(EGameType.WorldRunStart);
                EventHelper.CallGameEvent<WorldRunEnd>(EGameType.WorldRunEnd);
                after();
            }, MONTH_DELAY);
        }

        /// <summary>
        /// Safety net of a month run of the game: when the game is no longer running and its end call did not come, go
        /// on; never while it still runs (the next month would run on top of it). Gives up after RUN_TIMEOUT_MS.
        /// </summary>
        private void WatchRun(Func<bool> over, Action after, int waited, int savedAt = -1)
        {
            ModDelayEvent.Instance.DelayEvent(this, () =>
            {
                if (over()) return;
                // the player answers a drama / fights: the run waits for it, so does the skip
                if (PlayerBusy()) { WatchRun(over, after, waited, savedAt); return; }
                waited += 2000;
                // the save closes the month run; an error in the game's own AI can leave it "running" forever after it
                if (savedAt < 0 && DataMgr.isSaveData) savedAt = 0;
                else if (savedAt >= 0 && !DataMgr.isSaveData) savedAt += 2000;
                var stuck = savedAt >= SAVED_GRACE_MS;
                if (g.world.run.isRunning && !stuck && waited < RUN_TIMEOUT_MS) { WatchRun(over, after, waited, savedAt); return; }
                DebugHelper.WriteLine($"TimeSkipEvent: month run of the game {(!g.world.run.isRunning ? "ended without its end call" : stuck ? $"still running {savedAt / 1000} s after its save" : "timed out")}");
                WhenSaved(after);
            }, 2000);
        }

        /// <summary>Milliseconds after the save of a month run of the game after which it is considered over.</summary>
        public const int SAVED_GRACE_MS = 20000;

        /// <summary>The month run of the game (it raises WorldRunStart / WorldRunEnd for the mods and saves). False when it cannot run.</summary>
        private bool RunGameMonth(Action after)
        {
            try
            {
                AgeUnits(_monthsWithoutGameRun);
                _monthsWithoutGameRun = 0;
                // the run starts with the new month; its end call is registered once it is running
                // (registered before, it is called at once: two months would run together and their saves collide)
                g.world.run.AddDay(g.world.run.roundDayMax + 1, true);
                DebugHelper.WriteLine($"TimeSkipEvent: month {Done}/{Total}: month run of the game ({g.world.run.isRunning})");
                if (g.world.run.isRunning)
                {
                    g.world.run.AddRunEndCall(ActionHelper.TracedIl2Action(() => WhenSaved(after)));
                    return true;
                }
                // not started (blocked by the game): the month of the mods, saved; the units age by this month too
                AgeUnits(1);
                EventHelper.CallGameEvent<WorldRunStart>(EGameType.WorldRunStart);
                EventHelper.CallGameEvent<WorldRunEnd>(EGameType.WorldRunEnd);
                after();
                return true;
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"TimeSkipEvent: month run of the game failed ({ex.Message})");
                return false;
            }
        }

        /// <summary>
        /// Every living unit (the player too) ages by the months passed without a month run of the game, up to its last
        /// month of lifespan: the death of old age is left to the month run of the game that follows (a unit dying out of
        /// it stays the target of other units' AI: NullReferenceException in UnitActionSetPoint, the run never ends).
        /// </summary>
        public static void AgeUnits(int months)
        {
            if (months <= 0)
                return;
            foreach (var u in g.world.unit.GetUnits().ToArray())
            {
                if (u == null || u.isDie) continue;
                var age = u.GetProperty<int>(UnitPropertyEnum.Age);
                var aged = CatchUpAge(age, u.GetProperty<int>(UnitPropertyEnum.Life), months);
                if (aged != age) u.SetProperty<int>(UnitPropertyEnum.Age, aged);
            }
            DebugHelper.WriteLine($"TimeSkipEvent: units aged by {months} months");
        }

        /// <summary>Age (months) after a catch-up of the months skipped: at most life - 1; never lower than now.</summary>
        public static int CatchUpAge(int age, int life, int months) => Math.Max(age, Math.Min(age + Math.Max(0, months), life - 1));
        /// <summary>Runs the action once the save in progress (if any) is written.</summary>
        private void WhenSaved(Action act)
        {
            if (DataMgr.isSaveData)
                ModDelayEvent.Instance.DelayEvent(this, () => WhenSaved(act), 300);
            else
                act();
        }

        // ------------------------------------------------------------------ progress

        private UILoading _loading;

        /// <summary>The loading window of the game with "Time passes... n / N months (year, month)".</summary>
        private void ShowProgress()
        {
            try
            {
                var text = string.Format(GameTool.LS("skip_progress"), Done, Total, GameHelper.GetGameYear(), GameHelper.GetGameMonth());
                if (_loading == null || !g.ui.HasUI(UIType.Loading))
                    _loading = g.ui.OpenUISafe<UILoading>(UIType.Loading);
                _loading?.InitData(text);
                if (_loading?.textTip != null) _loading.textTip.text = text;
                if (_loading?.goTip != null) _loading.goTip.SetActive(true);
            }
            catch (Exception ex)
            {
                DebugHelper.WriteLine($"TimeSkipEvent.ShowProgress: {ex.Message}");
            }
        }

        private void HideProgress()
        {
            try { if (g.ui.HasUI(UIType.Loading)) g.ui.CloseUI(UIType.Loading); } catch { }
            _loading = null;
        }
    }
}
