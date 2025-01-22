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
        public static RealMarketEvent Instance { get; set; }

        public static float MinRate => ModMain.ModObj.GameSettings.RealMarketConfigs.MinSellRate;

        public static float MaxRate => ModMain.ModObj.GameSettings.RealMarketConfigs.MaxSellRate;

        private Text txtMarketST;
        private Text txtPrice2;
        private Text txtWarningMsg;

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
            var eventSellRate = ModMain.ModObj.GameSettings.RealMarketConfigs.GetAddSellRate();
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                MarketPriceRate[town.buildData.id] = CommonTool.Random(MinRate + eventSellRate, MaxRate + eventSellRate);
            }
        }

        [EventCondition]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.PropSell.uiName)
            {
                var uiPropSell = g.ui.GetUI<UIPropSell>(UIType.PropSell);
                var curMainTown = g.world.playerUnit.GetMapBuild<MapBuildTown>();

                //add component
                var txtInfo = uiPropSell.textMoney.Copy().Pos(uiPropSell.textMoney.gameObject, 0f, -0.4f).Align().Format(Color.red);
                txtInfo.text = $"Price rate: {GetSellRate(curMainTown, g.world.playerUnit):0.00}%";

                txtMarketST = uiPropSell.textMoney.Copy().Pos(uiPropSell.textMoney.gameObject, 0f, -0.2f).Align();
                txtPrice2 = uiPropSell.textPrice.Copy().Pos(uiPropSell.textPrice.gameObject, 0f, -0.2f);
                txtWarningMsg = uiPropSell.textPrice.Copy().Pos(uiPropSell.btnOK.gameObject).Format(Color.red).Set("Over price");
                txtWarningMsg.gameObject.SetActive(false);

                uiPropSell.btnOK.onClick.RemoveAllListeners();
                uiPropSell.btnOK.onClick.AddListener((UnityAction)SellEvent);
            }
        }

        [ErrorIgnore]
        [EventCondition]
        public override void OnTimeUpdate()
        {
            base.OnTimeUpdate();
            if (g.ui.HasUI(UIType.PropSell))
            {
                var uiPropSell = g.ui.GetUI<UIPropSell>(UIType.PropSell);
                var curMainTown = g.world.playerUnit.GetMapBuild<MapBuildTown>();

                var budget = MapBuildPropertyEvent.GetBuildProperty(curMainTown);
                var totalPrice = GetTotalPrice(uiPropSell);
                var cashback = (totalPrice * ((GetSellRate(curMainTown, g.world.playerUnit) - 100.00f) / 100.00f)).Parse<int>();
                uiPropSell.textMoney.text = $"Owned: {g.world.playerUnit.GetUnitMoney()} Spirit Stones";
                uiPropSell.textPrice.text = $"Total: {totalPrice + cashback} ({cashback})";
                uiPropSell.btnOK.gameObject.SetActive(totalPrice <= budget);
                txtMarketST.text = $"Market: {budget} Spirit Stones";
                txtPrice2.text = $"/{budget} Spirit Stones";
                txtWarningMsg.gameObject.SetActive(totalPrice > budget);
            }
        }

        private void SellEvent()
        {
            if (g.ui.HasUI(UIType.PropSell))
            {
                var uiPropSell = g.ui.GetUI<UIPropSell>(UIType.PropSell);
                var curMainTown = g.world.playerUnit.GetMapBuild<MapBuildTown>();

                var totalPrice = GetTotalPrice(uiPropSell);
                var cashback = (totalPrice * ((GetSellRate(curMainTown, g.world.playerUnit) - 100.00f) / 100.00f)).Parse<int>();
                g.world.playerUnit.AddUnitMoney((totalPrice + cashback).Parse<int>());
                MapBuildPropertyEvent.AddBuildProperty(curMainTown, -totalPrice);

                foreach (var item in uiPropSell.selectProps.allProps.ToArray())
                {
                    g.world.playerUnit.data.unitData.propData.DelProps(item.soleID, item.propsCount);
                    uiPropSell.selectProps.allProps.Remove(item);
                }

                uiPropSell.UpdateHasList();
                uiPropSell.UpdateSellList();
                uiPropSell.UpdateTitle();
            }
        }

        private long GetTotalPrice(UIPropSell uiPropSell)
        {
            return uiPropSell.selectProps.allProps.ToArray().Sum(x => x.propsInfoBase.sale.Parse<long>() * x.propsCount);
        }

        public static float GetSellRate(MapBuildTown town, WorldUnitBase wunit)
        {
            var r = Instance.MarketPriceRate[town.buildData.id];
            if (UnitTypeEvent.GetUnitTypeEnum(wunit) == UnitTypeEnum.Merchant)
                r += UnitTypeEnum.Merchant.CustomLuck.CustomEffects[ModConst.UTYPE_LUCK_EFX_SELL_VALUE].Value0.Parse<float>();
            r += MerchantLuckEnum.Merchant.GetCurLevel(wunit) * MerchantLuckEnum.Merchant.IncSellValueEachLvl;
            return r;
        }
    }
}
