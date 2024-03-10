using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.CONTRIBUTION_EXCHANGE_EVENT)]
    public class ContributionExchangeEvent : ModEvent
    {
        private const int EXCHANGE_RATIO = 200; //SpiritStone/Contribution
        private UISchool uiSchool;
        private UIPropSelect uiSelector;
        private Button btnExchangeContribution;
        private Text txtExchangeContribution;

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            uiSchool = MonoBehaviour.FindObjectOfType<UISchool>();
            if (uiSchool != null && g.world.playerUnit.data.school.schoolNameID == uiSchool.school.schoolNameID)
            {
                if (btnExchangeContribution == null)
                {
                    btnExchangeContribution = MonoBehaviour.Instantiate(uiSchool.btnGetMoney_En, uiSchool.transform, false);
                    btnExchangeContribution.onClick.AddListener((UnityAction)OpenSelector);
                    txtExchangeContribution = btnExchangeContribution.GetComponentInChildren<Text>();
                    txtExchangeContribution.text = "Exchange Contribution";
                }
            }
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            uiSchool = MonoBehaviour.FindObjectOfType<UISchool>();
            if (uiSchool == null)
            {
                btnExchangeContribution = null;
                txtExchangeContribution = null;
            }
        }

        private void OpenSelector()
        {
            uiSelector = g.ui.OpenUI<UIPropSelect>(UIType.PropSelect);
            uiSelector.onOKCall = (Il2CppSystem.Action)Exchange;
        }

        private void Exchange()
        {
            var spiritStone = 0;
            foreach (var item in UIPropSelect.allSlectItems)
            {
                spiritStone += item.propsInfoBase.sale * item.propsCount;
            }
            uiSchool.school.buildData.AddMoney(spiritStone);
            var contribution = Math.Max(1, spiritStone / EXCHANGE_RATIO);
            g.world.playerUnit.data.RewardPropItem(10011, contribution);
}
    }
}
