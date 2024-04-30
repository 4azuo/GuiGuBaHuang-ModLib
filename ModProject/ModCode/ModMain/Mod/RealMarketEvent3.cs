//using EGameTypeData;
//using MOD_nE7UL2.Const;
//using ModLib.Mod;
//using UnityEngine.Events;
//using UnityEngine;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine.UI;

//namespace MOD_nE7UL2.Mod
//{
//    /// <summary>
//    /// Buy book
//    /// </summary>
//    [Cache(ModConst.REAL_MARKET_EVENT3)]
//    public class RealMarketEvent3 : ModEvent
//    {
//        private UITownMarketBook uiTownMarketBook;
//        private MapBuildBase curMainTown;
//        private Text txtMarketST;

//        public override void OnOpenUIEnd(OpenUIEnd e)
//        {
//            uiTownMarketBook = MonoBehaviour.FindObjectOfType<UITownMarketBook>();
//            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
//            if (uiTownMarketBook != null && curMainTown != null)
//            {
//                if (txtMarketST == null)
//                {
//                    txtMarketST = MonoBehaviour.Instantiate(uiTownMarketBook.textMoneyTitle_En, uiTownMarketBook.transform, false);
//                    txtMarketST.transform.position = new Vector3(uiTownMarketBook.textReputationTitle_En.transform.position.x - 2.5f, uiTownMarketBook.textReputationTitle_En.transform.position.y);
//                    txtMarketST.verticalOverflow = VerticalWrapMode.Overflow;
//                    txtMarketST.horizontalOverflow = HorizontalWrapMode.Overflow;

//                    var buttons = uiTownMarketBook.GetComponentsInChildren<Button>().ToList();
//                    var btnRedeems = buttons.Where(x => x.name == "BtnBuy");
//                    foreach (var button in btnRedeems)
//                    {
//                        var btnIcon = buttons[buttons.IndexOf(button) - 1];
//                        button.onClick.m_Calls.m_RuntimeCalls.Add(new InvokableCall((UnityAction)(() =>
//                        {
//                            BuyEvent(button, btnIcon);
//                        })));
//                    }
//                }
//            }
//        }

//        public override void OnCloseUIEnd(CloseUIEnd e)
//        {
//            uiTownMarketBook = MonoBehaviour.FindObjectOfType<UITownMarketBook>();
//            curMainTown = g.world.build.GetBuild(g.world.playerUnit.data.unitData.GetPoint());
//            if (uiTownMarketBook == null || curMainTown == null)
//            {
//                txtMarketST = null;
//            }
//        }

//        public override void OnFrameUpdate()
//        {
//            if (uiTownMarketBook != null && curMainTown != null && txtMarketST != null)
//            {
//                txtMarketST.text = $"Market: {MapBuildPropertyEvent.GetBuildProperty(curMainTown)} Spirit Stones";
//            }
//        }

//        private void BuyEvent(Button btnRedeem, Button btnIcon)
//        {
//            //DebugHelper.WriteLine($"Test: {btnIcon.tag}");
//            DebugHelper.Save();
//        }
//    }
//}
