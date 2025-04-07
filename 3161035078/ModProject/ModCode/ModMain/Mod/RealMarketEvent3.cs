using System.Linq;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Object;
using System.Collections.Generic;
using System;

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

        public Dictionary<string, List<Tuple<string, int>>> NPCinMarket { get; set; } = new Dictionary<string, List<Tuple<string, int>>>();

        public const float JOIN_MARKET_RATE = 40f;
        public const float SELL_RATE = 40f;

        public static bool IsSellableItem(DataProps.PropsData x)
        {
            return x.propsInfoBase.sale > 0;
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            NPCinMarket.Clear();
            foreach (var town in g.world.build.GetBuilds<MapBuildTown>())
            {
                var market = town.GetBuildSub<MapBuildTownMarket>();
                if (market != null)
                {
                    var wunits = g.world.unit.GetUnitExact(town.GetOrigiPoint(), 4).ToArray();
                    foreach (var wunit in wunits)
                    {
                        if (!wunit.IsPlayer() && !NPCinMarket.ContainsKey(wunit.GetUnitId()) && CommonTool.Random(0f, 100f).IsBetween(0f, JOIN_MARKET_RATE))
                        {
                            var props = wunit.GetUnequippedProps()
                                .Where(x => IsSellableItem(x) && CommonTool.Random(0f, 100f).IsBetween(0f, SELL_RATE))
                                .Select(x => new Tuple<string, int>(x.soleID, CommonTool.Random(1, x.propsCount)))
                                .ToList();
                            if (props.Count > 0)
                                NPCinMarket.Add(wunit.GetUnitId(), props);
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
                ui.UI.uiProp.goItem.transform.parent = ui.UI.transform;
                ui.UI.uiProp.goItemRoot.transform.parent = ui.UI.transform;
                ui.UI.uiProp.goItemOption.transform.parent = ui.UI.transform;
                ui.UI.uiProp.goItemOptionGrid.transform.parent = ui.UI.transform;
                ui.UI.uiProp.goItemOptionRoot.transform.parent = ui.UI.transform;
                ui.UI.uiProp.goPropListLedItem.transform.parent = ui.UI.transform;
                ui.UI.uiProp.goPropListLedRoot.transform.parent = ui.UI.transform;
                ui.UI.uiProp.goPropListItem.transform.parent = ui.UI.transform;
                ui.UI.uiProp.goPropListRoot.transform.parent = ui.UI.transform;

                ui.UI.uiProp.ClearItem();

                //var count = 0;
                //foreach (var item in NPCinMarket[ui.UI.unit.GetUnitId()])
                //{
                //    //Il2CppSystem.Action onClick, onMouseEnter, onMouseExit;
                //    //UIIconTool.CreatePropsInfo(ui.UI.unit, ui.UI.unit.GetUnitPropN(item.Item1, item.Item2), out onClick, out onMouseEnter, out onMouseExit, (ReturnAction<Vector2>)(() => Vector2.zero));

                //    var unitProp = ui.UI.unit.GetUnitProp(item.Item1);
                //    //var propIcon = UIIconTool.CreatePropsIcon(ui.UI.unit, unitProp, ui.UI.transform);
                //    //propIcon.transform.position = new Vector3(0.5f * count++, 0);
                    
                //    ui.UI.uiProp.CreateProp(unitProp);
                //}
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
            var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = g.world.unit.GetUnitExact(g.world.playerUnit.GetUnitPos(), 4).ToArray().Where(x => NPCinMarket.ContainsKey(x.GetUnitId())).ToIl2CppList();
            ui.UpdateUI();
            isNpcMarket = true;
        }
    }
}
