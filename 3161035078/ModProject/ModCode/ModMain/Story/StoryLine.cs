using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static string T(StoryState s, string key, params object[] args)
        {
            var text = GameTool.LS("story_" + key) ?? key;
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

        /// <summary>A role speaks (portrait of the role and of the player). opts: text keys; pick(index from 0).</summary>
        protected static void Say(StoryState s, string role, string key, string[] opts, Action<int> pick, params object[] args)
        {
            var unit = U(s, role);
            var options = (opts ?? new[] { "continue" }).Select(o => T(s, o, args)).ToList();
            Action<ConfDramaOptionsItem> cb = x => { try { pick?.Invoke(x.id % 10); } catch (Exception ex) { DebugHelper.WriteLine($"Story {s.Id}{s.Act}.{s.Step}: {ex}"); } };
            StoryEvent.Log(s, key);
            var text = T(s, key, args);
            Show(() =>
            {
                if (unit == null)
                    DramaHelper.OpenDrama2(text, options, cb, BG);
                else
                    DramaHelper.OpenDrama1(text, options, cb, unit, Player, 1); // the NPC speaks: their portrait lit
            });
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
            var options = (opts ?? new[] { "continue" }).Select(o => T(s, o, args)).ToList();
            StoryEvent.Log(s, key);
            var text = T(s, key, args);
            Show(() => DramaHelper.OpenDrama2(text, options, x => { try { pick?.Invoke(x.id % 10); } catch (Exception ex) { DebugHelper.WriteLine($"Story {s.Id}{s.Act}.{s.Step}: {ex}"); } }, background));
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
