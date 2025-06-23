using System.Linq;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Object;
using System.Collections.Generic;
using ModLib.Const;
using MOD_nE7UL2.Object;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// NPC Market
    /// </summary>
    [Cache(ModConst.REAL_MARKET_EVENT3)]
    public class RealMarketEvent3 : ModEvent
    {
        public static RealMarketEvent3 Instance { get; set; }
        public static bool isNpcMarket = false;

        public const int MAX_PRICE = 100000000;
        public const int MIN_PRICE = 1;

        //temp
        private string sellerId;
        private DataProps.PropsData selectedProp;
        private MarketItem selectedMarketItem;
        private Dictionary<DataProps.PropsData, NegotiatingDeal> propToDeal;

        //data
        public List<NegotiatingDeal> NegotiatingDeals { get; set; } = new List<NegotiatingDeal>();
        public List<MarketItem> MarketStack { get; set; } = new List<MarketItem>();

        public const int JOIN_RANGE = 4;
        public const float SELLER_JOIN_MARKET_RATE = 20f;
        public const float SELL_RATE = 5f; //depend on grade
        public const float BUYER_JOIN_MARKET_RATE = 30f;
        public const float BUY_RATE = 3f; //depend on grade
        public const float GET_DEAL_RATE = 40f; //depend on price
        public const float CANCEL_DEAL_RATE = 2f; //depend on month

        public static bool IsSellableItem(DataProps.PropsData x)
        {
            return x.propsInfoBase.sale > 0 && ModLibConst.MONEY_PROP_ID != x.propsID && ModLibConst.CONTRIBUTION_PROP_ID != x.propsID && ModLibConst.MAYOR_DEGREE_PROP_ID != x.propsID;
        }

        public static MarketItem GetMarketItem(string sellerId, string propSoleId)
        {
            return Instance.MarketStack.FirstOrDefault(x => x.SellerId == sellerId && x.SoleId == propSoleId);
        }

        public static NegotiatingDeal GetBuyerDeal(MarketItem prop, string buyerId)
        {
            var deal = Instance.NegotiatingDeals.FirstOrDefault(x => x.TargetProp == prop && x.BuyerId == buyerId);
            if (deal == null)
            {
                deal = new NegotiatingDeal(buyerId, prop);
                Instance.NegotiatingDeals.Add(deal);
            }
            return deal;
        }

        public static void AddNegotiatingItem(MarketItem prop, string buyerId, int propId, string negotiatingPropSoleId, int count)
        {
            var curDeal = GetBuyerDeal(prop, buyerId);
            curDeal.Items.Add(new NegotiatingItem(curDeal) { NegotiatingPropPropId = propId, NegotiatingPropSoleId = negotiatingPropSoleId, Count = count });
        }

        public static bool RemoveNegotiatingItem(MarketItem prop, string buyerId, string negotiatingPropSoleId)
        {
            var curDeal = GetBuyerDeal(prop, buyerId);
            return curDeal.Items.RemoveAll(x => x.ForDeal == curDeal && x.NegotiatingPropSoleId == negotiatingPropSoleId) > 0;
        }

        public static NegotiatingDeal GetNegotiatingValue(MarketItem prop, string buyerId)
        {
            return Instance.NegotiatingDeals.FirstOrDefault(x => x.TargetProp == prop && x.BuyerId == buyerId);
        }

        public static NegotiatingDeal GetNegotiatingHighestValue(MarketItem prop)
        {
            return Instance.NegotiatingDeals.Where(x => x.TargetProp == prop).OrderByDescending(x => x.TotalValue).FirstOrDefault();
        }

        public static NegotiatingDeal GetNegotiatingLowestValue(MarketItem prop)
        {
            return Instance.NegotiatingDeals.Where(x => x.TargetProp == prop).OrderBy(x => x.TotalValue).FirstOrDefault();
        }

        public static int GetNegotiatingPrice(MarketItem prop, string buyerId)
        {
            var curDeal = GetBuyerDeal(prop, buyerId);
            var dealPrice = curDeal.Items.FirstOrDefault(x => x.NegotiatingPropSoleId == null);
            if (dealPrice == null || dealPrice.Count == 0)
            {
                return 0;
            }
            return dealPrice.Count;
        }

        public static void SetNegotiatingPrice(MarketItem prop, string buyerId, int value)
        {
            if (RemoveNegotiatingItem(prop, buyerId, null))
            {
                AddNegotiatingItem(prop, buyerId, 0, null, value);
            }
            else
            {
                AddNegotiatingItem(prop, buyerId, 0, null, prop.SellerPrice);
            }
        }

        public static void Cancel(MarketItem item)
        {
            Instance.MarketStack.Remove(item);
        }

        public static void Cancel(NegotiatingDeal deal)
        {
            Instance.NegotiatingDeals.Remove(deal);
        }

        public static void Deal(MarketItem item, NegotiatingDeal deal)
        {
            if (item == null || deal == null || !item.IsValid)
                return;

            var buyer = deal.Buyer;
            var seller = item.Seller;

            // check
            var price = GetNegotiatingPrice(item, deal.BuyerId);
            if (price > 0 && buyer.GetUnitMoney() < price ||
                deal.Items.Any(x => !CheckItemExists(x)))
            {
                NG();
            }

            // transfer money to seller
            if (price > 0 && buyer.GetUnitMoney() >= price)
            {
                buyer.AddUnitMoney(-price);
                seller.AddUnitMoney(price);
            }

            // transfer market item to buyer
            TransferItem(seller, buyer, item);

            // transfer negotiated items to seller
            foreach (var ni in deal.Items.Where(x => !string.IsNullOrEmpty(x.NegotiatingPropSoleId)))
            {
                TransferItem(seller, buyer, ni);
            }

            // cancel data
            Cancel(item);
            Cancel(deal);
        }

        public static void NG()
        {

        }

        public static void TransferItem(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            if (IsPartialItem(item.Prop))
            {
                buyer.RemoveUnitProp(item.PropId, item.Count);
                seller.AddUnitProp(item.PropId, item.Count);
            }
            else
            {
                buyer.RemoveUnitProp(item.SoleId);
                seller.AddUnitProp(item.Prop);
            }
        }

        public static void TransferItem(WorldUnitBase seller, WorldUnitBase buyer, NegotiatingItem item)
        {
            if (IsPartialItem(item.NegotiatingProp))
            {
                buyer.RemoveUnitProp(item.NegotiatingPropPropId, item.Count);
                seller.AddUnitProp(item.NegotiatingPropPropId, item.Count);
            }
            else
            {
                buyer.RemoveUnitProp(item.NegotiatingPropSoleId);
                seller.AddUnitProp(item.NegotiatingProp);
            }
        }

        public static bool IsPartialItem(DataProps.PropsData p)
        {
            return p != null && p.propsItem?.isOverlay == 1 && p.propsType != DataProps.PropsDataType.Martial;
        }

        public static bool IsHasItem(MarketItem item)
        {
            if (item.Prop == null)
                return false;
            return IsPartialItem(item.Prop) ? item.Seller.GetUnitProps(item.PropId).Count > 0 : true/*item.Prop != null*/;
        }

        public static bool CheckItemExists(NegotiatingItem item)
        {
            if (item.NegotiatingProp == null)
                return false;
            return IsPartialItem(item.NegotiatingProp) ? item.ForDeal.Buyer.GetUnitProps(item.NegotiatingPropPropId).Count > 0 : true/*item.NegotiatingProp != null*/;
        }

        public override void OnMonthly()
        {
            base.OnMonthly();

            //add item
            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                var market = town.GetBuildSub<MapBuildTownMarket>();
                if (market != null)
                {
                    var wunits = g.world.unit.GetUnitExact(town.GetOrigiPoint(), JOIN_RANGE).ToArray();
                    foreach (var seller in wunits)
                    {
                        if (!seller.IsPlayer() && CommonTool.Random(0f, 100f).IsBetween(0f, SELLER_JOIN_MARKET_RATE))
                        {
                            var sellerId = seller.GetUnitId();
                            var props = seller.GetUnequippedProps()
                                .Where(x => IsSellableItem(x) && !MarketStack.Any(z => z.SellerId == sellerId && z.SoleId == x.soleID) && CommonTool.Random(0f, 100f).IsBetween(0f, (7 - x.propsInfoBase.level) * SELL_RATE))
                                .Select(x => new MarketItem
                                {
                                    SellerId = sellerId,
                                    TownId = town.buildData.id,
                                    PropId = x.propsID,
                                    DataType = x.propsType,
                                    DataValues = x.values,
                                    Count = CommonTool.Random(1, x.propsCount),
                                    SellerPrice = (x.propsInfoBase.sale * CommonTool.Random(0.6f, 3.0f)).Parse<int>(),
                                    SoleId = x.soleID,
                                    CreateMonth = GameHelper.GetGameMonth()
                                })
                                .ToList();
                            if (props.Count > 0)
                            {
                                MarketStack.AddRange(props);
                            }
                        }
                    }
                }
            }

            //process
            foreach (var item in MarketStack.ToArray())
            {
                if (item.Seller.IsPlayer())
                {
                    continue;
                }

                if (!item.IsValid)
                {
                    Cancel(item);
                    continue;
                }

                var highestDeal = GetNegotiatingHighestValue(item);
                //get deal
                if (highestDeal != null && 
                    CommonTool.Random(0f, 100f).IsBetween(0f, GET_DEAL_RATE * (highestDeal.TotalValue / item.SellerPrice)))
                {
                    Deal(item, highestDeal);
                }
                //cancel item
                else if (CommonTool.Random(0f, 100f).IsBetween(0f, CANCEL_DEAL_RATE * (GameHelper.GetGameMonth() - item.CreateMonth)))
                {
                    Cancel(item);
                }
                //add deal
                else
                {
                    var wunits = g.world.unit.GetUnitExact(item.Town.GetOrigiPoint(), JOIN_RANGE).ToArray();
                    foreach (var buyer in wunits)
                    {
                        var dealProp = item.Prop;
                        var buyerId = buyer.GetUnitId();
                        if (item.SellerId != buyerId &&
                            CommonTool.Random(0f, 100f).IsBetween(0f, BUYER_JOIN_MARKET_RATE) &&
                            CommonTool.Random(0f, 100f).IsBetween(0f, dealProp.propsInfoBase.level * BUY_RATE))
                        {
                            var delta = 0.1f * dealProp.propsInfoBase.level;
                            SetNegotiatingPrice(item, buyerId,
                                (/*follow market price*/dealProp.propsInfoBase.sale *
                                    CommonTool.Random(0.5f + delta, 1.0f + delta)).Parse<int>().FixValue(0, buyer.GetUnitMoney() / 2).FixValue(MIN_PRICE, MAX_PRICE));
                            if (dealProp.propsInfoBase.level.IsBetween(5, 6) && dealProp.propsInfoBase.grade >= buyer.GetGradeLvl())
                            {
                                var curValue = GetNegotiatingValue(item, buyerId);
                                var highestValue = GetNegotiatingHighestValue(item);
                                var curDeal = GetBuyerDeal(item, buyerId);
                                foreach (var np in buyer.GetUnequippedProps().Where(x => IsSellableItem(x)))
                                {
                                    if (!curDeal.Items.Any(x => x.NegotiatingPropSoleId == np.soleID) &&
                                        np.propsInfoBase.level < dealProp.propsInfoBase.level && curValue.TotalValue + (np.propsInfoBase.sale * np.propsCount) < highestValue.TotalValue)
                                    {
                                        AddNegotiatingItem(item, buyerId, np.propsID, np.soleID, np.propsCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.TownMarket.uiName)
            {
                var ui = new UICover<UITownMarket>(e.ui);
                {
                    ui.AddToolTipButton(ui.MidCol - 3, ui.MidRow + 1, GameTool.LS("other500020041"));
                    ui.AddButton(ui.MidCol, ui.MidRow + 1, OpenMarket, GameTool.LS("other500020040")).Size(200, 40);
                    //ui.AddButton(ui.MidCol, ui.MidRow + 3, /*OpenSellingList*/ () => g.ui.MsgBox("Info", "Comming soon..."), GameTool.LS("other500020060")).Size(200, 40);
                    //ui.AddButton(ui.MidCol, ui.MidRow + 5, /*OpenRegister*/() => g.ui.MsgBox("Info", "Comming soon..."), GameTool.LS("other500020061")).Size(200, 40);
                }
                ui.UpdateUI();
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName && isNpcMarket)
            {
                var ui = new UICover<UINPCInfo>(e.ui);
                sellerId = ui.UI.unit.GetUnitId();
                ui.UI.tglTitle1.gameObject.SetActive(false);
                ui.UI.tglTitle2.gameObject.SetActive(false);
                ui.UI.tglTitle3.gameObject.SetActive(false);
                ui.UI.tglTitle4.gameObject.SetActive(false);
                ui.UI.tglTitle5.gameObject.SetActive(false);
                ui.UI.tglTitle6.gameObject.SetActive(false);
                ui.UI.tglTitle7.gameObject.SetActive(false);
                ui.UI.uiProperty.goGroupRoot.SetActive(false);
                OpenNPCSellList(ui.UI.unit);
            }
            else
            if (e.uiType.uiName == UIType.PropSelectCount.uiName && isNpcMarket)
            {
                var ui = g.ui.OpenUI<UIPropSelectCount>(UIType.PropSelectCount);
                //ui.InitData()
                //var ui = new UICover<UIPropSelectCount>(e.ui);
                //var marketItem = GetMarketItem(sellerId, ui.UI.ui)
                //ui.UI.minCount = 
            }
        }

        public override void OnCloseUIStart(CloseUIStart e)
        {
            base.OnCloseUIStart(e);
            if (e.uiType.uiName == UIType.NPCSearch.uiName)
            {
                isNpcMarket = false;
            }
        }

        private void OpenMarket()
        {
            var ui = g.ui.OpenUISafe<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            var town = g.world.playerUnit.GetMapBuild<MapBuildTown>();
            ui.units = g.world.unit.GetUnitExact(town.GetOrigiPoint(), JOIN_RANGE).ToArray()
                .Select(x => x.GetUnitId()).Distinct().Where(x => MarketStack.Any(z => z.IsValid && z.SellerId == x))
                .Select(x => g.world.unit.GetUnit(x)).ToIl2CppList();
            ui.UpdateUI();
            isNpcMarket = true;
        }

        private void OpenNPCSellList(WorldUnitBase wunit)
        {
            g.ui.CloseUI(UIType.PropSelect);

            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = GameTool.LS("other500020052");
            ui.textSearchTip.text = GameTool.LS("other500020053");
            ui.btnSearch.gameObject.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.selectOnePropID = true;
            ui.btnOK.gameObject.SetActive(false);
            ui.allItems = new DataProps
            {
                allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            };
            foreach (var item in MarketStack.Where(x => x.SellerId == wunit.GetUnitId()))
            {
                if (!item.IsValid)
                    continue;
                var org = item.Prop;
                if (IsPartialItem(org))
                {
                    ui.allItems.AddProps(ItemHelper.CopyProp(org, item.Count));
                }
                else
                {
                    ui.allItems.AddProps(org);
                }
            }
            var uiCover = new UICover<UIPropSelect>(ui);
            {
                uiCover.AddButton(0, 0, () => NegotiateDown(wunit, selectedProp, selectedMarketItem), GameTool.LS("other500020064")).Pos(ui.btnOK.transform, -150, 0);
                uiCover.AddButton(0, 0, () => Negotiate(wunit, selectedProp, selectedMarketItem), GameTool.LS("other500020067")).Pos(ui.btnOK.transform, -50, 0);
                uiCover.AddButton(0, 0, () => NegotiateUp(wunit, selectedProp, selectedMarketItem), GameTool.LS("other500020066")).Pos(ui.btnOK.transform, +50, 0);
                uiCover.AddButton(0, 0, () => GiveUp(wunit, selectedProp, selectedMarketItem), GameTool.LS("other500020077")).Pos(ui.btnOK.transform, +200, 0);
                uiCover.AddText(0, 0, GameTool.LS("other500020063")).SetWork(new UIItemWork
                {
                    Formatter = x =>
                    {
                        try
                        {
                            selectedProp = UIPropSelect.allSlectDataProps.allProps[0];
                            selectedMarketItem = GetMarketItem(wunit.GetUnitId(), selectedProp.soleID);
                            selectedProp.propsCount = selectedMarketItem.Count;
                            var playerId = g.world.playerUnit.GetUnitId();
                            var lowest = GetNegotiatingLowestValue(selectedMarketItem);
                            var market = selectedProp.propsInfoBase.sale;
                            var highest = GetNegotiatingHighestValue(selectedMarketItem);
                            var your = GetNegotiatingValue(selectedMarketItem, playerId);
                            return new object[] { (lowest?.TotalValue ?? 0).ToString(ModConst.FORMAT_NUMBER), market.ToString(ModConst.FORMAT_NUMBER), (highest?.TotalValue ?? 0).ToString(ModConst.FORMAT_NUMBER), (your?.TotalValue ?? 0).ToString(ModConst.FORMAT_NUMBER) };
                        }
                        catch
                        {
                            selectedProp = null;
                            selectedMarketItem = null;
                            return new object[] { 0, 0, 0, 0 };
                        }
                    }
                }).Pos(ui.btnOK.transform, 0, 40);
            }
            uiCover.IsAutoUpdate = true;
            ui.UpdateUI();
        }

        public void NegotiateDown(WorldUnitBase seller, DataProps.PropsData selectedProp, MarketItem selectedMarketItem)
        {
            if (seller == null || selectedProp == null || selectedMarketItem == null)
                return;
            var playerId = g.world.playerUnit.GetUnitId();
            var curPrice = GetNegotiatingPrice(selectedMarketItem, playerId);
            if (curPrice <= 0)
                return;
            SetNegotiatingPrice(selectedMarketItem, playerId, curPrice - (curPrice * CommonTool.Random(0.05f, 0.20f)).FixValue(0, g.world.playerUnit.GetUnitMoney()).FixValue(MIN_PRICE, MAX_PRICE).Parse<int>());
        }

        public void NegotiateUp(WorldUnitBase seller, DataProps.PropsData selectedProp, MarketItem selectedMarketItem)
        {
            if (seller == null || selectedProp == null || selectedMarketItem == null)
                return;
            var playerId = g.world.playerUnit.GetUnitId();
            var curPrice = GetNegotiatingPrice(selectedMarketItem, playerId);
            if (curPrice > MAX_PRICE)
                return;
            SetNegotiatingPrice(selectedMarketItem, playerId, curPrice + (curPrice * CommonTool.Random(0.05f, 0.20f)).FixValue(0, g.world.playerUnit.GetUnitMoney()).FixValue(MIN_PRICE, MAX_PRICE).Parse<int>());
        }

        public void Negotiate(WorldUnitBase seller, DataProps.PropsData selectedProp, MarketItem selectedMarketItem)
        {
            g.ui.CloseUI(UIType.PropSelect);

            var playerId = g.world.playerUnit.GetUnitId();
            var curDeal = GetBuyerDeal(selectedMarketItem, playerId);

            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = GameTool.LS("other500020052");
            ui.textSearchTip.text = GameTool.LS("other500020067");
            ui.btnSearch.gameObject.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.btnOK.gameObject.SetActive(false);
            ui.allItems = new DataProps
            {
                allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            };
            foreach (var prop in g.world.playerUnit.GetUnitProps())
            {
                if (IsSellableItem(prop)/* && !curDeal.Items.Any(y => y.NegotiatingPropSoleId == prop.soleID)*/)
                {
                    ui.allItems.AddProps(prop);
                }
            }
            var uiCover = new UICover<UIPropSelect>(ui);
            {
                uiCover.AddButton(0, 0, () =>
                {
                    foreach (var prop in UIPropSelect.allSlectDataProps.allProps.ToArray())
                    {
                        AddNegotiatingItem(selectedMarketItem, playerId, prop.propsID, prop.soleID, prop.propsCount);
                    }
                }, GameTool.LS("other500020074")).Pos(ui.btnOK.transform, -50, 0);
                uiCover.AddButton(0, 0, () =>
                {
                    OpenNPCSellList(seller);
                }, GameTool.LS("other500020076")).Pos(ui.btnOK.transform, +50, 0);
                uiCover.AddText(0, 0, GameTool.LS("other500020075")).SetWork(new UIItemWork
                {
                    Formatter = x =>
                    {
                        try
                        {
                            return new object[] { (GetNegotiatingValue(selectedMarketItem, playerId)?.TotalValue ?? 0).ToString(ModConst.FORMAT_NUMBER) };
                        }
                        catch
                        {
                            return new object[] { 0 };
                        }
                    }
                }).Pos(ui.btnOK.transform, 0, 40);
            }
            uiCover.IsAutoUpdate = true;
            ui.UpdateUI();
        }

        public void GiveUp(WorldUnitBase seller, DataProps.PropsData selectedProp, MarketItem selectedMarketItem)
        {
            if (seller == null || selectedProp == null || selectedMarketItem == null)
                return;
            Cancel(GetBuyerDeal(selectedMarketItem, g.world.playerUnit.GetUnitId()));
        }

        private void OpenRegister()
        {
            //var player = g.world.playerUnit;
            //var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            //ui.textTitle1.text = GameTool.LS("other500020058");
            //ui.textSearchTip.text = GameTool.LS("other500020060");
            //ui.btnSearch.gameObject.SetActive(false);
            //ui.goSubToggleRoot.SetActive(false);
            //ui.ClearSelectItem();
            //ui.selectOnePropID = true;
            //ui.allItems = new DataProps
            //{
            //    allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            //};
            //foreach (var item in g.world.playerUnit.GetUnequippedProps())
            //{
            //    if (IsSellableItem(item) && !PlayerStack.Any(y => y.Item6 == item.soleID))
            //    {
            //        ui.allItems.AddProps(item);
            //    }
            //}
            //ui.btnOK.onClick.RemoveAllListeners();
            //ui.btnOK.onClick.AddListener((UnityAction)(() =>
            //{
            //    PlayerStack.Clear();
            //    foreach (var prop in UIPropSelect.allSlectDataProps.allProps)
            //    {
            //        PlayerStack.Add(new Tuple<int, DataProps.PropsDataType, int[], int, int, string>
            //            (prop.propsID, prop.propsType, prop.values, prop.propsCount, 0, prop.soleID));
            //    }
            //    g.ui.CloseUI(ui);
            //}));
            //var uiCover = new UICover<UIPropSelect>(ui);
            //{
            //    uiCover.AddText(0, 0, GameTool.LS("other500020062")).Pos(ui.btnOK.transform, -1.0f, 1.0f);
            //    var inputComp = uiCover.AddInput(0, 0, string.Empty).Size(140, 30).Pos(ui.btnOK.transform, 0.5f, 1.0f);
            //    ui.allObjs.Add(inputComp.Component.name, inputComp.Component);
            //}
            //uiCover.IsAutoUpdate = true;
            //ui.UpdateUI();
        }

        private void OpenSellingList()
        {
            //g.ui.CloseUI(UIType.PropSelect);

            //var playerId = g.world.playerUnit.GetUnitId();
            //var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            //ui.textTitle1.text = GameTool.LS("other500020052");
            //ui.textSearchTip.text = GameTool.LS("other500020053");
            //ui.btnSearch.gameObject.SetActive(false);
            //ui.goSubToggleRoot.SetActive(false);
            //ui.ClearSelectItem();
            //ui.selectOnePropID = true;
            //ui.allItems = new DataProps
            //{
            //    allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            //};
            //propToDeal = new Dictionary<DataProps.PropsData, NegotiatingDeal>();
            //foreach (var d in NegotiatingDeals.Where(x => x.BuyerId == playerId))
            //{
            //    foreach (var p in ui.allItems.AddProps(d.TargetProp.Prop.CopyProp(d.TargetProp.Count)))
            //    {
            //        propToDeal.Add(p, d);
            //    }
            //}
            //ui.btnOK.gameObject.SetActive(false);
            //var uiCover = new UICover<UIPropSelect>(ui);
            //{
            //    uiCover.AddText(0, 0, GameTool.LS("other500020075")).SetWork(new UIItemWork
            //    {
            //        Formatter = x =>
            //        {
            //            try
            //            {
            //                selectedProp = UIPropSelect.allSlectDataProps.allProps[0];
            //                selectedMarketItem = propToDeal[selectedProp].TargetProp;
            //                selectedProp.propsCount = selectedMarketItem.Count;
            //                return new object[] { (GetNegotiatingValue(selectedMarketItem, playerId)?.TotalValue ?? 0).ToString(ModConst.FORMAT_NUMBER) };
            //            }
            //            catch
            //            {
            //                selectedProp = null;
            //                selectedMarketItem = null;
            //                return new object[] { 0 };
            //            }
            //        }
            //    }).Pos(ui.btnOK.transform, 0, 30);
            //    uiCover.AddButton(0, 0, () =>
            //    {
            //        GiveUp(selectedMarketItem.Seller, selectedProp, selectedMarketItem);
            //        OpenList();
            //    }, GameTool.LS("other500020059")).Pos(ui.btnOK.transform, 0, 0);
            //}
            //uiCover.IsAutoUpdate = true;
            //ui.UpdateUI();
        }
    }
}
