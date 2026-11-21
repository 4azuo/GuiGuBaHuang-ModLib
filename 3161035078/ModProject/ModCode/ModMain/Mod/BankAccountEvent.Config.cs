using MOD_nE7UL2.Object;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Bank accounts (BankAccountEvent), read from ModConf/bank_account_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class BankAccountConfig : JsonConfig<BankAccountConfig>
    {
        public const string FILE = "bank_account_configs.json";
        public override string FileName => FILE;

        /// <summary>Base fee to open an account, per area (before inflation and S&amp;M AddBankAccountCostRate).</summary>
        public Dictionary<int, int> OpenFee { get; set; } = new Dictionary<int, int>
        {
            [1] = 100,
            [2] = 1000,
            [3] = 1000,
            [4] = 10000,
            [5] = 10000,
            [6] = 100000,
            [7] = 100000,
            [8] = 1000000,
            [9] = 1000000,
            [10] = 10000000,
            [11] = 20000000,
            [12] = 40000000,
            [13] = 100000000,
        };

        protected override void Fix(BankAccountConfig d)
        {
            // an area left out: the default (BankAccountEvent reads the fee of every area)
            OpenFee = Fill(OpenFee, d.OpenFee);
            foreach (var k in OpenFee.Keys.ToList())
                OpenFee[k] = Math.Max(1, OpenFee[k]);
        }
    }
}
