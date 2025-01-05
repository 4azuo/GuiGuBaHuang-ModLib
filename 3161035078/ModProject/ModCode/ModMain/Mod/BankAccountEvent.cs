using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using static MOD_nE7UL2.Object.GameStts;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.BANK_ACCOUNT_EVENT)]
    public class BankAccountEvent : ModEvent
    {
        public static BankAccountEvent Instance { get; set; }
        public static _BankAccountConfigs BankAccountConfigs => ModMain.ModObj.GameSettings.BankAccountConfigs;

        public IList<string> RegisterdTown { get; set; } = new List<string>();

        [EventCondition]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (e.uiType.uiName == UIType.TownStorage.uiName)
            {
                var player = g.world.playerUnit;
                var curTown = g.world.build.GetBuild(new UnityEngine.Vector2Int(player.data.unitData.pointX, player.data.unitData.pointY));

                if (!RegisterdTown.Contains(curTown.buildData.id) && curTown.TryCast<MapBuildSchool>() == null)
                {
                    var uiTownStorage = g.ui.GetUI<UITownStorage>(UIType.TownStorage);
                    var btn1 = uiTownStorage.btnProps.Replace().Size(500f, 100f);
                    btn1.GetComponentInChildren<Text>().Align(UnityEngine.TextAnchor.MiddleCenter).text = $"Open account ({Cost(curTown.gridData.areaBaseID)} Spirit Stones)";
                    btn1.onClick.AddListener((UnityAction)(() =>
                    {
                        var cost = Cost(curTown.gridData.areaBaseID);
                        if (player.GetUnitMoney() >= cost)
                        {
                            player.AddUnitMoney(-cost);
                            RegisterdTown.Add(curTown.buildData.id);
                            g.ui.CloseUI(uiTownStorage);
                        }
                    }));
                }
            }
        }

        public static int Cost(int areaId)
        {
            var round = BankAccountConfigs.OpenFee[areaId] / 100;
            return SMLocalConfigsEvent.Instance.Calculate(InflationaryEvent.CalculateInflationary(BankAccountConfigs.OpenFee[areaId]) / round * round, SMLocalConfigsEvent.Instance.Configs.AddBankAccountCostRate).Parse<int>();
        }

        public static void RemoveAllAccounts()
        {
            Instance.RegisterdTown.Clear();
        }
    }
}
