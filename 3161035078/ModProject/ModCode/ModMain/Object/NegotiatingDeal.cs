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
        public int TotalValue => Items.Sum(x =>
        {
            if (x.NegotiatingPropSoleId == null)
                return x.Count;
            else
                return Buyer.GetUnitProp(x.NegotiatingPropSoleId).propsInfoBase.sale * x.Count;
        });

        public NegotiatingDeal(string buyerId, MarketItem i)
        {
            BuyerId = buyerId;
            TargetProp = i;
        }
    }
}
