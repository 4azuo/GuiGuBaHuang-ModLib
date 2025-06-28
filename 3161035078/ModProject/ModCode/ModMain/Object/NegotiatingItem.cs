using Newtonsoft.Json;
using System.Collections.Generic;

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
        [JsonIgnore]
        public List<DataProps.PropsData> NegotiatingSameProps => ForDeal.Buyer.GetUnitProps(NegotiatingPropPropId);
        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                var deal = ForDeal;
                return deal != null/* && deal.IsValid*/ && (NegotiatingProp != null || NegotiatingSameProps.Count > 0);
            }
        }

        public NegotiatingItem(NegotiatingDeal d)
        {
            ForDeal = d;
        }
    }
}
