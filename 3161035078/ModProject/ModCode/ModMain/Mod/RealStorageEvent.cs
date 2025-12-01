using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using MOD_nE7UL2.Enum;
using ModLib.Const;
using ModLib.Enum;
using ModLib.Attributes;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.REAL_STORAGE_EVENT)]
    public class RealStorageEvent : ModEvent
    {
        public static RealStorageEvent Instance { get; set; }

        public const int DEBT_DRAMA = 480020100;

        public static double FeeRate()
        {
            return SMLocalConfigsEvent.Instance.Calculate(ModMain.ModObj.ModSettings.RealStorageConfigs.FeeRate, SMLocalConfigsEvent.Instance.Configs.AddBankFee);
        }

        private Text txtStorageMoney;
        private Text txtFee;

        public long Debt { get; set; } = 0L;
        private int count = 0;

        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.TownStorageProps.uiName)
            {
                var uiTownStorageProps = g.ui.GetUI<UITownStorageProps>(UIType.TownStorageProps);

                txtStorageMoney = uiTownStorageProps.textTip.Copy().Align(TextAnchor.MiddleRight).Format().Pos(uiTownStorageProps.textTip.gameObject, 0, 40);
                txtFee = uiTownStorageProps.textTip.Copy().Align(TextAnchor.MiddleRight).Format(Color.blue).Pos(uiTownStorageProps.textTip.gameObject, 0, 20);

                if (Debt > 0)
                {
                    var txtWarning1 = uiTownStorageProps.textTip.Copy().Align(TextAnchor.MiddleCenter).Format(Color.red, 17)
                        .Pos(uiTownStorageProps.btnSearch1.gameObject, -300, 0)
                        .Set($"You have to pay your debt ({Debt} Spirit Stones) next month!");

                    var txtWarning2 = uiTownStorageProps.textTip.Copy().Align(TextAnchor.MiddleCenter).Format(Color.red, 17)
                        .Pos(uiTownStorageProps.btnSearch2.gameObject, -300, 0)
                        .Set($"You have to pay your debt ({Debt} Spirit Stones) next month!");

                    foreach (var item in uiTownStorageProps.GetComponentsInChildren<ScrollRect>().SelectMany(x => x.GetComponentsInChildren<Image>()))
                    {
                        item.raycastTarget = false;
                    }
                }
            }
        }

        [ErrorIgnore]
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnTimeUpdate200ms()
        {
            base.OnTimeUpdate200ms();
            if (g.ui.HasUI(UIType.TownStorageProps))
            {
                var storageValue = GetStorageValue();
                var spValue = GetStorageSpiritStones();
                txtStorageMoney.text = $"Storage: {storageValue} Spirit Stones ({spValue} cash, {storageValue - spValue} items)";
                //FreeStorage
                if (UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit) == UnitTypeEnum.Merchant || 
                    MapBuildPropertyEvent.IsTownGuardian(g.world.playerUnit.GetMapBuild<MapBuildTown>(), g.world.playerUnit))
                {
                    txtFee.text = GameTool.LS("other500020004");
                }
                else
                {
                    txtFee.text = $"Fee: {FeeRate() * 100:0.0}% (-{(storageValue * FeeRate()).Parse<int>()} Spirit Stones monthly)";
                }
            }
        }

        public override void OnMonthly()
        {
            base.OnMonthly();
            //FreeStorage
            if (UnitTypeEvent.GetUnitTypeEnum(g.world.playerUnit) != UnitTypeEnum.Merchant &&
                !MapBuildPropertyEvent.IsTownGuardian(g.world.playerUnit.GetMapBuild<MapBuildTown>(), g.world.playerUnit))
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
            Instance.Debt = 0L;
        }
    }
}
