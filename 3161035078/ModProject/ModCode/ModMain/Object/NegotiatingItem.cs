using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

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
                return GetPropInfo().sale * Count;
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
                return deal != null/* && deal.IsValid*/ && GetPropInfo() != null;
            }
        }

        public List<DataProps.PropsData> GetProps()
        {
            var p = NegotiatingProp;
            if (p == null)
                return new List<DataProps.PropsData>();
            return RealMarketEvent3.IsPartialItem(p) ? NegotiatingSameProps : new List<DataProps.PropsData> { p };
        }

        public UIIconTool.PropsInfoDataBase GetPropInfo()
        {
            return GetProps().FirstOrDefault()?.propsInfoBase;
        }

        public NegotiatingItem(NegotiatingDeal d)
        {
            ForDeal = d;
        }
    }
}
