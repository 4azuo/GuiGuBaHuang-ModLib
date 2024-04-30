using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Buy item
    /// </summary>
    [Cache(ModConst.REAL_MARKET_EVENT2)]
    public class RealMarketEvent2 : ModEvent
    {
        private const float MIN_RATE = 90.00f;
        private const float MAX_RATE = 115.00f;

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
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                MarketPriceRate[town.buildData.id] = CommonTool.Random(MIN_RATE, MAX_RATE);
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
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
                    txtInfo.transform.position = new Vector3(uiTownMarketBuy.textMoney.transform.position.x + 5.0f, uiTownMarketBuy.textMoney.transform.position.y);
                    txtInfo.verticalOverflow = VerticalWrapMode.Overflow;
                    txtInfo.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txtInfo.color = Color.red;
                }
                else
                {
                    uiPropSelectCount = MonoBehaviour.FindObjectOfType<UIPropSelectCount>();
                    if (uiPropSelectCount != null && txtInfo2 == null)
                    {
                        uiPropSelectCount.oneCost = (uiPropSelectCount.oneCost.Parse<float>() * MarketPriceRate[uiTownMarketBuy.town.buildData.id] / 100f).Parse<int>();
                        uiPropSelectCount.UpdateCountUI();

                        txtInfo2 = MonoBehaviour.Instantiate(uiPropSelectCount.textName, uiPropSelectCount.transform, false);
                        txtInfo2.text = $"Price rate: {MarketPriceRate[uiTownMarketBuy.town.buildData.id]:0.00}%";
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
