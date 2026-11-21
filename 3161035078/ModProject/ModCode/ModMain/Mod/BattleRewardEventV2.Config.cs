using MOD_nE7UL2.Object;
using System;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Battle exp rewards (BattleRewardEventV2), read from ModConf/battle_reward_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class BattleRewardConfig : JsonConfig<BattleRewardConfig>
    {
        public const string FILE = "battle_reward_configs.json";
        public override string FileName => FILE;

        /// <summary>Cultivation exp per damage dealt / received in a battle (at least 1).</summary>
        public float ExpPerDmgDealt { get; set; } = 0.001f;
        public float ExpPerDmgRecv { get; set; } = 0.01f;

        protected override void Fix(BattleRewardConfig d)
        {
            ExpPerDmgDealt = Math.Max(0f, ExpPerDmgDealt);
            ExpPerDmgRecv = Math.Max(0f, ExpPerDmgRecv);
        }
    }
}
