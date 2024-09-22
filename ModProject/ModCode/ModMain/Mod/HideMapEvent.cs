using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Mod;
using UnityEngine;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.HIDE_MAP_EVENT)]
    public class HideMapEvent : ModEvent
    {
        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            var uiBattleInfo = MonoBehaviour.FindObjectOfType<UIBattleInfo>();
            if (uiBattleInfo != null)
            {
                uiBattleInfo.uiInfo.goMonstCount1.SetActive(false);
                uiBattleInfo.uiInfo.goMonstCount2.SetActive(false);
                uiBattleInfo.uiMap.goGroupRoot.SetActive(false);
            }
        }
    }
}
