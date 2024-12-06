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
        public const int DEBT_DRAMA = 480020100;

        public static double FeeRate()
        {
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            return smConfigs.Calculate(ModMain.ModObj.InGameCustomSettings.RealStorageConfigs.FeeRate, smConfigs.Configs.AddBankFee);
        }

        private Text txtStorageMoney;
        private Text txtFee;

        public long Debt { get; set; } = 0L;
        private int count = 0;

        [EventCondition]
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
        [EventCondition]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);
            var storageValue = GetStorageValue();
            var spValue = GetStorageSpiritStones();
            txtStorageMoney.text = $"Storage: {storageValue} Spirit Stones ({spValue} cash, {storageValue - spValue} items)";
            //FreeStorage
            txtFee.text = uType == UnitTypeEnum.Merchant ? "Fee: free for merchant-master." : $"Fee: {FeeRate() * 100:0.0}% (-{(storageValue * FeeRate()).Parse<int>()} Spirit Stones monthly)";
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            //FreeStorage
            var uType = UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit);
            if (uType != UnitTypeEnum.Merchant)
            {
                var storageValue = GetStorageValue();
                var money = g.world.playerUnit.GetUnitMoney();
                Debt += (storageValue * FeeRate()).Parse<long>();
                if (money >= Debt)
                {
                    g.world.playerUnit.AddUnitMoney(-Debt.Parse<int>());
                    Debt = 0L;
                    count = 0;
                }
                if (Debt > 0 && (count < 3 || GameHelper.GetGameMonth() == 1))
                {
                    DramaTool.OpenDrama(DEBT_DRAMA);
                    count++;
                }
            }
        }

        public static long GetStorageValue()
        {
            var props = GameHelper.GetStorage().data.propData.allProps.ToArray() ?? new DataProps.PropsData[0];
            return props.Sum(x => x.propsCount * x.propsInfoBase.worth);
        }

        public static int GetStorageSpiritStones()
        {
            var props = GameHelper.GetStorage().data.propData.allProps.ToArray() ?? new DataProps.PropsData[0];
            return props.Where(x => x.propsID == ModLibConst.MONEY_PROP_ID).Sum(x => x.propsCount * x.propsInfoBase.worth);
        }

        public static void RemoveDebt()
        {
            EventHelper.GetEvent<RealStorageEvent>(ModConst.REAL_STORAGE_EVENT).Debt = 0L;
        }
    }
}
