using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    [Cache(ModConst.CUSTOM_SKILL_EVENT)]
    public class CustomSkillEvent : ModEvent
    {
        public static CustomSkillEvent Instance { get; set; }

        public override void OnOpenUIEnd(OpenUIEnd e)
        {
            base.OnOpenUIEnd(e);
            if (e.uiType.uiName == UIType.BattleInfo.uiName)
            {
            }
        }
    }
}
