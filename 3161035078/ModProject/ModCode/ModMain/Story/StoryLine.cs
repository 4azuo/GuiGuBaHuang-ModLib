using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// One storyline (docs/Taoist/Storylines/*.md). A line opens with a monthly chance when CanOpen, casts its roles,
    /// then runs its acts step by step: Run(state) is called when the step is due and the player is free (no drama,
    /// no battle); a step shows a drama and moves on (Next / Act / End) from the option chosen, starts a battle (Fight,
    /// result in OnBattle), or waits silently for a condition (returns without moving). Texts: story_localText.json,
    /// key "story_" + key; {p} = player name, {X} = name of the role X.
    /// </summary>
    public abstract class StoryLine
    {
        public abstract string Id { get; }

        /// <summary>Number of acts (0..Acts-1; fail branches above): the progress of the quest of the line.</summary>
        public virtual int Acts => 5;
        /// <summary>Chance (%) a month to open when CanOpen.</summary>
        public virtual float MonthlyChance => 0.3f;
        /// <summary>Roles cast when the line opens.</summary>
        public abstract StoryRole[] Roles { get; }
        public virtual bool CanOpen(StoryState s) => true;
        /// <summary>After the roles are cast: places ({S}, {T}...) and roles derived from them.</summary>
        public virtual void OnCast(StoryState s) { }
        /// <summary>Runs the current step.</summary>
        public abstract void Run(StoryState s);
        /// <summary>Time limit of an act in months (0: none).</summary>
        public virtual int TimeLimit(int act) => 0;
        /// <summary>The act is late (once per act).</summary>
        public virtual void OnLate(StoryState s) { }
        /// <summary>A battle of the line ended: flag tag = 1 won / -1 lost, next step.</summary>
        public virtual void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            Next(s);
        }
        /// <summary>A cast NPC died: the line closes (lines override for their own endings).</summary>
        public virtual void OnRoleDead(StoryState s, string role) => Close(s, "dead");
        /// <summary>Yearly perks of the ending (after the line closed with an ending).</summary>
        public virtual void Yearly(StoryState s) { }
        /// <summary>Roles whose death matters (default: all the roles).</summary>
        public virtual IEnumerable<string> VitalRoles => Roles.Select(x => x.Name);

        // ------------------------------------------------------------------ helpers

        protected static WorldUnitBase Player => g.world.playerUnit;
        protected static int Now => GameHelper.GetGameTotalMonth();
        public const string BG = "xinjianhualuo1";

        public static WorldUnitBase U(StoryState s, string role) =>
            s.Roles.TryGetValue(role, out var id) ? FamilyEvent.Unit(id) : null;

        public static string Name(StoryState s, string role) => U(s, role)?.GetName() ?? "?";

        /// <summary>Text of a key (story_ + key) with {p} and the role names, then {0}.. args.</summary>
        public static string T(StoryState s, string key, params object[] args) => Fill(s, GameTool.LS("story_" + key) ?? key, args);

        /// <summary>{p}, the role names and the places of a text, then {0}.. args.</summary>
        public static string Fill(StoryState s, string text, params object[] args)
        {
            text = text.Replace("{p}", Player.GetName());
            foreach (var r in s.Roles.Keys.Where(x => !Places.ContainsKey(x)))
                text = text.Replace("{" + r + "}", Name(s, r));
            foreach (var kv in Places)
                text = text.Replace("{" + kv.Key + "}", kv.Value(s));
            // A Li before she is an NPC (a child record)
            if (text.Contains("{Y}")) text = text.Replace("{Y}", GameTool.LS("story_name_Y"));
            return args.Length > 0 ? string.Format(text, args) : text;
        }

        /// <summary>Place tokens ({S} sect, {T} / {T2} towns) by line.</summary>
        protected static readonly Dictionary<string, Func<StoryState, string>> Places = new Dictionary<string, Func<StoryState, string>>
        {
            ["S"] = s => s.Roles.TryGetValue("S", out var id) ? (g.world.build.GetBuild<MapBuildSchool>(id)?.name ?? "?") : "?",
            ["T"] = s => s.Roles.TryGetValue("T", out var id) ? (g.world.build.GetBuild(id)?.name ?? "?") : "?",
            ["T2"] = s => s.Roles.TryGetValue("T2", out var id) ? (g.world.build.GetBuild(id)?.name ?? "?") : "?",
        };

        /// <summary>A role speaks (portrait of the role and of the player). opts: text keys; pick(index from 0).
        /// The text is shown frame by frame (Frames): narration, then every speaker with their portrait lit.</summary>
        protected static void Say(StoryState s, string role, string key, string[] opts, Action<int> pick, params object[] args)
        {
            var unit = U(s, role);
            StoryEvent.Log(s, key);
            ShowFrames(s, key, args, unit, unit == null, BG, Options(s, opts, args), Answer(s, pick));
        }

        private static List<string> Options(StoryState s, string[] opts, object[] args) => (opts ?? new[] { "continue" }).Select(o => T(s, o, args)).ToList();

        private static Action<ConfDramaOptionsItem> Answer(StoryState s, Action<int> pick) =>
            x => { try { pick?.Invoke(x.id % 10); } catch (Exception ex) { DebugHelper.WriteLine($"Story {s.Id}{s.Act}.{s.Step}: {ex}"); } };

        /// <summary>One frame of a drama: who speaks (null: narration) and the text.</summary>
        public class Frame
        {
            public WorldUnitBase Who;
            public string Text;
        }

        /// <summary>"Speaker: "words"" (any language: : or ：, then a quote " “ 「 « „ ‘).</summary>
        private static readonly Regex SPEECH = new Regex("^\\s*([^:：\"“「«„]{1,40}?)\\s*[:：]\\s*([\"“「«„‘].*)$");
        private static readonly Regex TOKEN = new Regex("\\{([A-Za-z][A-Za-z0-9]*)\\}");
        private static readonly char[] QUOTES = { '"', '“', '「', '«', '„', '‘' };

        /// <summary>
        /// The frames of a text: narration lines together; a line "{X}: "words"" is a frame of the role X (the name is
        /// on the nameplate: removed), "{X} does something: "words"" a frame of X with the whole line, "A disciple: "..."" a
        /// frame of a third person (Extra: an NPC of the sect / town / area, the label removed); a narration starting
        /// with a quote is said by `speaker` (the role of Say).
        /// </summary>
        public static List<Frame> Frames(StoryState s, string key, object[] args, WorldUnitBase speaker)
        {
            var raw = GameTool.LS("story_" + key) ?? key;
            var frames = new List<Frame>();
            var narration = new List<string>();
            void Flush()
            {
                if (narration.Count == 0) return;
                var text = string.Join("\n", narration);
                frames.Add(new Frame { Who = speaker != null && text.TrimStart().IndexOfAny(QUOTES) == 0 ? speaker : null, Text = Fill(s, text, args) });
                narration.Clear();
            }
            foreach (var line in raw.Split('\n'))
            {
                var m = SPEECH.Match(line);
                if (!m.Success) { narration.Add(line); continue; }
                var label = m.Groups[1].Value.Trim();
                var tk = TOKEN.Match(label);
                WorldUnitBase who;
                string text;
                if (tk.Success)
                {
                    var r = tk.Groups[1].Value;
                    who = r == "p" ? Player : U(s, r);
                    text = tk.Value == label ? m.Groups[2].Value : line;
                }
                else
                {
                    who = Extra(s, key);
                    text = m.Groups[2].Value;
                }
                if (who == null) { narration.Add(line); continue; }
                Flush();
                frames.Add(new Frame { Who = who, Text = Fill(s, text, args) });
            }
            Flush();
            if (frames.Count == 0) frames.Add(new Frame { Text = string.Empty });
            return frames;
        }

        private static readonly Dictionary<string, string> _extras = new Dictionary<string, string>();

        /// <summary>A third person of a drama ("A disciple", "The innkeeper"...): a member of the sect of the line, else
        /// an NPC of the player's town, else of the area; the same NPC for the whole drama.</summary>
        public static WorldUnitBase Extra(StoryState s, string key)
        {
            var k = s.Id + "/" + key;
            if (_extras.TryGetValue(k, out var known))
            {
                var u0 = FamilyEvent.Unit(known);
                if (u0 != null && !u0.isDie) return u0;
            }
            var roles = new HashSet<string>(s.Roles.Values);
            var pool = StoryEvent.Units().Where(u => !roles.Contains(u.GetUnitId())).ToList();
            var pick = new List<WorldUnitBase>();
            if (s.Roles.TryGetValue("S", out var sect)) pick = pool.Where(u => u.data.unitData.schoolID == sect).ToList();
            var town = PlayerTown();
            if (pick.Count == 0 && town != null) pick = pool.Where(u => u.GetMapBuild<MapBuildTown>()?.buildData.id == town.buildData.id).ToList();
            if (pick.Count == 0) { var area = Player.GetUnitPosAreaId(); pick = pool.Where(u => u.GetUnitPosAreaId() == area).ToList(); }
            if (pick.Count == 0) pick = pool;
            if (pick.Count == 0) return null;
            var unit = pick[CommonTool.Random(0, pick.Count * 100) % pick.Count];
            _extras[k] = unit.GetUnitId();
            return unit;
        }

        /// <summary>
        /// Shows the frames one after the other ("Continue"; the last one has the options and the answer): a speaker is
        /// lit on the left (the player on the right); narration shows `main` and the player unlit, or the background.
        /// </summary>
        private static void ShowFrames(StoryState s, string key, object[] args, WorldUnitBase main, bool narrationOnBackground, string background, List<string> options, Action<ConfDramaOptionsItem> answer)
        {
            var frames = Frames(s, key, args, narrationOnBackground ? null : main);
            var cont = new List<string> { T(s, "continue") };
            void Open(int i)
            {
                var f = frames[i];
                var last = i == frames.Count - 1;
                var opts = last ? options : cont;
                Action<ConfDramaOptionsItem> next = last ? answer : (_ => Show(() => Open(i + 1)));
                var other = main ?? frames.Select(x => x.Who).FirstOrDefault(x => x != null && !x.IsPlayer());
                if (f.Who == null)
                {
                    if (narrationOnBackground || main == null) DramaHelper.OpenDrama2(f.Text, opts, next, background);
                    else DramaHelper.OpenDrama1(f.Text, opts, next, main, Player, 0);
                }
                else if (f.Who.IsPlayer())
                {
                    if (other == null) DramaHelper.OpenDrama2(f.Text, opts, next, background);
                    else DramaHelper.OpenDrama1(f.Text, opts, next, other, Player, 2);
                }
                else DramaHelper.OpenDrama1(f.Text, opts, next, f.Who, Player, 1);
            }
            Show(() => Open(0));
        }

        /// <summary>Opens a drama now, or after the drama being answered has closed (StoryEvent.ShowLater).</summary>
        private static void Show(Action open)
        {
            if (StoryEvent.Busy() || StoryEvent.Pending) StoryEvent.ShowLater(open);
            else open();
        }

        /// <summary>Narration over a background.</summary>
        protected static void Tell(StoryState s, string key, string[] opts, Action<int> pick, string background = BG, params object[] args)
        {
            StoryEvent.Log(s, key);
            ShowFrames(s, key, args, null, true, background, Options(s, opts, args), Answer(s, pick));
        }

        /// <summary>Next step, after `months` (0: as soon as the player is free).</summary>
        public static void Next(StoryState s, int months = 0)
        {
            s.Step++;
            Wait(s, months);
        }

        public static void Wait(StoryState s, int months)
        {
            s.NextMonth = Now + Math.Max(0, months);
            s.Soon = months <= 0;
        }

        /// <summary>Goes to an act (step 1), after `months`; its time limit counts from then.</summary>
        public static void Act(StoryState s, int act, int months = 0)
        {
            s.Act = act;
            s.Step = 1;
            s.ActStart = Now + Math.Max(0, months);
            Wait(s, months);
        }

        /// <summary>The line ends with an ending (yearly perks follow in Yearly).</summary>
        public static void End(StoryState s, string ending)
        {
            s.Ending = ending;
            s.Closed = true;
            s.Open = false;
            s.LastPerkMonth = Now;
            TaoistLogEvent.Log("log_story_end", T(s, "name_" + s.Id), T(s, "end_" + ending));
            DebugHelper.WriteLine($"Story {s.Id}: ending {ending}");
        }

        /// <summary>The line closes without an ending (refused, a role died).</summary>
        public static void Close(StoryState s, string why)
        {
            s.Closed = true;
            s.Open = false;
            DebugHelper.WriteLine($"Story {s.Id}: closed ({why})");
        }

        /// <summary>Dungeon of a plain NPC fight on the map.</summary>
        public const int NPC_FIGHT_DUNGEON_ID = 11;

        /// <summary>
        /// A battle with an NPC (moved next to the player); result in OnBattle(tag). real: the NPC may die; never the
        /// player (a story battle lost is a setback, not a death). spar: nobody dies.
        /// </summary>
        protected static void Fight(StoryState s, WorldUnitBase enemy, string tag, bool real = true)
        {
            if (enemy == null) { StoryEvent.Instance.BattleEnded(s, tag, true); return; }
            s.Battle = tag;
            if (StoryEvent.NoBattles) { StoryEvent.Instance.BattleEnded(s, tag, true); return; }
            enemy.SetUnitPos(Player.GetUnitPos());
            g.world.battle.IntoBattle(enemy, NPC_FIGHT_DUNGEON_ID, null, new WorldBattleData
            {
                npcUnit = enemy,
                isRealBattle = real,
                npcWinNotKillPlayerUnit = true,
                npcWinInsultPlayerUnit = false,
            });
        }

        /// <summary>A sparring battle (nobody dies).</summary>
        protected static void Spar(StoryState s, WorldUnitBase enemy, string tag) => Fight(s, enemy, tag, false);

        /// <summary>A battle with a beast (single-boss dungeon, level by the player's grade).</summary>
        protected static void FightBeast(StoryState s, string tag, int extraLevel = 0)
        {
            s.Battle = tag;
            if (StoryEvent.NoBattles) { StoryEvent.Instance.BattleEnded(s, tag, true); return; }
            g.world.battle.IntoBattle(new DataMap.MonstData { id = StoryEvent.BEAST_DUNGEON_ID, level = Math.Max(1, Player.GetGradeLvl() * 5 + extraLevel) });
        }

        /// <summary>A temporary enemy NPC near the player's grade (righteous / demonic / any), created when none.</summary>
        protected static WorldUnitBase Foe(StoryState s, bool? righteous = null, int gradeOffset = 0, string sect = null)
        {
            var role = new StoryRole
            {
                Name = "foe",
                Righteous = righteous,
                MinGradeOffset = gradeOffset - 1,
                MaxGradeOffset = gradeOffset + 1,
                MinLived = 0.1f,
                MaxLived = 0.9f,
                Filter = sect == null ? (Func<WorldUnitBase, bool>)null : u => u.data.unitData.schoolID == sect,
            };
            var taken = StoryEvent.Instance.TakenUnits();
            return StoryCasting.Pick(role, taken, StoryEvent.Units(), Player) ?? StoryCasting.Create(role, Player);
        }

        public static int Intim(StoryState s, string role)
        {
            var u = U(s, role);
            return u == null ? 0 : u.data.unitData.relationData.GetIntim(Player.GetUnitId());
        }

        /// <summary>Intimacy between the role and the player, both ways.</summary>
        public static void AddIntim(StoryState s, string role, int value)
        {
            var u = U(s, role);
            if (u == null) return;
            var a = u.data.unitData.relationData;
            a.SetIntim(Player.GetUnitId(), a.GetIntim(Player.GetUnitId()) + value);
            var b = Player.data.unitData.relationData;
            b.SetIntim(u.GetUnitId(), b.GetIntim(u.GetUnitId()) + value);
        }

        /// <summary>Is the player in a town (any, or this one)?</summary>
        protected static MapBuildTown PlayerTown() => Player.GetMapBuild<MapBuildTown>();

        /// <summary>Gives an item of a kind at a grade (EncounterEvent.PickItem); its name.</summary>
        protected static string Give(Func<ConfItemPropsItem, bool> kind, int grade) => EncounterEvent.GiveItem(kind, grade);

        /// <summary>Takes one unequipped item of the kind with a grade ≥ minGrade; false when none.</summary>
        protected static bool TakeItem(Func<ConfItemPropsItem, bool> kind, int minGrade)
        {
            var p = Player.GetUnequippedProps().FirstOrDefault(x => x.propsItem != null && kind(x.propsItem) && x.propsItem.Grade() >= minGrade);
            if (p == null) return false;
            Player.RemoveUnitProp(p.soleID, 1);
            return true;
        }
    }
}
