using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SM_LOCAL_CONFIGS_EVENT)]
    public class SMLocalConfigsEvent : ModEvent
    {
        public SMGlobalConfigsEvent Configs { get; set; } = new SMGlobalConfigsEvent();

        public override void OnLoadNewGame()
        {
            base.OnLoadNewGame();
            Configs = EventHelper.GetEvent<SMGlobalConfigsEvent>(ModConst.SM_GLOBAL_CONFIGS_EVENT).Clone();
        }

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            var smConfigs = EventHelper.GetEvent<SMLocalConfigsEvent>(ModConst.SM_LOCAL_CONFIGS_EVENT);
            foreach (var item in g.conf.roleGrade._allConfList)
            {
                item.exp = smConfigs.Calculate(item.exp, smConfigs.Configs.AddLevelupExpRate).Parse<int>();
            }
        }

        public double Calculate(double bas, double addRate)
        {
            return bas + (bas * addRate);
        }
    }
}
