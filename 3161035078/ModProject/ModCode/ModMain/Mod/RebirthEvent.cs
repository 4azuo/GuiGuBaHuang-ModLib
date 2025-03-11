using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.REBIRTH_EVENT)]
    public class RebirthEvent : ModEvent
    {
        public static RebirthEvent Instance { get; set; }

        public const int REBIRTH_DRAMA = 420109999;
        public const int LUCK_BASE_ID = 420101000;
        public const int MAX_LUCK_LVL = 10;

        public int RebirthLevel { get; set; } = 0;
        public int RebirthCount { get; set; } = 0;
        public int TotalGradeLvl { get; set; } = 0;

        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (e.uiType.uiName == UIType.TownPub.uiName && !SMLocalConfigsEvent.Instance.Configs.NoRebirth)
            {
                var player = g.world.playerUnit;

                if (!string.IsNullOrEmpty(player.data.unitData.schoolID))
                {
                    g.ui.MsgBox(GameTool.LS("rebirth420103102"), GameTool.LS("rebirth420103100"));
                    return;
                }

                var playerGradeLvl = player.GetGradeLvl();
                var curTown = g.world.build.GetBuild(new UnityEngine.Vector2Int(player.data.unitData.pointX, player.data.unitData.pointY));
                var townLevel = curTown.gridData.areaBaseID;

                if (townLevel > RebirthLevel &&
                    playerGradeLvl > RebirthLevel &&
                    townLevel >= playerGradeLvl)
                {
                    var ui = new UICover<UITownPub>(e.ui);
                    {
                        ui.AddButton(0, 0, () =>
                        {
                            g.ui.MsgBox(GameTool.LS("rebirth420103102"), GameTool.LS("rebirth420103101"), MsgBoxButtonEnum.YesNo, () =>
                            {
                                //var
                                RebirthCount++;
                                RebirthLevel = playerGradeLvl;
                                TotalGradeLvl += playerGradeLvl;

                                //reset grade & exp
                                player.ResetGradeLevel();
                                player.ClearExp();

                                //reset items
                                player.SetProperty(UnitPropertyEnum.Reputation, 0);

                                //close rebirth ui
                                ui.Dispose();

                                //add luck & open drama
                                AddLuck();
                                DramaTool.OpenDrama(REBIRTH_DRAMA);
                            });
                        }, $"Rebirth {RebirthCount + 1}", ui.UI.btnPub).Pos(ui.UI.btnPub.gameObject, 0f, 1f);
                        ui.AddToolTipButton(GameTool.LS("rebirth420103103")).Pos(ui.UI.btnPub.gameObject, -1f, 1f);
                    }
                    ui.UpdateUI();
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
