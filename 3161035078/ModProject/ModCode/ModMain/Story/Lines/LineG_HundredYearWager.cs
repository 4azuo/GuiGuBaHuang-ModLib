using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line G "The Hundred-Year Wager" (docs/Taoist/Storylines/G-Hundred-Year-Wager.md): the lifelong rival Lan Qiu {W}.
    /// Acts G0-G5 over ~100 years, each at the later of its year and its realm (Gate). Counts wagers won (wins_p /
    /// wins_w) and debts of life (saved_p / saved_w). Endings Friend / Rival / Surpassed / Grave.
    /// </summary>
    public class LineG_HundredYearWager : StoryLine
    {
        public override string Id => "G";
        public override int Acts => 6;
        public override float MonthlyChance => 1f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole { Name = "W", MaxLived = 0.1f, MinGradeOffset = 0, MaxGradeOffset = 0, OutTraits = new[] { 12, 15 } },
        };

        /// <summary>The first years of a game: Qi Refining or Foundation.</summary>
        public override bool CanOpen(StoryState s) => Player.GetGradeLvl() <= 2;

        public override void OnCast(StoryState s) => U(s, "W")?.SetUnitPos(Player.GetUnitPos());

        public override int TimeLimit(int act) => act == 3 ? 24 : 0;

        /// <summary>G3 late: he went alone.</summary>
        public override void OnLate(StoryState s)
        {
            if (s.Act != 3) return;
            s.Set("refused");
            s.Step = 2;
        }

        /// <summary>He dies of old age (or in a battle of the world): his grave and his disciple.</summary>
        public override void OnRoleDead(StoryState s, string role)
        {
            if (role != "W") return;
            s.Set("W_dead");
            if (s.Act == 0) { Close(s, "dead"); return; }
            Tell(s, "endG_Grave", null, _ =>
            {
                Give(x => x.IsArtifact() != null, Player.GetGradeLvl());
                End(s, "G_Grave");
            });
        }

        public override void Run(StoryState s)
        {
            switch (s.Act * 100 + s.Step)
            {
                // ---- G0 The wager
                case 1:
                    Say(s, "W", "G0_1", new[] { "G0_1_o1", "G0_1_o2", "G0_1_o3" }, i =>
                    {
                        if (i == 0) Spar(s, U(s, "W"), "G0_shove");
                        else Next(s);
                    });
                    break;
                case 2:
                    Say(s, "W", "G0_2", new[] { "G0_2_o1", "G0_2_o2" }, i => { s.Set("bet", i == 0 ? 1 : 0); Next(s); });
                    break;
                case 3:
                    Say(s, "W", "G0_3", null, _ => Act(s, 1, 12));
                    break;

                // ---- G1 First rematch (15 years / Qi Condensation)
                case 101:
                    if (!Gate(s, 15, 3)) return;
                    U(s, "W")?.SetUnitPos(Player.GetUnitPos());
                    Say(s, "W", "G1_1", new[] { "G1_1_o1" }, _ => Spar(s, U(s, "W"), "G1_duel"));
                    break;
                case 102:
                    Say(s, "W", s.Flag("G1_duel") > 0 ? "G1_win" : "G1_lose", null, _ => Next(s));
                    break;
                case 103:
                    Tell(s, "G1_3", new[] { "G1_3_o1" }, _ => Fight(s, Foe(s, false), "G1_bandits"));
                    break;

                // ---- G2 A life owed (30 years / Golden Core): whoever is weaker falls in
                case 201:
                    if (!Gate(s, 30, 4)) return;
                    var w = U(s, "W");
                    if (w != null && w.GetGradeLvl() > Player.GetGradeLvl())
                    {
                        Count(s, "saved_w");
                        Say(s, "W", "G2_you", null, _ => Next(s));
                    }
                    else
                        Tell(s, "G2_he", new[] { "G2_he_o1", "G2_he_o2", "G2_he_o3" }, i =>
                        {
                            if (i == 0) FightBeast(s, "G2_beast");
                            else { if (i == 2) s.Set("walked"); Next(s); }
                        });
                    break;
                case 202:
                    Say(s, "W", "G2_3", null, _ => Act(s, 3, 12));
                    break;

                // ---- G3 His sect burns (45 years / Origin Spirit; 2 years)
                case 301:
                    if (!Gate(s, 45, 5)) return;
                    Say(s, "W", "G3_1", new[] { "G3_1_o1", "G3_1_o2", "G3_1_o3" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, false, 1), "G3_fight");
                        else if (i == 1) { s.Set("helped"); AddIntim(s, "W", 200); Next(s); }
                        else { s.Set("refused"); Next(s); }
                    });
                    break;
                case 302:
                    if (s.Has("refused"))
                    {
                        var wu = U(s, "W");
                        if (wu != null && wu.GetGradeLvl() > 1) wu.AddProperty<int>(UnitPropertyEnum.GradeID, -3);
                        Say(s, "W", "G3_alone", null, _ => Act(s, 4, 12));
                    }
                    else
                        Say(s, "W", "G3_owe", null, _ => Act(s, 4, 12));
                    break;

                // ---- G4 Two old men (70 years / Nascent Soul)
                case 401:
                    if (!Gate(s, 70, 6)) return;
                    Say(s, "W", "G4_1", new[] { "G4_1_o1", "G4_1_o2", "G4_1_o3" }, i =>
                    {
                        if (i == 0) Spar(s, U(s, "W"), "G4_duel");
                        else { if (i == 1) s.Set("disciple"); Next(s); }
                    }, s.Flag("wins_w"), s.Flag("wins_p"));
                    break;
                case 402:
                    Say(s, "W", "G4_3", null, _ => Act(s, 5, 12));
                    break;

                // ---- G5 The last wager (100 years / Soul Formation, or his last years)
                case 501:
                    var last = U(s, "W");
                    var dying = last != null && last.GetProperty<int>(UnitPropertyEnum.Life) - last.GetProperty<int>(UnitPropertyEnum.Age) <= 5 * 12;
                    if (!dying && !Gate(s, 100, 7)) return;
                    Say(s, "W", dying ? "G5_dying" : "G5_1", new[] { "G5_1_o1", "G5_1_o2" }, i =>
                    {
                        if (i == 0) { s.Set("trial"); FightBeast(s, "G5_trial", 5); }
                        else Next(s);
                    });
                    break;
                case 502:
                    Tell(s, "G5_2", null, _ => Ending(s));
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "G0_shove": Next(s); break;
                case "G1_duel":
                case "G4_duel":
                    Count(s, won ? "wins_p" : "wins_w");
                    Next(s);
                    break;
                case "G1_bandits": Act(s, 2, 12); break;
                case "G2_beast": if (won) { Count(s, "saved_p"); AddIntim(s, "W", 200); } Next(s); break;
                case "G3_fight": s.Set("helped"); AddIntim(s, "W", 200); Next(s); break;
                default: Next(s); break;
            }
        }

        private void Ending(StoryState s)
        {
            var w = U(s, "W");
            if (w == null || w.isDie)
                Tell(s, "endG_Grave", null, _ => { Give(x => x.IsArtifact() != null, Player.GetGradeLvl()); End(s, "G_Grave"); });
            else if (s.Flag("saved_p") + s.Flag("saved_w") >= 1 && s.Has("helped"))
                Say(s, "W", "endG_Friend", null, _ => { AddIntim(s, "W", 300); End(s, "G_Friend"); });
            else if (w.GetGradeLvl() > Player.GetGradeLvl())
                Say(s, "W", "endG_Surpassed", null, _ => { Give(x => x.IsPill() != null, Player.GetGradeLvl() + 1); End(s, "G_Surpassed"); });
            else
                Say(s, "W", "endG_Rival", null, _ => End(s, "G_Rival"));
        }

        /// <summary>Eternal Rival: every 10 years a duel letter and a technique lesson (exp).</summary>
        public override void Yearly(StoryState s)
        {
            if (s.Ending != "G_Rival" || (Now - s.Flag("opened")) / 12 % 10 != 0) return;
            Player.AddExp(500 * System.Math.Max(1, Player.GetGradeLvl()));
            TaoistLogEvent.Log("log_story_gift", Name(s, "W"), T(s, "G_rival_letter"));
        }
    }
}
