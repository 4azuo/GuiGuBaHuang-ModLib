using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using static MOD_nE7UL2.Object.ModStts;
using ModLib.Helper;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Town storage requires a bank account per town (one-time fee paid to the town budget).
    /// </summary>
    [Cache(ModConst.BANK_ACCOUNT_EVENT)]
    public class BankAccountEvent : ModEvent
    {
        public static BankAccountEvent Instance { get; set; }
        public static _BankAccountConfigs BankAccountConfigs => ModMain.ModObj.ModSettings.BankAccountConfigs;

        public IList<string> RegisterdTown { get; set; } = new List<string>();

        /// <summary>
        /// TownStorage without account: replaces the storage button with "Open account (cost)".
        /// </summary>
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (e.uiType.uiName == UIType.TownStorage.uiName)
            {
                var player = g.world.playerUnit;
                var curTown = player.GetMapBuild<MapBuildTown>();

                if (curTown != null)
                {
                    if (!RegisterdTown.Contains(curTown.buildData.id) && !MapBuildPropertyEvent.IsTownGuardian(curTown, g.world.playerUnit))
                    {
                        var uiTownStorage = g.ui.GetUI<UITownStorage>(UIType.TownStorage);
                        var btn1 = uiTownStorage.btnProps.Replace().Size(500f, 100f);
                        btn1.GetComponentInChildren<Text>().Align(UnityEngine.TextAnchor.MiddleCenter).text = $"Open account ({Cost(curTown.gridData.areaBaseID):#,##0} Spirit Stones)";
                        btn1.onClick.AddListener(ModLib.Helper.ActionHelper.TracedUnityAction(() =>
                        {
                            var cost = Cost(curTown.gridData.areaBaseID);
                            if (player.GetUnitMoney() >= cost)
                            {
                                player.AddUnitMoney(-cost);
                                MapBuildPropertyEvent.AddBuildProperty(curTown, cost);
                                RegisterdTown.Add(curTown.buildData.id);
                                g.ui.CloseUI(uiTownStorage);
                            }
                        }));
                    }
                }
            }
        }

        /// <summary>
        /// Account fee of an area (inflation and S&amp;M rate applied).
        /// </summary>
        public static int Cost(int areaId)
        {
            var round = System.Math.Max(1, BankAccountConfigs.OpenFee[areaId] / 100); // OpenFee < 100 (BUG-15)
            return SMLocalConfigsEvent.Instance.Calculate(InflationaryEvent.CalculateInflationary(BankAccountConfigs.OpenFee[areaId]) / round * round, SMLocalConfigsEvent.Instance.Configs.AddBankAccountCostRate).Parse<int>();
        }

        /// <summary>
        /// Closes every account.
        /// </summary>
        public static void RemoveAllAccounts()
        {
            Instance.RegisterdTown.Clear();
        }
    }
}
