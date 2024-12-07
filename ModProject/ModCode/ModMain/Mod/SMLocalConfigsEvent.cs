using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SM_LOCAL_CONFIGS_EVENT)]
    public class SMLocalConfigsEvent : ModEvent
    {
        public SMGlobalConfigsEvent Configs { get; set; } = new SMGlobalConfigsEvent();

        public override void OnLoadClass(bool isNew)
        {
            base.OnLoadClass(isNew);
            if (isNew)
                Configs = CacheHelper.ReadGlobalCacheFile<SMGlobalConfigsEvent>(ModConst.SM_GLOBAL_CONFIGS_EVENT);
        }

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            foreach (var item in g.conf.roleGrade._allConfList)
            {
                item.exp = Calculate(item.exp, Configs.AddLevelupExpRate).Parse<int>();
            }
        }

        public double Calculate(double bas, double addRate)
        {
            return bas + (bas * addRate);
        }
    }
}
