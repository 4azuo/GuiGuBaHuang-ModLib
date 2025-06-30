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
        public static bool IsOpenMarket { get; private set; } = false;
        public static bool IsOpenSellingList { get; private set; } = false;
        public static bool IsOpenNegotiatingList { get; private set; } = false;

        public const int MAX_PRICE = 100000000;
        public const int MIN_PRICE = 100;

        //temp
        private DataProps.PropsData selectedProp;
        private MarketItem selectedMarketItem;

        //data
        public List<NegotiatingDeal> NegotiatingDeals { get; set; } = new List<NegotiatingDeal>();
        public List<MarketItem> MarketStack { get; set; } = new List<MarketItem>();

        public const int JOIN_RANGE = 4;
        public const float SELLER_JOIN_MARKET_RATE = 8f;
        public const float SELL_RATE = 4f; //depend on grade
        public const float BUYER_JOIN_MARKET_RATE = 16f;
        public const float BUY_RATE = 4f; //depend on grade
        public const float GET_DEAL_RATE = 32f; //depend on price
        public const float CANCEL_DEAL_RATE = 2f; //depend on month
        public const float CANCEL_DEAL_LAST_MINUTE_RATE = 8f;

        public static bool IsSellableItem(DataProps.PropsData x)
        {
            return x.propsInfoBase.sale > 0 && ModLibConst.MONEY_PROP_ID != x.propsID && ModLibConst.CONTRIBUTION_PROP_ID != x.propsID && ModLibConst.MAYOR_DEGREE_PROP_ID != x.propsID;
        }

        public static MarketItem GetMarketItem(string sellerId, string propSoleId, int propId)
        {
            return Instance.MarketStack.FirstOrDefault(x =>
            {
                if (IsPartialItem(x.Prop))
                {
                    return x.SellerId == sellerId && x.PropId == propId;
                }
                else
                {
                    return x.SellerId == sellerId && x.SoleId == propSoleId;
                }
            });
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
            var seller = item.Seller;
            Instance.MarketStack.Remove(item);
            foreach (var deal in Instance.NegotiatingDeals.ToArray())
            {
                var buyer = deal.Buyer;
                if (deal.TargetProp == item)
                {
                    Cancel(deal);
                    NG_Seller1(seller, buyer, item);
                }
            }
        }

        public static void Cancel(NegotiatingDeal deal)
        {
            Instance.NegotiatingDeals.Remove(deal);
        }

        public static void Deal(MarketItem item, NegotiatingDeal deal)
        {
            if (item == null || deal == null || !item.IsValid || !deal.IsValid)
                return;

            //cancel data
            Cancel(item);

            var buyer = deal.Buyer;
            var seller = item.Seller;

            //seller cancel?
            if (CommonTool.Random(0f, 100f).IsBetween(0f, CANCEL_DEAL_LAST_MINUTE_RATE))
            {
                NG_Seller1(buyer, seller, item);
                return;
            }

            //check market item
            if (!CheckMarketItemExists(item))
            {
                NG_Seller2(buyer, seller, item);
                return;
            }

            //check negotiating items
            var price = GetNegotiatingPrice(item, deal.BuyerId);
            if ((price > 0 && buyer.GetUnitMoney() < price) ||
                deal.Items.Any(x => x.NegotiatingPropSoleId != null && !x.IsValid))
            {
                NG_Buyer(seller, buyer, item);
                return;
            }

            //show drama
            OK(seller, buyer, item);

            //transfer money to seller
            if (price > 0 && buyer.GetUnitMoney() >= price)
            {
                buyer.AddUnitMoney(-price);
                seller.AddUnitMoney(price);
            }

            //transfer market item to buyer
            TransferItem(seller, buyer, item);

            //transfer negotiated items to seller
            foreach (var ni in deal.Items.Where(x => x.NegotiatingPropSoleId != null))
            {
                TransferItem(seller, buyer, ni);
            }
        }

        private static void NG_Seller1(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            if (seller == null || buyer == null || item == null)
                return;
            var dealPropInfo = item.GetPropInfo();
            buyer.data.unitData.relationData.AddHate(seller.GetUnitId(), dealPropInfo.grade * 20);
            if (seller.IsPlayer() || buyer.IsPlayer())
                DramaHelper.OpenDrama1(GameTool.LS("other500020080"), new List<string> { GameTool.LS("other500020045") }, null, buyer, seller);
        }

        private static void NG_Seller2(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            if (seller == null || buyer == null || item == null)
                return;
            var dealPropInfo = item.GetPropInfo();
            buyer.data.unitData.relationData.AddHate(seller.GetUnitId(), dealPropInfo.grade * 20);
            if (seller.IsPlayer() || buyer.IsPlayer())
                DramaHelper.OpenDrama1(GameTool.LS("other500020081"), new List<string> { GameTool.LS("other500020045") }, null, buyer, seller);
        }

        private static void NG_Buyer(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            if (seller == null || buyer == null || item == null)
                return;
            var dealPropInfo = item.GetPropInfo();
            seller.data.unitData.relationData.AddHate(buyer.GetUnitId(), dealPropInfo.grade * 20);
            if (seller.IsPlayer() || buyer.IsPlayer())
                DramaHelper.OpenDrama1(GameTool.LS("other500020078"), new List<string> { GameTool.LS("other500020045") }, null, seller, buyer);
        }

        private static void OK(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            if (seller == null || buyer == null || item == null)
                return;
            var dealPropInfo = item.GetPropInfo();
            seller.data.unitData.relationData.AddIntim(buyer.GetUnitId(), dealPropInfo.grade * 10);
            buyer.data.unitData.relationData.AddIntim(seller.GetUnitId(), dealPropInfo.grade * 10);
            if (seller.IsPlayer() || buyer.IsPlayer())
            {
                DramaHelper.OpenDrama1(GameTool.LS("other500020079"), new List<string> { GameTool.LS("other500020045") }, null, seller, buyer);
            }
            else if (Instance.NegotiatingDeals.Any(x => x.TargetProp == item && x.Buyer.IsPlayer()))
            {
                DramaHelper.OpenDrama2(string.Format(GameTool.LS("other500020082"), dealPropInfo.name), new List<string> { GameTool.LS("other500020045") }, null, "taohuayuanji3");
            }
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

        public static bool CheckMarketItemExists(MarketItem item)
        {
            if (!item.IsValid)
                return false;
            return item.Prop != null && (IsPartialItem(item.Prop) ? (item.SameProps.Count > 0 && item.SameProps.Sum(x => x.propsCount) > item.Count) : item.Prop != null);
        }

        public override void OnMonthly()
        {
            base.OnMonthly();

            //remove error
            //DebugHelper.WriteLine("1");
            foreach (var item in MarketStack.ToArray())
            {
                //cancel
                if (!item.IsValid ||
                    CommonTool.Random(0f, 100f).IsBetween(0f, CANCEL_DEAL_RATE * (GameHelper.GetGameTotalMonth() - item.CreateMonth)))
                {
                    //DebugHelper.WriteLine("1.2");
                    Cancel(item);
                    continue;
                }

                var highestDeal = GetNegotiatingHighestValue(item);
                //get deal
                if (highestDeal != null &&
                    CommonTool.Random(0f, 100f).IsBetween(0f, GET_DEAL_RATE * (highestDeal.TotalValue / item.SellerPrice)))
                {
                    //DebugHelper.WriteLine("1.1");
                    Deal(item, highestDeal);
                    continue;
                }
            }

            //add item
            //DebugHelper.WriteLine("2");
            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                var market = town.GetBuildSub<MapBuildTownMarket>();
                if (market != null)
                {
                    //DebugHelper.WriteLine("2.1");
                    var wunits = g.world.unit.GetUnitExact(town.GetOrigiPoint(), JOIN_RANGE).ToArray();
                    foreach (var seller in wunits)
                    {
                        if (!seller.IsPlayer() && CommonTool.Random(0f, 100f).IsBetween(0f, SELLER_JOIN_MARKET_RATE))
                        {
                            //DebugHelper.WriteLine("2.1.1");
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
                                    CreateMonth = GameHelper.GetGameTotalMonth()
                                })
                                .ToList();
                            if (props.Count > 0)
                            {
                                //DebugHelper.WriteLine("2.1.2");
                                MarketStack.AddRange(props);
                            }
                        }
                    }
                }
            }

            //process
            //DebugHelper.WriteLine("3");
            foreach (var item in MarketStack.ToArray())
            {
                if (item.Seller.IsPlayer())
                {
                    continue;
                }

                var dealPropInfo = item.GetPropInfo();
                if (dealPropInfo == null)
                {
                    Cancel(item);
                    continue;
                }

                //add deal
                DebugHelper.WriteLine("3.1");
                var wunits = g.world.unit.GetUnitExact(item.Town.GetOrigiPoint(), JOIN_RANGE).ToArray();
                foreach (var buyer in wunits)
                {
                    var buyerId = buyer.GetUnitId();
                    if (item.SellerId != buyerId &&
                        CommonTool.Random(0f, 100f).IsBetween(0f, BUYER_JOIN_MARKET_RATE) &&
                        CommonTool.Random(0f, 100f).IsBetween(0f, dealPropInfo.level * BUY_RATE))
                    {
                        DebugHelper.WriteLine("3.1.1");
                        var delta = 0.1f * dealPropInfo.level;
                        SetNegotiatingPrice(item, buyerId,
                            (/*follow market price*/dealPropInfo.sale *
                                CommonTool.Random(0.5f + delta, 1.0f + delta)).Parse<int>().FixValue(0, buyer.GetUnitMoney() / 2).FixValue(MIN_PRICE, MAX_PRICE));
                        if (dealPropInfo.level.IsBetween(5, 6) && dealPropInfo.grade >= buyer.GetGradeLvl())
                        {
                            DebugHelper.WriteLine("3.1.2");
                            var curValue = GetNegotiatingValue(item, buyerId);
                            var highestValue = GetNegotiatingHighestValue(item);
                            var curDeal = GetBuyerDeal(item, buyerId);
                            foreach (var np in buyer.GetUnequippedProps().Where(x => IsSellableItem(x)))
                            {
                                DebugHelper.WriteLine("3.1.2.1");
                                if (!curDeal.Items.Any(x => x.NegotiatingPropSoleId == np.soleID) &&
                                    np.propsInfoBase.level < dealPropInfo.level && curValue.TotalValue + (np.propsInfoBase.sale * np.propsCount) < highestValue.TotalValue)
                                {
                                    DebugHelper.WriteLine("3.1.2.2");
                                    AddNegotiatingItem(item, buyerId, np.propsID, np.soleID, np.propsCount);
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
                    ui.AddButton(ui.MidCol, ui.MidRow + 3, /*OpenSellingList*/ () => g.ui.MsgBox("Info", "Comming soon..."), GameTool.LS("other500020060")).Size(200, 40);
                    ui.AddButton(ui.MidCol, ui.MidRow + 5, /*OpenRegister*/() => g.ui.MsgBox("Info", "Comming soon..."), GameTool.LS("other500020061")).Size(200, 40);
                }
                ui.UpdateUI();
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName && IsOpenMarket)
            {
                var ui = new UICover<UINPCInfo>(e.ui);
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
        }

        public override void OnCloseUIStart(CloseUIStart e)
        {
            base.OnCloseUIStart(e);
            if (e.uiType.uiName == UIType.NPCSearch.uiName)
            {
                IsOpenMarket = false;
                IsOpenSellingList = false;
                IsOpenNegotiatingList = false;
                selectedProp = null;
                selectedMarketItem = null;
            }
            else
            if (e.uiType.uiName == UIType.PropSelect.uiName)
            {
                if (IsOpenSellingList)
                {
                    IsOpenSellingList = false;
                }
                else
                if (IsOpenNegotiatingList)
                {
                    IsOpenNegotiatingList = false;
                }

                if (!IsOpenSellingList && !IsOpenNegotiatingList)
                {
                    selectedProp = null;
                    selectedMarketItem = null;
                }
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
            IsOpenMarket = true;
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
            ui.allItems.ClearAllProps();
            var sellerId = wunit.GetUnitId();
            var sellingItems = MarketStack.Where(x => x.SellerId == sellerId).ToList();
            foreach (var item in sellingItems)
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
                uiCover.AddButton(0, 0, () => NegotiateDown(ui, wunit, selectedProp, selectedMarketItem), GameTool.LS("other500020064")).Pos(ui.btnOK.transform, -150, 0);
                uiCover.AddButton(0, 0, () => Negotiate(ui, wunit, selectedProp, selectedMarketItem), GameTool.LS("other500020067")).Pos(ui.btnOK.transform, -50, 0);
                uiCover.AddButton(0, 0, () => NegotiateUp(ui, wunit, selectedProp, selectedMarketItem), GameTool.LS("other500020066")).Pos(ui.btnOK.transform, +50, 0);
                uiCover.AddButton(0, 0, () => GiveUp(ui, wunit, selectedProp, selectedMarketItem), GameTool.LS("other500020077")).Pos(ui.btnOK.transform, +200, 0);
                uiCover.AddText(0, 0, GameTool.LS("other500020063")).SetWork(new UIItemWork
                {
                    Formatter = x =>
                    {
                        try
                        {
                            var playerId = g.world.playerUnit.GetUnitId();
                            var lowest = GetNegotiatingLowestValue(selectedMarketItem);
                            var market = selectedProp.propsInfoBase.sale;
                            var highest = GetNegotiatingHighestValue(selectedMarketItem);
                            var your = GetNegotiatingValue(selectedMarketItem, playerId);
                            return new object[]
                                {
                                    (lowest?.TotalValue ?? 0).ToString(ModConst.FORMAT_NUMBER),
                                    market.ToString(ModConst.FORMAT_NUMBER),
                                    (highest?.TotalValue ?? 0).ToString(ModConst.FORMAT_NUMBER),
                                    (your?.TotalValue ?? 0).ToString(ModConst.FORMAT_NUMBER)
                                };
                        }
                        catch
                        {
                            return new object[] { 0, 0, 0, 0 };
                        }
                    }
                }).Pos(ui.btnOK.transform, 0, 40);
                uiCover.UI.onCustomSelectCall = (ReturnAction<string, DataProps.PropsData>)((x) =>
                {
                    uiCover.UI.ClearSelectItem();
                    uiCover.UI.AddSelectProps(x);
                    uiCover.UI.UpdateUI();

                    selectedProp = x;
                    selectedMarketItem = GetMarketItem(sellerId, selectedProp.soleID, selectedProp.propsID);
                    return selectedProp.propsInfoBase.name;
                });
            }
            uiCover.IsAutoUpdate = true;
            ui.UpdateUI();
            IsOpenSellingList = true;
            IsOpenNegotiatingList = false;
        }

        public void NegotiateDown(UIPropSelect uiSellingList, WorldUnitBase seller, DataProps.PropsData selectedProp, MarketItem selectedMarketItem)
        {
            if (seller == null || selectedProp == null || selectedMarketItem == null)
            {
                g.ui.MsgBox(GameTool.LS("other500020067"), GameTool.LS("other500020084"));
                return;
            }
            var playerId = g.world.playerUnit.GetUnitId();
            var curPrice = GetNegotiatingPrice(selectedMarketItem, playerId);
            if (curPrice <= MIN_PRICE)
                return;
            SetNegotiatingPrice(selectedMarketItem, playerId, curPrice - (curPrice * CommonTool.Random(0.05f, 0.20f)).FixValue(0, g.world.playerUnit.GetUnitMoney()).FixValue(MIN_PRICE, MAX_PRICE).Parse<int>());
        }

        public void NegotiateUp(UIPropSelect uiSellingList, WorldUnitBase seller, DataProps.PropsData selectedProp, MarketItem selectedMarketItem)
        {
            if (seller == null || selectedProp == null || selectedMarketItem == null)
            {
                g.ui.MsgBox(GameTool.LS("other500020067"), GameTool.LS("other500020084"));
                return;
            }
            var playerId = g.world.playerUnit.GetUnitId();
            var curPrice = GetNegotiatingPrice(selectedMarketItem, playerId);
            if (curPrice > MAX_PRICE)
                return;
            SetNegotiatingPrice(selectedMarketItem, playerId, curPrice + (curPrice * CommonTool.Random(0.05f, 0.20f)).FixValue(0, g.world.playerUnit.GetUnitMoney()).FixValue(MIN_PRICE, MAX_PRICE).Parse<int>());
        }

        public void Negotiate(UIPropSelect uiSellingList, WorldUnitBase seller, DataProps.PropsData selectedProp, MarketItem selectedMarketItem)
        {
            if (seller == null || selectedProp == null || selectedMarketItem == null)
            {
                g.ui.MsgBox(GameTool.LS("other500020067"), GameTool.LS("other500020084"));
                return;
            }
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
            ui.allItems.ClearAllProps();
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
            IsOpenSellingList = false;
            IsOpenNegotiatingList = true;
        }

        public void GiveUp(UIPropSelect uiSellingList, WorldUnitBase seller, DataProps.PropsData selectedProp, MarketItem selectedMarketItem)
        {
            if (seller == null || selectedProp == null || selectedMarketItem == null)
            {
                g.ui.MsgBox(GameTool.LS("other500020067"), GameTool.LS("other500020084"));
                return;
            }

            Cancel(GetBuyerDeal(selectedMarketItem, g.world.playerUnit.GetUnitId()));

            uiSellingList.ClearSelectItem();
            uiSellingList.UpdateUI();

            selectedProp = null;
            selectedMarketItem = null;
        }

        private void OpenRegister()
        {
        }

        private void OpenSellingList()
        {
        }
    }
}
