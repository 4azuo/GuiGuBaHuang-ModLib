using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using MOD_nE7UL2.Enum;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Buy item
    /// </summary>
    [Cache(ModConst.REAL_MARKET_EVENT2)]
    public class RealMarketEvent2 : ModEvent
    {
        public static RealMarketEvent2 Instance { get; set; }

        public static float MinRate => ModMain.ModObj.GameSettings.RealMarketConfigs.MinBuyRate;

        public static float MaxRate => ModMain.ModObj.GameSettings.RealMarketConfigs.MaxBuyRate;

        public static float TownGReduceBuyRate => ModMain.ModObj.GameSettings.RealMarketConfigs.TownGReduceBuyRate;

        public static float TownMReduceBuyRate => ModMain.ModObj.GameSettings.RealMarketConfigs.TownMReduceBuyRate;

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
            var eventBuyRate = ModMain.ModObj.GameSettings.RealMarketConfigs.GetAddBuyRate();
            foreach (var town in g.world.build.GetBuilds().ToArray().Where(x => x.allBuildSub.ContainsKey(MapBuildSubType.TownMarketPill)))
            {
                MarketPriceRate[town.buildData.id] = CommonTool.Random(MinRate + eventBuyRate, MaxRate + eventBuyRate);
            }
        }

        [EventCondition]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);

            if (e.uiType.uiName == UIType.TownMarketBuy.uiName)
            {
                var uiTownMarketBuy = new UICover<UITownMarketBuy>(e.ui);
                {
                    uiTownMarketBuy.AddText(0, 0, "Market: {0} Spirit Stones").Align().Pos(uiTownMarketBuy.UI.textMoney.gameObject, 1.2f, 0f).SetWork(new UIItemWork
                    {
                        Formatter = (ibase) => new object[] { MapBuildPropertyEvent.GetBuildProperty(uiTownMarketBuy.UI.town) }
                    });
                    uiTownMarketBuy.AddText(0, 0, $"Price rate: {GetBuyRate(uiTownMarketBuy.UI.town, g.world.playerUnit):0.00}%").Align().Format(Color.red).Pos(uiTownMarketBuy.UI.textMoney.gameObject, 4.0f, 0f);
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
                    uiCover.AddText(0, 0, $"Price rate: {GetBuyRate(uiTownMarketBuy.town, g.world.playerUnit):0.00}%").Align(TextAnchor.MiddleCenter).Format(Color.red).Pos(uiPropSelectCount.ptextInfo.gameObject, 0f, 0.2f);
                }

                uiPropSelectCount.btnOK.onClick.m_Calls.m_RuntimeCalls.Insert(0, new InvokableCall((UnityAction)(() =>
                {
                    var totalPrice = uiPropSelectCount.iptNum.text.Parse<int>() * uiPropSelectCount.oneCost;
                    MapBuildPropertyEvent.AddBuildProperty(uiTownMarketBuy.town, totalPrice);
                })));
            }
        }

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
