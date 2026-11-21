using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line O "Ashes of the Town" (docs/Taoist/Storylines/O-Ashes-Of-Town.md, merit and massacre): a child {O} survived a
    /// town {T} put to the sword (a ruin of MeritEvent, else a town burned by demonic cultivators). The player who did it
    /// (a ruin and karma) is the one the child will hunt; a righteous player helps rebuild. Acts O0-O3 over ~10 years:
    /// the child in the ashes, the question, the training, the reckoning / the new town. Endings Atonement / Ashes /
    /// Rebuilt.
    /// </summary>
    public class LineO_AshesOfTown : StoryLine
    {
        public override string Id => "O";
        public override int Acts => 4;
        public override float MonthlyChance => 0.6f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole { Name = "O", MaxLived = 0.12f, InTraits = new[] { 3, 5 }, Filter = u => u.GetGradeLvl() <= 1 },
        };

        /// <summary>The child grows up: the line goes on through the years.</summary>
        public override System.Collections.Generic.IEnumerable<string> VitalRoles => new[] { "O" };

        /// <summary>A ruin of a massacre (the player did it when it is under karma), else from grade 2.</summary>
        private static MapBuildTown Ruin() =>
            MeritEvent.Enabled ? (ModMain.Store?.Towns ?? new MapBuildTown[0]).FirstOrDefault(t => t != null && MeritEvent.IsRuin(t)) : null;

        public override bool CanOpen(StoryState s) => MeritEvent.Enabled && (Ruin() != null || Player.GetGradeLvl() >= 2);

        public override void OnCast(StoryState s)
        {
            var ruin = Ruin();
            s.Roles["T"] = (ruin ?? PlayerTown() ?? (ModMain.Store?.Towns ?? new MapBuildTown[0]).FirstOrDefault(t => t != null))?.buildData.id;
            // the butcher: a ruin and the player under karma
            if (ruin != null && MeritEvent.Level(Player) < 0) s.Set("butcher");
            s.Set("opened", Now);
            U(s, "O")?.SetUnitPos(Player.GetUnitPos());
        }

        public override void Run(StoryState s)
        {
            switch (s.Act * 100 + s.Step)
            {
                // ---- O0 The child in the ashes
                case 1:
                    Tell(s, s.Has("butcher") ? "O0_butcher" : "O0_1", new[] { "O0_1_o1", "O0_1_o2" }, i =>
                    {
                        if (i == 0) { s.Set("raised"); AddIntim(s, "O", 300); }
                        else { if (!Pay(s, EncounterEvent.Cost(2))) return; AddIntim(s, "O", 50); }
                        Act(s, 1, 24);
                    });
                    break;

                // ---- O1 The question (2 years)
                case 101:
                    if (!Gate(s, 2, 0)) return;
                    Say(s, "O", "O1_1", new[] { "O1_1_o1", "O1_1_o2", "O1_1_o3" }, i =>
                    {
                        if (i == 0) { s.Set("truth"); if (DaoHeartEvent.Enabled) DaoHeartEvent.Add(Player, 100, "heart_story"); }
                        else if (i == 1) { s.Set("lie"); if (DaoHeartEvent.Enabled) DaoHeartEvent.Add(Player, -100, "heart_story"); }
                        Act(s, 2, 24);
                    });
                    break;

                // ---- O2 The training (4 years)
                case 201:
                    if (!Gate(s, 4, 0)) return;
                    Say(s, "O", "O2_1", new[] { "O2_1_o1", "O2_1_o2" }, i =>
                    {
                        if (i == 0) Spar(s, U(s, "O"), "O2_spar");
                        else { AddIntim(s, "O", 100); Act(s, 3, 36); }
                    });
                    break;

                // ---- O3 The reckoning / the new town (7 years)
                case 301:
                    if (!Gate(s, 7, 0)) return;
                    if (s.Has("butcher") && !s.Has("lie"))
                    {
                        // the truth was told (or never asked): the child comes for the butcher
                        Say(s, "O", "O3_reckoning", new[] { "O3_r_o1", "O3_r_o2" }, i =>
                        {
                            if (i == 0) Tell(s, "endO_Atonement", null, _ => Atone(s));
                            else Fight(s, U(s, "O"), "O3_duel");
                        });
                        return;
                    }
                    var cost = EncounterEvent.Cost(20);
                    Say(s, "O", s.Has("lie") ? "O3_lie" : "O3_rebuild", new[] { "O3_b_o1", "O3_b_o2" }, i =>
                    {
                        if (i == 0)
                        {
                            if (!Pay(s, cost)) return;
                            Say(s, "O", "endO_Rebuilt", null, _ => Rebuild(s, cost));
                        }
                        else Tell(s, "endO_Ashes", null, _ => End(s, "O_Ashes"));
                    }, cost);
                    break;
            }
        }

        /// <summary>Atonement: the butcher lets the child win; merit (the karma shrinks) and Dao heart.</summary>
        private static void Atone(StoryState s)
        {
            if (MeritEvent.Enabled) MeritEvent.Add(Player, 2000, "merit_story");
            if (DaoHeartEvent.Enabled) DaoHeartEvent.Add(Player, 300, "heart_story");
            End(s, "O_Atonement");
        }

        /// <summary>Rebuilt: the town's budget gets the money, the ruin lifted; merit.</summary>
        private static void Rebuild(StoryState s, int cost)
        {
            var t = s.Roles.TryGetValue("T", out var id) ? g.world.build.GetBuild(id)?.TryCast<MapBuildTown>() : null;
            if (t != null)
            {
                MapBuildPropertyEvent.AddBuildProperty(t, cost);
                if (MeritEvent.Enabled) MeritEvent.Instance.Ruins.Remove(t.buildData.id);
            }
            if (MeritEvent.Enabled) MeritEvent.Add(Player, 1000, "merit_story");
            AddIntim(s, "O", 300);
            End(s, "O_Rebuilt");
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "O2_spar": AddIntim(s, "O", 100); Act(s, 3, 36); break;
                case "O3_duel":
                    if (won) Tell(s, "endO_Ashes", null, _ => { if (DaoHeartEvent.Enabled) DaoHeartEvent.Add(Player, -300, "heart_story"); End(s, "O_Ashes"); });
                    else Tell(s, "endO_Atonement", null, _ => Atone(s));
                    break;
                default: Next(s); break;
            }
        }
    }
}
