using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UI_CREATE_PLAYER_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
    public class UICreatePlayerEvent : ModEvent
    {
        public override void OnOpenUIStart(OpenUIStart e)
        {
            base.OnOpenUIStart(e);
            if (e.uiType.uiName == UIType.CreatePlayer.uiName)
            {
                var smConfigs = EventHelper.GetEvent<SMGlobalConfigsEvent>(ModConst.SM_GLOBAL_CONFIGS_EVENT);
                if (smConfigs.LowGradeDestiniesAtBeginning)
                {
                    OnlyGrayDestinies();
                }
            }
        }

        private void OnlyGrayDestinies()
        {
            foreach (var luck in g.conf.roleCreateFeature._allConfList)
            {
                if (luck.type == 1 && luck.level.IsBetween(4, 6))
                {
                    luck.weight = 0;
                }
            }
        }
    }
}
