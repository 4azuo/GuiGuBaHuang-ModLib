using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine.Events;
using UnityEngine;
using Il2CppSystem;
using System.Collections.Generic;
using System.Linq;
using ModLib.Enum;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Sell item
    /// </summary>
    [Cache(ModConst.REAL_MARKET_EVENT)]
    public class RealMarketEvent : ModEvent
    {
        private const float MIN_RATE = 85.00f;
        private const float MAX_RATE = 110.00f;

        private UIPropSell uiPropSell;
        private MapBuildBase curMainTown;
        private Text txtMarketST;
        private Text txtPrice2;
        private Text txtWarningMsg;
        private Text txtInfo;
        private long maxPrice;

        public IDictionary<string, float> MarketPriceRate { get; set; } = new Dictionary<string, float>();
        public IDictionary<string, long> MarketST { get; set; } = new Dictionary<string, long>();

        public override void OnLoadGame()
        {
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                if (!MarketPriceRate.ContainsKey(town.buildData.id))
                {
                    MarketPriceRate.Add(town.buildData.id, 100.00f);
                }
                if (!MarketST.ContainsKey(town.buildData.id))
                {
                    MarketST.Add(town.buildData.id, Math.Pow(5, town.gridData.areaBaseID).Parse<long>() * 100);
                }
            }
        }

        public override void OnMonthly()
        {
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                MarketPriceRate[town.buildData.id] = CommonTool.Random(MIN_RATE, MAX_RATE);
            }
            foreach (var wunit in g.world.unit.GetUnits())
            {
                var town = g.world.build.GetBuild(wunit.data.unitData.GetPoint());
                if (town != null && town.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill))
                {
                    var x = EventHelper.GetEvent<RealMarketEvent>(ModConst.REAL_MARKET_EVENT);
                    MarketST[town.buildData.id] += (Math.Pow(2, g.conf.roleGrade.GetItem(wunit.GetProperty<int>(UnitPropertyEnum.GradeID)).grade) * 10 * (x.MarketPriceRate[town.buildData.id] / 100.00f)).Parse<long>();
                }
            }
        }

        public override void OnOpenUIStart(OpenUIStart e)
        {
            uiPropSell = MonoBehaviour.FindObjectOfType<UIPropSell>();
            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
            if (uiPropSell != null && curMainTown != null)
            {
                //init
                maxPrice = MarketST[curMainTown.buildData.id];

                //fix price
                uiPropSell.propsPrice = new Il2CppSystem.Collections.Generic.Dictionary<int, int>();
                foreach (var p in g.world.playerUnit.data.unitData.propData.allProps)
                {
                    if (!uiPropSell.propsPrice.ContainsKey(p.propsID))
                    {
                        var basePrice = (p.propsInfoBase.sale * (MarketPriceRate[curMainTown.buildData.id] / 100.00f)).Parse<int>();
                        uiPropSell.propsPrice.Add(p.propsID, basePrice);
                    }
                }
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            uiPropSell = MonoBehaviour.FindObjectOfType<UIPropSell>();
            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
            if (uiPropSell != null && curMainTown != null)
            {
                if (txtMarketST == null)
                {
                    //add component
                    txtMarketST = MonoBehaviour.Instantiate(uiPropSell.textMoney, uiPropSell.transform, false);
                    txtMarketST.text = $"Market: {maxPrice} Spirit Stones";
                    txtMarketST.transform.position = new Vector3(uiPropSell.textMoney.transform.position.x, uiPropSell.textMoney.transform.position.y - 0.2f);
                    txtMarketST.verticalOverflow = VerticalWrapMode.Overflow;
                    txtMarketST.horizontalOverflow = HorizontalWrapMode.Overflow;

                    txtInfo = MonoBehaviour.Instantiate(uiPropSell.textMoney, uiPropSell.transform, false);
                    txtInfo.text = $"Price rate: {MarketPriceRate[curMainTown.buildData.id]:0.00}%";
                    txtInfo.transform.position = new Vector3(uiPropSell.textMoney.transform.position.x, uiPropSell.textMoney.transform.position.y - 0.4f);
                    txtInfo.verticalOverflow = VerticalWrapMode.Overflow;
                    txtInfo.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txtInfo.color = Color.red;

                    txtPrice2 = MonoBehaviour.Instantiate(uiPropSell.textPrice, uiPropSell.transform, false);
                    txtPrice2.text = $"/{maxPrice} Spirit Stones";
                    txtPrice2.transform.position = new Vector3(uiPropSell.textPrice.transform.position.x, uiPropSell.textPrice.transform.position.y - 0.2f);
                    txtPrice2.verticalOverflow = VerticalWrapMode.Overflow;
                    txtPrice2.horizontalOverflow = HorizontalWrapMode.Overflow;

                    txtWarningMsg = MonoBehaviour.Instantiate(uiPropSell.textPrice, uiPropSell.transform, false);
                    txtWarningMsg.text = $"Over price";
                    txtWarningMsg.transform.position = new Vector3(uiPropSell.btnOK.transform.position.x, uiPropSell.btnOK.transform.position.y);
                    txtWarningMsg.verticalOverflow = VerticalWrapMode.Overflow;
                    txtWarningMsg.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txtWarningMsg.color = Color.red;
                    txtWarningMsg.gameObject.SetActive(false);

                    uiPropSell.btnOK.onClick.m_Calls.m_RuntimeCalls.Insert(0, new InvokableCall((UnityAction)SellEvent));
                }
            }
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            uiPropSell = MonoBehaviour.FindObjectOfType<UIPropSell>();
            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
            if (uiPropSell == null || curMainTown == null)
            {
                txtMarketST = null;
                txtPrice2 = null;
                txtWarningMsg = null;
                txtInfo = null;
                maxPrice = 0;
            }
        }

        public override void OnFrameUpdate()
        {
            if (uiPropSell != null && curMainTown != null && txtMarketST != null)
            {
                var totalPrice = GetTotalPrice();
                txtMarketST.text = $"Market: {maxPrice} Spirit Stones";
                txtPrice2.text = $"/{maxPrice} Spirit Stones";
                uiPropSell.textPrice.text = $"Total Price: {totalPrice}";
                uiPropSell.btnOK.gameObject.SetActive(totalPrice <= maxPrice);
                txtWarningMsg.gameObject.SetActive(totalPrice > maxPrice);
            }
        }

        private void SellEvent()
        {
            var totalPrice = GetTotalPrice().Parse<int>();

            MarketST[curMainTown.buildData.id] -= totalPrice;
            maxPrice = MarketST[curMainTown.buildData.id];
        }

        private long GetTotalPrice()
        {
            return uiPropSell.selectProps.allProps.ToArray().Sum(x => (uiPropSell.propsPrice.ContainsKey(x.propsID) ? uiPropSell.propsPrice[x.propsID] : x.propsInfoBase.sale.Parse<long>()) * x.propsCount);
        }
    }
}
