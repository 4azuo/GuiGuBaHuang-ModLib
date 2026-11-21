using MOD_nE7UL2.Object;
using System;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Sect contribution exchange (ContributionExchangeEvent), read from ModConf/contribution_exchange_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class ContributionExchangeConfig : JsonConfig<ContributionExchangeConfig>
    {
        public const string FILE = "contribution_exchange_configs.json";
        public override string FileName => FILE;

        /// <summary>Contribution of an item = its sell value / this (before inflation and S&amp;M AddSectExchangeRate).</summary>
        public int ExchangeRatio { get; set; } = 40;

        protected override void Fix(ContributionExchangeConfig d)
        {
            ExchangeRatio = Math.Max(1, ExchangeRatio);
        }
    }
}
