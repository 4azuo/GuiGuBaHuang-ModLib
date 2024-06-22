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

        private UITownMarketBuy uiTownMarketBuy;
        private UIPropSelectCount uiPropSelectCount;
        private Text txtMarketST;
        private Text txtInfo;
        private Text txtInfo2;

        public IDictionary<string, float> MarketPriceRate { get; set; } = new Dictionary<string, float>();

        public override void OnLoadGame()
        {
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
            var eventBuyRate = ModMain.ModObj.InGameCustomSettings.RealMarketConfigs.GetAddBuyRate();
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                MarketPriceRate[town.buildData.id] = CommonTool.Random(MIN_RATE + eventBuyRate, MAX_RATE + eventBuyRate);
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);
            uiTownMarketBuy = MonoBehaviour.FindObjectOfType<UITownMarketBuy>();
            if (uiTownMarketBuy != null)
            {
                if (txtMarketST == null)
                {
                    //add component
                    txtMarketST = MonoBehaviour.Instantiate(uiTownMarketBuy.textMoney, uiTownMarketBuy.transform, false);
                    txtMarketST.transform.position = new Vector3(uiTownMarketBuy.textMoney.transform.position.x + 1.5f, uiTownMarketBuy.textMoney.transform.position.y);
                    txtMarketST.verticalOverflow = VerticalWrapMode.Overflow;
                    txtMarketST.horizontalOverflow = HorizontalWrapMode.Overflow;

                    txtInfo = MonoBehaviour.Instantiate(uiTownMarketBuy.textMoney, uiTownMarketBuy.transform, false);
                    txtInfo.text = $"Price rate: {MarketPriceRate[uiTownMarketBuy.town.buildData.id]:0.00}%";
                    if (uType == UnitTypeEnum.Merchant)
                        txtInfo.text += $" (Merchant {uType.CustomLuck.CustomEffects["BuyCost"].Value0.Parse<float>() * 100.0f:0.00}%)";
                    txtInfo.transform.position = new Vector3(uiTownMarketBuy.textMoney.transform.position.x + 4.5f, uiTownMarketBuy.textMoney.transform.position.y);
                    txtInfo.verticalOverflow = VerticalWrapMode.Overflow;
                    txtInfo.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txtInfo.color = Color.red;
                }
                else
                {
                    uiPropSelectCount = MonoBehaviour.FindObjectOfType<UIPropSelectCount>();
                    if (uiPropSelectCount != null && txtInfo2 == null)
                    {
                        var basePrice = (uiPropSelectCount.oneCost.Parse<float>() * MarketPriceRate[uiTownMarketBuy.town.buildData.id] / 100f).Parse<int>();
                        if (uType == UnitTypeEnum.Merchant)
                            basePrice += (basePrice * uType.CustomLuck.CustomEffects["BuyCost"].Value0.Parse<float>()).Parse<int>();
                        uiPropSelectCount.oneCost = basePrice;
                        uiPropSelectCount.UpdateCountUI();

                        txtInfo2 = MonoBehaviour.Instantiate(uiPropSelectCount.textName, uiPropSelectCount.transform, false);
                        txtInfo2.text = $"Price rate: {MarketPriceRate[uiTownMarketBuy.town.buildData.id]:0.00}%";
                        if (uType == UnitTypeEnum.Merchant)
                            txtInfo2.text += $" (Merchant {uType.CustomLuck.CustomEffects["BuyCost"].Value0.Parse<float>() * 100.0f:0.00}%)";
                        txtInfo2.transform.position = new Vector3(uiPropSelectCount.ptextInfo.transform.position.x, uiPropSelectCount.ptextInfo.transform.position.y + 0.2f);
                        txtInfo2.verticalOverflow = VerticalWrapMode.Overflow;
                        txtInfo2.horizontalOverflow = HorizontalWrapMode.Overflow;
                        txtInfo2.color = Color.red;

                        uiPropSelectCount.btnOK.onClick.m_Calls.m_RuntimeCalls.Insert(0, new InvokableCall((UnityAction)BuyEvent));
                    }
                }
            }
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            uiTownMarketBuy = MonoBehaviour.FindObjectOfType<UITownMarketBuy>();
            if (uiTownMarketBuy == null)
            {
                txtMarketST = null;
                txtInfo = null;
                txtInfo2 = null;
            }
        }

        public override void OnFrameUpdate()
        {
            if (uiTownMarketBuy != null && txtMarketST != null)
            {
                txtMarketST.text = $"Market: {MapBuildPropertyEvent.GetBuildProperty(uiTownMarketBuy.town)} Spirit Stones";
            }
        }

        private void BuyEvent()
        {
            var totalPrice = uiPropSelectCount.iptNum.text.Parse<int>() * uiPropSelectCount.oneCost;
            MapBuildPropertyEvent.AddBuildProperty(uiTownMarketBuy.town, totalPrice);
        }
    }
}
