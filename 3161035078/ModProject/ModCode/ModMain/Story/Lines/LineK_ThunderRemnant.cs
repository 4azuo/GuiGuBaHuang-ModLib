using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line K "The Thunder Remnant" (docs/Taoist/Storylines/K-Thunder-Remnant.md): the voice of an immortal who failed
    /// his ascension (no NPC: narration), opens at Soul Formation. Acts K0-K4: the voice, the broken platform (3 years),
    /// his descendants (2 years), the truth, the second tribulation. Endings Guided / Devourer / SecondChance. Line G:
    /// a rival who became a friend comes to the last battle, an eternal rival sides with the remnant.
    /// </summary>
    public class LineK_ThunderRemnant : StoryLine
    {
        public override string Id => "K";
        public override float MonthlyChance => 0.5f;
        public override StoryRole[] Roles => new StoryRole[0];

        public override bool CanOpen(StoryState s) => Player.GetGradeLvl() >= 7;

        public override int TimeLimit(int act) => act == 1 ? 36 : act == 2 ? 24 : 0;

        /// <summary>Lan Qiu of line G (alive), and how that line ended.</summary>
        private static WorldUnitBase Rival(out string ending)
        {
            var g = StoryEvent.Instance.State("G");
            ending = g.Ending;
            return U(g, "W") is WorldUnitBase w && !w.isDie ? w : null;
        }

        public override void Run(StoryState s)
        {
            switch (s.Act * 100 + s.Step)
            {
                case 1:
                    Tell(s, "K0_1", null, _ => Act(s, 1, 3));
                    break;
                case 101:
                    Tell(s, "K1_1", new[] { "K1_1_o1" }, _ => FightBeast(s, "K1_puppets", 5));
                    break;
                case 201:
                    var price = EncounterEvent.Cost(30);
                    Tell(s, "K2_1", new[] { "K2_1_o1", "K2_1_o2" }, i =>
                    {
                        if (i == 0) Fight(s, Foe(s, null, 1), "K2_sect");
                        else { if (!Pay(s, price)) return; Act(s, 3, 6); }
                    }, BG, price);
                    break;
                case 301:
                    Tell(s, "K3_1", new[] { "K3_1_o1", "K3_1_o2" }, i =>
                    {
                        if (i == 0) s.Set("witness");
                        Act(s, 4, 6);
                    });
                    break;
                case 401:
                    // line G: Lan Qiu comes to the tribulation, as a friend or with the remnant
                    var rival = Rival(out var gEnd);
                    var key = rival != null && gEnd == "G_Friend" ? "K4_friend" : rival != null && gEnd == "G_Rival" ? "K4_rival" : "K4_1";
                    Tell(s, key, new[] { "K4_1_o1", "K4_1_o2", "K4_1_o3" }, i =>
                    {
                        if (i == 0) Tell(s, "endK_Guided", null, _ => { Player.AddExp(5000 * Player.GetGradeLvl()); End(s, "K_Guided"); });
                        else if (i == 1)
                        {
                            if (key == "K4_rival") Fight(s, rival, "K4_rival_fight");
                            else FightBeast(s, "K4_fight", 8);
                        }
                        else Tell(s, "endK_SecondChance", null, _ => { Give(x => x.IsPill() != null, Player.GetGradeLvl() + 1); End(s, "K_SecondChance"); });
                    }, BG, rival?.GetName() ?? "?");
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            switch (tag)
            {
                case "K1_puppets": Act(s, 2, 6); break;
                case "K2_sect": Act(s, 3, 6); break;
                case "K4_rival_fight": FightBeast(s, "K4_fight", 8); break;
                case "K4_fight":
                    if (won) Tell(s, "endK_Devourer", null, _ => { Player.AddProperty<int>(UnitPropertyEnum.BasisThunder, 10); End(s, "K_Devourer"); });
                    else Wait(s, 12);
                    break;
                default: Next(s); break;
            }
        }
    }
}
