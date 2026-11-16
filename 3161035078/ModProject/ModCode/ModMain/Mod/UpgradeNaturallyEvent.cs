using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Helper;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Optional (S&amp;M): break through without breakthrough items when exp is full.
    /// </summary>
    [Cache(ModConst.UPGRADE_NATURALLY_EVENT)]
    public class UpgradeNaturallyEvent : ModEvent
    {
        public static UpgradeNaturallyEvent Instance { get; set; }

        /// <summary>
        /// Loaded only when AllowUpgradeNaturally is on.
        /// </summary>
        public override bool OnCacheHandler()
        {
            return SMLocalConfigsEvent.Instance.Configs.AllowUpgradeNaturally;
        }

        /// <summary>
        /// FateFeature UI with full exp: adds an "upgrade" button setting the next phase.
        /// </summary>
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.FateFeature.uiName)
            {
                var player = g.world.playerUnit;
                var nPhase = player.GetNextPhaseConf();
                if (player.IsFullExp() && nPhase != null)
                {
                    var ui = new UICover<UIFateFeature>(e.ui);
                    {
                        ui.AddButton(ui.MidCol, ui.MidRow + 8, () =>
                        {
                            player.SetProperty<int>(UnitPropertyEnum.GradeID, nPhase.id);
                            player.ClearExp();
                        }, GameTool.LS("other500020006")).Size(200f, 40f);
                        ui.AddToolTipButton(ui.MidCol - 3, ui.MidRow + 8, GameTool.LS("other500020018"));
                    }
                }
            }
        }
    }
}
