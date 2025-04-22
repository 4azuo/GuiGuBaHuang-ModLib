using System.Linq;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Object;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using ModLib.Enum;

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

        public Dictionary<string, List<Tuple<string, int, int>>> NPCStack { get; set; } = new Dictionary<string, List<Tuple<string, int, int>>>();
        public List<Tuple<string, int, int>> PlayerStack { get; set; } = new List<Tuple<string, int, int>>();

        public const int JOIN_RANGE = 4;
        public const float JOIN_MARKET_RATE = 40f;
        public const float SELL_RATE = 40f;

        public static bool IsSellableItem(DataProps.PropsData x)
        {
            return x.propsInfoBase.sale > 0;
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            NPCStack.Clear();
            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                var market = town.GetBuildSub<MapBuildTownMarket>();
                if (market != null)
                {
                    var wunits = g.world.unit.GetUnitExact(town.GetOrigiPoint(), JOIN_RANGE).ToArray();
                    foreach (var wunit in wunits)
                    {
                        if (!wunit.IsPlayer() && !NPCStack.ContainsKey(wunit.GetUnitId()) && CommonTool.Random(0f, 100f).IsBetween(0f, JOIN_MARKET_RATE))
                        {
                            var props = wunit.GetUnequippedProps()
                                .Where(x => IsSellableItem(x) && CommonTool.Random(0f, 100f).IsBetween(0f, SELL_RATE))
                                .Select(x => new Tuple<string, int, int>(x.soleID, CommonTool.Random(1, x.propsCount), (x.propsInfoBase.sale * CommonTool.Random(0.5f, 2.0f)).Parse<int>()))
                                .ToList();
                            if (props.Count > 0)
                                NPCStack.Add(wunit.GetUnitId(), props);
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
                    ui.AddButton(0, 0, OpenMarket, GameTool.LS("other500020040")).Size(200, 40).Pos(ui.UI.btnBuy.transform, 0.9f, -1f);
                    ui.AddToolTipButton(GameTool.LS("other500020041")).Pos(ui.UI.btnBuy.transform, -0.6f, -1f);
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
                OpenSellList(ui.UI.unit);
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
                .Select(x => x.GetUnitId()).Distinct().Where(x => NPCStack.ContainsKey(x))
                .Select(x => g.world.unit.GetUnit(x)).ToIl2CppList();
            if (PlayerStack.Count > 0)
                ui.units.Add(g.world.playerUnit);
            ui.UpdateUI();
            isNpcMarket = true;

            var uiCover = new UICover<UINPCSearch>(ui);
            {
                uiCover.AddButton(uiCover.LastCol - 10, uiCover.LastRow - 11, Clear, GameTool.LS("other500020059")).Format(Color.black, 17).Align(TextAnchor.MiddleCenter).Size(300, 64);
                uiCover.AddButton(uiCover.LastCol - 10, uiCover.LastRow - 8, OpenRegister, GameTool.LS("other500020055")).Format(Color.black, 17).Align(TextAnchor.MiddleCenter).Size(300, 64);
            }
            uiCover.UpdateUI();
        }

        private void OpenSellList(WorldUnitBase wunit)
        {
            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = GameTool.LS("other500020052");
            ui.textSearchTip.text = GameTool.LS("other500020053");
            ui.btnSearch.gameObject.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.selectOnePropID = false;
            ui.allItems = new DataProps
            {
                allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            };
            if (wunit.IsPlayer())
            {
                foreach (var item in PlayerStack)
                {
                    ui.allItems.AddProps(wunit.GetUnitPropN(item.Item1, Math.Max(1, item.Item2)));
                }
                ui.btnOK.gameObject.SetActive(false);
            }
            else
            {
                foreach (var item in NPCStack[wunit.GetUnitId()])
                {
                    ui.allItems.AddProps(wunit.GetUnitPropN(item.Item1, Math.Max(1, item.Item2)));
                }
                ui.btnOK.onClick.RemoveAllListeners();
                ui.btnOK.onClick.AddListener((UnityAction)(() =>
                {
                    Buy();
                }));
            }
            ui.UpdateUI();
        }

        public void Buy()
        {

        }

        private void OpenRegister()
        {
            var ui = g.ui.OpenUISafe<UITownAuctionAddProp>(UIType.TownAuctionAddProp);
            ui.textInfo.text = GameTool.LS("other500020058");
            ui.allProps = g.world.playerUnit.GetUnequippedProps().Where(x => IsSellableItem(x) && !PlayerStack.Any(y => y.Item1 == x.soleID)).ToIl2CppList();
            ui.btnOK.onClick.AddListener((UnityAction)(() =>
            {
                Sell(UITownAuctionAddProp.selectProp, UITownAuctionAddProp.totalPrice);
            }));
            ui.UpdateUI();
        }

        public void Sell(DataProps.PropsData prop, int totalPrice)
        {
            PlayerStack.Add(new Tuple<string, int, int>(prop.soleID, Math.Max(1, prop.propsCount), totalPrice));
        }

        public void Clear()
        {
            PlayerStack.Clear();
        }
    }
}
