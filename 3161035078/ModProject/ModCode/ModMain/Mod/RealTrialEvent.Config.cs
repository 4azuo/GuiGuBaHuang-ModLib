using MOD_nE7UL2.Object;
using System;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Trial of lightning (RealTrialEvent), read from ModConf/real_trial_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class RealTrialConfig : JsonConfig<RealTrialConfig>
    {
        public const string FILE = "real_trial_configs.json";
        public override string FileName => FILE;

        /// <summary>Lightning power-up = game level x PowerUpOnGameLevel + player grade x PowerUpOnGradeLevel.</summary>
        public float PowerUpOnGameLevel { get; set; } = 0.64f;
        public float PowerUpOnGradeLevel { get; set; } = 0.08f;

        protected override void Fix(RealTrialConfig d)
        {
            PowerUpOnGameLevel = Math.Max(0f, PowerUpOnGameLevel);
            PowerUpOnGradeLevel = Math.Max(0f, PowerUpOnGradeLevel);
        }
    }
}
