using MOD_nE7UL2.Object;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// NPC skill upgrade (NpcUpgradeSkillEvent), read from ModConf/npc_upgrade_skill_configs.json; the defaults below are the same as the file.
    /// Uses game enums (MartialType): the unit tests only check the keys of the file.
    /// </summary>
    public class NpcUpgradeSkillConfig : JsonConfig<NpcUpgradeSkillConfig>
    {
        public const string FILE = "npc_upgrade_skill_configs.json";
        public override string FileName => FILE;

        /// <summary>Chance in % a month to upgrade a skill of the type (x luck / 100).</summary>
        public Dictionary<MartialType, float> UpgradeRates { get; set; } = new Dictionary<MartialType, float>
        {
            [MartialType.SkillLeft] = 7.0f,
            [MartialType.SkillRight] = 7.0f,
            [MartialType.Step] = 12.0f,
            [MartialType.Ultimate] = 8.0f,
            [MartialType.Ability] = 16.0f,
        };

        protected override void Fix(NpcUpgradeSkillConfig d)
        {
            UpgradeRates = UpgradeRates ?? d.UpgradeRates;
        }

        /// <summary>
        /// Random martial type to upgrade.
        /// </summary>
        public MartialType RandomUpgradingMartial(float r, float ratio)
        {
            var min = 0.00f;
            var max = min;
            foreach (var ut in UpgradeRates)
            {
                min = max;
                max = min + ut.Value * ratio;
                if (ModLib.Helper.ValueHelper.IsBetween(r, min, max))
                {
                    return ut.Key;
                }
            }
            return MartialType.None;
        }
    }
}
