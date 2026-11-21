using MOD_nE7UL2.Object;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Inflation (InflationaryEvent), read from ModConf/inflationary_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class InflationaryConfig : JsonConfig<InflationaryConfig>
    {
        public const string FILE = "inflationary_configs.json";
        public override string FileName => FILE;

        /// <summary>World price x this every year (1.02 = +2 % a year; S&amp;M AddInflationRate on top).</summary>
        public float InflationaryRate { get; set; } = 1.02f;

        protected override void Fix(InflationaryConfig d)
        {
            if (InflationaryRate <= 0f) InflationaryRate = d.InflationaryRate;
        }
    }
}
