using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line H "The Roadside Inn" (docs/Taoist/Storylines/H-Roadside-Inn.md): the innkeeper Mei Niang {K} of the player's
    /// first town {T}, her husband {R}, her son {K2}. Acts H0-H5 over ~100 years (Gate: years only). Endings InnOfAges /
    /// Lineage / Ashes. The inn is read by line J (a place to hide Xiao Ye, his "Sealed" ending).
    /// </summary>
    public class LineH_RoadsideInn : StoryLine
    {
        public override string Id => "H";
        public override int Acts => 6;
        public override float MonthlyChance => 1f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole { Name = "K", Sex = UnitSexType.Woman, MaxLived = 0.15f, InTraits = new[] { 1 }, OutTraits = new[] { 13 }, Filter = u => u.GetGradeLvl() <= 2 },
        };

        /// <summary>Her death is part of the line (H4); the line goes on with her son.</summary>
        public override IEnumerable<string> VitalRoles => new string[0];

        /// <summary>A town, the first 5 years of the game.</summary>
        public override bool CanOpen(StoryState s) => PlayerTown() != null && Now < 60;

        public override void OnCast(StoryState s)
        {
            s.Roles["T"] = PlayerTown()?.buildData.id;
            U(s, "K")?.SetUnitPos(Player.GetUnitPos());
        }

        public override int TimeLimit(int act) => act == 2 ? 24 : 0;

        /// <summary>H2 late: nothing was done, the inn is gone.</summary>
        public override void OnLate(StoryState s)
        {
            if (s.Act != 2) return;
            s.Set("ashes");
            s.Step = 2;
        }

        /// <summary>The inn still stands (line J hides Xiao Ye there).</summary>
        public static bool InnStands(StoryState h) => h != null && (h.Open || h.Ending == "H_InnOfAges" || h.Ending == "H_Lineage") && !h.Has("ashes");

        public override void Run(StoryState s)
        {
            switch (s.Act * 100 + s.Step)
            {
                // ---- H0 A leaking roof
                case 1:
                    var loan = EncounterEvent.Cost(3);
                    Say(s, "K", "H0_1", new[] { "H0_1_o1", "H0_1_o2", "H0_1_o3" }, i =>
                    {
                        if (i == 0) { if (!Pay(s, loan)) return; s.Set("lent"); AddIntim(s, "K", 200); Next(s); }
                        else if (i == 1) Fight(s, Foe(s, false), "H0_shark");
                        else { s.Set("small"); Next(s); }
                    }, loan);
                    break;
                case 2:
                    Say(s, "K", s.Has("small") ? "H0_small" : "H0_2", null, _ => Act(s, 1, 12));
                    break;

                // ---- H1 The wedding (10 years)
                case 101:
                    if (!Gate(s, 10, 0)) return;
                    if (!s.Roles.ContainsKey("R")) CastHusband(s);
                    U(s, "K")?.SetUnitPos(Player.GetUnitPos());
                    var gift = EncounterEvent.Cost(2);
                    Say(s, "K", "H1_1", new[] { "H1_1_o1", "H1_1_o2" }, i =>
                    {
                        if (i == 0) { if (!Pay(s, gift)) return; AddIntim(s, "K", 150); }
                        else AddIntim(s, "K", 50);
                        Act(s, 2, 12);
                    }, gift);
                    break;

                // ---- H2 The fire (25 years; 2 years)
                case 201:
                    if (!Gate(s, 25, 0)) return;
                    var rebuild = EncounterEvent.Cost(10);
                    Tell(s, "H2_1", new[] { "H2_1_o1", "H2_1_o2", "H2_1_o3" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, false), "H2_bandits");
                        else if (i == 1) { if (!Pay(s, rebuild)) return; Next(s); }
                        else { s.Set("ashes"); Next(s); }
                    }, BG, rebuild);
                    break;
                case 202:
                    Say(s, "K", s.Has("ashes") ? "H2_ashes" : "H2_saved", null, _ => Act(s, 3, 12));
                    break;

                // ---- H3 The son who wants to fly (40 years)
                case 301:
                    if (!Gate(s, 40, 0)) return;
                    if (!s.Roles.ContainsKey("K2") && !CastSon(s)) { Wait(s, 6); return; }
                    U(s, "K2")?.SetUnitPos(Player.GetUnitPos());
                    Say(s, "K2", "H3_1", new[] { "H3_1_o1", "H3_1_o2", "H3_1_o3" }, i =>
                    {
                        if (i == 0) s.Set("disciple");
                        else if (i == 1) s.Set("sect");
                        Act(s, 4, 12);
                    });
                    break;

                // ---- H4 Mei Niang grows old (60 years, or her last years)
                case 401:
                    var k = U(s, "K");
                    var old = k != null && !k.isDie && k.GetProperty<int>(UnitPropertyEnum.Life) - k.GetProperty<int>(UnitPropertyEnum.Age) <= 5 * 12;
                    if (k != null && !k.isDie && !old && !Gate(s, 60, 0)) return;
                    if (k == null || k.isDie) { Tell(s, "H4_gone", null, _ => Act(s, 5, 12)); return; }
                    k.SetUnitPos(Player.GetUnitPos());
                    Say(s, "K", "H4_1", new[] { "H4_1_o1" }, _ => Next(s));
                    break;
                case 402:
                    Tell(s, "H4_2", null, _ =>
                    {
                        var kk = U(s, "K");
                        if (kk != null && !kk.isDie) kk.Die();
                        Act(s, 5, 12);
                    });
                    break;

                // ---- H5 The hundredth year: a feast with the people of the player's life
                case 501:
                    if (!Gate(s, 100, 0)) return;
                    Tell(s, "H5_1", null, _ => Ending(s), BG, Guests());
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            if (tag == "H0_shark") { if (won) AddIntim(s, "K", 200); Next(s); }
            else if (tag == "H2_bandits") { if (!won) s.Set("ashes"); Next(s); }
            else Next(s);
        }

        /// <summary>The people of the player's other lines (their endings), for the feast.</summary>
        private static string Guests()
        {
            var names = new List<string>();
            foreach (var line in new[] { "A", "B", "C", "D", "E", "G" })
            {
                var st = StoryEvent.Instance.State(line);
                if (st.Ending != null) names.Add(GameTool.LS("story_name_" + line));
            }
            return names.Count == 0 ? GameTool.LS("story_H5_alone") : string.Join(", ", names);
        }

        private void Ending(StoryState s)
        {
            if (s.Has("ashes")) Tell(s, "endH_Ashes", null, _ => End(s, "H_Ashes"));
            else if (s.Has("disciple")) Say(s, "K2", "endH_Lineage", null, _ => End(s, "H_Lineage"));
            else Tell(s, "endH_InnOfAges", null, _ => End(s, "H_InnOfAges"));
        }

        /// <summary>Inn of Ages: a yearly meal at the inn (mood full, health back).</summary>
        public override void Yearly(StoryState s)
        {
            if (s.Ending != "H_InnOfAges" && s.Ending != "H_Lineage") return;
            Player.SetProperty<int>(UnitPropertyEnum.Mood, Player.GetProperty<int>(UnitPropertyEnum.MoodMax));
            Player.SetProperty<int>(UnitPropertyEnum.Health, Player.GetProperty<int>(UnitPropertyEnum.HealthMax));
            TaoistLogEvent.Log("log_story_gift", T(s, "name_H"), T(s, "H_meal"));
        }

        /// <summary>{R}: a single man of her age (a caravan guard); married to her.</summary>
        private static void CastHusband(StoryState s)
        {
            var k = U(s, "K");
            var role = new StoryRole { Name = "R", Sex = UnitSexType.Man, MaxLived = 0.3f, Filter = u => string.IsNullOrEmpty(FamilyEvent.Spouse(u)) };
            var r = StoryCasting.Pick(role, StoryEvent.Instance.TakenUnits(), StoryEvent.Units(), Player) ?? StoryCasting.Create(role, Player);
            if (r == null || k == null) return;
            s.Roles["R"] = r.GetUnitId();
            if (string.IsNullOrEmpty(FamilyEvent.Spouse(k))) FamilyEvent.SetMarried(r, k);
        }

        /// <summary>{K2}: her son, 16 (a young man of the world), her child in the relations.</summary>
        private static bool CastSon(StoryState s)
        {
            var role = new StoryRole { Name = "K2", Sex = UnitSexType.Man, MaxLived = 0.06f, Filter = u => u.GetGradeLvl() <= 2 };
            var son = StoryCasting.Pick(role, StoryEvent.Instance.TakenUnits(), StoryEvent.Units(), Player) ?? StoryCasting.Create(role, Player);
            if (son == null) return false;
            s.Roles["K2"] = son.GetUnitId();
            var k = U(s, "K");
            var r = U(s, "R");
            if (k != null && r != null) FamilyEvent.LinkParents(son, r, k, new FamilyEvent.Child { FatherId = r.GetUnitId(), MotherId = k.GetUnitId() });
            return true;
        }
    }
}
