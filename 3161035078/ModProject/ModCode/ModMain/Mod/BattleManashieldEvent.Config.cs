using MOD_nE7UL2.Object;
using System;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Mana shield (BattleManashieldEvent), read from ModConf/battle_manashield_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class BattleManashieldConfig : JsonConfig<BattleManashieldConfig>
    {
        public const string FILE = "battle_manashield_configs.json";
        public override string FileName => FILE;

        /// <summary>Shield = cur MP x ManaShieldRate1 + max MP x ManaShieldRate2 + cur HP x 0.05 x Blood Energy level.</summary>
        public float ManaShieldRate1 { get; set; } = 0.15f;
        public float ManaShieldRate2 { get; set; } = 0.05f;

        protected override void Fix(BattleManashieldConfig d)
        {
            ManaShieldRate1 = Math.Max(0f, ManaShieldRate1);
            ManaShieldRate2 = Math.Max(0f, ManaShieldRate2);
        }
    }
}
