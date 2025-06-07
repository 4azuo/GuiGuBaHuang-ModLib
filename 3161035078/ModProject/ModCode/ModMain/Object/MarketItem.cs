using Newtonsoft.Json;

namespace MOD_nE7UL2.Object
{
    public class MarketItem
    {
        public string SellerId { get; set; }
        public int PropId { get; set; }
        public DataProps.PropsDataType DataType { get; set; }
        public int[] DataValues { get; set; }
        public int Count { get; set; }
        public int Price { get; set; }
        public string SoleId { get; set; }

        [JsonIgnore]
        public WorldUnitBase Seller => g.world.unit.GetUnit(SellerId);
        [JsonIgnore]
        public DataProps.PropsData Prop => Seller.GetUnitProp(SoleId);
    }
}
