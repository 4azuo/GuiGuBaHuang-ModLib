using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using ModLib.Helper;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Sect members can exchange items for sect contribution at a monthly ratio.
    /// </summary>
    [Cache(ModConst.CONTRIBUTION_EXCHANGE_EVENT)]
    public class ContributionExchangeEvent : ModEvent
    {
        public static ContributionExchangeEvent Instance { get; set; }

        //SpiritStones (Sell Price) / Contribution
        /// <summary>
        /// Base ratio (inflation and S&amp;M rate applied).
        /// </summary>
        public static int GetExchangeRatio()
        {
            return SMLocalConfigsEvent.Instance.Calculate(InflationaryEvent.CalculateInflationary(ModMain.ModObj.ModSettings.ContributionExchangeConfigs.ExchangeRatio), SMLocalConfigsEvent.Instance.Configs.AddSectExchangeRate).Parse<int>();
        }

        public int CurMonthRatio { get; set; }

        private Text txtExchangeContribution;

        /// <summary>
        /// Picks a new ratio.
        /// </summary>
        public override void OnMonthly()
        {
            base.OnMonthly();
            RandomRatio();
        }

        /// <summary>
        /// Ratio x random 0.8-1.3.
        /// </summary>
        private void RandomRatio()
        {
            CurMonthRatio = (GetExchangeRatio() * CommonTool.Random(0.80f, 1.30f)).Parse<int>();
        }

        /// <summary>
        /// Own sect UI: adds the exchange button.
        /// </summary>
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (!SMLocalConfigsEvent.Instance.Configs.SectNoExchange)
            {
                if (e.uiType.uiName == UIType.School.uiName)
                {
                    var uiSchool = g.ui.GetUI<UISchool>(UIType.School);
                    if (g.world?.playerUnit?.data?.school?.schoolNameID == uiSchool.school.schoolNameID)
                    {
                        var ui = new UICover<UISchool>(uiSchool);
                        {
                            ui.AddButton(5, 5, OpenSelector, GameTool.LS("other500020000")).Size(240, 40);
                        }
                        ui.UpdateUI();
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the exchange preview.
        /// </summary>
        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            if (!SMLocalConfigsEvent.Instance.Configs.SectNoExchange)
            {
                if (txtExchangeContribution != null)
                {
                    var sp = CalExchangeSpiritStones();
                    txtExchangeContribution.text = string.Format(GameTool.LS("other500020056"), sp, CalExchangeContributions(sp));
                }
            }
        }

        /// <summary>
        /// Opens the item selector.
        /// </summary>
        private void OpenSelector()
        {
            // no month passed yet on a new save: the ratio was never drawn (0 -> division by zero on exchange)
            if (CurMonthRatio <= 0)
                RandomRatio();
            var uiSelector = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            uiSelector.UpdateUI();
            uiSelector.tag = ModConst.CONTRIBUTION_EXCHANGE_EVENT;
            uiSelector.onOKCall = ModLib.Helper.ActionHelper.TracedIl2Action(Exchange);

            var txtExchangeRatio = uiSelector.textInfo.Copy().Format().Align(TextAnchor.MiddleCenter).Pos(uiSelector.textTitle1.gameObject, 0, 0);
            txtExchangeRatio.text = string.Format(GameTool.LS("other500020057"), CurMonthRatio);

            txtExchangeContribution = uiSelector.textInfo.Copy().Format().Align(TextAnchor.MiddleCenter).Pos(uiSelector.btnOK.gameObject, 0, 50);
        }

        /// <summary>
        /// Gives the items value to the sect and contribution to the player.
        /// </summary>
        private void Exchange()
        {
            if (UIPropSelect.allSlectItems.Count > 0)
            {
                var uiSchool = g.ui.GetUI<UISchool>(UIType.School);
                var sp = CalExchangeSpiritStones();
                uiSchool.school.buildData.AddMoney(sp);
                g.world.playerUnit.data.RewardPropItem(ModLibConst.CONTRIBUTION_PROP_ID, CalExchangeContributions(sp));
            }
        }

        /// <summary>
        /// Value of selected items.
        /// </summary>
        private int CalExchangeSpiritStones()
        {
            return UIPropSelect.allSlectDataProps.allProps.ToArray().Sum(x => x.propsInfoBase.sale * x.propsCount);
        }

        /// <summary>
        /// Contribution for a value (at least 1).
        /// </summary>
        private int CalExchangeContributions(int spiritStones)
        {
            if (spiritStones == 0)
                return 0;
            return Math.Max(1, spiritStones / CurMonthRatio);
        }
    }
}
