using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Const;
using ModLib.Mod;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
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
            return SMLocalConfigsEvent.Instance.Calculate(InflationaryEvent.CalculateInflationary(ModMain.ModObj.GameSettings.ContributionExchangeConfigs.ExchangeRatio), SMLocalConfigsEvent.Instance.Configs.AddSectExchangeRate).Parse<int>();
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

        [EventCondition]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (e.uiType.uiName == UIType.School.uiName && !SMLocalConfigsEvent.Instance.Configs.SectNoExchange)
            {
                var uiSchool = g.ui.GetUI<UISchool>(UIType.School);
                if (g.world?.playerUnit?.data?.school?.schoolNameID == uiSchool.school.schoolNameID)
                {
                    var btnExchangeContribution = MonoBehaviour.Instantiate(uiSchool.btnGetMoney_En, uiSchool.transform, false);
                    btnExchangeContribution.onClick.AddListener((UnityAction)OpenSelector);
                    var btnText = btnExchangeContribution.GetComponentInChildren<Text>();
                    btnText.text = GameTool.LS("other500020000");
                }
            }
        }

        [ErrorIgnore]
        [EventCondition]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            var sp = CalExchangeSpiritStones();
            txtExchangeContribution.text = $"{sp} Spirit Stones → {CalExchangeContributions(sp)} Contribution";
        }

        private void OpenSelector()
        {
            var uiSelector = g.ui.OpenUI<UIPropSelect>(UIType.PropSelect);
            uiSelector.tag = ModConst.CONTRIBUTION_EXCHANGE_EVENT;

            uiSelector.onOKCall = (Il2CppSystem.Action)Exchange;

            var txtExchangeRatio = uiSelector.textInfo.Copy().Format().Align(TextAnchor.MiddleCenter).Pos(uiSelector.textTitle1.gameObject, 0f, 0.3f);
            txtExchangeRatio.text = $"Exchannge Ratio {CurMonthRatio} Spirit Stones → 1 Contribution";

            txtExchangeContribution = uiSelector.textInfo.Copy().Format().Align(TextAnchor.MiddleCenter).Pos(uiSelector.btnOK.gameObject, 0f, -0.3f);
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
