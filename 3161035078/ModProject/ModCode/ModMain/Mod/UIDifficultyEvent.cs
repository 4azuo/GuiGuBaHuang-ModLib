using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Attributes;
using ModLib.Mod;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.UI_DIFFICULTY_EVENT, CacheType = CacheAttribute.CType.Global, WorkOn = CacheAttribute.WType.Global)]
    public class UIDifficultyEvent : ModEvent
    {
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.Difficulty.uiName)
            {
                var ui = g.ui.GetUI<UIDifficulty>(UIType.Difficulty);
                foreach (var item in ui.itemList)
                {
                    item.goRoot1.active = false;
                    item.goRoot1_En.active = false;
                    item.goRoot2.active = false;
                    item.goRoot2_En.active = false;
                }
            }
        }
    }
}
