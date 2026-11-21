using ModLib.Helper;
using MOD_nE7UL2.Mod;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line P "A Heartbeat in the Stone" (docs/Taoist/Storylines/P-Sense-Voice.md, spirit sense and body refining): the
    /// spirit sense of the player finds a heartbeat in a tempering place; the body refiner {B} is stuck half-stone in his
    /// body tribulation. Acts P0-P3: the heartbeat, the stone man (help him / take his essence), the training of the
    /// body, his second tribulation. Endings Brothers / Alone / Devourer.
    /// </summary>
    public class LineP_SenseVoice : StoryLine
    {
        public override string Id => "P";
        public override int Acts => 4;
        public override float MonthlyChance => 0.5f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole { Name = "B", Sex = UnitSexType.Man, MinGradeOffset = 0, MaxGradeOffset = 2, MinLived = 0.2f, MaxLived = 0.8f,
                OutTraits = new[] { 8, 15 }, BestBasis = new[] { ModLib.Enum.UnitPropertyEnum.BasisFist, ModLib.Enum.UnitPropertyEnum.BasisPalm } },
        };

        /// <summary>The spirit sense and the body refining on, from grade 3.</summary>
        public override bool CanOpen(StoryState s) => SpiritSenseEvent.Enabled && BodyCulEvent.Enabled && Player.GetGradeLvl() >= 3;

        public override void OnCast(StoryState s) => U(s, "B")?.SetUnitPos(Player.GetUnitPos());

        /// <summary>Body exp of the player (the path taken up when not yet).</summary>
        private static void BodyExp(double share)
        {
            if (!BodyCulEvent.Enabled) return;
            var d = BodyCulEvent.Get(Player) ?? BodyCulEvent.Start(Player);
            if (d == null) return;
            BodyCulEvent.AddExp(Player, BodyCulEvent.NeedExp(System.Math.Max(1, d.Level)) * share);
        }

        public override void Run(StoryState s)
        {
            var grade = System.Math.Max(1, Player.GetGradeLvl());
            switch (s.Act * 100 + s.Step)
            {
                // ---- P0 A heartbeat in the stone
                case 1:
                    Tell(s, "P0_1", new[] { "P0_1_o1", "P0_1_o2" }, i =>
                    {
                        if (i == 0) { SpiritSenseEvent.AddExp(Player, 200, "sense_story"); Act(s, 1); }
                        else Close(s, "ignored");
                    });
                    break;

                // ---- P1 The stone man
                case 101:
                    Say(s, "B", "P1_1", new[] { "P1_1_o1", "P1_1_o2", "P1_1_o3" }, i =>
                    {
                        if (i == 0) { if (!Offer(s, x => x.IsPill() != null, 1, "pill")) return; s.Set("helped"); AddIntim(s, "B", 300); Next(s); }
                        else if (i == 1) { if (!Offer(s, x => x.IsHerbItem() != null, 1, "herb")) return; s.Set("helped"); AddIntim(s, "B", 200); Next(s); }
                        else Fight(s, U(s, "B"), "P1_devour");
                    });
                    break;
                case 102:
                    Say(s, "B", "P1_2", null, _ => Act(s, 2, 6));
                    break;

                // ---- P2 The training of the body
                case 201:
                    Say(s, "B", "P2_1", new[] { "P2_1_o1", "P2_1_o2" }, i =>
                    {
                        if (i == 0) Spar(s, U(s, "B"), "P2_spar");
                        else { BodyExp(0.5); Act(s, 3, 12); }
                    });
                    break;

                // ---- P3 His second tribulation
                case 301:
                    Say(s, "B", "P3_1", new[] { "P3_1_o1", "P3_1_o2" }, i =>
                    {
                        if (i == 0) FightBeast(s, "P3_storm", 3);
                        else Tell(s, "endP_Alone", null, _ => { BodyExp(0.5); End(s, "P_Alone"); });
                    });
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "P1_devour":
                    if (won) Tell(s, "endP_Devourer", null, _ =>
                    {
                        BodyExp(2.0);
                        if (DaoHeartEvent.Enabled) DaoHeartEvent.Add(Player, -300, "heart_story");
                        if (MeritEvent.Enabled) MeritEvent.Add(Player, -500, "merit_story");
                        End(s, "P_Devourer");
                    });
                    else Wait(s, 6);
                    break;
                case "P2_spar": BodyExp(1.0); AddIntim(s, "B", 100); Act(s, 3, 12); break;
                case "P3_storm":
                    if (won) Say(s, "B", "endP_Brothers", null, _ => { BodyExp(1.5); AddIntim(s, "B", 500); End(s, "P_Brothers"); });
                    else Wait(s, 6);
                    break;
                default: Next(s); break;
            }
        }
    }
}
