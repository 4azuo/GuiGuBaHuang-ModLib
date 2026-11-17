using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line C "Exploding Furnace" (docs/Taoist/Storylines/C-Exploding-Furnace.md): Su Xiaoman {X}, apprentice
    /// alchemist; grandmaster {G} steals her recipe. Acts C0-C4, endings MasterAlchemist / Companion / LastExplosion.
    /// Opens line E (Granny Shi) in C1.
    /// </summary>
    public class LineC_ExplodingFurnace : StoryLine
    {
        public override string Id => "C";
        public override float MonthlyChance => 1f;

        private static readonly UnitPropertyEnum[] ARTS =
        {
            UnitPropertyEnum.RefineElixir, UnitPropertyEnum.RefineWeapon, UnitPropertyEnum.Geomancy,
            UnitPropertyEnum.Symbol, UnitPropertyEnum.Herbal, UnitPropertyEnum.Mine,
        };

        private static bool AlchemyFirst(WorldUnitBase u) => ARTS.OrderByDescending(a => u.GetProperty<int>(a)).First() == UnitPropertyEnum.RefineElixir;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole
            {
                Name = "X", Sex = UnitSexType.Woman, MaxLived = 0.15f, InTraits = new[] { 3 }, OutTraits = new[] { 16 },
                MaxGradeOffset = -1, Filter = u => AlchemyFirst(u) && PlayerTown() != null && u.GetMapBuild<MapBuildTown>()?.buildData.id == PlayerTown().buildData.id,
            },
        };

        public override System.Collections.Generic.IEnumerable<string> VitalRoles => new[] { "X" };

        public override bool CanOpen(StoryState s) => PlayerTown() != null;

        public override int TimeLimit(int act) => act == 1 ? 24 : act == 3 ? 12 : 0;

        public override void Run(StoryState s)
        {
            var x = U(s, "X");
            switch (s.Act * 100 + s.Step)
            {
                // ---- C0 Test subject
                case 1:
                    Say(s, "X", "C0_1", new[] { "C0_1_o1", "C0_1_o2" }, i =>
                    {
                        if (i == 0) Next(s);
                        else if (s.Has("backed")) Close(s, "refused twice");
                        else { s.Set("backed"); Wait(s, 1); }
                    });
                    break;
                case 2:
                    Tell(s, "C0_2", null, _ => { Player.AddProperty<int>(UnitPropertyEnum.Mood, 10); Next(s); });
                    break;
                case 3:
                    Say(s, "X", "C0_3", null, _ =>
                    {
                        TaoistLogEvent.Log("log_story_gift", Name(s, "X"), Give(p => p.IsPill() != null, Player.GetGradeLvl()));
                        Act(s, 1, 2);
                    });
                    break;

                // ---- C1 Moonfrost Lotus
                case 101:
                    Say(s, "X", "C1_1", null, _ => Next(s, 1));
                    break;
                case 102:
                    var price = EncounterEvent.Cost(3);
                    Tell(s, "C1_2", new[] { "C1_2_o1", "C1_2_o2", "C1_2_o3" }, i =>
                    {
                        if (i == 0 && Player.GetUnitMoney() >= price) { Player.AddUnitMoney(-price); Next(s); }
                        else if (i == 1 && TakeItem(p => p.IsHerbItem() != null, 1)) Next(s);
                        else Wait(s, 1);
                    }, BG, price);
                    break;
                case 103:
                    Tell(s, "C1_3", new[] { "C1_3_o1" }, _ => FightBeast(s, "C1_snake"));
                    break;
                case 104:
                    Tell(s, "C1_4", null, _ =>
                    {
                        // Granny Shi: line E opens from here
                        StoryEvent.Instance.State("E").Set("fromC");
                        StoryEvent.Instance.Open("E", true);
                        Act(s, 2, 4);
                    });
                    break;
                case 110: // "Failed Exam"
                    Say(s, "X", "C1_failed", new[] { "C1_failed_o1", "C1_failed_o2" }, i =>
                    {
                        if (i == 0 && TakeItem(p => p.IsPill() != null, 1)) { AddIntim(s, "X", 100); Act(s, 2, 12); }
                        else LastExplosion(s);
                    });
                    break;

                // ---- C2 The exam
                case 201:
                    var fee = EncounterEvent.Cost(4);
                    Tell(s, "C2_1", new[] { "C2_1_o1", "C2_1_o2" }, i =>
                    {
                        if (i == 0 && Player.GetUnitMoney() >= fee) { Player.AddUnitMoney(-fee); AddIntim(s, "X", 50); }
                        s.Set("right", 0);
                        Next(s);
                    }, BG, fee);
                    break;
                case 202:
                case 203:
                case 204:
                    var q = s.Step - 1; // 1..3; right answers: small fire, big fire, add the lotus
                    Say(s, "X", "C2_q" + q, new[] { "C2_q" + q + "_o1", "C2_q" + q + "_o2" }, i =>
                    {
                        var right = q == 2 ? 0 : (q == 1 ? 1 : 0);
                        if (i == right) s.Set("right", s.Flag("right") + 1);
                        Next(s);
                    });
                    break;
                case 205:
                    if (s.Flag("right") >= 2) Say(s, "X", "C2_pass", null, _ => { AddIntim(s, "X", 100); Act(s, 3, 6); });
                    else Say(s, "X", "C2_fail", null, _ => Act(s, 2, 12));
                    break;

                // ---- C3 Stolen recipe
                case 301:
                    if (!s.Roles.ContainsKey("G"))
                    {
                        var gm = StoryCasting.Pick(new StoryRole { Name = "G", MinLived = 0.5f, MinGradeOffset = 1, Filter = AlchemyFirst }, StoryEvent.Instance.TakenUnits(), StoryEvent.Units(), Player)
                            ?? StoryCasting.Create(new StoryRole { Name = "G", Sex = UnitSexType.Man, MinLived = 0.6f, MaxLived = 0.7f, MinGradeOffset = 1, MaxGradeOffset = 1 }, Player);
                        if (gm != null) s.Roles["G"] = gm.GetUnitId();
                    }
                    Say(s, "X", "C3_1", null, _ => Next(s));
                    break;
                case 302:
                case 303:
                    var n = s.Step - 1; // apprentice 1 / 2
                    Tell(s, "C3_app" + n, new[] { "C3_app_o1", "C3_app_o2", "C3_app_o3" }, i =>
                    {
                        if (i == 0 && TakeItem(p => p.IsPill() != null || p.IsHerbItem() != null, 1)) { s.Set("proof", s.Flag("proof") + 1); Next(s); }
                        else if (i == 1) Fight(s, Foe(s, null, -1), "C3_bully" + n);
                        else Next(s);
                    });
                    break;
                case 304:
                    var canExpose = s.Flag("proof") >= 2 || Player.GetProperty<int>(UnitPropertyEnum.Reputation) >= 500;
                    Say(s, "X", "C3_4", new[] { "C3_4_o1", "C3_4_o2", "C3_4_o3" }, i =>
                    {
                        if (i == 0 && canExpose) Tell(s, "C3_exposed", null, _ => Act(s, 4, 12));
                        else if (i == 0 || i == 1) Fight(s, U(s, "G"), "C3_duel");
                        else { AddIntim(s, "X", -100); LastExplosion(s); }
                    });
                    break;

                // ---- C4 Grand Gathering
                case 401:
                    Say(s, "G", "C4_1", null, _ => Next(s, 6));
                    break;
                case 402:
                    Tell(s, "C4_2", new[] { "C4_2_o1" }, _ => Fight(s, Foe(s, null), "C4_saboteur"));
                    break;
                case 403:
                    var won = CommonTool.Random(0, 100) < (s.Flag("C4_saboteur") > 0 ? 70 : 40);
                    Tell(s, won ? "C4_3_won" : "C4_3_lost", null, _ =>
                    {
                        if (!won) LastExplosion(s);
                        else if (Intim(s, "X") >= 300) Say(s, "X", "endC_Companion", null, __ => { HirePeopleEvent.Hire(Player, U(s, "X")); End(s, "C_Companion"); });
                        else Say(s, "X", "endC_MasterAlchemist", null, __ => End(s, "C_MasterAlchemist"));
                    });
                    break;
            }
        }

        public override void OnLate(StoryState s)
        {
            if (s.Act == 1) { s.Step = 10; Wait(s, 0); }
            else if (s.Act == 3) { AddIntim(s, "X", -100); LastExplosion(s); }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "C1_snake": if (won) Next(s); else Wait(s, 2); break;
                case "C3_bully1":
                case "C3_bully2": if (won) s.Set("proof", s.Flag("proof") + 1); Next(s); break;
                case "C3_duel": if (won) Act(s, 4, 12); else Wait(s, 6); break;
                default: Next(s); break;
            }
        }

        private void LastExplosion(StoryState s) => Tell(s, "endC_LastExplosion", null, _ => End(s, "C_LastExplosion"));

        /// <summary>Master Alchemist: a pill a year. Last Explosion: food that restores mood.</summary>
        public override void Yearly(StoryState s)
        {
            if (U(s, "X") == null || U(s, "X").isDie) return;
            if (s.Ending == "C_MasterAlchemist")
                TaoistLogEvent.Log("log_story_gift", Name(s, "X"), Give(p => p.IsPill() != null, Player.GetGradeLvl()));
            else if (s.Ending == "C_LastExplosion")
            {
                Player.AddProperty<int>(UnitPropertyEnum.Mood, 20);
                TaoistLogEvent.Log("log_story_tea", Name(s, "X"));
            }
        }
    }
}
