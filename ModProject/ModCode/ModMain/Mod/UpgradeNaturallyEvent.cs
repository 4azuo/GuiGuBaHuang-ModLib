using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using UnityEngine.Events;
using static UIHelper;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UPGRADE_NATURALLY_EVENT)]
    public class UpgradeNaturallyEvent : ModEvent
    {
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            if (smConfigs.Configs.EnableTrainer)
            {
                var player = g.world.playerUnit;
                if (e.uiType.uiName == UIType.FateFeature.uiName &&
                    player.GetMaxExpCurrentGrade() == player.GetProperty<int>(UnitPropertyEnum.Exp))
                {
                    using (var a = new UISample())
                    {
                        var ui = e.ui.TryCast<UIFateFeature>();
                        a.sampleUI.btnKeyOK.Create(ui.transform).AddSize(100f, 40f).Setup($"Up Grade").onClick.AddListener((UnityAction)(() =>
                        {
                            player.SetProperty<int>(UnitPropertyEnum.GradeID, player.GetNextPhaseLvl());
                        }));
                    }
                }
            }
        }
    }
}
