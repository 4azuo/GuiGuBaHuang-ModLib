using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line N "The House on the Old Street" (docs/Taoist/Storylines/N-House-Seven.md, the estates): the player's house in a
    /// town {T} hides the cellar of a cultivator who lived there before; the neighbour {N} and the street. Acts N0-N4: the
    /// cellar (open it: its puppet / seal it), the bandits of the street, the cellar's notes (pull the street down for a
    /// manor / keep it), the attack on the town (the wall), the street's verdict. Endings Home / Manor / Empty.
    /// </summary>
    public class LineN_HouseSeven : StoryLine
    {
        public override string Id => "N";
        public override float MonthlyChance => 1f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole { Name = "N", Sex = UnitSexType.Woman, MinLived = 0.3f, MaxLived = 0.8f, InTraits = new[] { 1, 2 }, Filter = u => u.GetGradeLvl() <= 2 },
        };

        /// <summary>The town of the player's house (the first one found).</summary>
        public static MapBuildTown HouseTown() =>
            (ModMain.Store?.Towns ?? new MapBuildTown[0]).FirstOrDefault(t => t != null && EstateEvent.Instance != null && EstateEvent.Get(t) != null && EstateEvent.HasHouse(t, Player));

        /// <summary>The estates on and a house of the player.</summary>
        public override bool CanOpen(StoryState s) => EstateEvent.Enabled && HouseTown() != null;

        public override void OnCast(StoryState s)
        {
            s.Roles["T"] = HouseTown()?.buildData.id;
            U(s, "N")?.SetUnitPos(Player.GetUnitPos());
        }

        public override int TimeLimit(int act) => act == 3 ? 12 : 0;

        /// <summary>N3 late: the wall held without the player (the street remembers).</summary>
        public override void OnLate(StoryState s)
        {
            if (s.Act != 3) return;
            s.Set("absent");
            Act(s, 4);
        }

        private static MapBuildTown Town(StoryState s) => s.Roles.TryGetValue("T", out var id) ? g.world.build.GetBuild(id)?.TryCast<MapBuildTown>() : null;

        public override void Run(StoryState s)
        {
            var grade = System.Math.Max(1, Player.GetGradeLvl());
            switch (s.Act * 100 + s.Step)
            {
                // ---- N0 The cellar
                case 1:
                    Tell(s, "N0_1", new[] { "N0_1_o1", "N0_1_o2" }, i =>
                    {
                        if (i == 0) FightBeast(s, "N0_puppet", 0);
                        else { s.Set("sealed"); Next(s); }
                    });
                    break;
                case 2:
                    if (!s.Has("sealed")) Give(x => x.IsPill() != null, grade);
                    Say(s, "N", s.Has("sealed") ? "N0_sealed" : "N0_2", null, _ => Act(s, 1, 6));
                    break;

                // ---- N1 The bandits of the street
                case 101:
                    var guards = EncounterEvent.Cost(4);
                    Say(s, "N", "N1_1", new[] { "N1_1_o1", "N1_1_o2", "N1_1_o3" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, false, 0), "N1_bandits");
                        else if (i == 1) { if (!Pay(s, guards)) return; s.Set("street"); AddIntim(s, "N", 100); Act(s, 2, 12); }
                        else { s.Set("cold"); if (DaoHeartEvent.Enabled) DaoHeartEvent.Add(Player, -50, "heart_story"); Act(s, 2, 12); }
                    }, guards);
                    break;

                // ---- N2 The notes of the cellar
                case 201:
                    Tell(s, "N2_1", new[] { "N2_1_o1", "N2_1_o2" }, i =>
                    {
                        if (i == 0) { s.Set("manor"); Give(x => x.IsArtifact() != null, grade); AddIntim(s, "N", -300); }
                        else { s.Set("street"); AddIntim(s, "N", 200); if (MeritEvent.Enabled) MeritEvent.Add(Player, 200, "merit_story"); }
                        Act(s, 3, 12);
                    });
                    break;

                // ---- N3 The attack (1 year)
                case 301:
                    var wall = EncounterEvent.Cost(10);
                    Say(s, "N", "N3_1", new[] { "N3_1_o1", "N3_1_o2", "N3_1_o3" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, false, 1), "N3_raid");
                        else if (i == 1)
                        {
                            if (!Pay(s, wall)) return;
                            var t = Town(s);
                            if (t != null) MapBuildPropertyEvent.AddBuildProperty(t, wall);
                            s.Set("wall");
                            Act(s, 4, 3);
                        }
                        else { s.Set("absent"); Act(s, 4, 3); }
                    }, wall);
                    break;

                // ---- N4 The verdict of the street
                case 401:
                    var good = (s.Has("street") ? 1 : 0) + (s.Flag("N1_bandits") > 0 ? 1 : 0) + (s.Flag("N3_raid") > 0 || s.Has("wall") ? 1 : 0);
                    if (s.Has("manor"))
                        Tell(s, "endN_Manor", null, _ => { Give(x => x.IsArtifact() != null, grade + 1); End(s, "N_Manor"); });
                    else if (good >= 2 && !s.Has("absent"))
                        Say(s, "N", "endN_Home", null, _ => { AddIntim(s, "N", 300); if (MeritEvent.Enabled) MeritEvent.Add(Player, 300, "merit_story"); End(s, "N_Home"); });
                    else
                        Tell(s, "endN_Empty", null, _ => End(s, "N_Empty"));
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "N0_puppet":
                    if (!won) s.Set("sealed");
                    Next(s);
                    break;
                case "N1_bandits":
                    if (won) { AddIntim(s, "N", 200); if (MeritEvent.Enabled) MeritEvent.Add(Player, 100, "merit_story"); }
                    Act(s, 2, 12);
                    break;
                case "N3_raid": Act(s, 4, 3); break;
                default: Next(s); break;
            }
        }

        /// <summary>Home: the street brings a gift every year (a herb or a pill).</summary>
        public override void Yearly(StoryState s)
        {
            if (s.Ending == "N_Home") Give(x => x.IsHerbItem() != null, System.Math.Max(1, Player.GetGradeLvl()));
        }

        /// <summary>The neighbour may die: the street goes on (the verdict without her words).</summary>
        public override System.Collections.Generic.IEnumerable<string> VitalRoles => new string[0];
    }
}
