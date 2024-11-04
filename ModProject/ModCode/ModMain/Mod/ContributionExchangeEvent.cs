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
        //SpiritStones (Sell Price) / Contribution
        public static int GetExchangeRatio()
        {
            return InflationaryEvent.CalculateInflationary(ModMain.ModObj.InGameCustomSettings.ContributionExchangeConfigs.ExchangeRatio, GameHelper.GetGameYear());
        }

        public int CurMonthRatio { get; set; }

        private Text txtExchangeRatio;
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

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.School.uiName)
            {
                var uiSchool = g.ui.GetUI<UISchool>(UIType.School);
                if (g.world?.playerUnit?.data?.school?.schoolNameID == uiSchool.school.schoolNameID)
                {
                    var btnExchangeContribution = MonoBehaviour.Instantiate(uiSchool.btnGetMoney_En, uiSchool.transform, false);
                    btnExchangeContribution.onClick.AddListener((UnityAction)OpenSelector);
                    var btnText = btnExchangeContribution.GetComponentInChildren<Text>();
                    btnText.text = "Exchange Contribution";
                }
            }
        }

        [ErrorIgnore]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            var uiSelector = g.ui.GetUI<UIPropSelect>(UIType.PropSelect);
            if ((uiSelector?.gameObject?.active ?? false) && uiSelector.CompareTag(ModConst.CONTRIBUTION_EXCHANGE_EVENT))
            {
                var sp = CalExchangeSpiritStones();
                txtExchangeContribution.text = $"{sp} Spirit Stones → {CalExchangeContributions(sp)} Contribution";
            }
        }

        private void OpenSelector()
        {
            var uiSelector = g.ui.OpenUI<UIPropSelect>(UIType.PropSelect);
            uiSelector.tag = ModConst.CONTRIBUTION_EXCHANGE_EVENT;

            uiSelector.onOKCall = (Il2CppSystem.Action)Exchange;

            txtExchangeRatio = MonoBehaviour.Instantiate(uiSelector.textInfo, uiSelector.transform, false);
            txtExchangeRatio.text = $"Exchannge Ratio {CurMonthRatio} Spirit Stones → 1 Contribution";
            txtExchangeRatio.transform.position = new Vector3(uiSelector.textTitle1.transform.position.x, uiSelector.textTitle1.transform.position.y + 0.3f);
            txtExchangeRatio.verticalOverflow = VerticalWrapMode.Overflow;
            txtExchangeRatio.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtExchangeRatio.alignment = TextAnchor.MiddleLeft;
            txtExchangeRatio.fontSize = 15;
            txtExchangeRatio.color = Color.black;

            txtExchangeContribution = MonoBehaviour.Instantiate(uiSelector.textInfo, uiSelector.transform, false);
            txtExchangeContribution.transform.position = new Vector3(uiSelector.btnOK.transform.position.x, uiSelector.btnOK.transform.position.y - 0.5f);
            txtExchangeContribution.verticalOverflow = VerticalWrapMode.Overflow;
            txtExchangeContribution.horizontalOverflow = HorizontalWrapMode.Overflow;
            txtExchangeContribution.fontSize = 15;
            txtExchangeContribution.color = Color.black;
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
