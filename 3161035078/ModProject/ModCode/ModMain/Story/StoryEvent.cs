using EBattleTypeData;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using EGameTypeData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// NPC storylines (docs/Taoist/06-Storylines-Draft.md, dialogue in docs/Taoist/Storylines/):
    /// - monthly: a line not opened yet opens with its chance when its conditions are met (one a month), casting its
    ///   roles (the best fitting NPC; none: it waits, and a fitting NPC is created with a chance growing every year);
    /// - the due step of an open line runs when the player is free (on the map, no drama, no battle): at the month tick,
    ///   or within a second for the steps that follow at once ("Soon");
    /// - time limits of the acts (OnLate once per act), deaths of the roles (OnRoleDead), battles (OnBattle);
    /// - yearly perks of the endings.
    /// Off with S&amp;M "No NPC storylines".
    /// </summary>
    [Cache(ModConst.STORY_EVENT)]
    public class StoryEvent : ModEvent
    {
        public static StoryEvent Instance { get; set; }

        /// <summary>Single-boss dungeon of the beast battles (level by the player's grade).</summary>
        public const int BEAST_DUNGEON_ID = 9031;

        /// <summary>The lines, in the order they are checked.</summary>
        public static readonly List<StoryLine> LINES = new List<StoryLine>
        {
            new LineA_ColdSword(), new LineB_OldHuangDebt(), new LineC_ExplodingFurnace(),
            new LineD_SwornBrothers(), new LineE_HerbGarden(), new LineF_RedLotus(),
        };

        public Dictionary<string, StoryState> Lines { get; set; } = new Dictionary<string, StoryState>();

        /// <summary>Tests only: battles are won at once (no battle scene).</summary>
        [JsonIgnore]
        public static bool NoBattles { get; set; }

        /// <summary>Autotests: no line opens by itself (the story tests open theirs; other tests are not interrupted).</summary>
        [JsonIgnore]
        public static bool NoAutoOpen { get; set; }

        public static bool Disabled => SMLocalConfigsEvent.Instance?.Configs?.NoStory ?? false;
        private static WorldUnitBase Player => g.world.playerUnit;
        private static int Now => GameHelper.GetGameTotalMonth();

        public static StoryLine Line(string id) => LINES.FirstOrDefault(x => x.Id == id);

        public StoryState State(string id)
        {
            if (!Lines.TryGetValue(id, out var s))
                Lines[id] = s = new StoryState { Id = id };
            return s;
        }

        public static IEnumerable<WorldUnitBase> Units() =>
            (ModMain.Store?.WUnits ?? new WorldUnitBase[0]).Where(x => x != null && !x.isDie && !x.IsPlayer());

        /// <summary>Units holding a role in any line.</summary>
        public HashSet<string> TakenUnits() => new HashSet<string>(Lines.Values.SelectMany(x => x.Roles.Values));

        /// <summary>Busy: a drama, a battle, not on the map (the steps wait).</summary>
        public static bool Busy() =>
            GameHelper.IsInBattlle() || !g.ui.HasUI(UIType.MapMain) ||
            new[] { UIType.DramaDialogue, UIType.DramaFortuitous, UIType.DramaDialogueBigTexture, UIType.DramaBigTexture }.Any(t => g.ui.HasUI(t));

        // ------------------------------------------------------------------ opening

        /// <summary>
        /// Casts the roles of a line: all of them or none. Missing role: the line waits (WaitingSince) and, every year
        /// of waiting, creates the NPC with StoryCasting.CreateChance. True when cast.
        /// </summary>
        public bool Cast(StoryLine line, StoryState s, bool forceCreate = false)
        {
            var taken = TakenUnits();
            var picked = new Dictionary<string, WorldUnitBase>();
            var years = s.WaitingSince < 0 ? 0 : (Now - s.WaitingSince) / 12;
            foreach (var role in line.Roles)
            {
                var u = StoryCasting.Pick(role, taken, Units(), Player);
                if (u == null && (forceCreate || (years > 0 && CommonTool.Random(0f, 100f) < StoryCasting.CreateChance(years))))
                    u = StoryCasting.Create(role, Player);
                if (u == null)
                {
                    if (s.WaitingSince < 0) s.WaitingSince = Now;
                    return false;
                }
                picked[role.Name] = u;
                taken.Add(u.GetUnitId());
            }
            foreach (var kv in picked) s.Roles[kv.Key] = kv.Value.GetUnitId();
            s.WaitingSince = -1;
            return true;
        }

        /// <summary>Opens a line now (tests, or the monthly chance): casts, act 0 step 1. False when it cannot.</summary>
        public bool Open(string id, bool forceCreate = false)
        {
            var line = Line(id);
            var s = State(id);
            if (line == null || s.Open || s.Closed || !Cast(line, s, forceCreate))
                return false;
            s.Open = true;
            line.OnCast(s);
            StoryLine.Act(s, 0);
            DebugHelper.WriteLine($"Story {id}: opened ({string.Join(", ", s.Roles.Select(x => $"{x.Key}={x.Value}"))})");
            return true;
        }

        // ------------------------------------------------------------------ ticks

        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnMonthly()
        {
            base.OnMonthly();
            if (Disabled) return;
            // one new line a month at most
            foreach (var line in NoAutoOpen ? new List<StoryLine>() : LINES)
            {
                var s = State(line.Id);
                if (s.Open || s.Closed) continue;
                if (!line.CanOpen(s) || CommonTool.Random(0f, 100f) >= ModLib.Mod.ModMaster.ChanceOver(line.MonthlyChance, ModLib.Mod.ModMaster.MonthsPassed)) continue;
                if (Open(line.Id)) break;
            }
            foreach (var line in LINES)
            {
                var s = State(line.Id);
                if (!s.Open) continue;
                if (CheckRoles(line, s)) continue;
                var limit = line.TimeLimit(s.Act);
                if (limit > 0 && Now - s.ActStart > limit && !s.Has("late" + s.Act))
                {
                    s.Set("late" + s.Act);
                    DebugHelper.WriteLine($"Story {s.Id}: act {s.Act} late");
                    line.OnLate(s);
                }
            }
            LineE_HerbGarden.GrowUpALi(State("E"));
            if (ModLib.Mod.ModMaster.YearsPassed > 0) LineF_RedLotus.Spread(State("F"));
            // yearly perks of the endings
            foreach (var line in LINES)
            {
                var s = State(line.Id);
                if (s.Ending == null || Now - s.LastPerkMonth < 12) continue;
                s.LastPerkMonth = Now;
                try { line.Yearly(s); } catch (Exception ex) { DebugHelper.WriteLine($"Story {s.Id}.Yearly: {ex}"); }
            }
            RunDue(false);
        }

        /// <summary>A vital role died: OnRoleDead. True when the line changed.</summary>
        private bool CheckRoles(StoryLine line, StoryState s)
        {
            foreach (var role in line.VitalRoles)
            {
                if (!s.Roles.TryGetValue(role, out var id)) continue;
                var u = FamilyEvent.Unit(id);
                if (u != null && !u.isDie) continue;
                if (s.Has("dead_" + role)) continue;
                s.Set("dead_" + role);
                DebugHelper.WriteLine($"Story {s.Id}: role {role} died");
                line.OnRoleDead(s, role);
                return true;
            }
            return false;
        }

        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnTimeUpdate1000ms()
        {
            base.OnTimeUpdate1000ms();
            UpdateGarden();
            UpdateTasks();
            if (!Disabled) RunDue(true);
        }

        private static readonly Queue<Action> _later = new Queue<Action>();
        private static int _freeTicks;

        /// <summary>A drama is waiting to be shown (opened from the answer of another one).</summary>
        public static bool Pending => _later.Count > 0;

        /// <summary>
        /// A drama opened from the answer of another drama is shown once that one has closed: opened at once, the game
        /// drops it while closing the first (the line would then repeat its step).
        /// </summary>
        public static void ShowLater(Action open) => _later.Enqueue(open);

        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            if (_later.Count == 0) return;
            // free for 2 ticks: the drama answered has really closed
            if (Busy()) { _freeTicks = 0; return; }
            if (++_freeTicks < 2) return;
            _freeTicks = 0;
            var open = _later.Dequeue();
            try { open(); } catch (Exception ex) { DebugHelper.WriteLine($"Story drama: {ex}"); }
        }

        /// <summary>Runs one due step (soon: only the "Soon" steps) when the player is free.</summary>
        public void RunDue(bool soonOnly)
        {
            if (Busy() || Pending) return;
            foreach (var line in LINES)
            {
                var s = State(line.Id);
                if (!s.Open) continue;
                // deaths are seen at once (not only at the month tick): a waiting step may need one
                if (CheckRoles(line, s)) return;
                if (s.Battle != null || s.NextMonth > Now || (soonOnly && !s.Soon)) continue;
                var key = $"{s.Act}.{s.Step}";
                try { line.Run(s); }
                catch (Exception ex) { DebugHelper.WriteLine($"Story {s.Id}{key}: {ex}"); s.Soon = false; }
                // a waiting step (nothing changed) is checked again at the next month tick only
                if ($"{s.Act}.{s.Step}" == key && !Busy()) s.Soon = false;
                return;
            }
        }

        // ------------------------------------------------------------------ battles

        public override void OnBattleEndHandler(BattleEndHandler e)
        {
            base.OnBattleEndHandler(e);
            foreach (var s in Lines.Values.Where(x => x.Battle != null).ToList())
                BattleEnded(s, s.Battle, e.isWin);
        }

        public void BattleEnded(StoryState s, string tag, bool won)
        {
            s.Battle = null;
            DebugHelper.WriteLine($"Story {s.Id}: battle {tag} {(won ? "won" : "lost")}");
            try { Line(s.Id)?.OnBattle(s, tag, won); } catch (Exception ex) { DebugHelper.WriteLine($"Story {s.Id}.OnBattle: {ex}"); }
        }

        // ------------------------------------------------------------------ garden seclusion (line E)

        /// <summary>Quest "Granny Shi's garden" (story_taskBase.json, type 104 never counted) and its map position
        /// (story_mapPosition.json, type 3 "x_y": the game's quest guide leads there).</summary>
        public const int GARDEN_TASK_ID = 444447001;
        public const int GARDEN_POSITION_ID = 444447001;

        /// <summary>The garden on the world map: a fruit tree icon (story_worldFortuitousEventBase.json, no drama of its
        /// own: standing there opens the garden window).</summary>
        public const int GARDEN_EVENT_ID = 444447001;

        /// <summary>The garden on the world map: where Granny Shi met the player (E0, outside towns); fixed for the game.</summary>
        public static UnityEngine.Vector2Int? GardenPoint(StoryState s)
        {
            if (s == null || !s.Has("gx")) return null;
            return new UnityEngine.Vector2Int(s.Flag("gx") - 1, s.Flag("gy") - 1);
        }

        /// <summary>Stores the garden point (once).</summary>
        public static void SetGardenPoint(StoryState s, UnityEngine.Vector2Int p)
        {
            if (s.Has("gx")) return;
            s.Set("gx", p.x + 1); // +1: a flag of 0 reads as "not set"
            s.Set("gy", p.y + 1);
        }

        /// <summary>The player stands on the garden (or next to it).</summary>
        public static bool AtGarden(StoryState s)
        {
            var p = GardenPoint(s);
            if (p == null || Player == null) return false;
            var q = Player.GetUnitPos();
            return System.Math.Abs(q.x - p.Value.x) <= 1 && System.Math.Abs(q.y - p.Value.y) <= 1;
        }

        private bool _gardenShown;

        /// <summary>Every second: the garden quest while the garden is open; the garden window when the player arrives
        /// there (once a visit: leave and come back to open it again).</summary>
        public void UpdateGarden()
        {
            var s = State("E");
            var open = !Disabled && LineE_HerbGarden.GardenOpen(s) && GardenPoint(s) != null;
            var task = Player?.GetTask(GARDEN_TASK_ID)?.ToArray().FirstOrDefault();
            UpdateGardenMarker(s, open);
            if (!open)
            {
                if (task != null) Player.DelTask(task);
                _gardenShown = false;
                return;
            }
            var p = GardenPoint(s).Value;
            var pos = g.conf.mapPosition.GetItem(GARDEN_POSITION_ID);
            var posi = $"{p.x}_{p.y}";
            if (pos != null && pos.posiID != posi) pos.posiID = posi;
            if (task == null)
                Player.CreateTask(new DataUnit.TaskData { id = GARDEN_TASK_ID, soleID = CommonTool.SoleID(), state = TaskState.Unfinished, curCount = 0 });
            if (!AtGarden(s)) { _gardenShown = false; return; }
            if (_gardenShown || SeclusionEvent.IsSecluded || Busy()) return;
            _gardenShown = true;
            OpenGarden();
        }

        private bool _markerFailed;

        /// <summary>The fruit tree of the garden on the world map while the garden is open (map events are saved with
        /// the game: created once, removed when the garden closes).</summary>
        private void UpdateGardenMarker(StoryState s, bool open)
        {
            var events = g.world.mapEvent.GetGridEvent(GARDEN_EVENT_ID);
            var has = events != null && events.Count > 0;
            if (open && !has && !_markerFailed)
            {
                var p = GardenPoint(s).Value;
                var rs = g.world.mapEvent.AddGridEvent(p, GARDEN_EVENT_ID, null, false, false);
                var ok = g.world.mapEvent.GetGridEvent(GARDEN_EVENT_ID)?.Count > 0;
                if (!ok) { _markerFailed = true; DebugHelper.WriteLine($"Story garden: no map icon at {p} (grid taken?)"); }
            }
            else if (!open && has)
            {
                foreach (var e in events.ToArray())
                    g.world.mapEvent.DelGridEvent(e, true);
            }
        }

        /// <summary>The garden icon on the world map (tests).</summary>
        public static bool GardenMarkerShown => g.world.mapEvent.GetGridEvent(GARDEN_EVENT_ID)?.Count > 0;

        // ------------------------------------------------------------------ quests of the lines

        /// <summary>Quests of the open lines (story_taskBase.json LINE_TASK_ID + 1..6 for A-F, type 104 never counted
        /// by the game): name = the line, progress = the act, description = the guide of the act (story_task_&lt;line&gt;&lt;act&gt;
        /// with names and places), rewritten in the text story_task_desc_&lt;line&gt; (all languages of the game).</summary>
        public const int LINE_TASK_ID = 444447010;

        public static int TaskId(string line) => LINE_TASK_ID + 1 + LINES.FindIndex(l => l.Id == line);

        public void UpdateTasks()
        {
            if (Player == null) return;
            foreach (var line in LINES)
            {
                var s = State(line.Id);
                var id = TaskId(line.Id);
                var task = Player.GetTask(id)?.ToArray().FirstOrDefault();
                if (Disabled || !s.Open)
                {
                    if (task != null) Player.DelTask(task);
                    continue;
                }
                if (task == null)
                    task = Player.CreateTask(new DataUnit.TaskData { id = id, soleID = CommonTool.SoleID(), state = TaskState.Unfinished, curCount = 0 });
                var act = Math.Min(s.Act, line.Acts - 1);
                if (task?.taskData != null && task.taskData.curCount != act) task.taskData.curCount = act;
                // roles not cast yet: "?"
                SetText("story_task_desc_" + line.Id, System.Text.RegularExpressions.Regex.Replace(StoryLine.T(s, $"task_{line.Id}{s.Act}"), @"\{[A-Za-z][A-Za-z0-9]*\}", "?"));
            }
        }

        /// <summary>The text of a key in every language of the game (the key exists: story_localText.json).</summary>
        private static void SetText(string key, string text)
        {
            if (!g.conf.localText.allText.ContainsKey(key)) return;
            var t = g.conf.localText.allText[key];
            if (t.en == text && t.ch == text) return;
            t.en = text; t.ch = text; t.tc = text; t.kr = text;
        }

        public static readonly int[] GARDEN_YEARS = { 1, 3, 5, 10, 20 };

        /// <summary>Cost of a garden seclusion: spirit stones by year (inflation).</summary>
        public static int GardenCost(int years) => EncounterEvent.Cost(5 * years);

        /// <summary>Choose the years (1-20) of a garden seclusion, paid in spirit stones.</summary>
        public static void OpenGarden()
        {
            var s = Instance.State("E");
            var opts = GARDEN_YEARS.Select(y => StoryLine.T(s, "garden_o", y, GardenCost(y))).ToList();
            opts.Add(StoryLine.T(s, "garden_no"));
            DramaHelper.OpenDrama2(StoryLine.T(s, "garden"), opts, x =>
            {
                var i = x.id % 10;
                if (i < 0 || i >= GARDEN_YEARS.Length) return;
                var years = GARDEN_YEARS[i];
                if (Player.GetUnitMoney() < GardenCost(years)) { ShowLater(() => DramaHelper.OpenDrama2(StoryLine.T(s, "garden_money"), new List<string> { StoryLine.T(s, "continue") }, null, StoryLine.BG)); return; }
                Player.AddUnitMoney(-GardenCost(years));
                SeclusionEvent.Instance.Start(SeclusionEvent.Kind.Garden, years * 12);
            }, StoryLine.BG);
        }

        // ------------------------------------------------------------------ log

        /// <summary>Story page of the Taoist log: the step texts shown (by key).</summary>
        public static void Log(StoryState s, string key)
        {
            s.Set("seen_" + key);
        }
    }
}
