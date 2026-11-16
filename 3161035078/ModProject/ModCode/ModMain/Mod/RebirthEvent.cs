using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Helper;
using ModLib.Mod;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Rebirth at the pub: reset grade/exp/reputation (must leave sect) for a permanent luck; monsters scale with total rebirth grades.
    /// </summary>
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

        /// <summary>
        /// TownPub of a high enough town: adds the rebirth button.
        /// </summary>
        [EventCondition(IsInGame = HandleEnum.True, IsInBattle = HandleEnum.False)]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (!SMLocalConfigsEvent.Instance.Configs.NoRebirth)
            {
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
                        var ui = new UICover<UITownPub>(e.ui);
                        {
                            ui.AddButton(0, 0, () =>
                            {
                                if (!string.IsNullOrEmpty(player.data.unitData.schoolID))
                                {
                                    g.ui.MsgBox(GameTool.LS("rebirth420103102"), GameTool.LS("rebirth420103100"));
                                    return;
                                }

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
                            }, GameTool.LS($"rebirth{420101001 + RebirthCount}"), ui.UI.btnPub).Pos(ui.UI.btnPub.gameObject, 0, 100);
                            ui.AddToolTipButton(GameTool.LS("rebirth420103103")).Pos(ui.UI.btnPub.gameObject, -100, 100);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes rebirth lucks.
        /// </summary>
        private void RemoveLuck()
        {
            for (int i = 1; i <= MAX_LUCK_LVL; i++)
            {
                var luckId = LUCK_BASE_ID + i;
                g.world.playerUnit.DelLuck(luckId);
            }
        }

        /// <summary>
        /// Gives the luck of the rebirth count.
        /// </summary>
        private void AddLuck()
        {
            RemoveLuck();
            g.world.playerUnit.AddLuck(LUCK_BASE_ID + RebirthCount);
        }
    }
}
