using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UPGRADE_NATURALLY_EVENT)]
    public class UpgradeNaturallyEvent : ModEvent
    {
        public static UpgradeNaturallyEvent Instance { get; set; }

        public override bool OnCacheHandler()
        {
            return SMLocalConfigsEvent.Instance.Configs.AllowUpgradeNaturally;
        }

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
                        ui.AddButton(ui.MidCol, ui.MidRow + 9, () =>
                        {
                            player.SetProperty<int>(UnitPropertyEnum.GradeID, nPhase.id);
                            player.ClearExp();
                        }, GameTool.LS("other500020006")).Size(200f, 40f);
                        ui.AddToolTipButton(ui.MidCol - 3, ui.MidRow + 9, GameTool.LS("other500020018"));
                    }
                    ui.UpdateUI();
                }
            }
        }
    }
}
