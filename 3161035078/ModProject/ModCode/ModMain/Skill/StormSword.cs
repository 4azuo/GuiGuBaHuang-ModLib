using EGameTypeData;
using MOD_nE7UL2.Const;
using ModLib.Enum;
using ModLib.Mod;
using ModLib.Object;

namespace MOD_nE7UL2.Mod
{
    [Skill("SpecialSkill_StormSword")]
    public class StormSword : ModSkill
    {
        public override void OnCast(UnitCtrlBase cunit, SkillBase skill, StepBase step, PropItemBase prop)
        {
            base.OnCast(cunit, skill, step, prop);
            var sa = skill?.TryCast<SkillAttack>();
            //if (skill != null && sa.data.ma)
            ModBattleEvent.SceneBattle.effect.Create("", ModBattleEvent.PlayerUnit.transform.position, 3f);
            
        }
    }
}
