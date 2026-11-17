using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line E "Granny's Herb Garden" (docs/Taoist/Storylines/E-Herb-Garden.md): Granny Shi {O}, the orphan A Li {Y}
    /// (a child record until 16, then an NPC). Acts E0-E4, endings Successor / LostChild / GardenOfGenerations.
    /// The garden is a place on the world map (where she met the player) and a manual seclusion there with intimacy ≥ 300
    /// with Granny Shi (SeclusionEvent.Kind.Garden; quest "Granny Shi's garden" guides the player there).
    /// </summary>
    public class LineE_HerbGarden : StoryLine
    {
        public override string Id => "E";
        public const int GARDEN_INTIM = 300;
        public const int MAX_LESSONS = 5;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole { Name = "O", Sex = UnitSexType.Woman, MinLived = 0.75f, MaxLived = 0.93f, InTraits = new[] { 3, 1 }, OutTraits = new[] { 18 }, NoSect = true, MaxGradeOffset = 1 },
        };

        /// <summary>Opened by line C (the lotus valley) or met outside towns.</summary>
        public override bool CanOpen(StoryState s) => PlayerTown() == null;

        /// <summary>Granny's death is the last act, not the end of the line.</summary>
        public override System.Collections.Generic.IEnumerable<string> VitalRoles => new[] { "O" };

        /// <summary>A Li's birth month (a child record).</summary>
        private static int Born(StoryState s) => s.Flag("Y_born");
        private static int AgeOf(StoryState s) => (Now - Born(s)) / 12;

        public override void Run(StoryState s)
        {
            switch (s.Act * 100 + s.Step)
            {
                // ---- E0 Mountain garden
                case 1:
                    // her garden: here, outside towns (fixed for the game; StoryEvent.GardenPoint)
                    StoryEvent.SetGardenPoint(s, Player.GetUnitPos());
                    U(s, "O")?.SetUnitPos(Player.GetUnitPos());
                    Say(s, "O", "E0_1", new[] { "E0_1_o1", "E0_1_o2" }, i =>
                    {
                        if (i == 0) Next(s); else Close(s, "refused");
                    });
                    break;
                case 2:
                    Tell(s, "E0_2", new[] { "E0_2_o1" }, _ => FightBeast(s, "E0_monkeys", -5));
                    break;
                case 3:
                    Say(s, "O", "E0_3", null, _ => { Lesson(s, 1); Act(s, 1, 12); });
                    break;

                // ---- E1 Child in the ashes
                case 101:
                    Tell(s, "E1_1", null, _ => Next(s));
                    break;
                case 102:
                    Say(s, "O", "E1_2", null, _ => Next(s));
                    break;
                case 103:
                    Say(s, "O", "E1_3", new[] { "E1_3_o1", "E1_3_o2", "E1_3_o3" }, i =>
                    {
                        s.Set("Y_born", Now - 8 * 12);
                        s.Set("Y_path", i); // 0 adopted, 1 sect, 2 granny
                        if (i == 0)
                        {
                            var spouse = FamilyEvent.Unit(FamilyEvent.Spouse(Player));
                            var father = FamilyEvent.IsMan(Player) ? Player : spouse;
                            var mother = FamilyEvent.IsMan(Player) ? spouse : Player;
                            var c = new FamilyEvent.Child
                            {
                                FatherId = father?.GetUnitId(), MotherId = mother?.GetUnitId(),
                                FatherName = father?.GetName(), MotherName = mother?.GetName(), BirthMonth = Born(s),
                            };
                            FamilyEvent.Instance?.Children.Add(c);
                            s.Roles["Yrec"] = c.Id;
                        }
                        Lesson(s, 1);
                        Act(s, 2, 1);
                    });
                    break;

                // ---- E2 A Li grows up (8, 12, 15)
                case 201:
                    Tell(s, "E2_8", new[] { "E2_8_o1", "E2_8_o2" }, i => { s.Set("Y_alch", s.Flag("Y_alch") + (i == 0 ? 1 : 0)); s.Set("Y_sword", s.Flag("Y_sword") + (i == 1 ? 1 : 0)); Next(s, 48); });
                    break;
                case 202:
                    Tell(s, "E2_12", new[] { "E2_12_o1", "E2_12_o2", "E2_12_o3" }, i => { if (i == 2) s.Set("Y_sword", s.Flag("Y_sword") + 1); else if (i == 1) s.Set("Y_free"); Lesson(s, 1); Next(s, 36); });
                    break;
                case 203:
                    Tell(s, "E2_15", new[] { "E2_15_o1", "E2_15_o2" }, i => { if (i == 0) s.Set("Y_truth"); Act(s, 3, 1); });
                    break;

                // ---- E3 Granny falls ill (≤ 3 years of life left)
                case 301:
                    var o = U(s, "O");
                    if (o != null && o.GetProperty<int>(UnitPropertyEnum.Life) - o.GetProperty<int>(UnitPropertyEnum.Age) > 36) return;
                    Tell(s, "E3_1", null, _ => Next(s));
                    break;
                case 302:
                    Tell(s, "E3_2", new[] { "E3_2_o1", "E3_2_o2" }, i =>
                    {
                        if (i == 0) FightBeast(s, "E3_herb", 5);
                        else Next(s);
                    });
                    break;
                case 303:
                    Say(s, "O", "E3_3", s.Flag("E3_herb") > 0 ? new[] { "E3_3_o1", "E3_3_o2" } : new[] { "E3_3_o2" }, i =>
                    {
                        if (s.Flag("E3_herb") > 0 && i == 0)
                        {
                            U(s, "O")?.AddProperty<int>(UnitPropertyEnum.Life, 120);
                            AddIntim(s, "O", 200);
                        }
                        else Lesson(s, 3);
                        Act(s, 4, 1);
                    });
                    break;

                // ---- E4 Last wish (after Granny's death, OnRoleDead)
                case 401:
                    if (!s.Has("O_dead")) return; // waits for her death
                    Tell(s, "E4_1", null, _ => Next(s));
                    break;
                case 402:
                    Tell(s, "E4_2", new[] { "E4_2_o1", "E4_2_o2" }, i => { s.Set("give_Y", i); Next(s); });
                    break;
                case 403:
                    Ending(s);
                    break;
            }
        }

        /// <summary>Granny dies: her intimacy is kept (the garden), the last act follows.</summary>
        public override void OnRoleDead(StoryState s, string role)
        {
            s.Set("O_dead");
            s.Set("O_intim", Intim(s, "O"));
            if (s.Act < 4) { s.Set("O_early"); Act(s, 4, 0); }
            else Wait(s, 0);
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            if (tag == "E0_monkeys") AddIntim(s, "O", 100);
            if (tag == "E3_herb" && !won) { Wait(s, 3); return; }
            Next(s);
        }

        /// <summary>+1 random basis by lesson, at most MAX_LESSONS; intimacy +50 each.</summary>
        private void Lesson(StoryState s, int count)
        {
            var bases = StoryRole.WEAPON_BASES;
            for (var i = 0; i < count && s.Flag("lessons") < MAX_LESSONS + 2; i++)
            {
                s.Set("lessons", s.Flag("lessons") + 1);
                Player.AddProperty<int>(bases[CommonTool.Random(0, bases.Length * 100) % bases.Length], 1);
            }
            AddIntim(s, "O", 50);
        }

        private void Ending(StoryState s)
        {
            var lost = s.Flag("Y_path") == 2 && s.Has("O_early");
            if (lost)
                Tell(s, "endE_LostChild", null, _ => End(s, "E_LostChild"));
            else if (s.Flag("give_Y") == 1)
                Tell(s, "endE_Successor", null, _ => End(s, "E_Successor"));
            else
                Tell(s, GardenOpen(s) ? "endE_Garden" : "endE_Keepsake", null, _ => End(s, "E_GardenOfGenerations"));
        }

        /// <summary>The garden seclusion is open: intimacy with Granny ≥ 300 (at her death once she died).</summary>
        public static bool GardenOpen(StoryState s)
        {
            if (s == null || !(s.Open || s.Ending == "E_GardenOfGenerations")) return false;
            var o = U(s, "O");
            var intim = s.Has("O_dead") ? s.Flag("O_intim") : (o == null ? 0 : o.data.unitData.relationData.GetIntim(Player.GetUnitId()));
            return intim >= GARDEN_INTIM && s.Act >= 0;
        }

        /// <summary>A Li becomes an NPC at 16 (adopted: FamilyEvent grows her up; otherwise created here); the ending's lesson.</summary>
        public override void Yearly(StoryState s)
        {
            if (s.Ending == "E_GardenOfGenerations" && !s.Has("gen_lesson_" + (FamilyEvent.Instance?.Generation ?? 1)))
            {
                s.Set("gen_lesson_" + (FamilyEvent.Instance?.Generation ?? 1));
                Lesson(s, 1);
                TaoistLogEvent.Log("log_story_lesson");
            }
        }

        /// <summary>Monthly from StoryEvent: A Li comes of age.</summary>
        public static void GrowUpALi(StoryState s)
        {
            if (!s.Has("Y_born") || s.Roles.ContainsKey("Y") || AgeOf(s) < FamilyEvent.ADULT_YEARS) return;
            WorldUnitBase y = null;
            if (s.Roles.TryGetValue("Yrec", out var rid))
            {
                var c = FamilyEvent.Instance?.Children.FirstOrDefault(x => x.Id == rid);
                if (c == null || !c.Grown) return; // FamilyEvent grows her up
                y = FamilyEvent.Unit(c.UnitId);
            }
            else
            {
                y = StoryCasting.Create(new StoryRole
                {
                    Name = "Y", Sex = UnitSexType.Woman, MinLived = 0.1f, MaxLived = 0.12f, MaxGradeOffset = -1,
                    Righteous = s.Has("O_early") && s.Flag("Y_path") == 2 ? false : (bool?)null,
                }, Player);
            }
            if (y == null) return;
            s.Roles["Y"] = y.GetUnitId();
            y.SetProperty<int>(UnitPropertyEnum.Age, FamilyEvent.ADULT_YEARS * 12);
            if (s.Flag("Y_alch") > s.Flag("Y_sword")) y.AddProperty<int>(UnitPropertyEnum.RefineElixir, 50);
            else y.AddProperty<int>(UnitPropertyEnum.BasisSword, 20);
            if (s.Flag("Y_path") == 1)
            {
                var sect = (ModMain.Store?.Schools ?? new MapBuildSchool[0]).FirstOrDefault();
                if (sect != null) y.CreateAction(new UnitActionSchoolJoin(sect.buildData.id, SchoolPostType.Out, SchoolDepartmentType.None));
            }
            TaoistLogEvent.Log("log_story_ali", y.GetName());
        }
    }
}
