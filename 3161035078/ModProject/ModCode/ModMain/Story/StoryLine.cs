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

        /// <summary>
        /// Background of a step's narration by its text key (pictures of docs/GameAssets/DramaBackgrounds, chosen to match
        /// the scene: a cave, a tomb, bandits, a storm, a lake...); BG for the others.
        /// </summary>
        public static readonly Dictionary<string, string> BACKGROUNDS = new Dictionary<string, string>
        {
            ["A0_3"] = "menshangzhizi1",
            ["A0_handed"] = "zhuanshenjianxue1",
            ["A1_2_search"] = "linzhongxiaodao",
            ["A1_late"] = "shandong",
            ["A2_2_market"] = "laoren",
            ["A2_2_robber"] = "shanzei2",
            ["A2_jailed"] = "wujinlangdao",
            ["A3_2"] = "heipaoansha1",
            ["A3_ledger"] = "baojuanziji",
            ["A4_3"] = "tianjianfadong",
            ["A5_lost"] = "faqigongji",
            ["endA_Sacrifice"] = "zhanshengwuluo",
            ["B0_2"] = "shanzei",
            ["B1_1"] = "laoren",
            ["B2_2"] = "shanzei",
            ["B2_vanished"] = "menshangzhizi1",
            ["B3_1"] = "xunwenshenminanzi",
            ["B4_1"] = "qunxingfumie1",
            ["B4_3"] = "faqigongji",
            ["endB_Betrayed"] = "menshangzhizi1",
            ["endB_FallenTown"] = "faqigongji",
            ["endB_Heir"] = "baojuanziji",
            ["C0_2"] = "baozha",
            ["C1_2"] = "mozhuiditu",
            ["C1_3"] = "bingyuan",
            ["C1_4"] = "lianhua2",
            ["C2_1"] = "tianqifu2",
            ["C3_exposed"] = "dalaoguangdian",
            ["C4_2"] = "heipaoansha1",
            ["C4_3_lost"] = "baozha",
            ["C4_3_won"] = "lingqixuanwo",
            ["endC_LastExplosion"] = "linjiantingtai",
            ["D1_2"] = "yaoshoudaoxia",
            ["D2_2"] = "yanxinbingkui",
            ["D3_2"] = "weizhijitan",
            ["D3_3"] = "heipaoansha1",
            ["E0_2"] = "yaoyuan",
            ["E1_1"] = "suirenyouyu",
            ["E2_8"] = "yaoyuan",
            ["E2_12"] = "suirenyouyu",
            ["E2_15"] = "lengyankanlai1",
            ["E3_1"] = "laoren",
            ["E3_2"] = "jiandaoshoushan",
            ["E4_1"] = "shuxiayuxi",
            ["E4_2"] = "baojuanziji",
            ["endE_LostChild"] = "zhuanshenjianxue1",
            ["endE_Successor"] = "shuxiayuxi",
            ["F_dead"] = "lianhua2",
            ["F_spread"] = "faqigongji",
            ["F1_1"] = "qunxingfumie1",
            ["F1_3"] = "qunxingfumie1",
            ["F3_1"] = "faqigongji",
            ["F3_3"] = "shoulingchuxian",
            ["F4_1"] = "lianhua2",
            ["F4_2"] = "mengjingguodu",
            ["F4_3"] = "gaoshanshnag",
            ["G1_3"] = "shanzei",
            ["G2_he"] = "dixiamushi",
            ["G5_2"] = "mingshan",
            ["endG_Grave"] = "shibei2",
            ["H2_1"] = "baozha",
            ["H4_2"] = "yuexiazhucha1",
            ["H4_gone"] = "shibei2",
            ["H5_1"] = "linjiantingtai",
            ["endH_Ashes"] = "shibei2",
            ["endH_InnOfAges"] = "linjiantingtai",
            ["I2_1"] = "youshendongxue",
            ["I2_2"] = "wujinlangdao",
            ["I2_3"] = "zongmenmijing1",
            ["I4_1"] = "shenguan",
            ["endI_FreedSoul"] = "lingqixuanwo",
            ["endI_Heir"] = "tianqifu2",
            ["endI_Looted"] = "zongmenbaozang1",
            ["endI_Sealer"] = "shenmizhenfa",
            ["J0_1"] = "zhanshengwuluo",
            ["J2_1"] = "xueguangchongxiao",
            ["J3_1"] = "heipaoansha1",
            ["endJ_Sealed"] = "linjiantingtai",
            ["endJ_SealedInn"] = "linjiantingtai",
            ["K0_1"] = "tianjianfadong",
            ["K1_1"] = "weizhijitan",
            ["K2_1"] = "tianqifu2",
            ["K3_1"] = "lingqixuanwo",
            ["K4_1"] = "tianjianfadong",
            ["K4_friend"] = "tianjianfadong",
            ["K4_rival"] = "tianjianfadong",
            ["endK_Devourer"] = "tianjianfadong",
            ["endK_Guided"] = "lingqixuanwo",
            ["endK_SecondChance"] = "mingchichenggong1",
            ["L0_1"] = "jinrushihai",
            ["L1_1"] = "lianhua2",
            ["L1_2"] = "lianhua2",
            ["L1_3"] = "lianhua2",
            ["L1_tomb"] = "shenguan",
            ["L2_1"] = "faqigongji",
            ["L3_1"] = "lianhua2",
            ["L4_1"] = "mingzhongkuangnu",
            ["endL_ClearHeart"] = "lianhua2",
            ["endL_Embraced"] = "mingzhongkuangnu",
            ["endL_Scarred"] = "mengjingguodu",
        };

        /// <summary>The background of a text key.</summary>
        public static string Bg(string key) => key != null && BACKGROUNDS.TryGetValue(key, out var bg) ? bg : BG;


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
            ShowFrames(s, key, args, unit, unit == null, Bg(key), Options(s, opts, args), Answer(s, pick));
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
            // the player chooses (options other than "Continue"): a narration frame lights the player's portrait
            var choice = !(options.Count == 1 && options[0] == cont[0]);
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
                    else DramaHelper.OpenDrama1(f.Text, opts, next, main, Player, last && choice ? 2 : 0);
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
            ShowFrames(s, key, args, null, true, background == BG ? Bg(key) : background, Options(s, opts, args), Answer(s, pick));
        }

        /// <summary>
        /// The player pays `price` spirit stones (an answer of a step): true when paid. Too few: false, a message box (the
        /// price and the stones owned), then the same step again (its choice is shown anew); the caller does nothing more.
        /// </summary>
        protected static bool Pay(StoryState s, int price)
        {
            if (Player.GetUnitMoney() >= price)
            {
                Player.AddUnitMoney(-price);
                return true;
            }
            Refuse(s, T(s, "no_money", price, Player.GetUnitMoney()));
            return false;
        }

        /// <summary>
        /// The player gives one unequipped item of the kind with a grade ≥ minGrade (an answer of a step): true when
        /// given. None: false, a message box (text story_no_&lt;what&gt;, {0} = minGrade), then the same step again.
        /// </summary>
        protected static bool Offer(StoryState s, Func<ConfItemPropsItem, bool> kind, int minGrade, string what)
        {
            if (TakeItem(kind, minGrade))
                return true;
            Refuse(s, T(s, "no_" + what, minGrade));
            return false;
        }

        /// <summary>
        /// A condition of the option chosen (an answer of a step): true when met. Not met: false, a message box (text
        /// story_&lt;key&gt; with args), then the choice again (retry; null: the same step); the caller does nothing more.
        /// </summary>
        protected static bool Require(StoryState s, bool ok, string key, Action retry, params object[] args)
        {
            if (!ok) Refuse(s, T(s, key, args), retry);
            return ok;
        }

        /// <summary>A condition of the option chosen is not met: a message box (title: the line), and once it is closed
        /// the choice again (retry; null: the same step). Opened after the drama answered has closed.</summary>
        private static void Refuse(StoryState s, string text, Action retry = null)
        {
            var title = T(s, "name_" + s.Id);
            StoryEvent.ShowLater(() => g.ui.MsgBox(title, text, MsgBoxButtonEnum.Ok, () => { if (retry != null) retry(); else Wait(s, 0); }));
        }

        /// <summary>
        /// The long lines (G-L): an act opens at the later of its year (years since the line opened) and its realm (grade),
        /// so a slow player meets it later and never skips it. Not yet: false, the step is checked again in 6 months.
        /// </summary>
        protected static bool Gate(StoryState s, int years, int grade)
        {
            if (StoryEvent.NoGates) return true;
            if (Now - s.Flag("opened") >= years * 12 && Player.GetGradeLvl() >= grade) return true;
            Wait(s, 6);
            return false;
        }

        /// <summary>Ending reached by another line (null when it has none): the lines change each other (06 section 5).</summary>
        public static string EndingOf(string line) => StoryEvent.Instance?.State(line)?.Ending;

        /// <summary>Counter flag + 1.</summary>
        protected static void Count(StoryState s, string flag) => s.Set(flag, s.Flag(flag) + 1);

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
            FamilyEvent.ChangeIntim(a, Player.GetUnitId(), value);
            var b = Player.data.unitData.relationData;
            FamilyEvent.ChangeIntim(b, u.GetUnitId(), value);
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
