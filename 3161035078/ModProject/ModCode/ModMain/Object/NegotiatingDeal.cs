using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Object
{
    public class NegotiatingDeal
    {
        public string BuyerId { get; set; }
        public MarketItem TargetProp { get; set; }
        public List<NegotiatingItem> Items { get; set; } = new List<NegotiatingItem>();

        [JsonIgnore]
        public WorldUnitBase Buyer => g.world.unit.GetUnit(BuyerId);
        [JsonIgnore]
        public int TotalValue
        {
            get
            {
                if (Items?.Count == 0)
                    return 0;
                return Items.Sum(x =>
                {
                    if (x.NegotiatingPropSoleId == null)
                        return x.Count;
                    else
                        return x.Value;
                });
            }
        }
        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                var buyer = Buyer;
                return buyer != null && !buyer.isDie && TargetProp != null && TargetProp.IsValid && Items.Count > 0;
            }
        }

        public NegotiatingDeal(string buyerId, MarketItem i)
        {
            BuyerId = buyerId;
            TargetProp = i;
        }
    }
}
