using System.Linq;
using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using ModLib.Object;
using Boo.Lang;

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

        public List<string> NPCinMarket = new List<string>();

        public override void OnMonthly()
        {
            base.OnMonthly();
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
            g.ui.MsgBox("Fouru", "Wait for next version...");
            //var ui = g.ui.OpenUI<UINPCSearch>(UIType.NPCSearch);
            //ui.InitData(new Vector2Int(0, 0));
            //ui.units = NPCinMarket.Select(x => g.world.unit.GetUnit(x)).ToIl2CppList();
            //ui.UpdateUI();
            //isShowNPCList = true;
        }
    }
}
