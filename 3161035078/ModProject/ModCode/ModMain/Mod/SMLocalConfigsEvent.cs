using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.SM_LOCAL_CONFIGS_EVENT)]
    public class SMLocalConfigsEvent : ModEvent
    {
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
            //item value
            foreach (var props in g.conf.itemProps._allConfList)
            {
                if (props.type == (int)PropsType.Money)
                    continue;

                props.sale = Calculate(props.sale, Configs.AddItemValueRate).Parse<int>();
                props.worth = Calculate(props.worth, Configs.AddItemValueRate).Parse<int>();
            }
            foreach (var item in g.conf.itemSkill._allConfList)
            {
                item.price = Calculate(item.price, Configs.AddItemValueRate).Parse<int>();
                item.cost = Calculate(item.cost, Configs.AddItemValueRate).Parse<int>();
                item.sale = Calculate(item.sale, Configs.AddItemValueRate).Parse<int>();
                item.worth = Calculate(item.worth, Configs.AddItemValueRate).Parse<int>();
            }
            foreach (var refine in g.conf.townRefine._allConfList)
            {
                refine.moneyCost = Calculate(refine.moneyCost, Configs.AddItemValueRate).Parse<int>();
            }
        }

        public double Calculate(double bas, double addRate)
        {
            return bas + (bas * addRate);
        }
    }
}
