using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using MOD_nE7UL2.Enum;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.REAL_STORAGE_EVENT)]
    public class RealStorageEvent : ModEvent
    {
        public static float FEE_RATE
        {
            get
            {
                return ModMain.ModObj.InGameCustomSettings.RealStorageConfigs.FeeRate;
            }
        }

        private UITownStorageProps uiTownStorageProps;
        private Text txtWarning1;
        private Text txtWarning2;
        private Text txtStorageMoney;
        private Text txtFee;

        public long StorageValue { get; set; } = 0L;
        public long Debt { get; set; } = 0L;

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            uiTownStorageProps = MonoBehaviour.FindObjectOfType<UITownStorageProps>();
            if (uiTownStorageProps != null)
            {
                if (txtFee == null)
                {
                    txtStorageMoney = MonoBehaviour.Instantiate(uiTownStorageProps.textTip, uiTownStorageProps.transform, false);
                    txtStorageMoney.transform.position = new Vector3(uiTownStorageProps.textTip.transform.position.x, uiTownStorageProps.textTitle1.transform.position.y + 0.4f);
                    txtStorageMoney.verticalOverflow = VerticalWrapMode.Overflow;
                    txtStorageMoney.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txtStorageMoney.alignment = TextAnchor.MiddleRight;
                    txtStorageMoney.fontSize = 15;
                    txtStorageMoney.color = Color.black;

                    txtFee = MonoBehaviour.Instantiate(uiTownStorageProps.textTip, uiTownStorageProps.transform, false);
                    txtFee.transform.position = new Vector3(uiTownStorageProps.textTip.transform.position.x, uiTownStorageProps.textTitle1.transform.position.y + 0.2f);
                    txtFee.verticalOverflow = VerticalWrapMode.Overflow;
                    txtFee.horizontalOverflow = HorizontalWrapMode.Overflow;
                    txtFee.alignment = TextAnchor.MiddleRight;
                    txtFee.fontSize = 15;
                    txtFee.color = Color.blue;

                    var money = g.world.playerUnit.GetUnitMoney();
                    if (money < Debt)
                    {
                        txtWarning1 = MonoBehaviour.Instantiate(uiTownStorageProps.textTip, uiTownStorageProps.transform, false);
                        txtWarning1.transform.position = new Vector3(uiTownStorageProps.textTitle1.transform.position.x, uiTownStorageProps.textTitle1.transform.position.y - 1.2f);
                        txtWarning1.verticalOverflow = VerticalWrapMode.Overflow;
                        txtWarning1.horizontalOverflow = HorizontalWrapMode.Overflow;
                        txtWarning1.alignment = TextAnchor.MiddleCenter;
                        txtWarning1.fontSize = 17;
                        txtWarning1.color = Color.red;
                        txtWarning1.text = $"You have to pay your debt ({Debt} Spirit Stones) next month!";

                        txtWarning2 = MonoBehaviour.Instantiate(uiTownStorageProps.textTip, uiTownStorageProps.transform, false);
                        txtWarning2.transform.position = new Vector3(uiTownStorageProps.textTitle2.transform.position.x, uiTownStorageProps.textTitle2.transform.position.y - 1.2f);
                        txtWarning2.verticalOverflow = VerticalWrapMode.Overflow;
                        txtWarning2.horizontalOverflow = HorizontalWrapMode.Overflow;
                        txtWarning2.alignment = TextAnchor.MiddleCenter;
                        txtWarning2.fontSize = 17;
                        txtWarning2.color = Color.red;
                        txtWarning2.text = $"You have to pay your debt ({Debt} Spirit Stones) next month!";

                        foreach (var item in uiTownStorageProps.GetComponentsInChildren<ScrollRect>().SelectMany(x => x.GetComponentsInChildren<Image>()))
                        {
                            item.raycastTarget = false;
                        }
                    }
                }
            }
        }

        public override void OnCloseUIEnd(CloseUIEnd e)
        {
            base.OnCloseUIEnd(e);
            uiTownStorageProps = MonoBehaviour.FindObjectOfType<UITownStorageProps>();
            if (uiTownStorageProps == null)
            {
                txtStorageMoney = null;
                txtFee = null;
            }
        }

        public override void OnFrameUpdate()
        {
            base.OnFrameUpdate();
            if (uiTownStorageProps != null && txtFee != null)
            {
                var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);
                var props = uiTownStorageProps.townStorage?.data?.propData?.allProps?.ToArray() ?? new DataProps.PropsData[0];
                StorageValue = props.Sum(x => x.propsCount * x.propsInfoBase.worth);
                var spValue = props.Where(x => x.propsID == 10001).Sum(x => x.propsCount * x.propsInfoBase.worth);
                txtStorageMoney.text = $"Storage: {StorageValue} Spirit Stones ({spValue} cash, {StorageValue - spValue} items)";
                //FreeStorage
                txtFee.text = uType == UnitTypeEnum.Merchant ? string.Empty : $"Fee: {FEE_RATE * 100:0.0}% (-{(StorageValue * FEE_RATE).Parse<int>()} Spirit Stones monthly)";
            }
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            //FreeStorage
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);
            if (uType != UnitTypeEnum.Merchant)
            {
                var money = g.world.playerUnit.GetUnitMoney();
                Debt += (StorageValue * FEE_RATE).Parse<long>();
                if (money >= Debt)
                {
                    g.world.playerUnit.AddUnitMoney(-Debt.Parse<int>());
                    Debt = 0L;
                }
                if (Debt > 0)
                {
                    DramaTool.OpenDrama(480020100);
                }
            }
        }
    }
}
