using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line M "Old Tie's Forge" (docs/Taoist/Storylines/M-Old-Tie-Forge.md, the Artisan Dao): the old smith {F} of a town
    /// {T} burned his hand in a furnace accident. Acts M0-M4: the burned hand (take the exam for him / a pill), the ore of
    /// the mine vein (give ore / pay / fight the vein's guardian), the apprentice or the secret, the guild's sabotage
    /// (2 years), the last blade. Endings Masterwork / Heir / Secret.
    /// </summary>
    public class LineM_OldTieForge : StoryLine
    {
        public override string Id => "M";
        public override float MonthlyChance => 0.6f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole { Name = "F", Sex = UnitSexType.Man, MinLived = 0.55f, MaxLived = 0.95f, OutTraits = new[] { 13, 14 }, MaxGradeOffset = 0 },
        };

        /// <summary>The Artisan Dao on, in a town, from grade 2 or a craft learned.</summary>
        public override bool CanOpen(StoryState s) => ArtisanEvent.Enabled && PlayerTown() != null && (Player.GetGradeLvl() >= 2 || ArtisanEvent.BestRank(Player) >= 1);

        public override void OnCast(StoryState s)
        {
            s.Roles["T"] = PlayerTown()?.buildData.id;
            U(s, "F")?.SetUnitPos(Player.GetUnitPos());
        }

        public override int TimeLimit(int act) => act == 3 ? 24 : 0;

        /// <summary>M3 late: the guild burned the forge.</summary>
        public override void OnLate(StoryState s)
        {
            if (s.Act != 3) return;
            s.Set("burned");
            Act(s, 4);
        }

        /// <summary>Forge exp of the player (the craft learned when not yet).</summary>
        private static void ForgeExp(long exp)
        {
            if (!ArtisanEvent.Enabled) return;
            var d = ArtisanEvent.Get(Player, ArtisanEvent.Craft.Forge, true);
            if (d.Rank < 1) d.Rank = 1;
            ArtisanEvent.AddExp(d, exp);
            ArtisanEvent.Sync(Player, ArtisanEvent.Craft.Forge);
        }

        private static bool IsOre(ConfItemPropsItem x) => (x.IsMineItem()?.Length ?? 0) > 0;

        public override void Run(StoryState s)
        {
            var grade = System.Math.Max(1, Player.GetGradeLvl());
            switch (s.Act * 100 + s.Step)
            {
                // ---- M0 The burned hand
                case 1:
                    Say(s, "F", "M0_1", new[] { "M0_1_o1", "M0_1_o2", "M0_1_o3" }, i =>
                    {
                        if (i == 0) { s.Set("exam"); ForgeExp(ArtisanEvent.NeedExp(1) / 2); AddIntim(s, "F", 150); Next(s); }
                        else if (i == 1) { if (!Offer(s, x => x.IsPill() != null, 1, "pill")) return; AddIntim(s, "F", 100); Next(s); }
                        else Close(s, "refused");
                    });
                    break;
                case 2:
                    Say(s, "F", s.Has("exam") ? "M0_exam" : "M0_2", null, _ => Act(s, 1, 6));
                    break;

                // ---- M1 The ore of the vein
                case 101:
                    var spot = ArtisanEvent.NearestSpot(Player.GetUnitPos());
                    var price = EncounterEvent.Cost(5);
                    Say(s, "F", "M1_1", new[] { "M1_1_o1", "M1_1_o2", "M1_1_o3" }, i =>
                    {
                        if (i == 0) { if (!Offer(s, IsOre, 1, "ore")) return; s.Set("ore"); Next(s); }
                        else if (i == 1) { if (!Pay(s, price)) return; Next(s); }
                        else FightBeast(s, "M1_guardian", 2);
                    }, spot != null ? ArtisanEvent.SpotName(spot) : "?", price);
                    break;
                case 102:
                    Say(s, "F", "M1_2", null, _ => { ForgeExp(ArtisanEvent.NeedExp(1)); Act(s, 2, 12); });
                    break;

                // ---- M2 The apprentice or the secret
                case 201:
                    Say(s, "F", "M2_1", new[] { "M2_1_o1", "M2_1_o2" }, i =>
                    {
                        s.Set(i == 0 ? "heir" : "secret");
                        if (i == 0) { AddIntim(s, "F", 200); if (MeritEvent.Enabled) MeritEvent.Add(Player, 100, "merit_story"); }
                        else ForgeExp(ArtisanEvent.NeedExp(2));
                        Act(s, 3, 12);
                    });
                    break;

                // ---- M3 The guild (2 years)
                case 301:
                    var bribe = EncounterEvent.Cost(8);
                    Say(s, "F", "M3_1", new[] { "M3_1_o1", "M3_1_o2" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, null, 0), "M3_guild");
                        else { if (!Pay(s, bribe)) return; s.Set("bribed"); Act(s, 4, 6); }
                    }, bribe);
                    break;

                // ---- M4 The last blade
                case 401:
                    if (s.Has("burned"))
                    {
                        Say(s, "F", "M4_burned", null, _ => Tell(s, "endM_Secret", null, __ => { ForgeExp(ArtisanEvent.NeedExp(2)); End(s, "M_Secret"); }));
                        return;
                    }
                    Say(s, "F", "M4_1", new[] { "M4_1_o1", "M4_1_o2" }, i =>
                    {
                        if (i == 0)
                        {
                            // the forge's last fire: the masterwork, or the apprentice's (heir) / the player's (secret)
                            if (s.Has("heir")) Say(s, "F", "endM_Heir", null, _ => { Give(x => x.IsArtifact() != null, grade); AddIntim(s, "F", 200); End(s, "M_Heir"); });
                            else Tell(s, "endM_Secret", null, _ => { ForgeExp(ArtisanEvent.NeedExp(3)); End(s, "M_Secret"); });
                        }
                        else
                        {
                            if (!Offer(s, IsOre, grade, "ore")) return;
                            Say(s, "F", "endM_Masterwork", null, _ => { Give(x => x.IsArtifact() != null, grade + 1); AddIntim(s, "F", 300); End(s, "M_Masterwork"); });
                        }
                    }, grade);
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "M1_guardian":
                    if (won) Next(s); else Wait(s, 3);
                    break;
                case "M3_guild":
                    if (!won) s.Set("burned");
                    Act(s, 4, 6);
                    break;
                default: Next(s); break;
            }
        }

        /// <summary>Heir: the apprentice sends a piece every year.</summary>
        public override void Yearly(StoryState s)
        {
            if (s.Ending == "M_Heir") Give(x => x.IsArtifact() != null, System.Math.Max(1, Player.GetGradeLvl() - 1));
        }
    }
}
