using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line I "The Sealed Tomb" (docs/Taoist/Storylines/I-Sealed-Tomb.md): Elder Gu {Q}, opens at Golden Core outside a
    /// town. Acts I0-I4: the map, the team, the outer halls (2 years), Gu's betrayal, the tomb owner. Endings Heir /
    /// FreedSoul / Sealer / Looted. The legacy taken comes back in line L (the heart demon's temptation).
    /// </summary>
    public class LineI_SealedTomb : StoryLine
    {
        public override string Id => "I";
        public override float MonthlyChance => 0.5f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole { Name = "Q", Sex = UnitSexType.Man, MinLived = 0.6f, MinGradeOffset = -1, MaxGradeOffset = 1, OutTraits = new[] { 16 } },
        };

        public override bool CanOpen(StoryState s) => Player.GetGradeLvl() >= 4 && PlayerTown() == null;

        public override void OnCast(StoryState s) => U(s, "Q")?.SetUnitPos(Player.GetUnitPos());

        public override int TimeLimit(int act) => act == 1 ? 12 : act == 2 ? 24 : 0;

        /// <summary>I1 late: he goes with the player alone; I2 late: another sect opened the tomb first.</summary>
        public override void OnLate(StoryState s)
        {
            if (s.Act == 2) { s.Set("looted"); Tell(s, "endI_Looted", null, _ => { Give(x => x.IsHerbItem() != null, Player.GetGradeLvl()); End(s, "I_Looted"); }); }
        }

        /// <summary>Gu dies before the betrayal: the line goes on without him (the tomb owner at the end).</summary>
        public override void OnRoleDead(StoryState s, string role)
        {
            s.Set("Q_dead");
            if (s.Act < 3) Close(s, "dead");
        }

        public override void Run(StoryState s)
        {
            switch (s.Act * 100 + s.Step)
            {
                // ---- I0 A map in a corpse
                case 1:
                    Say(s, "Q", "I0_1", new[] { "I0_1_o1", "I0_1_o2" }, i =>
                    {
                        if (i == 0) AddIntim(s, "Q", 100); else { s.Set("rival"); AddIntim(s, "Q", -100); }
                        Act(s, 1, 3);
                    });
                    break;

                // ---- I1 The team (1 year)
                case 101:
                    Say(s, "Q", "I1_1", new[] { "I1_1_o1", "I1_1_o2" }, i =>
                    {
                        if (i == 0)
                        {
                            var team = HirePeopleEvent.Instance?.TeamData.TryGetValue(Player.GetUnitId(), out var t) == true ? t.Count : 0;
                            if (!Require(s, team >= 2, "need_team", null, team)) return;
                            s.Set("team");
                        }
                        Act(s, 2, 3);
                    });
                    break;

                // ---- I2 The outer halls (2 years)
                case 201:
                    Tell(s, "I2_1", new[] { "I2_1_o1" }, _ => FightBeast(s, "I2_puppets", 3));
                    break;
                case 202:
                    Tell(s, "I2_2", new[] { "I2_2_o1", "I2_2_o2", "I2_2_o3" }, i =>
                    {
                        if (i == 1) s.Set("riddle");
                        else Player.AddProperty<int>(UnitPropertyEnum.Hp, -System.Math.Max(1, Player.GetProperty<int>(UnitPropertyEnum.Hp) / 5));
                        Next(s);
                    });
                    break;
                case 203:
                    Tell(s, "I2_3", new[] { "I2_3_o1", "I2_3_o2" }, i =>
                    {
                        if (i == 0) { s.Set("poison"); Give(x => x.IsArtifact() != null, Player.GetGradeLvl()); }
                        Act(s, 3, 1);
                    });
                    break;

                // ---- I3 The knife in the back
                case 301:
                    var q = U(s, "Q");
                    if (q == null || q.isDie) { Act(s, 4); return; }
                    q.SetUnitPos(Player.GetUnitPos());
                    Say(s, "Q", s.Has("team") ? "I3_team" : "I3_1", new[] { "I3_1_o1" }, _ => Fight(s, q, "I3_gu"));
                    break;

                // ---- I4 The tomb owner
                case 401:
                    Tell(s, "I4_1", new[] { "I4_1_o1", "I4_1_o2", "I4_1_o3" }, i =>
                    {
                        if (i == 0) Tell(s, "endI_Heir", null, _ => { HeirBonus(); End(s, "I_Heir"); });
                        else if (i == 1) Tell(s, "endI_FreedSoul", null, _ => End(s, "I_FreedSoul"));
                        else Tell(s, "endI_Sealer", null, _ =>
                        {
                            Player.AddProperty<int>(UnitPropertyEnum.Reputation, 500);
                            End(s, "I_Sealer");
                        });
                    });
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            if (tag == "I3_gu" && !won)
            {
                s.Set("looted");
                Tell(s, "endI_Looted", null, _ => { Give(x => x.IsHerbItem() != null, Player.GetGradeLvl()); End(s, "I_Looted"); });
                return;
            }
            if (tag == "I3_gu") { Act(s, 4); return; }
            Next(s);
        }

        /// <summary>Heir of the Tomb: +5 to the player's best weapon basis.</summary>
        private static void HeirBonus()
        {
            var best = StoryRole.WEAPON_BASES.OrderByDescending(b => Player.GetProperty<int>(b)).First();
            Player.AddProperty<int>(best, 5);
        }

        /// <summary>Freed Soul: a yearly counsel (a pill of the player's grade).</summary>
        public override void Yearly(StoryState s)
        {
            if (s.Ending != "I_FreedSoul") return;
            var item = Give(x => x.IsPill() != null, Player.GetGradeLvl());
            TaoistLogEvent.Log("log_story_gift", T(s, "I_owner"), item);
        }
    }
}
