using MOD_nE7UL2.Mod;
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
        public int Value
        {
            get
            {
                if (!IsValid)
                    return 0;
                if (NegotiatingPropSoleId == null)
                    return Count;
                var sale = RealMarketEvent3.IsPartialItem(NegotiatingProp) ? NegotiatingSameProps[0].propsInfoBase.sale : NegotiatingProp.propsInfoBase.sale;
                return sale * Count;
            }
        }
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
                return deal != null/* && deal.IsValid*/ && (RealMarketEvent3.IsPartialItem(NegotiatingProp) ? NegotiatingSameProps.Count > 0 : NegotiatingProp != null);
            }
        }

        public NegotiatingItem(NegotiatingDeal d)
        {
            ForDeal = d;
        }
    }
}
