using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.CONTRIBUTION_EXCHANGE_EVENT)]
    public class ContributionExchangeEvent : ModEvent
    {
        public static ContributionExchangeEvent Instance { get; set; }

        //SpiritStones (Sell Price) / Contribution
        public static int GetExchangeRatio()
        {
            return SMLocalConfigsEvent.Instance.Calculate(InflationaryEvent.CalculateInflationary(ModMain.ModObj.ModSettings.ContributionExchangeConfigs.ExchangeRatio), SMLocalConfigsEvent.Instance.Configs.AddSectExchangeRate).Parse<int>();
        }

        public int CurMonthRatio { get; set; }

        private Text txtExchangeContribution;

        public override void OnMonthly()
        {
            base.OnMonthly();
            RandomRatio();
        }

        private void RandomRatio()
        {
            CurMonthRatio = (GetExchangeRatio() * CommonTool.Random(0.80f, 1.30f)).Parse<int>();
        }

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

        private void OpenSelector()
        {
            var uiSelector = g.ui.OpenUISafe<UIPropSelect>(UIType.PropSelect);
            uiSelector.UpdateUI();
            uiSelector.tag = ModConst.CONTRIBUTION_EXCHANGE_EVENT;
            uiSelector.onOKCall = ModLib.Helper.ActionHelper.TracedIl2Action(Exchange);

            var txtExchangeRatio = uiSelector.textInfo.Copy().Format().Align(TextAnchor.MiddleCenter).Pos(uiSelector.textTitle1.gameObject, 0, 0);
            txtExchangeRatio.text = string.Format(GameTool.LS("other500020057"), CurMonthRatio);

            txtExchangeContribution = uiSelector.textInfo.Copy().Format().Align(TextAnchor.MiddleCenter).Pos(uiSelector.btnOK.gameObject, 0, 50);
        }

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

        private int CalExchangeSpiritStones()
        {
            return UIPropSelect.allSlectDataProps.allProps.ToArray().Sum(x => x.propsInfoBase.sale * x.propsCount);
        }

        private int CalExchangeContributions(int spiritStones)
        {
            if (spiritStones == 0)
                return 0;
            return Math.Max(1, spiritStones / CurMonthRatio);
        }
    }
}
