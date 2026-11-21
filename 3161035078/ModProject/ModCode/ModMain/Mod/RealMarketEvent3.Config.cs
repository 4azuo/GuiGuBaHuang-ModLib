using MOD_nE7UL2.Object;
using System;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// The player market (RealMarketEvent3), read from ModConf/market_item_configs.json; the defaults below are the same as the file.
    /// </summary>
    public class MarketItemConfig : JsonConfig<MarketItemConfig>
    {
        public const string FILE = "market_item_configs.json";
        public override string FileName => FILE;

        /// <summary>Cells around a town where NPCs come to the market.</summary>
        public int JoinRange { get; set; } = 3;
        /// <summary>Chances in % a month (S&amp;M MarketItemNpcJoinRate on top).</summary>
        public float SellerJoinMarketRate { get; set; } = 8.00f;
        /// <summary>Chance in % that a seller puts up an item: (7 - item level) x this.</summary>
        public float SellRateDependOnGrade { get; set; } = 3.00f;
        public float BuyerJoinMarketRate { get; set; } = 12.00f;
        /// <summary>Chance in % that a buyer bids: item level x this.</summary>
        public float BuyRateDependOnGrade { get; set; } = 4.00f;
        /// <summary>Chance in % a month of a deal: this x best bid / price.</summary>
        public float GetDealRate { get; set; } = 24.00f;
        /// <summary>Chance in % a month that a seller takes the item back: this x months on sale.</summary>
        public float CancelDealOnMonth { get; set; } = 3.00f;
        public float CancelDealLastMinuteRate { get; set; } = 8.00f;
        /// <summary>Chance in % that a seller hides the name.</summary>
        public float HiddenRate { get; set; } = 20.00f;
        /// <summary>Chance in % that a stalker follows a buyer (S&amp;M MarketItemGetAttackedRate on top).</summary>
        public float GetStalkedRate { get; set; } = 10.00f;

        protected override void Fix(MarketItemConfig d)
        {
            JoinRange = Math.Max(0, JoinRange);
            SellerJoinMarketRate = Rate(SellerJoinMarketRate);
            SellRateDependOnGrade = Math.Max(0f, SellRateDependOnGrade);
            BuyerJoinMarketRate = Rate(BuyerJoinMarketRate);
            BuyRateDependOnGrade = Math.Max(0f, BuyRateDependOnGrade);
            GetDealRate = Math.Max(0f, GetDealRate);
            CancelDealOnMonth = Math.Max(0f, CancelDealOnMonth);
            CancelDealLastMinuteRate = Rate(CancelDealLastMinuteRate);
            HiddenRate = Rate(HiddenRate);
            GetStalkedRate = Rate(GetStalkedRate);
        }
    }
}
