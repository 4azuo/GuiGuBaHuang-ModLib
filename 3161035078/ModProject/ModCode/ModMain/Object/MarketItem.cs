using MOD_nE7UL2.Mod;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MOD_nE7UL2.Object
{
    public class MarketItem
    {
        public string SellerId { get; set; }
        public string TownId { get; set; }
        public int PropId { get; set; }
        public DataProps.PropsDataType DataType { get; set; }
        public int[] DataValues { get; set; }
        public int Count { get; set; }
        public int SellerPrice { get; set; }
        public string SoleId { get; set; }
        public int CreateMonth { get; set; }

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
            var p = Prop;
            if (p == null)
                return new List<DataProps.PropsData>();
            return RealMarketEvent3.IsPartialItem(Prop) ? SameProps : new List<DataProps.PropsData> { Prop };
        }

        public UIIconTool.PropsInfoDataBase GetPropInfo()
        {
            return GetProps().FirstOrDefault()?.propsInfoBase;
        }
    }
}
