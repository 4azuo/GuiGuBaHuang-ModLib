using ModLib.Enum;
using ModLib.Helper;
using ModLib.Object;
using MOD_nE7UL2.Const;
using MOD_nE7UL2.Mod;
using MOD_nE7UL2.Object;
using System.Linq;
using static MOD_nE7UL2.AutoTest.AutoTestHelper;
using static MOD_nE7UL2.AutoTest.TownTestHelper;

namespace MOD_nE7UL2.AutoTest.Scenarios.Town
{
    /// <summary>
    /// TG-46 (RealMarketEvent3): the player market end to end, through its windows like a player.
    /// Sell: VIP market (fee paid to the town), register an item (Register window), the same item cannot be registered
    /// twice, the listing is shown in "My Items" with its price, Up/Down change the price, an NPC makes an offer, the
    /// offer is shown in "Deals", the player opens the NPC deal and clicks "Deal" (money and item transferred).
    /// Buy: an NPC lists an item, "Market" shows the NPC, its sell list shows the item, the player makes an offer
    /// ("Up": first offer = seller price), the seller accepts it (monthly handler) and money/item move.
    /// The monthly random parts (NPCs joining, accept chance) are not driven, the deal they lead to is.
    /// Replaces the manual TM-07.
    /// </summary>
    public class TG46_PlayerMarket : IScenarioDefinition
    {
        /// <summary>Scenario id (see 3161035078/tests/scenarios.json).</summary>
        public string Id => "TG-46";

        private const int OFFER = 123;
        private const int NPC_PRICE = 50;

        private static int HerbId => g.conf.itemProps._allConfList.ToArray().First(x => x.IsHerbItem() != null && x.sale > 0).id;

        /// <summary>Active custom button of a game UI whose label is exactly the text (the newest one).</summary>
        private static UIItemButton Btn(UIType.UITypeBase t, string key) =>
            UIHelper.GetUICustomBase(t).SelectMany(x => x.Items).OfType<UIItemButton>()
                .LastOrDefault(x => IsActive(x) && (x.GetFormat() ?? string.Empty) == GameTool.LS(key));

        // a cover of an earlier opening can hold destroyed objects
        private static bool IsActive(UIItemButton x)
        {
            try { return x.Item != null && x.Item.gameObject.activeInHierarchy; }
            catch { return false; }
        }

        private static UIPropSelect PropSelect => g.ui.GetUI<UIPropSelect>(UIType.PropSelect);

        private static bool Lists(UIPropSelect ui, MarketItem item) =>
            ui != null && ui.allItems.allProps.ToArray().Any(x => item.IsPartialItem ? x.propsID == item.PropId : x.soleID == item.SoleId);

        private static int PlayerListings => RealMarketEvent3.Instance.MarketStack.Count(x => x.SellerId == Player.GetUnitId());

        private static void NoCancel(System.Action act)
        {
            var cancelRate = RealMarketEvent3.Configs.CancelDealLastMinuteRate;
            RealMarketEvent3.Configs.CancelDealLastMinuteRate = 0;
            try { act(); }
            finally { RealMarketEvent3.Configs.CancelDealLastMinuteRate = cancelRate; }
        }

        /// <summary>Builds the scenario and its steps.</summary>
        public Scenario Create()
        {
            WorldUnitBase npc = null;
            MarketItem item = null;
            DataProps.PropsData herb = null;
            MarketItem listing = null;
            DataProps.PropsData npcProp = null;
            return new Scenario { Id = "TG-46", Feature = "RealMarketEvent3", Title = "Player market: VIP, register, my items, price, deals, sell, buy, barter, buy orders", Overlay = false }
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

                // ------------------------------------------------------------ sell
                .Step("register", c =>
                {
                    CloseAll(UIType.PropSelect, UIType.NPCSearch);
                    Player.AddUnitProp(HerbId, 3);
                    var btn = FindButtons(UIType.TownMarket, GameTool.LS("other500020061")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'Register' button")) return;
                    btn.Item.onClick.Invoke();
                    var ui = PropSelect;
                    if (!c.Assert(ui != null && ui.onCustomSelectCall != null, "register window opened")) return;
                    herb = Player.GetUnitProps(HerbId).FirstOrDefault();
                    if (!c.Assert(herb != null, "a herb to sell")) return;
                    c.Assert(ui.allItems.allProps.ToArray().Any(x => x.propsID == HerbId), "the herb is listed in the register window");
                    c.Bag["listings"] = PlayerListings;
                    ui.onCustomSelectCall.Invoke(herb);
                    ui.btnOK.onClick.Invoke();
                    item = RealMarketEvent3.GetMarketItem(Player, herb);
                    if (!c.Assert(item != null, $"listing registered for {herb.propsInfoBase.name}")) return;
                    c.AssertEqual((int)c.Bag["listings"] + 1, PlayerListings, "one more listing of the player");
                    c.Assert(item.TownId == BagTown(c).buildData.id && item.Count >= 1 && item.SellerPrice.IsBetween(RealMarketEvent3.MIN_PRICE, RealMarketEvent3.MAX_PRICE),
                        $"listing: {item.Count} x {herb.propsInfoBase.name} for {item.SellerPrice} in {item.Town?.name}");
                    c.Assert(!g.ui.HasUI(UIType.PropSelect), "register window closed after OK");
                }, 1000)
                .Step("register twice", c =>
                {
                    if (item == null) { c.Skip("no listing"); return; }
                    ClickButton(CONFIRM); // "Registered sale" message
                    FindButtons(UIType.TownMarket, GameTool.LS("other500020061")).FirstOrDefault()?.Item.onClick.Invoke();
                    var ui = PropSelect;
                    if (!c.Assert(ui != null, "register window opened again")) return;
                    var listings = PlayerListings;
                    ui.onCustomSelectCall.Invoke(herb);
                    ui.btnOK.onClick.Invoke();
                    c.AssertEqual(listings, PlayerListings, "the same item cannot be registered twice");
                    c.Assert(g.ui.HasUI(UIType.PropSelect), "register window stays open (error message)");
                }, 1000)
                .Step("my items", c =>
                {
                    if (item == null) { c.Skip("no listing"); return; }
                    ClickButton(CONFIRM); // "You are selling this item at ..." message
                    CloseAll(UIType.PropSelect);
                    var btn = FindButtons(UIType.TownMarket, GameTool.LS("other500020060")).FirstOrDefault();
                    if (!c.Assert(btn != null, "'My Items' button")) return;
                    btn.Item.onClick.Invoke();
                    var ui = PropSelect;
                    if (!c.Assert(ui != null && RealMarketEvent3.IsOpenSellingList, "my sell list opened")) return;
                    c.Assert(Lists(ui, item), "the registered herb is shown in My Items");
                    ui.onCustomSelectCall.Invoke(ui.allItems.allProps.ToArray().First(x => x.propsID == item.PropId));
                }, 1000)
                .Step("price shown", c =>
                {
                    if (item == null || PropSelect == null) { c.Skip("no sell list"); return; }
                    var price = item.SellerPrice.ToString(ModConst.FORMAT_NUMBER);
                    var texts = FindTexts(PropSelect, price);
                    c.Assert(texts.Any(), $"selected listing info shows the price {price}: {string.Join(" | ", texts)}");
                })
                .Step("price up/down", c =>
                {
                    if (item == null || PropSelect == null) { c.Skip("no sell list"); return; }
                    var up = Btn(UIType.PropSelect, "other500020066");
                    var down = Btn(UIType.PropSelect, "other500020064");
                    if (!c.Assert(up != null && down != null, "'Up' and 'Down' buttons")) return;
                    item.SellerPrice = 1000;
                    up.Item.onClick.Invoke();
                    var raised = item.SellerPrice;
                    c.Assert(raised > 1000, $"Up raises the price: 1000 -> {raised}");
                    down.Item.onClick.Invoke();
                    c.Assert(item.SellerPrice < raised, $"Down lowers the price: {raised} -> {item.SellerPrice}");
                }, 1000)
                .Step("NPC offer", c =>
                {
                    if (item == null) { c.Skip("no listing"); return; }
                    npc = Npcs.FirstOrDefault(x => !HirePeopleEvent.IsHired(x));
                    if (!c.Assert(npc != null, "an NPC buyer")) return;
                    if (npc.GetUnitMoney() < OFFER * 10) npc.AddUnitMoney(OFFER * 10);
                    RealMarketEvent3.SetNegotiatingPrice(item, npc, OFFER);
                    c.AssertEqual(OFFER, RealMarketEvent3.GetNegotiatingPrice(item, npc), "first offer of the NPC keeps its price");
                })
                .Step("deals", c =>
                {
                    if (item == null || npc == null) { c.Skip("no offer"); return; }
                    var btn = Btn(UIType.PropSelect, "other500020092");
                    if (!c.Assert(btn != null, "'Deals' button")) return;
                    btn.Item.onClick.Invoke();
                    var search = g.ui.GetUI<UINPCSearch>(UIType.NPCSearch);
                    if (!c.Assert(search != null && RealMarketEvent3.IsOpenMySellingList, "buyers list opened")) return;
                    c.Assert(search.units.ToArray().Any(x => x.GetUnitId() == npc.GetUnitId()), $"the NPC buyer {npc.GetName()} is in the buyers list");
                }, 1000)
                .Step("open NPC deal", c =>
                {
                    if (item == null || npc == null || !g.ui.HasUI(UIType.NPCSearch)) { c.Skip("no buyers list"); return; }
                    // clicking a buyer opens its NPC info: Taoist replaces it by the deal list of that buyer
                    g.ui.OpenUISafe<UINPCInfo>(UIType.NPCInfo).InitData(npc, false);
                }, 1000)
                .Step("NPC deal list", c =>
                {
                    if (item == null || npc == null) { c.Skip("no offer"); return; }
                    // the deal list replaces the NPC info once it is opened (next frames)
                    var ui = PropSelect;
                    c.Assert(ui != null && RealMarketEvent3.IsOpenMyDeals, "deal list of the NPC opened");
                    c.Assert(ui != null && ui.allItems.allProps.ToArray().Any(x => x.propsID == ModLib.Const.ModLibConst.MONEY_PROP_ID && x.propsCount == OFFER),
                        $"the deal shows {OFFER} spirit stones");
                }, 1000)
                .Step("accept deal", c =>
                {
                    if (item == null || npc == null) { c.Skip("no offer"); return; }
                    var btn = Btn(UIType.PropSelect, "other500020074");
                    if (!c.Assert(btn != null, "'Deal' button")) return;
                    var price = RealMarketEvent3.GetNegotiatingPrice(item, npc);
                    var pMoney = Player.GetUnitMoney();
                    var nMoney = npc.GetUnitMoney();
                    var pHerbs = Player.GetUnitPropCount(item.PropId);
                    var nHerbs = npc.GetUnitPropCount(item.PropId);
                    var count = item.Count;
                    NoCancel(() => btn.Item.onClick.Invoke());
                    c.AssertEqual(pMoney + price, Player.GetUnitMoney(), $"seller (player) received {price}");
                    c.AssertEqual(nMoney - price, npc.GetUnitMoney(), "buyer (NPC) paid");
                    c.AssertEqual(pHerbs - count, Player.GetUnitPropCount(item.PropId), $"{count} item(s) left the player bag");
                    c.AssertEqual(nHerbs + count, npc.GetUnitPropCount(item.PropId), "the NPC received them");
                    c.Assert(!RealMarketEvent3.Instance.MarketStack.Contains(item), "listing removed");
                }, 1000)
                .Step("close deal", c =>
                {
                    if (item != null && npc != null)
                        c.Assert(g.ui.HasUI(UIType.DramaDialogue) || g.ui.HasUI(UIType.DramaFortuitous) || g.ui.HasUI(UIType.DramaDialogueBigTexture) || g.ui.HasUI(UIType.DramaBigTexture), "deal drama shown");
                    DismissDramas();
                    for (var i = 0; i < 3; i++) ClickButton(CONFIRM);
                    CloseAll(UIType.PropSelect, UIType.NPCInfo, UIType.NPCSearch);
                }, 1500)

                // ------------------------------------------------------------ buy
                .Step("NPC listing", c =>
                {
                    if (npc == null) { c.Skip("no NPC"); return; }
                    npc.AddUnitProp(HerbId, 2);
                    npcProp = npc.GetUnitProps(HerbId).FirstOrDefault();
                    if (!c.Assert(npcProp != null, "the NPC has an item to sell")) return;
                    // what the monthly handler does for an NPC seller in the town
                    listing = new MarketItem
                    {
                        SellerId = npc.GetUnitId(),
                        TownId = BagTown(c).buildData.id,
                        PropId = npcProp.propsID,
                        Count = 1,
                        SellerPrice = NPC_PRICE,
                        SoleId = npcProp.soleID,
                        CreateMonth = GameHelper.GetGameTotalMonth(),
                        IsPartialItem = npcProp.IsPartialItem() == CheckEnum.True,
                    };
                    RealMarketEvent3.Instance.MarketStack.Add(listing);
                    c.Assert(listing.IsValid, $"NPC listing: 1 x {npcProp.propsInfoBase.name} for {NPC_PRICE}");
                })
                .Step("market sellers", c =>
                {
                    if (listing == null) { c.Skip("no NPC listing"); return; }
                    var btn = Btn(UIType.TownMarket, "other500020040");
                    if (!c.Assert(btn != null, "'Market' button")) return;
                    btn.Item.onClick.Invoke();
                    var search = g.ui.GetUI<UINPCSearch>(UIType.NPCSearch);
                    if (!c.Assert(search != null && RealMarketEvent3.IsOpenMarket, "sellers list opened")) return;
                    c.Assert(search.units.ToArray().Any(x => x.GetUnitId() == npc.GetUnitId()), $"the NPC seller {npc.GetName()} is in the sellers list");
                }, 1000)
                .Step("NPC sell list", c =>
                {
                    if (listing == null || !g.ui.HasUI(UIType.NPCSearch)) { c.Skip("no sellers list"); return; }
                    g.ui.OpenUISafe<UINPCInfo>(UIType.NPCInfo).InitData(npc, false);
                }, 1000)
                .Step("select NPC item", c =>
                {
                    if (listing == null) { c.Skip("no NPC listing"); return; }
                    var ui = PropSelect;
                    if (!c.Assert(ui != null && RealMarketEvent3.IsOpenSellingList, "sell list of the NPC opened")) return;
                    c.Assert(Lists(ui, listing), "the NPC listing is shown");
                    ui.onCustomSelectCall.Invoke(ui.allItems.allProps.ToArray().First(x => x.propsID == listing.PropId));
                }, 1000)
                .Step("make offer", c =>
                {
                    if (listing == null || PropSelect == null) { c.Skip("no sell list"); return; }
                    var up = Btn(UIType.PropSelect, "other500020066");
                    if (!c.Assert(up != null && Btn(UIType.PropSelect, "other500020067") != null && Btn(UIType.PropSelect, "other500020077") != null,
                        "'Up', 'Negotiate' and 'Give up' buttons")) return;
                    if (Player.GetUnitMoney() < NPC_PRICE * 2) Player.AddUnitMoney(NPC_PRICE * 2);
                    up.Item.onClick.Invoke();
                    c.AssertEqual(NPC_PRICE, RealMarketEvent3.GetNegotiatingPrice(listing, Player), "player first offer = seller price");
                    c.Assert(FindTexts(PropSelect, NPC_PRICE.ToString(ModConst.FORMAT_NUMBER)).Any(), "the offer is shown in the sell list");
                }, 1000)
                .Step("seller accepts", c =>
                {
                    if (listing == null) { c.Skip("no NPC listing"); return; }
                    var price = RealMarketEvent3.GetNegotiatingPrice(listing, Player);
                    var pMoney = Player.GetUnitMoney();
                    var nMoney = npc.GetUnitMoney();
                    var pHerbs = Player.GetUnitPropCount(listing.PropId);
                    var nHerbs = npc.GetUnitPropCount(listing.PropId);
                    // the monthly handler accepts the best offer (random chance): done here directly
                    NoCancel(() => RealMarketEvent3.AcceptDeal(listing, RealMarketEvent3.GetNegotiatingHighestValue(listing)));
                    c.AssertEqual(pMoney - price, Player.GetUnitMoney(), $"player paid {price}");
                    c.AssertEqual(nMoney + price, npc.GetUnitMoney(), "the NPC seller received the money");
                    c.AssertEqual(pHerbs + 1, Player.GetUnitPropCount(listing.PropId), "player received the item");
                    c.AssertEqual(nHerbs - 1, npc.GetUnitPropCount(listing.PropId), "the item left the NPC bag");
                    c.Assert(!RealMarketEvent3.Instance.MarketStack.Contains(listing), "NPC listing removed");
                })
                // ------------------------------------------------------------ additions
                .Step("price history", c =>
                {
                    DismissDramas();
                    var avg = RealMarketEvent3.AvgPrice(HerbId);
                    c.Assert(avg != null, $"recent deal prices of the herb recorded (avg {avg})");
                    c.Assert(RealMarketEvent3.PriceHint(HerbId).Contains(avg?.ToString("#,##0") ?? "?"), $"hint: {RealMarketEvent3.PriceHint(HerbId)}");
                })
                .Step("offer notification", c =>
                {
                    if (npc == null) { c.Skip("no NPC"); return; }
                    Player.AddUnitProp(HerbId, 1);
                    var prop = Player.GetUnitProps(HerbId).First();
                    var mine = new MarketItem { SellerId = Player.GetUnitId(), TownId = BagTown(c).buildData.id, PropId = HerbId, Count = 1, SellerPrice = 100, SoleId = prop.soleID, CreateMonth = GameHelper.GetGameTotalMonth(), IsPartialItem = prop.IsPartialItem() == CheckEnum.True };
                    RealMarketEvent3.Instance.MarketStack.Add(mine);
                    var snapshot = RealMarketEvent3.SnapshotPlayerOffers();
                    RealMarketEvent3.SetNegotiatingPrice(mine, npc, 77);
                    var lines = RealMarketEvent3.NotifyNewOffers(snapshot);
                    c.Assert(lines.Count == 1 && lines[0].Contains("77") && lines[0].Contains(npc.GetName()), $"new offer notified: {lines.FirstOrDefault()}");
                    c.Assert(TaoistLogEvent.Instance.Entries.Any(x => x.Key == "market_offer_line"), "offer in the Taoist log");
                    c.AssertEqual(0, RealMarketEvent3.NotifyNewOffers(RealMarketEvent3.SnapshotPlayerOffers()).Count, "no new offer: no message");
                    RealMarketEvent3.CancelWithoutMessage(mine);
                }, 1500)
                .Step("barter", c =>
                {
                    ClickButton(CONFIRM);
                    if (npc == null) { c.Skip("no NPC"); return; }
                    // the NPC sells a herb, the player offers spirit stones and one of its own items
                    npc.AddUnitProp(HerbId, 1);
                    var sell = npc.GetUnitProps(HerbId).First();
                    var listing2 = new MarketItem { SellerId = npc.GetUnitId(), TownId = BagTown(c).buildData.id, PropId = HerbId, Count = 1, SellerPrice = 20, SoleId = sell.soleID, CreateMonth = GameHelper.GetGameTotalMonth(), IsPartialItem = sell.IsPartialItem() == CheckEnum.True };
                    RealMarketEvent3.Instance.MarketStack.Add(listing2);
                    var barterConf = g.conf.itemProps._allConfList.ToArray().First(x => x.IsMineItem() != null && x.sale > 0);
                    Player.AddUnitProp(barterConf.id, 1);
                    var barter = Player.GetUnitProps(barterConf.id).First();
                    if (Player.GetUnitMoney() < 100) Player.AddUnitMoney(100);
                    RealMarketEvent3.SetNegotiatingPrice(listing2, Player, 20);
                    RealMarketEvent3.AddNegotiatingItem(listing2, Player, barter, 1);
                    var pBarter = Player.GetUnitPropCount(barterConf.id); var nBarter = npc.GetUnitPropCount(barterConf.id);
                    NoCancel(() => RealMarketEvent3.AcceptDeal(listing2, RealMarketEvent3.GetNegotiatingValue(listing2, Player)));
                    c.AssertEqual(pBarter - 1, Player.GetUnitPropCount(barterConf.id), "the offered item left the player (buyer)");
                    c.AssertEqual(nBarter + 1, npc.GetUnitPropCount(barterConf.id), "the NPC (seller) received it");
                }, 1500)
                .Step("buy orders window", c =>
                {
                    DismissDramas();
                    ClickButton(CONFIRM);
                    var btn = Btn(UIType.TownMarket, "market_buyorders");
                    if (!c.Assert(btn != null, "\u0027Buy Orders\u0027 button")) return;
                    btn.Item.onClick.Invoke();
                }, 1500)
                .Step("new order item list", c =>
                {
                    c.Assert(g.ui.HasUI(UIType.TextInfoLong), "buy orders window");
                    var b = UIHelper.GetUICustomBase(UIType.TextInfoLong).SelectMany(x => x.Items).OfType<UIItemButton>().LastOrDefault(x => IsActive(x) && x.GetFormat() == GameTool.LS("market_buyorder_new"));
                    if (!c.Assert(b != null, "\u0027New buy order\u0027 button")) return;
                    b.Item.onClick.Invoke();
                }, 1500)
                .Step("orderable items", c =>
                {
                    var ui = PropSelect;
                    c.Assert(ui != null && ui.allItems.allProps.ToArray().Any(x => x.propsID == HerbId), "the item list offers the herb");
                    CloseAll(UIType.PropSelect, UIType.TextInfoLong);
                })
                .Step("buy order filled", c =>
                {
                    if (npc == null) { c.Skip("no NPC"); return; }
                    var town = BagTown(c);
                    if (Player.GetUnitMoney() < 200) Player.AddUnitMoney(200);
                    var m = Player.GetUnitMoney();
                    var order = RealMarketEvent3.AddBuyOrder(town, HerbId, 2, 30);
                    if (!c.Assert(order != null, "order placed")) return;
                    c.AssertEqual(m - 60, Player.GetUnitMoney(), "60 spirit stones held");
                    npc.AddUnitProp(HerbId, 3);
                    var nm = npc.GetUnitMoney(); var ph = Player.GetUnitPropCount(HerbId);
                    RealMarketEvent3.Instance.ProcessBuyOrders(new System.Collections.Generic.Dictionary<string, WorldUnitBase[]> { [town.buildData.id] = new[] { npc } }, true);
                    c.AssertEqual(ph + 2, Player.GetUnitPropCount(HerbId), "2 herbs delivered");
                    c.AssertEqual(nm + 60, npc.GetUnitMoney(), "the NPC paid from the held stones");
                    c.Assert(!RealMarketEvent3.Instance.BuyOrders.Contains(order), "filled order removed");
                }, 1500)
                .Step("buy order cancelled", c =>
                {
                    ClickButton(CONFIRM);
                    var town = BagTown(c);
                    if (Player.GetUnitMoney() < 200) Player.AddUnitMoney(200);
                    var m = Player.GetUnitMoney();
                    var order = RealMarketEvent3.AddBuyOrder(town, HerbId, 3, 20);
                    RealMarketEvent3.CancelBuyOrder(order);
                    c.AssertEqual(m, Player.GetUnitMoney(), "cancel: the held stones come back");
                    var keys = TaoistLogEvent.Instance.Entries.Select(x => x.Key).ToList();
                    c.Assert(new[] { "log_market_sold", "log_market_bought", "log_buyorder_new", "market_buyorder_filled", "log_buyorder_cancel" }.All(keys.Contains), "market events in the Taoist log");
                })
                .Step("close", c =>
                {
                    DismissDramas();
                    for (var i = 0; i < 3; i++) ClickButton(CONFIRM);
                    CloseAll(UIType.PropSelect, UIType.NPCInfo, UIType.NPCSearch, UIType.TownMarket);
                    c.Assert(!g.ui.HasUI(UIType.TownMarket), "market closed");
                }, 1500);
        }
    }
}
