using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Object
{
    public class NegotiatingItem
    {
        public NegotiatingDeal ForDeal { get; set; }
        public int NegotiatingPropPropId { get; set; } = 0;
        public string NegotiatingPropSoleId { get; set; } = null;
        public int Count { get; set; }
        public bool IsPartialItem { get; set; } = false;
        public bool IsSpiritStones { get; set; } = true;

        [JsonIgnore]
        public int Value
        {
            get
            {
                if (!IsValid)
                    return 0;
                if (IsSpiritStones)
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
                if (IsSpiritStones)
                    return true;
                return ForDeal != null && GetPropInfo() != null;
            }
        }

        public List<DataProps.PropsData> GetProps()
        {
            return IsPartialItem ? NegotiatingSameProps : new List<DataProps.PropsData> { NegotiatingProp };
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
