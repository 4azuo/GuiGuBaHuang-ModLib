using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Helper;
using ModLib.Mod;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    /// <summary>
    /// Per-save copy of the S&amp;M configs (taken from the global configs when the save is created). Read by most features.
    /// </summary>
    [Cache(ModConst.SM_LOCAL_CONFIGS_EVENT)]
    public class SMLocalConfigsEvent : ModEvent
    {
        public static SMLocalConfigsEvent Instance { get; set; }
        public SMGlobalConfigsEvent Configs { get; set; }

        /// <summary>
        /// New save: copies the global S&amp;M configs.
        /// </summary>
        public override void OnLoadClass(bool isNew, string modId, CacheAttribute attr)
        {
            base.OnLoadClass(isNew, modId, attr);
            if (isNew)
                Configs = CacheHelper.ReadGlobalCacheFile<SMGlobalConfigsEvent>(modId, ModConst.SM_GLOBAL_CONFIGS_EVENT) ?? new SMGlobalConfigsEvent();
        }

        /// <summary>
        /// Applies AddLevelupExpRate to the level-up exp table.
        /// </summary>
        public override void OnLoadGame()
        {
            base.OnLoadGame();
            //exp
            foreach (var item in g.conf.roleGrade._allConfList)
            {
                item.exp = Calculate(item.exp, Configs.AddLevelupExpRate).Parse<int>();
            }
        }

        /// <summary>
        /// bas + bas x addRate (helper used everywhere to apply S&amp;M rates).
        /// </summary>
        public double Calculate(double bas, double addRate)
        {
            return bas + (bas * addRate);
        }
    }
}
