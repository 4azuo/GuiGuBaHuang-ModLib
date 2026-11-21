using MOD_nE7UL2.Object;
using System;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// NPCs joining the battles of the player (BattleEvent) and teaming up (HirePeopleEvent), read from ModConf/battle_configs.json;
    /// the defaults below are the same as the file.
    /// </summary>
    public class BattleConfig : JsonConfig<BattleConfig>
    {
        public const string FILE = "battle_configs.json";
        public override string FileName => FILE;

        /// <summary>Cells around the player where NPCs may join.</summary>
        public int JoinRange { get; set; } = 6;
        /// <summary>Chances in %, checked every second of the battle.</summary>
        public float RandomNpcJoinRate { get; set; } = 0.10f;
        public float SectNpcJoinRate { get; set; } = 1.00f;
        public float TownGuardNpcJoinRate { get; set; } = 3.00f;
        public float TeamMemberJoinRate { get; set; } = 5.00f;
        public float TeamMemberBetrayRate { get; set; } = 0.20f;
        public float StalkerJoinRate { get; set; } = 1.00f;
        /// <summary>Intimacy (250 to -250) from which an NPC is a friend / an enemy.</summary>
        public int FriendlyIntim { get; set; } = 160;
        public int EnemyIntim { get; set; } = -160;
        /// <summary>Difference of righteousness (NPC vs player) from which an NPC takes the other side.</summary>
        public int DifferenceRighteous { get; set; } = 80;

        protected override void Fix(BattleConfig d)
        {
            JoinRange = Math.Max(0, JoinRange);
            RandomNpcJoinRate = Rate(RandomNpcJoinRate);
            SectNpcJoinRate = Rate(SectNpcJoinRate);
            TownGuardNpcJoinRate = Rate(TownGuardNpcJoinRate);
            TeamMemberJoinRate = Rate(TeamMemberJoinRate);
            TeamMemberBetrayRate = Rate(TeamMemberBetrayRate);
            StalkerJoinRate = Rate(StalkerJoinRate);
            DifferenceRighteous = Math.Max(0, DifferenceRighteous);
        }
    }
}
