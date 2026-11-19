using EBattleTypeData;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Families (the game already marries NPCs of opposite sexes now and then, it has no births):
    /// - Dao companions (yearly): single NPCs of 18+ years with 20+ years of lifespan left marry with a chance that
    ///   grows from 18 to 30 years and falls with the share of the lifespan already lived. The partner: same area,
    ///   single, not of the same blood (3 generations), grades at most 2 apart, the highest intimacy / same alignment
    ///   first; opposite sex unless S&M "Same-sex dao companions" is on.
    /// - Children (yearly): a couple of opposite sexes (the player and the companion included) has a child with a chance
    ///   that falls with age and with grade (high cultivators rarely have children); at most MAX_CHILDREN each.
    ///   The child is only a record until 16 years later (a cultivator starts at 16, the game has adult portraits only):
    ///   then a grade-1 NPC is created next to the parents, 16 years old, with the surname of the father / mother /
    ///   either (S&M), the talent and the alignment of the parents, linked as their child.
    /// - Clans: a unit killed by the player: its relatives who are town masters put a bounty on the player.
    /// - "Family" button in NPC info: parents, companion, children (also the ones not grown up yet), siblings.
    /// Off with S&M "No family system".
    /// </summary>
    [Cache(ModConst.FAMILY_EVENT)]
    public class FamilyEvent : ModEvent
    {
        public static FamilyEvent Instance { get; set; }

        public const int ADULT_YEARS = 16;
        public const int MARRY_MIN_YEARS = 18;
        public const int MARRY_MIN_LIFE_LEFT_YEARS = 20;
        public const float MARRY_RATE = 8f;          // % a year at the peak
        public const float BIRTH_RATE = 6f;          // % a year for a young couple of grade 1
        public const int MAX_CHILDREN = 4;
        public const int MAX_MARRIAGES_A_YEAR = 30;
        public const int MAX_BIRTHS_A_YEAR = 20;
        public const int MAX_PENDING = 300;

        public enum Surname { Father, Mother, Random }

        public class Child
        {
            public string Id { get; set; } = Guid.NewGuid().ToString("N");
            public string FatherId { get; set; }
            public string MotherId { get; set; }
            public string FatherName { get; set; }
            public string MotherName { get; set; }
            public int BirthMonth { get; set; }
            /// <summary>Unit id once grown up (null before).</summary>
            public string UnitId { get; set; }
            [JsonIgnore] public bool Grown => !string.IsNullOrEmpty(UnitId);
            public int YearsToAdult(int cur) => Math.Max(0, (BirthMonth + ADULT_YEARS * 12 - cur + 11) / 12);
        }

        public List<Child> Children { get; set; } = new List<Child>();
        public int Marriages { get; set; }

        private static WorldUnitBase Player => g.world.playerUnit;
        private static SMGlobalConfigsEvent Cfg => SMLocalConfigsEvent.Instance?.Configs;
        public static bool Disabled => Cfg?.NoFamily ?? false;
        public static bool SameSex => Cfg?.AllowSameSexCouple ?? false;
        public static Surname SurnameRule => (Surname)(Cfg?.ChildSurname ?? 0);

        // ------------------------------------------------------------------ formulas (pure, unit tests)

        /// <summary>Marriage chance (% a year) at an age (years) with a lifespan (years).</summary>
        public static float MarryChance(int ageYears, int lifeYears)
        {
            if (ageYears < MARRY_MIN_YEARS || lifeYears - ageYears < MARRY_MIN_LIFE_LEFT_YEARS)
                return 0f;
            var ramp = Math.Min(1f, (ageYears - MARRY_MIN_YEARS + 1) / 12f);
            var lived = ageYears / (float)Math.Max(1, lifeYears);
            return MARRY_RATE * ramp * Clamp01(1.2f - lived * 1.5f);
        }

        /// <summary>Birth chance (% a year) of a couple: the older one's share of life lived, the average grade, children already born.</summary>
        public static float BirthChance(float livedShare, float avgGrade, int children)
        {
            if (children >= MAX_CHILDREN || livedShare >= 0.8f)
                return 0f;
            return BIRTH_RATE * Clamp01(1.4f - livedShare * 1.75f) / (1f + 0.35f * Math.Max(0f, avgGrade - 1f)) / (1f + children);
        }

        private static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);

        /// <summary>Whose surname the child takes.</summary>
        public static bool FatherSurname(Surname rule, int roll) => rule == Surname.Father || (rule == Surname.Random && roll % 2 == 0);

        // ------------------------------------------------------------------ helpers

        public static WorldUnitBase Unit(string id) => string.IsNullOrEmpty(id) ? null : (id == Player.GetUnitId() ? Player : g.world.unit.GetUnit(id));
        private static DataUnit.RelationData Rel(WorldUnitBase u) => u.data.unitData.relationData;
        public static int AgeYears(WorldUnitBase u) => u.GetProperty<int>(UnitPropertyEnum.Age) / 12;
        public static int LifeYears(WorldUnitBase u) => u.GetProperty<int>(UnitPropertyEnum.Life) / 12;
        public static bool IsMan(WorldUnitBase u) => u.data.unitData.propertyData.sex == UnitSexType.Man;
        public static string Spouse(WorldUnitBase u) => Rel(u).married;
        private static bool Alive(WorldUnitBase u) => u != null && !u.isDie;

        /// <summary>The unit and its ancestors up to 3 generations.</summary>
        public static HashSet<string> Blood(WorldUnitBase u)
        {
            var rs = new HashSet<string> { u.GetUnitId() };
            try { foreach (var id in Rel(u).LoopParent(u.GetUnitId(), 3).ToArray()) rs.Add(id); } catch { }
            foreach (var id in Rel(u).parent?.ToArray() ?? new string[0]) if (!string.IsNullOrEmpty(id)) rs.Add(id);
            return rs;
        }

        private static IEnumerable<WorldUnitBase> Npcs() =>
            (ModMain.Store?.WUnits ?? new WorldUnitBase[0]).Where(x => Alive(x) && !x.IsPlayer());

        // ------------------------------------------------------------------ yearly

        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnYearly()
        {
            base.OnYearly();
            if (Disabled)
                return;
            // one dispatch may stand for several years (time skip): a round of marriages and births a year
            for (var y = 0; y < Math.Max(1, ModLib.Mod.ModMaster.YearsPassed); y++)
            {
                try { Marry(); } catch (Exception ex) { DebugHelper.WriteLine($"FamilyEvent.Marry: {ex}"); }
                try { Births(); } catch (Exception ex) { DebugHelper.WriteLine($"FamilyEvent.Births: {ex}"); }
            }
        }

        /// <summary>Monthly: children who reach 16 years become NPCs.</summary>
        [EventCondition(IsInGame = HandleEnum.True)]
        public override void OnMonthly()
        {
            base.OnMonthly();
            if (Disabled)
                return;
            var cur = GameHelper.GetGameTotalMonth();
            foreach (var c in Children.Where(x => !x.Grown && cur - x.BirthMonth >= ADULT_YEARS * 12).ToList())
            {
                try { GrowUp(c); } catch (Exception ex) { DebugHelper.WriteLine($"FamilyEvent.GrowUp: {ex}"); c.UnitId = "-"; }
            }
            if (!GameHelper.IsInBattlle())
                CheckOldAge();
        }

        public void Marry()
        {
            var singles = Npcs().Where(x => string.IsNullOrEmpty(Spouse(x))).ToList();
            var taken = new HashSet<string>();
            var count = 0;
            foreach (var u in singles.OrderBy(x => CommonTool.Random(0, 10000)))
            {
                if (count >= MAX_MARRIAGES_A_YEAR) break;
                if (taken.Contains(u.GetUnitId()) || !CommonTool.Random(0f, 100f).IsBetween(0f, MarryChance(AgeYears(u), LifeYears(u))))
                    continue;
                var partner = FindPartner(u, singles.Where(x => !taken.Contains(x.GetUnitId())));
                if (partner == null)
                    continue;
                SetMarried(u, partner);
                taken.Add(u.GetUnitId()); taken.Add(partner.GetUnitId());
                count++;
            }
            Marriages += count;
            if (count > 0) DebugHelper.WriteLine($"FamilyEvent: {count} marriages");
        }

        public static WorldUnitBase FindPartner(WorldUnitBase u, IEnumerable<WorldUnitBase> candidates)
        {
            var area = u.GetUnitPosAreaId();
            var blood = Blood(u);
            var righteous = u.GetProperty<int>(UnitPropertyEnum.StandUp) >= u.GetProperty<int>(UnitPropertyEnum.StandDown);
            return candidates
                .Where(x => x.GetUnitId() != u.GetUnitId() && string.IsNullOrEmpty(Spouse(x)) && x.GetUnitPosAreaId() == area)
                .Where(x => SameSex || IsMan(x) != IsMan(u))
                .Where(x => AgeYears(x) >= MARRY_MIN_YEARS && LifeYears(x) - AgeYears(x) >= MARRY_MIN_LIFE_LEFT_YEARS)
                .Where(x => Math.Abs(x.GetGradeLvl() - u.GetGradeLvl()) <= 2 && !Blood(x).Overlaps(blood))
                .OrderByDescending(x => Rel(u).GetIntim(x) + ((x.GetProperty<int>(UnitPropertyEnum.StandUp) >= x.GetProperty<int>(UnitPropertyEnum.StandDown)) == righteous ? 50 : 0) + CommonTool.Random(0, 30))
                .FirstOrDefault();
        }

        /// <summary>Marries two units (the relation of the game, both ways).</summary>
/// <summary>
        /// Changes an intimacy by delta (no clamp: AddIntim with clamp 0 does nothing). Toward the player the game also
        /// keeps intimToPlayerUnit, read by GetIntim for some units (a spouse): both are written.
        /// </summary>
        public static void ChangeIntim(DataUnit.RelationData r, string toId, float delta)
        {
            if (r == null || string.IsNullOrEmpty(toId)) return;
            var v = r.GetIntim(toId) + delta;
            r.SetIntim(toId, v);
            if (g.world.playerUnit != null && toId == g.world.playerUnit.GetUnitId())
                r.intimToPlayerUnit = v;
        }

                public static void SetMarried(WorldUnitBase a, WorldUnitBase b)
        {
            try { a.CreateAction(new UnitActionRelationSet(b, UnitRelationType.Married, 0)); } catch (Exception ex) { DebugHelper.WriteLine($"FamilyEvent.SetMarried: {ex.Message}"); }
            if (Spouse(a) != b.GetUnitId()) Rel(a).married = b.GetUnitId();
            if (Spouse(b) != a.GetUnitId()) Rel(b).married = a.GetUnitId();
            TaoistLogEvent.LogIf(a.IsPlayer() || b.IsPlayer() || IsFamilyOfPlayer(a) || IsFamilyOfPlayer(b), "log_family_married", a.GetName(), b.GetName());
        }

        public void Births()
        {
            var cur = GameHelper.GetGameTotalMonth();
            if (Children.Count(x => !x.Grown) >= MAX_PENDING)
                return;
            var done = new HashSet<string>();
            var count = 0;
            foreach (var u in Npcs().Concat(new[] { Player }).Where(x => !string.IsNullOrEmpty(Spouse(x))).ToList())
            {
                if (count >= MAX_BIRTHS_A_YEAR) break;
                var s = Unit(Spouse(u));
                if (!Alive(s) || done.Contains(u.GetUnitId()) || IsMan(u) == IsMan(s))
                    continue;
                done.Add(u.GetUnitId()); done.Add(s.GetUnitId());
                var father = IsMan(u) ? u : s;
                var mother = IsMan(u) ? s : u;
                var lived = Math.Max(AgeYears(u) / (float)Math.Max(1, LifeYears(u)), AgeYears(s) / (float)Math.Max(1, LifeYears(s)));
                var born = Children.Count(x => x.FatherId == father.GetUnitId() && x.MotherId == mother.GetUnitId());
                if (!CommonTool.Random(0f, 100f).IsBetween(0f, BirthChance(lived, (u.GetGradeLvl() + s.GetGradeLvl()) / 2f, born)))
                    continue;
                Birth(father, mother, cur);
                count++;
            }
        }

        /// <summary>A child is born (a record; the NPC comes 16 years later).</summary>
        public Child Birth(WorldUnitBase father, WorldUnitBase mother, int month)
        {
            var c = new Child { FatherId = father.GetUnitId(), MotherId = mother.GetUnitId(), FatherName = father.GetName(), MotherName = mother.GetName(), BirthMonth = month };
            Children.Add(c);
            var mine = father.IsPlayer() || mother.IsPlayer();
            TaoistLogEvent.LogIf(mine || IsFamilyOfPlayer(father) || IsFamilyOfPlayer(mother), "log_family_birth", father.GetName(), mother.GetName());
            if (mine && !(TimeSkipEvent.Instance?.IsProcessing ?? false) && !GameHelper.IsInBattlle())
                DramaHelper.OpenDrama2(string.Format(GameTool.LS("family_birth"), (father.IsPlayer() ? mother : father).GetName(), ADULT_YEARS),
                    new List<string> { GameTool.LS("family_birth_o1") }, null, "shimengzhai_denglong");
            return c;
        }

        /// <summary>The child turns 16: a grade-1 NPC next to the parents, linked to them.</summary>
        public WorldUnitBase GrowUp(Child c)
        {
            var father = Unit(c.FatherId);
            var mother = Unit(c.MotherId);
            var units = g.world.run.npc.AddUnit(1, 1);
            var u = units == null || units.Count == 0 ? null : units[0];
            if (u == null) { c.UnitId = "-"; return null; }
            c.UnitId = u.GetUnitId();
            var p = u.data.unitData.propertyData;
            var surnameFrom = FatherSurname(SurnameRule, CommonTool.Random(0, 100)) ? c.FatherName : c.MotherName;
            var surname = SurnameOf(father, surnameFrom);
            if (!string.IsNullOrEmpty(surname) && p.name != null && p.name.Length > 0)
                p.name[0] = surname;
            u.SetProperty<int>(UnitPropertyEnum.Age, ADULT_YEARS * 12);
            var parents = new[] { father, mother }.Where(x => x != null).ToList();
            if (parents.Count > 0)
            {
                u.SetProperty<int>(UnitPropertyEnum.Talent, (int)parents.Average(x => x.GetProperty<int>(UnitPropertyEnum.Talent)) + CommonTool.Random(-10, 11));
                u.SetProperty<int>(UnitPropertyEnum.StandUp, (int)parents.Average(x => x.GetProperty<int>(UnitPropertyEnum.StandUp)));
                u.SetProperty<int>(UnitPropertyEnum.StandDown, (int)parents.Average(x => x.GetProperty<int>(UnitPropertyEnum.StandDown)));
                var near = parents.FirstOrDefault(x => Alive(x) && !x.IsPlayer()) ?? parents[0];
                try { u.SetUnitRandomPos(near.GetUnitPos(), 3); } catch { }
            }
            LinkParents(u, father, mother, c);
            var mine = (father?.IsPlayer() ?? false) || (mother?.IsPlayer() ?? false);
            if (mine) ChangeIntim(Rel(u), Player.GetUnitId(), 200f);
            TaoistLogEvent.LogIf(mine || IsFamilyOfPlayer(u), "log_family_grown", u.GetName(), c.FatherName, c.MotherName);
            if (mine && !(TimeSkipEvent.Instance?.IsProcessing ?? false) && !GameHelper.IsInBattlle())
                DramaHelper.OpenDrama1(string.Format(GameTool.LS("family_grown"), u.GetName()), new List<string> { GameTool.LS("family_grown_o1") }, null, u, Player);
            DebugHelper.WriteLine($"FamilyEvent: {u.GetName()} ({u.GetUnitId()}) grown up, parents {c.FatherName} / {c.MotherName}");
            return u;
        }

        /// <summary>The surname of a parent: the first part of its name (the parent unit when alive, else its recorded full name).</summary>
        private static string SurnameOf(WorldUnitBase father, string recordedName)
        {
            return string.IsNullOrEmpty(recordedName) ? null : recordedName.Substring(0, Math.Min(recordedName.Length, SurnameLength(recordedName)));
        }

        private static int SurnameLength(string fullName)
        {
            // "Lin Feng" (latin) -> "Lin"; "林峰" -> "林"; "欧阳峰" (compound surnames are rare) -> "欧"
            var space = fullName.IndexOf(' ');
            return space > 0 ? space : 1;
        }

        /// <summary>Child of the parents in the relations of the game (both ways).</summary>
        public static void LinkParents(WorldUnitBase child, WorldUnitBase father, WorldUnitBase mother, Child c)
        {
            var r = Rel(child);
            r.parent = new UnhollowerBaseLib.Il2CppStringArray(new[] { c.FatherId ?? string.Empty, c.MotherId ?? string.Empty });
            foreach (var parent in new[] { father, mother }.Where(x => x != null))
            {
                var pr = Rel(parent);
                if (!pr.children.Contains(child.GetUnitId())) pr.children.Add(child.GetUnitId());
            }
        }

        /// <summary>The unit is a child / parent / companion of the player.</summary>
        public static bool IsFamilyOfPlayer(WorldUnitBase u)
        {
            if (u == null || Instance == null) return false;
            var pid = Player.GetUnitId();
            var id = u.GetUnitId();
            return Spouse(Player) == id || Instance.Children.Any(x => x.UnitId == id && (x.FatherId == pid || x.MotherId == pid));
        }

        // ------------------------------------------------------------------ family members

        public class Members
        {
            public List<string> Parents = new List<string>();
            public string Spouse;
            public List<string> Children = new List<string>();
            public List<Child> Unborn = new List<Child>();
            public List<string> Siblings = new List<string>();
        }

        /// <summary>Family of a unit (relations of the game + the children records).</summary>
        public static Members Family(WorldUnitBase u)
        {
            var m = new Members();
            var r = Rel(u);
            var id = u.GetUnitId();
            m.Parents.AddRange((r.parent?.ToArray() ?? new string[0]).Where(x => !string.IsNullOrEmpty(x)));
            m.Spouse = string.IsNullOrEmpty(r.married) ? null : r.married;
            m.Children.AddRange(r.children?.ToArray() ?? new string[0]);
            if (Instance != null)
            {
                m.Children.AddRange(Instance.Children.Where(x => x.Grown && x.UnitId != "-" && (x.FatherId == id || x.MotherId == id)).Select(x => x.UnitId));
                m.Unborn.AddRange(Instance.Children.Where(x => !x.Grown && (x.FatherId == id || x.MotherId == id)));
            }
            m.Children = m.Children.Distinct().ToList();
            m.Siblings.AddRange((r.brother?.ToArray() ?? new string[0]));
            foreach (var pid in m.Parents)
            {
                var p = Unit(pid);
                if (p != null) m.Siblings.AddRange((Rel(p).children?.ToArray() ?? new string[0]));
            }
            m.Siblings = m.Siblings.Where(x => x != id).Distinct().ToList();
            return m;
        }

        // ------------------------------------------------------------------ continue as a descendant

        /// <summary>Month the player last declined to pass on (asked once).</summary>
        public int InheritDeclinedMonth { get; set; } = -1;
        public int Generation { get; set; } = 1;
        public const int MAX_HEIRS_SHOWN = 3;
        public static bool CanInherit => !Disabled && (Cfg?.ContinueAsDescendant ?? false);

        /// <summary>Living grown-up children and grandchildren of the player, strongest first.</summary>
        public static List<WorldUnitBase> Heirs()
        {
            var rs = new List<WorldUnitBase>();
            var children = Family(Player).Children.Select(Unit).Where(Alive).ToList();
            rs.AddRange(children);
            foreach (var c in children)
                rs.AddRange(Family(c).Children.Select(Unit).Where(Alive));
            return rs.Where(x => !x.IsPlayer()).GroupBy(x => x.GetUnitId()).Select(x => x.First())
                .OrderByDescending(x => x.GetGradeLvl()).ThenByDescending(x => x.GetProperty<int>(UnitPropertyEnum.Exp)).ToList();
        }

        /// <summary>
        /// The player is about to die (old age, one-life): with an heir, a drama offers to play on as one of them.
        /// Returns false when nothing is offered (the caller lets the player die); onDecline runs when the player refuses.
        /// </summary>
        public bool OfferInherit(Action onDecline)
        {
            if (!CanInherit) return false;
            var heirs = Heirs().Take(MAX_HEIRS_SHOWN).ToList();
            if (heirs.Count == 0) return false;
            var opts = heirs.Select(x => string.Format(GameTool.LS("family_inherit_o"), x.GetName(), AgeYears(x), GameTool.LS(x.GetGradeConf()?.gradeName ?? string.Empty))).ToList();
            opts.Add(GameTool.LS("family_inherit_no"));
            DramaHelper.OpenDrama2(GameTool.LS("family_inherit"), opts, x =>
            {
                var i = x.id % 10; // option index from 0
                if (i >= 0 && i < heirs.Count) AskSkills(heirs[i]);
                else { InheritDeclinedMonth = GameHelper.GetGameTotalMonth(); onDecline?.Invoke(); }
            }, "huapaonanziganxie");
            return true;
        }

        /// <summary>Second question: the heir learns the player's techniques, or keeps their own.</summary>
        public void AskSkills(WorldUnitBase heir)
        {
            var opts = new List<string> { GameTool.LS("family_inherit_skills_keep"), string.Format(GameTool.LS("family_inherit_skills_own"), heir.GetName()) };
            DramaHelper.OpenDrama2(string.Format(GameTool.LS("family_inherit_skills"), heir.GetName()), opts,
                x => Inherit(heir, x.id % 10 == 0), "huapaonanziganxie");
        }

        /// <summary>Old age: offered when 2 months of lifespan are left (again 2 years after a refusal).</summary>
        public void CheckOldAge()
        {
            if (!CanInherit || (TimeSkipEvent.Instance?.IsProcessing ?? false) || InheritDeclinedMonth >= 0 && GameHelper.GetGameTotalMonth() - InheritDeclinedMonth < 24)
                return;
            var left = Player.GetProperty<int>(UnitPropertyEnum.Life) - Player.GetProperty<int>(UnitPropertyEnum.Age);
            if (left <= 2)
                OfferInherit(null);
        }

        /// <summary>Base properties not taken from the heir: the unit links and the acquired destinies (addLuck) of the player.</summary>
        private static readonly HashSet<string> KEEP = new HashSet<string> { "unit", "_unit", "unitInfo", "addLuck", "Pointer", "ObjectClass", "WasCollected" };

        /// <summary>
        /// Intimacy of the heir-player with someone: the old player's friendships count half, its enmities (negative)
        /// in full; the heir's own bond wins, except that an enmity on either side stays (the lowest one).
        /// </summary>
        public static float? InheritIntim(float? old, float? own)
        {
            var legacy = old.HasValue && old.Value > 0 ? old.Value / 2f : old;
            if (!own.HasValue) return legacy;
            if (!legacy.HasValue) return own;
            return own.Value < 0 || legacy.Value < 0 ? Math.Min(own.Value, legacy.Value) : Math.Max(own.Value, legacy.Value);
        }

        /// <summary>
        /// The player becomes the heir (the player unit keeps its id; the heir NPC is removed):
        /// - from the heir: name, look, sex, age, lifespan, grade, exp, basis, talent, reputation..., companion, children,
        ///   lovers, master/students, sect and post, town master seats; its techniques when keepSkills is false;
        /// - from the old player: items, spirit stones, acquired destinies, Taoist loans; its techniques when keepSkills;
        ///   its relations at half the intimacy, its enmities in full;
        /// - gone with the old player: its reputation, sect post, town master seats, bounties on it (refunded), companion.
        /// </summary>
        public void Inherit(WorldUnitBase heir, bool keepSkills = true)
        {
            var oldName = Player.GetName();
            var pid = Player.GetUnitId();
            var hid = heir.GetUnitId();
            var heirSchool = heir.data.unitData.schoolID;
            var heirPost = string.IsNullOrEmpty(heirSchool) ? SchoolPostType.Out : (g.world.build.GetBuild<MapBuildSchool>(heirSchool)?.buildData?.GetPostType(hid) ?? SchoolPostType.Out);

            // 0. bounties (before the name changes): the ones on the old player end (refunded), the ones on the heir follow the player
            var wanted = WantedEvent.Instance;
            if (wanted != null)
            {
                foreach (var b in wanted.Bounties.Where(x => x.TargetId == pid).ToList())
                    wanted.Refund(b, "log_wanted_heir");
                foreach (var b in wanted.Bounties.Where(x => x.TargetId == hid)) b.TargetId = pid;
            }

            // 1. base properties of the heir
            var src = heir.data.unitData.propertyData;
            var dst = Player.data.unitData.propertyData;
            foreach (var prop in typeof(DataUnit.PropertyData).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite || KEEP.Contains(prop.Name) || prop.GetIndexParameters().Length > 0)
                    continue;
                try { prop.SetValue(dst, prop.GetValue(src)); } catch (Exception ex) { DebugHelper.WriteLine($"FamilyEvent.Inherit {prop.Name}: {ex.Message}"); }
            }

            // 2. techniques of the heir (learned martial arts and the equipped slots)
            if (!keepSkills)
            {
                var pu = Player.data.unitData;
                var hu = heir.data.unitData;
                try
                {
                    pu.allActionMartial = hu.allActionMartial;
                    pu.skillLeft = hu.skillLeft;
                    pu.skillRight = hu.skillRight;
                    pu.ultimate = hu.ultimate;
                    pu.step = hu.step;
                    pu.abilitys = hu.abilitys;
                    hu.allActionMartial = new Il2CppSystem.Collections.Generic.Dictionary<string, DataUnit.ActionMartialData>();
                }
                catch (Exception ex) { DebugHelper.WriteLine($"FamilyEvent.Inherit skills: {ex.Message}"); }
            }

            // 3. sect: the old player's post ends; the heir's membership and post go on
            try
            {
                if (!string.IsNullOrEmpty(heirSchool)) heir.CreateAction(new UnitActionSchoolExit());
                if (!string.IsNullOrEmpty(Player.data.unitData.schoolID)) Player.CreateAction(new UnitActionSchoolExit());
                if (!string.IsNullOrEmpty(heirSchool)) Player.CreateAction(new UnitActionSchoolJoin(heirSchool, heirPost, SchoolDepartmentType.None));
            }
            catch (Exception ex) { DebugHelper.WriteLine($"FamilyEvent.Inherit sect: {ex.Message}"); }

            // 4. town master seats: the old player's end, the heir's go on
            var towns = MapBuildPropertyEvent.Instance?.TownMasters;
            if (towns != null)
                foreach (var seats in towns.Values)
                {
                    var heirSeat = seats.Contains(hid);
                    seats.RemoveAll(x => x == pid || x == hid);
                    if (heirSeat) seats.Add(pid);
                }

            // 5. relations
            InheritRelations(heir, pid, hid);

            // 6. family records: the old player is an ancestor now
            var ancestor = $"ancestor{Generation}_{pid}";
            foreach (var c in Children)
            {
                if (c.FatherId == pid) c.FatherId = ancestor;
                if (c.MotherId == pid) c.MotherId = ancestor;
            }
            foreach (var c in Children)
            {
                if (c.FatherId == hid) c.FatherId = pid;
                if (c.MotherId == hid) c.MotherId = pid;
                if (c.UnitId == hid) c.UnitId = pid;
            }

            // 7. S&M score: with easy settings (P < 0) the descendant keeps only a share of the spirit stones
            var share = SMScoreEvent.InheritShare(SMScoreEvent.P);
            if (share < 1f)
            {
                var lost = (int)(Player.GetUnitMoney() * (1f - share));
                if (lost > 0)
                {
                    Player.AddUnitMoney(-lost);
                    TaoistLogEvent.Log("log_score_inherit", lost, SMScoreEvent.P);
                }
            }

            g.world.unit.DelUnitSecret(heir);
            RefreshPlayerLook();
            Generation++;
            InheritDeclinedMonth = -1;
            TaoistLogEvent.Log("log_family_inherit", oldName, Player.GetName(), Generation);
            DebugHelper.WriteLine($"FamilyEvent: {oldName} -> {Player.GetName()} (heir {hid}), generation {Generation}, techniques of the {(keepSkills ? "old player" : "heir")}");
            DramaHelper.OpenDrama2(string.Format(GameTool.LS("family_inherited"), oldName, Player.GetName(), Generation), new List<string> { GameTool.LS("family_inherited_o1") },
                x => GameHelper.SaveGame(), "huapaonanziganxie");
        }

        /// <summary>
        /// Relations after the inheritance. The player takes the heir's family and bonds (parents, companion, children,
        /// lovers, master, students, siblings); the old player's ones end, except intimacies: everybody's intimacy with
        /// the old player and with the heir merge into the player (InheritIntim). Every id of the heir becomes the player's.
        /// </summary>
        private static void InheritRelations(WorldUnitBase heir, string pid, string hid)
        {
            var pr = Rel(Player);
            var hr = Rel(heir);
            var oldSpouse = pr.married;
            var oldChildren = pr.children.ToArray().Where(x => x != hid).ToList();

            // the old player's companion and children are no longer the player's
            var spouse = Unit(oldSpouse);
            if (spouse != null && Rel(spouse).married == pid) Rel(spouse).married = string.Empty;
            foreach (var cid in oldChildren)
            {
                var c = Unit(cid);
                if (c == null) continue;
                var cp = Rel(c).parent;
                if (cp != null) for (var i = 0; i < cp.Length; i++) if (cp[i] == pid) cp[i] = string.Empty;
            }

            // the heir's bonds
            var parents = (hr.parent?.ToArray() ?? new string[0]).Select(x => x == pid ? string.Empty : x).ToArray();
            pr.parent = new UnhollowerBaseLib.Il2CppStringArray(new[] { parents.ElementAtOrDefault(0) ?? string.Empty, parents.ElementAtOrDefault(1) ?? string.Empty });
            pr.married = hr.married ?? string.Empty;
            CopyList(hr.children, pr.children, pid);
            CopyList(hr.brother, pr.brother, pid);
            CopyList(hr.lover, pr.lover, pid);
            CopyList(hr.master, pr.master, pid);
            CopyList(hr.student, pr.student, pid);
            foreach (var sid in oldChildren) if (!pr.brother.Contains(sid)) pr.brother.Add(sid);

            // intimacies of the player
            var mine = ToDict(pr.intimToUnit);
            var heirs = ToDict(hr.intimToUnit);
            pr.intimToUnit.Clear();
            foreach (var id in mine.Keys.Union(heirs.Keys).Where(x => x != pid && x != hid))
            {
                var v = InheritIntim(mine.TryGetValue(id, out var a) ? a : (float?)null, heirs.TryGetValue(id, out var b) ? b : (float?)null);
                if (v.HasValue) pr.intimToUnit[id] = v.Value;
            }

            // everybody else: intimacies and links to the heir / the old player
            foreach (var u in g.world.unit.GetUnits().ToArray())
            {
                if (u == null || u.GetUnitId() == pid || u.GetUnitId() == hid) continue;
                var r = Rel(u);
                var d = r.intimToUnit;
                float? old = d.ContainsKey(pid) ? d[pid] : (float?)null;
                float? own = d.ContainsKey(hid) ? d[hid] : (float?)null;
                if (old.HasValue || own.HasValue)
                {
                    d.Remove(hid);
                    d.Remove(pid);
                    var v = InheritIntim(old, own);
                    if (v.HasValue) d[pid] = v.Value;
                }
                // bonds with the old player end (it is dead), bonds with the heir point to the player
                foreach (var list in new[] { r.lover, r.master, r.student, r.brother })
                {
                    if (list == null) continue;
                    list.Remove(pid);
                    if (list.Remove(hid)) list.Add(pid);
                }
                if (r.married == hid) r.married = pid;
                var up = r.parent;
                if (up != null) for (var i = 0; i < up.Length; i++) if (up[i] == hid) up[i] = pid;
                if (r.children != null && r.children.Remove(hid) && !r.children.Contains(pid)) r.children.Add(pid);
            }
        }

        private static void CopyList(Il2CppSystem.Collections.Generic.List<string> from, Il2CppSystem.Collections.Generic.List<string> to, string self)
        {
            if (to == null) return;
            to.Clear();
            if (from == null) return;
            foreach (var id in from.ToArray()) if (id != self && !to.Contains(id)) to.Add(id);
        }

        private static Dictionary<string, float> ToDict(Il2CppSystem.Collections.Generic.Dictionary<string, float> d)
        {
            var rs = new Dictionary<string, float>();
            if (d == null) return rs;
            foreach (var kv in d) rs[kv.Key] = kv.Value;
            return rs;
        }

        /// <summary>The look of the player (map model, portrait of the map) after its base properties changed.</summary>
        public static void RefreshPlayerLook()
        {
            try { SceneType.map?.world?.ResetUnitModel(); } catch (Exception ex) { DebugHelper.WriteLine($"FamilyEvent: map model {ex.Message}"); }
            try { SceneType.map?.world?.UpdatePlayerModel(true); } catch (Exception ex) { DebugHelper.WriteLine($"FamilyEvent: player model {ex.Message}"); }
            try
            {
                var info = g.ui.GetUI<UIMapMain>(UIType.MapMain)?.uiPlayerInfo;
                info?.ResetUnitModel();
                info?.UpdateUI();
            }
            catch (Exception ex) { DebugHelper.WriteLine($"FamilyEvent: portrait {ex.Message}"); }
        }

        // ------------------------------------------------------------------ clan revenge

        /// <summary>The player kills a unit: its relatives who are town masters put a bounty on the player.</summary>
        public override void OnBattleUnitDie(UnitDie e)
        {
            base.OnBattleUnitDie(e);
            if (Disabled || WantedEvent.Disabled || e?.unit == null || !e.unit.IsWorldUnit())
                return;
            var killer = e.hitData?.attackUnit?.GetWorldUnit();
            if (killer == null || !killer.IsPlayer())
                return;
            var dead = e.unit.GetWorldUnit();
            if (dead == null) return;
            var f = Family(dead);
            var relatives = f.Parents.Concat(f.Children).Concat(f.Siblings).Concat(f.Spouse == null ? new string[0] : new[] { f.Spouse });
            foreach (var town in ModMain.Store?.Towns ?? new MapBuildTown[0])
            {
                var master = town == null ? null : MapBuildPropertyEvent.GetTownMaster(town);
                if (master == null || master.IsPlayer() || !relatives.Contains(master.GetUnitId()))
                    continue;
                var reward = Math.Max(WantedEvent.MinReward(Player), (int)Math.Min(int.MaxValue, MapBuildPropertyEvent.GetBuildProperty(town) / 5));
                if (WantedEvent.Instance.Post(town, master, Player, reward) == null)
                    TaoistLogEvent.Log("log_family_revenge", master.GetName(), dead.GetName(), town.name);
            }
        }

        // ------------------------------------------------------------------ UI

        /// <summary>
        /// Family tree of a unit: the game's kinship screen (NPCInfoRelation: parents / master above, the unit and its
        /// dao companion, children below; the family of the mod is in the relations of the game: SetMarried,
        /// LinkParents), plus what only the mod knows (children not born yet, siblings) in a note at the bottom left.
        /// Opened from the household accounts window (NPC info has the game's own Family tab).
        /// </summary>
        public static void OpenFamily(WorldUnitBase u)
        {
            if (u == null) return;
            var ui = g.ui.OpenUISafe<UINPCInfoRelation>(UIType.NPCInfoRelation);
            ui.InitData(u);
            var lines = FamilyNotes(u);
            if (lines.Count == 0) return;
            var cover = new UICover<UINPCInfoRelation>(ui);
            cover.AddText(cover.FirstCol + 6, cover.LastRow - 3, string.Join("\n", lines)).Format(Color.black, 16).Align(TextAnchor.LowerLeft);
        }

        /// <summary>The lines of the family note: unborn children (years to adult), siblings.</summary>
        public static List<string> FamilyNotes(WorldUnitBase u)
        {
            var f = Family(u);
            var cur = GameHelper.GetGameTotalMonth();
            Func<string, string> name = id => { var x = Unit(id); return x == null ? "?" : x.GetName() + (x.isDie ? GameTool.LS("family_dead") : string.Empty); };
            var lines = f.Unborn.Select(c => string.Format(GameTool.LS("family_unborn"), c.YearsToAdult(cur))).ToList();
            if (f.Siblings.Count > 0)
                lines.Add(string.Format(GameTool.LS("family_siblings"), string.Join(", ", f.Siblings.Select(name))));
            return lines;
        }
    }
}
