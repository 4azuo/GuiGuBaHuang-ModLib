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
    /// Sell item
    /// </summary>
    [Cache(ModConst.REAL_MARKET_EVENT)]
    public class RealMarketEvent : ModEvent
    {
        public static float MIN_RATE
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.RealMarketConfigs.MinSellRate;
            }
        }
        public static float MAX_RATE
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.RealMarketConfigs.MaxSellRate;
            }
        }

        private UIPropSell uiPropSell;
        private MapBuildBase curMainTown;
        private Text txtMarketST;
        private Text txtPrice2;
        private Text txtWarningMsg;
        private Text txtInfo;

        #region old
        public IDictionary<string, long> MarketST { get; set; } = new Dictionary<string, long>();
        #endregion
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
            var eventSellRate = ModMain.ModObj.InGameCustomSettings.RealMarketConfigs.GetAddSellRate();
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                MarketPriceRate[town.buildData.id] = CommonTool.Random(MIN_RATE + eventSellRate, MAX_RATE + eventSellRate);
            }
        }

        public override void OnOpenUIStart(OpenUIStart e)
        {
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);
            uiPropSell = MonoBehaviour.FindObjectOfType<UIPropSell>();
            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
            if (uiPropSell != null && curMainTown != null)
            {
                //fix price
                uiPropSell.propsPrice = new Il2CppSystem.Collections.Generic.Dictionary<int, int>();
                foreach (var p in g.world.playerUnit.data.unitData.propData.allProps)
                {
                    if (!uiPropSell.propsPrice.ContainsKey(p.propsID))
                    {
                        var basePrice = (p.propsInfoBase.sale * (MarketPriceRate[curMainTown.buildData.id] / 100.00f)).Parse<int>();
                        if (uType == UnitTypeEnum.Merchant)
                            basePrice += (basePrice * uType.CustomLuck.CustomEffects["SellValue"].Value0.Parse<float>()).Parse<int>();
                        uiPropSell.propsPrice.Add(p.propsID, basePrice);
                    }
                }
            }
        }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);
            uiPropSell = MonoBehaviour.FindObjectOfType<UIPropSell>();
            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
            if (uiPropSell != null && curMainTown != null)
            {
                if (txtMarketST == null)
                {
                    //add component
                    txtMarketST = MonoBehaviour.Instantiate(uiPropSell.textMoney, uiPropSell.transform, false);
                    txtMarketST.transform.position = new Vector3(uiPropSell.textMoney.transform.position.x, uiPropSell.textMoney.transform.position.y - 0.2f);
                    txtMarketST.verticalOverflow = VerticalWrapMode.Overflow;
                    txtMarketST.horizontalOverflow = HorizontalWrapMode.Overflow;

                    txtInfo = MonoBehaviour.Instantiate(uiPropSell.textMoney, uiPropSell.transform, false);
                    txtInfo.text = $"Price rate: {MarketPriceRate[curMainTown.buildData.id]:0.00}%";
                    if (uType == UnitTypeEnum.Merchant)
                        txtInfo.text += $" (Merchant +{uType.CustomLuck.CustomEffects["SellValue"].Value0.Parse<float>() * 100.0f:0.00}%)";
                    txtInfo.transform.position = new Vector3(uiPropSell.textMoney.transform.position.x, uiPropSell.textMoney.transform.position.y - 0.4f);
                    txtInfo.verticalOverflow = VerticalWrapMode.Overflow;
                    txtInfo.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txtInfo.color = Color.red;

                    txtPrice2 = MonoBehaviour.Instantiate(uiPropSell.textPrice, uiPropSell.transform, false);
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
            }
        }

        public override void OnFrameUpdate()
        {
            if (uiPropSell != null && curMainTown != null && txtMarketST != null)
            {
                var budget = MapBuildPropertyEvent.GetBuildProperty(curMainTown);
                var totalPrice = GetTotalPrice();
                txtMarketST.text = $"Market: {budget} Spirit Stones";
                txtPrice2.text = $"/{budget} Spirit Stones";
                uiPropSell.textPrice.text = $"Total Price: {totalPrice}";
                uiPropSell.btnOK.gameObject.SetActive(totalPrice <= budget);
                txtWarningMsg.gameObject.SetActive(totalPrice > budget);
            }
        }

        private void SellEvent()
        {
            var totalPrice = GetTotalPrice().Parse<int>();
            MapBuildPropertyEvent.AddBuildProperty(curMainTown, -totalPrice);
        }

        private long GetTotalPrice()
        {
            return uiPropSell.selectProps.allProps.ToArray().Sum(x => (uiPropSell.propsPrice.ContainsKey(x.propsID) ? uiPropSell.propsPrice[x.propsID] : x.propsInfoBase.sale.Parse<long>()) * x.propsCount);
        }
    }
}
