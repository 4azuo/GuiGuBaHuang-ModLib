using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;
using System.Linq;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UPGRADE_NATURALLY_EVENT)]
    public class UpgradeNaturallyEvent : ModEvent
    {
        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            if (!smConfigs.Configs.AllowUpgradeNaturally)
            {
                CacheHelper.RemoveCachableObject(this);
            }
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
                        }, "Up Grade").Size(200f, 40f);
                    }
                }
            }
        }
    }
}
