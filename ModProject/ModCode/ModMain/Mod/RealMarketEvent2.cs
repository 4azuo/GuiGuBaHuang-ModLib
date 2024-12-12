using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using MOD_nE7UL2.Enum;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Buy item
    /// </summary>
    [Cache(ModConst.REAL_MARKET_EVENT2)]
    public class RealMarketEvent2 : ModEvent
    {
        public static float MIN_RATE
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.RealMarketConfigs.MinBuyRate;
            }
        }
        public static float MAX_RATE
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.RealMarketConfigs.MaxBuyRate;
            }
        }

        private Text txtMarketST;

        public IDictionary<string, float> MarketPriceRate { get; set; } = new Dictionary<string, float>();

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                if (!MarketPriceRate.ContainsKey(town.buildData.id))
                {
                    MarketPriceRate.Add(town.buildData.id, 100.00f);
                }
            }
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            var eventBuyRate = ModMain.ModObj.InGameCustomSettings.RealMarketConfigs.GetAddBuyRate();
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                MarketPriceRate[town.buildData.id] = CommonTool.Random(MIN_RATE + eventBuyRate, MAX_RATE + eventBuyRate);
            }
        }

        [EventCondition]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);

            if (e.uiType.uiName == UIType.TownMarketBuy.uiName)
            {
                var uiTownMarketBuy = g.ui.GetUI<UITownMarketBuy>(UIType.TownMarketBuy);

                //add component
                txtMarketST = uiTownMarketBuy.textMoney.Copy().Pos(uiTownMarketBuy.textMoney.gameObject, 1.2f, 0f).Align();

                var txtInfo = uiTownMarketBuy.textMoney.Copy().Pos(uiTownMarketBuy.textMoney.gameObject, 4.0f, 0f).Align().Format(Color.red);
                txtInfo.text = $"Price rate: {MarketPriceRate[uiTownMarketBuy.town.buildData.id]:0.00}%";
                if (uType == UnitTypeEnum.Merchant)
                    txtInfo.text += $" (Merchant {uType.CustomLuck.CustomEffects[ModConst.UTYPE_LUCK_EFX_BUY_COST].Value0.Parse<float>() * 100.0f:0.00}%)";
            }

            if (e.uiType.uiName == UIType.PropSelectCount.uiName && g.ui.HasUI(UIType.TownMarketBuy))
            {
                var uiTownMarketBuy = g.ui.GetUI<UITownMarketBuy>(UIType.TownMarketBuy);
                var uiPropSelectCount = g.ui.GetUI<UIPropSelectCount>(UIType.PropSelectCount);

                var basePrice = (uiPropSelectCount.oneCost.Parse<float>() * MarketPriceRate[uiTownMarketBuy.town.buildData.id] / 100f).Parse<int>();
                if (uType == UnitTypeEnum.Merchant)
                    basePrice += (basePrice * uType.CustomLuck.CustomEffects[ModConst.UTYPE_LUCK_EFX_BUY_COST].Value0.Parse<float>()).Parse<int>();
                uiPropSelectCount.oneCost = basePrice;
                uiPropSelectCount.UpdateCountUI();

                var txtInfo = uiPropSelectCount.textName.Copy().Pos(uiPropSelectCount.ptextInfo.gameObject, 0f, 0.2f).Align(TextAnchor.MiddleCenter).Format(Color.red);
                txtInfo.text = $"Price rate: {MarketPriceRate[uiTownMarketBuy.town.buildData.id]:0.00}%";
                if (uType == UnitTypeEnum.Merchant)
                    txtInfo.text += $" (Merchant {uType.CustomLuck.CustomEffects[ModConst.UTYPE_LUCK_EFX_BUY_COST].Value0.Parse<float>() * 100.0f:0.00}%)";

                uiPropSelectCount.btnOK.onClick.m_Calls.m_RuntimeCalls.Insert(0, new InvokableCall((UnityAction)(() =>
                {
                    var totalPrice = uiPropSelectCount.iptNum.text.Parse<int>() * uiPropSelectCount.oneCost;
                    MapBuildPropertyEvent.AddBuildProperty(uiTownMarketBuy.town, totalPrice);
                })));
            }
        }

        [ErrorIgnore]
        [EventCondition]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            if (g.ui.HasUI(UIType.TownMarketBuy))
            {
                var uiTownMarketBuy = g.ui.GetUI<UITownMarketBuy>(UIType.TownMarketBuy);
                txtMarketST.text = $"Market: {MapBuildPropertyEvent.GetBuildProperty(uiTownMarketBuy.town)} Spirit Stones";
            }
        }
    }
}
