using MOD_nE7UL2.Helper;
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
    /// Rebirth (player info screen, beside the damage calculation): reset grade/exp/reputation (must leave sect) for a permanent luck; monsters scale with total rebirth grades.
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
        /// The rebirth is offered on the player info screen, beside the damage calculation (UIEvent; until 6.0 at the
        /// pub). Null when the player can be reborn now, else the text key of what is missing: off (S&amp;M), a grade above the
        /// last rebirth, in a town whose area is above the last rebirth and at least the player's grade, no sect.
        /// </summary>
        public static string Missing(WorldUnitBase player)
        {
            if (Instance == null || SMLocalConfigsEvent.Instance.Configs.NoRebirth) return "rebirth_off";
            var grade = player.GetGradeLvl();
            if (grade <= Instance.RebirthLevel) return "rebirth_need_grade";
            var town = player.GetMapBuild<MapBuildTown>();
            var area = town?.gridData.areaBaseID ?? 0;
            if (town == null || area <= Instance.RebirthLevel || area < grade) return "rebirth_need_town";
            if (!string.IsNullOrEmpty(player.data.unitData.schoolID)) return "rebirth420103100";
            return null;
        }

        /// <summary>The button label: the next rebirth ("Rebirth 1"...).</summary>
        public static string ButtonText => GameTool.LS($"rebirth{420101001 + (Instance?.RebirthCount ?? 0)}");

        /// <summary>The rebirth (button of the player info): what is missing, or the confirmation, then the rebirth.</summary>
        public static void Open(System.Action after = null)
        {
            var player = g.world.playerUnit;
            var title = GameTool.LS("rebirth420103102");
            var missing = Missing(player);
            if (missing != null)
            {
                var grade = player.GetGradeLvl();
                InfoDialog.Msg(title, string.Format(GameTool.LS(missing), Instance?.RebirthLevel ?? 0, System.Math.Max(grade, (Instance?.RebirthLevel ?? 0) + 1)));
                return;
            }
            InfoDialog.Msg(title, GameTool.LS("rebirth420103101") + "\n" + GameTool.LS("rebirth420103103"), MsgBoxButtonEnum.YesNo, () =>
            {
                var playerGradeLvl = player.GetGradeLvl();
                Instance.RebirthCount++;
                Instance.RebirthLevel = playerGradeLvl;
                Instance.TotalGradeLvl += playerGradeLvl;

                //reset grade & exp
                player.ResetGradeLevel();
                player.ClearExp();

                //reset items
                player.SetProperty(UnitPropertyEnum.Reputation, 0);

                after?.Invoke();

                //add luck & open drama
                Instance.AddLuck();
                DramaTool.OpenDrama(REBIRTH_DRAMA);
            });
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
        internal void AddLuck()
        {
            RemoveLuck();
            g.world.playerUnit.AddLuck(LUCK_BASE_ID + RebirthCount);
        }
    }
}
