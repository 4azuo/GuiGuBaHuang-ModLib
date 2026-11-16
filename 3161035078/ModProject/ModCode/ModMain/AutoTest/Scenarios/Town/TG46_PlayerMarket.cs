using ModLib.Enum;
using ModLib.Helper;
using MOD_nE7UL2.Mod;
using MOD_nE7UL2.Object;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-46 (RealMarketEvent3): the player market end to end. VIP market (fee paid to the town), register an item
    /// through the Register window, an NPC makes an offer, the deal is accepted (money and item transferred), then
    /// the player buys an NPC listing at its price. The monthly random offers are not driven (random), the deal
    /// they lead to is. Replaces the manual TM-07.
    /// </summary>
    public class TG46_PlayerMarket : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-46";

        private const int OFFER = 123;
        private const int NPC_PRICE = 50;

        private static int HerbId => g.conf.itemProps._allConfList.ToArray().First(x => x.IsHerbItem() != null && x.sale > 0).id;

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase npc = null;
            MarketItem item = null;
            return new Scenario { Id = "TG-46", Feature = "RealMarketEvent3", Title = "Player market: VIP, register, NPC offer, deal, buy", Overlay = false }
                .Step("travel", c => TravelTo(c, FindTown(t => t.GetBuildSub<MapBuildTownMarket>() != null)))
                .Step("open market", c =>
                {
                    if (SMLocalConfigsEvent.Instance.Configs.NoMarketItem) { c.Skip("player market disabled (NoMarketItem)"); return; }
                    OpenTownUI<UITownMarket>(UIType.TownMarket, ui => ui.InitData(BagTown(c)));
                })
                .Step("buy VIP", c =>
                {
                    var btn = FindButtons(UIType.TownMarket, GameTool.LS("other500020085")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'VIP Market' button")) return;
                    var vip = RealMarketEvent3.VIPCost(BagTown(c).gridData.areaBaseID);
                    if (Player.GetUnitMoney() < vip * 2) Player.AddUnitMoney(vip * 2);
                    c.Bag["vip"] = vip;
                    c.Bag["money"] = Player.GetUnitMoney();
                    c.Bag["budget"] = MapBuildPropertyEvent.GetBuildProperty(BagTown(c));
                    btn.Item.onClick.Invoke();
                })
                .Step("confirm VIP", c => c.Info($"clicked '{ClickButton("Yes", "是", "OK", "Enable")}'"), 1000)
                .Step("VIP paid", c =>
                {
                    if (!c.Bag.ContainsKey("vip")) { c.Skip("VIP not bought"); return; }
                    var vip = (int)c.Bag["vip"];
                    c.AssertEqual((int)c.Bag["money"] - vip, Player.GetUnitMoney(), $"VIP fee {vip} paid");
                    c.AssertEqual((long)c.Bag["budget"] + vip, MapBuildPropertyEvent.GetBuildProperty(BagTown(c)), "fee added to the town budget");
                    c.Assert(g.ui.HasUI(UIType.PropSelect) || g.ui.HasUI(UIType.NPCSearch) || RealMarketEvent3.IsRealMarket3Open(), "VIP listings opened");
                }, 1500)
                .Step("register", c =>
                {
                    CloseAll(UIType.PropSelect, UIType.NPCSearch);
                    Player.AddUnitProp(HerbId, 3);
                    var btn = FindButtons(UIType.TownMarket, GameTool.LS("other500020061")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'Register' button")) return;
                    btn.Item.onClick.Invoke();
                    var ui = g.ui.GetUI<UIPropSelect>(UIType.PropSelect);
                    if (!c.Assert(ui != null && ui.onCustomSelectCall != null, "register window opened")) return;
                    var prop = Player.GetUnitProps(HerbId).FirstOrDefault();
                    if (!c.Assert(prop != null, "a herb to sell")) return;
                    ui.onCustomSelectCall.Invoke(prop);
                    ui.btnOK.onClick.Invoke();
                    item = RealMarketEvent3.GetMarketItem(Player, prop);
                    if (!c.Assert(item != null, $"listing registered for {prop.propsInfoBase.name}")) return;
                    c.Info($"listing: {item.Count} x {prop.propsInfoBase.name} for {item.SellerPrice} in {item.Town?.name}");
                }, 1000)
                .Step("NPC offer", c =>
                {
                    if (item == null) { c.Skip("no listing"); return; }
                    CloseAll(UIType.PropSelect);
                    npc = Npcs.FirstOrDefault(x => !HirePeopleEvent.IsHired(x));
                    if (!c.Assert(npc != null, "an NPC buyer")) return;
                    if (npc.GetUnitMoney() < OFFER * 10) npc.AddUnitMoney(OFFER * 10);
                    RealMarketEvent3.SetNegotiatingPrice(item, npc, OFFER);
                    c.AssertEqual(OFFER, RealMarketEvent3.GetNegotiatingPrice(item, npc), "first offer of the NPC keeps its price");
                })
                .Step("accept deal", c =>
                {
                    if (item == null || npc == null) { c.Skip("no offer"); return; }
                    var deal = RealMarketEvent3.GetNegotiatingValue(item, npc);
                    var cancelRate = RealMarketEvent3.Configs.CancelDealLastMinuteRate;
                    var price = RealMarketEvent3.GetNegotiatingPrice(item, npc);
                    var pMoney = Player.GetUnitMoney();
                    var nMoney = npc.GetUnitMoney();
                    var pHerbs = Player.GetUnitPropCount(item.PropId);
                    var nHerbs = npc.GetUnitPropCount(item.PropId);
                    var count = item.Count;
                    RealMarketEvent3.Configs.CancelDealLastMinuteRate = 0;
                    try { RealMarketEvent3.AcceptDeal(item, deal); }
                    finally { RealMarketEvent3.Configs.CancelDealLastMinuteRate = cancelRate; }
                    c.AssertEqual(pMoney + price, Player.GetUnitMoney(), $"seller (player) received {price}");
                    c.AssertEqual(nMoney - price, npc.GetUnitMoney(), "buyer (NPC) paid");
                    c.AssertEqual(pHerbs - count, Player.GetUnitPropCount(item.PropId), $"{count} item(s) left the player bag");
                    c.AssertEqual(nHerbs + count, npc.GetUnitPropCount(item.PropId), "the NPC received them");
                    c.Assert(!RealMarketEvent3.Instance.MarketStack.Contains(item), "listing removed");
                })
                .Step("close deal drama", c => { for (var i = 0; i < 3; i++) ClickButton(CONFIRM); }, 1500)
                .Step("buy NPC listing", c =>
                {
                    if (npc == null) { c.Skip("no NPC"); return; }
                    npc.AddUnitProp(HerbId, 2);
                    var prop = npc.GetUnitProps(HerbId).FirstOrDefault();
                    if (!c.Assert(prop != null, "the NPC has an item to sell")) return;
                    var listing = new MarketItem
                    {
                        SellerId = npc.GetUnitId(),
                        TownId = BagTown(c).buildData.id,
                        PropId = prop.propsID,
                        Count = 1,
                        SellerPrice = NPC_PRICE,
                        SoleId = prop.soleID,
                        CreateMonth = GameHelper.GetGameTotalMonth(),
                        IsPartialItem = prop.IsPartialItem() == CheckEnum.True,
                    };
                    RealMarketEvent3.Instance.MarketStack.Add(listing);
                    RealMarketEvent3.SetNegotiatingPrice(listing, Player, 0);
                    c.AssertEqual(NPC_PRICE, RealMarketEvent3.GetNegotiatingPrice(listing, Player), "player first offer = seller price");
                    if (Player.GetUnitMoney() < NPC_PRICE * 2) Player.AddUnitMoney(NPC_PRICE * 2);
                    var pMoney = Player.GetUnitMoney();
                    var pHerbs = Player.GetUnitPropCount(prop.propsID);
                    var cancelRate = RealMarketEvent3.Configs.CancelDealLastMinuteRate;
                    RealMarketEvent3.Configs.CancelDealLastMinuteRate = 0;
                    try { RealMarketEvent3.AcceptDeal(listing, RealMarketEvent3.GetNegotiatingValue(listing, Player)); }
                    finally { RealMarketEvent3.Configs.CancelDealLastMinuteRate = cancelRate; }
                    c.AssertEqual(pMoney - NPC_PRICE, Player.GetUnitMoney(), $"player paid {NPC_PRICE}");
                    c.AssertEqual(pHerbs + 1, Player.GetUnitPropCount(prop.propsID), "player received the item");
                })
                .Step("close", c =>
                {
                    for (var i = 0; i < 3; i++) ClickButton(CONFIRM);
                    CloseAll(UIType.PropSelect, UIType.NPCSearch, UIType.TownMarket);
                    c.Assert(!g.ui.HasUI(UIType.TownMarket), "market closed");
                }, 1500);
        }
    }
}
