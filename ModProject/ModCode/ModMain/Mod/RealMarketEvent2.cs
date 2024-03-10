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
        private UITownMarketBuy uiTownMarketBuy;
        private UIPropSelectCount uiPropSelectCount;
        private MapBuildBase curMainTown;
        private Text txtMarketST;
        private Text txtInfo;
        private long marketST;

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
                MarketPriceRate[town.buildData.id] = CommonTool.Random(80.00f, 125.00f);
            }
        }

        public override void OnOpenUIStart(OpenUIStart e)
        {
            uiTownMarketBuy = MonoBehaviour.FindObjectOfType<UITownMarketBuy>();
            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
            if (uiTownMarketBuy != null && curMainTown != null)
            {
                //init
                var x = EventHelper.GetEvent<RealMarketEvent>(ModConst.REAL_MARKET_EVENT);
                marketST = x.MarketST[curMainTown.buildData.id];
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            uiTownMarketBuy = MonoBehaviour.FindObjectOfType<UITownMarketBuy>();
            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
            if (uiTownMarketBuy != null && curMainTown != null)
            {
                if (txtMarketST == null)
                {
                    //add component
                    txtMarketST = MonoBehaviour.Instantiate(uiTownMarketBuy.textMoney, uiTownMarketBuy.transform, false);
                    txtMarketST.text = $"Market: {marketST} Spirit Stones";
                    txtMarketST.transform.position = new Vector3(uiTownMarketBuy.textMoney.transform.position.x + 1.5f, uiTownMarketBuy.textMoney.transform.position.y);
                    txtMarketST.verticalOverflow = VerticalWrapMode.Overflow;
                    txtMarketST.horizontalOverflow = HorizontalWrapMode.Overflow;

                    txtInfo = MonoBehaviour.Instantiate(uiTownMarketBuy.textMoney, uiTownMarketBuy.transform, false);
                    txtInfo.text = $"Price rate: {MarketPriceRate[curMainTown.buildData.id]:0.00}%";
                    txtInfo.transform.position = new Vector3(uiTownMarketBuy.textMoney.transform.position.x + 5.0f, uiTownMarketBuy.textMoney.transform.position.y);
                    txtInfo.verticalOverflow = VerticalWrapMode.Overflow;
                    txtInfo.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txtInfo.color = Color.red;
                }

                uiPropSelectCount = MonoBehaviour.FindObjectOfType<UIPropSelectCount>();
                if (uiPropSelectCount != null)
                {
                    uiPropSelectCount.btnOK.onClick.m_Calls.m_RuntimeCalls.Insert(0, new InvokableCall((UnityAction)BuyEvent));
                }
            }
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            uiTownMarketBuy = MonoBehaviour.FindObjectOfType<UITownMarketBuy>();
            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
            if (uiTownMarketBuy == null || curMainTown == null)
            {
                txtMarketST = null;
                txtInfo = null;
                marketST = 0;
            }
        }

        public override void OnFrameUpdate()
        {
            if (uiTownMarketBuy != null && curMainTown != null && txtMarketST != null)
            {
                txtMarketST.text = $"Market: {marketST} Spirit Stones";
            }
        }

        private void BuyEvent()
        {
            var x = EventHelper.GetEvent<RealMarketEvent>(ModConst.REAL_MARKET_EVENT);
            x.MarketST[curMainTown.buildData.id] += uiPropSelectCount.iptNum.text.Parse<int>() * uiPropSelectCount.oneCost;
            marketST = x.MarketST[curMainTown.buildData.id];
        }
    }
}
