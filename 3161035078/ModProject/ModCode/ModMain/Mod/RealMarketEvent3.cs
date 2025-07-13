using System.Linq;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Object;
using System.Collections.Generic;
using ModLib.Const;
using MOD_nE7UL2.Object;
using System;
using ModLib.Enum;
using static MOD_nE7UL2.Object.ModStts;
using MOD_nE7UL2.Enum;

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

        //configs
        public static _MarketItemConfigs Configs => ModMain.ModObj.ModSettings.MarketItemConfigs;

        public static bool IsSellableItem(DataProps.PropsData x)
        {
            return x.propsInfoBase.sale > 0 && ModLibConst.MONEY_PROP_ID != x.propsID && ModLibConst.CONTRIBUTION_PROP_ID != x.propsID && ModLibConst.MAYOR_DEGREE_PROP_ID != x.propsID;
        }

        public static MarketItem GetMarketItem(WorldUnitBase seller, DataProps.PropsData prop)
        {
            var sellerId = seller.GetUnitId();
            return Instance.MarketStack.FirstOrDefault(x =>
            {
                if (x.IsPartialItem)
                {
                    return x.SellerId == sellerId && x.PropId == prop.propsID;
                }
                else
                {
                    return x.SellerId == sellerId && x.SoleId == prop.soleID;
                }
            });
        }

        public static NegotiatingDeal GetBuyerDeal(MarketItem prop, WorldUnitBase buyer)
        {
            var buyerId = buyer.GetUnitId();
            var deal = prop.Deals.FirstOrDefault(x => x.BuyerId == buyerId);
            if (deal == null)
            {
                deal = new NegotiatingDeal(buyerId, prop);
                Instance.NegotiatingDeals.Add(deal);
            }
            return deal;
        }

        public static void AddNegotiatingItem(MarketItem prop, WorldUnitBase buyer, DataProps.PropsData negoProp, int count)
        {
            var curDeal = GetBuyerDeal(prop, buyer);
            curDeal.Items.Add(new NegotiatingItem(curDeal)
            {
                NegotiatingPropPropId = negoProp.propsID,
                NegotiatingPropSoleId = negoProp.soleID,
                Count = count,
                IsPartialItem = negoProp.IsPartialItem() == CheckEnum.True,
            });
        }

        public static NegotiatingDeal GetNegotiatingValue(MarketItem prop, WorldUnitBase buyer)
        {
            var buyerId = buyer.GetUnitId();
            return prop.Deals.FirstOrDefault(x => x.BuyerId == buyerId);
        }

        public static NegotiatingDeal GetNegotiatingHighestValue(MarketItem prop)
        {
            return prop.Deals.OrderByDescending(x => x.TotalValue).FirstOrDefault();
        }

        public static NegotiatingDeal GetNegotiatingLowestValue(MarketItem prop)
        {
            return prop.Deals.OrderBy(x => x.TotalValue).FirstOrDefault();
        }

        public static int GetNegotiatingPrice(MarketItem prop, WorldUnitBase buyer)
        {
            var curDeal = GetBuyerDeal(prop, buyer);
            var dealPrice = curDeal.Items.FirstOrDefault(x => x.IsSpiritStones);
            if (dealPrice == null || dealPrice.Count == 0)
            {
                return 0;
            }
            return dealPrice.Count;
        }

        public static void SetNegotiatingPrice(MarketItem prop, WorldUnitBase buyer, int value)
        {
            var curDeal = GetBuyerDeal(prop, buyer);
            var spItem = curDeal.Items.FirstOrDefault(x => x.IsSpiritStones);
            if (spItem != null)
            {
                curDeal.Items.Remove(spItem);
                curDeal.Items.Add(new NegotiatingItem(curDeal)
                {
                    Count = value,
                    IsSpiritStones = true,
                });
            }
            else
            {
                curDeal.Items.Add(new NegotiatingItem(curDeal)
                {
                    Count = prop.SellerPrice,
                    IsSpiritStones = true,
                });
            }
        }

        public static void CancelWithoutMessage(MarketItem item)
        {
            Instance.MarketStack.Remove(item);
        }

        public static void Cancel(MarketItem item)
        {
            CancelWithoutMessage(item);

            var seller = item.Seller;
            foreach (var deal in item.Deals)
            {
                CancelWithoutMessage(deal);
                NG_Seller1(seller, deal.Buyer, item);
            }
        }

        public static void CancelWithoutMessage(NegotiatingDeal deal)
        {
            Instance.NegotiatingDeals.Remove(deal);
        }

        public static void Cancel(NegotiatingDeal deal)
        {
            CancelWithoutMessage(deal);
        }

        public static void Deal(MarketItem item, NegotiatingDeal deal)
        {
            if (item == null || deal == null)
            {
                //stop
                return;
            }

            var seller = item.Seller;
            if (seller == null || !item.IsValid)
            {
                //seller cancel
                Cancel(item);
                return;
            }

            var buyer = deal.Buyer;
            if (buyer == null || !deal.IsValid)
            {
                //buyer safe cancel
                CancelWithoutMessage(deal);
                return;
            }

            //cancel data for processing
            CancelWithoutMessage(item);

            //seller cancel (%)
            if (CommonTool.Random(0f, 100f).IsBetween(0f, Configs.CancelDealLastMinuteRate))
            {
                DebugHelper.WriteLine($"【{seller.data.unitData.propertyData.GetName()}】→【{buyer.data.unitData.propertyData.GetName()}】：「{item.GetPropInfo().name}」：The seller removed the product from sale.");
                NG_Seller1(seller, buyer, item);
                return;
            }

            //check market item
            if (!CheckMarketItemExists(item))
            {
                DebugHelper.WriteLine($"【{seller.data.unitData.propertyData.GetName()}】→【{buyer.data.unitData.propertyData.GetName()}】：「{item.GetPropInfo().name}」：The item is no longer available.");
                NG_Seller2(seller, buyer, item);
                return;
            }

            //check negotiating items
            var price = GetNegotiatingPrice(item, buyer);
            if ((price > 0 && buyer.GetUnitMoney() < price) ||
                deal.Items.Any(x => !x.IsSpiritStones && !x.IsValid))
            {
                DebugHelper.WriteLine($"【{seller.data.unitData.propertyData.GetName()}】→【{buyer.data.unitData.propertyData.GetName()}】：「{item.GetPropInfo().name}」：The buyer is unable to pay.");
                NG_Buyer1(seller, buyer, item);
                return;
            }

            //are u try to dump me?
            if (deal.Items.Any(x =>
            {
                if (x.IsSpiritStones)
                    return false;
                if (x.IsPartialItem)
                {
                    return deal.Buyer.GetUnitPropCount(x.NegotiatingPropPropId) < deal.Items.Where(y => y.NegotiatingPropPropId == x.NegotiatingPropPropId).Sum(y => y.Count);
                }
                else
                {
                    return deal.Items.Any(y => x != y && y.NegotiatingPropSoleId == x.NegotiatingPropSoleId);
                }
            }))
            {
                DebugHelper.WriteLine($"【{seller.data.unitData.propertyData.GetName()}】→【{buyer.data.unitData.propertyData.GetName()}】：「{item.GetPropInfo().name}」：Are you try to dump me?");
                NG_Buyer2(seller, buyer, item);
                return;
            }

            //ok
            DebugHelper.WriteLine($"【{seller.data.unitData.propertyData.GetName()}】→【{buyer.data.unitData.propertyData.GetName()}】：「{item.GetPropInfo().name}」：OK：{price}／{string.Join(", ", deal.Items.Where(x => x.NegotiatingPropSoleId != null).Select(x => x.GetPropInfo().name))}");
            OK(seller, buyer, item);

            //transfer money to seller
            buyer.AddUnitMoney(-price);
            seller.AddUnitMoney(price);

            //transfer market item to buyer
            TransferItem(seller, buyer, item);

            //transfer negotiated items to seller
            foreach (var ni in deal.Items.Where(x => !x.IsSpiritStones))
            {
                TransferItem(seller, buyer, ni);
            }
        }

        private static void NG_Seller1(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            var dealPropInfo = item.GetPropInfo();
            if (seller == null || buyer == null || item == null || dealPropInfo == null)
                return;
            buyer.data.unitData.relationData.AddHate(seller.GetUnitId(), dealPropInfo.grade * 10);
            if (seller.IsPlayer() || buyer.IsPlayer())
                DramaHelper.OpenDrama1(GameTool.LS("other500020080"), new List<string> { GameTool.LS("other500020045") }, null, buyer, seller);
        }

        private static void NG_Seller2(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            var dealPropInfo = item.GetPropInfo();
            if (seller == null || buyer == null || item == null || dealPropInfo == null)
                return;
            buyer.data.unitData.relationData.AddHate(seller.GetUnitId(), dealPropInfo.grade * 10);
            if (seller.IsPlayer() || buyer.IsPlayer())
                DramaHelper.OpenDrama1(GameTool.LS("other500020081"), new List<string> { GameTool.LS("other500020045") }, null, buyer, seller);
        }

        private static void NG_Buyer1(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            var dealPropInfo = item.GetPropInfo();
            if (seller == null || buyer == null || item == null || dealPropInfo == null)
                return;
            seller.data.unitData.relationData.AddHate(buyer.GetUnitId(), dealPropInfo.grade * 10);
            if (seller.IsPlayer() || buyer.IsPlayer())
                DramaHelper.OpenDrama1(GameTool.LS("other500020078"), new List<string> { GameTool.LS("other500020045") }, null, seller, buyer);
        }

        private static void NG_Buyer2(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            var dealPropInfo = item.GetPropInfo();
            if (seller == null || buyer == null || item == null || dealPropInfo == null)
                return;
            seller.data.unitData.relationData.AddHate(buyer.GetUnitId(), dealPropInfo.grade * 15);
            if (seller.IsPlayer() || buyer.IsPlayer())
                DramaHelper.OpenDrama1(GameTool.LS("other500020088"), new List<string> { GameTool.LS("other500020045") }, null, seller, buyer);
        }

        private static void OK(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            var dealPropInfo = item.GetPropInfo();
            if (seller == null || buyer == null || item == null || dealPropInfo == null)
                return;
            seller.data.unitData.relationData.AddIntim(buyer.GetUnitId(), dealPropInfo.grade * 5);
            buyer.data.unitData.relationData.AddIntim(seller.GetUnitId(), dealPropInfo.grade * 5);
            if (seller.IsPlayer() || buyer.IsPlayer())
            {
                DramaHelper.OpenDrama1(GameTool.LS("other500020079"), new List<string> { GameTool.LS("other500020045") }, null, seller, buyer);
            }
            else if (item.Deals.Any(x => x.Buyer.IsPlayer()))
            {
                DramaHelper.OpenDrama2(string.Format(GameTool.LS("other500020082"), dealPropInfo.name), new List<string> { GameTool.LS("other500020045") }, null, "taohuayuanji3");
            }
        }

        public static void TransferItem(WorldUnitBase seller, WorldUnitBase buyer, MarketItem item)
        {
            if (item.IsPartialItem)
            {
                buyer.AddUnitProp(item.PropId, item.Count);
                seller.RemoveUnitProp(item.PropId, item.Count);
            }
            else
            {
                var fromProp = item.Prop;
                buyer.AddUnitProp(fromProp);
                foreach (var toProp in WUnitHelper.LastAddedItems)
                {
                    if (toProp != null && toProp.propsItem != null && (
                            toProp.propsItem.IsArtifact() != null ||
                            toProp.propsItem.IsRing() != null ||
                            toProp.propsItem.IsOutfit() != null ||
                            toProp.propsItem.IsMount() != null
                        ))
                    {
                        CustomRefineEvent.CopyAdj(fromProp, toProp);
                    }
                }
                seller.RemoveUnitProp(item.SoleId);
            }
        }

        public static void TransferItem(WorldUnitBase seller, WorldUnitBase buyer, NegotiatingItem item)
        {
            if (item.IsPartialItem)
            {
                buyer.AddUnitProp(item.NegotiatingPropPropId, item.Count);
                seller.RemoveUnitProp(item.NegotiatingPropPropId, item.Count);
            }
            else
            {
                var fromProp = item.NegotiatingProp;
                buyer.AddUnitProp(fromProp);
                foreach (var toProp in WUnitHelper.LastAddedItems)
                {
                    if (toProp != null && toProp.propsItem != null && (
                            toProp.propsItem.IsArtifact() != null ||
                            toProp.propsItem.IsRing() != null ||
                            toProp.propsItem.IsOutfit() != null ||
                            toProp.propsItem.IsMount() != null
                        ))
                    {
                        CustomRefineEvent.CopyAdj(fromProp, toProp);
                    }
                }
                seller.RemoveUnitProp(item.NegotiatingPropSoleId);
            }
        }

        public static bool CheckMarketItemExists(MarketItem item)
        {
            if (!item.IsValid)
                return false;
            return item.GetProps().Sum(x => x.propsCount) >= item.Count;
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            if (!SMLocalConfigsEvent.Instance.Configs.NoMarketItem)
            {
                var curMonth = GameHelper.GetGameTotalMonth();
                var sellerJoinRate = SMLocalConfigsEvent.Instance.Calculate(Configs.SellerJoinMarketRate, SMLocalConfigsEvent.Instance.Configs.MarketItemNpcJoinRate).Parse<float>();
                var buyerJoinRate = SMLocalConfigsEvent.Instance.Calculate(Configs.BuyerJoinMarketRate, SMLocalConfigsEvent.Instance.Configs.MarketItemNpcJoinRate).Parse<float>();

                //get wunits
                var towns = g.world.build.GetBuilds<MapBuildTown>().ToArray().Where(x => x.GetBuildSub<MapBuildTownMarket>() != null).ToArray();
                var wunitsInTowns = towns.ToDictionary(x => x.buildData.id, x => g.world.unit.GetUnitExact(x.GetOrigiPoint(), Configs.JoinRange).ToArray());

                //remove error
                //DebugHelper.WriteLine("1");
                foreach (var item in MarketStack.ToArray())
                {
                    //cancel
                    if (!item.IsValid ||
                        CommonTool.Random(0f, 100f).IsBetween(0f, Configs.CancelDealOnMonth * (curMonth - item.CreateMonth)))
                    {
                        //DebugHelper.WriteLine("1.2");
                        Cancel(item);
                        continue;
                    }

                    var highestDeal = GetNegotiatingHighestValue(item);
                    //get deal
                    if (highestDeal != null &&
                        CommonTool.Random(0f, 100f).IsBetween(0f, Configs.GetDealRate * (highestDeal.TotalValue / item.SellerPrice)))
                    {
                        //DebugHelper.WriteLine("1.1");
                        Deal(item, highestDeal);
                        continue;
                    }
                }

                //add item
                //DebugHelper.WriteLine("2");
                foreach (var wunitsInTown in wunitsInTowns)
                {
                    //DebugHelper.WriteLine("2.1");
                    foreach (var seller in wunitsInTown.Value)
                    {
                        if (!seller.IsPlayer() && CommonTool.Random(0f, 100f).IsBetween(0f, sellerJoinRate))
                        {
                            //DebugHelper.WriteLine("2.1.1");
                            var sellerId = seller.GetUnitId();
                            var props = seller.GetUnequippedProps()
                                .Where(x => IsSellableItem(x) && !MarketStack.Any(z => z.SellerId == sellerId && z.SoleId == x.soleID) && CommonTool.Random(0f, 100f).IsBetween(0f, (7 - x.propsInfoBase.level) * Configs.SellRateDependOnGrade))
                                .Select(x => new MarketItem
                                {
                                    SellerId = sellerId,
                                    TownId = wunitsInTown.Key,
                                    PropId = x.propsID,
                                    DataType = x.propsType,
                                    DataValues = x.values,
                                    Count = CommonTool.Random(1, x.propsCount),
                                    SellerPrice = (x.propsInfoBase.sale * CommonTool.Random(0.6f, 3.0f)).Parse<int>(),
                                    SoleId = x.soleID,
                                    CreateMonth = GameHelper.GetGameTotalMonth(),
                                    IsPartialItem = x.IsPartialItem() == CheckEnum.True,
                                    IsHidden = CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, Configs.HiddenRate),
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

                //process
                //DebugHelper.WriteLine("3");
                foreach (var item in MarketStack.ToArray())
                {
                    if (!item.IsValid)
                    {
                        Cancel(item);
                        continue;
                    }

                    var dealPropInfo = item.GetPropInfo();

                    //add deal
                    //DebugHelper.WriteLine("3.1");
                    foreach (var buyer in wunitsInTowns[item.TownId])
                    {
                        var buyerId = buyer.GetUnitId();
                        if (item.SellerId != buyerId && !buyer.IsPlayer() &&
                            CommonTool.Random(0f, 100f).IsBetween(0f, buyerJoinRate) &&
                            CommonTool.Random(0f, 100f).IsBetween(0f, dealPropInfo.level * Configs.BuyRateDependOnGrade))
                        {
                            //DebugHelper.WriteLine("3.1.1");
                            var delta = 0.1f * dealPropInfo.level;
                            SetNegotiatingPrice(item, buyer,
                                (/*follow market price*/dealPropInfo.sale *
                                    CommonTool.Random(0.5f + delta, 1.0f + delta)).Parse<int>().FixValue(0, buyer.GetUnitMoney() / 2).FixValue(MIN_PRICE, MAX_PRICE));
                            if (dealPropInfo.level.IsBetween(5, 6) && dealPropInfo.grade >= buyer.GetGradeLvl())
                            {
                                //DebugHelper.WriteLine("3.1.2");
                                var curValue = GetNegotiatingValue(item, buyer);
                                var highestValue = GetNegotiatingHighestValue(item);
                                var curDeal = GetBuyerDeal(item, buyer);
                                foreach (var np in buyer.GetUnequippedProps().Where(x => IsSellableItem(x)))
                                {
                                    //DebugHelper.WriteLine("3.1.2.1");
                                    if (!curDeal.Items.Any(x => x.NegotiatingPropSoleId == np.soleID) &&
                                        np.propsInfoBase.level < dealPropInfo.level && curValue.TotalValue + (np.propsInfoBase.sale * np.propsCount) < highestValue.TotalValue)
                                    {
                                        //DebugHelper.WriteLine("3.1.2.2");
                                        AddNegotiatingItem(item, buyer, np, np.propsCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static int VIPCost(int areaId)
        {
            var price = 100 * areaId;
            return InflationaryEvent.CalculateInflationary(price) / 10 * 10;
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (!SMLocalConfigsEvent.Instance.Configs.NoMarketItem)
            {
                if (e.uiType.uiName == UIType.TownMarket.uiName)
                {
                    var ui = new UICover<UITownMarket>(e.ui);
                    {
                        ui.AddToolTipButton(ui.MidCol - 11, ui.MidRow - 1, GameTool.LS("other500020041"));
                        ui.AddButton(ui.MidCol - 8, ui.MidRow - 1, () =>
                        {
                            var vipPrice = VIPCost(ui.UI.town.gridData.areaBaseID);
                            g.ui.MsgBox(GameTool.LS("other500020040"), string.Format(GameTool.LS("other500020086"), vipPrice), ModLib.Enum.MsgBoxButtonEnum.YesNo, () =>
                            {
                                if (g.world.playerUnit.GetUnitMoney() >= vipPrice)
                                {
                                    g.world.playerUnit.AddUnitMoney(-vipPrice);
                                    OpenMarket(x => x.IsHidden || (x.GetPropInfo()?.grade ?? 0) >= 4);
                                }
                                else
                                {
                                    g.ui.MsgBox(GameTool.LS("other500020040"), GameTool.LS("other500020087"));
                                }
                            });
                        }, GameTool.LS("other500020085")).Size(200, 40);
                        ui.AddButton(ui.MidCol - 8, ui.MidRow + 1, () => OpenMarket(x => !x.IsHidden), GameTool.LS("other500020040")).Size(200, 40);
                        ui.AddButton(ui.MidCol - 8, ui.MidRow + 3, /*OpenSellingList*/ () => g.ui.MsgBox("Info", "Comming soon..."), GameTool.LS("other500020060")).Size(200, 40);
                        ui.AddButton(ui.MidCol - 8, ui.MidRow + 5, /*OpenRegister*/() => g.ui.MsgBox("Info", "Comming soon..."), GameTool.LS("other500020061")).Size(200, 40);
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
        }

        public override void OnCloseUIStart(CloseUIStart e)
        {
            base.OnCloseUIStart(e);
            if (!SMLocalConfigsEvent.Instance.Configs.NoMarketItem)
            {
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
        }

        private void OpenMarket(Func<MarketItem, bool> predicate)
        {
            var ui = g.ui.OpenUISafe<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            var town = g.world.playerUnit.GetMapBuild<MapBuildTown>();
            ui.units = MarketStack.Where(x => x.TownId == town.buildData.id && predicate(x)).DistinctBy(x => x.SellerId).Select(x => x.Seller).ToIl2CppList();
            ui.UpdateUI();
            IsOpenMarket = true;
        }

        private void OpenNPCSellList(WorldUnitBase seller)
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
            var sellerId = seller.GetUnitId();
            var sellingItems = MarketStack.Where(x => x.SellerId == sellerId).ToList();
            foreach (var item in sellingItems)
            {
                if (!item.IsValid)
                    continue;
                if (item.IsPartialItem)
                {
                    ui.allItems.AddProps(item.Prop.CopyProp(item.Count));
                }
                else
                {
                    ui.allItems.AddProps(item.Prop);
                }
            }
            var uiCover = new UICover<UIPropSelect>(ui);
            {
                uiCover.AddButton(0, 0, () => NegotiateDown(ui, seller, selectedProp, selectedMarketItem), GameTool.LS("other500020064")).Pos(ui.btnOK.transform, -150, 0);
                uiCover.AddButton(0, 0, () => Negotiate(ui, seller, selectedProp, selectedMarketItem), GameTool.LS("other500020067")).Pos(ui.btnOK.transform, -50, 0);
                uiCover.AddButton(0, 0, () => NegotiateUp(ui, seller, selectedProp, selectedMarketItem), GameTool.LS("other500020066")).Pos(ui.btnOK.transform, +50, 0);
                uiCover.AddButton(0, 0, () => GiveUp(ui, seller, selectedProp, selectedMarketItem), GameTool.LS("other500020077")).Pos(ui.btnOK.transform, +200, 0);
                uiCover.AddText(0, 0, GameTool.LS("other500020063")).SetWork(new UIItemWork
                {
                    Formatter = x =>
                    {
                        try
                        {
                            var lowest = GetNegotiatingLowestValue(selectedMarketItem);
                            var market = selectedProp.propsInfoBase.sale;
                            var highest = GetNegotiatingHighestValue(selectedMarketItem);
                            var your = GetNegotiatingValue(selectedMarketItem, g.world.playerUnit);
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
                    selectedMarketItem = GetMarketItem(seller, selectedProp);
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
            var curPrice = GetNegotiatingPrice(selectedMarketItem, g.world.playerUnit);
            if (curPrice <= MIN_PRICE)
                return;
            SetNegotiatingPrice(selectedMarketItem, g.world.playerUnit, curPrice - (curPrice * CommonTool.Random(0.05f, 0.20f)).FixValue(0, g.world.playerUnit.GetUnitMoney()).FixValue(MIN_PRICE, MAX_PRICE).Parse<int>());
        }

        public void NegotiateUp(UIPropSelect uiSellingList, WorldUnitBase seller, DataProps.PropsData selectedProp, MarketItem selectedMarketItem)
        {
            if (seller == null || selectedProp == null || selectedMarketItem == null)
            {
                g.ui.MsgBox(GameTool.LS("other500020067"), GameTool.LS("other500020084"));
                return;
            }
            var curPrice = GetNegotiatingPrice(selectedMarketItem, g.world.playerUnit);
            if (curPrice > MAX_PRICE)
                return;
            var newPrice = curPrice + (curPrice * CommonTool.Random(0.05f, 0.20f)).FixValue(0, g.world.playerUnit.GetUnitMoney()).FixValue(MIN_PRICE, MAX_PRICE).Parse<int>();
            SetNegotiatingPrice(selectedMarketItem, g.world.playerUnit, newPrice);

            //stalker
            if (!SMLocalConfigsEvent.Instance.Configs.NoStalker)
            {
                var getStalkedRate = SMLocalConfigsEvent.Instance.Calculate(Configs.GetStalkedRate, SMLocalConfigsEvent.Instance.Configs.MarketItemGetAttackedRate).Parse<float>();
                BattleAfterEvent.AddStalker(StalkReasonEnum.MarketBuyerShowUp_SpiritStones,
                    selectedMarketItem.Deals
                    .Where(x => x.IsValid && BattleAfterEvent.IsStalker(x.Buyer) && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, getStalkedRate) && newPrice > 2 * x.Buyer.GetUnitMoney())
                    .Select(x => x.Buyer).ToArray());
            }
        }

        public void Negotiate(UIPropSelect uiSellingList, WorldUnitBase seller, DataProps.PropsData selectedProp, MarketItem selectedMarketItem)
        {
            if (seller == null || selectedProp == null || selectedMarketItem == null)
            {
                g.ui.MsgBox(GameTool.LS("other500020067"), GameTool.LS("other500020084"));
                return;
            }
            g.ui.CloseUI(UIType.PropSelect);

            var curDeal = GetBuyerDeal(selectedMarketItem, g.world.playerUnit);

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
                if (IsSellableItem(prop))
                {
                    ui.allItems.AddProps(prop);
                }
            }
            var uiCover = new UICover<UIPropSelect>(ui);
            {
                uiCover.AddButton(0, 0, () =>
                {
                    var getStalkedRate = SMLocalConfigsEvent.Instance.Calculate(Configs.GetStalkedRate, SMLocalConfigsEvent.Instance.Configs.MarketItemGetAttackedRate).Parse<float>();
                    foreach (var prop in UIPropSelect.allSlectDataProps.allProps.ToArray())
                    {
                        AddNegotiatingItem(selectedMarketItem, g.world.playerUnit, prop, prop.propsCount);

                        //stalker
                        if (!SMLocalConfigsEvent.Instance.Configs.NoStalker)
                        {
                            BattleAfterEvent.AddStalker(StalkReasonEnum.MarketBuyerShowUp_Items,
                                selectedMarketItem.Deals
                                .Where(x => x.IsValid && BattleAfterEvent.IsStalker(x.Buyer) && CommonTool.Random(0.00f, 100.00f).IsBetween(0.00f, getStalkedRate) && x.Buyer.CheckItemCouldBeRobbed(prop))
                                .Select(x => x.Buyer).ToArray());
                        }
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
                            return new object[] { (GetNegotiatingValue(selectedMarketItem, g.world.playerUnit)?.TotalValue ?? 0).ToString(ModConst.FORMAT_NUMBER) };
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

            Cancel(GetBuyerDeal(selectedMarketItem, g.world.playerUnit));

            uiSellingList.ClearSelectItem();
            uiSellingList.UpdateUI();

            this.selectedProp = null;
            this.selectedMarketItem = null;
        }

        private void OpenRegister(WorldUnitBase seller)
        {
            g.ui.CloseUI(UIType.PropSelect);

            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = "xxx";
            ui.textSearchTip.text = "xxx";
            ui.btnSearch.gameObject.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.selectOnePropID = true;
            ui.btnOK.gameObject.SetActive(false);
            ui.allItems.ClearAllProps();
            foreach (var prop in g.world.playerUnit.GetUnitProps())
            {
                if (IsSellableItem(prop))
                {
                    ui.allItems.AddProps(prop);
                }
            }
        }

        private void OpenSellingList(WorldUnitBase seller)
        {
            g.ui.CloseUI(UIType.PropSelect);

            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = "xxx";
            ui.textSearchTip.text = "xxx";
            ui.btnSearch.gameObject.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.selectOnePropID = true;
            ui.btnOK.gameObject.SetActive(false);
            ui.allItems.ClearAllProps();
            var sellerId = seller.GetUnitId();
            var sellingItems = MarketStack.Where(x => x.SellerId == sellerId).ToList();
            foreach (var item in sellingItems)
            {
                if (!item.IsValid)
                    continue;
                if (item.IsPartialItem)
                {
                    ui.allItems.AddProps(item.Prop.CopyProp(item.Count));
                }
                else
                {
                    ui.allItems.AddProps(item.Prop);
                }
            }
        }
    }
}
