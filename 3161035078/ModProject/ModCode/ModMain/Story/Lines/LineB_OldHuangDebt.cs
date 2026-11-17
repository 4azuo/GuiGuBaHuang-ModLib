using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line B "Old Huang's Debt" (docs/Taoist/Storylines/B-Old-Huang-Debt.md): Old Huang {H}, merchant of {T} in
    /// debt to the Red Lotus. Acts B0-B5, endings MerchantPrince / Heir / FallenTown / Betrayed / Benefactor.
    /// </summary>
    public class LineB_OldHuangDebt : StoryLine
    {
        public override string Id => "B";
        public override int Acts => 6;
        public override float MonthlyChance => 1f;

        public override StoryRole[] Roles => new[]
        {
            new StoryRole
            {
                Name = "H", Sex = UnitSexType.Man, MinLived = 0.6f, MaxLived = 0.95f,
                InTraits = new[] { 4, 6 }, OutTraits = new[] { 12, 13 },
                // a merchant of the town the player is in
                Filter = u => PlayerTown() != null && u.GetMapBuild<MapBuildTown>()?.buildData.id == PlayerTown().buildData.id,
            },
        };

        /// <summary>The player is in a town with a market.</summary>
        public override bool CanOpen(StoryState s) => PlayerTown()?.GetBuildSub<MapBuildTownMarket>() != null;

        public override void OnCast(StoryState s)
        {
            // the player's town (CanOpen); a forced opening elsewhere takes the merchant's town
            var town = PlayerTown() ?? U(s, "H")?.GetMapBuild<MapBuildTown>() ?? ModMain.Store?.Towns?.FirstOrDefault(t => t != null);
            if (town != null) s.Roles["T"] = town.buildData.id;
        }

        public override int TimeLimit(int act) => act == 1 ? 24 : act == 2 ? 12 : act == 3 ? 24 : act == 4 ? 36 : 0;

        private static int Debt => EncounterEvent.Cost(30);

        public override void Run(StoryState s)
        {
            var h = U(s, "H");
            switch (s.Act * 100 + s.Step)
            {
                // ---- B0 Bargain
                case 1:
                    var price = EncounterEvent.Cost(3);
                    Say(s, "H", "B0_1", new[] { "B0_1_o1", "B0_1_o2" }, i =>
                    {
                        if (i == 0 && Player.GetUnitMoney() >= price)
                        {
                            Player.AddUnitMoney(-price);
                            s.Set("item_name_set");
                            TaoistLogEvent.Log("log_story_item", Give(x => x.IsHerbItem() != null || x.IsPill() != null, Player.GetGradeLvl() + 1));
                            Next(s, 1);
                        }
                        else if (s.Has("declined")) Close(s, "declined twice");
                        else { s.Set("declined"); Wait(s, 12); }
                    }, price);
                    break;
                case 2:
                    Tell(s, "B0_2", new[] { "B0_2_o1", "B0_2_o2", "B0_2_o3" }, i =>
                    {
                        if (i == 0) { s.Set("returned"); Next(s, 24); }
                        else if (i == 1) Fight(s, Foe(s, false), "B0_fight");
                        else Next(s);
                    });
                    break;
                case 3:
                    if (h != null) h.SetUnitPos(Player.GetUnitPos());
                    Say(s, "H", "B0_3", null, _ => Act(s, 1, 1));
                    break;

                // ---- B1 Confession
                case 101:
                    Tell(s, "B1_1", null, _ => { h?.SetUnitPos(Player.GetUnitPos()); Next(s); });
                    break;
                case 102:
                    Say(s, "H", "B1_2", null, _ => Next(s));
                    break;
                case 103:
                    var loan = EncounterEvent.Cost(10);
                    Say(s, "H", "B1_3", new[] { "B1_3_o1", "B1_3_o2", "B1_3_o3" }, i =>
                    {
                        if (i == 0) s.Set("vouched");
                        else if (i == 1 && Player.GetUnitMoney() >= loan) { Player.AddUnitMoney(-loan); s.Set("lent", loan); AddIntim(s, "H", 100); }
                        else s.Set("refused");
                        Act(s, 2, 2);
                    }, loan);
                    break;

                // ---- B2 Last caravan
                case 201:
                    var towns = (ModMain.Store?.Towns ?? new MapBuildTown[0]).Where(t => t != null && t.buildData.id != s.Roles["T"]).ToList();
                    if (towns.Count > 0) s.Roles["T2"] = towns[CommonTool.Random(0, towns.Count * 100) % towns.Count].buildData.id;
                    Say(s, "H", "B2_1", null, _ => Next(s, 2));
                    break;
                case 202:
                    Tell(s, "B2_2", new[] { "B2_2_o1" }, _ => Fight(s, Foe(s, s.Has("returned") ? (bool?)null : false), "B2_ambush"));
                    break;
                case 203:
                    var share = EncounterEvent.Cost(5);
                    Say(s, "H", "B2_3", null, _ => { Player.AddUnitMoney(share); Act(s, 3, 6); }, share);
                    break;
                case 210: // "Vanished Debtor"
                    Tell(s, "B2_vanished", null, _ => { s.Set("vanished"); Next(s, 12); });
                    break;
                case 211:
                    if (PlayerTown() == null || PlayerTown().buildData.id == s.Roles["T"]) return; // found in a far town
                    h?.SetUnitPos(Player.GetUnitPos());
                    Say(s, "H", "B2_found", null, _ =>
                    {
                        TaoistLogEvent.Log("log_story_gift", Name(s, "H"), Give(x => x.IsMineItem() != null, Player.GetGradeLvl()));
                        Act(s, 3, 3);
                    });
                    break;

                // ---- B3 Red ledger
                case 301:
                    Tell(s, "B3_1", null, _ => Next(s));
                    break;
                case 302:
                    var interest = EncounterEvent.Cost(8);
                    Say(s, "H", "B3_2", new[] { "B3_2_o1", "B3_2_o2", "B3_2_o3" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, false, 1), "B3_ledger");
                        else if (i == 1 && Player.GetUnitMoney() >= interest) { Player.AddUnitMoney(-interest); s.Set("paid"); Next(s); }
                        else { s.Set("stalled"); Next(s); }
                    }, interest);
                    break;
                case 303:
                    var key = s.Flag("B3_ledger") > 0 ? "B3_3_ledger" : s.Has("paid") ? "B3_3_paid" : "B3_3_stalled";
                    Tell(s, key, null, _ => Act(s, 4, s.Has("paid") ? 24 : s.Has("stalled") ? 0 : 6));
                    break;

                // ---- B4 Siege
                case 401:
                    Tell(s, "B4_1", null, _ => Next(s));
                    break;
                case 402:
                    Say(s, "H", "B4_2", new[] { "B4_2_o1", "B4_2_o2", "B4_2_o3" }, i =>
                    {
                        var town = Town(s);
                        if (i == 0) { if (town != null) MapBuildPropertyEvent.AddBuildProperty(town, EncounterEvent.Cost(20)); Next(s); }
                        else if (i == 1) { if (town != null) MapBuildPropertyEvent.AddBuildProperty(town, -MapBuildPropertyEvent.GetBuildProperty(town) * 3 / 10); s.Set("tribute"); Act(s, 5, 12); }
                        else { s.Set("fallen"); Act(s, 5, 1); }
                    });
                    break;
                case 403:
                    Tell(s, "B4_3", new[] { "B4_3_o1" }, _ => Fight(s, Foe(s, false, 1), "B4_siege"));
                    break;

                // ---- B5 Aftermath
                case 501:
                    Say(s, "H", "B5_1", null, _ => Next(s));
                    break;
                case 502:
                    TaoistLogEvent.Log("log_story_gift", Name(s, "H"), Give(x => x.IsPill() != null || x.IsMineItem() != null, Player.GetGradeLvl()));
                    Say(s, "H", "B5_2", null, _ => Next(s));
                    break;
                case 503:
                    Ending(s);
                    break;
            }
        }

        public override void OnLate(StoryState s)
        {
            if (s.Act == 2 && s.Flag("B2_ambush") <= 0) { s.Step = 10; Wait(s, 0); }
            else if (s.Act == 4) { s.Step = 3; Wait(s, 0); }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "B0_fight": Next(s); break;
                case "B2_ambush": if (won) Next(s); else { s.Step = 10; Wait(s, 0); } break;
                case "B3_ledger": Next(s); break;
                case "B4_siege": if (!won) s.Set("fallen"); else s.Set("defended"); Act(s, 5, 1); break;
                default: Next(s); break;
            }
        }

        /// <summary>He dies of old age (from act 1 on): The Heir.</summary>
        public override void OnRoleDead(StoryState s, string role)
        {
            if (s.Act < 1) { Close(s, "died"); return; }
            Tell(s, "endB_Heir", null, _ =>
            {
                for (var i = 0; i < 3; i++)
                    TaoistLogEvent.Log("log_story_item", Give(x => x.IsHerbItem() != null || x.IsMineItem() != null || x.IsPill() != null, Player.GetGradeLvl()));
                End(s, "B_Heir");
            });
        }

        private void Ending(StoryState s)
        {
            if (s.Has("vouched") && CommonTool.Random(0, 2) == 0)
                Tell(s, "endB_Betrayed", null, _ =>
                {
                    Player.AddUnitMoney(-System.Math.Min(Player.GetUnitMoney(), Debt));
                    var town = Town(s);
                    var master = town == null ? null : MapBuildPropertyEvent.GetTownMaster(town);
                    if (town != null && master != null && !master.IsPlayer() && U(s, "H") != null)
                        WantedEvent.Instance?.Post(town, master, U(s, "H"), WantedEvent.MinReward(U(s, "H")));
                    End(s, "B_Betrayed");
                });
            else if (s.Has("fallen"))
                Tell(s, "endB_FallenTown", null, _ =>
                {
                    var town = Town(s);
                    if (town != null && MapBuildPropertyEvent.Instance.TaxRate.ContainsKey(town.buildData.id))
                        MapBuildPropertyEvent.Instance.TaxRate[town.buildData.id] *= 2;
                    End(s, "B_FallenTown");
                });
            else if (s.Has("lent") && !s.Has("defended"))
                Say(s, "H", "endB_Benefactor", null, _ => { Player.AddUnitMoney(s.Flag("lent") * 11 / 10); End(s, "B_Benefactor"); });
            else
                Say(s, "H", "endB_MerchantPrince", null, _ => End(s, "B_MerchantPrince"));
        }

        /// <summary>Merchant Prince: goods every year. Benefactor: once, pays off a bounty on the player.</summary>
        public override void Yearly(StoryState s)
        {
            if (s.Ending == "B_MerchantPrince" && U(s, "H") != null && !U(s, "H").isDie)
                TaoistLogEvent.Log("log_story_gift", Name(s, "H"), Give(x => x.IsHerbItem() != null || x.IsMineItem() != null, Player.GetGradeLvl()));
            if (s.Ending == "B_Benefactor" && !s.Has("bounty_cleared") && WantedEvent.Instance != null)
            {
                var b = WantedEvent.Instance.Bounties.FirstOrDefault(x => x.OnPlayer);
                if (b == null) return;
                s.Set("bounty_cleared");
                WantedEvent.Instance.Refund(b, "log_wanted_cleared");
                TaoistLogEvent.Log("log_story_benefactor", Name(s, "H"));
            }
        }

        private static MapBuildTown Town(StoryState s) =>
            s.Roles.TryGetValue("T", out var id) ? g.world.build.GetBuild(id)?.TryCast<MapBuildTown>() : null;
    }
}
