using Newtonsoft.Json;

namespace MOD_nE7UL2.Object
{
    public class NegotiatingItem
    {
        public NegotiatingDeal ForDeal { get; set; }
        public int NegotiatingPropPropId { get; set; }
        public string NegotiatingPropSoleId { get; set; }
        public int Count { get; set; }

        [JsonIgnore]
        public DataProps.PropsData NegotiatingProp => ForDeal.Buyer.GetUnitProp(NegotiatingPropSoleId);

        public NegotiatingItem(NegotiatingDeal d)
        {
            ForDeal = d;
        }
    }
}
