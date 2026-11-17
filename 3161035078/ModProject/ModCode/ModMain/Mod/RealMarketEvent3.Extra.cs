using MOD_nE7UL2.Const;
using MOD_nE7UL2.Object;
using ModLib.Helper;
using ModLib.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Player market additions:
    /// - Price history: the unit price of the last deals of each item (shown in the Register window and the sell lists).
    /// - Offer notifications: after the monthly NPC buyers, one message lists the new/raised offers on the player
    ///   listings (Taoist Settings: NotifyMarketOffers).
    /// - Buy orders: the player asks for an item at a unit price in a town (the spirit stones are held at once). Each
    ///   month NPCs of that town who own the item may sell it to the order (the better the price compared to the recent
    ///   deals, the more likely). Unfilled orders expire after 12 months and are refunded.
    /// </summary>
    public partial class RealMarketEvent3
    {
        public const int PRICE_HISTORY = 10;
        public const int BUY_ORDER_MONTHS = 12;
        public const int MAX_BUY_ORDERS = 8;
        public const float BUY_ORDER_RATE = 25f;

        public class BuyOrder
        {
            public string TownId { get; set; }
            public int PropId { get; set; }
            public int Count { get; set; }
            /// <summary>Spirit stones per item.</summary>
            public int Price { get; set; }
            public int CreateMonth { get; set; }

            public MapBuildTown Town => g.world.build.GetBuild(TownId)?.TryCast<MapBuildTown>();
            public string Name => GameTool.LS(g.conf.itemProps.GetItem(PropId)?.name ?? PropId.ToString());
        }

        /// <summary>Unit prices of the last deals, by item id.</summary>
        public Dictionary<int, List<int>> PriceHistory { get; set; } = new Dictionary<int, List<int>>();
        public List<BuyOrder> BuyOrders { get; set; } = new List<BuyOrder>();

        // ------------------------------------------------------------------ price history

        /// <summary>Records a deal (total value for count items).</summary>
        public static void RecordPrice(int propId, int totalValue, int count)
        {
            if (Instance == null || totalValue <= 0)
                return;
            if (!Instance.PriceHistory.ContainsKey(propId))
                Instance.PriceHistory[propId] = new List<int>();
            var list = Instance.PriceHistory[propId];
            list.Add(Math.Max(1, totalValue / Math.Max(1, count)));
            if (list.Count > PRICE_HISTORY)
                list.RemoveRange(0, list.Count - PRICE_HISTORY);
        }

        /// <summary>Average unit price of the recent deals (null when none).</summary>
        public static int? AvgPrice(int propId)
        {
            if (Instance == null || !Instance.PriceHistory.ContainsKey(propId) || Instance.PriceHistory[propId].Count == 0)
                return null;
            return (int)Instance.PriceHistory[propId].Average();
        }

        /// <summary>"Recent deals: avg X (n)" or "No recent deal".</summary>
        public static string PriceHint(int propId)
        {
            var avg = AvgPrice(propId);
            if (avg == null)
                return GameTool.LS("market_nohistory");
            var list = Instance.PriceHistory[propId];
            return string.Format(GameTool.LS("market_history"), avg.Value, list.Min(), list.Max(), list.Count);
        }

        // ------------------------------------------------------------------ offer notifications

        /// <summary>Best offer and buyer count of each player listing.</summary>
        public static Dictionary<MarketItem, Tuple<int, int>> SnapshotPlayerOffers()
        {
            var playerId = g.world.playerUnit.GetUnitId();
            return Instance.MarketStack.Where(x => x.SellerId == playerId).ToDictionary(
                x => x,
                x => Tuple.Create(x.Deals.Count(d => d.IsValid), GetNegotiatingHighestValue(x)?.TotalValue ?? 0));
        }

        /// <summary>New/raised offers since the snapshot: logged, and one message when the setting is on.</summary>
        public static List<string> NotifyNewOffers(Dictionary<MarketItem, Tuple<int, int>> before)
        {
            var lines = new List<string>();
            foreach (var kv in before)
            {
                var item = kv.Key;
                if (!Instance.MarketStack.Contains(item))
                    continue;
                var best = GetNegotiatingHighestValue(item);
                var buyers = item.Deals.Count(d => d.IsValid);
                if (best == null || (buyers <= kv.Value.Item1 && best.TotalValue <= kv.Value.Item2))
                    continue;
                var name = item.GetPropInfo()?.name;
                lines.Add(string.Format(GameTool.LS("market_offer_line"), name, best.Buyer?.GetName(), best.TotalValue, buyers));
                TaoistLogEvent.Log("market_offer_line", name, best.Buyer?.GetName(), best.TotalValue, buyers);
            }
            if (lines.Count > 0 && (UISettingScreenEvent.Instance?.NotifyMarketOffers ?? true))
                g.ui.MsgBox(GameTool.LS("market_offer_title"), string.Join("\n", lines));
            return lines;
        }

        // ------------------------------------------------------------------ buy orders

        /// <summary>
        /// New order (the spirit stones are held at once). Null when the player cannot pay, has too many orders or the
        /// values are wrong.
        /// </summary>
        public static BuyOrder AddBuyOrder(MapBuildTown town, int propId, int count, int price)
        {
            var player = g.world.playerUnit;
            var total = (long)count * price;
            if (SMLocalConfigsEvent.Instance.Configs.NoBuyOrder || town == null || count <= 0 || price < MIN_PRICE || total > int.MaxValue || player.GetUnitMoney() < total || Instance.BuyOrders.Count >= MAX_BUY_ORDERS)
                return null;
            player.AddUnitMoney(-(int)total);
            var order = new BuyOrder { TownId = town.buildData.id, PropId = propId, Count = count, Price = price, CreateMonth = GameHelper.GetGameTotalMonth() };
            Instance.BuyOrders.Add(order);
            TaoistLogEvent.Log("log_buyorder_new", count, order.Name, price, town.name);
            DebugHelper.WriteLine($"RealMarketEvent3: buy order {count} x {order.Name} at {price} in {town.name}");
            return order;
        }

        /// <summary>Cancels an order: the held spirit stones of the remaining items come back.</summary>
        public static void CancelBuyOrder(BuyOrder order, string logKey = "log_buyorder_cancel")
        {
            if (order == null || !Instance.BuyOrders.Remove(order))
                return;
            var refund = order.Count * order.Price;
            g.world.playerUnit.AddUnitMoney(refund);
            TaoistLogEvent.Log(logKey, order.Count, order.Name, refund);
        }

        /// <summary>Chance (%) that an NPC owning the item sells it to the order this month.</summary>
        public static float BuyOrderChance(BuyOrder order)
        {
            var reference = AvgPrice(order.PropId) ?? Math.Max(1, g.conf.itemProps.GetItem(order.PropId)?.sale ?? 1);
            return Math.Min(90f, BUY_ORDER_RATE * order.Price / (float)reference);
        }

        /// <summary>
        /// Monthly: expired orders are refunded; NPCs in the order town owning the item may sell it (paid from the held
        /// stones). force: every owner sells (autotest).
        /// </summary>
        public void ProcessBuyOrders(Dictionary<string, WorldUnitBase[]> wunitsInTowns, bool force = false)
        {
            var curMonth = GameHelper.GetGameTotalMonth();
            var player = g.world.playerUnit;
            var filled = new List<string>();
            foreach (var order in BuyOrders.ToArray())
            {
                if (curMonth - order.CreateMonth >= BUY_ORDER_MONTHS)
                {
                    CancelBuyOrder(order, "log_buyorder_expired");
                    continue;
                }
                WorldUnitBase[] units;
                if (wunitsInTowns == null || !wunitsInTowns.TryGetValue(order.TownId, out units))
                {
                    var town = order.Town;
                    units = town == null ? new WorldUnitBase[0] : g.world.unit.GetUnitExact(town.GetOrigiPoint(), Configs.JoinRange).ToArray();
                }
                var chance = BuyOrderChance(order);
                foreach (var seller in units)
                {
                    if (order.Count <= 0)
                        break;
                    if (seller == null || seller.isDie || seller.IsPlayer())
                        continue;
                    if (!force && !CommonTool.Random(0f, 100f).IsBetween(0f, chance))
                        continue;
                    var props = seller.GetUnequippedProps().Where(x => x.propsID == order.PropId && IsSellableItem(x)).ToArray();
                    if (props.Length == 0)
                        continue;
                    var n = Math.Min(order.Count, props.Sum(x => x.propsCount));
                    var sold = 0;
                    foreach (var prop in props)
                    {
                        if (sold >= n) break;
                        if (prop.IsPartialItem() == ModLib.Enum.CheckEnum.True)
                        {
                            var k = Math.Min(n - sold, prop.propsCount);
                            player.AddUnitProp(prop.propsID, k);
                            seller.RemoveUnitProp(prop.propsID, k);
                            sold += k;
                        }
                        else
                        {
                            player.AddUnitProp(prop);
                            seller.RemoveUnitProp(prop.soleID);
                            sold++;
                        }
                    }
                    seller.AddUnitMoney(sold * order.Price);
                    order.Count -= sold;
                    RecordPrice(order.PropId, sold * order.Price, sold);
                    seller.data.unitData.relationData.AddIntim(player.GetUnitId(), 5);
                    filled.Add(string.Format(GameTool.LS("market_buyorder_filled"), sold, order.Name, seller.GetName(), sold * order.Price));
                    TaoistLogEvent.Log("market_buyorder_filled", sold, order.Name, seller.GetName(), sold * order.Price);
                }
                if (order.Count <= 0)
                    BuyOrders.Remove(order);
            }
            if (filled.Count > 0 && (UISettingScreenEvent.Instance?.NotifyMarketOffers ?? true))
                g.ui.MsgBox(GameTool.LS("market_buyorders"), string.Join("\n", filled));
        }

        // ------------------------------------------------------------------ buy orders UI

        /// <summary>Items that can be ordered: seen in deals or listings, and herbs/ores/pills up to the player's grade + 1.</summary>
        public static List<int> OrderableItems()
        {
            var maxGrade = g.world.playerUnit.GetGradeLvl() + 1;
            var ids = new HashSet<int>(Instance.PriceHistory.Keys);
            foreach (var m in Instance.MarketStack) ids.Add(m.PropId);
            foreach (var conf in g.conf.itemProps._allConfList.ToArray())
                if (conf.sale > 0 && (conf.IsHerbItem() != null || conf.IsMineItem() != null || conf.IsPill() != null) && conf.Grade() <= maxGrade)
                    ids.Add(conf.id);
            return ids.Where(x => x != ModLib.Const.ModLibConst.MONEY_PROP_ID && g.conf.itemProps.GetItem(x) != null).ToList();
        }

        /// <summary>Buy orders window of a town: the player's orders (cancel), a new order.</summary>
        public void OpenBuyOrders(MapBuildTown town)
        {
            if (town == null)
                return;
            g.ui.CloseUI(UIType.TextInfoLong);
            var ui = new UICustom1(GameTool.LS("market_buyorders"));
            var row = ui.FirstRow + 1;
            ui.AddText(ui.MidCol, row++, string.Format(GameTool.LS("market_buyorders_desc"), BUY_ORDER_MONTHS)).Format(null, 14);
            row++;
            var orders = BuyOrders.ToArray();
            if (orders.Length == 0)
                ui.AddText(ui.MidCol, row++, GameTool.LS("market_buyorders_none")).Format(null, 15);
            foreach (var order in orders)
            {
                var o = order;
                ui.AddText(ui.MidCol - 3, row, string.Format(GameTool.LS("market_buyorder_line"), o.Count, o.Name, o.Price, o.Town?.name,
                    BUY_ORDER_MONTHS - (GameHelper.GetGameTotalMonth() - o.CreateMonth))).Format(null, 15);
                ui.AddButton(ui.MidCol + 9, row++, () =>
                {
                    CancelBuyOrder(o);
                    OpenBuyOrders(town);
                }, GameTool.LS("other500020090")).Size(120, 32);
            }
            row++;
            ui.AddButton(ui.MidCol, row, () => SelectOrderItem(town), GameTool.LS("market_buyorder_new")).Size(220, 40);
        }

        /// <summary>New order step 1: choose the item.</summary>
        private void SelectOrderItem(MapBuildTown town)
        {
            g.ui.CloseUI(UIType.TextInfoLong);
            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.Set(GameTool.LS("market_buyorder_new"));
            ui.textSearchTip.Set(GameTool.LS("market_buyorder_pick"));
            ui.btnSearch.gameObject.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.selectOnePropID = true;
            ui.allItems.ClearAllProps();
            foreach (var id in OrderableItems())
                ui.allItems.AddProps(DataProps.PropsData.NewProps(id, 1));
            DataProps.PropsData selected = null;
            ui.onCustomSelectCall = ActionHelper.TracedFunc<DataProps.PropsData, string>(x =>
            {
                ui.ClearSelectItem();
                ui.AddSelectProps(x);
                ui.UpdateUI();
                selected = x;
                return x.propsInfoBase.name;
            });
            ui.btnOK.onClick.RemoveAllListeners();
            ui.btnOK.onClick.AddListener(ActionHelper.TracedUnityAction(() =>
            {
                if (selected == null)
                    return;
                var id = selected.propsID;
                g.ui.CloseUI(UIType.PropSelect);
                OpenOrderPrice(town, id);
            }));
            ui.UpdateUI();
        }

        /// <summary>New order step 2: count and unit price, OK holds the spirit stones.</summary>
        private void OpenOrderPrice(MapBuildTown town, int propId)
        {
            var conf = g.conf.itemProps.GetItem(propId);
            var reference = AvgPrice(propId) ?? Math.Max(MIN_PRICE, conf?.sale ?? MIN_PRICE);
            var ui = new UICustom1(GameTool.LS("market_buyorder_new"));
            var row = ui.FirstRow + 2;
            ui.AddText(ui.MidCol, row++, GameTool.LS(conf?.name ?? string.Empty)).Format(null, 18);
            ui.AddText(ui.MidCol, row++, PriceHint(propId)).Format(null, 14);
            row++;
            var count = ui.AddCompositeSlider(ui.MidCol - 3, row++, GameTool.LS("other500020096"), 1, 20, 1, "{0}");
            var price = ui.AddCompositeSlider(ui.MidCol - 3, row++, GameTool.LS("market_buyorder_unitprice"), MIN_PRICE, Math.Max(MIN_PRICE * 2, reference * 3), reference, "{0}");
            count.Postfix.SetWork(new UIItemWork { Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { count.MainComponent.Get().Parse<int>() }) });
            price.Postfix.SetWork(new UIItemWork { Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x => new object[] { price.MainComponent.Get().Parse<int>() }) });
            ui.AddText(ui.MidCol, row++, "{0}").SetWork(new UIItemWork
            {
                Formatter = ActionHelper.WTracedFunc<UIItemBase, object[]>(x =>
                {
                    var o = new BuyOrder { PropId = propId, Price = price.MainComponent.Get().Parse<int>() };
                    var total = count.MainComponent.Get().Parse<int>() * o.Price;
                    return new object[] { string.Format(GameTool.LS("market_buyorder_total"), total, BuyOrderChance(o)) };
                })
            });
            ui.IsAutoUpdate = true;
            row++;
            ui.AddButton(ui.MidCol, row, () =>
            {
                var order = AddBuyOrder(town, propId, count.MainComponent.Get().Parse<int>(), price.MainComponent.Get().Parse<int>());
                if (order == null)
                {
                    g.ui.MsgBox(GameTool.LS("other500020102"), string.Format(GameTool.LS("market_buyorder_fail"), MAX_BUY_ORDERS));
                    return;
                }
                OpenBuyOrders(town);
            }, GameTool.LS("market_buyorder_place")).Size(220, 40);
        }
    }
}
