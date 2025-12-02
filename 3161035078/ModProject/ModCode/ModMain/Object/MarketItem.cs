using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using ModLib.Helper;

namespace MOD_nE7UL2.Object
{
    public class MarketItem
    {
        public string SellerId { get; set; }
        public string TownId { get; set; }
        public int PropId { get; set; }
        //public DataProps.PropsDataType DataType { get; set; }
        //public string DataValues { get; set; }
        public int Count { get; set; }
        public int SellerPrice { get; set; }
        public string SoleId { get; set; }
        public int CreateMonth { get; set; }
        public bool IsPartialItem { get; set; }
        public bool IsHidden { get; set; } = false;

        [JsonIgnore]
        public List<NegotiatingDeal> Deals
        {
            get
            {
                return RealMarketEvent3.Instance.NegotiatingDeals.Where(x => x.TargetProp == this).ToList();
            }
        }
        [JsonIgnore]
        public WorldUnitBase Seller => g.world.unit.GetUnit(SellerId);
        [JsonIgnore]
        public MapBuildTown Town => g.world.build.GetBuild<MapBuildTown>(TownId);
        [JsonIgnore]
        public DataProps.PropsData Prop => Seller.GetUnitProp(SoleId);
        [JsonIgnore]
        public List<DataProps.PropsData> SameProps => Seller.GetUnitProps(PropId);
        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                var seller = Seller;
                return seller != null && !seller.isDie && Town != null && GetPropInfo() != null;
            }
        }

        public List<DataProps.PropsData> GetProps()
        {
            return IsPartialItem ? SameProps : new List<DataProps.PropsData> { Prop };
        }

        public UIIconTool.PropsInfoDataBase GetPropInfo()
        {
            return GetProps().FirstOrDefault()?.propsInfoBase;
        }
    }
}
