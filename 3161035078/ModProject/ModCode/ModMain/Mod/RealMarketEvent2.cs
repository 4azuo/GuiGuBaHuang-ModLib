using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using MOD_nE7UL2.Enum;
using ModLib.Helper;
using ModLib.Object;
using ModLib.Enum;
using ModLib.Attributes;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Buy item
    /// </summary>
    [Cache(ModConst.REAL_MARKET_EVENT2)]
    public class RealMarketEvent2 : ModEvent
    {
        public static RealMarketEvent2 Instance { get; set; }

        public static float MinRate => ModMain.ModObj.ModSettings.RealMarketConfigs.MinBuyRate;

        public static float MaxRate => ModMain.ModObj.ModSettings.RealMarketConfigs.MaxBuyRate;

        public static float TownGReduceBuyRate => ModMain.ModObj.ModSettings.RealMarketConfigs.TownGReduceBuyRate;

        public static float TownMReduceBuyRate => ModMain.ModObj.ModSettings.RealMarketConfigs.TownMReduceBuyRate;

        public IDictionary<string, float> MarketPriceRate { get; set; } = new Dictionary<string, float>();

        /// <summary>
        /// Initializes the rate of each market town.
        /// </summary>
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            foreach (var town in ModMain.Store.Buildings.Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                if (!MarketPriceRate.ContainsKey(town.buildData.id))
                {
                    MarketPriceRate.Add(town.buildData.id, 100.00f);
                }
            }
        }

        /// <summary>
        /// Randomizes the buy rate (plus market events).
        /// </summary>
        public override void OnMonthly()
        {
            base.OnMonthly();
            var eventBuyRate = ModMain.ModObj.ModSettings.RealMarketConfigs.GetAddBuyRate();
            foreach (var town in ModMain.Store.Buildings.Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                MarketPriceRate[town.buildData.id] = CommonTool.Random(MinRate + eventBuyRate, MaxRate + eventBuyRate);
            }
        }

        /// <summary>
        /// TownMarketBuy: shows budget and rate; PropSelectCount: applies the rate to the unit price and credits the town budget on OK.
        /// </summary>
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);

            if (e.uiType.uiName == UIType.TownMarketBuy.uiName)
            {
                var uiTownMarketBuy = new UICover<UITownMarketBuy>(e.ui);
                {
                    uiTownMarketBuy.AddText(-300f, 210f, GameTool.LS("ui_market_budget_fmt")).Align().SetWork(new UIItemWork
                    {
                        Formatter = ModLib.Helper.ActionHelper.WTracedFunc<UIItemBase, object[]>((ibase) => new object[] { MapBuildPropertyEvent.GetBuildProperty(uiTownMarketBuy.UI.town) })
                    });
                    uiTownMarketBuy.AddText(-300f, 190f, ModTextHelper.LSF("ui_price_rate", GetBuyRate(uiTownMarketBuy.UI.town, g.world.playerUnit))).Align().Format(Color.red);
                }
                uiTownMarketBuy.IsAutoUpdate = true;
            }

            if (e.uiType.uiName == UIType.PropSelectCount.uiName && g.ui.HasUI(UIType.TownMarketBuy))
            {
                var uiTownMarketBuy = g.ui.GetUI<UITownMarketBuy>(UIType.TownMarketBuy);
                var uiPropSelectCount = g.ui.GetUI<UIPropSelectCount>(UIType.PropSelectCount);

                uiPropSelectCount.oneCost = (uiPropSelectCount.oneCost.Parse<float>() * GetBuyRate(uiTownMarketBuy.town, g.world.playerUnit) / 100f).Parse<int>();
                uiPropSelectCount.UpdateCountUI();

                var uiCover = new UICover<UIPropSelectCount>(e.ui);
                {
                    uiCover.AddText(0, 0, ModTextHelper.LSF("ui_price_rate", GetBuyRate(uiTownMarketBuy.town, g.world.playerUnit))).Align(TextAnchor.MiddleCenter).Format(Color.red).Pos(uiPropSelectCount.ptextInfo.gameObject, 0f, 20f);
                }

                uiPropSelectCount.btnOK.onClick.m_Calls.m_RuntimeCalls.Insert(0, new InvokableCall(ModLib.Helper.ActionHelper.TracedUnityAction(() =>
                {
                    var totalPrice = uiPropSelectCount.iptNum.text.Parse<int>() * uiPropSelectCount.oneCost;
                    MapBuildPropertyEvent.AddBuildProperty(uiTownMarketBuy.town, totalPrice);
                })));
            }
        }

        /// <summary>
        /// Buy rate of a town for a unit (merchant, town master/guardian discounts).
        /// </summary>
        public static float GetBuyRate(MapBuildTown town, WorldUnitBase wunit)
        {
            var r = Instance.MarketPriceRate[town.buildData.id];
            if (UnitTypeEvent.GetUnitTypeEnum(wunit) == UnitTypeEnum.Merchant)
                r += UnitTypeEnum.Merchant.CustomLuck.CustomEffects[ModConst.UTYPE_LUCK_EFX_BUY_COST].Value0.Parse<float>();
            if (MapBuildPropertyEvent.IsTownGuardian(town, wunit))
            {
                if (MapBuildPropertyEvent.IsTownMaster(town, wunit))
                    r += TownMReduceBuyRate;
                else
                    r += TownGReduceBuyRate;
            }
            return r;
        }
    }
}
