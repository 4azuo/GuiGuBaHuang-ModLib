using MOD_nE7UL2.Object;
using System;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Storage fee (RealStorageEvent), read from ModConf/real_storage_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class RealStorageConfig : JsonConfig<RealStorageConfig>
    {
        public const string FILE = "real_storage_configs.json";
        public override string FileName => FILE;

        /// <summary>Monthly fee = this x value of the stored items (0.001 = 0.1 %; S&amp;M AddBankFee on top).</summary>
        public float FeeRate { get; set; } = 0.001f;

        protected override void Fix(RealStorageConfig d)
        {
            FeeRate = Math.Max(0f, Math.Min(1f, FeeRate));
        }
    }
}
