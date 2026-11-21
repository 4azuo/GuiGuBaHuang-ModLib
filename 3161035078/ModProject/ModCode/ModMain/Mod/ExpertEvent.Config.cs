using MOD_nE7UL2.Object;
using System;
using System.Collections.Generic;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Expertise (ExpertEvent, UnitModifyHelper), read from ModConf/expert_configs.json; the defaults below are the same as the file.
    /// Uses game enums (MartialType): the unit tests only check the keys of the file.
    /// </summary>
    public class ExpertConfig : JsonConfig<ExpertConfig>
    {
        public const string FILE = "expert_configs.json";
        public override string FileName => FILE;

        /// <summary>Damage bonus per expertise level, by martial type.</summary>
        public Dictionary<MartialType, float> SkillDmgRatios { get; set; } = new Dictionary<MartialType, float>
        {
            [MartialType.None] = 0.0f,
            [MartialType.SkillLeft] = 0.1f,
            [MartialType.SkillRight] = 1.0f,
            [MartialType.Step] = 1.0f,
            [MartialType.Ability] = 0.4f,
            [MartialType.Ultimate] = 5.0f,
        };
        /// <summary>Expertise exp per use (battle) or per month (cultivation), by martial type.</summary>
        public Dictionary<MartialType, float> SkillExpRatios { get; set; } = new Dictionary<MartialType, float>
        {
            [MartialType.None] = 0.0f,
            [MartialType.SkillLeft] = 0.02f,
            [MartialType.SkillRight] = 0.8f,
            [MartialType.Step] = 0.1f,
            [MartialType.Ability] = 1.0f,
            [MartialType.Ultimate] = 0.6f,
        };
        /// <summary>Artifact expertise exp a month (equipped) / per use in a battle.</summary>
        public float AutoArtifactExpRate { get; set; } = 1.0f;
        public float BattleArtifactExpRate { get; set; } = 0.02f;

        protected override void Fix(ExpertConfig d)
        {
            // read as SkillDmgRatios[type] / SkillExpRatios[type]: every type of the defaults
            SkillDmgRatios = Fill(SkillDmgRatios, d.SkillDmgRatios);
            SkillExpRatios = Fill(SkillExpRatios, d.SkillExpRatios);
            AutoArtifactExpRate = Math.Max(0f, AutoArtifactExpRate);
            BattleArtifactExpRate = Math.Max(0f, BattleArtifactExpRate);
        }
    }
}
