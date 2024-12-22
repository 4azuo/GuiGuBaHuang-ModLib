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
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            if (smConfigs.Configs.AllowUpgradeNaturally)
            {
                var player = g.world.playerUnit;
                if (e.uiType.uiName == UIType.FateFeature.uiName && player.IsFullExp())
                {
                    new UICover<UIFateFeature>(e.ui, (ui) =>
                    {
                        ui.AddButton(ui.MidCol, ui.MidRow + 5, () => player.SetProperty<int>(UnitPropertyEnum.GradeID, player.GetNextPhaseLvl()), "Up Grade").Size(200f, 40f);
                    });
                }
            }
        }
    }
}
