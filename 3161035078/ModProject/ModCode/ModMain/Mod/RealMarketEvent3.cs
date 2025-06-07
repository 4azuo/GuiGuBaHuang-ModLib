using System.Linq;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Object;
using System.Collections.Generic;
using System;
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

        //temp
        private DataProps.PropsData selectedProp;

        //negotiate
        //soleID, wunitID, soleID/propID, count
        public List<NegotiatingItem> NegotiatingItems { get; set; } = new List<NegotiatingItem>();

        //wunitID, List<propsID, propsType, values, count, price, soleID>
        public List<MarketItem> MarketStack { get; set; } = new List<MarketItem>();

        public const int JOIN_RANGE = 4;
        public const float SELLER_JOIN_MARKET_RATE = 30f;
        public const float SELL_RATE = 5f;
        public const float BUYER_JOIN_MARKET_RATE = 50f;
        public const float BUY_RATE = 3f;

        public static bool IsSellableItem(DataProps.PropsData x)
        {
            return x.propsInfoBase.sale > 0 && ModLibConst.MONEY_PROP_ID != x.propsID && ModLibConst.CONTRIBUTION_PROP_ID != x.propsID && ModLibConst.MAYOR_DEGREE_PROP_ID != x.propsID;
        }

        public static Dictionary<string, int> GetNegotiatingValues(string propSoleID, string buyerId = null)
        {
            var rs = new Dictionary<string, int>();
            var l = Instance.NegotiatingItems.Where(x => x.TargetPropSoleId == propSoleID);
            foreach (var i in l)
            {
                if (buyerId == null || buyerId == i.BuyerId)
                {
                    if (!rs.ContainsKey(i.BuyerId))
                        rs[i.BuyerId] = 0;
                    var wunit = g.world.unit.GetUnit(i.BuyerId);
                    if (i.NegotiatingPropSoleId == null)
                        rs[i.BuyerId] += i.Count;
                    else
                        rs[i.BuyerId] += wunit.GetUnitProp(i.NegotiatingPropSoleId).propsInfoBase.sale * i.Count;
                }
            }
            return rs;
        }

        public static int GetNegotiatingValue(string propSoleID, string buyerId)
        {
            var negotiatingValues = GetNegotiatingValues(propSoleID, buyerId);
            if (negotiatingValues == null || negotiatingValues.Count == 0)
                return 0;
            return negotiatingValues[buyerId];
        }

        public static int GetNegotiatingMaxValue(string propSoleID)
        {
            var negotiatingValues = GetNegotiatingValues(propSoleID);
            if (negotiatingValues == null || negotiatingValues.Count == 0)
                return 0;
            return negotiatingValues.Max(x => x.Value);
        }

        public static int GetNegotiatingMinValue(string propSoleID)
        {
            var negotiatingValues = GetNegotiatingValues(propSoleID);
            if (negotiatingValues == null || negotiatingValues.Count == 0)
                return 0;
            return negotiatingValues.Min(x => x.Value);
        }

        public static int GetNegotiatingPrice(string propSoleID, string buyerId)
        {
            var curObj = Instance.NegotiatingItems.FirstOrDefault(x => x.TargetPropSoleId == propSoleID && x.BuyerId == buyerId && x.NegotiatingPropSoleId == null);
            if (curObj == null)
            {
                Instance.NegotiatingItems.Add(new NegotiatingItem { TargetPropSoleId = propSoleID, BuyerId = buyerId, NegotiatingPropSoleId = null, Count = 0 });
                return 0;
            }
            return curObj.Count;
        }

        public static void SetNegotiatingPrice(string propSoleID, string buyerId, int value)
        {
            var curObj = Instance.NegotiatingItems.FirstOrDefault(x => x.TargetPropSoleId == propSoleID && x.BuyerId == buyerId && x.NegotiatingPropSoleId == null);
            if (curObj == null)
            {
                Instance.NegotiatingItems.Add(new NegotiatingItem { TargetPropSoleId = propSoleID, BuyerId = buyerId, NegotiatingPropSoleId = null, Count = value});
            }
            curObj.Count = value;
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
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
                                    PropId = x.propsID,
                                    DataType = x.propsType,
                                    DataValues = x.values,
                                    Count = CommonTool.Random(1, x.propsCount),
                                    Price = (x.propsInfoBase.sale * CommonTool.Random(0.6f, 3.0f)).Parse<int>(),
                                    SoleId = x.soleID
                                })
                                .ToList();
                            if (props.Count > 0)
                            {
                                MarketStack.AddRange(props);

                                //add deals
                                foreach (var buyer in wunits)
                                {
                                    if (seller != buyer && CommonTool.Random(0f, 100f).IsBetween(0f, BUYER_JOIN_MARKET_RATE))
                                    {
                                        var buyerId = buyer.GetUnitId();

                                        foreach (var prop in props)
                                        {
                                            var p = prop.Prop;
                                            if (CommonTool.Random(0f, 100f).IsBetween(0f, (7 - p.propsInfoBase.level) * BUY_RATE))
                                            {
                                                var delta = 0.1f * p.propsInfoBase.level;
                                                NegotiatingItems.Add(new NegotiatingItem
                                                {
                                                    TargetPropSoleId = prop.SoleId,
                                                    BuyerId = buyerId,
                                                    NegotiatingPropSoleId = null,
                                                    Count = (p.propsInfoBase.sale * CommonTool.Random(0.5f + delta, 1.0f + delta)).Parse<int>().FixValue(0, buyer.GetUnitMoney()).FixValue(0, MAX_PRICE)
                                                });
                                                if (p.propsInfoBase.level.IsBetween(5, 6) && p.propsInfoBase.grade >= buyer.GetGradeLvl())
                                                {
                                                    var curValue = GetNegotiatingValue(prop.SoleId, buyerId);
                                                    var max = GetNegotiatingMaxValue(prop.SoleId);
                                                    foreach (var np in buyer.GetUnequippedProps().Where(x => IsSellableItem(x)))
                                                    {
                                                        if (np.propsInfoBase.level < p.propsInfoBase.level && curValue + (np.propsInfoBase.sale * np.propsCount) < max)
                                                        {
                                                            NegotiatingItems.Add(new NegotiatingItem
                                                            {
                                                                TargetPropSoleId = prop.SoleId,
                                                                BuyerId = buyerId,
                                                                NegotiatingPropSoleId = np.soleID,
                                                                Count = np.propsCount
                                                            });
                                                        }
                                                    }
                                                }
                                            }
                                        }
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
                    ui.AddButton(ui.MidCol, ui.MidRow + 3, /*OpenList*/() => g.ui.MsgBox("Info", "Comming soon..."), GameTool.LS("other500020060")).Size(200, 40);
                    ui.AddButton(ui.MidCol, ui.MidRow + 5, /*OpenRegister*/() => g.ui.MsgBox("Info", "Comming soon..."), GameTool.LS("other500020061")).Size(200, 40);
                }
                ui.UpdateUI();
            }
            else
            if (e.uiType.uiName == UIType.NPCInfo.uiName && isNpcMarket)
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
                isNpcMarket = false;
            }
        }

        private void OpenMarket()
        {
            var ui = g.ui.OpenUISafe<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            var town = g.world.playerUnit.GetMapBuild<MapBuildTown>();
            ui.units = g.world.unit.GetUnitExact(town.GetOrigiPoint(), JOIN_RANGE).ToArray()
                .Select(x => x.GetUnitId()).Distinct().Where(x => MarketStack.Any(z => z.SellerId == x))
                .Select(x => g.world.unit.GetUnit(x)).ToIl2CppList();
            ui.UpdateUI();
            isNpcMarket = true;
        }

        private void OpenNPCSellList(WorldUnitBase wunit)
        {
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
                var org = item.Prop;
                if (org.propsItem?.isOverlay == 1 && org.propsType != DataProps.PropsDataType.Martial)
                {
                    var prop = ItemHelper.CopyProp(item.PropId, item.DataType, item.DataValues, item.Count);
                    ui.allItems.AddProps(prop);
                }
                else
                {
                    var prop = org;
                    ui.allItems.AddProps(prop);
                }
            }
            var uiCover = new UICover<UIPropSelect>(ui);
            {
                uiCover.AddButton(0, 0, () => NegotiateDown(wunit), GameTool.LS("other500020064")).Pos(ui.btnOK.transform, -100, 0);
                uiCover.AddButton(0, 0, () => Negotiate(wunit), GameTool.LS("other500020067")).Pos(ui.btnOK.transform, 0, 0);
                uiCover.AddButton(0, 0, () => NegotiateUp(wunit), GameTool.LS("other500020066")).Pos(ui.btnOK.transform, +100, 0);
                uiCover.AddText(0, 0, GameTool.LS("other500020063")).SetWork(new UIItemWork
                {
                    Formatter = x =>
                    {
                        try
                        {
                            var playerId = g.world.playerUnit.GetUnitId();
                            selectedProp = UIPropSelect.allSlectDataProps.allProps[0];
                            var soleID = selectedProp.soleID;
                            var min = GetNegotiatingMinValue(soleID);
                            var market = selectedProp.propsInfoBase.sale;
                            var max = GetNegotiatingMaxValue(soleID);
                            var your = GetNegotiatingValue(soleID, playerId);
                            return new object[] { min.ToString(ModConst.FORMAT_NUMBER), market.ToString(ModConst.FORMAT_NUMBER), max.ToString(ModConst.FORMAT_NUMBER), your.ToString(ModConst.FORMAT_NUMBER) };
                        }
                        catch
                        {
                            selectedProp = null;
                            return new object[] { 0, 0, 0, 0 };
                        }
                    }
                }).Pos(ui.btnOK.transform, 0, 40);
            }
            uiCover.IsAutoUpdate = true;
            ui.UpdateUI();
        }

        public void NegotiateDown(WorldUnitBase wunit)
        {
            if (selectedProp == null)
                return;
            var playerId = g.world.playerUnit.GetUnitId();
            var soleID = selectedProp.soleID;
            var curPrice = GetNegotiatingPrice(soleID, playerId);
            if (curPrice <= 0)
                return;
            SetNegotiatingPrice(soleID, playerId, curPrice - (curPrice * CommonTool.Random(0.05f, 0.20f)).FixValue(0, g.world.playerUnit.GetUnitMoney()).FixValue(0, MAX_PRICE).Parse<int>());
        }

        public void NegotiateUp(WorldUnitBase wunit)
        {
            if (selectedProp == null)
                return;
            var playerId = g.world.playerUnit.GetUnitId();
            var soleID = selectedProp.soleID;
            var curPrice = GetNegotiatingPrice(soleID, playerId);
            if (curPrice > MAX_PRICE)
                return;
            SetNegotiatingPrice(soleID, playerId, curPrice + (curPrice * CommonTool.Random(0.05f, 0.20f)).FixValue(0, g.world.playerUnit.GetUnitMoney()).FixValue(0, MAX_PRICE).Parse<int>());
        }

        public void Negotiate(WorldUnitBase wunit)
        {
            g.ui.CloseUI(UIType.PropSelect);

            var playerId = g.world.playerUnit.GetUnitId();
            var selectingItemSoleID = selectedProp.soleID;

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
                if (IsSellableItem(prop))
                {
                    ui.allItems.AddProps(prop);
                }
            }
            var uiCover = new UICover<UIPropSelect>(ui);
            {
                uiCover.AddButton(0, 0, () =>
                {
                    NegotiatingItems.AddRange(UIPropSelect.allSlectDataProps.allProps.ToArray().Select(x => new NegotiatingItem { TargetPropSoleId = selectingItemSoleID, BuyerId = playerId, NegotiatingPropSoleId = x.soleID, Count = x.propsCount }));
                }, GameTool.LS("other500020074")).Pos(ui.btnOK.transform, 0, 0);
                uiCover.AddText(0, 0, GameTool.LS("other500020075")).SetWork(new UIItemWork
                {
                    Formatter = x =>
                    {
                        try
                        {
                            var your = GetNegotiatingValue(selectingItemSoleID, playerId) + UIPropSelect.allSlectDataProps.allProps.ToArray().Sum(z => z.propsInfoBase.sale * z.propsCount);
                            return new object[] { your };
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

        private void OpenList()
        {
            //var player = g.world.playerUnit;
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
            //foreach (var item in PlayerStack)
            //{
            //    var org = player.GetUnitProp(item.Item6);
            //    if (org.propsItem?.isOverlay == 1 && org.propsType != DataProps.PropsDataType.Martial)
            //    {
            //        var prop = ItemHelper.CopyProp(item.Item1, item.Item2, item.Item3, item.Item4);
            //        NegotiatingValues[$"{prop.soleID}_price"] = item.Item5;
            //        ui.allItems.AddProps(prop);
            //    }
            //    else
            //    {
            //        var prop = org;
            //        NegotiatingValues[$"{prop.soleID}_price"] = item.Item5;
            //        ui.allItems.AddProps(prop);
            //    }
            //}
            //ui.btnOK.gameObject.SetActive(false);
            //var uiCover = new UICover<UIPropSelect>(ui);
            //{
            //    uiCover.AddText(0, 0, GameTool.LS("other500020063")).SetWork(new UIItemWork
            //    {
            //        Formatter = x =>
            //        {
            //            try
            //            {
            //                return new object[] { $"{NegotiatingValues[$"{UIPropSelect.allSlectItems[0].soleID}_price"].ToString(ModConst.FORMAT_NUMBER)}" };
            //            }
            //            catch
            //            {
            //                return new object[] { 0 };
            //            }
            //        }
            //    }).Pos(ui.btnOK.transform, 0, 30);
            //    uiCover.AddButton(0, 0, () =>
            //    {
            //        PlayerStack.Clear();
            //        g.ui.CloseUI(ui);
            //    }, GameTool.LS("other500020059")).Pos(ui.btnOK.transform, 0, 0);
            //}
            //uiCover.IsAutoUpdate = true;
            //ui.UpdateUI();
        }
    }
}
