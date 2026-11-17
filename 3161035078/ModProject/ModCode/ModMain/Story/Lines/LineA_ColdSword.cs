using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line A "Cold Sword" (docs/Taoist/Storylines/A-Cold-Sword.md): Yue Han {A}, exiled swordswoman of the sect {S},
    /// framed by her master {M}; Old Lu {L} the witness. Acts A0-A5, endings Companion / SectMaster / Peace / Fallen /
    /// Sacrifice / Traitor.
    /// </summary>
    public class LineA_ColdSword : StoryLine
    {
        public override string Id => "A";
        public override int Acts => 6;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole
            {
                Name = "A", Sex = UnitSexType.Woman, MinLived = 0.15f, MaxLived = 0.35f, MinBeauty = 250,
                InTraits = new[] { 2, 1 }, OutTraits = new[] { 15, 19 }, Righteous = true,
                BestBasis = new[] { UnitPropertyEnum.BasisSword }, MinGradeOffset = 0, MaxGradeOffset = 1, InSect = true,
            },
        };

        /// <summary>The master and Old Lu are cast later: only Yue Han matters for the line.</summary>
        public override System.Collections.Generic.IEnumerable<string> VitalRoles => new[] { "A" };

        public override bool CanOpen(StoryState s) => Player.GetGradeLvl() >= 2 && PlayerTown() == null;

        public override void OnCast(StoryState s)
        {
            var a = U(s, "A");
            var sect = a.data.unitData.schoolID;
            s.Roles["S"] = sect;
            // her master: the strongest member of her sect
            var m = StoryEvent.Units().Where(x => x.data.unitData.schoolID == sect && x.GetUnitId() != a.GetUnitId() && !StoryEvent.Instance.TakenUnits().Contains(x.GetUnitId()))
                .OrderByDescending(x => x.GetGradeLvl()).FirstOrDefault()
                ?? Foe(s, true, 1);
            if (m != null) s.Roles["M"] = m.GetUnitId();
        }

        public override int TimeLimit(int act) => act == 1 ? 12 : act == 2 ? 36 : act == 3 ? 24 : act == 5 ? 24 : 0;

        public override void Run(StoryState s)
        {
            switch (s.Act)
            {
                case 0: Act0(s); break;
                case 1: Act1(s); break;
                case 2: Act2(s); break;
                case 3: Act3(s); break;
                case 4: Act4(s); break;
                case 5: Act5(s); break;
                case 9: // abandoned in "Broken Sword": she escapes years later, fallen
                    Say(s, "A", "A_fallen", null, _ => Fallen(s));
                    break;
            }
        }

        private void Act0(StoryState s)
        {
            switch (s.Step)
            {
                case 1:
                    Say(s, "A", "A0_1", new[] { "A0_1_o1", "A0_1_o2", "A0_1_o3" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, null, 0, s.Roles["S"]), "A0_fight");
                        else if (i == 1) Tell(s, "A0_handed", null, _ => Traitor(s));
                        else if (s.Has("walked")) Close(s, "walked away twice");
                        else { s.Set("walked"); Act(s, 0, 36); }
                    });
                    break;
                case 2:
                    Say(s, "A", s.Flag("A0_fight") > 0 ? "A0_2" : "A0_2_lost", null, _ =>
                    {
                        AddIntim(s, "A", s.Flag("A0_fight") > 0 ? 100 : 50);
                        s.Set("silk");
                        Next(s, CommonTool.Random(1, 4));
                    });
                    break;
                case 3:
                    var town = PlayerTown();
                    if (town == null) return; // waits for a town
                    Tell(s, "A0_3", new[] { "A0_3_o1", "A0_3_o2" }, i =>
                    {
                        if (i == 0)
                        {
                            var master = MapBuildPropertyEvent.GetTownMaster(town);
                            if (master != null && !master.IsPlayer())
                                FamilyEvent.ChangeIntim(master.data.unitData.relationData, Player.GetUnitId(), -20);
                        }
                        Act(s, 1, 6);
                    });
                    break;
            }
        }

        private void Act1(StoryState s)
        {
            var grade = U(s, "A")?.GetGradeLvl() ?? 1;
            switch (s.Step)
            {
                case 1:
                    Say(s, "A", "A1_1", null, _ => Next(s));
                    break;
                case 2:
                    Say(s, "A", "A1_2", new[] { "A1_2_o1", "A1_2_o2", "A1_2_o3" }, i =>
                    {
                        var ok = i == 0 ? TakeItem(x => x.IsPill() != null, grade)
                            : i == 1 && TakeItem(x => x.IsHerbItem() != null, grade);
                        if (ok) Next(s);
                        else Tell(s, i == 2 ? "A1_2_search" : "A1_2_none", null, _ => Wait(s, 1));
                    }, grade);
                    break;
                case 3:
                    Say(s, "A", "A1_3", null, _ => Act(s, 2, 6));
                    break;
            }
        }

        public override void OnLate(StoryState s)
        {
            switch (s.Act)
            {
                case 1:
                    AddIntim(s, "A", -100);
                    Tell(s, "A1_late", null, _ => Act(s, 2, 3));
                    break;
                case 2: // "Broken Sword"
                    s.Set("jailed");
                    s.Step = 10;
                    Wait(s, 0);
                    break;
                case 3:
                    if (!s.Has("witness")) { s.Step = 10; Wait(s, 0); }
                    break;
                case 5:
                    Say(s, "A", "A5_late", null, _ => Fallen(s));
                    break;
            }
        }

        private void Act2(StoryState s)
        {
            switch (s.Step)
            {
                case 1:
                    // the town where the letter went astray
                    var area = Player.GetUnitPosAreaId();
                    var towns = (ModMain.Store?.Towns ?? new MapBuildTown[0]).Where(t => t != null && System.Math.Abs(t.gridData.areaBaseID - area) <= 1).ToList();
                    var town = towns.Count == 0 ? null : towns[CommonTool.Random(0, towns.Count * 100) % towns.Count];
                    if (town != null) s.Roles["T"] = town.buildData.id;
                    s.Set("robber", CommonTool.Random(0, 2));
                    Say(s, "A", "A2_1", null, _ => Next(s, 1));
                    break;
                case 2:
                    if (!InTown(s, "T") && s.Roles.ContainsKey("T")) return; // waits for the player in {T}
                    if (s.Flag("robber") == 1)
                        Tell(s, "A2_2_robber", new[] { "A2_2_fight" }, _ => Fight(s, Foe(s, false), "A2_robber"));
                    else
                    {
                        var price = EncounterEvent.Cost(5);
                        Tell(s, "A2_2_market", new[] { "A2_2_buy", "A2_2_later" }, i =>
                        {
                            if (i == 0 && Player.GetUnitMoney() >= price) { Player.AddUnitMoney(-price); Next(s); }
                            else Wait(s, 1);
                        }, BG, price);
                    }
                    break;
                case 3:
                    Say(s, "A", "A2_3", new[] { "A2_3_o1", "A2_3_o2" }, i =>
                    {
                        s.Set("letter");
                        if (i == 1) s.Set("lady");
                        Act(s, 3, i == 1 ? 12 : 3);
                    });
                    break;
                case 10: // "Broken Sword": jailed
                    Tell(s, "A2_jailed", new[] { "A2_jailed_o1", "A2_jailed_o2" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, null, 1, s.Roles["S"]), "A2_rescue");
                        else { s.Set("abandoned"); Act(s, 9, 60); }
                    });
                    break;
            }
        }

        private void Act3(StoryState s)
        {
            switch (s.Step)
            {
                case 1:
                    if (!s.Roles.ContainsKey("L"))
                    {
                        var lu = StoryCasting.Pick(new StoryRole { Name = "L", Sex = UnitSexType.Man, MinLived = 0.6f }, StoryEvent.Instance.TakenUnits(), StoryEvent.Units(), Player)
                            ?? StoryCasting.Create(new StoryRole { Name = "L", Sex = UnitSexType.Man, MinLived = 0.7f, MaxLived = 0.8f, MaxGradeOffset = 0 }, Player);
                        if (lu != null) s.Roles["L"] = lu.GetUnitId();
                    }
                    Say(s, "A", "A3_1", null, _ => Next(s, 2));
                    break;
                case 2:
                    Tell(s, "A3_2", new[] { "A3_2_o1" }, _ => Fight(s, Foe(s, false), "A3_killers"));
                    break;
                case 3:
                    Say(s, "L", "A3_3", null, _ => { s.Set("witness"); Act(s, 4, CommonTool.Random(60, 121)); });
                    break;
                case 10: // the witness died: the treasury ledger
                    Tell(s, "A3_ledger", new[] { "A3_ledger_o1", "A3_ledger_o2" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, null, 1, s.Roles["S"]), "A3_ledger");
                        else Wait(s, 1);
                    });
                    break;
            }
        }

        private void Act4(StoryState s)
        {
            var a = U(s, "A");
            switch (s.Step)
            {
                case 1:
                    Say(s, "A", "A4_1", new[] { "A4_1_o1", "A4_1_o2" }, i =>
                    {
                        if (i == 0) Spar(s, a, "A4_spar");
                        else Wait(s, 3);
                    });
                    break;
                case 2:
                    var price = EncounterEvent.Cost(20);
                    Say(s, "A", "A4_2", new[] { "A4_2_o1", "A4_2_o2", "A4_2_o3" }, i =>
                    {
                        if (i == 0 && TakeItem(x => x.IsArtifact() != null, a?.GetGradeLvl() ?? 1)) { s.Set("sword"); AddIntim(s, "A", 100); }
                        else if (i == 1 && Player.GetUnitMoney() >= price) { Player.AddUnitMoney(-price); s.Set("sword"); AddIntim(s, "A", 50); }
                        Next(s, 12);
                    }, price);
                    break;
                case 3:
                    Tell(s, "A4_3", null, _ =>
                    {
                        var next = a?.GetNextPhaseConf();
                        if (next != null) { a.SetProperty<int>(UnitPropertyEnum.GradeID, next.id); a.ClearExp(); }
                        Act(s, 5);
                    });
                    break;
            }
        }

        private void Act5(StoryState s)
        {
            switch (s.Step)
            {
                case 1:
                    Say(s, "A", "A5_1", new[] { "A5_1_o1", "A5_1_o2", "A5_1_o3" }, i =>
                    {
                        if (i == 0) Next(s, 1);
                        else if (i == 1) { if (Intim(s, "A") >= 300) Peace(s); else Fallen(s); }
                        else Traitor(s);
                    });
                    break;
                case 2:
                    var proof = s.Has("letter") && (s.Has("witness") || s.Flag("A3_ledger") > 0);
                    Say(s, "M", "A5_2", new[] { "A5_2_o1", "A5_2_o2" }, i =>
                    {
                        if (i == 0 && proof) Fight(s, U(s, "M"), "A5_master");
                        else Fight(s, Foe(s, null, 1, s.Roles["S"]), "A5_guard");
                    });
                    break;
                case 3:
                    Say(s, "A", "A5_3", new[] { "A5_3_o1", "A5_3_o2" }, _ =>
                    {
                        if (Intim(s, "A") >= 500) Companion(s); else SectMaster(s);
                    });
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "A0_fight": Next(s); break; // won or lost, she thanks the player (step 2)
                case "A2_robber": if (won) Next(s); else Wait(s, 1); break;
                case "A2_rescue":
                    if (won) Act(s, 3, 1);
                    else { s.Set("abandoned"); Act(s, 9, 60); }
                    break;
                case "A3_killers": if (won) Next(s); else { s.Set("lu_dead"); s.Step = 10; Wait(s, 1); } break;
                case "A3_ledger": if (won) Act(s, 4, CommonTool.Random(60, 121)); else Wait(s, 3); break;
                case "A4_spar":
                    if (won) { AddIntim(s, "A", 50); U(s, "A")?.AddExp(U(s, "A").GetGradeLvl() * 1000); }
                    Next(s);
                    break;
                case "A5_guard": if (won) Fight(s, U(s, "M"), "A5_master"); else Lost(s); break;
                case "A5_master": if (won) Next(s); else Lost(s); break;
                default: Next(s); break;
            }
        }

        /// <summary>A5 lost: with a deep bond she takes the blow (Sacrifice); otherwise once more 3 years later, then fallen.</summary>
        private void Lost(StoryState s)
        {
            if (Intim(s, "A") >= 500) { Sacrifice(s); return; }
            if (s.Has("lost_once")) { Fallen(s); return; }
            s.Set("lost_once");
            Tell(s, "A5_lost", null, _ => Act(s, 5, 36));
        }

        // ------------------------------------------------------------------ endings

        private void Companion(StoryState s) => Say(s, "A", "endA_Companion", new[] { "endA_Companion_o1", "endA_Companion_o2" }, i =>
        {
            if (i == 0 && string.IsNullOrEmpty(FamilyEvent.Spouse(Player)) && string.IsNullOrEmpty(FamilyEvent.Spouse(U(s, "A"))))
                FamilyEvent.SetMarried(Player, U(s, "A"));
            End(s, "A_Companion");
        });

        private void SectMaster(StoryState s) => Say(s, "A", "endA_SectMaster", null, _ => End(s, "A_SectMaster"));
        private void Peace(StoryState s) => Say(s, "A", "endA_Peace", null, _ => End(s, "A_Peace"));
        private void Traitor(StoryState s) => End(s, "A_Traitor");

        private void Fallen(StoryState s)
        {
            var a = U(s, "A");
            if (a != null)
            {
                a.SetProperty<int>(UnitPropertyEnum.StandUp, 0);
                a.SetProperty<int>(UnitPropertyEnum.StandDown, 500);
                AddIntim(s, "A", -300);
            }
            Say(s, "A", "endA_Fallen", null, _ => End(s, "A_Fallen"));
        }

        private void Sacrifice(StoryState s) => Tell(s, "endA_Sacrifice", null, _ =>
        {
            var a = U(s, "A");
            var sword = Give(x => x.IsArtifact() != null, a?.GetGradeLvl() ?? Player.GetGradeLvl());
            a?.Die();
            TaoistLogEvent.Log("log_story_item", sword);
            End(s, "A_Sacrifice");
        });

        /// <summary>Yearly perks: Sect Master a pill, Peace a sword, Fallen / Traitor a hunt every 3-5 years.</summary>
        public override void Yearly(StoryState s)
        {
            switch (s.Ending)
            {
                case "A_SectMaster":
                    TaoistLogEvent.Log("log_story_gift", Name(s, "A"), Give(x => x.IsPill() != null, Player.GetGradeLvl()));
                    break;
                case "A_Peace":
                    TaoistLogEvent.Log("log_story_gift", Name(s, "A"), Give(x => x.IsArtifact() != null, Player.GetGradeLvl()));
                    break;
                case "A_Fallen":
                case "A_Traitor":
                    var a = U(s, "A");
                    if (a == null || a.isDie || StoryEvent.Busy() || CommonTool.Random(0, 100) >= 30) return;
                    Say(s, "A", "endA_hunt", new[] { "endA_hunt_o1" }, _ => Fight(s, a, "A_hunt"));
                    break;
            }
        }

        private bool InTown(StoryState s, string place)
        {
            var town = PlayerTown();
            return town != null && s.Roles.TryGetValue(place, out var id) && town.buildData.id == id;
        }
    }
}
