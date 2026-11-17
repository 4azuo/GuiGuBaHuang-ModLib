using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line D "Sworn Brothers" (docs/Taoist/Storylines/D-Sworn-Brothers.md): Iron Ox {N}, Murong Ge {C} and the woman
    /// Ge courts {L}. Acts D0-D4, endings ThreeSworn / MurongCompanion / BrotherLost / Rivals.
    /// </summary>
    public class LineD_SwornBrothers : StoryLine
    {
        public override string Id => "D";
        public override float MonthlyChance => 0.5f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole
            {
                Name = "N", Sex = UnitSexType.Man, MinLived = 0.15f, MaxLived = 0.4f, MaxBeauty = 249,
                InTraits = new[] { 2 }, OutTraits = new[] { 9 }, BestBasis = new[] { UnitPropertyEnum.BasisFist, UnitPropertyEnum.BasisPalm },
                MinGradeOffset = -1, MaxGradeOffset = 1,
            },
            new StoryRole
            {
                Name = "C", Sex = UnitSexType.Man, MinLived = 0.15f, MaxLived = 0.3f, MinBeauty = 250,
                OutTraits = new[] { 17 }, InTraits = new[] { 4 }, MinGradeOffset = -1, MaxGradeOffset = 1,
            },
        };

        public override bool CanOpen(StoryState s) => PlayerTown() != null;

        /// <summary>Both are at the inn of the player's town.</summary>
        public override void OnCast(StoryState s)
        {
            U(s, "N")?.SetUnitPos(Player.GetUnitPos());
            U(s, "C")?.SetUnitPos(Player.GetUnitPos());
        }

        public override int TimeLimit(int act) => act == 1 ? 12 : act == 2 ? 12 : act == 3 ? 24 : 0;

        public override void Run(StoryState s)
        {
            switch (s.Act * 100 + s.Step)
            {
                // ---- D0 Eleven bowls
                case 1:
                    Say(s, "N", "D0_1", null, _ => Next(s));
                    break;
                case 2:
                    var bill = EncounterEvent.Cost(1);
                    Say(s, "N", "D0_2", new[] { "D0_2_o1", "D0_2_o2" }, i =>
                    {
                        if (i == 0 && Player.GetUnitMoney() >= bill) { Player.AddUnitMoney(-bill); AddIntim(s, "N", 150); Next(s); }
                        else Spar(s, U(s, "N"), "D0_duel");
                    }, bill);
                    break;
                case 3:
                    Say(s, "C", "D0_3", null, _ => Act(s, 1, 3));
                    break;

                // ---- D1 Beast hunt
                case 101:
                    Say(s, "N", "D1_1", null, _ => Next(s));
                    break;
                case 102:
                    Tell(s, "D1_2", new[] { "D1_2_o1" }, _ => FightBeast(s, "D1_beast"));
                    break;
                case 103:
                    var canSwear = Intim(s, "N") >= 200 && Intim(s, "C") >= 200;
                    Say(s, "C", "D1_3", canSwear ? new[] { "D1_3_o1", "D1_3_o2" } : new[] { "D1_3_o2" }, i =>
                    {
                        if (canSwear && i == 0) { s.Set("sworn"); AddIntim(s, "N", 100); AddIntim(s, "C", 100); }
                        Act(s, 2, 6);
                    });
                    break;

                // ---- D2 Forty-two poems
                case 201:
                    if (!s.Roles.ContainsKey("L") && !CastL(s)) return; // waits for a single woman
                    Say(s, "C", "D2_1", null, _ => Next(s));
                    break;
                case 202:
                    Tell(s, "D2_2", new[] { "D2_2_o1", "D2_2_o2" }, i =>
                    {
                        if (i == 0 && TakeItem(p => p.IsHobby() != null || p.IsHerbItem() != null || p.IsPill() != null, 1)) s.Set("gift");
                        Next(s);
                    });
                    break;
                case 203:
                    Say(s, "C", "D2_3", new[] { "D2_3_o1", "D2_3_o2", "D2_3_o3" }, i =>
                    {
                        var c = U(s, "C");
                        var l = U(s, "L");
                        if (i == 0 && c != null && l != null)
                        {
                            var bonus = s.Has("gift") ? 400 : 200;
                            var lr = l.data.unitData.relationData;
                            lr.SetIntim(c.GetUnitId(), lr.GetIntim(c.GetUnitId()) + bonus);
                            AddIntim(s, "C", 100);
                            if (string.IsNullOrEmpty(FamilyEvent.Spouse(c)) && string.IsNullOrEmpty(FamilyEvent.Spouse(l)) && FamilyEvent.IsMan(c) != FamilyEvent.IsMan(l))
                                FamilyEvent.SetMarried(c, l);
                        }
                        else if (i == 1) AddIntim(s, "C", -100);
                        else if (i == 2 && c != null && l != null)
                        {
                            var lr = l.data.unitData.relationData;
                            if (lr.GetIntim(Player.GetUnitId()) + (s.Has("gift") ? 200 : 0) > lr.GetIntim(c.GetUnitId())) { s.Set("rivals"); AddIntim(s, "C", -200); }
                            else s.Set("wedding");
                        }
                        Act(s, 3, 6);
                    });
                    break;

                // ---- D3 Murong's blood debt
                case 301:
                    Say(s, "C", "D3_1", null, _ => Next(s));
                    break;
                case 302:
                    Tell(s, "D3_2", new[] { "D3_2_o1" }, _ => Fight(s, Foe(s, false), "D3_killer"));
                    break;
                case 303:
                    Tell(s, "D3_3", new[] { "D3_3_o1", "D3_3_o2" }, i =>
                    {
                        if (i == 0) { s.Set("ally_C"); AddIntim(s, "C", 100); }
                        Act(s, 4, CommonTool.Random(12, 37));
                    });
                    break;

                // ---- D4 Two battle lines
                case 401:
                    U(s, "N")?.SetUnitPos(Player.GetUnitPos());
                    Say(s, "N", "D4_1", new[] { "D4_1_o1", "D4_1_o2", "D4_1_o3" }, i =>
                    {
                        if (i == 0 && Intim(s, "N") >= 300) { Say(s, "N", "D4_walk", null, _ => Ending(s)); }
                        else if (i == 2) { s.Set("switched"); Say(s, "N", "D4_switch", null, _ => Ending(s)); }
                        else Fight(s, U(s, "N"), "D4_duel", Intim(s, "N") < 300);
                    });
                    break;
                case 402:
                    Ending(s);
                    break;
            }
        }

        /// <summary>{L}: Yue Han when line A runs without a companion ending, else a single woman of their age.</summary>
        private bool CastL(StoryState s)
        {
            var a = StoryEvent.Instance.State("A");
            WorldUnitBase l = null;
            if (a.Open && a.Roles.TryGetValue("A", out var aid)) l = FamilyEvent.Unit(aid);
            if (l == null || l.isDie || !string.IsNullOrEmpty(FamilyEvent.Spouse(l)))
                l = StoryCasting.Pick(new StoryRole { Name = "L", Sex = UnitSexType.Woman, MinLived = 0.15f, MaxLived = 0.4f, MinBeauty = 250, Filter = u => string.IsNullOrEmpty(FamilyEvent.Spouse(u)) },
                    StoryEvent.Instance.TakenUnits(), StoryEvent.Units(), Player);
            if (l == null) return false;
            s.Roles["L"] = l.GetUnitId();
            return true;
        }

        public override System.Collections.Generic.IEnumerable<string> VitalRoles => new[] { "C" };

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "D0_duel": AddIntim(s, "N", 100); Say(s, "N", "D0_ribs", null, _ => Next(s)); break;
                case "D3_killer": if (won) Next(s); else Wait(s, 3); break;
                case "D4_duel":
                    // a real fight won with a weak bond: Ox does not survive it
                    if (won && Intim(s, "N") < 300) U(s, "N")?.Die();
                    s.Step = 2;
                    Wait(s, 0);
                    break;
                default: Next(s); break;
            }
        }

        private void Ending(StoryState s)
        {
            var n = U(s, "N");
            var c = U(s, "C");
            var femaleOrSameSex = !FamilyEvent.IsMan(Player) || FamilyEvent.SameSex;
            if (s.Has("rivals"))
                Say(s, "C", "endD_Rivals", null, _ => End(s, "D_Rivals"));
            else if (n == null || n.isDie)
                Say(s, "C", "endD_BrotherLost", null, _ =>
                {
                    Player.AddProperty<int>(UnitPropertyEnum.BasisFist, 10);
                    End(s, "D_BrotherLost");
                });
            else if (femaleOrSameSex && Intim(s, "C") >= 500 && string.IsNullOrEmpty(FamilyEvent.Spouse(Player)) && c != null && string.IsNullOrEmpty(FamilyEvent.Spouse(c)))
                Say(s, "C", "endD_MurongCompanion", null, _ =>
                {
                    FamilyEvent.SetMarried(Player, c);
                    var p = c.data.unitData.propertyData;
                    if (p.outTrait1 == 17) p.outTrait1 = 19; else if (p.outTrait2 == 17) p.outTrait2 = 19;
                    End(s, "D_MurongCompanion");
                });
            else
                Say(s, "N", "endD_ThreeSworn", null, _ =>
                {
                    AddIntim(s, "N", 200);
                    AddIntim(s, "C", 200);
                    End(s, "D_ThreeSworn");
                });
        }
    }
}
