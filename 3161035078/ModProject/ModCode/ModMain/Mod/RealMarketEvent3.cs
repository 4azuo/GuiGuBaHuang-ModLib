using System.Linq;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Object;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

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

        //temp
        private readonly Dictionary<string, object> customData = new Dictionary<string, object>();

        //propsID, propsType, values, count, price, soleID
        public Dictionary<string, List<Tuple<int, DataProps.PropsDataType, int[], int, int, string>>> NPCStack { get; set; } = 
            new Dictionary<string, List<Tuple<int, DataProps.PropsDataType, int[], int, int, string>>>();
        public List<Tuple<int, DataProps.PropsDataType, int[], int, int, string>> PlayerStack { get; set; } = 
            new List<Tuple<int, DataProps.PropsDataType, int[], int, int, string>>();

        public const int JOIN_RANGE = 4;
        public const float JOIN_MARKET_RATE = 30f;
        public const float SELL_RATE = 30f;

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
                                .Select(x => new Tuple<int, DataProps.PropsDataType, int[], int, int, string>
                                    (x.propsID, x.propsType, x.values, CommonTool.Random(1, x.propsCount), (x.propsInfoBase.sale * CommonTool.Random(0.5f, 2.0f)).Parse<int>(), x.soleID))
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
                    ui.AddToolTipButton(ui.MidCol - 3, ui.MidRow + 1, GameTool.LS("other500020041"));
                    ui.AddButton(ui.MidCol, ui.MidRow + 1, OpenMarket, GameTool.LS("other500020040")).Size(200, 40);
                    ui.AddButton(ui.MidCol, ui.MidRow + 3, OpenList, GameTool.LS("other500020060")).Size(200, 40);
                    ui.AddButton(ui.MidCol, ui.MidRow + 5, OpenRegister, GameTool.LS("other500020061")).Size(200, 40);
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
                OpenNPCSellList(ui.UI.unit);
            }
        }

        public override void OnCloseUIStart(CloseUIStart e)
        {
            base.OnCloseUIStart(e);
            if (e.uiType.uiName == UIType.NPCSearch.uiName)
            {
                isNpcMarket = false;
            }
            else
            if (e.uiType.uiName == UIType.TownMarket.uiName)
            {
                customData.Clear();
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
            ui.UpdateUI();
            isNpcMarket = true;
        }

        private void OpenNPCSellList(WorldUnitBase wunit)
        {
            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = GameTool.LS("other500020052");
            ui.textSearchTip.text = GameTool.LS("other500020053");
            ui.btnSearch.gameObject.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.selectOnePropID = true;
            ui.allItems = new DataProps
            {
                allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            };
            foreach (var item in NPCStack[wunit.GetUnitId()])
            {
                var org = wunit.GetUnitProp(item.Item6);
                if (org.propsItem?.isOverlay == 1 && org.propsType != DataProps.PropsDataType.Martial)
                {
                    var prop = ItemHelper.CopyProp(item.Item1, item.Item2, item.Item3, item.Item4);
                    customData[$"{prop.soleID}_price"] = item.Item5;
                    ui.allItems.AddProps(prop);
                }
                else
                {
                    var prop = org;
                    customData[$"{prop.soleID}_price"] = item.Item5;
                    ui.allItems.AddProps(prop);
                }
            }
            ui.btnOK.onClick.RemoveAllListeners();
            ui.btnOK.onClick.AddListener((UnityAction)(() =>
            {
                Buy();
            }));
            var uiCover = new UICover<UIPropSelect>(ui);
            {
                uiCover.AddButton(0, 0, Negotiate, GameTool.LS("other500020064")).Pos(ui.btnOK.transform, 0, 1.0f);
                uiCover.AddText(0, 0, GameTool.LS("other500020063")).SetWork(new UIItemWork
                {
                    Formatter = x =>
                    {
                        try
                        {
                            return new object[] { $"{customData[$"{UIPropSelect.allSlectItems[0].soleID}_price"]:#,##0}" };
                        }
                        catch
                        {
                            return new object[] { 0 };
                        }
                    }
                }).Pos(ui.btnOK.transform, 0, 0.5f);
            }
            uiCover.IsAutoUpdate = true;
            ui.UpdateUI();
        }

        public void Negotiate()
        {

        }

        public void Buy()
        {

        }

        private void OpenRegister()
        {
            var player = g.world.playerUnit;
            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = GameTool.LS("other500020058");
            ui.textSearchTip.text = GameTool.LS("other500020060");
            ui.btnSearch.gameObject.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.selectOnePropID = true;
            ui.allItems = new DataProps
            {
                allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            };
            foreach (var item in g.world.playerUnit.GetUnequippedProps())
            {
                if (IsSellableItem(item) && !PlayerStack.Any(y => y.Item6 == item.soleID))
                {
                    ui.allItems.AddProps(item);
                }
            }
            ui.btnOK.onClick.RemoveAllListeners();
            ui.btnOK.onClick.AddListener((UnityAction)(() =>
            {
                PlayerStack.Clear();
                foreach (var prop in UIPropSelect.allSlectDataProps.allProps)
                {
                    PlayerStack.Add(new Tuple<int, DataProps.PropsDataType, int[], int, int, string>
                        (prop.propsID, prop.propsType, prop.values, prop.propsCount, 0, prop.soleID));
                }
                g.ui.CloseUI(ui);
            }));
            var uiCover = new UICover<UIPropSelect>(ui);
            {
                uiCover.AddText(0, 0, GameTool.LS("other500020062")).Pos(ui.btnOK.transform, -1.0f, 1.0f);
                var inputComp = uiCover.AddInput(0, 0, string.Empty).Size(140, 30).Pos(ui.btnOK.transform, 0.5f, 1.0f);
                ui.allObjs.Add(inputComp.Component.name, inputComp.Component);
            }
            uiCover.IsAutoUpdate = true;
            ui.UpdateUI();
        }

        private void OpenList()
        {
            var player = g.world.playerUnit;
            var ui = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            ui.textTitle1.text = GameTool.LS("other500020052");
            ui.textSearchTip.text = GameTool.LS("other500020053");
            ui.btnSearch.gameObject.SetActive(false);
            ui.goSubToggleRoot.SetActive(false);
            ui.ClearSelectItem();
            ui.selectOnePropID = true;
            ui.allItems = new DataProps
            {
                allProps = new Il2CppSystem.Collections.Generic.List<DataProps.PropsData>()
            };
            foreach (var item in PlayerStack)
            {
                var org = player.GetUnitProp(item.Item6);
                if (org.propsItem?.isOverlay == 1 && org.propsType != DataProps.PropsDataType.Martial)
                {
                    var prop = ItemHelper.CopyProp(item.Item1, item.Item2, item.Item3, item.Item4);
                    customData[$"{prop.soleID}_price"] = item.Item5;
                    ui.allItems.AddProps(prop);
                }
                else
                {
                    var prop = org;
                    customData[$"{prop.soleID}_price"] = item.Item5;
                    ui.allItems.AddProps(prop);
                }
            }
            ui.btnOK.gameObject.SetActive(false);
            var uiCover = new UICover<UIPropSelect>(ui);
            {
                uiCover.AddText(0, 0, GameTool.LS("other500020063")).SetWork(new UIItemWork
                {
                    Formatter = x =>
                    {
                        try
                        {
                            return new object[] { $"{customData[$"{UIPropSelect.allSlectItems[0].soleID}_price"]:#,##0}" };
                        }
                        catch
                        {
                            return new object[] { 0 };
                        }
                    }
                }).Pos(ui.btnOK.transform, 0, 0.5f);
                uiCover.AddButton(0, 0, () =>
                {
                    PlayerStack.Clear();
                    g.ui.CloseUI(ui);
                }, GameTool.LS("other500020059")).Pos(ui.btnOK.transform, 0, 0);
            }
            uiCover.IsAutoUpdate = true;
            ui.UpdateUI();
        }
    }
}
