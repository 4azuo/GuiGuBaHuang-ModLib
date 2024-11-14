using EGameTypeData;
using Il2CppSystem;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.REBIRTH_EVENT)]
    public class RebirthEvent : ModEvent
    {
        public const int LUCK_BASE_ID = 420101000;
        public const int MAX_LUCK_LVL = 10;

        public int RebirthLevel { get; set; } = 0;
        public int RebirthCount { get; set; } = 0;
        public int TotalGradeLvl { get; set; } = 0;

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (e.uiType.uiName == UIType.TownPub.uiName)
            {
                var player = g.world.playerUnit;
                var playerGradeLvl = player.GetGradeLvl();
                var curTown = g.world.build.GetBuild(new UnityEngine.Vector2Int(player.data.unitData.pointX, player.data.unitData.pointY));
                var townLevel = curTown.gridData.areaBaseID;

                if (townLevel > RebirthLevel &&
                    playerGradeLvl > RebirthLevel &&
                    townLevel >= playerGradeLvl)
                {
                    var uiTownPub = g.ui.GetUI<UITownPub>(UIType.TownPub);
                    var btn1 = uiTownPub.btnPub.Create().Pos(uiTownPub.btnPub.gameObject, 0f, 1f);
                    btn1.GetComponentInChildren<Text>().text = $"Rebirth {RebirthCount + 1}";
                    btn1.onClick.AddListener((UnityAction)(() =>
                    {
                        var uiConfirm = g.ui.OpenUI<UICheckPopup>(UIType.CheckPopup);
                        uiConfirm.InitData("Rebirth", "All items (excluded storage items) will be deleted!", 2,
                            (Action)(() =>
                            {
                                RebirthCount++;
                                RebirthLevel = playerGradeLvl;
                                TotalGradeLvl += playerGradeLvl;
                                player.ResetGradeLevel();
                                player.ClearExp();
                                player.SetUnitMoney(0);
                                player.SetUnitContribution(0);
                                player.SetUnitMayorDegree(0);
                                player.RemoveAllItems();
                                //player.RemoveAllStorageItems();
                                BankAccountEvent.RemoveAllAccounts();
                                //RealStorageEvent.RemoveDebt();
                                player.SetProperty(UnitPropertyEnum.Reputation, 0);
                                player.data.school.ExitSchool(player);
                                AddLuck();
                            }));
                    }));
                }
            }
        }

        private void RemoveLuck()
        {
            for (int i = 1; i <= MAX_LUCK_LVL; i++)
            {
                var luckId = LUCK_BASE_ID + i;
                g.world.playerUnit.DelLuck(luckId);
            }
        }

        private void AddLuck()
        {
            RemoveLuck();
            g.world.playerUnit.AddLuck(LUCK_BASE_ID + RebirthCount);
        }
    }
}
