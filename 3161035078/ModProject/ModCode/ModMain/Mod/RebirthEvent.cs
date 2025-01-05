using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using UnityEngine.Events;

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

        [EventCondition]
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);

            if (e.uiType.uiName == UIType.TownPub.uiName && !SMLocalConfigsEvent.Instance.Configs.NoRebirth)
            {
                var player = g.world.playerUnit;

                if (!string.IsNullOrEmpty(player.data.unitData.schoolID))
                {
                    g.ui.MsgBox("Rebirth", "You have to leave the sect!");
                    return;
                }

                var playerGradeLvl = player.GetGradeLvl();
                var curTown = g.world.build.GetBuild(new UnityEngine.Vector2Int(player.data.unitData.pointX, player.data.unitData.pointY));
                var townLevel = curTown.gridData.areaBaseID;

                if (townLevel > RebirthLevel &&
                    playerGradeLvl > RebirthLevel &&
                    townLevel >= playerGradeLvl)
                {
                    var uiTownPub = g.ui.GetUI<UITownPub>(UIType.TownPub);
                    var btn1 = uiTownPub.btnPub.Copy().Pos(uiTownPub.btnPub.gameObject, 0f, 1f).Set($"Rebirth {RebirthCount + 1}");
                    btn1.onClick.AddListener((UnityAction)(() =>
                    {
                        g.ui.MsgBox("Rebirth", "Reputation will be reseted!", MsgBoxButtonEnum.YesNo, () =>
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

                            //remote button
                            btn1.gameObject.SetActive(false);
                            UnityEngine.Object.DestroyImmediate(btn1);

                            //add luck & open drama
                            AddLuck();
                            DramaTool.OpenDrama(REBIRTH_DRAMA);
                        });
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
