using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line J "The Cursed Disciple" (docs/Taoist/Storylines/J-Cursed-Disciple.md): the boy Xiao Ye {J} with a demon mark,
    /// opens at Origin Spirit in a town. Acts J0-J5: the execution ground, 3 lessons (mercy / strength / freedom), the
    /// mark wakes, the hunters, the heart of the boy, the last duel. Endings Righteous / Demon / Sealed. Line H: when
    /// Mei Niang's inn stands, he can hide there (J2), and a sealed Xiao Ye becomes an innkeeper there.
    /// </summary>
    public class LineJ_CursedDisciple : StoryLine
    {
        public override string Id => "J";
        public override int Acts => 6;
        public override float MonthlyChance => 0.5f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole { Name = "J", Sex = UnitSexType.Man, MaxLived = 0.06f, Filter = u => u.GetGradeLvl() <= 2 },
        };

        public override bool CanOpen(StoryState s) => Player.GetGradeLvl() >= 5 && PlayerTown() != null;

        public override void OnCast(StoryState s) => U(s, "J")?.SetUnitPos(Player.GetUnitPos());

        public override int TimeLimit(int act) => act == 2 ? 12 : act == 3 ? 24 : 0;

        /// <summary>The inn of line H stands: a place to hide him.</summary>
        private static bool Inn => LineH_RoadsideInn.InnStands(StoryEvent.Instance.State("H"));

        public override void Run(StoryState s)
        {
            switch (s.Act * 100 + s.Step)
            {
                // ---- J0 The execution ground
                case 1:
                    var price = EncounterEvent.Cost(20);
                    Tell(s, "J0_1", new[] { "J0_1_o1", "J0_1_o2", "J0_1_o3" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, true), "J0_save");
                        else if (i == 1) { if (!Pay(s, price)) return; Next(s); }
                        else Close(s, "watched");
                    }, BG, price);
                    break;
                case 2:
                    Say(s, "J", "J0_2", null, _ => Act(s, 1, 6));
                    break;

                // ---- J1 First lessons: 3 moments, a year apart
                case 101:
                case 102:
                case 103:
                    U(s, "J")?.SetUnitPos(Player.GetUnitPos());
                    var n = s.Step;
                    Say(s, "J", $"J1_{n}", new[] { "J1_o1", "J1_o2", "J1_o3" }, i =>
                    {
                        Count(s, i == 0 ? "mercy" : i == 1 ? "strength" : "freedom");
                        AddIntim(s, "J", 100);
                        if (n < 3) Next(s, 12); else Act(s, 2, 12);
                    });
                    break;

                // ---- J2 The mark wakes (1 year)
                case 201:
                    var opts = Inn ? new[] { "J2_1_o1", "J2_1_o2", "J2_1_o3", "J2_1_inn" } : new[] { "J2_1_o1", "J2_1_o2", "J2_1_o3" };
                    Tell(s, "J2_1", opts, i =>
                    {
                        if (i == 0) { if (!Require(s, TakeItem(x => x.IsPill() != null, 1), "no_pill", null)) return; s.Set("suppressed"); Act(s, 3, 12); }
                        else if (i == 1) Spar(s, U(s, "J"), "J2_fight");
                        else if (i == 3) { s.Set("inn"); Act(s, 3, 12); }
                        else { Count(s, "strength"); Act(s, 3, 12); }
                    });
                    break;

                // ---- J3 The hunters (2 years)
                case 301:
                    Tell(s, s.Has("inn") ? "J3_inn" : "J3_1", new[] { "J3_1_o1" }, _ => Fight(s, Foe(s, true, 1), "J3_hunters"));
                    break;
                case 302:
                    Say(s, "J", "J3_2", null, _ => Act(s, 4, 12));
                    break;

                // ---- J4 The heart of the boy: the lessons decide
                case 401:
                    var demon = s.Flag("strength") > s.Flag("mercy") + s.Flag("freedom");
                    if (demon) s.Set("demon");
                    Say(s, "J", demon ? "J4_demon" : "J4_1", null, _ => Act(s, 5, 6));
                    break;

                // ---- J5 The student surpasses
                case 501:
                    U(s, "J")?.SetUnitPos(Player.GetUnitPos());
                    Say(s, "J", s.Has("demon") ? "J5_demon" : "J5_1", new[] { "J5_1_o1", "J5_1_o2", "J5_1_o3" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, true, 1), "J5_master");
                        else if (i == 1) Spar(s, U(s, "J"), "J5_duel");
                        else Tell(s, Inn ? "endJ_SealedInn" : "endJ_Sealed", null, _ => End(s, "J_Sealed"));
                    });
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "J0_save": if (won) Next(s); else Close(s, "lost"); break;
                case "J2_fight": Count(s, "mercy"); Act(s, 3, 12); break;
                case "J5_master":
                case "J5_duel":
                    if (s.Has("demon") && !(tag == "J5_duel" && won))
                        Say(s, "J", "endJ_Demon", null, _ => End(s, "J_Demon"));
                    else
                        Say(s, "J", "endJ_Righteous", null, _ => { AddIntim(s, "J", 300); End(s, "J_Righteous"); });
                    break;
                default: Next(s); break;
            }
        }
    }
}
