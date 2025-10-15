using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SM_LOCAL_CONFIGS_EVENT)]
    public class SMLocalConfigsEvent : ModEvent
    {
        public static SMLocalConfigsEvent Instance { get; set; }
        public SMGlobalConfigsEvent Configs { get; set; }

        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            if (isNew)
                Configs = CacheHelper.ReadGlobalCacheFile<SMGlobalConfigsEvent>(modId, ModConst.SM_GLOBAL_CONFIGS_EVENT) ?? new SMGlobalConfigsEvent();
        }

        public override void OnLoadGame()
        {
            base.OnLoadGame();
            //exp
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
