using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using MOD_nE7UL2.Enum;
using ModLib.Const;

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

        private Text txtStorageMoney;
        private Text txtFee;

        public long StorageValue { get; set; } = 0L;
        public long Debt { get; set; } = 0L;

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.TownStorageProps.uiName)
            {
                var uiTownStorageProps = g.ui.GetUI<UITownStorageProps>(UIType.TownStorageProps);

                txtStorageMoney = ObjectHelper.Create(uiTownStorageProps.textTip).Align(TextAnchor.MiddleRight).Format();
                txtStorageMoney.transform.position = new Vector3(uiTownStorageProps.textTip.transform.position.x, uiTownStorageProps.textTitle1.transform.position.y + 0.4f);

                txtFee = ObjectHelper.Create(uiTownStorageProps.textTip).Align(TextAnchor.MiddleRight).Format(Color.blue);
                txtFee.transform.position = new Vector3(uiTownStorageProps.textTip.transform.position.x, uiTownStorageProps.textTitle1.transform.position.y + 0.2f);

                if (Debt > 0)
                {
                    var txtWarning1 = ObjectHelper.Create(uiTownStorageProps.textTip).Align(TextAnchor.MiddleCenter).Format(Color.red, 17).Pos(uiTownStorageProps.textTitle1.gameObject, 0f, -1.2f);
                    txtWarning1.text = $"You have to pay your debt ({Debt} Spirit Stones) next month!";

                    var txtWarning2 = ObjectHelper.Create(uiTownStorageProps.textTip).Align(TextAnchor.MiddleCenter).Format(Color.red, 17).Pos(uiTownStorageProps.textTitle2.gameObject, 0f, -1.2f);
                    txtWarning2.text = $"You have to pay your debt ({Debt} Spirit Stones) next month!";

                    foreach (var item in uiTownStorageProps.GetComponentsInChildren<ScrollRect>().SelectMany(x => x.GetComponentsInChildren<Image>()))
                    {
                        item.raycastTarget = false;
                    }
                }
            }
        }

        [ErrorIgnore]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            var uiTownStorageProps = g.ui.GetUI<UITownStorageProps>(UIType.TownStorageProps);
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);
            var props = uiTownStorageProps.townStorage?.data?.propData?.allProps?.ToArray() ?? new DataProps.PropsData[0];
            StorageValue = props.Sum(x => x.propsCount * x.propsInfoBase.worth);
            var spValue = props.Where(x => x.propsID == ModLibConst.MONEY_PROP_ID).Sum(x => x.propsCount * x.propsInfoBase.worth);
            txtStorageMoney.text = $"Storage: {StorageValue} Spirit Stones ({spValue} cash, {StorageValue - spValue} items)";
            //FreeStorage
            txtFee.text = uType == UnitTypeEnum.Merchant ? "Fee: free for merchant-master." : $"Fee: {FEE_RATE * 100:0.0}% (-{(StorageValue * FEE_RATE).Parse<int>()} Spirit Stones monthly)";
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
