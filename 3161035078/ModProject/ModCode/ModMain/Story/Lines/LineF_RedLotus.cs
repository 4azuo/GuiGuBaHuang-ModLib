using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Final battle "Red Lotus" (docs/Taoist/Storylines/F-Red-Lotus.md): Lady Honglian {R}. Opens when line A or B
    /// reached act 4 and two other lines ended. Acts F1-F4 (F0 = F1 here), endings Victory / Kneel; late: "Spreading
    /// Demon Realm" (towns' taxes doubled every year until the battle).
    /// </summary>
    public class LineF_RedLotus : StoryLine
    {
        public override string Id => "F";
        public override float MonthlyChance => 5f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole
            {
                Name = "R", Sex = UnitSexType.Woman, MinLived = 0.4f, MaxLived = 0.7f, MinBeauty = 450,
                InTraits = new[] { 7, 5 }, OutTraits = new[] { 14 }, Righteous = false, MinGradeOffset = 1, MaxGradeOffset = 2,
            },
        };

        public override bool CanOpen(StoryState s)
        {
            var ev = StoryEvent.Instance;
            var a = ev.State("A");
            var b = ev.State("B");
            var core = (a.Act >= 4 || a.Ending != null) || (b.Act >= 4 || b.Ending != null);
            var ended = ev.Lines.Values.Count(x => x.Ending != null && x.Id != "F");
            return core && ended >= 2;
        }

        public override int TimeLimit(int act) => act <= 2 ? 60 : 0;

        /// <summary>Allies: the units of the good endings (role keys of their lines).</summary>
        private static List<(string line, string role)> Allies()
        {
            var ev = StoryEvent.Instance;
            var rs = new List<(string, string)>();
            void Add(string line, string role, params string[] endings)
            {
                var st = ev.State(line);
                if (st.Ending == null || !endings.Contains(st.Ending)) return;
                var u = StoryLine.U(st, role);
                if (u != null && !u.isDie) rs.Add((line, role));
            }
            Add("A", "A", "A_Companion", "A_SectMaster", "A_Peace");
            Add("B", "H", "B_MerchantPrince", "B_Benefactor");
            Add("D", "N", "D_ThreeSworn", "D_MurongCompanion");
            Add("D", "C", "D_ThreeSworn", "D_MurongCompanion");
            Add("E", "Y", "E_Successor", "E_GardenOfGenerations");
            Add("C", "X", "C_Companion", "C_MasterAlchemist");
            return rs;
        }

        public override void Run(StoryState s)
        {
            switch (s.Act * 100 + s.Step)
            {
                // ---- F1 Demon realm
                case 1:
                    Tell(s, "F1_1", null, _ => Next(s, 1));
                    break;
                case 2:
                    Say(s, "R", "F1_2", null, _ => Next(s));
                    break;
                case 3:
                    Tell(s, "F1_3", new[] { "F1_3_o1", "F1_3_o2" }, i =>
                    {
                        if (i == 0) Act(s, 2);
                        else { s.Set("alone"); Act(s, 3); }
                    });
                    break;

                // ---- F2 Allies (one drama per ally)
                default:
                    if (s.Act == 2)
                    {
                        var allies = Allies();
                        if (s.Step > allies.Count) { Act(s, 3, 1); return; }
                        var (line, role) = allies[s.Step - 1];
                        var st = StoryEvent.Instance.State(line);
                        var u = U(st, role);
                        DramaHelper.OpenDrama1(T(st, "F2_" + role), new List<string> { T(st, "F2_o1") }, _ =>
                        {
                            // already in a team (the player's or an NPC's): comes along, not hired again
                            if (!HirePeopleEvent.IsHired(u)) { HirePeopleEvent.Hire(Player, u); s.Set("hired_" + role); }
                            s.Set("ally_" + role);
                            s.Roles["ally_" + role] = u.GetUnitId();
                            Next(s);
                        }, u, Player, 1);
                        return;
                    }
                    if (s.Act == 3) Act3(s);
                    else if (s.Act == 4) Act4(s);
                    break;
            }
        }

        private void Act3(StoryState s)
        {
            switch (s.Step)
            {
                case 1:
                    Tell(s, "F3_1", new[] { "F3_1_o1" }, _ => Fight(s, Foe(s, false, s.Has("alone") ? 2 : 1), "F3_gate"));
                    break;
                case 2:
                    U(s, "R")?.SetUnitPos(Player.GetUnitPos());
                    Say(s, "R", "F3_2", new[] { "F3_2_o1", "F3_2_o2" }, i =>
                    {
                        if (i == 0) Next(s);
                        else Say(s, "R", "endF_Kneel", null, _ => Kneel(s));
                    });
                    break;
                case 3:
                    Tell(s, "F3_3", new[] { "F3_3_o1" }, _ => Fight(s, U(s, "R"), "F3_lady"));
                    break;
            }
        }

        private void Act4(StoryState s)
        {
            switch (s.Step)
            {
                case 1:
                    Tell(s, "F4_1", null, _ => Next(s));
                    break;
                case 2:
                    var mirror = Give(x => x.IsArtifact() != null, Player.GetGradeLvl() + 1);
                    if (s.Has("alone")) TaoistLogEvent.Log("log_story_item", Give(x => x.IsPill() != null, Player.GetGradeLvl() + 1));
                    // the towns taken by the Red Lotus are freed
                    var b = StoryEvent.Instance.State("B");
                    if (b.Ending == "B_FallenTown" && b.Roles.TryGetValue("T", out var tid) && MapBuildPropertyEvent.Instance.TaxRate.ContainsKey(tid))
                        MapBuildPropertyEvent.Instance.TaxRate[tid] /= 2;
                    Tell(s, "F4_2", null, _ => Next(s), BG, mirror);
                    break;
                case 3:
                    DismissAllies(s);
                    Tell(s, "F4_3", null, _ => End(s, "F_Victory"));
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            if (tag == "F3_gate") { if (won) Next(s); else Wait(s, 6); }
            else if (tag == "F3_lady")
            {
                if (won) { s.Set("dead_R"); U(s, "R")?.Die(); Act(s, 4); } // killed here: not "died elsewhere" (OnRoleDead)
                else Wait(s, 12);
            }
            else Next(s);
        }

        /// <summary>Late: the demon realm spreads (taxes of two towns doubled), once a year until the battle.</summary>
        public override void OnLate(StoryState s)
        {
            s.Set("spread");
            Tell(s, "F_spread", null, _ => Wait(s, 0));
        }

        /// <summary>The demon realm spreads every year while late; Kneel: bounties of the towns on the player.</summary>
        public override void Yearly(StoryState s)
        {
            if (s.Ending != "F_Kneel") return;
            var towns = (ModMain.Store?.Towns ?? new MapBuildTown[0]).Where(t => t != null).ToList();
            foreach (var t in towns.Take(2))
            {
                var m = MapBuildPropertyEvent.GetTownMaster(t);
                if (m != null && !m.IsPlayer()) WantedEvent.Instance?.Post(t, m, Player, WantedEvent.MinReward(Player));
            }
        }

        private void Kneel(StoryState s)
        {
            Player.SetProperty<int>(UnitPropertyEnum.StandUp, 0);
            Player.SetProperty<int>(UnitPropertyEnum.StandDown, 500);
            End(s, "F_Kneel");
            Yearly(s);
        }

        /// <summary>Lady Honglian dies otherwise (old age, killed): the line closes.</summary>
        /// <summary>The Lady died elsewhere (not in the last battle): the line ends, the allies go home.</summary>
        public override void OnRoleDead(StoryState s, string role)
        {
            if (s.Act >= 4) return;
            DismissAllies(s);
            Tell(s, "F_dead", null, _ => End(s, "F_Victory"));
        }

        /// <summary>The allies hired for the battle leave the team (those already in a team stay where they were).</summary>
        private static void DismissAllies(StoryState s)
        {
            foreach (var key in s.Roles.Keys.Where(k => k.StartsWith("ally_")).ToList())
            {
                var u = FamilyEvent.Unit(s.Roles[key]);
                if (u != null && s.Has("hired_" + key.Substring(5))) HirePeopleEvent.Dismiss(Player, u);
            }
        }

        /// <summary>Spreading demon realm while late: yearly, two towns' taxes doubled.</summary>
        public static void Spread(StoryState s)
        {
            if (!s.Open || !s.Has("spread") || s.Act >= 4) return;
            foreach (var t in (ModMain.Store?.Towns ?? new MapBuildTown[0]).Where(t => t != null && MapBuildPropertyEvent.Instance.TaxRate.ContainsKey(t.buildData.id)).Take(2))
                MapBuildPropertyEvent.Instance.TaxRate[t.buildData.id] = System.Math.Min(20f, MapBuildPropertyEvent.Instance.TaxRate[t.buildData.id] * 2);
        }
    }
}
