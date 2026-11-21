using MOD_nE7UL2.Object;
using System;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Bounty commissions (MissionDeclareEvent, CommissionTask), read from ModConf/mission_declare_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class MissionDeclareConfig : JsonConfig<MissionDeclareConfig>
    {
        public const string FILE = "mission_declare_configs.json";
        public override string FileName => FILE;

        /// <summary>Item levels 1-6 (CostTime, SuccessRate: one value a level).</summary>
        public const int LEVELS = 6;

        /// <summary>Fee in spirit stones = total value x FeeRate + FeeMinCost.</summary>
        public float FeeRate { get; set; } = 0.2f;
        public int FeeMinCost { get; set; } = 100;
        /// <summary>Reputation (degree) cost = total value x DegreeCostRate + DegreeMinCost.</summary>
        public float DegreeCostRate { get; set; } = 0.001f;
        public int DegreeMinCost { get; set; } = 1;
        /// <summary>Months per item, by item level 1-6.</summary>
        public int[] CostTime { get; set; } = { 1, 2, 3, 5, 8, 12 };
        /// <summary>Success chance in %, by item level 1-6 (the lowest of the items; + luck / 100).</summary>
        public float[] SuccessRate { get; set; } = { 100.0f, 99.0f, 98.0f, 96.0f, 94.0f, 91.0f };

        protected override void Fix(MissionDeclareConfig d)
        {
            FeeRate = Math.Max(0f, FeeRate);
            FeeMinCost = Math.Max(0, FeeMinCost);
            DegreeCostRate = Math.Max(0f, DegreeCostRate);
            DegreeMinCost = Math.Max(0, DegreeMinCost);
            CostTime = Pad(CostTime, d.CostTime, LEVELS).Select(x => Math.Max(1, x)).ToArray();
            SuccessRate = Pad(SuccessRate, d.SuccessRate, LEVELS).Select(Rate).ToArray();
        }
    }
}
