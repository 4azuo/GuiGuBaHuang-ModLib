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
        public static bool isShowNPCList = false;

        public Dictionary<string, List<Tuple<string, int>>> NPCinMarket = new Dictionary<string, List<Tuple<string, int>>>();

        public const float JOIN_MARKET_RATE = 30f;
        public const float SELL_RATE = 30f;

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
                        if (CommonTool.Random(0f, 100f).IsBetween(0f, JOIN_MARKET_RATE))
                        {
                            var props = wunit.GetUnequippedProps()
                                .Where(x => x.propsInfoBase.sale > 0 && CommonTool.Random(0f, 100f).IsBetween(0f, SELL_RATE))
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
            if (e.uiType.uiName == UIType.NPCSearch.uiName)
            {

            }
        }

        public override void OnCloseUIStart(CloseUIStart e)
        {
            base.OnCloseUIStart(e);
            if (e.uiType.uiName == UIType.NPCSearch.uiName)
            {
                isShowNPCList = false;
            }
        }

        private void OpenMarket()
        {
            //g.ui.MsgBox("Fouru", "Wait for next version...");
            var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
            ui.InitData(new Vector2Int(0, 0));
            ui.units = NPCinMarket.Select(x => g.world.unit.GetUnit(x)).ToIl2CppList();
            ui.UpdateUI();
            isShowNPCList = true;
        }
    }
}
