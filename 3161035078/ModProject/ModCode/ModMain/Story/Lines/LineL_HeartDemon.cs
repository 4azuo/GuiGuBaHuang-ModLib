using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Story
{
    /// <summary>
    /// Line L "The Heart Demon" (docs/Taoist/Storylines/L-Heart-Demon.md): the last line, opens at Enlightenment. Acts
    /// L0-L4: dreams of the people of the other lines (their endings), the mirror lake (3 regrets; the tomb legacy of
    /// line I as the temptation), the ghosts, the copy of the player, clear heart. Endings ClearHeart / Scarred / Embraced.
    /// </summary>
    public class LineL_HeartDemon : StoryLine
    {
        public override string Id => "L";
        public override float MonthlyChance => 1f;
        public override StoryRole[] Roles => new StoryRole[0];

        public override bool CanOpen(StoryState s) => Player.GetGradeLvl() >= 8;

        /// <summary>The endings reached by the other lines, by name (the dreams).</summary>
        public static string Dreams()
        {
            var parts = new List<string>();
            foreach (var line in StoryEvent.LINES.Where(l => l.Id != "L"))
            {
                var st = StoryEvent.Instance.State(line.Id);
                if (st.Ending != null) parts.Add($"{GameTool.LS("story_name_" + line.Id)} ({GameTool.LS("story_end_" + st.Ending)})");
            }
            return parts.Count == 0 ? GameTool.LS("story_L0_none") : string.Join(", ", parts);
        }

        public override void Run(StoryState s)
        {
            switch (s.Act * 100 + s.Step)
            {
                case 1:
                    Tell(s, "L0_1", null, _ => Act(s, 1, 3), BG, Dreams());
                    break;

                // ---- L1 The mirror lake: 3 regrets (the tomb legacy of line I first, when taken)
                case 101:
                case 102:
                case 103:
                    var key = s.Step == 1 && EndingOf("I") == "I_Heir" ? "L1_tomb" : $"L1_{s.Step}";
                    Tell(s, key, new[] { "L1_o1", "L1_o2" }, i =>
                    {
                        if (i == 0) Count(s, "accepted");
                        if (s.Step < 3) Next(s); else Act(s, 2, 6);
                    });
                    break;

                case 201:
                    Tell(s, "L2_1", new[] { "L2_1_o1" }, _ => Fight(s, Foe(s, false, 1), "L2_ghosts"));
                    break;
                case 301:
                    Tell(s, "L3_1", new[] { "L3_1_o1" }, _ => Fight(s, Foe(s, null, 0), "L3_copy"));
                    break;
                case 401:
                    Tell(s, "L4_1", new[] { "L4_1_o1", "L4_1_o2", "L4_1_o3" }, i =>
                    {
                        if (i == 0) Tell(s, "endL_ClearHeart", null, _ => { Player.AddProperty<int>(UnitPropertyEnum.Talent, 10); End(s, "L_ClearHeart"); }, BG, Dreams());
                        else if (i == 1) Tell(s, "endL_Scarred", null, _ => End(s, "L_Scarred"));
                        else Tell(s, "endL_Embraced", null, _ => { Player.AddProperty<int>(UnitPropertyEnum.Attack, Player.GetProperty<int>(UnitPropertyEnum.Attack) / 10); End(s, "L_Embraced"); });
                    });
                    break;
            }
        }

        public override void OnBattle(StoryState s, string tag, bool won)
        {
            s.Set(tag, won ? 1 : -1);
            if (tag == "L2_ghosts") Act(s, 3, 6);
            else if (tag == "L3_copy") { if (won) Act(s, 4, 1); else Wait(s, 12); }
            else Next(s);
        }

        /// <summary>Scarred Heart: the demon comes back every 20 years (a dream, mood lost).</summary>
        public override void Yearly(StoryState s)
        {
            if (s.Ending != "L_Scarred" || (Now - s.Flag("opened")) / 12 % 20 != 0) return;
            Player.AddProperty<int>(UnitPropertyEnum.Mood, -20);
            TaoistLogEvent.Log("log_story_gift", T(s, "L_demon"), T(s, "L_nightmare"));
        }
    }
}
