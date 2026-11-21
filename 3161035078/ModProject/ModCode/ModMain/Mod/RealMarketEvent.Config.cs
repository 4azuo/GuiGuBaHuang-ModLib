using MOD_nE7UL2.Object;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Market sell rates (RealMarketEvent) and buy rates (RealMarketEvent2), read from ModConf/real_market_configs.json;
    /// the defaults below are the same as the file.
    /// </summary>
    public class RealMarketConfig : JsonConfig<RealMarketConfig>
    {
        public const string FILE = "real_market_configs.json";
        public override string FileName => FILE;

        /// <summary>
        /// Market price event window (Start, Dur, Period in months) and its rate bonus (%).
        /// </summary>
        public class _Event : GameEvent
        {
            public float AddSellRate { get; set; }
            public float AddBuyRate { get; set; }
        }

        /// <summary>Rates in % of the item value.</summary>
        public float MinSellRate { get; set; } = 85.0f;
        public float MaxSellRate { get; set; } = 110.0f;
        public float MinBuyRate { get; set; } = 90.0f;
        public float MaxBuyRate { get; set; } = 115.0f;
        /// <summary>Buy rate change (%) for the town guardian / town master.</summary>
        public float TownGReduceBuyRate { get; set; } = -5.0f;
        public float TownMReduceBuyRate { get; set; } = -10.0f;
        /// <summary>Market events ({} = none).</summary>
        public Dictionary<string, _Event> Events { get; set; } = new Dictionary<string, _Event>
        {
            ["HappyNewYear"] = new _Event { Name = "Happy New Year", Start = 1, Dur = 1, Period = 12, AddSellRate = 25.0f, AddBuyRate = 20.0f },
            ["HappyMidYear"] = new _Event { Name = "Happy Middle Year", Start = 6, Dur = 1, Period = 12, AddSellRate = -20.0f, AddBuyRate = -25.0f },
        };

        protected override void Fix(RealMarketConfig d)
        {
            MinSellRate = Math.Max(0f, MinSellRate);
            MaxSellRate = Math.Max(MinSellRate, MaxSellRate);
            MinBuyRate = Math.Max(0f, MinBuyRate);
            MaxBuyRate = Math.Max(MinBuyRate, MaxBuyRate);
            Events = (Events ?? d.Events).Where(e => e.Value != null).ToDictionary(e => e.Key, e => e.Value);
            foreach (var e in Events.Values)
            {
                e.Start = Math.Max(1, e.Start);
                e.Dur = Math.Max(1, e.Dur);
                e.Period = Math.Max(1, e.Period);   // IsRunningEvent loops on Period: 0 would never end
            }
        }

        /// <summary>
        /// Sell rate bonus of running market events.
        /// </summary>
        public float GetAddSellRate()
        {
            return Events.Where(e => e.Value.IsRunningEvent()).Sum(e => e.Value.AddSellRate);
        }

        /// <summary>
        /// Buy rate bonus of running market events.
        /// </summary>
        public float GetAddBuyRate()
        {
            return Events.Where(e => e.Value.IsRunningEvent()).Sum(e => e.Value.AddBuyRate);
        }
    }
}
